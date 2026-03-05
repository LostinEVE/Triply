using Microsoft.EntityFrameworkCore;
using Triply.Core.Interfaces;
using Triply.Core.Models;
using Triply.Core.Enums;

namespace Triply.Services;

public class TruckingBusinessService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeolocationService _geolocationService;

    public TruckingBusinessService(IUnitOfWork unitOfWork, IGeolocationService geolocationService)
    {
        _unitOfWork = unitOfWork;
        _geolocationService = geolocationService;
    }

    public async Task<FuelEntry> RecordFuelPurchaseAsync(string truckId, Guid? driverId, decimal gallons, decimal pricePerGallon, FuelType fuelType)
    {
        var location = await _geolocationService.GetCurrentLocationAsync();

        var truck = await _unitOfWork.Trucks.GetByIdAsync(truckId);
        if (truck == null)
            throw new InvalidOperationException("Truck not found");

        var fuelEntry = new FuelEntry
        {
            FuelEntryId = Guid.NewGuid(),
            TruckId = truckId,
            DriverId = driverId,
            FuelDate = DateTime.UtcNow,
            Odometer = truck.CurrentOdometer,
            Gallons = gallons,
            PricePerGallon = pricePerGallon,
            TotalCost = gallons * pricePerGallon,
            FuelType = fuelType,
            City = location?.City,
            State = location?.State,
            Latitude = location?.Latitude,
            Longitude = location?.Longitude
        };

        await _unitOfWork.FuelEntries.AddAsync(fuelEntry);
        await _unitOfWork.SaveChangesAsync();

        return fuelEntry;
    }

    public async Task<decimal> CalculateMonthlyProfitAsync(int year, int month)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var revenue = await _unitOfWork.Loads
                .GetQueryable()
                .Where(l => l.Status == LoadStatus.Paid &&
                           l.DeliveryDate >= startDate &&
                           l.DeliveryDate < endDate)
                .SumAsync(l => l.TotalAmount);

            var expenses = await _unitOfWork.Expenses
                .GetQueryable()
                .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate < endDate)
                .SumAsync(e => e.Amount);

            var fuelCosts = await _unitOfWork.FuelEntries
                .GetQueryable()
                .Where(f => f.FuelDate >= startDate && f.FuelDate < endDate)
                .SumAsync(f => f.TotalCost);

            var maintenanceCosts = await _unitOfWork.MaintenanceRecords
                .GetQueryable()
                .Where(m => m.MaintenanceDate >= startDate && m.MaintenanceDate < endDate)
                .SumAsync(m => m.TotalCost);

            await _unitOfWork.CommitTransactionAsync();

            return revenue - expenses - fuelCosts - maintenanceCosts;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<IEnumerable<Driver>> GetAvailableDriversAsync()
    {
        return await _unitOfWork.Drivers
            .GetQueryable()
            .Include(d => d.AssignedTruck)
            .Include(d => d.Loads.Where(l => l.Status == LoadStatus.InTransit))
            .Where(d => d.IsActive && !d.Loads.Any(l => l.Status == LoadStatus.InTransit))
            .ToListAsync();
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var stats = new DashboardStats
        {
            TotalTrucks = await _unitOfWork.Trucks.GetQueryable().CountAsync(),
            ActiveDrivers = await _unitOfWork.Drivers.GetQueryable().CountAsync(d => d.IsActive),
            TrucksInShop = await _unitOfWork.Trucks.GetQueryable().CountAsync(t => t.Status == TruckStatus.InShop),
            ActiveLoads = await _unitOfWork.Loads.GetQueryable().CountAsync(l => l.Status == LoadStatus.InTransit),
            PendingInvoices = await _unitOfWork.Invoices.GetQueryable().CountAsync(i => i.Status == InvoiceStatus.Sent),
            OverdueInvoices = await _unitOfWork.Invoices.GetQueryable().CountAsync(i => i.Status == InvoiceStatus.Overdue)
        };

        var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        
        stats.MonthlyRevenue = await _unitOfWork.Loads
            .GetQueryable()
            .Where(l => l.DeliveryDate >= thisMonth && l.Status == LoadStatus.Paid)
            .SumAsync(l => l.TotalAmount);

        stats.MonthlyExpenses = await _unitOfWork.Expenses
            .GetQueryable()
            .Where(e => e.ExpenseDate >= thisMonth)
            .SumAsync(e => e.Amount);

        return stats;
    }

    public async Task ScheduleMaintenanceAsync(string truckId, MaintenanceType type, int currentOdometer, int nextDueOdometer)
    {
        var truck = await _unitOfWork.Trucks.GetByIdAsync(truckId);
        if (truck == null)
            throw new InvalidOperationException("Truck not found");

        var lastMaintenance = await _unitOfWork.MaintenanceRecords
            .GetQueryable()
            .Where(m => m.TruckId == truckId && m.Type == type)
            .OrderByDescending(m => m.MaintenanceDate)
            .FirstOrDefaultAsync();

        var upcomingMaintenance = new MaintenanceRecord
        {
            MaintenanceId = Guid.NewGuid(),
            TruckId = truckId,
            Type = type,
            Description = $"Scheduled {type} maintenance",
            Odometer = currentOdometer,
            NextDueOdometer = nextDueOdometer
        };

        await _unitOfWork.MaintenanceRecords.AddAsync(upcomingMaintenance);
        await _unitOfWork.SaveChangesAsync();
    }
}

public class DashboardStats
{
    public int TotalTrucks { get; set; }
    public int ActiveDrivers { get; set; }
    public int TrucksInShop { get; set; }
    public int ActiveLoads { get; set; }
    public int PendingInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal MonthlyExpenses { get; set; }
}
