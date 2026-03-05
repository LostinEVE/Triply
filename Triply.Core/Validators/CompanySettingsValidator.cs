using FluentValidation;
using Triply.Core.Models;

namespace Triply.Core.Validators;

public class CompanySettingsValidator : AbstractValidator<CompanySettings>
{
    public CompanySettingsValidator()
    {
        RuleFor(c => c.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(200).WithMessage("Company name cannot exceed 200 characters");

        RuleFor(c => c.DOTNumber)
            .Matches(@"^\d{7,8}$").WithMessage("DOT number must be 7 or 8 digits")
            .When(c => !string.IsNullOrEmpty(c.DOTNumber));

        RuleFor(c => c.MCNumber)
            .Matches(@"^MC-?\d{6,7}$").WithMessage("MC number must be in format MC-123456 or MC123456")
            .When(c => !string.IsNullOrEmpty(c.MCNumber));

        RuleFor(c => c.EIN)
            .Matches(@"^\d{2}-\d{7}$").WithMessage("EIN must be in format 12-3456789")
            .When(c => !string.IsNullOrEmpty(c.EIN));

        RuleFor(c => c.Phone)
            .Matches(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$")
            .WithMessage("Phone must be a valid US phone number")
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters")
            .When(c => !string.IsNullOrEmpty(c.Phone));

        RuleFor(c => c.Email)
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
            .When(c => !string.IsNullOrEmpty(c.Email));

        RuleFor(c => c.Zip)
            .Matches(@"^\d{5}(-\d{4})?$").WithMessage("ZIP code must be valid (12345 or 12345-6789)")
            .MaximumLength(10).WithMessage("ZIP code cannot exceed 10 characters")
            .When(c => !string.IsNullOrEmpty(c.Zip));

        RuleFor(c => c.State)
            .Length(2).WithMessage("State must be 2 characters")
            .When(c => !string.IsNullOrEmpty(c.State));

        RuleFor(c => c.InvoicePrefix)
            .NotEmpty().WithMessage("Invoice prefix is required")
            .MaximumLength(10).WithMessage("Invoice prefix cannot exceed 10 characters");

        RuleFor(c => c.NextInvoiceNumber)
            .GreaterThan(0).WithMessage("Next invoice number must be greater than 0");

        RuleFor(c => c.FederalTaxRate)
            .InclusiveBetween(0, 100).WithMessage("Federal tax rate must be between 0 and 100");

        RuleFor(c => c.StateTaxRate)
            .InclusiveBetween(0, 100).WithMessage("State tax rate must be between 0 and 100");

        RuleFor(c => c.SelfEmploymentTaxRate)
            .InclusiveBetween(0, 100).WithMessage("Self-employment tax rate must be between 0 and 100");

        RuleFor(c => c.FiscalYearStart)
            .InclusiveBetween(1, 12).WithMessage("Fiscal year start must be between 1 and 12");

        // Email settings validation
        RuleFor(c => c.SMTPPort)
            .InclusiveBetween(1, 65535).WithMessage("SMTP port must be between 1 and 65535")
            .When(c => !string.IsNullOrEmpty(c.SMTPServer));

        RuleFor(c => c.FromEmail)
            .EmailAddress().WithMessage("From email must be a valid email address")
            .When(c => !string.IsNullOrEmpty(c.FromEmail));

        RuleFor(c => c.SMTPUsername)
            .NotEmpty().WithMessage("SMTP username is required when SMTP server is configured")
            .When(c => !string.IsNullOrEmpty(c.SMTPServer));

        RuleFor(c => c.SMTPPassword)
            .NotEmpty().WithMessage("SMTP password is required when SMTP server is configured")
            .When(c => !string.IsNullOrEmpty(c.SMTPServer));
    }
}
