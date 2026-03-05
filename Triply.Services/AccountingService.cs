using Microsoft.EntityFrameworkCore;
using Triply.Core.Enums;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public class AccountingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDbContextFactory<Data.TriplyDbContext> _contextFactory;

    public AccountingService(IUnitOfWork unitOfWork, IDbContextFactory<Data.TriplyDbContext> contextFactory)
    {
        _unitOfWork = unitOfWork;
        _contextFactory = contextFactory;
    }

    #region Double-Entry Bookkeeping Helpers

    /// <summary>
    /// Records an invoice (creates AR and Revenue entries)
    /// </summary>
    public async Task<JournalEntry> RecordInvoiceAsync(Invoice invoice)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var entry = new JournalEntry
        {
            EntryNumber = $"INV-{invoice.InvoiceId}",
            EntryDate = invoice.InvoiceDate,
            PostedDate = DateTime.UtcNow,
            Description = $"Invoice {invoice.InvoiceNumber} - {invoice.Customer?.CompanyName}",
            EntryType = JournalEntryType.Standard,
            ReferenceNumber = invoice.InvoiceNumber,
            SourceDocument = "Invoice",
            SourceId = invoice.InvoiceId.ToString(),
            IsPosted = true,
            CreatedBy = "System",
            Lines = new List<JournalEntryLine>()
        };

        // Debit: Accounts Receivable
        var arAccount = await GetAccountByNumberAsync(context, "1100"); // AR
        entry.Lines.Add(new JournalEntryLine
        {
            AccountId = arAccount.AccountId,
            DebitAmount = invoice.TotalAmount,
            CreditAmount = 0,
            Description = $"AR for invoice {invoice.InvoiceNumber}"
        });

        // Credit: Freight Revenue
        var revenueAccount = await GetAccountByNumberAsync(context, "4000"); // Freight Revenue
        entry.Lines.Add(new JournalEntryLine
        {
            AccountId = revenueAccount.AccountId,
            DebitAmount = 0,
            CreditAmount = invoice.TotalAmount,
            Description = $"Revenue from invoice {invoice.InvoiceNumber}"
        });

        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync();

        return entry;
    }

    /// <summary>
    /// Records a payment received (debits Cash, credits AR)
    /// </summary>
    public async Task<JournalEntry> RecordPaymentAsync(Invoice invoice, decimal amountPaid)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var entry = new JournalEntry
        {
            EntryNumber = $"PMT-{invoice.InvoiceId}-{DateTime.UtcNow.Ticks}",
            EntryDate = invoice.PaidDate ?? DateTime.UtcNow,
            PostedDate = DateTime.UtcNow,
            Description = $"Payment for invoice {invoice.InvoiceNumber}",
            EntryType = JournalEntryType.Standard,
            ReferenceNumber = invoice.InvoiceNumber,
            SourceDocument = "Payment",
            SourceId = invoice.InvoiceId.ToString(),
            IsPosted = true,
            CreatedBy = "System",
            Lines = new List<JournalEntryLine>()
        };

        // Debit: Cash
        var cashAccount = await GetAccountByNumberAsync(context, "1000");
        entry.Lines.Add(new JournalEntryLine
        {
            AccountId = cashAccount.AccountId,
            DebitAmount = amountPaid,
            CreditAmount = 0,
            Description = $"Payment received for {invoice.InvoiceNumber}"
        });

        // Credit: Accounts Receivable
        var arAccount = await GetAccountByNumberAsync(context, "1100");
        entry.Lines.Add(new JournalEntryLine
        {
            AccountId = arAccount.AccountId,
            DebitAmount = 0,
            CreditAmount = amountPaid,
            Description = $"Reduce AR for {invoice.InvoiceNumber}"
        });

        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync();

        return entry;
    }

    /// <summary>
    /// Records an expense (debits Expense account, credits Cash or AP)
    /// </summary>
    public async Task<JournalEntry> RecordExpenseAsync(Expense expense, bool paid = true)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var entry = new JournalEntry
        {
            EntryNumber = $"EXP-{expense.ExpenseId}",
            EntryDate = expense.ExpenseDate,
            PostedDate = DateTime.UtcNow,
            Description = expense.Description ?? "Expense",
            EntryType = JournalEntryType.Standard,
            SourceDocument = "Expense",
            SourceId = expense.ExpenseId.ToString(),
            IsPosted = true,
            CreatedBy = "System",
            Lines = new List<JournalEntryLine>()
        };

        // Debit: Appropriate Expense Account
        var expenseAccount = await GetExpenseAccountForCategoryAsync(context, expense.Category);
        entry.Lines.Add(new JournalEntryLine
        {
            AccountId = expenseAccount.AccountId,
            DebitAmount = expense.Amount,
            CreditAmount = 0,
            Description = expense.Description
        });

        // Credit: Cash or Accounts Payable
        var creditAccount = paid 
            ? await GetAccountByNumberAsync(context, "1000") // Cash
            : await GetAccountByNumberAsync(context, "2000"); // Accounts Payable

        entry.Lines.Add(new JournalEntryLine
        {
            AccountId = creditAccount.AccountId,
            DebitAmount = 0,
            CreditAmount = expense.Amount,
            Description = paid ? "Cash payment" : "Accounts payable"
        });

        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync();

        return entry;
    }

    /// <summary>
    /// Records fuel purchase
    /// </summary>
    public async Task<JournalEntry> RecordFuelPurchaseAsync(FuelEntry fuelEntry, bool paid = true)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var entry = new JournalEntry
        {
            EntryNumber = $"FUEL-{fuelEntry.FuelEntryId}",
            EntryDate = fuelEntry.FuelDate,
            PostedDate = DateTime.UtcNow,
            Description = $"Fuel purchase - {fuelEntry.Gallons:N1} gallons",
            EntryType = JournalEntryType.Standard,
            SourceDocument = "FuelEntry",
            IsPosted = true,
            CreatedBy = "System",
            Lines = new List<JournalEntryLine>()
        };

        // Debit: Fuel Expense
        var fuelAccount = fuelEntry.FuelType == FuelType.Diesel
            ? await GetAccountByNumberAsync(context, "5000") // Diesel
            : await GetAccountByNumberAsync(context, "5010"); // DEF

        entry.Lines.Add(new JournalEntryLine
        {
            AccountId = fuelAccount.AccountId,
            DebitAmount = fuelEntry.TotalCost,
            CreditAmount = 0,
            Description = $"{fuelEntry.Gallons:N1} gal @ {fuelEntry.PricePerGallon:C}"
        });

        // Credit: Cash or Fuel Card Payable
        var creditAccount = paid
            ? await GetAccountByNumberAsync(context, "1000") // Cash
            : await GetAccountByNumberAsync(context, "2020"); // Fuel Card Payable

        entry.Lines.Add(new JournalEntryLine
        {
            AccountId = creditAccount.AccountId,
            DebitAmount = 0,
            CreditAmount = fuelEntry.TotalCost,
            Description = paid ? "Cash payment" : "Fuel card charge"
        });

        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync();

        return entry;
    }

    /// <summary>
    /// Records maintenance expense
    /// </summary>
    public async Task<JournalEntry> RecordMaintenanceAsync(MaintenanceRecord maintenance, bool paid = true)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var entry = new JournalEntry
        {
            EntryNumber = $"MAINT-{maintenance.MaintenanceId}",
            EntryDate = maintenance.MaintenanceDate,
            PostedDate = DateTime.UtcNow,
            Description = maintenance.Description ?? "Maintenance",
            EntryType = JournalEntryType.Standard,
            SourceDocument = "Maintenance",
            IsPosted = true,
            CreatedBy = "System",
            Lines = new List<JournalEntryLine>()
        };

        // Debit: Maintenance Expense
        var maintenanceAccount = await GetAccountByNumberAsync(context, "5100"); // Preventive Maintenance
        entry.Lines.Add(new JournalEntryLine
        {
            AccountId = maintenanceAccount.AccountId,
            DebitAmount = maintenance.TotalCost,
            CreditAmount = 0,
            Description = maintenance.Description
        });

        // Credit: Cash or AP
        var creditAccount = paid
            ? await GetAccountByNumberAsync(context, "1000")
            : await GetAccountByNumberAsync(context, "2000");

        entry.Lines.Add(new JournalEntryLine
        {
            AccountId = creditAccount.AccountId,
            DebitAmount = 0,
            CreditAmount = maintenance.TotalCost,
            Description = paid ? "Cash payment" : "Accounts payable"
        });

        context.JournalEntries.Add(entry);
        await context.SaveChangesAsync();

        return entry;
    }

    #endregion

    #region Financial Calculations

    /// <summary>
    /// Gets account balance as of a specific date
    /// </summary>
    public async Task<decimal> GetAccountBalanceAsync(int accountId, DateTime asOfDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var account = await context.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == accountId);

        if (account == null)
            return 0;

        var lines = await context.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Where(l => l.AccountId == accountId &&
                       l.JournalEntry.IsPosted &&
                       !l.JournalEntry.IsReversed &&
                       l.JournalEntry.PostedDate <= asOfDate)
            .ToListAsync();

        var debits = lines.Sum(l => l.DebitAmount);
        var credits = lines.Sum(l => l.CreditAmount);

        // Normal balance depends on account type
        return account.AccountType switch
        {
            AccountType.Asset => debits - credits,
            AccountType.Expense => debits - credits,
            AccountType.Liability => credits - debits,
            AccountType.Equity => credits - debits,
            AccountType.Revenue => credits - debits,
            _ => debits - credits
        };
    }

    /// <summary>
    /// Generates Income Statement (Profit & Loss)
    /// </summary>
    public async Task<IncomeStatement> GetIncomeStatementAsync(DateTime startDate, DateTime endDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var statement = new IncomeStatement
        {
            StartDate = startDate,
            EndDate = endDate
        };

        // Get all accounts with balances
        var accounts = await context.Accounts
            .Where(a => a.IsActive)
            .ToListAsync();

        foreach (var account in accounts)
        {
            var balance = await GetAccountBalanceInPeriodAsync(account.AccountId, startDate, endDate);
            
            if (Math.Abs(balance) < 0.01m) continue;

            var accountBalance = new AccountBalance
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                AccountName = account.AccountName,
                AccountType = account.AccountType,
                Balance = Math.Abs(balance)
            };

            switch (account.AccountType)
            {
                case AccountType.Revenue:
                    if (account.Category == AccountCategory.OperatingRevenue)
                        statement.RevenueAccounts.Add(accountBalance);
                    else
                        statement.OtherIncome.Add(accountBalance);
                    break;

                case AccountType.Expense:
                    if (account.Category == AccountCategory.CostOfGoodsSold)
                        statement.COGSAccounts.Add(accountBalance);
                    else if (account.Category == AccountCategory.OperatingExpense)
                        statement.OperatingExpenses.Add(accountBalance);
                    else
                        statement.OtherExpenses.Add(accountBalance);
                    break;
            }
        }

        return statement;
    }

    /// <summary>
    /// Generates Balance Sheet
    /// </summary>
    public async Task<BalanceSheet> GetBalanceSheetAsync(DateTime asOfDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var balanceSheet = new BalanceSheet
        {
            AsOfDate = asOfDate
        };

        var accounts = await context.Accounts
            .Where(a => a.IsActive)
            .ToListAsync();

        foreach (var account in accounts)
        {
            var balance = await GetAccountBalanceAsync(account.AccountId, asOfDate);
            
            if (Math.Abs(balance) < 0.01m) continue;

            var accountBalance = new AccountBalance
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                AccountName = account.AccountName,
                AccountType = account.AccountType,
                Balance = Math.Abs(balance)
            };

            switch (account.AccountType)
            {
                case AccountType.Asset:
                    if (account.Category == AccountCategory.CurrentAsset)
                        balanceSheet.CurrentAssets.Add(accountBalance);
                    else if (account.Category == AccountCategory.FixedAsset)
                        balanceSheet.FixedAssets.Add(accountBalance);
                    else
                        balanceSheet.OtherAssets.Add(accountBalance);
                    break;

                case AccountType.Liability:
                    if (account.Category == AccountCategory.CurrentLiability)
                        balanceSheet.CurrentLiabilities.Add(accountBalance);
                    else
                        balanceSheet.LongTermLiabilities.Add(accountBalance);
                    break;

                case AccountType.Equity:
                    if (account.Category == AccountCategory.RetainedEarnings)
                        balanceSheet.RetainedEarnings = balance;
                    else
                        balanceSheet.EquityAccounts.Add(accountBalance);
                    break;
            }
        }

        return balanceSheet;
    }

    /// <summary>
    /// Generates Cash Flow Statement
    /// </summary>
    public async Task<CashFlowStatement> GetCashFlowStatementAsync(DateTime startDate, DateTime endDate)
    {
        var statement = new CashFlowStatement
        {
            StartDate = startDate,
            EndDate = endDate
        };

        // Get net income from income statement
        var incomeStatement = await GetIncomeStatementAsync(startDate, endDate);
        statement.NetIncome = incomeStatement.NetIncome;

        // Operating adjustments (non-cash expenses)
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // Depreciation (add back)
        var depreciationAccounts = await context.Accounts
            .Where(a => a.AccountNumber.StartsWith("58")) // Depreciation expenses
            .ToListAsync();

        foreach (var account in depreciationAccounts)
        {
            var balance = await GetAccountBalanceInPeriodAsync(account.AccountId, startDate, endDate);
            if (balance > 0)
            {
                statement.OperatingAdjustments.Add(new CashFlowItem
                {
                    Description = $"Depreciation - {account.AccountName}",
                    Amount = balance,
                    Category = CashFlowCategory.OperatingAdjustment
                });
            }
        }

        // Changes in working capital (simplified)
        var arChange = await GetAccountBalanceChangeAsync(1100, startDate, endDate); // AR
        if (arChange != 0)
        {
            statement.OperatingAdjustments.Add(new CashFlowItem
            {
                Description = "Change in Accounts Receivable",
                Amount = -arChange, // Increase in AR is use of cash
                Category = CashFlowCategory.OperatingAdjustment
            });
        }

        var apChange = await GetAccountBalanceChangeAsync(2000, startDate, endDate); // AP
        if (apChange != 0)
        {
            statement.OperatingAdjustments.Add(new CashFlowItem
            {
                Description = "Change in Accounts Payable",
                Amount = apChange, // Increase in AP is source of cash
                Category = CashFlowCategory.OperatingAdjustment
            });
        }

        // Beginning and ending cash
        statement.BeginningCashBalance = await GetAccountBalanceAsync(1000, startDate.AddDays(-1));

        return statement;
    }

    #endregion

    #region Period Management

    /// <summary>
    /// Creates accounting periods for a year
    /// </summary>
    public async Task CreateAccountingPeriodsAsync(int year)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var periods = new List<AccountingPeriod>();

        // Monthly periods
        for (int month = 1; month <= 12; month++)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            periods.Add(new AccountingPeriod
            {
                Year = year,
                Month = month,
                Quarter = (month - 1) / 3 + 1,
                StartDate = startDate,
                EndDate = endDate,
                IsClosed = false
            });
        }

        // Quarterly periods
        for (int quarter = 1; quarter <= 4; quarter++)
        {
            var startMonth = (quarter - 1) * 3 + 1;
            var startDate = new DateTime(year, startMonth, 1);
            var endDate = startDate.AddMonths(3).AddDays(-1);

            periods.Add(new AccountingPeriod
            {
                Year = year,
                Month = null,
                Quarter = quarter,
                StartDate = startDate,
                EndDate = endDate,
                IsClosed = false
            });
        }

        // Annual period
        periods.Add(new AccountingPeriod
        {
            Year = year,
            Month = null,
            Quarter = null,
            StartDate = new DateTime(year, 1, 1),
            EndDate = new DateTime(year, 12, 31),
            IsClosed = false
        });

        await context.AccountingPeriods.AddRangeAsync(periods);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Compares financial performance year-over-year
    /// </summary>
    public async Task<List<FinancialComparison>> CompareYearOverYearAsync(
        DateTime currentPeriodStart,
        DateTime currentPeriodEnd)
    {
        var priorPeriodStart = currentPeriodStart.AddYears(-1);
        var priorPeriodEnd = currentPeriodEnd.AddYears(-1);

        var currentIncome = await GetIncomeStatementAsync(currentPeriodStart, currentPeriodEnd);
        var priorIncome = await GetIncomeStatementAsync(priorPeriodStart, priorPeriodEnd);

        var comparisons = new List<FinancialComparison>
        {
            new() { Label = "Total Revenue", CurrentPeriod = currentIncome.TotalRevenue, PriorPeriod = priorIncome.TotalRevenue },
            new() { Label = "Gross Profit", CurrentPeriod = currentIncome.GrossProfit, PriorPeriod = priorIncome.GrossProfit },
            new() { Label = "Operating Expenses", CurrentPeriod = currentIncome.TotalOperatingExpenses, PriorPeriod = priorIncome.TotalOperatingExpenses },
            new() { Label = "Operating Income", CurrentPeriod = currentIncome.OperatingIncome, PriorPeriod = priorIncome.OperatingIncome },
            new() { Label = "Net Income", CurrentPeriod = currentIncome.NetIncome, PriorPeriod = priorIncome.NetIncome }
        };

        return comparisons;
    }

    /// <summary>
    /// Gets monthly performance summary
    /// </summary>
    public async Task<List<MonthlyFinancialSummary>> GetMonthlyFinancialSummaryAsync(int year)
    {
        var summaries = new List<MonthlyFinancialSummary>();

        for (int month = 1; month <= 12; month++)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            if (endDate > DateTime.UtcNow)
                break;

            var income = await GetIncomeStatementAsync(startDate, endDate);

            summaries.Add(new MonthlyFinancialSummary
            {
                Year = year,
                Month = month,
                Revenue = income.TotalRevenue,
                Expenses = income.TotalOperatingExpenses + income.TotalCOGS,
                NetIncome = income.NetIncome,
                ProfitMargin = income.NetProfitMargin
            });
        }

        return summaries;
    }

    #endregion

    #region Account Reconciliation

    /// <summary>
    /// Creates bank reconciliation
    /// </summary>
    public async Task<BankReconciliation> StartBankReconciliationAsync(
        int accountId,
        DateTime statementDate,
        decimal statementBeginningBalance,
        decimal statementEndingBalance)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var bookBeginning = await GetAccountBalanceAsync(accountId, statementDate.AddMonths(-1));
        var bookEnding = await GetAccountBalanceAsync(accountId, statementDate);

        var reconciliation = new BankReconciliation
        {
            AccountId = accountId,
            StatementDate = statementDate,
            StatementBeginningBalance = statementBeginningBalance,
            StatementEndingBalance = statementEndingBalance,
            BookBeginningBalance = bookBeginning,
            BookEndingBalance = bookEnding,
            IsReconciled = Math.Abs(statementEndingBalance - bookEnding) < 0.01m
        };

        context.BankReconciliations.Add(reconciliation);
        await context.SaveChangesAsync();

        return reconciliation;
    }

    /// <summary>
    /// Marks journal entry lines as reconciled
    /// </summary>
    public async Task ReconcileTransactionsAsync(List<int> journalEntryLineIds, int reconciliationId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var lines = await context.JournalEntryLines
            .Where(l => journalEntryLineIds.Contains(l.JournalEntryLineId))
            .ToListAsync();

        foreach (var line in lines)
        {
            line.IsReconciled = true;
            line.ReconciledDate = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets unreconciled transactions for an account
    /// </summary>
    public async Task<List<LedgerEntry>> GetUnreconciledTransactionsAsync(
        int accountId,
        DateTime throughDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var lines = await context.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Where(l => l.AccountId == accountId &&
                       !l.IsReconciled &&
                       l.JournalEntry.IsPosted &&
                       l.JournalEntry.PostedDate <= throughDate)
            .OrderBy(l => l.JournalEntry.EntryDate)
            .ToListAsync();

        return lines.Select(l => new LedgerEntry
        {
            Date = l.JournalEntry.EntryDate,
            EntryNumber = l.JournalEntry.EntryNumber,
            Description = l.Description ?? l.JournalEntry.Description,
            DebitAmount = l.DebitAmount,
            CreditAmount = l.CreditAmount,
            IsReconciled = l.IsReconciled
        }).ToList();
    }

    #endregion

    #region Additional Reports

    /// <summary>
    /// Gets trial balance (all accounts with debits/credits)
    /// </summary>
    public async Task<TrialBalance> GetTrialBalanceAsync(DateTime asOfDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var trialBalance = new TrialBalance
        {
            AsOfDate = asOfDate
        };

        var accounts = await context.Accounts
            .Where(a => a.IsActive)
            .OrderBy(a => a.AccountNumber)
            .ToListAsync();

        foreach (var account in accounts)
        {
            var balance = await GetAccountBalanceAsync(account.AccountId, asOfDate);

            if (Math.Abs(balance) < 0.01m) continue;

            var line = new TrialBalanceLine
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                AccountName = account.AccountName,
                AccountType = account.AccountType
            };

            // Debits for asset/expense, credits for liability/equity/revenue
            if (account.AccountType == AccountType.Asset || account.AccountType == AccountType.Expense)
            {
                line.DebitBalance = Math.Abs(balance);
                line.CreditBalance = 0;
            }
            else
            {
                line.DebitBalance = 0;
                line.CreditBalance = Math.Abs(balance);
            }

            trialBalance.Lines.Add(line);
        }

        return trialBalance;
    }

    /// <summary>
    /// Gets general ledger for an account
    /// </summary>
    public async Task<AccountLedger> GetAccountLedgerAsync(
        int accountId,
        DateTime startDate,
        DateTime endDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var account = await context.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == accountId)
            ?? throw new Exception($"Account {accountId} not found");

        var ledger = new AccountLedger
        {
            AccountId = accountId,
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
            StartDate = startDate,
            EndDate = endDate,
            BeginningBalance = await GetAccountBalanceAsync(accountId, startDate.AddDays(-1))
        };

        var lines = await context.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Where(l => l.AccountId == accountId &&
                       l.JournalEntry.IsPosted &&
                       !l.JournalEntry.IsReversed &&
                       l.JournalEntry.PostedDate >= startDate &&
                       l.JournalEntry.PostedDate <= endDate)
            .OrderBy(l => l.JournalEntry.EntryDate)
            .ToListAsync();

        decimal runningBalance = ledger.BeginningBalance;

        foreach (var line in lines)
        {
            var debitAmount = line.DebitAmount;
            var creditAmount = line.CreditAmount;

            // Adjust running balance based on account type
            if (account.AccountType == AccountType.Asset || account.AccountType == AccountType.Expense)
                runningBalance += debitAmount - creditAmount;
            else
                runningBalance += creditAmount - debitAmount;

            ledger.Entries.Add(new LedgerEntry
            {
                Date = line.JournalEntry.EntryDate,
                EntryNumber = line.JournalEntry.EntryNumber,
                Description = line.Description ?? line.JournalEntry.Description,
                DebitAmount = debitAmount,
                CreditAmount = creditAmount,
                RunningBalance = runningBalance,
                IsReconciled = line.IsReconciled
            });
        }

        ledger.EndingBalance = runningBalance;

        return ledger;
    }

    /// <summary>
    /// Calculates financial ratios
    /// </summary>
    public async Task<FinancialRatios> CalculateFinancialRatiosAsync(DateTime asOfDate, int periodsBack = 12)
    {
        var balanceSheet = await GetBalanceSheetAsync(asOfDate);
        var incomeStart = asOfDate.AddMonths(-periodsBack);
        var incomeStatement = await GetIncomeStatementAsync(incomeStart, asOfDate);

        var ratios = new FinancialRatios
        {
            AsOfDate = asOfDate
        };

        // Liquidity
        ratios.CurrentRatio = balanceSheet.TotalCurrentLiabilities > 0
            ? balanceSheet.TotalCurrentAssets / balanceSheet.TotalCurrentLiabilities
            : 0;

        ratios.QuickRatio = balanceSheet.TotalCurrentLiabilities > 0
            ? balanceSheet.TotalCurrentAssets / balanceSheet.TotalCurrentLiabilities
            : 0;

        // Profitability
        ratios.GrossProfitMargin = incomeStatement.GrossProfitMargin;
        ratios.OperatingMargin = incomeStatement.OperatingMargin;
        ratios.NetProfitMargin = incomeStatement.NetProfitMargin;

        ratios.ReturnOnAssets = balanceSheet.TotalAssets > 0
            ? (incomeStatement.NetIncome / balanceSheet.TotalAssets) * 100
            : 0;

        ratios.ReturnOnEquity = balanceSheet.TotalEquity > 0
            ? (incomeStatement.NetIncome / balanceSheet.TotalEquity) * 100
            : 0;

        // Efficiency
        ratios.AssetTurnover = balanceSheet.TotalAssets > 0
            ? incomeStatement.TotalRevenue / balanceSheet.TotalAssets
            : 0;

        // AR Days Outstanding
        var arBalance = balanceSheet.CurrentAssets.FirstOrDefault(a => a.AccountNumber == "1100")?.Balance ?? 0;
        ratios.DaysReceivableOutstanding = incomeStatement.TotalRevenue > 0
            ? (int)((arBalance / (incomeStatement.TotalRevenue / 365)) )
            : 0;

        // Leverage
        ratios.DebtToAssetRatio = balanceSheet.TotalAssets > 0
            ? (balanceSheet.TotalLiabilities / balanceSheet.TotalAssets) * 100
            : 0;

        ratios.DebtToEquityRatio = balanceSheet.TotalEquity > 0
            ? (balanceSheet.TotalLiabilities / balanceSheet.TotalEquity) * 100
            : 0;

        return ratios;
    }

    /// <summary>
    /// Closes an accounting period (transfers net income to retained earnings)
    /// </summary>
    public async Task<JournalEntry> ClosePeriodAsync(int periodId, string closedBy)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var period = await context.AccountingPeriods
            .FirstOrDefaultAsync(p => p.PeriodId == periodId)
            ?? throw new Exception($"Period {periodId} not found");

        if (period.IsClosed)
            throw new Exception("Period is already closed");

        var income = await GetIncomeStatementAsync(period.StartDate, period.EndDate);

        // Create closing entry
        var entry = new JournalEntry
        {
            EntryNumber = $"CLOSE-{period.Year}-{period.Month ?? period.Quarter ?? 0}",
            EntryDate = period.EndDate,
            PostedDate = DateTime.UtcNow,
            Description = $"Closing entry for {period.PeriodName}",
            EntryType = JournalEntryType.Closing,
            IsPosted = true,
            CreatedBy = closedBy,
            Lines = new List<JournalEntryLine>()
        };

        var retainedEarningsAccount = await context.Accounts
            .FirstAsync(a => a.AccountNumber == "3900");

        // Close revenue accounts
        var revenueAccounts = await context.Accounts
            .Where(a => a.AccountType == AccountType.Revenue && a.IsActive)
            .ToListAsync();

        foreach (var account in revenueAccounts)
        {
            var balance = await GetAccountBalanceInPeriodAsync(account.AccountId, period.StartDate, period.EndDate);
            if (Math.Abs(balance) < 0.01m) continue;

            entry.Lines.Add(new JournalEntryLine
            {
                AccountId = account.AccountId,
                DebitAmount = balance,
                CreditAmount = 0,
                Description = "Close revenue to retained earnings"
            });
        }

        // Close expense accounts
        var expenseAccounts = await context.Accounts
            .Where(a => a.AccountType == AccountType.Expense && a.IsActive)
            .ToListAsync();

        foreach (var account in expenseAccounts)
        {
            var balance = await GetAccountBalanceInPeriodAsync(account.AccountId, period.StartDate, period.EndDate);
            if (Math.Abs(balance) < 0.01m) continue;

            entry.Lines.Add(new JournalEntryLine
            {
                AccountId = account.AccountId,
                DebitAmount = 0,
                CreditAmount = balance,
                Description = "Close expense to retained earnings"
            });
        }

        // Net to retained earnings
        entry.Lines.Add(new JournalEntryLine
        {
            AccountId = retainedEarningsAccount.AccountId,
            DebitAmount = income.NetIncome < 0 ? Math.Abs(income.NetIncome) : 0,
            CreditAmount = income.NetIncome > 0 ? income.NetIncome : 0,
            Description = "Transfer net income to retained earnings"
        });

        context.JournalEntries.Add(entry);

        // Mark period as closed
        period.IsClosed = true;
        period.ClosedDate = DateTime.UtcNow;
        period.ClosedBy = closedBy;

        await context.SaveChangesAsync();

        return entry;
    }

    #endregion

    #region Helper Methods

    private async Task<Account> GetAccountByNumberAsync(Data.TriplyDbContext context, string accountNumber)
    {
        return await context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber)
            ?? throw new Exception($"Account {accountNumber} not found");
    }

    private async Task<Account> GetExpenseAccountForCategoryAsync(Data.TriplyDbContext context, ExpenseCategory category)
    {
        var accountNumber = category switch
        {
            ExpenseCategory.Insurance => "5300",
            ExpenseCategory.Permits => "5400",
            ExpenseCategory.Tolls => "5500",
            ExpenseCategory.Parking => "5510",
            ExpenseCategory.Scales => "5520",
            ExpenseCategory.Lumper => "5530",
            ExpenseCategory.TruckPayment => "5700",
            ExpenseCategory.DriverPay => "5200",
            ExpenseCategory.Tires => "5130",
            ExpenseCategory.OfficeExpense => "5600",
            ExpenseCategory.Trailer => "5520", // Trailer expenses
            _ => "5900" // Miscellaneous
        };

        return await GetAccountByNumberAsync(context, accountNumber);
    }

    private async Task<decimal> GetAccountBalanceInPeriodAsync(int accountId, DateTime startDate, DateTime endDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var account = await context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
        if (account == null) return 0;

        var lines = await context.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Where(l => l.AccountId == accountId &&
                       l.JournalEntry.IsPosted &&
                       !l.JournalEntry.IsReversed &&
                       l.JournalEntry.PostedDate >= startDate &&
                       l.JournalEntry.PostedDate <= endDate)
            .ToListAsync();

        var debits = lines.Sum(l => l.DebitAmount);
        var credits = lines.Sum(l => l.CreditAmount);

        return account.AccountType switch
        {
            AccountType.Asset => debits - credits,
            AccountType.Expense => debits - credits,
            _ => credits - debits
        };
    }

    private async Task<decimal> GetAccountBalanceChangeAsync(int accountNumber, DateTime startDate, DateTime endDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var account = await context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber.ToString("0000"));

        if (account == null) return 0;

        var startBalance = await GetAccountBalanceAsync(account.AccountId, startDate.AddDays(-1));
        var endBalance = await GetAccountBalanceAsync(account.AccountId, endDate);

        return endBalance - startBalance;
    }

    #endregion
}
