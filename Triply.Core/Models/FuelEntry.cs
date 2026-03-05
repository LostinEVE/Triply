using Triply.Core.Enums;

namespace Triply.Core.Models;

public class FuelEntry
{
    public Guid FuelEntryId { get; set; }
    public string TruckId { get; set; } = string.Empty;
    public Guid? DriverId { get; set; }
    public DateTime FuelDate { get; set; }
    public int Odometer { get; set; }
    public decimal Gallons { get; set; }
    public decimal PricePerGallon { get; set; }
    public decimal TotalCost { get; set; }
    public FuelType FuelType { get; set; }
    public string? TruckStop { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? FuelCardLast4 { get; set; }
    public byte[]? ReceiptImage { get; set; }
    public string? Notes { get; set; }
    public string? IFTA_Quarter { get; set; }
    
    // Navigation properties
    public Truck Truck { get; set; } = null!;
    public Driver? Driver { get; set; }
}
