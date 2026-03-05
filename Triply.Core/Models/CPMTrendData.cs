namespace Triply.Core.Models;

public class CPMTrendData
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    public int TotalMiles { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalCPM { get; set; }
    public decimal FuelCPM { get; set; }
    public decimal MaintenanceCPM { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenuePerMile { get; set; }
    public decimal ProfitPerMile { get; set; }
    public double AverageMPG { get; set; }
}

public class CPMTrendReport
{
    public string? TruckId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CPMTrendData> MonthlyTrends { get; set; } = new();
    
    // Summary Statistics
    public decimal AverageCPM => MonthlyTrends.Any() ? MonthlyTrends.Average(t => t.TotalCPM) : 0;
    public decimal LowestCPM => MonthlyTrends.Any() ? MonthlyTrends.Min(t => t.TotalCPM) : 0;
    public decimal HighestCPM => MonthlyTrends.Any() ? MonthlyTrends.Max(t => t.TotalCPM) : 0;
    public decimal CPMTrend => CalculateTrend();
    
    private decimal CalculateTrend()
    {
        if (MonthlyTrends.Count < 2) return 0;
        
        var firstMonth = MonthlyTrends.First().TotalCPM;
        var lastMonth = MonthlyTrends.Last().TotalCPM;
        
        if (firstMonth == 0) return 0;
        
        return ((lastMonth - firstMonth) / firstMonth) * 100;
    }
}

public class AnnualProjection
{
    public string? TruckId { get; set; }
    public int EstimatedAnnualMiles { get; set; }
    public decimal CurrentCPM { get; set; }
    
    // Projected Annual Costs
    public decimal ProjectedFuelCost { get; set; }
    public decimal ProjectedMaintenanceCost { get; set; }
    public decimal ProjectedInsuranceCost { get; set; }
    public decimal ProjectedPermitsCost { get; set; }
    public decimal ProjectedTollsCost { get; set; }
    public decimal ProjectedTruckPaymentCost { get; set; }
    public decimal ProjectedDriverPayCost { get; set; }
    public decimal ProjectedTiresCost { get; set; }
    public decimal ProjectedOtherCosts { get; set; }
    
    public decimal ProjectedTotalExpenses =>
        ProjectedFuelCost + ProjectedMaintenanceCost + ProjectedInsuranceCost +
        ProjectedPermitsCost + ProjectedTollsCost + ProjectedTruckPaymentCost +
        ProjectedDriverPayCost + ProjectedTiresCost + ProjectedOtherCosts;
    
    public decimal ProjectedCPM => EstimatedAnnualMiles > 0 ? 
        ProjectedTotalExpenses / EstimatedAnnualMiles : 0;
    
    // Revenue Projections
    public decimal RequiredRevenuePerMile { get; set; }
    public decimal ProjectedAnnualRevenue => EstimatedAnnualMiles * RequiredRevenuePerMile;
    public decimal ProjectedAnnualProfit => ProjectedAnnualRevenue - ProjectedTotalExpenses;
    public decimal ProjectedProfitMargin => ProjectedAnnualRevenue > 0 ? 
        (ProjectedAnnualProfit / ProjectedAnnualRevenue) * 100 : 0;
}
