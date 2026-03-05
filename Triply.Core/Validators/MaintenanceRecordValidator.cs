using FluentValidation;
using Triply.Core.Models;

namespace Triply.Core.Validators;

public class MaintenanceRecordValidator : AbstractValidator<MaintenanceRecord>
{
    public MaintenanceRecordValidator()
    {
        RuleFor(m => m.TruckId)
            .NotEmpty().WithMessage("Truck is required");

        RuleFor(m => m.MaintenanceDate)
            .NotEmpty().WithMessage("Maintenance date is required")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Maintenance date cannot be in the future");

        RuleFor(m => m.Odometer)
            .GreaterThan(0).WithMessage("Odometer must be greater than 0");

        RuleFor(m => m.LaborCost)
            .GreaterThanOrEqualTo(0).WithMessage("Labor cost cannot be negative");

        RuleFor(m => m.PartsCost)
            .GreaterThanOrEqualTo(0).WithMessage("Parts cost cannot be negative");

        RuleFor(m => m.TotalCost)
            .GreaterThanOrEqualTo(0).WithMessage("Total cost cannot be negative");

        // Business rule: Total cost should equal labor cost + parts cost
        RuleFor(m => m.TotalCost)
            .Must((maintenance, totalCost) => Math.Abs(totalCost - (maintenance.LaborCost + maintenance.PartsCost)) < 0.01m)
            .WithMessage("Total cost must equal labor cost plus parts cost");

        RuleFor(m => m.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(m => !string.IsNullOrEmpty(m.Description));

        RuleFor(m => m.Vendor)
            .MaximumLength(200).WithMessage("Vendor name cannot exceed 200 characters")
            .When(m => !string.IsNullOrEmpty(m.Vendor));

        RuleFor(m => m.NextDueOdometer)
            .GreaterThan(m => m.Odometer).WithMessage("Next due odometer must be greater than current odometer")
            .When(m => m.NextDueOdometer.HasValue);

        RuleFor(m => m.NextDueDate)
            .GreaterThan(m => m.MaintenanceDate).WithMessage("Next due date must be after maintenance date")
            .When(m => m.NextDueDate.HasValue);
    }
}
