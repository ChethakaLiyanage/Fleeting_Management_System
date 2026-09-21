using FleetManagement.Application.DTOs.Documents;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetManagement.Application.Services;

public class DocumentService : IDocumentService
{{
    private readonly IFleetDbContext _context;
    private readonly string _uploadsPath;

    public DocumentService(IFleetDbContext context)
    {{
        _context = context;
        // Relative upload folder — in production, inject via IOptions<StorageOptions>
        _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(_uploadsPath);
    }}

    public async Task<IEnumerable<DocumentDto>> GetAllAsync()
    {{
        var docs = await _context.Documents
            .Include(d => d.Vehicle)
            .Include(d => d.Driver)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return docs.Select(MapToDto);
    }}

    public async Task<IEnumerable<DocumentDto>> GetByVehicleAsync(Guid vehicleId)
    {{
        var docs = await _context.Documents
            .Include(d => d.Vehicle)
            .Include(d => d.Driver)
            .Where(d => d.VehicleId == vehicleId && !d.IsDeleted)
            .ToListAsync();

        return docs.Select(MapToDto);
    }}

    public async Task<IEnumerable<DocumentDto>> GetByDriverAsync(Guid driverId)
    {{
        var docs = await _context.Documents
            .Include(d => d.Vehicle)
            .Include(d => d.Driver)
            .Where(d => d.DriverId == driverId && !d.IsDeleted)
            .ToListAsync();

        return docs.Select(MapToDto);
    }}

    public async Task<IEnumerable<DocumentDto>> GetExpiringAsync()
    {{
        var threshold = DateTime.UtcNow.AddDays(30);
        var docs = await _context.Documents
            .Include(d => d.Vehicle)
            .Include(d => d.Driver)
            .Where(d => !d.IsDeleted && d.ExpiryDate.HasValue
                && d.ExpiryDate.Value <= threshold
                && d.ExpiryDate.Value >= DateTime.UtcNow)
            .OrderBy(d => d.ExpiryDate)
            .ToListAsync();

        return docs.Select(MapToDto);
    }}

    public async Task<DocumentDto> GetByIdAsync(Guid id)
    {{
        var doc = await _context.Documents
            .Include(d => d.Vehicle)
            .Include(d => d.Driver)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new Exception($"Document {{id}} not found.");

        return MapToDto(doc);
    }}

    public async Task<DocumentDto> UploadAsync(UploadDocumentRequest request)
    {{
        var extension    = Path.GetExtension(request.OriginalFileName);
        var storedName   = $"{{Guid.NewGuid()}}{{extension}}";
        var fullPath     = Path.Combine(_uploadsPath, storedName);

        await File.WriteAllBytesAsync(fullPath, request.FileContent);

        var document = new Document
        {{
            VehicleId          = request.VehicleId,
            DriverId           = request.DriverId,
            UploadedByUserId   = request.UploadedByUserId,
            DocumentType       = request.DocumentType,
            FileName           = request.OriginalFileName,
            StoredFileName     = storedName,
            ContentType        = request.ContentType,
            FileSizeBytes      = request.FileContent.LongLength,
            FilePath           = storedName,
            ExpiryDate         = request.ExpiryDate,
            Description        = request.Description
        }};

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(document.Id);
    }}

    public async Task<(byte[] Content, string FileName, string ContentType)> DownloadAsync(Guid id)
    {{
        var doc = await _context.Documents.FindAsync(id)
            ?? throw new Exception($"Document {{id}} not found.");

        var fullPath = Path.Combine(_uploadsPath, doc.FilePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Physical file not found.", fullPath);

        var content = await File.ReadAllBytesAsync(fullPath);
        return (content, doc.FileName, doc.ContentType);
    }}

    public async Task DeleteAsync(Guid id)
    {{
        var doc = await _context.Documents.FindAsync(id)
            ?? throw new Exception($"Document {{id}} not found.");

        doc.IsDeleted = true;
        await _context.SaveChangesAsync();
    }}

    private static DocumentDto MapToDto(Document d) => new()
    {{
        Id                  = d.Id,
        VehicleId           = d.VehicleId,
        VehicleRegistration = d.Vehicle?.RegistrationNumber,
        DriverId            = d.DriverId,
        DriverName          = d.Driver != null ? $"{{d.Driver.FirstName}} {{d.Driver.LastName}}" : null,
        DocumentType        = d.DocumentType.ToString(),
        FileName            = d.FileName,
        ContentType         = d.ContentType,
        FileSizeBytes       = d.FileSizeBytes,
        ExpiryDate          = d.ExpiryDate,
        Description         = d.Description,
        IsExpired           = d.IsExpired,
        IsExpiringSoon      = d.IsExpiringSoon,
        UploadedAt          = d.CreatedAt
    }};
}}
