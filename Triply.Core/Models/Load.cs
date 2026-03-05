using Triply.Core.Enums;

namespace Triply.Core.Models;

public class Load
{
    public Guid LoadId { get; set; }
    public string LoadNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string? TruckId { get; set; }
    public Guid? DriverId { get; set; }
    
    // Pickup details
    public string? PickupAddress { get; set; }
    public string? PickupCity { get; set; }
    public string? PickupState { get; set; }
    public string? PickupZip { get; set; }
    public DateTime? PickupDate { get; set; }
    public TimeSpan? PickupTime { get; set; }
    
    // Delivery details
    public string? DeliveryAddress { get; set; }
    public string? DeliveryCity { get; set; }
    public string? DeliveryState { get; set; }
    public string? DeliveryZip { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public TimeSpan? DeliveryTime { get; set; }
    
    // Financial details
    public int Miles { get; set; }
    public decimal Rate { get; set; }
    public RateType RateType { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Status and documentation
    public LoadStatus Status { get; set; }
    public bool PODReceived { get; set; }
    public byte[]? PODDocument { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public Truck? Truck { get; set; }
    public Driver? Driver { get; set; }
    public ICollection<InvoiceLineItem> InvoiceLineItems { get; set; } = new List<InvoiceLineItem>();
}
