namespace Triply.Core.Models;

public class Account
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public AccountCategory Category { get; set; }
    public string? Description { get; set; }
    public int? ParentAccountId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystemAccount { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public Account? ParentAccount { get; set; }
    public ICollection<Account> SubAccounts { get; set; } = new List<Account>();
    public ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
}

public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}

public enum AccountCategory
{
    // Assets
    CurrentAsset = 101,
    FixedAsset = 102,
    OtherAsset = 103,
    
    // Liabilities
    CurrentLiability = 201,
    LongTermLiability = 202,
    
    // Equity
    OwnersEquity = 301,
    RetainedEarnings = 302,
    
    // Revenue
    OperatingRevenue = 401,
    OtherRevenue = 402,
    
    // Expenses
    CostOfGoodsSold = 501,
    OperatingExpense = 502,
    OtherExpense = 503
}

public class JournalEntry
{
    public int JournalEntryId { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public DateTime PostedDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public JournalEntryType EntryType { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? SourceDocument { get; set; }
    public string? SourceId { get; set; } // Support for Guid IDs
    public bool IsPosted { get; set; }
    public bool IsReversed { get; set; }
    public int? ReversedByEntryId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    
    // Calculated
    public decimal TotalDebits => Lines.Sum(l => l.DebitAmount);
    public decimal TotalCredits => Lines.Sum(l => l.CreditAmount);
    public bool IsBalanced => TotalDebits == TotalCredits;
}

public enum JournalEntryType
{
    Standard = 0,
    Opening = 1,
    Adjusting = 2,
    Closing = 3,
    Reversing = 4
}

public class JournalEntryLine
{
    public int JournalEntryLineId { get; set; }
    public int JournalEntryId { get; set; }
    public int AccountId { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? Description { get; set; }
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledDate { get; set; }
    
    // Navigation
    public JournalEntry JournalEntry { get; set; } = null!;
    public Account Account { get; set; } = null!;
}

public class AccountingPeriod
{
    public int PeriodId { get; set; }
    public int Year { get; set; }
    public int? Month { get; set; } // Null for annual
    public int? Quarter { get; set; } // Null for monthly
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? ClosedBy { get; set; }
    
    public string PeriodName => Month.HasValue 
        ? $"{GetMonthName(Month.Value)} {Year}" 
        : Quarter.HasValue 
            ? $"Q{Quarter} {Year}" 
            : $"FY {Year}";
    
    private static string GetMonthName(int month) => new DateTime(2000, month, 1).ToString("MMMM");
}

public class BankReconciliation
{
    public int ReconciliationId { get; set; }
    public int AccountId { get; set; }
    public DateTime StatementDate { get; set; }
    public decimal StatementBeginningBalance { get; set; }
    public decimal StatementEndingBalance { get; set; }
    public decimal BookBeginningBalance { get; set; }
    public decimal BookEndingBalance { get; set; }
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledDate { get; set; }
    public string? Notes { get; set; }
    
    // Navigation
    public Account Account { get; set; } = null!;
    
    // Calculated
    public decimal Difference => StatementEndingBalance - BookEndingBalance;
}
