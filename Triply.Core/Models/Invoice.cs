using Triply.Core.Enums;

namespace Triply.Core.Models;

public class Invoice
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Balance { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime? SentDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
}
