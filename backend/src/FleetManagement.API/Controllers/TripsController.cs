using FleetManagement.Application.Common;
using FleetManagement.Application.DTOs.Trips;
using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[Authorize(Roles = "Admin,FleetManager,Driver")]
[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TripDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrips([FromQuery] TripFilterParams filterParams, CancellationToken cancellationToken)
    {
        var result = await _tripService.GetTripsAsync(filterParams, cancellationToken);
        return Ok(ApiResponse<PagedResult<TripDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TripDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTripById(Guid id, CancellationToken cancellationToken)
    {
        var trip = await _tripService.GetTripByIdAsync(id, cancellationToken);
        if (trip == null)
        {
            return NotFound(ApiResponse<TripDto>.Fail($"Trip with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<TripDto>.Ok(trip));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,FleetManager")]
    [ProducesResponseType(typeof(ApiResponse<TripDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTrip([FromBody] CreateTripDto dto, CancellationToken cancellationToken)
    {
        var trip = await _tripService.CreateTripAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetTripById), new { id = trip.Id }, ApiResponse<TripDto>.Ok(trip, "Trip created successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,FleetManager")]
    [ProducesResponseType(typeof(ApiResponse<TripDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTrip(Guid id, [FromBody] UpdateTripDto dto, CancellationToken cancellationToken)
    {
        var trip = await _tripService.UpdateTripAsync(id, dto, cancellationToken);
        if (trip == null)
        {
            return NotFound(ApiResponse<TripDto>.Fail($"Trip with ID '{id}' was not found."));
        }
        return Ok(ApiResponse<TripDto>.Ok(trip, "Trip updated successfully."));
    }

    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(ApiResponse<TripDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartTrip(Guid id, [FromBody] StartTripDto dto, CancellationToken cancellationToken)
    {
        var trip = await _tripService.StartTripAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<TripDto>.Ok(trip, "Trip started successfully."));
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<TripDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteTrip(Guid id, [FromBody] CompleteTripDto dto, CancellationToken cancellationToken)
    {
        var trip = await _tripService.CompleteTripAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<TripDto>.Ok(trip, "Trip completed successfully."));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,FleetManager")]
    [ProducesResponseType(typeof(ApiResponse<TripDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelTrip(Guid id, [FromQuery] string? reason, CancellationToken cancellationToken)
    {
        var trip = await _tripService.CancelTripAsync(id, reason, cancellationToken);
        return Ok(ApiResponse<TripDto>.Ok(trip, "Trip cancelled successfully."));
    }
}
