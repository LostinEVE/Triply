using FluentValidation;
using Triply.Core.Models;

namespace Triply.Core.Validators;

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(c => c.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(200).WithMessage("Company name cannot exceed 200 characters");

        RuleFor(c => c.ContactName)
            .MaximumLength(100).WithMessage("Contact name cannot exceed 100 characters")
            .When(c => !string.IsNullOrEmpty(c.ContactName));

        RuleFor(c => c.ContactEmail)
            .EmailAddress().WithMessage("Contact email must be a valid email address")
            .MaximumLength(255).WithMessage("Contact email cannot exceed 255 characters")
            .When(c => !string.IsNullOrEmpty(c.ContactEmail));

        RuleFor(c => c.ContactPhone)
            .Matches(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$")
            .WithMessage("Contact phone must be a valid US phone number (e.g., 555-555-5555)")
            .MaximumLength(20).WithMessage("Contact phone cannot exceed 20 characters")
            .When(c => !string.IsNullOrEmpty(c.ContactPhone));

        RuleFor(c => c.BillingAddress)
            .MaximumLength(200).WithMessage("Billing address cannot exceed 200 characters")
            .When(c => !string.IsNullOrEmpty(c.BillingAddress));

        RuleFor(c => c.BillingCity)
            .MaximumLength(100).WithMessage("Billing city cannot exceed 100 characters")
            .When(c => !string.IsNullOrEmpty(c.BillingCity));

        RuleFor(c => c.BillingState)
            .Length(2).WithMessage("Billing state must be 2 characters")
            .When(c => !string.IsNullOrEmpty(c.BillingState));

        RuleFor(c => c.BillingZip)
            .Matches(@"^\d{5}(-\d{4})?$").WithMessage("Billing zip must be a valid US ZIP code (e.g., 12345 or 12345-6789)")
            .MaximumLength(10).WithMessage("Billing zip cannot exceed 10 characters")
            .When(c => !string.IsNullOrEmpty(c.BillingZip));

        RuleFor(c => c.PaymentTerms)
            .NotEmpty().WithMessage("Payment terms are required")
            .MaximumLength(50).WithMessage("Payment terms cannot exceed 50 characters");
    }
}
