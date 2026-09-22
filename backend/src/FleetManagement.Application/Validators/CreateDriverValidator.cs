using FleetManagement.Application.DTOs.Drivers;
using FluentValidation;

namespace FleetManagement.Application.Validators;

public class CreateDriverValidator : AbstractValidator<CreateDriverDto>
{
    public CreateDriverValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("License number is required.")
            .MaximumLength(50);

        RuleFor(x => x.LicenseExpiry)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("License expiry date must be a future date.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.");

        RuleFor(x => x.EmployeeNumber)
            .NotEmpty().WithMessage("Employee number is required.");
    }
}
