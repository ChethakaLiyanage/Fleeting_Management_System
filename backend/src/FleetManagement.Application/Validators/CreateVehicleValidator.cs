using FleetManagement.Application.DTOs.Vehicles;
using FluentValidation;

namespace FleetManagement.Application.Validators;

public class CreateVehicleValidator : AbstractValidator<CreateVehicleDto>
{
    public CreateVehicleValidator()
    {
        RuleFor(x => x.RegistrationNumber)
            .NotEmpty().WithMessage("Registration number is required.")
            .MaximumLength(50);

        RuleFor(x => x.EngineNumber)
            .NotEmpty().WithMessage("Engine number is required.")
            .MaximumLength(100);

        RuleFor(x => x.Make)
            .NotEmpty().WithMessage("Make is required.")
            .MaximumLength(100);

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required.")
            .MaximumLength(100);

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 1}.");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).WithMessage("Mileage cannot be negative.");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).When(x => x.PurchasePrice.HasValue)
            .WithMessage("Purchase price cannot be negative.");
    }
}
