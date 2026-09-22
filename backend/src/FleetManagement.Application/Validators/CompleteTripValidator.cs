using FleetManagement.Application.DTOs.Trips;
using FluentValidation;

namespace FleetManagement.Application.Validators;

public class CompleteTripValidator : AbstractValidator<CompleteTripDto>
{
    public CompleteTripValidator()
    {
        RuleFor(x => x.EndingMileage)
            .GreaterThan(0).WithMessage("Ending mileage must be greater than zero.");
    }
}
