using FleetManagement.Application.DTOs.Maintenance;
using FluentValidation;

namespace FleetManagement.Application.Validators;

public class CreateMaintenanceValidator : AbstractValidator<CreateMaintenanceRequest>
{
    public CreateMaintenanceValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Vehicle is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500);

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).When(x => x.Cost.HasValue)
            .WithMessage("Cost cannot be negative.");
    }
}
