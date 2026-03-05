using Microsoft.EntityFrameworkCore;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public class IFTAReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public IFTAReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IFTAQuarterReport> GenerateQuarterlyReportAsync(string truckId, int year, int quarter)
    {
        var quarterKey = $"Q{quarter}-{year}";
        
        var fuelEntries = await _unitOfWork.FuelEntries
            .GetQueryable()
            .Where(f => f.TruckId == truckId && f.IFTA_Quarter == quarterKey)
            .Include(f => f.Truck)
            .OrderBy(f => f.FuelDate)
            .ToListAsync();

        var report = new IFTAQuarterReport
        {
            TruckId = truckId,
            Year = year,
            Quarter = quarter,
            TotalGallons = fuelEntries.Sum(f => f.Gallons),
            TotalCost = fuelEntries.Sum(f => f.TotalCost),
            TotalMiles = await CalculateMilesForQuarterAsync(truckId, year, quarter),
            FuelEntries = fuelEntries.ToList()
        };

        var stateBreakdown = fuelEntries
            .Where(f => !string.IsNullOrEmpty(f.State))
            .GroupBy(f => f.State)
            .Select(g => new IFTAStateBreakdown
            {
                State = g.Key!,
                Gallons = g.Sum(f => f.Gallons),
                TotalCost = g.Sum(f => f.TotalCost)
            })
            .ToList();

        report.StateBreakdowns = stateBreakdown;
        report.AverageMPG = report.TotalGallons > 0 ? report.TotalMiles / (double)report.TotalGallons : 0;

        return report;
    }

    private async Task<int> CalculateMilesForQuarterAsync(string truckId, int year, int quarter)
    {
        var startDate = new DateTime(year, (quarter - 1) * 3 + 1, 1);
        var endDate = startDate.AddMonths(3);

        var loads = await _unitOfWork.Loads
            .GetQueryable()
            .Where(l => l.TruckId == truckId &&
                       l.PickupDate >= startDate &&
                       l.PickupDate < endDate)
            .ToListAsync();

        return loads.Sum(l => l.Miles);
    }
}

public class IFTAQuarterReport
{
    public string TruckId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Quarter { get; set; }
    public decimal TotalGallons { get; set; }
    public decimal TotalCost { get; set; }
    public int TotalMiles { get; set; }
    public double AverageMPG { get; set; }
    public List<FuelEntry> FuelEntries { get; set; } = new();
    public List<IFTAStateBreakdown> StateBreakdowns { get; set; } = new();
}

public class IFTAStateBreakdown
{
    public string State { get; set; } = string.Empty;
    public decimal Gallons { get; set; }
    public decimal TotalCost { get; set; }
}
