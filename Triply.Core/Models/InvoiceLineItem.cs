namespace Triply.Core.Models;

public class InvoiceLineItem
{
    public int LineItemId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid? LoadId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    
    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
    public Load? Load { get; set; }
}
