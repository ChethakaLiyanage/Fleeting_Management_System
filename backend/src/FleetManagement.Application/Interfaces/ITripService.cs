using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Trips;

namespace FleetManagement.Application.Interfaces;

public interface ITripService
{
    Task<PagedResult<TripDto>> GetTripsAsync(TripFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<TripDto?> GetTripByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TripDto> CreateTripAsync(CreateTripDto dto, CancellationToken cancellationToken = default);
    Task<TripDto?> UpdateTripAsync(Guid id, UpdateTripDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteTripAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TripDto> StartTripAsync(Guid id, StartTripDto dto, CancellationToken cancellationToken = default);
    Task<TripDto> CompleteTripAsync(Guid id, CompleteTripDto dto, CancellationToken cancellationToken = default);
    Task<TripDto> CancelTripAsync(Guid id, string? reason, CancellationToken cancellationToken = default);
}
