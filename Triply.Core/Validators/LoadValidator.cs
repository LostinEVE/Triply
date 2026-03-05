using FluentValidation;
using Triply.Core.Models;
using Triply.Core.Enums;

namespace Triply.Core.Validators;

public class LoadValidator : AbstractValidator<Load>
{
    public LoadValidator()
    {
        RuleFor(l => l.LoadNumber)
            .NotEmpty().WithMessage("Load number is required")
            .MaximumLength(50).WithMessage("Load number cannot exceed 50 characters");

        RuleFor(l => l.CustomerId)
            .NotEmpty().WithMessage("Customer is required");

        RuleFor(l => l.PickupDate)
            .NotEmpty().WithMessage("Pickup date is required");

        RuleFor(l => l.DeliveryDate)
            .GreaterThanOrEqualTo(l => l.PickupDate)
            .WithMessage("Delivery date must be on or after pickup date")
            .When(l => l.PickupDate.HasValue && l.DeliveryDate.HasValue);

        RuleFor(l => l.Miles)
            .GreaterThanOrEqualTo(0).WithMessage("Miles cannot be negative");

        RuleFor(l => l.Rate)
            .GreaterThan(0).WithMessage("Rate must be greater than 0");

        RuleFor(l => l.TotalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Total amount cannot be negative");

        // Business rule: Cannot invoice a load that hasn't been booked or in transit
        RuleFor(l => l.Status)
            .Must((load, status) => status != LoadStatus.Booked)
            .WithMessage("Cannot create invoice for a load that is only booked")
            .When(l => l.InvoiceLineItems.Any());

        RuleFor(l => l.PickupZip)
            .Matches(@"^\d{5}(-\d{4})?$").WithMessage("Pickup zip must be a valid US ZIP code")
            .When(l => !string.IsNullOrEmpty(l.PickupZip));

        RuleFor(l => l.DeliveryZip)
            .Matches(@"^\d{5}(-\d{4})?$").WithMessage("Delivery zip must be a valid US ZIP code")
            .When(l => !string.IsNullOrEmpty(l.DeliveryZip));

        RuleFor(l => l.PickupState)
            .Length(2).WithMessage("Pickup state must be 2 characters")
            .When(l => !string.IsNullOrEmpty(l.PickupState));

        RuleFor(l => l.DeliveryState)
            .Length(2).WithMessage("Delivery state must be 2 characters")
            .When(l => !string.IsNullOrEmpty(l.DeliveryState));
    }
}
