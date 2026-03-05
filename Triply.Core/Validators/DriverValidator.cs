using FluentValidation;
using Triply.Core.Models;
using System.Text.RegularExpressions;

namespace Triply.Core.Validators;

public class DriverValidator : AbstractValidator<Driver>
{
    public DriverValidator()
    {
        RuleFor(d => d.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(d => d.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(d => d.CDLNumber)
            .MaximumLength(50).WithMessage("CDL number cannot exceed 50 characters")
            .When(d => !string.IsNullOrEmpty(d.CDLNumber));

        RuleFor(d => d.CDLState)
            .Length(2).WithMessage("CDL state must be 2 characters")
            .When(d => !string.IsNullOrEmpty(d.CDLState));

        RuleFor(d => d.CDLExpiration)
            .GreaterThan(DateTime.Now).WithMessage("CDL has expired")
            .When(d => d.CDLExpiration.HasValue);

        RuleFor(d => d.Phone)
            .Matches(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$")
            .WithMessage("Phone must be a valid US phone number (e.g., 555-555-5555)")
            .When(d => !string.IsNullOrEmpty(d.Phone));

        RuleFor(d => d.Email)
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
            .When(d => !string.IsNullOrEmpty(d.Email));

        RuleFor(d => d.HireDate)
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Hire date cannot be in the future")
            .When(d => d.HireDate.HasValue);

        RuleFor(d => d.PayRate)
            .GreaterThan(0).WithMessage("Pay rate must be greater than 0")
            .When(d => d.PayRate.HasValue);
    }
}
