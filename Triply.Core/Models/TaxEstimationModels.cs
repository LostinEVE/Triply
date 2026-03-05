namespace Triply.Core.Models;

public class QuarterlyTaxEstimate
{
    // Period Information
    public int Year { get; set; }
    public int Quarter { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string QuarterLabel => $"Q{Quarter} {Year}";
    
    // Income
    public decimal GrossIncome { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetProfit => GrossIncome - TotalDeductions;
    
    // Self-Employment Tax
    public decimal SelfEmploymentTaxableIncome => NetProfit * 0.9235m; // 92.35% of net profit
    public decimal SelfEmploymentTax => SelfEmploymentTaxableIncome * 0.153m; // 15.3%
    public decimal SelfEmploymentTaxDeduction => SelfEmploymentTax * 0.5m; // 50% deductible
    
    // Taxable Income for Federal
    public decimal AdjustedGrossIncome => NetProfit - SelfEmploymentTaxDeduction;
    public decimal StandardDeduction { get; set; } // Based on filing status
    public decimal TaxableIncome => Math.Max(0, AdjustedGrossIncome - StandardDeduction);
    
    // Tax Calculations
    public decimal FederalIncomeTax { get; set; }
    public decimal StateIncomeTax { get; set; }
    public decimal TotalTaxLiability => SelfEmploymentTax + FederalIncomeTax + StateIncomeTax;
    
    // Quarterly Payment
    public decimal QuarterlyEstimatedPayment => TotalTaxLiability / 4;
    public decimal PreviousPaymentsMade { get; set; }
    public decimal RemainingDue => Math.Max(0, QuarterlyEstimatedPayment - PreviousPaymentsMade);
    
    // Effective Tax Rate
    public decimal EffectiveTaxRate => GrossIncome > 0 ? (TotalTaxLiability / GrossIncome) * 100 : 0;
    
    // Deduction Breakdown
    public Dictionary<string, decimal> DeductionsByCategory { get; set; } = new();
    
    // Safe Harbor Status
    public bool MeetsSafeHarbor { get; set; }
    public string SafeHarborMessage { get; set; } = string.Empty;
}

public class AnnualTaxProjection
{
    public int Year { get; set; }
    
    // YTD Actuals
    public decimal YTDGrossIncome { get; set; }
    public decimal YTDDeductions { get; set; }
    public decimal YTDNetProfit => YTDGrossIncome - YTDDeductions;
    public int MonthsElapsed { get; set; }
    
    // Projected Annual
    public decimal ProjectedAnnualIncome { get; set; }
    public decimal ProjectedAnnualDeductions { get; set; }
    public decimal ProjectedAnnualNetProfit => ProjectedAnnualIncome - ProjectedAnnualDeductions;
    
    // Tax Projections
    public decimal ProjectedSelfEmploymentTax { get; set; }
    public decimal ProjectedFederalIncomeTax { get; set; }
    public decimal ProjectedStateIncomeTax { get; set; }
    public decimal ProjectedTotalTax => 
        ProjectedSelfEmploymentTax + ProjectedFederalIncomeTax + ProjectedStateIncomeTax;
    
    // Payments
    public decimal EstimatedPaymentsMade { get; set; }
    public decimal RemainingTaxLiability => Math.Max(0, ProjectedTotalTax - EstimatedPaymentsMade);
    
    // Quarterly Breakdown
    public List<QuarterlyTaxEstimate> QuarterlyEstimates { get; set; } = new();
    
    // Recommendations
    public decimal RecommendedNextQuarterlyPayment { get; set; }
    public List<string> TaxSavingTips { get; set; } = new();
}

public class TaxDeductionSummary
{
    public int Year { get; set; }
    public int? Quarter { get; set; }
    
    // IRS Schedule C Categories
    public decimal FuelExpenses { get; set; }
    public decimal MaintenanceAndRepairs { get; set; }
    public decimal Insurance { get; set; }
    public decimal PermitsAndLicenses { get; set; }
    public decimal TollsAndParking { get; set; }
    public decimal DepreciationExpense { get; set; }
    public decimal InterestExpense { get; set; }
    public decimal OfficeExpenses { get; set; }
    public decimal ProfessionalFees { get; set; }
    public decimal Supplies { get; set; }
    public decimal Tires { get; set; }
    public decimal Wages { get; set; }
    public decimal PerDiemDeduction { get; set; }
    public decimal OtherExpenses { get; set; }
    
    public decimal TotalDeductions =>
        FuelExpenses + MaintenanceAndRepairs + Insurance + PermitsAndLicenses +
        TollsAndParking + DepreciationExpense + InterestExpense + OfficeExpenses +
        ProfessionalFees + Supplies + Tires + Wages + PerDiemDeduction + OtherExpenses;
    
    // Breakdown by category with percentages
    public Dictionary<string, (decimal Amount, decimal Percentage)> GetBreakdown()
    {
        var breakdown = new Dictionary<string, (decimal, decimal)>();
        if (TotalDeductions == 0) return breakdown;
        
        breakdown["Fuel"] = (FuelExpenses, (FuelExpenses / TotalDeductions) * 100);
        breakdown["Maintenance & Repairs"] = (MaintenanceAndRepairs, (MaintenanceAndRepairs / TotalDeductions) * 100);
        breakdown["Insurance"] = (Insurance, (Insurance / TotalDeductions) * 100);
        breakdown["Permits & Licenses"] = (PermitsAndLicenses, (PermitsAndLicenses / TotalDeductions) * 100);
        breakdown["Tolls & Parking"] = (TollsAndParking, (TollsAndParking / TotalDeductions) * 100);
        breakdown["Depreciation"] = (DepreciationExpense, (DepreciationExpense / TotalDeductions) * 100);
        breakdown["Interest"] = (InterestExpense, (InterestExpense / TotalDeductions) * 100);
        breakdown["Office"] = (OfficeExpenses, (OfficeExpenses / TotalDeductions) * 100);
        breakdown["Professional Fees"] = (ProfessionalFees, (ProfessionalFees / TotalDeductions) * 100);
        breakdown["Supplies"] = (Supplies, (Supplies / TotalDeductions) * 100);
        breakdown["Tires"] = (Tires, (Tires / TotalDeductions) * 100);
        breakdown["Wages"] = (Wages, (Wages / TotalDeductions) * 100);
        breakdown["Per Diem"] = (PerDiemDeduction, (PerDiemDeduction / TotalDeductions) * 100);
        breakdown["Other"] = (OtherExpenses, (OtherExpenses / TotalDeductions) * 100);
        
        return breakdown;
    }
}

public class PerDiemCalculation
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDaysAway { get; set; }
    public int FullDays { get; set; }
    public int PartialDays { get; set; }
    public decimal DOTPerDiemRate { get; set; } = 69.00m; // 2024 rate
    public decimal FullDayAmount => FullDays * DOTPerDiemRate;
    public decimal PartialDayAmount => PartialDays * (DOTPerDiemRate * 0.75m);
    public decimal TotalPerDiemDeduction => FullDayAmount + PartialDayAmount;
    public List<string> TravelDays { get; set; } = new();
}

public enum TaxFilingStatus
{
    Single = 0,
    MarriedFilingJointly = 1,
    MarriedFilingSeparately = 2,
    HeadOfHousehold = 3
}
