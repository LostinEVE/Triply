using Triply.Core.Enums;

namespace Triply.Core.Models;

public class Driver
{
    public Guid DriverId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CDLNumber { get; set; }
    public string? CDLState { get; set; }
    public DateTime? CDLExpiration { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AssignedTruckId { get; set; }
    public DateTime? HireDate { get; set; }
    public decimal? PayRate { get; set; }
    public PayType PayType { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public Truck? AssignedTruck { get; set; }
    public ICollection<Load> Loads { get; set; } = new List<Load>();
    public ICollection<FuelEntry> FuelEntries { get; set; } = new List<FuelEntry>();
}
