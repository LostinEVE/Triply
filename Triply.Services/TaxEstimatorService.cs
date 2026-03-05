using Microsoft.EntityFrameworkCore;
using Triply.Core.Enums;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public class TaxEstimatorService
{
    private readonly IUnitOfWork _unitOfWork;

    public TaxEstimatorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    #region Quarterly Tax Estimation

    /// <summary>
    /// Calculates quarterly estimated tax payment
    /// </summary>
    public async Task<QuarterlyTaxEstimate> CalculateQuarterlyTaxAsync(
        int year,
        int quarter,
        TaxFilingStatus filingStatus,
        decimal stateIncomeTaxRate = 0,
        string? truckId = null)
    {
        var (startDate, endDate) = GetQuarterDates(year, quarter);
        
        var estimate = new QuarterlyTaxEstimate
        {
            Year = year,
            Quarter = quarter,
            StartDate = startDate,
            EndDate = endDate
        };

        // Calculate gross income from paid invoices
        var invoicesQuery = _unitOfWork.Invoices.GetQueryable()
            .Where(i => i.Status == InvoiceStatus.Paid &&
                       i.PaidDate >= startDate &&
                       i.PaidDate <= endDate);

        estimate.GrossIncome = await invoicesQuery.SumAsync(i => i.TotalAmount);

        // Calculate deductions
        var deductions = await CalculateDeductionsAsync(startDate, endDate, truckId);
        estimate.TotalDeductions = deductions.TotalDeductions;
        estimate.DeductionsByCategory = deductions.GetBreakdown()
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Amount);

        // Set standard deduction based on filing status (2024 rates)
        estimate.StandardDeduction = GetStandardDeduction(filingStatus);

        // Calculate federal income tax
        estimate.FederalIncomeTax = CalculateFederalIncomeTax(
            estimate.TaxableIncome, 
            filingStatus);

        // Calculate state income tax
        estimate.StateIncomeTax = estimate.TaxableIncome * (stateIncomeTaxRate / 100);

        // Check safe harbor
        estimate.MeetsSafeHarbor = await CheckSafeHarborAsync(year - 1, estimate.QuarterlyEstimatedPayment);
        estimate.SafeHarborMessage = estimate.MeetsSafeHarbor
            ? "✅ Safe from underpayment penalty"
            : "⚠️ May be subject to underpayment penalty";

        return estimate;
    }

    /// <summary>
    /// Projects annual tax liability based on YTD data
    /// </summary>
    public async Task<AnnualTaxProjection> CalculateAnnualProjectionAsync(
        int year,
        TaxFilingStatus filingStatus,
        decimal stateIncomeTaxRate = 0,
        string? truckId = null)
    {
        var projection = new AnnualTaxProjection { Year = year };
        
        var yearStart = new DateTime(year, 1, 1);
        var today = DateTime.UtcNow;
        var yearEnd = new DateTime(year, 12, 31);

        // YTD Actuals
        var ytdInvoices = await _unitOfWork.Invoices.GetQueryable()
            .Where(i => i.Status == InvoiceStatus.Paid &&
                       i.PaidDate >= yearStart &&
                       i.PaidDate <= today)
            .SumAsync(i => i.TotalAmount);

        projection.YTDGrossIncome = ytdInvoices;

        var ytdDeductions = await CalculateDeductionsAsync(yearStart, today, truckId);
        projection.YTDDeductions = ytdDeductions.TotalDeductions;
        projection.MonthsElapsed = today.Month;

        // Project Annual
        var monthsRemaining = 12 - projection.MonthsElapsed;
        var avgMonthlyIncome = projection.MonthsElapsed > 0 
            ? projection.YTDGrossIncome / projection.MonthsElapsed 
            : 0;
        var avgMonthlyDeductions = projection.MonthsElapsed > 0 
            ? projection.YTDDeductions / projection.MonthsElapsed 
            : 0;

        projection.ProjectedAnnualIncome = projection.YTDGrossIncome + (avgMonthlyIncome * monthsRemaining);
        projection.ProjectedAnnualDeductions = projection.YTDDeductions + (avgMonthlyDeductions * monthsRemaining);

        // Calculate projected taxes
        var projectedNetProfit = projection.ProjectedAnnualNetProfit;
        var projectedSETaxable = projectedNetProfit * 0.9235m;
        projection.ProjectedSelfEmploymentTax = projectedSETaxable * 0.153m;

        var seDeduction = projection.ProjectedSelfEmploymentTax * 0.5m;
        var agi = projectedNetProfit - seDeduction;
        var standardDeduction = GetStandardDeduction(filingStatus);
        var taxableIncome = Math.Max(0, agi - standardDeduction);

        projection.ProjectedFederalIncomeTax = CalculateFederalIncomeTax(taxableIncome, filingStatus);
        projection.ProjectedStateIncomeTax = taxableIncome * (stateIncomeTaxRate / 100);

        // Get quarterly estimates
        for (int q = 1; q <= 4; q++)
        {
            var qEstimate = await CalculateQuarterlyTaxAsync(year, q, filingStatus, stateIncomeTaxRate, truckId);
            projection.QuarterlyEstimates.Add(qEstimate);
        }

        // Calculate payments made
        var paymentsQuery = _unitOfWork.TaxPayments.GetQueryable()
            .Where(tp => tp.TaxYear == year && tp.PaidDate.HasValue && tp.PaidDate <= today);

        projection.EstimatedPaymentsMade = await paymentsQuery.SumAsync(tp => tp.AmountPaid);

        // Recommendation
        var quarterlyTarget = projection.ProjectedTotalTax / 4;
        projection.RecommendedNextQuarterlyPayment = quarterlyTarget;

        // Tax saving tips
        projection.TaxSavingTips = GenerateTaxSavingTips(projection);

        return projection;
    }

    #endregion

    #region IFTA Calculations

    /// <summary>
    /// Generates IFTA quarterly report
    /// </summary>
    public async Task<IFTAQuarterlyReport> GenerateIFTAReportAsync(
        int year,
        int quarter,
        string? truckId = null)
    {
        var (startDate, endDate) = GetQuarterDates(year, quarter);
        
        var report = new IFTAQuarterlyReport
        {
            Year = year,
            Quarter = quarter,
            StartDate = startDate,
            EndDate = endDate,
            TruckId = truckId,
            FilingDeadline = GetIFTAFilingDeadline(year, quarter)
        };

        // Get fleet MPG from fuel entries
        var fuelQuery = _unitOfWork.FuelEntries.GetQueryable()
            .Where(f => f.FuelDate >= startDate && f.FuelDate <= endDate);

        if (!string.IsNullOrEmpty(truckId))
            fuelQuery = fuelQuery.Where(f => f.TruckId == truckId);

        var fuelEntries = await fuelQuery.ToListAsync();
        var totalGallons = fuelEntries.Sum(f => f.Gallons);

        // Get miles from loads
        var loadsQuery = _unitOfWork.Loads.GetQueryable()
            .Where(l => l.DeliveryDate >= startDate && l.DeliveryDate <= endDate);

        if (!string.IsNullOrEmpty(truckId))
            loadsQuery = loadsQuery.Where(l => l.TruckId == truckId);

        var loads = await loadsQuery.ToListAsync();
        var totalMiles = loads.Sum(l => l.Miles);

        var fleetMPG = totalGallons > 0 ? (double)totalMiles / (double)totalGallons : 0;

        // Group fuel purchases by state
        var fuelByState = fuelEntries
            .Where(f => !string.IsNullOrEmpty(f.State))
            .GroupBy(f => f.State!)
            .ToDictionary(g => g.Key, g => new
            {
                Gallons = g.Sum(f => f.Gallons),
                TotalCost = g.Sum(f => f.TotalCost)
            });

        // Group miles by state (from load origin/destination)
        var milesByState = new Dictionary<string, int>();
        foreach (var load in loads)
        {
            // Simplified: split miles between pickup and delivery states
            if (!string.IsNullOrEmpty(load.PickupState))
            {
                var halfMiles = load.Miles / 2;
                if (milesByState.ContainsKey(load.PickupState))
                    milesByState[load.PickupState] += halfMiles;
                else
                    milesByState[load.PickupState] = halfMiles;
            }

            if (!string.IsNullOrEmpty(load.DeliveryState))
            {
                var halfMiles = load.Miles / 2;
                if (milesByState.ContainsKey(load.DeliveryState))
                    milesByState[load.DeliveryState] += halfMiles;
                else
                    milesByState[load.DeliveryState] = halfMiles;
            }
        }

        // Build state data
        var allStates = fuelByState.Keys.Union(milesByState.Keys).Distinct();
        var taxRates = IFTATaxRates.GetStateFuelTaxRates();
        var stateNames = IFTATaxRates.GetStateFullNames();

        foreach (var state in allStates)
        {
            var stateData = new IFTAStateData
            {
                State = state,
                StateFullName = stateNames.GetValueOrDefault(state, state),
                MilesDriven = milesByState.GetValueOrDefault(state, 0),
                GallonsPurchased = fuelByState.ContainsKey(state) ? fuelByState[state].Gallons : 0,
                StateFuelTaxRate = taxRates.GetValueOrDefault(state, 0),
                FleetMPG = fleetMPG
            };

            // Calculate tax paid on purchases
            if (fuelByState.ContainsKey(state))
            {
                var avgPricePerGallon = fuelByState[state].Gallons > 0
                    ? fuelByState[state].TotalCost / fuelByState[state].Gallons
                    : 0;
                
                // Estimate tax portion (tax rate / total pump price)
                var taxRate = stateData.StateFuelTaxRate / 100;
                stateData.TaxPaidOnPurchase = stateData.GallonsPurchased * taxRate;
            }

            report.StateData.Add(stateData);
        }

        return report;
    }

    #endregion

    #region Deduction Tracking

    /// <summary>
    /// Calculates all tax deductions for a period
    /// </summary>
    public async Task<TaxDeductionSummary> CalculateDeductionsAsync(
        DateTime startDate,
        DateTime endDate,
        string? truckId = null)
    {
        var summary = new TaxDeductionSummary
        {
            Year = startDate.Year,
            Quarter = GetQuarter(startDate)
        };

        // Fuel expenses
        var fuelQuery = _unitOfWork.FuelEntries.GetQueryable()
            .Where(f => f.FuelDate >= startDate && f.FuelDate <= endDate);

        if (!string.IsNullOrEmpty(truckId))
            fuelQuery = fuelQuery.Where(f => f.TruckId == truckId);

        summary.FuelExpenses = await fuelQuery.SumAsync(f => f.TotalCost);

        // Maintenance
        var maintenanceQuery = _unitOfWork.MaintenanceRecords.GetQueryable()
            .Where(m => m.MaintenanceDate >= startDate && m.MaintenanceDate <= endDate);

        if (!string.IsNullOrEmpty(truckId))
            maintenanceQuery = maintenanceQuery.Where(m => m.TruckId == truckId);

        summary.MaintenanceAndRepairs = await maintenanceQuery.SumAsync(m => m.TotalCost);

        // Categorized expenses
        var expensesQuery = _unitOfWork.Expenses.GetQueryable()
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate);

        if (!string.IsNullOrEmpty(truckId))
            expensesQuery = expensesQuery.Where(e => e.TruckId == truckId);

        var expenses = await expensesQuery.ToListAsync();

        summary.Insurance = expenses
            .Where(e => e.Category == ExpenseCategory.Insurance)
            .Sum(e => e.Amount);

        summary.PermitsAndLicenses = expenses
            .Where(e => e.Category == ExpenseCategory.Permits)
            .Sum(e => e.Amount);

        summary.TollsAndParking = expenses
            .Where(e => e.Category == ExpenseCategory.Tolls || e.Category == ExpenseCategory.Parking)
            .Sum(e => e.Amount);

        summary.Tires = expenses
            .Where(e => e.Category == ExpenseCategory.Tires)
            .Sum(e => e.Amount);

        summary.OfficeExpenses = expenses
            .Where(e => e.Category == ExpenseCategory.OfficeExpense)
            .Sum(e => e.Amount);

        summary.Wages = expenses
            .Where(e => e.Category == ExpenseCategory.DriverPay)
            .Sum(e => e.Amount);

        summary.OtherExpenses = expenses
            .Where(e => e.Category == ExpenseCategory.Other ||
                       e.Category == ExpenseCategory.Lumper ||
                       e.Category == ExpenseCategory.Scales ||
                       e.Category == ExpenseCategory.Trailer)
            .Sum(e => e.Amount);

        // Per diem calculation
        var perDiem = await CalculatePerDiemDeductionAsync(startDate, endDate, truckId);
        summary.PerDiemDeduction = perDiem.TotalPerDiemDeduction;

        return summary;
    }

    /// <summary>
    /// Calculates per diem deduction for DOT drivers
    /// </summary>
    public async Task<PerDiemCalculation> CalculatePerDiemDeductionAsync(
        DateTime startDate,
        DateTime endDate,
        string? truckId = null)
    {
        var perDiem = new PerDiemCalculation
        {
            StartDate = startDate,
            EndDate = endDate,
            DOTPerDiemRate = 69.00m // 2024 IRS rate
        };

        // Get loads to determine days away from home
        var loadsQuery = _unitOfWork.Loads.GetQueryable()
            .Where(l => l.PickupDate.HasValue && 
                       l.PickupDate >= startDate && 
                       l.DeliveryDate <= endDate);

        if (!string.IsNullOrEmpty(truckId))
            loadsQuery = loadsQuery.Where(l => l.TruckId == truckId);

        var loads = await loadsQuery.ToListAsync();

        var travelDays = new HashSet<DateTime>();

        foreach (var load in loads)
        {
            var currentDate = load.PickupDate.Value.Date;
            var endLoadDate = load.DeliveryDate.HasValue ? load.DeliveryDate.Value.Date : load.PickupDate.Value.Date;

            while (currentDate <= endLoadDate)
            {
                travelDays.Add(currentDate);
                currentDate = currentDate.AddDays(1);
            }
        }

        perDiem.TotalDaysAway = travelDays.Count;
        perDiem.TravelDays = travelDays.Select(d => d.ToString("yyyy-MM-dd")).ToList();

        // First and last day are partial days (75%)
        if (perDiem.TotalDaysAway > 0)
        {
            perDiem.PartialDays = 2;
            perDiem.FullDays = Math.Max(0, perDiem.TotalDaysAway - 2);
        }

        return perDiem;
    }

    #endregion

    #region Tax Calendar

    /// <summary>
    /// Gets tax calendar for a specific year
    /// </summary>
    public TaxCalendar GetTaxCalendar(int year)
    {
        return year switch
        {
            2024 => TaxCalendarFactory.Create2024Calendar(),
            2025 => TaxCalendarFactory.Create2025Calendar(),
            _ => CreateGenericCalendar(year)
        };
    }

    /// <summary>
    /// Gets upcoming tax deadlines
    /// </summary>
    public List<TaxDeadline> GetUpcomingDeadlines(int daysAhead = 60)
    {
        var currentYear = DateTime.UtcNow.Year;
        var calendar = GetTaxCalendar(currentYear);
        
        var upcoming = calendar.GetUpcomingDeadlines(daysAhead);
        
        // Also check next year for deadlines that might be soon
        if (DateTime.UtcNow.Month >= 11)
        {
            var nextYearCalendar = GetTaxCalendar(currentYear + 1);
            upcoming.AddRange(nextYearCalendar.GetUpcomingDeadlines(daysAhead));
        }

        return upcoming.OrderBy(d => d.DueDate).ToList();
    }

    #endregion

    #region Helper Methods

    private (DateTime StartDate, DateTime EndDate) GetQuarterDates(int year, int quarter)
    {
        return quarter switch
        {
            1 => (new DateTime(year, 1, 1), new DateTime(year, 3, 31)),
            2 => (new DateTime(year, 4, 1), new DateTime(year, 6, 30)),
            3 => (new DateTime(year, 7, 1), new DateTime(year, 9, 30)),
            4 => (new DateTime(year, 10, 1), new DateTime(year, 12, 31)),
            _ => throw new ArgumentException("Quarter must be 1-4", nameof(quarter))
        };
    }

    private int GetQuarter(DateTime date)
    {
        return (date.Month - 1) / 3 + 1;
    }

    private DateTime GetIFTAFilingDeadline(int year, int quarter)
    {
        return quarter switch
        {
            1 => new DateTime(year, 4, 30),
            2 => new DateTime(year, 7, 31),
            3 => new DateTime(year, 10, 31),
            4 => new DateTime(year + 1, 1, 31),
            _ => DateTime.MinValue
        };
    }

    private decimal GetStandardDeduction(TaxFilingStatus filingStatus)
    {
        // 2024 standard deductions
        return filingStatus switch
        {
            TaxFilingStatus.Single => 14600m,
            TaxFilingStatus.MarriedFilingJointly => 29200m,
            TaxFilingStatus.MarriedFilingSeparately => 14600m,
            TaxFilingStatus.HeadOfHousehold => 21900m,
            _ => 14600m
        };
    }

    private decimal CalculateFederalIncomeTax(decimal taxableIncome, TaxFilingStatus filingStatus)
    {
        // 2024 federal tax brackets
        if (filingStatus == TaxFilingStatus.Single)
        {
            if (taxableIncome <= 11600) return taxableIncome * 0.10m;
            if (taxableIncome <= 47150) return 1160 + (taxableIncome - 11600) * 0.12m;
            if (taxableIncome <= 100525) return 5426 + (taxableIncome - 47150) * 0.22m;
            if (taxableIncome <= 191950) return 17168.50m + (taxableIncome - 100525) * 0.24m;
            if (taxableIncome <= 243725) return 39110.50m + (taxableIncome - 191950) * 0.32m;
            if (taxableIncome <= 609350) return 55678.50m + (taxableIncome - 243725) * 0.35m;
            return 183647.25m + (taxableIncome - 609350) * 0.37m;
        }
        else if (filingStatus == TaxFilingStatus.MarriedFilingJointly)
        {
            if (taxableIncome <= 23200) return taxableIncome * 0.10m;
            if (taxableIncome <= 94300) return 2320 + (taxableIncome - 23200) * 0.12m;
            if (taxableIncome <= 201050) return 10852 + (taxableIncome - 94300) * 0.22m;
            if (taxableIncome <= 383900) return 34337 + (taxableIncome - 201050) * 0.24m;
            if (taxableIncome <= 487450) return 78221 + (taxableIncome - 383900) * 0.32m;
            if (taxableIncome <= 731200) return 111357 + (taxableIncome - 487450) * 0.35m;
            return 196669.50m + (taxableIncome - 731200) * 0.37m;
        }
        else if (filingStatus == TaxFilingStatus.HeadOfHousehold)
        {
            if (taxableIncome <= 16550) return taxableIncome * 0.10m;
            if (taxableIncome <= 63100) return 1655 + (taxableIncome - 16550) * 0.12m;
            if (taxableIncome <= 100500) return 7241 + (taxableIncome - 63100) * 0.22m;
            if (taxableIncome <= 191950) return 15469 + (taxableIncome - 100500) * 0.24m;
            if (taxableIncome <= 243700) return 37417 + (taxableIncome - 191950) * 0.32m;
            if (taxableIncome <= 609350) return 53977 + (taxableIncome - 243700) * 0.35m;
            return 181954.50m + (taxableIncome - 609350) * 0.37m;
        }
        
        // MarriedFilingSeparately uses Single brackets
        return CalculateFederalIncomeTax(taxableIncome, TaxFilingStatus.Single);
    }

    private async Task<bool> CheckSafeHarborAsync(int priorYear, decimal estimatedQuarterlyPayment)
    {
        // Safe harbor: pay 100% of prior year tax (110% if AGI > $150k)
        var priorYearQuery = _unitOfWork.TaxPayments.GetQueryable()
            .Where(tp => tp.TaxYear == priorYear);

        var priorYearTax = await priorYearQuery.SumAsync(tp => tp.AmountPaid);
        
        var safeHarborAmount = priorYearTax * 1.10m; // Conservative 110%
        var annualEstimate = estimatedQuarterlyPayment * 4;

        return annualEstimate >= safeHarborAmount;
    }

    private List<string> GenerateTaxSavingTips(AnnualTaxProjection projection)
    {
        var tips = new List<string>();

        // Check deduction utilization
        if (projection.ProjectedAnnualDeductions < projection.ProjectedAnnualIncome * 0.3m)
        {
            tips.Add("💡 Your deductions seem low. Make sure you're tracking all expenses like per diem, tolls, and maintenance.");
        }

        // Per diem reminder
        tips.Add("📅 Don't forget to claim per diem ($69/day) for days away from home terminal.");

        // Section 179 reminder
        if (projection.ProjectedAnnualIncome > 50000)
        {
            tips.Add("🚛 Consider Section 179 deduction for new truck/equipment purchases (up to $1.22M in 2024).");
        }

        // Quarterly payment reminder
        if (projection.RemainingTaxLiability > 1000)
        {
            tips.Add($"⚠️ You have ${projection.RemainingTaxLiability:N0} remaining tax liability. Consider increasing quarterly payments to avoid penalties.");
        }

        // Retirement contribution tip
        if (projection.ProjectedAnnualNetProfit > 75000)
        {
            tips.Add("💰 Consider SEP-IRA contributions (up to 25% of net earnings) to reduce taxable income.");
        }

        return tips;
    }

    private TaxCalendar CreateGenericCalendar(int year)
    {
        var calendar = new TaxCalendar { Year = year };
        
        // Generic quarterly deadlines
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = $"Q1 {year} Estimated Tax",
            DueDate = new DateTime(year, 4, 15),
            Type = TaxDeadlineType.QuarterlyEstimate
        });
        
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = $"Q1 {year} IFTA Return",
            DueDate = new DateTime(year, 4, 30),
            Type = TaxDeadlineType.IFTA
        });
        
        // Add others...
        return calendar;
    }

    #endregion
}
