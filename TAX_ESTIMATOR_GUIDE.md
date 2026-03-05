# Tax Estimator Service - Complete Guide

## Overview

The `TaxEstimatorService` provides comprehensive tax calculation and planning tools specifically designed for OTR trucking businesses. It handles quarterly estimated taxes, IFTA fuel tax reporting, deduction tracking, and tax compliance deadlines.

## Why This Matters for Trucking

- **Quarterly Estimated Taxes**: Self-employed truckers must pay estimated taxes quarterly to avoid penalties
- **IFTA Compliance**: Multi-state fuel tax reporting is mandatory for interstate trucking
- **Deduction Maximization**: Trucking has unique deductions (per diem, Section 179, etc.)
- **Cash Flow Management**: Know tax liability in advance to avoid surprise tax bills
- **Penalty Avoidance**: Miss estimated payments = 8-10% IRS penalty + interest

## Core Features

### 1. Quarterly Tax Estimation
### 2. IFTA Fuel Tax Reporting  
### 3. Annual Tax Projection
### 4. Deduction Tracking
### 5. Tax Calendar & Deadlines

---

## 1. Quarterly Tax Estimation

Calculates federal self-employment tax, federal income tax, and state income tax for each quarter.

### Method: CalculateQuarterlyTaxAsync()

```csharp
@inject TaxEstimatorService TaxService

var q1Estimate = await TaxService.CalculateQuarterlyTaxAsync(
    year: 2024,
    quarter: 1,
    filingStatus: TaxFilingStatus.Single,
    stateIncomeTaxRate: 5.0m, // 5% state tax
    truckId: null // Fleet-wide or specific truck
);

Console.WriteLine($"Gross Income: {q1Estimate.GrossIncome:C}");
Console.WriteLine($"Total Deductions: {q1Estimate.TotalDeductions:C}");
Console.WriteLine($"Net Profit: {q1Estimate.NetProfit:C}");
Console.WriteLine($"Self-Employment Tax: {q1Estimate.SelfEmploymentTax:C}");
Console.WriteLine($"Federal Income Tax: {q1Estimate.FederalIncomeTax:C}");
Console.WriteLine($"State Income Tax: {q1Estimate.StateIncomeTax:C}");
Console.WriteLine($"");
Console.WriteLine($"💰 Quarterly Payment Due: {q1Estimate.QuarterlyEstimatedPayment:C}");
Console.WriteLine($"Due Date: April 15, 2024");
```

### How It Works

#### Step 1: Calculate Gross Income
- Pulls all **paid invoices** from the quarter
- Only counts invoices marked as "Paid" (cash basis accounting)

#### Step 2: Calculate Deductions
Automatically categorizes expenses by IRS Schedule C categories:
- Fuel expenses
- Maintenance & repairs  
- Insurance
- Permits & licenses
- Tolls & parking
- Depreciation
- Interest
- Office expenses
- Professional fees
- Supplies
- Tires
- Wages (driver pay)
- Per diem
- Other expenses

#### Step 3: Calculate Net Profit
```
Net Profit = Gross Income - Total Deductions
```

#### Step 4: Calculate Self-Employment Tax
```
SE Taxable Income = Net Profit × 92.35%
Self-Employment Tax = SE Taxable Income × 15.3%
```

**Why 92.35%?** IRS allows deducting half of SE tax before calculating it.  
**Why 15.3%?** Social Security (12.4%) + Medicare (2.9%)

#### Step 5: Calculate Federal Income Tax
Uses **2024 tax brackets** based on filing status:

**Single**:
- 10% on income up to $11,600
- 12% on $11,601 - $47,150
- 22% on $47,151 - $100,525
- 24% on $100,526 - $191,950
- 32% on $191,951 - $243,725
- 35% on $243,726 - $609,350
- 37% over $609,350

**Married Filing Jointly**: (doubles most thresholds)

#### Step 6: Calculate State Tax
```
State Tax = Taxable Income × State Rate
```

#### Step 7: Calculate Quarterly Payment
```
Total Annual Tax = SE Tax + Federal Tax + State Tax
Quarterly Payment = Total Annual Tax ÷ 4
```

### Safe Harbor Protection

The service checks if your estimated payments meet "safe harbor" rules to avoid penalties:

**Safe Harbor Rules:**
- Pay **100%** of prior year's tax (if AGI < $150k)
- Pay **110%** of prior year's tax (if AGI >= $150k)
- OR pay **90%** of current year's actual tax

```csharp
if (q1Estimate.MeetsSafeHarbor)
{
    Console.WriteLine("✅ Safe from underpayment penalty");
}
else
{
    Console.WriteLine("⚠️ May need to increase payment to avoid penalty");
}
```

---

## 2. IFTA Fuel Tax Reporting

Generates quarterly IFTA (International Fuel Tax Agreement) reports tracking fuel purchased and miles driven by state.

### Method: GenerateIFTAReportAsync()

```csharp
var iftaQ1 = await TaxService.GenerateIFTAReportAsync(
    year: 2024,
    quarter: 1,
    truckId: "TRUCK-001" // or null for fleet
);

Console.WriteLine($"Total Miles: {iftaQ1.TotalMiles:N0}");
Console.WriteLine($"Total Gallons: {iftaQ1.TotalGallons:N0}");
Console.WriteLine($"Fleet MPG: {iftaQ1.FleetMPG:N2}");
Console.WriteLine($"");

foreach (var state in iftaQ1.StateData.OrderByDescending(s => s.MilesDriven))
{
    Console.WriteLine($"{state.State} ({state.StateFullName}):");
    Console.WriteLine($"  Miles: {state.MilesDriven:N0}");
    Console.WriteLine($"  Gallons Purchased: {state.GallonsPurchased:N1}");
    Console.WriteLine($"  Tax Rate: ${state.StateFuelTaxRate/100:N3}/gallon");
    Console.WriteLine($"  Tax Owed: {state.TaxOwed:C}");
    Console.WriteLine($"  Tax Paid: {state.TaxPaid:C}");
    
    if (state.NetTaxOwed > 0)
        Console.WriteLine($"  💰 OWE: {state.NetTaxOwed:C}");
    else if (state.NetTaxCredit > 0)
        Console.WriteLine($"  ✅ CREDIT: {state.NetTaxCredit:C}");
}

Console.WriteLine($"");
Console.WriteLine($"Net IFTA Liability: {iftaQ1.NetIFTATax:C}");
Console.WriteLine($"Filing Deadline: {iftaQ1.FilingDeadline:MM/dd/yyyy}");
```

### How IFTA Works

#### The Problem
You pay fuel tax when you buy fuel, but you may owe different rates depending on where you drove.

**Example:**
- Buy 100 gallons in Texas (20¢/gallon tax) = $20 paid
- Drive 500 miles in Pennsylvania (75.2¢/gallon tax)
- At 6 MPG, you "consumed" 83.3 gallons in PA
- PA tax owed: 83.3 gal × $0.752 = $62.64
- You paid: $20 in TX
- **You owe PA: $42.64**

#### How the Service Calculates It

**Step 1: Get Fleet MPG**
```
Total Miles ÷ Total Gallons = Fleet MPG
```

**Step 2: Calculate Taxable Gallons per State**
```
Taxable Gallons = Miles Driven in State ÷ Fleet MPG
```

**Step 3: Calculate Tax Owed per State**
```
Tax Owed = Taxable Gallons × State Tax Rate
```

**Step 4: Calculate Tax Paid per State**
- Gets fuel purchases from `FuelEntry` records
- Groups by state
- Estimates tax paid based on state rate

**Step 5: Net Tax**
```
Net = Tax Owed - Tax Paid

If Net > 0: You OWE that state
If Net < 0: You get CREDIT from that state
```

#### State Fuel Tax Rates (2024)

Highest:
- Pennsylvania: **75.2¢/gal**
- California: **53.5¢/gal**
- Illinois: **54.5¢/gal**
- Indiana: **52.0¢/gal**
- Washington: **49.4¢/gal**

Lowest:
- Alaska: **8.95¢/gal**
- Missouri: **17.0¢/gal**
- Mississippi: **18.4¢/gal**
- Texas/Oklahoma/Louisiana: **20.0¢/gal**

### IFTA Filing Deadlines

- **Q1**: April 30
- **Q2**: July 31
- **Q3**: October 31
- **Q4**: January 31 (next year)

---

## 3. Annual Tax Projection

Projects full-year tax liability based on year-to-date actuals.

### Method: CalculateAnnualProjectionAsync()

```csharp
var projection = await TaxService.CalculateAnnualProjectionAsync(
    year: 2024,
    filingStatus: TaxFilingStatus.MarriedFilingJointly,
    stateIncomeTaxRate: 4.5m
);

Console.WriteLine("=== 2024 Tax Projection ===");
Console.WriteLine($"");
Console.WriteLine("YTD Actuals ({projection.MonthsElapsed} months):");
Console.WriteLine($"  Income: {projection.YTDGrossIncome:C}");
Console.WriteLine($"  Deductions: {projection.YTDDeductions:C}");
Console.WriteLine($"  Net Profit: {projection.YTDNetProfit:C}");
Console.WriteLine($"");
Console.WriteLine("Projected Annual:");
Console.WriteLine($"  Income: {projection.ProjectedAnnualIncome:C}");
Console.WriteLine($"  Deductions: {projection.ProjectedAnnualDeductions:C}");
Console.WriteLine($"  Net Profit: {projection.ProjectedAnnualNetProfit:C}");
Console.WriteLine($"");
Console.WriteLine("Projected Tax:");
Console.WriteLine($"  Self-Employment: {projection.ProjectedSelfEmploymentTax:C}");
Console.WriteLine($"  Federal Income: {projection.ProjectedFederalIncomeTax:C}");
Console.WriteLine($"  State Income: {projection.ProjectedStateIncomeTax:C}");
Console.WriteLine($"  TOTAL: {projection.ProjectedTotalTax:C}");
Console.WriteLine($"");
Console.WriteLine($"Estimated Payments Made: {projection.EstimatedPaymentsMade:C}");
Console.WriteLine($"Remaining Liability: {projection.RemainingTaxLiability:C}");
Console.WriteLine($"");
Console.WriteLine($"Recommended Next Quarterly Payment: {projection.RecommendedNextQuarterlyPayment:C}");
```

### Tax Saving Tips

The service generates personalized recommendations:

```csharp
foreach (var tip in projection.TaxSavingTips)
{
    Console.WriteLine($"💡 {tip}");
}
```

**Example Tips:**
- "Your deductions seem low. Make sure you're tracking all expenses."
- "Don't forget per diem ($69/day) for days away from home."
- "Consider Section 179 deduction for equipment purchases."
- "Consider SEP-IRA contributions to reduce taxable income."

---

## 4. Deduction Tracking

### Method: CalculateDeductionsAsync()

```csharp
var deductions = await TaxService.CalculateDeductionsAsync(
    startDate: new DateTime(2024, 1, 1),
    endDate: new DateTime(2024, 12, 31),
    truckId: null
);

Console.WriteLine("=== Tax Deductions ===");

var breakdown = deductions.GetBreakdown();
foreach (var (category, (amount, percentage)) in breakdown.OrderByDescending(kvp => kvp.Value.Amount))
{
    Console.WriteLine($"{category}: {amount:C} ({percentage:N1}%)");
}

Console.WriteLine($"");
Console.WriteLine($"Total Deductions: {deductions.TotalDeductions:C}");
```

### IRS Schedule C Categories

The service automatically maps Triply expenses to IRS Schedule C line items:

| Triply Category | Schedule C Line | Typical % |
|----------------|-----------------|-----------|
| Fuel | Car & truck expenses | 30-40% |
| Maintenance | Repairs & maintenance | 10-15% |
| Insurance | Insurance | 5-8% |
| Permits & Licenses | Taxes & licenses | 1-2% |
| Tolls & Parking | Other expenses | 1-2% |
| Tires | Supplies | 2-3% |
| Truck Payment | Interest | 8-12% |
| Driver Pay | Wages | 20-25% |
| Per Diem | Travel | 5-10% |

### Per Diem Deduction

Special calculation for DOT drivers away from home overnight.

#### Method: CalculatePerDiemDeductionAsync()

```csharp
var perDiem = await TaxService.CalculatePerDiemDeductionAsync(
    startDate: new DateTime(2024, 1, 1),
    endDate: new DateTime(2024, 3, 31),
    truckId: "TRUCK-001"
);

Console.WriteLine($"Days Away: {perDiem.TotalDaysAway}");
Console.WriteLine($"Full Days: {perDiem.FullDays} × ${perDiem.DOTPerDiemRate} = {perDiem.FullDayAmount:C}");
Console.WriteLine($"Partial Days: {perDiem.PartialDays} × ${perDiem.DOTPerDiemRate * 0.75m} = {perDiem.PartialDayAmount:C}");
Console.WriteLine($"Total Per Diem Deduction: {perDiem.TotalPerDiemDeduction:C}");
```

#### Per Diem Rules

**2024 IRS Rate**: $69/day for transportation workers

**How It's Calculated:**
- **Full Days**: Entire 24-hour period away = $69
- **Partial Days**: Travel days (first & last) = $69 × 75% = $51.75

**Example:**
- Leave Monday 6am, return Friday 8pm
- Monday (partial): $51.75
- Tuesday-Thursday (full): $69 × 3 = $207
- Friday (partial): $51.75
- **Total**: $310.50

**The service automatically:**
1. Gets all loads in the period
2. Counts unique travel days
3. Marks first/last as partial
4. Calculates deduction

---

## 5. Tax Calendar & Deadlines

### Method: GetTaxCalendar()

```csharp
var calendar = TaxService.GetTaxCalendar(2024);

foreach (var deadline in calendar.Deadlines.OrderBy(d => d.DueDate))
{
    var urgency = deadline.IsOverdue ? "🔴 OVERDUE" :
                  deadline.DaysUntilDue <= 7 ? "⚠️ CRITICAL" :
                  deadline.DaysUntilDue <= 30 ? "🟡 IMPORTANT" :
                  "🟢 UPCOMING";
    
    Console.WriteLine($"{urgency} - {deadline.Name}");
    Console.WriteLine($"  Due: {deadline.DueDate:MM/dd/yyyy} ({deadline.DaysUntilDue} days)");
    Console.WriteLine($"  {deadline.Description}");
    Console.WriteLine();
}
```

### Key Tax Deadlines for Trucking

#### Quarterly Estimated Tax (Form 1040-ES)
- **Q1 (Jan-Mar)**: April 15
- **Q2 (Apr-May)**: June 17
- **Q3 (Jun-Aug)**: September 16
- **Q4 (Sep-Dec)**: January 15 (next year)

#### IFTA Returns
- **Q1**: April 30
- **Q2**: July 31
- **Q3**: October 31
- **Q4**: January 31

#### Annual Returns
- **Form 1040 + Schedule C**: April 15
- **Form 2290 (Heavy Vehicle Use Tax)**: August 31
- **UCR Registration**: December 31

### Method: GetUpcomingDeadlines()

```csharp
var upcoming = TaxService.GetUpcomingDeadlines(daysAhead: 60);

Console.WriteLine("=== Next 60 Days ===");
foreach (var deadline in upcoming)
{
    Console.WriteLine($"📅 {deadline.DueDate:MM/dd/yyyy} - {deadline.Name}");
    
    if (deadline.DaysUntilDue <= 7)
        Console.WriteLine($"   ⚠️ DUE IN {deadline.DaysUntilDue} DAYS!");
}
```

---

## Real-World Examples

### Example 1: Monthly Tax Planning Meeting

```csharp
public class MonthlyTaxReview
{
    private readonly TaxEstimatorService _taxService;

    public async Task RunMonthlyReviewAsync()
    {
        var year = DateTime.UtcNow.Year;
        var currentQuarter = (DateTime.UtcNow.Month - 1) / 3 + 1;

        // Check current quarter status
        var qEstimate = await _taxService.CalculateQuarterlyTaxAsync(
            year, currentQuarter, TaxFilingStatus.Single, 5.0m);

        Console.WriteLine("=== Quarterly Tax Status ===");
        Console.WriteLine($"Q{currentQuarter} {year}");
        Console.WriteLine($"Net Profit: {qEstimate.NetProfit:C}");
        Console.WriteLine($"Estimated Payment Due: {qEstimate.QuarterlyEstimatedPayment:C}");
        Console.WriteLine($"Effective Tax Rate: {qEstimate.EffectiveTaxRate:N1}%");

        // Check upcoming deadlines
        var deadlines = _taxService.GetUpcomingDeadlines(30);
        Console.WriteLine($"");
        Console.WriteLine($"=== Next 30 Days ===");
        foreach (var deadline in deadlines)
        {
            Console.WriteLine($"{deadline.DueDate:MM/dd/yyyy} - {deadline.Name}");
        }

        // Annual projection
        var projection = await _taxService.CalculateAnnualProjectionAsync(
            year, TaxFilingStatus.Single, 5.0m);

        Console.WriteLine($"");
        Console.WriteLine($"=== Year-End Projection ===");
        Console.WriteLine($"Projected Net Profit: {projection.ProjectedAnnualNetProfit:C}");
        Console.WriteLine($"Projected Total Tax: {projection.ProjectedTotalTax:C}");
        Console.WriteLine($"Payments Made: {projection.EstimatedPaymentsMade:C}");
        Console.WriteLine($"Remaining: {projection.RemainingTaxLiability:C}");
    }
}
```

### Example 2: Year-End Tax Strategy

```csharp
public async Task YearEndTaxStrategyAsync()
{
    var projection = await _taxService.CalculateAnnualProjectionAsync(
        DateTime.UtcNow.Year,
        TaxFilingStatus.MarriedFilingJointly,
        4.5m
    );

    Console.WriteLine("=== Year-End Tax Strategy ===");

    // Check if profitable
    if (projection.ProjectedAnnualNetProfit <= 0)
    {
        Console.WriteLine("⚠️ Projected loss for the year. Consider:");
        Console.WriteLine("   - Deferring income to next year");
        Console.WriteLine("   - Accelerating expenses if expecting better year");
        return;
    }

    // Tax bracket analysis
    var effectiveRate = projection.ProjectedTotalTax / projection.ProjectedAnnualIncome * 100;
    Console.WriteLine($"Projected Effective Tax Rate: {effectiveRate:N1}%");

    // Deduction optimization
    var deductions = await _taxService.CalculateDeductionsAsync(
        new DateTime(projection.Year, 1, 1),
        new DateTime(projection.Year, 12, 31)
    );

    Console.WriteLine($"");
    Console.WriteLine("Top Deduction Opportunities:");

    // Check for missed per diem
    if (deductions.PerDiemDeduction < projection.ProjectedAnnualIncome * 0.05m)
    {
        Console.WriteLine($"💡 Per Diem only {deductions.PerDiemDeduction:C}");
        Console.WriteLine($"   Make sure tracking all days away from home!");
    }

    // Section 179 opportunity
    if (projection.ProjectedAnnualNetProfit > 100000)
    {
        Console.WriteLine($"💡 Consider Section 179 equipment purchase");
        Console.WriteLine($"   Can deduct up to $1.22M in 2024");
        Console.WriteLine($"   Example: New trailer = instant deduction");
    }

    // SEP-IRA contribution
    var maxSEP = projection.ProjectedAnnualNetProfit * 0.20m; // 20% of net
    Console.WriteLine($"");
    Console.WriteLine($"💡 SEP-IRA Opportunity:");
    Console.WriteLine($"   Max contribution: {maxSEP:C}");
    Console.WriteLine($"   Tax savings: {maxSEP * (effectiveRate / 100):C}");
}
```

### Example 3: IFTA Compliance Dashboard

```csharp
public async Task GenerateIFTADashboardAsync(int year, int quarter)
{
    var ifta = await _taxService.GenerateIFTAReportAsync(year, quarter);

    Console.WriteLine($"=== IFTA Q{quarter} {year} ===");
    Console.WriteLine($"Filing Deadline: {ifta.FilingDeadline:MM/dd/yyyy}");
    Console.WriteLine($"Days Until Due: {(ifta.FilingDeadline - DateTime.UtcNow).Days}");
    Console.WriteLine($"");

    // Fleet summary
    Console.WriteLine($"Total Miles: {ifta.TotalMiles:N0}");
    Console.WriteLine($"Total Gallons: {ifta.TotalGallons:N1}");
    Console.WriteLine($"Fleet MPG: {ifta.FleetMPG:N2}");
    Console.WriteLine($"");

    // States summary
    var statesWithTaxDue = ifta.StateData.Where(s => s.NetTaxOwed > 0).ToList();
    var statesWithCredit = ifta.StateData.Where(s => s.NetTaxCredit > 0).ToList();

    Console.WriteLine($"States with Tax Owed ({statesWithTaxDue.Count}):");
    foreach (var state in statesWithTaxDue.OrderByDescending(s => s.NetTaxOwed))
    {
        Console.WriteLine($"  {state.State}: {state.NetTaxOwed:C}");
    }

    Console.WriteLine($"");
    Console.WriteLine($"States with Tax Credit ({statesWithCredit.Count}):");
    foreach (var state in statesWithCredit.OrderByDescending(s => s.NetTaxCredit))
    {
        Console.WriteLine($"  {state.State}: {state.NetTaxCredit:C}");
    }

    Console.WriteLine($"");
    Console.WriteLine($"💰 Net IFTA: {ifta.NetIFTATax:C}");

    if (ifta.NetIFTATax > 0)
        Console.WriteLine($"   Amount to remit with return");
    else
        Console.WriteLine($"   Credit/refund expected");
}
```

### Example 4: Safe Harbor Check

```csharp
public async Task CheckSafeHarborStatusAsync()
{
    var currentYear = DateTime.UtcNow.Year;
    var currentQuarter = (DateTime.UtcNow.Month - 1) / 3 + 1;

    var estimate = await _taxService.CalculateQuarterlyTaxAsync(
        currentYear, currentQuarter,
        TaxFilingStatus.Single, 5.0m
    );

    Console.WriteLine("=== Safe Harbor Status ===");

    if (estimate.MeetsSafeHarbor)
    {
        Console.WriteLine("✅ SAFE HARBOR MET");
        Console.WriteLine("You are protected from underpayment penalties.");
    }
    else
    {
        Console.WriteLine("⚠️ SAFE HARBOR NOT MET");
        Console.WriteLine("You may be subject to underpayment penalties.");
        Console.WriteLine($"");
        Console.WriteLine($"Current quarterly payment: {estimate.QuarterlyEstimatedPayment:C}");
        Console.WriteLine($"Recommended: Increase payment or ensure 90% of actual tax is paid");
    }

    // Show prior year comparison
    var projection = await _taxService.CalculateAnnualProjectionAsync(
        currentYear, TaxFilingStatus.Single, 5.0m);

    Console.WriteLine($"");
    Console.WriteLine($"Current year projected tax: {projection.ProjectedTotalTax:C}");
    Console.WriteLine($"Estimated payments made: {projection.EstimatedPaymentsMade:C}");
    Console.WriteLine($"Percentage paid: {(projection.EstimatedPaymentsMade / projection.ProjectedTotalTax * 100):N1}%");

    if (projection.EstimatedPaymentsMade / projection.ProjectedTotalTax < 0.90m)
    {
        var shortfall = projection.ProjectedTotalTax * 0.90m - projection.EstimatedPaymentsMade;
        Console.WriteLine($"⚠️ Pay additional {shortfall:C} to meet 90% rule");
    }
}
```

---

## Tax Planning Strategies

### Timing Income & Expenses

#### Defer Income (if in high tax year)
```csharp
// Wait to invoice until January if profitable year
```

#### Accelerate Expenses (if expecting lower income next year)
```csharp
// Buy equipment/supplies in December
// Pay next year's insurance premium
// Perform maintenance before year-end
```

### Maximizing Deductions

#### Per Diem
- **Always** claim per diem for days away from home
- Track using load pickup/delivery dates
- 2024 rate: $69/day

#### Section 179 Expensing
- Deduct full cost of equipment in year purchased
- 2024 limit: $1.22 million
- Examples: trucks, trailers, computers, shop equipment

#### Home Office Deduction
- If you have dedicated office space at home
- Simplified method: $5 per sq ft (max 300 sq ft = $1,500)
- Regular method: Percentage of home expenses

#### Depreciation
- MACRS depreciation for trucks (5-year)
- Bonus depreciation: 60% in 2024 (phasing out)

### Retirement Contributions

#### SEP-IRA
- Contribute up to 25% of net self-employment earnings
- 2024 max: $69,000
- **Tax-deductible** - reduces current year tax
- Example: $100k net profit = $20k SEP contribution = ~$7k tax savings

#### Solo 401(k)
- Employee contribution: up to $23,000
- Employer contribution: up to 25% of net
- 2024 total max: $69,000
- Plus $7,500 catch-up if age 50+

---

## Compliance & Penalties

### Underpayment Penalty

**Penalty Rate**: Approximately 8% annually (varies quarterly)

**How to Avoid:**
1. Pay 100% of prior year tax (110% if AGI > $150k)
2. Pay 90% of current year tax
3. Owe less than $1,000 at year-end

**Example:**
- 2023 tax: $40,000
- Must pay $44,000 in 2024 estimates (110%)
- $11,000 per quarter

### IFTA Penalties

- **Late Filing**: 10% of tax due or $50 minimum
- **Late Payment**: 10% of tax due plus interest
- **No Report**: License suspension

### Form 2290 (HVUT)

- **Required**: Trucks over 55,000 lbs GVW
- **Due**: August 31
- **Penalty**: $22/month per vehicle
- **Proof needed**: For truck registration renewal

---

## Best Practices

### Monthly
- [ ] Review income & expenses
- [ ] Ensure all expenses categorized correctly
- [ ] Check per diem calculation

### Quarterly
- [ ] Calculate estimated tax payment
- [ ] Submit payment by deadline (or early)
- [ ] File IFTA return
- [ ] Review year-to-date vs. projection

### Annually
- [ ] Final tax return (Form 1040 + Schedule C)
- [ ] Form 2290 (HVUT) by Aug 31
- [ ] Plan equipment purchases for deductions
- [ ] Max out retirement contributions
- [ ] Review next year's estimated payment schedule

---

## API Reference

### TaxEstimatorService Methods

```csharp
// Quarterly Taxes
Task<QuarterlyTaxEstimate> CalculateQuarterlyTaxAsync(int year, int quarter, 
    TaxFilingStatus filingStatus, decimal stateRate = 0, string? truckId = null)

// Annual Projection
Task<AnnualTaxProjection> CalculateAnnualProjectionAsync(int year, 
    TaxFilingStatus filingStatus, decimal stateRate = 0, string? truckId = null)

// IFTA Reporting
Task<IFTAQuarterlyReport> GenerateIFTAReportAsync(int year, int quarter, string? truckId = null)

// Deductions
Task<TaxDeductionSummary> CalculateDeductionsAsync(DateTime start, DateTime end, string? truckId = null)

Task<PerDiemCalculation> CalculatePerDiemDeductionAsync(DateTime start, DateTime end, string? truckId = null)

// Tax Calendar
TaxCalendar GetTaxCalendar(int year)

List<TaxDeadline> GetUpcomingDeadlines(int daysAhead = 60)
```

---

## Integration Examples

### With Cost Per Mile Service

```csharp
// Analyze tax efficiency of operations
var cpm = await _cpmService.CalculateCPMAsync(...);
var taxes = await _taxService.CalculateQuarterlyTaxAsync(...);

var afterTaxProfitPerMile = cpm.ProfitPerMile - (taxes.TotalTaxLiability / cpm.TotalMiles / 4);
Console.WriteLine($"After-Tax Profit/Mile: {afterTaxProfitPerMile:C}");
```

### With Load Management

```csharp
// Estimate tax on proposed load
var loadRevenue = 2500m;
var estimatedProfit = loadRevenue * 0.20m; // 20% margin
var estimatedTax = estimatedProfit * 0.30m; // ~30% effective rate
var afterTaxProfit = estimatedProfit - estimatedTax;
```

---

## Summary

The TaxEstimatorService provides:
- ✅ Quarterly tax calculations (SE, Federal, State)
- ✅ IFTA fuel tax reporting by state
- ✅ Annual tax projections
- ✅ Comprehensive deduction tracking
- ✅ Per diem calculations
- ✅ Tax calendar & deadline reminders
- ✅ Safe harbor compliance checking
- ✅ Tax planning recommendations

**Critical for trucking businesses to:**
- Avoid quarterly payment penalties
- Maximize deductions
- Stay IFTA compliant
- Plan cash flow for tax obligations

---

## Disclaimer

This service provides estimates for planning purposes. Always consult with a qualified tax professional for:
- Final tax return preparation
- Complex situations
- State-specific rules
- Audit representation

Tax laws change annually - ensure rates and brackets are updated each year.
