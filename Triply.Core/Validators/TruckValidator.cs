using FluentValidation;
using Triply.Core.Models;

namespace Triply.Core.Validators;

public class TruckValidator : AbstractValidator<Truck>
{
    public TruckValidator()
    {
        RuleFor(t => t.TruckId)
            .NotEmpty().WithMessage("Truck ID is required")
            .MaximumLength(50).WithMessage("Truck ID cannot exceed 50 characters");

        RuleFor(t => t.Make)
            .NotEmpty().WithMessage("Make is required")
            .MaximumLength(100).WithMessage("Make cannot exceed 100 characters");

        RuleFor(t => t.Model)
            .NotEmpty().WithMessage("Model is required")
            .MaximumLength(100).WithMessage("Model cannot exceed 100 characters");

        RuleFor(t => t.Year)
            .GreaterThan(1900).WithMessage("Year must be after 1900")
            .LessThanOrEqualTo(DateTime.Now.Year + 1).WithMessage("Year cannot be in the future");

        RuleFor(t => t.VIN)
            .Length(17).WithMessage("VIN must be exactly 17 characters")
            .When(t => !string.IsNullOrEmpty(t.VIN));

        RuleFor(t => t.LicensePlate)
            .MaximumLength(20).WithMessage("License plate cannot exceed 20 characters")
            .When(t => !string.IsNullOrEmpty(t.LicensePlate));

        RuleFor(t => t.LicensePlateState)
            .Length(2).WithMessage("State must be 2 characters")
            .When(t => !string.IsNullOrEmpty(t.LicensePlateState));

        RuleFor(t => t.PurchaseDate)
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Purchase date cannot be in the future")
            .When(t => t.PurchaseDate.HasValue);

        RuleFor(t => t.PurchasePrice)
            .GreaterThan(0).WithMessage("Purchase price must be greater than 0")
            .When(t => t.PurchasePrice.HasValue);

        RuleFor(t => t.CurrentOdometer)
            .GreaterThanOrEqualTo(0).WithMessage("Odometer cannot be negative");
    }
}
