using FluentValidation;
using Triply.Core.Models;

namespace Triply.Core.Validators;

public class ExpenseValidator : AbstractValidator<Expense>
{
    public ExpenseValidator()
    {
        RuleFor(e => e.ExpenseDate)
            .NotEmpty().WithMessage("Expense date is required")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Expense date cannot be in the future");

        RuleFor(e => e.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(e => e.Vendor)
            .MaximumLength(200).WithMessage("Vendor name cannot exceed 200 characters")
            .When(e => !string.IsNullOrEmpty(e.Vendor));

        RuleFor(e => e.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(e => !string.IsNullOrEmpty(e.Description));

        RuleFor(e => e.TaxCategory)
            .MaximumLength(100).WithMessage("Tax category cannot exceed 100 characters")
            .When(e => !string.IsNullOrEmpty(e.TaxCategory));
    }
}
