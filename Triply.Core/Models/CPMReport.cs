namespace Triply.Core.Models;

public class CPMReport
{
    // Period Information
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? TruckId { get; set; }
    public int TotalMiles { get; set; }
    
    // Fuel Costs
    public decimal FuelCost { get; set; }
    public decimal FuelCPM => TotalMiles > 0 ? FuelCost / TotalMiles : 0;
    
    // Maintenance Costs
    public decimal MaintenanceCost { get; set; }
    public decimal MaintenanceCPM => TotalMiles > 0 ? MaintenanceCost / TotalMiles : 0;
    
    // Insurance Costs
    public decimal InsuranceCost { get; set; }
    public decimal InsuranceCPM => TotalMiles > 0 ? InsuranceCost / TotalMiles : 0;
    
    // Permits & Licenses
    public decimal PermitsCost { get; set; }
    public decimal PermitsCPM => TotalMiles > 0 ? PermitsCost / TotalMiles : 0;
    
    // Tolls
    public decimal TollsCost { get; set; }
    public decimal TollsCPM => TotalMiles > 0 ? TollsCost / TotalMiles : 0;
    
    // Truck Payments
    public decimal TruckPaymentCost { get; set; }
    public decimal TruckPaymentCPM => TotalMiles > 0 ? TruckPaymentCost / TotalMiles : 0;
    
    // Driver Pay
    public decimal DriverPayCost { get; set; }
    public decimal DriverPayCPM => TotalMiles > 0 ? DriverPayCost / TotalMiles : 0;
    
    // Tires
    public decimal TiresCost { get; set; }
    public decimal TiresCPM => TotalMiles > 0 ? TiresCost / TotalMiles : 0;
    
    // Other Expenses (Parking, Scales, Lumper, Office, etc.)
    public decimal OtherExpenses { get; set; }
    public decimal OtherCPM => TotalMiles > 0 ? OtherExpenses / TotalMiles : 0;
    
    // Total Operating Costs
    public decimal TotalExpenses => 
        FuelCost + MaintenanceCost + InsuranceCost + PermitsCost + 
        TollsCost + TruckPaymentCost + DriverPayCost + TiresCost + OtherExpenses;
    
    public decimal TotalCPM => TotalMiles > 0 ? TotalExpenses / TotalMiles : 0;
    
    // Fixed vs Variable Costs
    public decimal FixedCosts => InsuranceCost + PermitsCost + TruckPaymentCost;
    public decimal FixedCPM => TotalMiles > 0 ? FixedCosts / TotalMiles : 0;
    
    public decimal VariableCosts => 
        FuelCost + MaintenanceCost + TollsCost + DriverPayCost + TiresCost + OtherExpenses;
    public decimal VariableCPM => TotalMiles > 0 ? VariableCosts / TotalMiles : 0;
    
    // Revenue & Profit
    public decimal TotalRevenue { get; set; }
    public decimal RevenuePerMile => TotalMiles > 0 ? TotalRevenue / TotalMiles : 0;
    
    public decimal ProfitPerMile => RevenuePerMile - TotalCPM;
    public decimal TotalProfit => TotalRevenue - TotalExpenses;
    public decimal ProfitMargin => TotalRevenue > 0 ? (TotalProfit / TotalRevenue) * 100 : 0;
    
    // Break-Even Analysis
    public decimal BreakEvenRatePerMile => TotalCPM;
    public decimal TargetRatePerMile => TotalCPM * 1.15m; // Target 15% profit margin
    
    // Average MPG (Miles Per Gallon)
    public decimal TotalGallons { get; set; }
    public double AverageMPG => TotalGallons > 0 ? (double)TotalMiles / (double)TotalGallons : 0;
    
    // Load Statistics
    public int TotalLoads { get; set; }
    public decimal AverageRevenuePerLoad => TotalLoads > 0 ? TotalRevenue / TotalLoads : 0;
    public decimal AverageMilesPerLoad => TotalLoads > 0 ? TotalMiles / TotalLoads : 0;
}
