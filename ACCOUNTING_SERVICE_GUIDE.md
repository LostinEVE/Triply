# Accounting Service - Complete Guide

## Overview

The `AccountingService` provides full double-entry bookkeeping and financial reporting specifically designed for trucking businesses. It includes a pre-configured Chart of Accounts, automated journal entries, and standard financial statements.

## Why Proper Accounting Matters for Trucking

- **Financial Visibility**: Know exactly where money is coming from and going
- **Tax Compliance**: Proper records for IRS Schedule C and audits
- **Business Decisions**: Make data-driven decisions with accurate financials
- **Loan Applications**: Banks require proper financial statements
- **Profitability Analysis**: Track which trucks/lanes are profitable
- **Cash Flow Management**: Prevent cash crunches

## Core Features

### 1. Chart of Accounts (Pre-Configured for Trucking)
### 2. Double-Entry Bookkeeping Helpers
### 3. Financial Statements (P&L, Balance Sheet, Cash Flow)
### 4. Period Management (Monthly, Quarterly, Annual)
### 5. Bank Reconciliation

---

## 1. Chart of Accounts

Pre-configured accounts specifically for OTR trucking businesses, following IRS Schedule C categories.

### Account Structure

```
1000-1999: Assets
2000-2999: Liabilities
3000-3999: Equity
4000-4999: Revenue
5000-5999: Expenses
```

### Complete Account List

#### **Assets (1000-1999)**

**Current Assets:**
- `1000` Cash (checking/savings)
- `1010` Petty Cash
- `1100` Accounts Receivable (outstanding invoices)
- `1200` Fuel Advances (prepaid fuel cards)
- `1210` Prepaid Insurance
- `1220` Prepaid Expenses

**Fixed Assets:**
- `1500` Trucks (at cost)
- `1510` Accumulated Depreciation - Trucks (contra-asset)
- `1520` Trailers (at cost)
- `1530` Accumulated Depreciation - Trailers
- `1540` Equipment (office equipment, computers)
- `1550` Accumulated Depreciation - Equipment

**Other Assets:**
- `1800` Deposits (security deposits)

#### **Liabilities (2000-2999)**

**Current Liabilities:**
- `2000` Accounts Payable (vendor bills)
- `2010` Credit Cards Payable
- `2020` Fuel Card Payable
- `2100` Sales Tax Payable
- `2110` IFTA Payable (fuel tax)
- `2120` Payroll Taxes Payable
- `2200` Current Portion - Truck Loans

**Long-Term Liabilities:**
- `2500` Truck Loans Payable
- `2510` Trailer Loans Payable
- `2520` Equipment Loans Payable

#### **Equity (3000-3999)**

- `3000` Owner's Equity (capital)
- `3010` Owner's Draw (withdrawals)
- `3900` Retained Earnings (accumulated profits)

#### **Revenue (4000-4999)**

- `4000` Freight Revenue (main revenue)
- `4010` Detention Pay
- `4020` Fuel Surcharge
- `4030` Accessorial Charges
- `4900` Other Income

#### **Expenses (5000-5999)**

**Fuel:**
- `5000` Fuel - Diesel
- `5010` Fuel - DEF

**Maintenance:**
- `5100` Maintenance - Preventive
- `5110` Repairs - Truck
- `5120` Repairs - Trailer
- `5130` Tires

**Driver:**
- `5200` Driver Wages
- `5210` Driver Per Diem
- `5220` Payroll Taxes

**Insurance:**
- `5300` Truck Insurance
- `5310` Cargo Insurance
- `5320` Health Insurance

**Permits & Taxes:**
- `5400` Permits & Licenses
- `5410` IFTA Tax
- `5420` Heavy Vehicle Use Tax (Form 2290)

**Road Expenses:**
- `5500` Tolls
- `5510` Parking
- `5520` Scales
- `5530` Lumper Fees

**Office & Admin:**
- `5600` Office Supplies
- `5610` Professional Fees (accounting, legal)
- `5620` Software & Subscriptions
- `5630` Telephone & Internet
- `5640` Advertising & Marketing

**Financing:**
- `5700` Interest - Truck Loans
- `5710` Interest - Credit Cards
- `5720` Bank Fees

**Depreciation:**
- `5800` Depreciation - Trucks
- `5810` Depreciation - Trailers
- `5820` Depreciation - Equipment

**Other:**
- `5900` Miscellaneous Expense

---

## 2. Double-Entry Bookkeeping

### Understanding Double-Entry

Every transaction affects **at least two accounts**:
- **Debit**: Increases Assets/Expenses, Decreases Liabilities/Equity/Revenue
- **Credit**: Increases Liabilities/Equity/Revenue, Decreases Assets/Expenses

**The Accounting Equation:**
```
Assets = Liabilities + Equity
```

**Must always balance:**
```
Total Debits = Total Credits
```

### Method: RecordInvoiceAsync()

Creates journal entry when you invoice a customer.

```csharp
@inject AccountingService AccountingService

// When invoice is created
var invoice = new Invoice
{
    InvoiceNumber = "INV-1001",
    CustomerId = customerId,
    TotalAmount = 2500m,
    InvoiceDate = DateTime.UtcNow
};

await UnitOfWork.Invoices.AddAsync(invoice);
await UnitOfWork.SaveAsync();

// Record the accounting entry
var journalEntry = await AccountingService.RecordInvoiceAsync(invoice);

// This creates:
// DR Accounts Receivable $2,500
// CR Freight Revenue      $2,500
```

**What Happens:**
1. **Increases** Accounts Receivable (Asset) - someone owes you money
2. **Increases** Freight Revenue (Revenue) - you earned revenue
3. Creates audit trail with invoice reference

### Method: RecordPaymentAsync()

Records when customer pays their invoice.

```csharp
// When payment is received
invoice.Status = InvoiceStatus.Paid;
invoice.PaidDate = DateTime.UtcNow;
await UnitOfWork.SaveAsync();

// Record the accounting entry
await AccountingService.RecordPaymentAsync(invoice, invoice.TotalAmount);

// This creates:
// DR Cash                 $2,500
// CR Accounts Receivable  $2,500
```

**What Happens:**
1. **Increases** Cash (Asset) - money in the bank
2. **Decreases** Accounts Receivable (Asset) - they don't owe you anymore

### Method: RecordExpenseAsync()

Records an expense with option for cash or accounts payable.

```csharp
// Record expense paid with cash
var expense = new Expense
{
    Category = ExpenseCategory.Insurance,
    Amount = 850m,
    ExpenseDate = DateTime.UtcNow,
    Description = "Monthly truck insurance"
};

await UnitOfWork.Expenses.AddAsync(expense);
await UnitOfWork.SaveAsync();

// Record as paid expense
await AccountingService.RecordExpenseAsync(expense, paid: true);

// This creates:
// DR Insurance Expense $850
// CR Cash             $850
```

**Unpaid Expenses (Accounts Payable):**
```csharp
// Record expense not yet paid
await AccountingService.RecordExpenseAsync(expense, paid: false);

// This creates:
// DR Insurance Expense  $850
// CR Accounts Payable   $850

// Later, when you pay:
// DR Accounts Payable $850
// CR Cash            $850
```

### Method: RecordFuelPurchaseAsync()

Records fuel purchases with automatic account mapping.

```csharp
var fuelEntry = new FuelEntry
{
    TruckId = "TRUCK-001",
    Gallons = 125m,
    PricePerGallon = 3.85m,
    TotalCost = 481.25m,
    FuelType = FuelType.Diesel,
    FuelDate = DateTime.UtcNow
};

await UnitOfWork.FuelEntries.AddAsync(fuelEntry);
await UnitOfWork.SaveAsync();

// Record with fuel card (unpaid)
await AccountingService.RecordFuelPurchaseAsync(fuelEntry, paid: false);

// This creates:
// DR Fuel - Diesel        $481.25
// CR Fuel Card Payable    $481.25
```

### Method: RecordMaintenanceAsync()

Records maintenance and repair expenses.

```csharp
var maintenance = new MaintenanceRecord
{
    TruckId = "TRUCK-001",
    MaintenanceType = MaintenanceType.Preventive,
    Description = "Oil change and PM service",
    TotalCost = 450m,
    MaintenanceDate = DateTime.UtcNow
};

await UnitOfWork.MaintenanceRecords.AddAsync(maintenance);
await UnitOfWork.SaveAsync();

await AccountingService.RecordMaintenanceAsync(maintenance, paid: true);

// This creates:
// DR Maintenance - Preventive $450
// CR Cash                     $450
```

---

## 3. Financial Statements

### Income Statement (Profit & Loss)

Shows revenue and expenses over a period of time.

#### Method: GetIncomeStatementAsync()

```csharp
var incomeStatement = await AccountingService.GetIncomeStatementAsync(
    startDate: new DateTime(2024, 1, 1),
    endDate: new DateTime(2024, 12, 31)
);

Console.WriteLine("=== INCOME STATEMENT ===");
Console.WriteLine(incomeStatement.PeriodLabel);
Console.WriteLine();

// Revenue
Console.WriteLine("REVENUE:");
foreach (var account in incomeStatement.RevenueAccounts)
{
    Console.WriteLine($"  {account.AccountName}: {account.Balance:C}");
}
Console.WriteLine($"Total Revenue: {incomeStatement.TotalRevenue:C}");
Console.WriteLine();

// Cost of Goods Sold (if any)
if (incomeStatement.TotalCOGS > 0)
{
    Console.WriteLine($"Cost of Goods Sold: ({incomeStatement.TotalCOGS:C})");
    Console.WriteLine($"Gross Profit: {incomeStatement.GrossProfit:C} ({incomeStatement.GrossProfitMargin:N1}%)");
    Console.WriteLine();
}

// Operating Expenses
Console.WriteLine("OPERATING EXPENSES:");
foreach (var account in incomeStatement.OperatingExpenses.OrderByDescending(a => a.Balance))
{
    var pct = incomeStatement.TotalRevenue > 0 ? (account.Balance / incomeStatement.TotalRevenue * 100) : 0;
    Console.WriteLine($"  {account.AccountName}: {account.Balance:C} ({pct:N1}%)");
}
Console.WriteLine($"Total Operating Expenses: ({incomeStatement.TotalOperatingExpenses:C})");
Console.WriteLine();

Console.WriteLine($"Operating Income: {incomeStatement.OperatingIncome:C} ({incomeStatement.OperatingMargin:N1}%)");
Console.WriteLine();

// Other Income/Expenses
if (incomeStatement.TotalOtherIncome > 0 || incomeStatement.TotalOtherExpenses > 0)
{
    Console.WriteLine($"Other Income: {incomeStatement.TotalOtherIncome:C}");
    Console.WriteLine($"Other Expenses: ({incomeStatement.TotalOtherExpenses:C})");
    Console.WriteLine();
}

Console.WriteLine($"===============================");
Console.WriteLine($"NET INCOME: {incomeStatement.NetIncome:C} ({incomeStatement.NetProfitMargin:N1}%)");
```

**Example Output:**
```
=== INCOME STATEMENT ===
Jan 1, 2024 - Dec 31, 2024

REVENUE:
  Freight Revenue: $450,000
  Detention Pay: $12,000
  Fuel Surcharge: $18,000
Total Revenue: $480,000

OPERATING EXPENSES:
  Fuel - Diesel: $160,000 (33.3%)
  Driver Wages: $120,000 (25.0%)
  Truck Insurance: $28,000 (5.8%)
  Maintenance - Preventive: $24,000 (5.0%)
  Tires: $12,000 (2.5%)
  ...
Total Operating Expenses: ($380,000)

Operating Income: $100,000 (20.8%)

===============================
NET INCOME: $100,000 (20.8%)
```

### Balance Sheet

Snapshot of financial position at a specific point in time.

#### Method: GetBalanceSheetAsync()

```csharp
var balanceSheet = await AccountingService.GetBalanceSheetAsync(
    asOfDate: new DateTime(2024, 12, 31)
);

Console.WriteLine("=== BALANCE SHEET ===");
Console.WriteLine(balanceSheet.DateLabel);
Console.WriteLine();

// Assets
Console.WriteLine("ASSETS");
Console.WriteLine("Current Assets:");
foreach (var account in balanceSheet.CurrentAssets)
{
    Console.WriteLine($"  {account.AccountName}: {account.Balance:C}");
}
Console.WriteLine($"Total Current Assets: {balanceSheet.TotalCurrentAssets:C}");
Console.WriteLine();

Console.WriteLine("Fixed Assets:");
foreach (var account in balanceSheet.FixedAssets)
{
    Console.WriteLine($"  {account.AccountName}: {account.Balance:C}");
}
Console.WriteLine($"Total Fixed Assets: {balanceSheet.TotalFixedAssets:C}");
Console.WriteLine($"TOTAL ASSETS: {balanceSheet.TotalAssets:C}");
Console.WriteLine();

// Liabilities
Console.WriteLine("LIABILITIES");
Console.WriteLine("Current Liabilities:");
foreach (var account in balanceSheet.CurrentLiabilities)
{
    Console.WriteLine($"  {account.AccountName}: {account.Balance:C}");
}
Console.WriteLine($"Total Current Liabilities: {balanceSheet.TotalCurrentLiabilities:C}");
Console.WriteLine();

Console.WriteLine("Long-Term Liabilities:");
foreach (var account in balanceSheet.LongTermLiabilities)
{
    Console.WriteLine($"  {account.AccountName}: {account.Balance:C}");
}
Console.WriteLine($"Total Liabilities: {balanceSheet.TotalLiabilities:C}");
Console.WriteLine();

// Equity
Console.WriteLine("EQUITY");
foreach (var account in balanceSheet.EquityAccounts)
{
    Console.WriteLine($"  {account.AccountName}: {account.Balance:C}");
}
Console.WriteLine($"  Retained Earnings: {balanceSheet.RetainedEarnings:C}");
Console.WriteLine($"Total Equity: {balanceSheet.TotalEquity:C}");
Console.WriteLine();

Console.WriteLine($"TOTAL LIABILITIES & EQUITY: {balanceSheet.TotalLiabilitiesAndEquity:C}");
Console.WriteLine($"Balanced: {(balanceSheet.IsBalanced ? "✅ YES" : "❌ NO")}");
```

### Cash Flow Statement

Shows sources and uses of cash over a period.

#### Method: GetCashFlowStatementAsync()

```csharp
var cashFlow = await AccountingService.GetCashFlowStatementAsync(
    startDate: new DateTime(2024, 1, 1),
    endDate: new DateTime(2024, 12, 31)
);

Console.WriteLine("=== CASH FLOW STATEMENT ===");
Console.WriteLine(cashFlow.PeriodLabel);
Console.WriteLine();

Console.WriteLine($"Net Income: {cashFlow.NetIncome:C}");
Console.WriteLine();

Console.WriteLine("Adjustments:");
foreach (var adj in cashFlow.OperatingAdjustments)
{
    Console.WriteLine($"  {adj.Description}: {adj.Amount:C}");
}
Console.WriteLine($"Net Cash from Operations: {cashFlow.NetCashFromOperations:C}");
Console.WriteLine();

Console.WriteLine($"Net Cash from Investing: {cashFlow.NetCashFromInvesting:C}");
Console.WriteLine($"Net Cash from Financing: {cashFlow.NetCashFromFinancing:C}");
Console.WriteLine();

Console.WriteLine($"Net Change in Cash: {cashFlow.NetChangeInCash:C}");
Console.WriteLine($"Beginning Cash: {cashFlow.BeginningCashBalance:C}");
Console.WriteLine($"Ending Cash: {cashFlow.EndingCashBalance:C}");
```

---

## 4. Period Management

### Method: CreateAccountingPeriodsAsync()

Sets up periods for a year (monthly, quarterly, annual).

```csharp
// Create all periods for 2024
await AccountingService.CreateAccountingPeriodsAsync(2024);

// This creates:
// - 12 monthly periods (Jan, Feb, Mar, ...)
// - 4 quarterly periods (Q1, Q2, Q3, Q4)
// - 1 annual period (FY 2024)
```

### Method: CompareYearOverYearAsync()

Compares current year to prior year.

```csharp
var comparisons = await AccountingService.CompareYearOverYearAsync(
    currentPeriodStart: new DateTime(2024, 1, 1),
    currentPeriodEnd: new DateTime(2024, 12, 31)
);

Console.WriteLine("=== YEAR-OVER-YEAR COMPARISON ===");
Console.WriteLine("2024 vs 2023");
Console.WriteLine();

foreach (var comparison in comparisons)
{
    var trend = comparison.Change >= 0 ? "📈" : "📉";
    Console.WriteLine($"{comparison.Label}:");
    Console.WriteLine($"  2024: {comparison.CurrentPeriod:C}");
    Console.WriteLine($"  2023: {comparison.PriorPeriod:C}");
    Console.WriteLine($"  Change: {comparison.Change:C} ({comparison.ChangePercent:+0.0;-0.0}%) {trend}");
    Console.WriteLine();
}
```

**Example Output:**
```
Total Revenue:
  2024: $520,000
  2023: $450,000
  Change: $70,000 (+15.6%) 📈

Net Income:
  2024: $105,000
  2023: $85,000
  Change: $20,000 (+23.5%) 📈
```

### Method: GetMonthlyFinancialSummaryAsync()

Gets month-by-month performance.

```csharp
var monthlySummary = await AccountingService.GetMonthlyFinancialSummaryAsync(2024);

Console.WriteLine("=== MONTHLY SUMMARY ===");
foreach (var month in monthlySummary)
{
    Console.WriteLine($"{month.MonthName}:");
    Console.WriteLine($"  Revenue: {month.Revenue:C}");
    Console.WriteLine($"  Expenses: {month.Expenses:C}");
    Console.WriteLine($"  Net Income: {month.NetIncome:C} ({month.ProfitMargin:N1}%)");
    Console.WriteLine();
}

// Calculate totals
var totalRevenue = monthlySummary.Sum(m => m.Revenue);
var totalExpenses = monthlySummary.Sum(m => m.Expenses);
var totalNetIncome = monthlySummary.Sum(m => m.NetIncome);

Console.WriteLine($"YTD Totals:");
Console.WriteLine($"  Revenue: {totalRevenue:C}");
Console.WriteLine($"  Expenses: {totalExpenses:C}");
Console.WriteLine($"  Net Income: {totalNetIncome:C}");
```

### Method: ClosePeriodAsync()

Closes a period and transfers net income to retained earnings.

```csharp
// Close December 2024
var closingEntry = await AccountingService.ClosePeriodAsync(
    periodId: 12, // December period
    closedBy: "Owner"
);

// This creates:
// DR Revenue accounts (close them to zero)
// CR Expense accounts (close them to zero)
// DR/CR Retained Earnings (net income transfer)
```

**When to Close Periods:**
- **Monthly**: After month-end reconciliation
- **Quarterly**: After IFTA and tax filings
- **Annually**: After tax return is filed

---

## 5. Bank Reconciliation

### Method: StartBankReconciliationAsync()

Begins bank statement reconciliation process.

```csharp
// Start reconciliation for Cash account
var reconciliation = await AccountingService.StartBankReconciliationAsync(
    accountId: 1, // Cash account
    statementDate: new DateTime(2024, 3, 31),
    statementBeginningBalance: 45000m,
    statementEndingBalance: 52000m
);

Console.WriteLine($"Bank Statement: {reconciliation.StatementEndingBalance:C}");
Console.WriteLine($"Book Balance: {reconciliation.BookEndingBalance:C}");
Console.WriteLine($"Difference: {reconciliation.Difference:C}");

if (!reconciliation.IsReconciled)
{
    Console.WriteLine("⚠️ Not reconciled - investigate difference");
}
```

### Method: GetUnreconciledTransactionsAsync()

Gets transactions not yet matched to bank statement.

```csharp
var unreconciled = await AccountingService.GetUnreconciledTransactionsAsync(
    accountId: 1, // Cash
    throughDate: new DateTime(2024, 3, 31)
);

Console.WriteLine("=== UNRECONCILED TRANSACTIONS ===");
foreach (var entry in unreconciled)
{
    Console.WriteLine($"{entry.Date:MM/dd/yyyy} - {entry.Description}");
    Console.WriteLine($"  DR: {entry.DebitAmount:C}  CR: {entry.CreditAmount:C}");
}
```

### Method: ReconcileTransactionsAsync()

Marks transactions as reconciled.

```csharp
// After matching to bank statement, mark as reconciled
var lineIds = new List<int> { 101, 102, 103, 104 };
await AccountingService.ReconcileTransactionsAsync(lineIds, reconciliationId: 5);
```

---

## 6. Additional Reports

### Trial Balance

Lists all accounts with their debit/credit balances.

#### Method: GetTrialBalanceAsync()

```csharp
var trialBalance = await AccountingService.GetTrialBalanceAsync(
    asOfDate: new DateTime(2024, 12, 31)
);

Console.WriteLine("=== TRIAL BALANCE ===");
Console.WriteLine(trialBalance.AsOfDate.ToString("MMMM d, yyyy"));
Console.WriteLine();

Console.WriteLine($"{"Account",-40} {"Debit",15} {"Credit",15}");
Console.WriteLine(new string('-', 70));

foreach (var line in trialBalance.Lines)
{
    var name = $"{line.AccountNumber} - {line.AccountName}";
    Console.WriteLine($"{name,-40} {(line.DebitBalance > 0 ? line.DebitBalance.ToString("C") : ""),15} {(line.CreditBalance > 0 ? line.CreditBalance.ToString("C") : ""),15}");
}

Console.WriteLine(new string('-', 70));
Console.WriteLine($"{"TOTALS",-40} {trialBalance.TotalDebits.ToString("C"),15} {trialBalance.TotalCredits.ToString("C"),15}");
Console.WriteLine();
Console.WriteLine($"Balanced: {(trialBalance.IsBalanced ? "✅ YES" : "❌ NO")}");
```

### General Ledger

Detailed listing of all transactions for an account.

#### Method: GetAccountLedgerAsync()

```csharp
var ledger = await AccountingService.GetAccountLedgerAsync(
    accountId: 1, // Cash account
    startDate: new DateTime(2024, 1, 1),
    endDate: new DateTime(2024, 12, 31)
);

Console.WriteLine($"=== GENERAL LEDGER ===");
Console.WriteLine($"{ledger.AccountNumber} - {ledger.AccountName}");
Console.WriteLine($"{ledger.StartDate:MM/dd/yyyy} - {ledger.EndDate:MM/dd/yyyy}");
Console.WriteLine();

Console.WriteLine($"Beginning Balance: {ledger.BeginningBalance:C}");
Console.WriteLine();

Console.WriteLine($"{"Date",-12} {"Entry",-15} {"Description",-30} {"Debit",12} {"Credit",12} {"Balance",12}");
Console.WriteLine(new string('-', 95));

foreach (var entry in ledger.Entries)
{
    var reconciled = entry.IsReconciled ? "✓" : " ";
    Console.WriteLine($"{entry.Date:MM/dd/yyyy} {reconciled} {entry.EntryNumber,-15} {entry.Description,-30} {(entry.DebitAmount > 0 ? entry.DebitAmount.ToString("N2") : ""),12} {(entry.CreditAmount > 0 ? entry.CreditAmount.ToString("N2") : ""),12} {entry.RunningBalance,12:N2}");
}

Console.WriteLine(new string('-', 95));
Console.WriteLine($"{"Ending Balance",-60} {ledger.EndingBalance,12:C}");
```

### Financial Ratios

Key performance indicators.

#### Method: CalculateFinancialRatiosAsync()

```csharp
var ratios = await AccountingService.CalculateFinancialRatiosAsync(
    asOfDate: DateTime.UtcNow,
    periodsBack: 12 // Last 12 months
);

Console.WriteLine("=== FINANCIAL RATIOS ===");
Console.WriteLine();

Console.WriteLine("LIQUIDITY:");
Console.WriteLine($"  Current Ratio: {ratios.CurrentRatio:N2} (target: > 1.0)");
Console.WriteLine($"  Quick Ratio: {ratios.QuickRatio:N2} (target: > 1.0)");
Console.WriteLine();

Console.WriteLine("PROFITABILITY:");
Console.WriteLine($"  Gross Profit Margin: {ratios.GrossProfitMargin:N1}%");
Console.WriteLine($"  Operating Margin: {ratios.OperatingMargin:N1}%");
Console.WriteLine($"  Net Profit Margin: {ratios.NetProfitMargin:N1}% (target: > 10%)");
Console.WriteLine($"  Return on Assets: {ratios.ReturnOnAssets:N1}%");
Console.WriteLine($"  Return on Equity: {ratios.ReturnOnEquity:N1}%");
Console.WriteLine();

Console.WriteLine("EFFICIENCY:");
Console.WriteLine($"  Asset Turnover: {ratios.AssetTurnover:N2}x");
Console.WriteLine($"  Days Receivable Outstanding: {ratios.DaysReceivableOutstanding} days (target: < 45)");
Console.WriteLine();

Console.WriteLine("LEVERAGE:");
Console.WriteLine($"  Debt to Asset Ratio: {ratios.DebtToAssetRatio:N1}% (target: < 60%)");
Console.WriteLine($"  Debt to Equity Ratio: {ratios.DebtToEquityRatio:N1}%");
```

**Ratio Benchmarks for Trucking:**

| Ratio | Good | Warning | Poor |
|-------|------|---------|------|
| Current Ratio | > 1.5 | 1.0-1.5 | < 1.0 |
| Net Profit Margin | > 15% | 10-15% | < 10% |
| Days Receivable | < 30 | 30-45 | > 45 |
| Debt to Asset | < 50% | 50-70% | > 70% |

---

## Real-World Usage Examples

### Example 1: End-of-Month Close

```csharp
public async Task PerformMonthEndCloseAsync(int year, int month)
{
    Console.WriteLine($"=== MONTH-END CLOSE: {new DateTime(year, month, 1):MMMM yyyy} ===");
    
    var monthStart = new DateTime(year, month, 1);
    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

    // 1. Generate financial statements
    var income = await _accountingService.GetIncomeStatementAsync(monthStart, monthEnd);
    var balanceSheet = await _accountingService.GetBalanceSheetAsync(monthEnd);

    Console.WriteLine($"Revenue: {income.TotalRevenue:C}");
    Console.WriteLine($"Expenses: {income.TotalOperatingExpenses:C}");
    Console.WriteLine($"Net Income: {income.NetIncome:C}");
    Console.WriteLine();

    // 2. Bank reconciliation
    Console.WriteLine("📊 Starting bank reconciliation...");
    var unreconciled = await _accountingService.GetUnreconciledTransactionsAsync(
        accountId: 1, // Cash
        throughDate: monthEnd
    );

    Console.WriteLine($"Unreconciled transactions: {unreconciled.Count}");

    // 3. Check if balanced
    var trialBalance = await _accountingService.GetTrialBalanceAsync(monthEnd);
    Console.WriteLine($"Trial Balance: {(trialBalance.IsBalanced ? "✅ Balanced" : "❌ Out of Balance")}");

    if (!trialBalance.IsBalanced)
    {
        Console.WriteLine($"  Difference: {Math.Abs(trialBalance.TotalDebits - trialBalance.TotalCredits):C}");
        throw new Exception("Books are out of balance - cannot close period");
    }

    // 4. Review key metrics
    var ratios = await _accountingService.CalculateFinancialRatiosAsync(monthEnd, 1);
    Console.WriteLine($"Net Profit Margin: {ratios.NetProfitMargin:N1}%");
    Console.WriteLine($"Current Ratio: {ratios.CurrentRatio:N2}");

    // 5. If quarterly, close the period
    if (month % 3 == 0)
    {
        Console.WriteLine("📆 End of quarter - closing period...");
        // await _accountingService.ClosePeriodAsync(periodId, "System");
    }

    Console.WriteLine("✅ Month-end close complete!");
}
```

### Example 2: Automated Entry Creation

```csharp
public class AutomatedBookkeeping
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AccountingService _accountingService;

    public async Task ProcessAllTransactionsAsync(DateTime date)
    {
        Console.WriteLine($"Processing transactions for {date:MM/dd/yyyy}...");

        // Process invoices
        var newInvoices = await _unitOfWork.Invoices
            .GetQueryable()
            .Where(i => i.InvoiceDate.Date == date.Date && i.Status == InvoiceStatus.Sent)
            .ToListAsync();

        foreach (var invoice in newInvoices)
        {
            await _accountingService.RecordInvoiceAsync(invoice);
            Console.WriteLine($"  ✅ Recorded invoice {invoice.InvoiceNumber}");
        }

        // Process payments received
        var paidInvoices = await _unitOfWork.Invoices
            .GetQueryable()
            .Where(i => i.PaidDate.HasValue && i.PaidDate.Value.Date == date.Date)
            .ToListAsync();

        foreach (var invoice in paidInvoices)
        {
            await _accountingService.RecordPaymentAsync(invoice, invoice.TotalAmount);
            Console.WriteLine($"  ✅ Recorded payment for {invoice.InvoiceNumber}");
        }

        // Process fuel purchases
        var fuelEntries = await _unitOfWork.FuelEntries
            .GetQueryable()
            .Where(f => f.FuelDate.Date == date.Date)
            .ToListAsync();

        foreach (var fuel in fuelEntries)
        {
            await _accountingService.RecordFuelPurchaseAsync(fuel, paid: false);
            Console.WriteLine($"  ✅ Recorded fuel purchase: {fuel.Gallons:N1} gal");
        }

        // Process expenses
        var expenses = await _unitOfWork.Expenses
            .GetQueryable()
            .Where(e => e.ExpenseDate.Date == date.Date)
            .ToListAsync();

        foreach (var expense in expenses)
        {
            await _accountingService.RecordExpenseAsync(expense, paid: true);
            Console.WriteLine($"  ✅ Recorded expense: {expense.Description}");
        }

        // Process maintenance
        var maintenance = await _unitOfWork.MaintenanceRecords
            .GetQueryable()
            .Where(m => m.MaintenanceDate.Date == date.Date)
            .ToListAsync();

        foreach (var maint in maintenance)
        {
            await _accountingService.RecordMaintenanceAsync(maint, paid: true);
            Console.WriteLine($"  ✅ Recorded maintenance: {maint.Description}");
        }

        Console.WriteLine($"✅ All transactions processed for {date:MM/dd/yyyy}");
    }
}
```

### Example 3: Financial Dashboard

```csharp
public async Task<FinancialDashboard> GetFinancialDashboardAsync()
{
    var today = DateTime.UtcNow;
    var monthStart = new DateTime(today.Year, today.Month, 1);
    var yearStart = new DateTime(today.Year, 1, 1);

    // Current month
    var monthIncome = await _accountingService.GetIncomeStatementAsync(monthStart, today);

    // Year to date
    var ytdIncome = await _accountingService.GetIncomeStatementAsync(yearStart, today);

    // Current financial position
    var balanceSheet = await _accountingService.GetBalanceSheetAsync(today);

    // Ratios
    var ratios = await _accountingService.CalculateFinancialRatiosAsync(today);

    return new FinancialDashboard
    {
        // Month to Date
        MTDRevenue = monthIncome.TotalRevenue,
        MTDExpenses = monthIncome.TotalOperatingExpenses,
        MTDNetIncome = monthIncome.NetIncome,
        MTDMargin = monthIncome.NetProfitMargin,

        // Year to Date
        YTDRevenue = ytdIncome.TotalRevenue,
        YTDExpenses = ytdIncome.TotalOperatingExpenses,
        YTDNetIncome = ytdIncome.NetIncome,
        YTDMargin = ytdIncome.NetProfitMargin,

        // Current Position
        TotalAssets = balanceSheet.TotalAssets,
        TotalLiabilities = balanceSheet.TotalLiabilities,
        TotalEquity = balanceSheet.TotalEquity,
        CashBalance = balanceSheet.CurrentAssets
            .FirstOrDefault(a => a.AccountNumber == "1000")?.Balance ?? 0,
        ARBalance = balanceSheet.CurrentAssets
            .FirstOrDefault(a => a.AccountNumber == "1100")?.Balance ?? 0,

        // Key Metrics
        CurrentRatio = ratios.CurrentRatio,
        ProfitMargin = ratios.NetProfitMargin,
        DaysReceivable = ratios.DaysReceivableOutstanding,
        DebtRatio = ratios.DebtToAssetRatio
    };
}
```

### Example 4: Tax Preparation Support

```csharp
public async Task<Schedule C_Data> PrepareScheduleCDataAsync(int year)
{
    var yearStart = new DateTime(year, 1, 1);
    var yearEnd = new DateTime(year, 12, 31);

    // Income Statement for the year
    var income = await _accountingService.GetIncomeStatementAsync(yearStart, yearEnd);

    // Detailed deduction breakdown
    var deductions = await _taxService.CalculateDeductionsAsync(yearStart, yearEnd);

    return new ScheduleCData
    {
        // Part I: Income
        GrossReceipts = income.TotalRevenue,

        // Part II: Expenses
        Advertising = deductions.GetBreakdown()
            .FirstOrDefault(kvp => kvp.Key == "Advertising & Marketing").Value.Amount,
        
        CarAndTruckExpenses = deductions.FuelExpenses,
        
        Insurance = deductions.Insurance,
        
        Interest = deductions.InterestExpense,
        
        LegalAndProfessional = deductions.ProfessionalFees,
        
        OfficeExpense = deductions.OfficeExpenses,
        
        Repairs = deductions.MaintenanceAndRepairs,
        
        Supplies = deductions.Supplies + deductions.Tires,
        
        TaxesAndLicenses = deductions.PermitsAndLicenses,
        
        Travel = deductions.PerDiemDeduction,
        
        MealsAndEntertainment = 0, // Per diem covers this
        
        Utilities = 0,
        
        Wages = deductions.Wages,
        
        OtherExpenses = deductions.OtherExpenses,
        
        // Part III: Cost of Goods Sold (usually N/A for trucking)
        // Part IV: Vehicle Information
        // Part V: Other Expenses
        
        TotalExpenses = deductions.TotalDeductions,
        NetProfit = income.NetIncome
    };
}
```

---

## Integration with Other Services

### With Cost Per Mile Service

```csharp
// Calculate all-in cost including taxes
var cpm = await _cpmService.CalculateCPMAsync(...);
var taxes = await _taxService.CalculateQuarterlyTaxAsync(...);

var taxPerMile = taxes.TotalTaxLiability / cpm.TotalMiles / 4;
var trueAllInCPM = cpm.TotalCPM + taxPerMile;

Console.WriteLine($"Operating CPM: {cpm.TotalCPM:C}");
Console.WriteLine($"Tax Per Mile: {taxPerMile:C}");
Console.WriteLine($"All-In CPM: {trueAllInCPM:C}");
```

### With Tax Estimator Service

```csharp
// Verify tax deductions match accounting records
var accountingDeductions = await _accountingService
    .GetIncomeStatementAsync(startDate, endDate);

var taxDeductions = await _taxService
    .CalculateDeductionsAsync(startDate, endDate);

if (Math.Abs(accountingDeductions.TotalOperatingExpenses - taxDeductions.TotalDeductions) > 1.00m)
{
    Console.WriteLine("⚠️ Mismatch between accounting and tax records!");
}
```

---

## Best Practices

### Daily
- [ ] Enter all transactions daily (don't batch)
- [ ] Categorize expenses correctly
- [ ] Attach receipts/documentation

### Weekly
- [ ] Review accounts receivable aging
- [ ] Follow up on overdue invoices
- [ ] Check cash balance

### Monthly
- [ ] Generate income statement
- [ ] Reconcile bank accounts
- [ ] Review trial balance
- [ ] Close the month

### Quarterly
- [ ] Generate all three financial statements
- [ ] Calculate financial ratios
- [ ] File IFTA return
- [ ] Pay estimated taxes
- [ ] Close the quarter

### Annually
- [ ] Generate year-end financials
- [ ] Prepare Schedule C data
- [ ] File tax returns
- [ ] Close the year
- [ ] Set up new year periods

---

## Common Accounting Scenarios

### Scenario 1: Customer Pays Invoice

```csharp
// 1. Create invoice
var invoice = new Invoice { ... };
await _unitOfWork.Invoices.AddAsync(invoice);
await _accountingService.RecordInvoiceAsync(invoice);
// Result: DR AR, CR Revenue

// 2. Customer pays
invoice.Status = InvoiceStatus.Paid;
invoice.PaidDate = DateTime.UtcNow;
await _unitOfWork.SaveAsync();
await _accountingService.RecordPaymentAsync(invoice, invoice.TotalAmount);
// Result: DR Cash, CR AR
```

### Scenario 2: Buy Fuel with Fuel Card

```csharp
// 1. Purchase fuel
var fuel = new FuelEntry { ... };
await _unitOfWork.FuelEntries.AddAsync(fuel);
await _accountingService.RecordFuelPurchaseAsync(fuel, paid: false);
// Result: DR Fuel Expense, CR Fuel Card Payable

// 2. Pay fuel card bill (later)
var payment = new JournalEntry { ... };
// DR Fuel Card Payable, CR Cash
```

### Scenario 3: Truck Payment

```csharp
// Monthly truck payment with principal & interest
var payment = new Expense
{
    Category = ExpenseCategory.TruckPayment,
    Amount = 1500m, // Total payment
    Description = "Truck payment - includes interest"
};

// Split between principal and interest
var interestPortion = 250m;
var principalPortion = 1250m;

// Manual journal entry for proper accounting:
// DR Interest Expense      $250
// DR Truck Loan Payable    $1,250
// CR Cash                  $1,500
```

### Scenario 4: Year-End Financial Review

```csharp
public async Task GenerateYearEndPackageAsync(int year)
{
    var yearStart = new DateTime(year, 1, 1);
    var yearEnd = new DateTime(year, 12, 31);

    Console.WriteLine($"=== {year} YEAR-END FINANCIAL PACKAGE ===");
    Console.WriteLine();

    // 1. Income Statement
    var income = await _accountingService.GetIncomeStatementAsync(yearStart, yearEnd);
    Console.WriteLine($"NET INCOME: {income.NetIncome:C}");
    Console.WriteLine($"Net Margin: {income.NetProfitMargin:N1}%");
    Console.WriteLine();

    // 2. Balance Sheet
    var balanceSheet = await _accountingService.GetBalanceSheetAsync(yearEnd);
    Console.WriteLine($"TOTAL ASSETS: {balanceSheet.TotalAssets:C}");
    Console.WriteLine($"TOTAL LIABILITIES: {balanceSheet.TotalLiabilities:C}");
    Console.WriteLine($"TOTAL EQUITY: {balanceSheet.TotalEquity:C}");
    Console.WriteLine($"Balanced: {(balanceSheet.IsBalanced ? "✅" : "❌")}");
    Console.WriteLine();

    // 3. Cash Flow
    var cashFlow = await _accountingService.GetCashFlowStatementAsync(yearStart, yearEnd);
    Console.WriteLine($"NET CASH CHANGE: {cashFlow.NetChangeInCash:C}");
    Console.WriteLine($"Ending Cash: {cashFlow.EndingCashBalance:C}");
    Console.WriteLine();

    // 4. Key Ratios
    var ratios = await _accountingService.CalculateFinancialRatiosAsync(yearEnd, 12);
    Console.WriteLine("KEY RATIOS:");
    Console.WriteLine($"  Current Ratio: {ratios.CurrentRatio:N2}");
    Console.WriteLine($"  Profit Margin: {ratios.NetProfitMargin:N1}%");
    Console.WriteLine($"  ROE: {ratios.ReturnOnEquity:N1}%");
    Console.WriteLine($"  Days Receivable: {ratios.DaysReceivableOutstanding}");
    Console.WriteLine();

    // 5. Year-over-year
    var comparison = await _accountingService.CompareYearOverYearAsync(yearStart, yearEnd);
    Console.WriteLine("VS PRIOR YEAR:");
    foreach (var comp in comparison)
    {
        Console.WriteLine($"  {comp.Label}: {comp.ChangePercent:+0.0;-0.0}%");
    }
}
```

---

## Troubleshooting

### Issue: Trial Balance Doesn't Balance

**Possible Causes:**
1. Entry posted with debits ≠ credits
2. Manual entry error
3. Deleted journal entry line without deleting pair

**Solution:**
```csharp
var trialBalance = await _accountingService.GetTrialBalanceAsync(DateTime.UtcNow);
var difference = trialBalance.TotalDebits - trialBalance.TotalCredits;

Console.WriteLine($"Out of balance by: {Math.Abs(difference):C}");
// Review recent journal entries for the error
```

### Issue: Balance Sheet Assets ≠ Liabilities + Equity

**Possible Causes:**
1. Forgot to close prior periods
2. Net income not transferred to retained earnings
3. Missing equity transactions

**Solution:** Close all prior periods properly.

### Issue: Cash Account Doesn't Match Bank

**Causes:**
1. Outstanding checks
2. Deposits in transit
3. Bank fees not recorded
4. Errors in recording

**Solution:** Perform bank reconciliation monthly.

---

## API Reference

### AccountingService Methods

```csharp
// Double-Entry Recording
Task<JournalEntry> RecordInvoiceAsync(Invoice invoice)
Task<JournalEntry> RecordPaymentAsync(Invoice invoice, decimal amountPaid)
Task<JournalEntry> RecordExpenseAsync(Expense expense, bool paid = true)
Task<JournalEntry> RecordFuelPurchaseAsync(FuelEntry fuelEntry, bool paid = true)
Task<JournalEntry> RecordMaintenanceAsync(MaintenanceRecord maintenance, bool paid = true)

// Financial Statements
Task<decimal> GetAccountBalanceAsync(int accountId, DateTime asOfDate)
Task<IncomeStatement> GetIncomeStatementAsync(DateTime startDate, DateTime endDate)
Task<BalanceSheet> GetBalanceSheetAsync(DateTime asOfDate)
Task<CashFlowStatement> GetCashFlowStatementAsync(DateTime startDate, DateTime endDate)

// Period Management
Task CreateAccountingPeriodsAsync(int year)
Task<List<FinancialComparison>> CompareYearOverYearAsync(DateTime currentStart, DateTime currentEnd)
Task<List<MonthlyFinancialSummary>> GetMonthlyFinancialSummaryAsync(int year)
Task<JournalEntry> ClosePeriodAsync(int periodId, string closedBy)

// Reconciliation
Task<BankReconciliation> StartBankReconciliationAsync(int accountId, DateTime statementDate, decimal beginBal, decimal endBal)
Task ReconcileTransactionsAsync(List<int> journalEntryLineIds, int reconciliationId)
Task<List<LedgerEntry>> GetUnreconciledTransactionsAsync(int accountId, DateTime throughDate)

// Additional Reports
Task<TrialBalance> GetTrialBalanceAsync(DateTime asOfDate)
Task<AccountLedger> GetAccountLedgerAsync(int accountId, DateTime startDate, DateTime endDate)
Task<FinancialRatios> CalculateFinancialRatiosAsync(DateTime asOfDate, int periodsBack = 12)
```

---

## Summary

The AccountingService provides:
- ✅ Pre-configured Chart of Accounts for trucking
- ✅ Automated double-entry journal entries
- ✅ Three standard financial statements
- ✅ Period management and closing
- ✅ Bank reconciliation
- ✅ Financial ratio analysis
- ✅ Year-over-year comparisons
- ✅ General ledger and trial balance

**Essential for:**
- Proper financial record-keeping
- Tax compliance and preparation
- Business performance analysis
- Loan applications
- Financial planning

**Remember:** Good accounting = good business decisions!
