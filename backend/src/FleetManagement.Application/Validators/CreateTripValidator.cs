using FleetManagement.Application.DTOs.Trips;
using FluentValidation;

namespace FleetManagement.Application.Validators;

/// <summary>
/// Input validation only. Business rules (vehicle availability, driver status,
/// license expiry, active trip conflicts) stay in TripService.
/// </summary>
public class CreateTripValidator : AbstractValidator<CreateTripDto>
{
    public CreateTripValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Vehicle is required.");

        RuleFor(x => x.DriverId)
            .NotEmpty().WithMessage("Driver is required.");

        RuleFor(x => x.StartLocation)
            .NotEmpty().WithMessage("Start location is required.")
            .MaximumLength(200);

        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("Destination is required.")
            .MaximumLength(200);

        RuleFor(x => x.Purpose)
            .NotEmpty().WithMessage("Trip purpose is required.")
            .MaximumLength(200);
    }
}
