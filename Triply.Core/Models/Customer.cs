namespace Triply.Core.Models;

public class Customer
{
    public Guid CustomerId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingZip { get; set; }
    public string PaymentTerms { get; set; } = "Net30";
    public string? Notes { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public ICollection<Load> Loads { get; set; } = new List<Load>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
