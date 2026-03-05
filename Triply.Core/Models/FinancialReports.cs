namespace Triply.Core.Models;

public class IncomeStatement
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string PeriodLabel => $"{StartDate:MMM d, yyyy} - {EndDate:MMM d, yyyy}";
    
    // Revenue
    public List<AccountBalance> RevenueAccounts { get; set; } = new();
    public decimal TotalRevenue => RevenueAccounts.Sum(a => a.Balance);
    
    // Cost of Goods Sold (if applicable)
    public List<AccountBalance> COGSAccounts { get; set; } = new();
    public decimal TotalCOGS => COGSAccounts.Sum(a => a.Balance);
    
    // Gross Profit
    public decimal GrossProfit => TotalRevenue - TotalCOGS;
    public decimal GrossProfitMargin => TotalRevenue > 0 ? (GrossProfit / TotalRevenue) * 100 : 0;
    
    // Operating Expenses
    public List<AccountBalance> OperatingExpenses { get; set; } = new();
    public decimal TotalOperatingExpenses => OperatingExpenses.Sum(a => a.Balance);
    
    // Operating Income
    public decimal OperatingIncome => GrossProfit - TotalOperatingExpenses;
    public decimal OperatingMargin => TotalRevenue > 0 ? (OperatingIncome / TotalRevenue) * 100 : 0;
    
    // Other Income/Expenses
    public List<AccountBalance> OtherIncome { get; set; } = new();
    public List<AccountBalance> OtherExpenses { get; set; } = new();
    public decimal TotalOtherIncome => OtherIncome.Sum(a => a.Balance);
    public decimal TotalOtherExpenses => OtherExpenses.Sum(a => a.Balance);
    
    // Net Income
    public decimal NetIncome => OperatingIncome + TotalOtherIncome - TotalOtherExpenses;
    public decimal NetProfitMargin => TotalRevenue > 0 ? (NetIncome / TotalRevenue) * 100 : 0;
}

public class BalanceSheet
{
    public DateTime AsOfDate { get; set; }
    public string DateLabel => $"As of {AsOfDate:MMMM d, yyyy}";
    
    // Assets
    public List<AccountBalance> CurrentAssets { get; set; } = new();
    public List<AccountBalance> FixedAssets { get; set; } = new();
    public List<AccountBalance> OtherAssets { get; set; } = new();
    public decimal TotalCurrentAssets => CurrentAssets.Sum(a => a.Balance);
    public decimal TotalFixedAssets => FixedAssets.Sum(a => a.Balance);
    public decimal TotalOtherAssets => OtherAssets.Sum(a => a.Balance);
    public decimal TotalAssets => TotalCurrentAssets + TotalFixedAssets + TotalOtherAssets;
    
    // Liabilities
    public List<AccountBalance> CurrentLiabilities { get; set; } = new();
    public List<AccountBalance> LongTermLiabilities { get; set; } = new();
    public decimal TotalCurrentLiabilities => CurrentLiabilities.Sum(a => a.Balance);
    public decimal TotalLongTermLiabilities => LongTermLiabilities.Sum(a => a.Balance);
    public decimal TotalLiabilities => TotalCurrentLiabilities + TotalLongTermLiabilities;
    
    // Equity
    public List<AccountBalance> EquityAccounts { get; set; } = new();
    public decimal RetainedEarnings { get; set; }
    public decimal TotalEquity => EquityAccounts.Sum(a => a.Balance) + RetainedEarnings;
    
    // Balance Check
    public decimal TotalLiabilitiesAndEquity => TotalLiabilities + TotalEquity;
    public bool IsBalanced => Math.Abs(TotalAssets - TotalLiabilitiesAndEquity) < 0.01m;
}

public class CashFlowStatement
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string PeriodLabel => $"{StartDate:MMM d, yyyy} - {EndDate:MMM d, yyyy}";
    
    // Operating Activities
    public decimal NetIncome { get; set; }
    public List<CashFlowItem> OperatingAdjustments { get; set; } = new();
    public decimal TotalOperatingAdjustments => OperatingAdjustments.Sum(i => i.Amount);
    public decimal NetCashFromOperations => NetIncome + TotalOperatingAdjustments;
    
    // Investing Activities
    public List<CashFlowItem> InvestingActivities { get; set; } = new();
    public decimal NetCashFromInvesting => InvestingActivities.Sum(i => i.Amount);
    
    // Financing Activities
    public List<CashFlowItem> FinancingActivities { get; set; } = new();
    public decimal NetCashFromFinancing => FinancingActivities.Sum(i => i.Amount);
    
    // Net Change in Cash
    public decimal NetChangeInCash => NetCashFromOperations + NetCashFromInvesting + NetCashFromFinancing;
    public decimal BeginningCashBalance { get; set; }
    public decimal EndingCashBalance => BeginningCashBalance + NetChangeInCash;
}

public class AccountBalance
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal Balance { get; set; }
    public List<AccountBalance> SubAccounts { get; set; } = new();
}

public class CashFlowItem
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public CashFlowCategory Category { get; set; }
}

public enum CashFlowCategory
{
    OperatingAdjustment = 0,
    Investing = 1,
    Financing = 2
}

public class TrialBalance
{
    public DateTime AsOfDate { get; set; }
    public List<TrialBalanceLine> Lines { get; set; } = new();
    public decimal TotalDebits => Lines.Sum(l => l.DebitBalance);
    public decimal TotalCredits => Lines.Sum(l => l.CreditBalance);
    public bool IsBalanced => Math.Abs(TotalDebits - TotalCredits) < 0.01m;
}

public class TrialBalanceLine
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
}

public class AccountLedger
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal BeginningBalance { get; set; }
    public List<LedgerEntry> Entries { get; set; } = new();
    public decimal TotalDebits => Entries.Sum(e => e.DebitAmount);
    public decimal TotalCredits => Entries.Sum(e => e.CreditAmount);
    public decimal EndingBalance { get; set; }
}

public class LedgerEntry
{
    public DateTime Date { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal RunningBalance { get; set; }
    public bool IsReconciled { get; set; }
}

public class FinancialComparison
{
    public string Label { get; set; } = string.Empty;
    public decimal CurrentPeriod { get; set; }
    public decimal PriorPeriod { get; set; }
    public decimal Change => CurrentPeriod - PriorPeriod;
    public decimal ChangePercent => PriorPeriod != 0 ? (Change / Math.Abs(PriorPeriod)) * 100 : 0;
}

public class FinancialRatios
{
    public DateTime AsOfDate { get; set; }
    
    // Liquidity Ratios
    public decimal CurrentRatio { get; set; } // Current Assets / Current Liabilities
    public decimal QuickRatio { get; set; } // (Current Assets - Inventory) / Current Liabilities
    
    // Profitability Ratios
    public decimal GrossProfitMargin { get; set; }
    public decimal OperatingMargin { get; set; }
    public decimal NetProfitMargin { get; set; }
    public decimal ReturnOnAssets { get; set; } // Net Income / Total Assets
    public decimal ReturnOnEquity { get; set; } // Net Income / Total Equity
    
    // Efficiency Ratios
    public decimal AssetTurnover { get; set; } // Revenue / Total Assets
    public int DaysReceivableOutstanding { get; set; } // (AR / Revenue) * 365
    
    // Leverage Ratios
    public decimal DebtToAssetRatio { get; set; } // Total Liabilities / Total Assets
    public decimal DebtToEquityRatio { get; set; } // Total Liabilities / Total Equity
}

public class MonthlyFinancialSummary
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetIncome { get; set; }
    public decimal ProfitMargin { get; set; }
}
