using Triply.Core.Enums;

namespace Triply.Core.Models;

public class MaintenanceRecord
{
    public Guid MaintenanceId { get; set; }
    public string TruckId { get; set; } = string.Empty;
    public DateTime MaintenanceDate { get; set; }
    public int Odometer { get; set; }
    public MaintenanceType Type { get; set; }
    public string? Description { get; set; }
    public string? Vendor { get; set; }
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal TotalCost { get; set; }
    public bool IsWarranty { get; set; }
    public int? NextDueOdometer { get; set; }
    public DateTime? NextDueDate { get; set; }
    public byte[]? Documents { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Truck Truck { get; set; } = null!;
}
