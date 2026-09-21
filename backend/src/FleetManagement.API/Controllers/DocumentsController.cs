using FleetManagement.Application.DTOs.Documents;
using FleetManagement.Application.Interfaces;
using FleetManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService) =>
        _documentService = documentService;

    /// <summary>Get all documents.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _documentService.GetAllAsync());

    /// <summary>Get documents expiring within 30 days.</summary>
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiring() =>
        Ok(await _documentService.GetExpiringAsync());

    /// <summary>Get documents for a specific vehicle.</summary>
    [HttpGet("vehicle/{{vehicleId:guid}}")]
    public async Task<IActionResult> GetByVehicle(Guid vehicleId) =>
        Ok(await _documentService.GetByVehicleAsync(vehicleId));

    /// <summary>Get documents for a specific driver.</summary>
    [HttpGet("driver/{{driverId:guid}}")]
    public async Task<IActionResult> GetByDriver(Guid driverId) =>
        Ok(await _documentService.GetByDriverAsync(driverId));

    /// <summary>Get document metadata by ID.</summary>
    [HttpGet("{{id:guid}}")]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await _documentService.GetByIdAsync(id));

    /// <summary>Download a document file.</summary>
    [HttpGet("{{id:guid}}/download")]
    public async Task<IActionResult> Download(Guid id)
    {{
        var (content, fileName, contentType) = await _documentService.DownloadAsync(id);
        return File(content, contentType, fileName);
    }}

    /// <summary>Upload a new document (multipart/form-data).</summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] Guid? vehicleId,
        [FromForm] Guid? driverId,
        [FromForm] Guid uploadedByUserId,
        [FromForm] DocumentType documentType,
        [FromForm] string? description,
        [FromForm] DateTime? expiryDate)
    {{
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var request = new UploadDocumentRequest
        {{
            VehicleId          = vehicleId,
            DriverId           = driverId,
            UploadedByUserId   = uploadedByUserId,
            DocumentType       = documentType,
            Description        = description,
            ExpiryDate         = expiryDate,
            FileContent        = ms.ToArray(),
            OriginalFileName   = file.FileName,
            ContentType        = file.ContentType
        }};

        var result = await _documentService.UploadAsync(request);
        return CreatedAtAction(nameof(GetById), new {{ id = result.Id }}, result);
    }}

    /// <summary>Soft-delete a document.</summary>
    [HttpDelete("{{id:guid}}")]
    public async Task<IActionResult> Delete(Guid id)
    {{
        await _documentService.DeleteAsync(id);
        return NoContent();
    }}
}}
