using FleetManagement.Application.DTOs.Documents;

namespace FleetManagement.Application.Interfaces;

public interface IDocumentService
{{
    Task<IEnumerable<DocumentDto>> GetAllAsync();
    Task<IEnumerable<DocumentDto>> GetByVehicleAsync(Guid vehicleId);
    Task<IEnumerable<DocumentDto>> GetByDriverAsync(Guid driverId);
    Task<IEnumerable<DocumentDto>> GetExpiringAsync();
    Task<DocumentDto> GetByIdAsync(Guid id);
    Task<DocumentDto> UploadAsync(UploadDocumentRequest request);
    Task DeleteAsync(Guid id);
    Task<(byte[] Content, string FileName, string ContentType)> DownloadAsync(Guid id);
}}
