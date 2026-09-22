using FleetManagement.Application.DTOs.Trips;
using FluentValidation;

namespace FleetManagement.Application.Validators;

public class StartTripValidator : AbstractValidator<StartTripDto>
{
    public StartTripValidator()
    {
        RuleFor(x => x.StartingMileage)
            .GreaterThanOrEqualTo(0).When(x => x.StartingMileage.HasValue)
            .WithMessage("Starting mileage cannot be negative.");
    }
}
