# Cost Per Mile (CPM) Service - Complete Guide

## Overview

The `CostPerMileService` provides comprehensive cost-per-mile analysis for OTR trucking operations. CPM is the most critical metric in trucking profitability, representing the total cost to operate per mile driven.

## Why CPM Matters

- **Profitability**: Determines if you're making money on each load
- **Rate Negotiation**: Know your break-even rate
- **Cost Control**: Identify which expenses are eating into profits
- **Fleet Management**: Compare truck performance
- **Business Decisions**: Lease vs buy, when to replace trucks

## Core Methods

### 1. CalculateCPMAsync()

Calculates a comprehensive CPM report for a specific period.

```csharp
CPMReport report = await cpmService.CalculateCPMAsync(
    startDate: DateTime.UtcNow.AddMonths(-1),
    endDate: DateTime.UtcNow,
    truckId: "TRUCK-001" // or null for fleet-wide
);

Console.WriteLine($"Total CPM: {report.TotalCPM:C}");
Console.WriteLine($"Profit per Mile: {report.ProfitPerMile:C}");
Console.WriteLine($"Break-Even Rate: {report.BreakEvenRatePerMile:C}");
```

**CPMReport includes:**

#### Cost Breakdown
- `FuelCost`, `FuelCPM` - Diesel and DEF costs
- `MaintenanceCost`, `MaintenanceCPM` - Repairs and service
- `InsuranceCost`, `InsuranceCPM` - Commercial insurance
- `PermitsCost`, `PermitsCPM` - Licenses and permits
- `TollsCost`, `TollsCPM` - Road tolls
- `TruckPaymentCost`, `TruckPaymentCPM` - Lease/loan payments
- `DriverPayCost`, `DriverPayCPM` - Driver compensation
- `TiresCost`, `TiresCPM` - Tire replacement
- `OtherExpenses`, `OtherCPM` - Parking, scales, lumper, etc.

#### Summary Metrics
- `TotalExpenses` - Sum of all expenses
- `TotalCPM` - Total cost per mile
- `TotalMiles` - Miles driven in period
- `TotalLoads` - Number of loads completed

#### Fixed vs Variable
- `FixedCosts`, `FixedCPM` - Insurance, permits, truck payment
- `VariableCosts`, `VariableCPM` - Fuel, maintenance, tolls, etc.

#### Revenue & Profit
- `TotalRevenue` - Revenue from loads
- `RevenuePerMile` - Average revenue per mile
- `ProfitPerMile` - Revenue - Expenses per mile
- `TotalProfit` - Total profit in period
- `ProfitMargin` - Profit as % of revenue

#### Break-Even
- `BreakEvenRatePerMile` - Minimum rate to cover costs
- `TargetRatePerMile` - Rate for 15% profit margin

#### Fuel Efficiency
- `TotalGallons` - Diesel consumed
- `AverageMPG` - Miles per gallon

### 2. CalculateCPMTrendsAsync()

Analyzes CPM trends over multiple months.

```csharp
CPMTrendReport trends = await cpmService.CalculateCPMTrendsAsync(
    startDate: DateTime.UtcNow.AddMonths(-6),
    endDate: DateTime.UtcNow,
    truckId: null // Fleet-wide
);

foreach (var month in trends.MonthlyTrends)
{
    Console.WriteLine($"{month.MonthName}: {month.TotalCPM:C} CPM");
}

Console.WriteLine($"Average CPM: {trends.AverageCPM:C}");
Console.WriteLine($"CPM Trend: {trends.CPMTrend:N1}%");
```

**Use Cases:**
- Identify seasonal patterns
- Track improvement/decline over time
- Predict future costs
- Measure impact of cost reduction initiatives

### 3. CalculateAnnualProjectionAsync()

Projects annual costs based on historical data.

```csharp
AnnualProjection projection = await cpmService.CalculateAnnualProjectionAsync(
    estimatedAnnualMiles: 120000,
    truckId: "TRUCK-001",
    historicalMonths: 6
);

Console.WriteLine($"Projected Annual Expenses: {projection.ProjectedTotalExpenses:C}");
Console.WriteLine($"Required Rate: {projection.RequiredRevenuePerMile:C}/mile");
Console.WriteLine($"Projected Profit: {projection.ProjectedAnnualProfit:C}");
```

**Use Cases:**
- Budgeting for next year
- Evaluating new truck purchase
- Setting annual goals
- Financial planning

### 4. CompareCPMAsync()

Compares CPM between two trucks or time periods.

```csharp
CPMComparisonReport comparison = await cpmService.CompareCPMAsync(
    period1Start: DateTime.UtcNow.AddMonths(-2),
    period1End: DateTime.UtcNow.AddMonths(-1),
    period2Start: DateTime.UtcNow.AddMonths(-1),
    period2End: DateTime.UtcNow,
    truck1Id: "TRUCK-001",
    truck2Id: "TRUCK-002"
);

Console.WriteLine($"CPM Difference: {comparison.TotalCPMDifference:C}");
Console.WriteLine($"Change: {comparison.TotalCPMChangePercent:N1}%");
```

**Use Cases:**
- Compare truck efficiency
- Track month-over-month changes
- Identify which truck is more profitable
- Decide which truck to assign to premium loads

### 5. CalculateFleetCPMAsync()

Calculates CPM for all active trucks.

```csharp
Dictionary<string, CPMReport> fleetReports = await cpmService.CalculateFleetCPMAsync(
    startDate: DateTime.UtcNow.AddMonths(-1),
    endDate: DateTime.UtcNow
);

foreach (var (truckId, report) in fleetReports)
{
    Console.WriteLine($"{truckId}: {report.TotalCPM:C} CPM, {report.ProfitPerMile:C} profit/mile");
}
```

**Use Cases:**
- Identify underperforming trucks
- Fleet-wide cost analysis
- Decide which trucks to retire
- Optimize fleet composition

### 6. CalculateBreakEvenRateAsync()

Detailed break-even analysis with minimum miles calculation.

```csharp
BreakEvenAnalysis breakEven = await cpmService.CalculateBreakEvenRateAsync(
    startDate: DateTime.UtcNow.AddMonths(-1),
    endDate: DateTime.UtcNow,
    truckId: "TRUCK-001",
    desiredProfitMargin: 0.15m // 15%
);

Console.WriteLine($"Break-Even Rate: {breakEven.BreakEvenRate:C}/mile");
Console.WriteLine($"Target Rate: {breakEven.TargetRateWithProfit:C}/mile");
Console.WriteLine($"Minimum Monthly Miles: {breakEven.MinimumMonthlyMiles:N0}");
Console.WriteLine($"Fixed Costs/Mile: {breakEven.FixedCostPerMile:C}");
```

**Use Cases:**
- Rate negotiation
- Load acceptance decisions
- Calculate minimum miles to be profitable
- Understand cost structure

## Real-World Usage Examples

### Example 1: Monthly Profitability Dashboard

```csharp
public class DashboardService
{
    private readonly CostPerMileService _cpmService;

    public async Task<MonthlyDashboard> GetMonthlyDashboardAsync()
    {
        var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var nextMonth = thisMonth.AddMonths(1);

        var fleetCPM = await _cpmService.CalculateFleetCPMAsync(thisMonth, nextMonth.AddDays(-1));

        return new MonthlyDashboard
        {
            TotalTrucks = fleetCPM.Count,
            FleetAverageCPM = fleetCPM.Values.Average(r => r.TotalCPM),
            ProfitableTrucks = fleetCPM.Count(kvp => kvp.Value.ProfitPerMile > 0),
            BestPerformer = fleetCPM.OrderByDescending(kvp => kvp.Value.ProfitPerMile).First().Key,
            WorstPerformer = fleetCPM.OrderBy(kvp => kvp.Value.ProfitPerMile).First().Key
        };
    }
}
```

### Example 2: Load Acceptance Decision

```csharp
public async Task<bool> ShouldAcceptLoadAsync(
    string truckId, 
    int miles, 
    decimal offeredRate)
{
    // Get recent CPM data
    var endDate = DateTime.UtcNow;
    var startDate = endDate.AddMonths(-1);
    
    var report = await _cpmService.CalculateCPMAsync(startDate, endDate, truckId);

    // Check if offered rate covers costs + desired profit
    var minimumAcceptableRate = report.TotalCPM * 1.10m; // Want 10% profit minimum

    if (offeredRate < minimumAcceptableRate)
    {
        Console.WriteLine($"❌ REJECT: Rate ${offeredRate:N2} is below minimum ${minimumAcceptableRate:N2}");
        return false;
    }

    var projectedProfit = (offeredRate - report.TotalCPM) * miles;
    Console.WriteLine($"✅ ACCEPT: Projected profit: ${projectedProfit:N2}");
    return true;
}
```

### Example 3: Cost Reduction Tracking

```csharp
public async Task<CostReductionReport> TrackCostReductionAsync(string truckId)
{
    var thisMonth = DateTime.UtcNow.AddMonths(-1);
    var lastMonth = thisMonth.AddMonths(-1);

    var comparison = await _cpmService.CompareCPMAsync(
        lastMonth, thisMonth.AddDays(-1),
        thisMonth, DateTime.UtcNow,
        truckId, truckId
    );

    return new CostReductionReport
    {
        TruckId = truckId,
        FuelSavings = comparison.Period1Report.FuelCPM - comparison.Period2Report.FuelCPM,
        MaintenanceSavings = comparison.Period1Report.MaintenanceCPM - comparison.Period2Report.MaintenanceCPM,
        TotalSavings = comparison.Period1Report.TotalCPM - comparison.Period2Report.TotalCPM,
        PercentImprovement = comparison.TotalCPMChangePercent,
        Message = comparison.TotalCPMChangePercent < 0 ? 
            "✅ Costs decreased" : "⚠️ Costs increased"
    };
}
```

### Example 4: New Truck ROI Analysis

```csharp
public async Task<NewTruckROI> AnalyzeNewTruckPurchaseAsync(
    decimal purchasePrice,
    decimal downPayment,
    decimal monthlyPayment,
    int estimatedAnnualMiles)
{
    // Get current fleet average
    var endDate = DateTime.UtcNow;
    var startDate = endDate.AddMonths(-6);
    
    var fleetCPM = await _cpmService.CalculateFleetCPMAsync(startDate, endDate);
    var avgCPM = fleetCPM.Values.Average(r => r.TotalCPM);
    var avgRevenuePerMile = fleetCPM.Values.Average(r => r.RevenuePerMile);

    // Project new truck costs (assume similar to fleet average, but add new payment)
    var newTruckCPM = avgCPM + (monthlyPayment * 12 / estimatedAnnualMiles);
    var projectedRevenue = avgRevenuePerMile * estimatedAnnualMiles;
    var projectedExpenses = newTruckCPM * estimatedAnnualMiles;
    var annualProfit = projectedRevenue - projectedExpenses;
    
    var paybackYears = (purchasePrice - downPayment) / annualProfit;

    return new NewTruckROI
    {
        ProjectedCPM = newTruckCPM,
        ProjectedAnnualProfit = annualProfit,
        PaybackPeriodYears = paybackYears,
        IsGoodInvestment = paybackYears <= 3 && annualProfit > 0
    };
}
```

### Example 5: Rate Negotiation Tool

```csharp
public async Task<RateNegotiationData> GetNegotiationDataAsync(
    string truckId,
    int loadMiles,
    string origin,
    string destination)
{
    var report = await _cpmService.CalculateCPMAsync(
        DateTime.UtcNow.AddMonths(-1),
        DateTime.UtcNow,
        truckId
    );

    var breakEven = await _cpmService.CalculateBreakEvenRateAsync(
        DateTime.UtcNow.AddMonths(-1),
        DateTime.UtcNow,
        truckId
    );

    return new RateNegotiationData
    {
        // Bottom line - won't haul for less
        AbsoluteMinimum = report.TotalCPM,
        
        // Acceptable with tight margin
        BreakEvenRate = report.TotalCPM * 1.05m,
        
        // Target rate for good profit
        TargetRate = report.TotalCPM * 1.15m,
        
        // Ideal rate
        IdealRate = report.TotalCPM * 1.25m,
        
        LoadRevenue = new
        {
            AtMinimum = loadMiles * report.TotalCPM,
            AtTarget = loadMiles * (report.TotalCPM * 1.15m),
            AtIdeal = loadMiles * (report.TotalCPM * 1.25m)
        },
        
        Message = $"Won't haul for less than ${report.TotalCPM:N2}/mile. " +
                 $"Target ${(report.TotalCPM * 1.15m):N2}/mile for {origin} to {destination}."
    };
}
```

### Example 6: Maintenance Decision Support

```csharp
public async Task<MaintenanceDecision> ShouldRepairOrReplaceAsync(
    string truckId,
    decimal repairCost,
    int truckAge,
    int currentMileage)
{
    // Get recent performance
    var report = await _cpmService.CalculateCPMAsync(
        DateTime.UtcNow.AddMonths(-3),
        DateTime.UtcNow,
        truckId
    );

    // Calculate if maintenance CPM is excessive
    var maintenanceRatio = report.MaintenanceCost / report.TotalExpenses;
    var isHighMaintenance = maintenanceRatio > 0.20m; // > 20% of costs

    // Calculate payback period for repair
    var monthlyProfit = report.ProfitPerMile * (report.TotalMiles / 3); // 3 months avg
    var repairPaybackMonths = repairCost / monthlyProfit;

    var decision = new MaintenanceDecision
    {
        TruckId = truckId,
        RepairCost = repairCost,
        CurrentMaintenanceCPM = report.MaintenanceCPM,
        MaintenancePercentage = maintenanceRatio * 100,
        IsHighMaintenance = isHighMaintenance,
        RepairPaybackMonths = repairPaybackMonths,
        
        Recommendation = repairPaybackMonths > 6 || isHighMaintenance || truckAge > 10 ?
            "🚨 Consider Replacing - High maintenance costs" :
            "✅ Repair - Still economical"
    };

    return decision;
}
```

## Industry Benchmarks

### Typical CPM Breakdown (2024)

| Category | Typical CPM | % of Total |
|----------|-------------|------------|
| Fuel | $0.40 - $0.65 | 25-35% |
| Driver Pay | $0.35 - $0.50 | 20-25% |
| Truck Payment | $0.20 - $0.35 | 12-18% |
| Maintenance | $0.15 - $0.25 | 10-12% |
| Insurance | $0.08 - $0.12 | 5-7% |
| Tires | $0.03 - $0.05 | 2-3% |
| Tolls | $0.02 - $0.04 | 1-2% |
| Permits | $0.01 - $0.02 | 1% |
| Other | $0.05 - $0.10 | 3-5% |
| **TOTAL** | **$1.50 - $2.10** | **100%** |

### Target Metrics

- **Total CPM**: $1.50 - $2.10 (varies by region)
- **Revenue per Mile**: $2.00 - $2.50
- **Profit Margin**: 10-20%
- **Fuel MPG**: 6.0 - 7.5 MPG
- **Fixed CPM**: $0.30 - $0.50
- **Variable CPM**: $1.20 - $1.60

## Best Practices

### ✅ DO

1. **Calculate monthly** - Track trends, identify issues early
2. **Compare trucks** - Find which are most profitable
3. **Use for rate negotiation** - Never haul below break-even
4. **Track fuel efficiency** - MPG impacts CPM significantly
5. **Set targets** - Aim for specific CPM improvements
6. **Review before major purchases** - Use for ROI analysis

### ❌ DON'T

1. **Don't ignore fixed costs** - They still need to be covered
2. **Don't compare different periods** - Seasonal variations exist
3. **Don't forget deadhead miles** - Include in calculations
4. **Don't use outdated data** - Costs change frequently
5. **Don't accept loads below CPM** - Losing money per mile
6. **Don't forget driver pay** - Major component of CPM

## Performance Tips

### Reduce Fuel CPM
- Maintain tire pressure
- Reduce idle time
- Use cruise control
- Plan efficient routes
- Train drivers on fuel economy

### Reduce Maintenance CPM
- Follow preventive maintenance schedule
- Address issues early
- Use quality parts
- Track maintenance history
- Consider warranty coverage

### Increase Revenue per Mile
- Negotiate better rates
- Reduce deadhead miles
- Target high-paying lanes
- Build broker relationships
- Avoid low-paying freight

## Integration with Other Services

### With IFTA Reporting
```csharp
var cpmReport = await _cpmService.CalculateCPMAsync(...);
var iftaReport = await _iftaService.GenerateReportAsync(...);

// Correlate fuel costs with IFTA data
var fuelEfficiency = new
{
    CPMFuelCost = cpmReport.FuelCPM,
    IFTATaxOwed = iftaReport.TotalTaxOwed,
    TruePerMileFuelCost = cpmReport.FuelCPM + (iftaReport.TotalTaxOwed / cpmReport.TotalMiles)
};
```

### With Load Management
```csharp
// Before accepting a load
var cpm = await _cpmService.CalculateCPMAsync(...);
var shouldAccept = offeredRate > cpm.BreakEvenRatePerMile;

if (shouldAccept)
{
    await _loadService.AcceptLoadAsync(loadId);
}
```

## Troubleshooting

### Issue: CPM Seems Too High

**Check:**
1. Are all trucks included? (filter by truckId)
2. Are deadhead miles included?
3. Are one-time expenses skewing data?
4. Is maintenance catching up from deferred service?

### Issue: Negative Profit Per Mile

**Actions:**
1. Review rate structure - need higher rates
2. Analyze expense categories - where can you cut?
3. Consider fuel efficiency improvements
4. Renegotiate insurance/truck payments
5. May need to sideline unprofitable trucks

### Issue: CPM Varies Wildly Month-to-Month

**Causes:**
1. Seasonal fuel price changes
2. Major maintenance events
3. Variable mileage (fixed costs spread over fewer miles)
4. Mix of loaded vs deadhead miles

**Solution:** Use 3-6 month rolling average for stability

## API Reference

### CostPerMileService

```csharp
// Calculate CPM for period
Task<CPMReport> CalculateCPMAsync(DateTime start, DateTime end, string? truckId)

// Calculate monthly trends
Task<CPMTrendReport> CalculateCPMTrendsAsync(DateTime start, DateTime end, string? truckId)

// Project annual costs
Task<AnnualProjection> CalculateAnnualProjectionAsync(int miles, string? truckId, int months = 6)

// Compare two periods or trucks
Task<CPMComparisonReport> CompareCPMAsync(DateTime p1Start, DateTime p1End, DateTime p2Start, DateTime p2End, string? truck1, string? truck2)

// Calculate fleet-wide CPM
Task<Dictionary<string, CPMReport>> CalculateFleetCPMAsync(DateTime start, DateTime end)

// Break-even analysis
Task<BreakEvenAnalysis> CalculateBreakEvenRateAsync(DateTime start, DateTime end, string? truckId, decimal margin = 0.15m)
```

## Summary

The CostPerMileService is essential for:
- ✅ Understanding true operating costs
- ✅ Making data-driven business decisions
- ✅ Negotiating profitable rates
- ✅ Identifying cost reduction opportunities
- ✅ Tracking fleet performance
- ✅ Planning for growth

**Remember:** In trucking, pennies per mile add up to thousands per year!
