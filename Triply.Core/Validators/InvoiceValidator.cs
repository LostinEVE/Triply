using FluentValidation;
using Triply.Core.Models;

namespace Triply.Core.Validators;

public class InvoiceValidator : AbstractValidator<Invoice>
{
    public InvoiceValidator()
    {
        RuleFor(i => i.InvoiceNumber)
            .NotEmpty().WithMessage("Invoice number is required")
            .MaximumLength(50).WithMessage("Invoice number cannot exceed 50 characters");

        RuleFor(i => i.CustomerId)
            .NotEmpty().WithMessage("Customer is required");

        RuleFor(i => i.InvoiceDate)
            .NotEmpty().WithMessage("Invoice date is required")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Invoice date cannot be in the future");

        RuleFor(i => i.DueDate)
            .GreaterThanOrEqualTo(i => i.InvoiceDate)
            .WithMessage("Due date must be on or after invoice date");

        RuleFor(i => i.Subtotal)
            .GreaterThanOrEqualTo(0).WithMessage("Subtotal cannot be negative");

        RuleFor(i => i.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount cannot be negative");

        RuleFor(i => i.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than 0");

        RuleFor(i => i.AmountPaid)
            .GreaterThanOrEqualTo(0).WithMessage("Amount paid cannot be negative")
            .LessThanOrEqualTo(i => i.TotalAmount).WithMessage("Amount paid cannot exceed total amount");

        RuleFor(i => i.Balance)
            .GreaterThanOrEqualTo(0).WithMessage("Balance cannot be negative");

        RuleFor(i => i.LineItems)
            .NotEmpty().WithMessage("Invoice must have at least one line item");

        // Business rule: Total amount should equal subtotal + tax
        RuleFor(i => i.TotalAmount)
            .Must((invoice, total) => Math.Abs(total - (invoice.Subtotal + invoice.TaxAmount)) < 0.01m)
            .WithMessage("Total amount must equal subtotal plus tax amount");

        // Business rule: Balance should equal total - amount paid
        RuleFor(i => i.Balance)
            .Must((invoice, balance) => Math.Abs(balance - (invoice.TotalAmount - invoice.AmountPaid)) < 0.01m)
            .WithMessage("Balance must equal total amount minus amount paid");
    }
}
