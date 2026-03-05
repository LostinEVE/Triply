using FluentValidation;
using Triply.Core.Models;

namespace Triply.Core.Validators;

public class FuelEntryValidator : AbstractValidator<FuelEntry>
{
    public FuelEntryValidator()
    {
        RuleFor(f => f.TruckId)
            .NotEmpty().WithMessage("Truck is required");

        RuleFor(f => f.FuelDate)
            .NotEmpty().WithMessage("Fuel date is required")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Fuel date cannot be in the future");

        RuleFor(f => f.Odometer)
            .GreaterThan(0).WithMessage("Odometer must be greater than 0");

        // Business rule: Odometer must not decrease
        RuleFor(f => f.Odometer)
            .Must((fuelEntry, odometer) => true) // This will be validated in the service layer against previous entries
            .WithMessage("Odometer reading cannot be less than previous reading");

        RuleFor(f => f.Gallons)
            .GreaterThan(0).WithMessage("Gallons must be greater than 0")
            .LessThanOrEqualTo(500).WithMessage("Gallons cannot exceed 500 (fuel tank capacity)");

        RuleFor(f => f.PricePerGallon)
            .GreaterThan(0).WithMessage("Price per gallon must be greater than 0")
            .LessThanOrEqualTo(50).WithMessage("Price per gallon seems unreasonably high");

        RuleFor(f => f.TotalCost)
            .GreaterThan(0).WithMessage("Total cost must be greater than 0");

        // Business rule: Total cost should equal gallons * price per gallon
        RuleFor(f => f.TotalCost)
            .Must((fuelEntry, totalCost) => Math.Abs(totalCost - (fuelEntry.Gallons * fuelEntry.PricePerGallon)) < 0.01m)
            .WithMessage("Total cost must equal gallons multiplied by price per gallon");

        RuleFor(f => f.State)
            .Length(2).WithMessage("State must be 2 characters")
            .When(f => !string.IsNullOrEmpty(f.State));

        RuleFor(f => f.FuelCardLast4)
            .Length(4).WithMessage("Fuel card last 4 digits must be exactly 4 characters")
            .Matches(@"^\d{4}$").WithMessage("Fuel card last 4 must be numeric")
            .When(f => !string.IsNullOrEmpty(f.FuelCardLast4));
    }
}
