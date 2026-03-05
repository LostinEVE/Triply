using Microsoft.EntityFrameworkCore;
using Triply.Core.Enums;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public class CostPerMileService
{
    private readonly IUnitOfWork _unitOfWork;

    public CostPerMileService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Calculates comprehensive CPM report for a specific truck or entire fleet
    /// </summary>
    public async Task<CPMReport> CalculateCPMAsync(
        DateTime startDate, 
        DateTime endDate, 
        string? truckId = null)
    {
        var report = new CPMReport
        {
            StartDate = startDate,
            EndDate = endDate,
            TruckId = truckId
        };

        // Calculate total miles from loads
        var loadsQuery = _unitOfWork.Loads.GetQueryable()
            .Where(l => l.DeliveryDate >= startDate && l.DeliveryDate <= endDate);

        if (!string.IsNullOrEmpty(truckId))
            loadsQuery = loadsQuery.Where(l => l.TruckId == truckId);

        var loads = await loadsQuery.ToListAsync();
        report.TotalMiles = loads.Sum(l => l.Miles);
        report.TotalLoads = loads.Count;
        
        // If no miles from loads, try to calculate from fuel entries (odometer differences)
        if (report.TotalMiles == 0)
        {
            report.TotalMiles = await CalculateMilesFromOdometerAsync(startDate, endDate, truckId);
        }

        // Calculate revenue from paid/delivered loads
        report.TotalRevenue = loads
            .Where(l => l.Status == LoadStatus.Paid || l.Status == LoadStatus.Delivered)
            .Sum(l => l.TotalAmount);

        // Calculate fuel costs and gallons
        var fuelQuery = _unitOfWork.FuelEntries.GetQueryable()
            .Where(f => f.FuelDate >= startDate && f.FuelDate <= endDate);

        if (!string.IsNullOrEmpty(truckId))
            fuelQuery = fuelQuery.Where(f => f.TruckId == truckId);

        var fuelEntries = await fuelQuery.ToListAsync();
        report.FuelCost = fuelEntries.Sum(f => f.TotalCost);
        report.TotalGallons = fuelEntries.Sum(f => f.Gallons);

        // Calculate maintenance costs
        var maintenanceQuery = _unitOfWork.MaintenanceRecords.GetQueryable()
            .Where(m => m.MaintenanceDate >= startDate && m.MaintenanceDate <= endDate);

        if (!string.IsNullOrEmpty(truckId))
            maintenanceQuery = maintenanceQuery.Where(m => m.TruckId == truckId);

        report.MaintenanceCost = await maintenanceQuery.SumAsync(m => m.TotalCost);

        // Calculate categorized expenses
        var expensesQuery = _unitOfWork.Expenses.GetQueryable()
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate);

        if (!string.IsNullOrEmpty(truckId))
            expensesQuery = expensesQuery.Where(e => e.TruckId == truckId);

        var expenses = await expensesQuery.ToListAsync();

        report.InsuranceCost = expenses
            .Where(e => e.Category == ExpenseCategory.Insurance)
            .Sum(e => e.Amount);

        report.PermitsCost = expenses
            .Where(e => e.Category == ExpenseCategory.Permits)
            .Sum(e => e.Amount);

        report.TollsCost = expenses
            .Where(e => e.Category == ExpenseCategory.Tolls)
            .Sum(e => e.Amount);

        report.TruckPaymentCost = expenses
            .Where(e => e.Category == ExpenseCategory.TruckPayment)
            .Sum(e => e.Amount);

        report.DriverPayCost = expenses
            .Where(e => e.Category == ExpenseCategory.DriverPay)
            .Sum(e => e.Amount);

        report.TiresCost = expenses
            .Where(e => e.Category == ExpenseCategory.Tires)
            .Sum(e => e.Amount);

        report.OtherExpenses = expenses
            .Where(e => e.Category == ExpenseCategory.Parking ||
                       e.Category == ExpenseCategory.Scales ||
                       e.Category == ExpenseCategory.Lumper ||
                       e.Category == ExpenseCategory.OfficeExpense ||
                       e.Category == ExpenseCategory.Trailer ||
                       e.Category == ExpenseCategory.Other)
            .Sum(e => e.Amount);

        return report;
    }

    /// <summary>
    /// Calculates CPM trends over monthly periods
    /// </summary>
    public async Task<CPMTrendReport> CalculateCPMTrendsAsync(
        DateTime startDate,
        DateTime endDate,
        string? truckId = null)
    {
        var trendReport = new CPMTrendReport
        {
            StartDate = startDate,
            EndDate = endDate,
            TruckId = truckId
        };

        var currentDate = new DateTime(startDate.Year, startDate.Month, 1);
        var endMonth = new DateTime(endDate.Year, endDate.Month, 1);

        while (currentDate <= endMonth)
        {
            var monthStart = currentDate;
            var monthEnd = currentDate.AddMonths(1).AddDays(-1);

            // Calculate CPM for this month
            var monthReport = await CalculateCPMAsync(monthStart, monthEnd, truckId);

            var trendData = new CPMTrendData
            {
                Year = currentDate.Year,
                Month = currentDate.Month,
                TotalMiles = monthReport.TotalMiles,
                TotalExpenses = monthReport.TotalExpenses,
                TotalCPM = monthReport.TotalCPM,
                FuelCPM = monthReport.FuelCPM,
                MaintenanceCPM = monthReport.MaintenanceCPM,
                TotalRevenue = monthReport.TotalRevenue,
                RevenuePerMile = monthReport.RevenuePerMile,
                ProfitPerMile = monthReport.ProfitPerMile,
                AverageMPG = monthReport.AverageMPG
            };

            trendReport.MonthlyTrends.Add(trendData);
            currentDate = currentDate.AddMonths(1);
        }

        return trendReport;
    }

    /// <summary>
    /// Projects annual costs based on historical data and estimated mileage
    /// </summary>
    public async Task<AnnualProjection> CalculateAnnualProjectionAsync(
        int estimatedAnnualMiles,
        string? truckId = null,
        int historicalMonths = 6)
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddMonths(-historicalMonths);

        // Get historical CPM data
        var historicalReport = await CalculateCPMAsync(startDate, endDate, truckId);

        var projection = new AnnualProjection
        {
            TruckId = truckId,
            EstimatedAnnualMiles = estimatedAnnualMiles,
            CurrentCPM = historicalReport.TotalCPM
        };

        // Project costs based on historical CPM
        projection.ProjectedFuelCost = historicalReport.FuelCPM * estimatedAnnualMiles;
        projection.ProjectedMaintenanceCost = historicalReport.MaintenanceCPM * estimatedAnnualMiles;
        projection.ProjectedInsuranceCost = historicalReport.InsuranceCPM * estimatedAnnualMiles;
        projection.ProjectedPermitsCost = historicalReport.PermitsCPM * estimatedAnnualMiles;
        projection.ProjectedTollsCost = historicalReport.TollsCPM * estimatedAnnualMiles;
        projection.ProjectedTruckPaymentCost = historicalReport.TruckPaymentCPM * estimatedAnnualMiles;
        projection.ProjectedDriverPayCost = historicalReport.DriverPayCPM * estimatedAnnualMiles;
        projection.ProjectedTiresCost = historicalReport.TiresCPM * estimatedAnnualMiles;
        projection.ProjectedOtherCosts = historicalReport.OtherCPM * estimatedAnnualMiles;

        // Calculate required revenue for desired profit margin (15%)
        projection.RequiredRevenuePerMile = projection.ProjectedCPM * 1.15m;

        return projection;
    }

    /// <summary>
    /// Compares CPM between two trucks or time periods
    /// </summary>
    public async Task<CPMComparisonReport> CompareCPMAsync(
        DateTime period1Start,
        DateTime period1End,
        DateTime period2Start,
        DateTime period2End,
        string? truck1Id = null,
        string? truck2Id = null)
    {
        var report1 = await CalculateCPMAsync(period1Start, period1End, truck1Id);
        var report2 = await CalculateCPMAsync(period2Start, period2End, truck2Id);

        return new CPMComparisonReport
        {
            Period1Report = report1,
            Period2Report = report2,
            TotalCPMDifference = report2.TotalCPM - report1.TotalCPM,
            FuelCPMDifference = report2.FuelCPM - report1.FuelCPM,
            MaintenanceCPMDifference = report2.MaintenanceCPM - report1.MaintenanceCPM,
            TotalCPMChangePercent = report1.TotalCPM > 0 ? 
                ((report2.TotalCPM - report1.TotalCPM) / report1.TotalCPM) * 100 : 0,
            ProfitPerMileDifference = report2.ProfitPerMile - report1.ProfitPerMile
        };
    }

    /// <summary>
    /// Calculates fleet-wide average CPM for all active trucks
    /// </summary>
    public async Task<Dictionary<string, CPMReport>> CalculateFleetCPMAsync(
        DateTime startDate,
        DateTime endDate)
    {
        var fleetReports = new Dictionary<string, CPMReport>();

        var trucks = await _unitOfWork.Trucks
            .GetQueryable()
            .Where(t => t.Status == TruckStatus.Active)
            .ToListAsync();

        foreach (var truck in trucks)
        {
            var report = await CalculateCPMAsync(startDate, endDate, truck.TruckId);
            fleetReports[truck.TruckId] = report;
        }

        return fleetReports;
    }

    /// <summary>
    /// Calculates break-even rate needed to cover all costs
    /// </summary>
    public async Task<BreakEvenAnalysis> CalculateBreakEvenRateAsync(
        DateTime startDate,
        DateTime endDate,
        string? truckId = null,
        decimal desiredProfitMargin = 0.15m)
    {
        var report = await CalculateCPMAsync(startDate, endDate, truckId);

        return new BreakEvenAnalysis
        {
            TruckId = truckId,
            Period = $"{startDate:MM/dd/yyyy} - {endDate:MM/dd/yyyy}",
            TotalCPM = report.TotalCPM,
            BreakEvenRate = report.TotalCPM,
            TargetRateWithProfit = report.TotalCPM * (1 + desiredProfitMargin),
            FixedCostPerMile = report.FixedCPM,
            VariableCostPerMile = report.VariableCPM,
            CurrentAverageRate = report.RevenuePerMile,
            IsAboveBreakEven = report.RevenuePerMile > report.TotalCPM,
            ProfitMargin = report.ProfitMargin,
            MonthlyFixedCosts = report.FixedCosts,
            MinimumMonthlyMiles = report.FixedCPM > 0 ? 
                (int)(report.FixedCosts / report.FixedCPM) : 0
        };
    }

    #region Private Helper Methods

    private async Task<int> CalculateMilesFromOdometerAsync(
        DateTime startDate,
        DateTime endDate,
        string? truckId)
    {
        var fuelQuery = _unitOfWork.FuelEntries.GetQueryable()
            .Where(f => f.FuelDate >= startDate && f.FuelDate <= endDate);

        if (!string.IsNullOrEmpty(truckId))
            fuelQuery = fuelQuery.Where(f => f.TruckId == truckId);

        var fuelEntries = await fuelQuery.OrderBy(f => f.FuelDate).ToListAsync();

        if (fuelEntries.Count < 2)
            return 0;

        var firstEntry = fuelEntries.First();
        var lastEntry = fuelEntries.Last();

        return lastEntry.Odometer - firstEntry.Odometer;
    }

    #endregion
}

#region Supporting Models

public class CPMComparisonReport
{
    public CPMReport Period1Report { get; set; } = null!;
    public CPMReport Period2Report { get; set; } = null!;
    public decimal TotalCPMDifference { get; set; }
    public decimal FuelCPMDifference { get; set; }
    public decimal MaintenanceCPMDifference { get; set; }
    public decimal TotalCPMChangePercent { get; set; }
    public decimal ProfitPerMileDifference { get; set; }
}

public class BreakEvenAnalysis
{
    public string? TruckId { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal TotalCPM { get; set; }
    public decimal BreakEvenRate { get; set; }
    public decimal TargetRateWithProfit { get; set; }
    public decimal FixedCostPerMile { get; set; }
    public decimal VariableCostPerMile { get; set; }
    public decimal CurrentAverageRate { get; set; }
    public bool IsAboveBreakEven { get; set; }
    public decimal ProfitMargin { get; set; }
    public decimal MonthlyFixedCosts { get; set; }
    public int MinimumMonthlyMiles { get; set; }
}

#endregion
