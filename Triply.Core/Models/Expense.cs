using Triply.Core.Enums;

namespace Triply.Core.Models;

public class Expense
{
    public Guid ExpenseId { get; set; }
    public string? TruckId { get; set; }
    public DateTime ExpenseDate { get; set; }
    public ExpenseCategory Category { get; set; }
    public string? Vendor { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public byte[]? ReceiptImage { get; set; }
    public bool IsDeductible { get; set; }
    public string? TaxCategory { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Truck? Truck { get; set; }
}
