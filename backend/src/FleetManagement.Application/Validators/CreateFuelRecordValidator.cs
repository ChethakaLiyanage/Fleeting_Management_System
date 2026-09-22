using FleetManagement.Application.DTOs.Fuel;
using FluentValidation;

namespace FleetManagement.Application.Validators;

public class CreateFuelRecordValidator : AbstractValidator<CreateFuelRecordRequest>
{
    public CreateFuelRecordValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Vehicle is required.");

        RuleFor(x => x.Litres)
            .GreaterThan(0).WithMessage("Litres must be greater than zero.");

        RuleFor(x => x.CostPerLitre)
            .GreaterThanOrEqualTo(0).WithMessage("Cost per litre cannot be negative.");

        RuleFor(x => x.OdometerReading)
            .GreaterThanOrEqualTo(0).WithMessage("Odometer reading cannot be negative.");
    }
}
