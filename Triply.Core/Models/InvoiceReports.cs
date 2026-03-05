namespace Triply.Core.Models;

public class InvoiceAgingReport
{
    public DateTime ReportDate { get; set; }
    public List<InvoiceAgingBucket> AgingBuckets { get; set; } = new();
    public List<CustomerAgingSummary> CustomerSummaries { get; set; } = new();
    
    // Totals
    public decimal TotalOutstanding => AgingBuckets.Sum(b => b.TotalAmount);
    public int TotalInvoices => AgingBuckets.Sum(b => b.InvoiceCount);
    public decimal AverageDaysOutstanding => CustomerSummaries.Any() 
        ? CustomerSummaries.Average(c => c.AverageDaysOutstanding) 
        : 0;
}

public class InvoiceAgingBucket
{
    public string Label { get; set; } = string.Empty;
    public int MinDays { get; set; }
    public int? MaxDays { get; set; }
    public int InvoiceCount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<InvoiceAgingDetail> Invoices { get; set; } = new();
}

public class InvoiceAgingDetail
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Balance { get; set; }
    public int DaysOutstanding { get; set; }
    public bool IsOverdue => DaysOutstanding > 0;
}

public class CustomerAgingSummary
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Current { get; set; }
    public decimal Days1to30 { get; set; }
    public decimal Days31to60 { get; set; }
    public decimal Days61to90 { get; set; }
    public decimal Over90Days { get; set; }
    public decimal TotalOutstanding => Current + Days1to30 + Days31to60 + Days61to90 + Over90Days;
    public int InvoiceCount { get; set; }
    public decimal AverageDaysOutstanding { get; set; }
    public string RiskLevel
    {
        get
        {
            if (Over90Days > TotalOutstanding * 0.5m) return "High Risk";
            if (Days61to90 + Over90Days > TotalOutstanding * 0.3m) return "Medium Risk";
            return "Low Risk";
        }
    }
}

public class InvoiceSummary
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int DraftCount { get; set; }
    public int SentCount { get; set; }
    public int PaidCount { get; set; }
    public int OverdueCount { get; set; }
    public int VoidCount { get; set; }
    public decimal AverageInvoiceAmount => TotalInvoices > 0 ? TotalInvoiced / TotalInvoices : 0;
    public decimal CollectionRate => TotalInvoiced > 0 ? (TotalPaid / TotalInvoiced) * 100 : 0;
}

public class PaymentHistory
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public List<PaymentRecord> Payments { get; set; } = new();
    public decimal TotalPaid => Payments.Sum(p => p.Amount);
}

public class PaymentRecord
{
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
