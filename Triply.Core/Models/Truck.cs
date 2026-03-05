using Triply.Core.Enums;

namespace Triply.Core.Models;

public class Truck
{
    public string TruckId { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? VIN { get; set; }
    public string? LicensePlate { get; set; }
    public string? LicensePlateState { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public int CurrentOdometer { get; set; }
    public TruckStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime? LastModified { get; set; }

    // Navigation properties
    public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
    public ICollection<Load> Loads { get; set; } = new List<Load>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<FuelEntry> FuelEntries { get; set; } = new List<FuelEntry>();
    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
}
