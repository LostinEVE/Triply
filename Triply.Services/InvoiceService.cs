using Microsoft.EntityFrameworkCore;
using Triply.Core.Enums;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public class InvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AccountingService? _accountingService;

    public InvoiceService(IUnitOfWork unitOfWork, AccountingService? accountingService = null)
    {
        _unitOfWork = unitOfWork;
        _accountingService = accountingService;
    }

    #region Invoice Creation

    /// <summary>
    /// Creates a new invoice in draft status with auto-generated invoice number
    /// </summary>
    public async Task<Invoice> CreateInvoiceAsync(
        Guid customerId,
        List<InvoiceLineItem> lineItems,
        DateTime? invoiceDate = null,
        string? notes = null)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId)
            ?? throw new Exception($"Customer {customerId} not found");

        var invoice = new Invoice
        {
            InvoiceId = Guid.NewGuid(),
            InvoiceNumber = await GenerateInvoiceNumberAsync(),
            CustomerId = customerId,
            InvoiceDate = invoiceDate ?? DateTime.UtcNow,
            Status = InvoiceStatus.Draft,
            Notes = notes,
            LineItems = lineItems
        };

        // Set due date based on customer payment terms
        invoice.DueDate = CalculateDueDate(invoice.InvoiceDate, customer.PaymentTerms);

        // Calculate totals
        RecalculateTotals(invoice);

        await _unitOfWork.Invoices.AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        return invoice;
    }

    /// <summary>
    /// Creates invoice from selected loads
    /// </summary>
    public async Task<Invoice> CreateInvoiceFromLoadsAsync(
        Guid customerId,
        List<Guid> loadIds,
        DateTime? invoiceDate = null,
        string? notes = null)
    {
        var loads = await _unitOfWork.Loads.GetQueryable()
            .Where(l => loadIds.Contains(l.LoadId))
            .Include(l => l.Customer)
            .ToListAsync();

        if (!loads.Any())
            throw new Exception("No loads found with provided IDs");

        // Verify all loads belong to the same customer
        if (loads.Any(l => l.CustomerId != customerId))
            throw new Exception("All loads must belong to the same customer");

        // Verify loads can be invoiced
        var invalidLoads = loads.Where(l => l.Status != LoadStatus.Delivered).ToList();
        if (invalidLoads.Any())
            throw new Exception($"Loads must be Delivered to be invoiced. Invalid loads: {string.Join(", ", invalidLoads.Select(l => l.LoadNumber))}");

        // Create line items from loads
        var lineItems = new List<InvoiceLineItem>();
        foreach (var load in loads)
        {
            lineItems.Add(new InvoiceLineItem
            {
                LoadId = load.LoadId,
                Description = $"Load {load.LoadNumber}: {load.PickupCity}, {load.PickupState} → {load.DeliveryCity}, {load.DeliveryState}",
                Quantity = load.Miles,
                UnitPrice = load.Rate,
                LineTotal = load.TotalAmount
            });
        }

        // Create invoice
        var invoice = await CreateInvoiceAsync(customerId, lineItems, invoiceDate, notes);

        // Update load statuses
        foreach (var load in loads)
        {
            load.Status = LoadStatus.Invoiced;
        }
        await _unitOfWork.SaveChangesAsync();

        // Record accounting entry if service available
        if (_accountingService != null)
        {
            await _accountingService.RecordInvoiceAsync(invoice);
        }

        return invoice;
    }

    /// <summary>
    /// Generates unique invoice number in format INV-YYYY-#### 
    /// </summary>
    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";

        // Get the highest invoice number for this year
        var lastInvoice = await _unitOfWork.Invoices.GetQueryable()
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastInvoice != null)
        {
            var lastNumberStr = lastInvoice.InvoiceNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    #endregion

    #region Invoice Updates

    /// <summary>
    /// Updates an existing invoice (only Draft or Sent)
    /// </summary>
    public async Task<Invoice> UpdateInvoiceAsync(
        Guid invoiceId,
        List<InvoiceLineItem>? lineItems = null,
        DateTime? dueDate = null,
        string? notes = null)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId)
            ?? throw new Exception($"Invoice {invoiceId} not found");

        // Only allow editing Draft or Sent invoices
        if (invoice.Status != InvoiceStatus.Draft && invoice.Status != InvoiceStatus.Sent)
            throw new Exception($"Cannot edit invoice in {invoice.Status} status");

        // Update line items if provided
        if (lineItems != null)
        {
            // Remove existing line items
            var existingLines = await _unitOfWork.InvoiceLineItems.GetQueryable()
                .Where(li => li.InvoiceId == invoiceId)
                .ToListAsync();

            foreach (var line in existingLines)
            {
                await _unitOfWork.InvoiceLineItems.DeleteAsync(line);
            }

            // Add new line items
            foreach (var line in lineItems)
            {
                line.InvoiceId = invoiceId;
                await _unitOfWork.InvoiceLineItems.AddAsync(line);
            }

            invoice.LineItems = lineItems;
        }

        // Update other fields
        if (dueDate.HasValue)
            invoice.DueDate = dueDate.Value;

        if (notes != null)
            invoice.Notes = notes;

        // Recalculate totals
        RecalculateTotals(invoice);

        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        return invoice;
    }

    /// <summary>
    /// Sends invoice to customer (changes status to Sent)
    /// </summary>
    public async Task<Invoice> SendInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId)
            ?? throw new Exception($"Invoice {invoiceId} not found");

        if (invoice.Status != InvoiceStatus.Draft && invoice.Status != InvoiceStatus.Sent)
            throw new Exception($"Cannot send invoice in {invoice.Status} status");

        invoice.Status = InvoiceStatus.Sent;
        invoice.SentDate = DateTime.UtcNow;

        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        return invoice;
    }

    /// <summary>
    /// Voids an invoice and creates reversing entries
    /// </summary>
    public async Task<Invoice> VoidInvoiceAsync(Guid invoiceId, string reason)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId)
            ?? throw new Exception($"Invoice {invoiceId} not found");

        if (invoice.Status == InvoiceStatus.Paid)
            throw new Exception("Cannot void a paid invoice. Issue credit memo instead.");

        if (invoice.Status == InvoiceStatus.Void)
            throw new Exception("Invoice is already void");

        invoice.Status = InvoiceStatus.Void;
        invoice.Notes = $"{invoice.Notes}\n\nVOIDED: {reason} (on {DateTime.UtcNow:MM/dd/yyyy})";

        // If invoice was sent, update load statuses back to POD Received
        if (invoice.Status == InvoiceStatus.Sent)
        {
            var loadIds = invoice.LineItems.Where(li => li.LoadId.HasValue).Select(li => li.LoadId!.Value);
            foreach (var loadId in loadIds)
            {
                var load = await _unitOfWork.Loads.GetByIdAsync(loadId);
                if (load != null && load.Status == LoadStatus.Invoiced)
                {
                    load.Status = LoadStatus.Delivered;
                    await _unitOfWork.Loads.UpdateAsync(load);
                }
            }
        }

        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        // TODO: Create reversing journal entry if accounting service available

        return invoice;
    }

    #endregion

    #region Payment Recording

    /// <summary>
    /// Records a payment against an invoice
    /// </summary>
    public async Task<Invoice> RecordPaymentAsync(
        Guid invoiceId,
        decimal amount,
        DateTime? paymentDate = null,
        string? paymentMethod = null,
        string? referenceNumber = null)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId)
            ?? throw new Exception($"Invoice {invoiceId} not found");

        if (invoice.Status == InvoiceStatus.Void)
            throw new Exception("Cannot record payment on void invoice");

        if (invoice.Status == InvoiceStatus.Draft)
            throw new Exception("Cannot record payment on draft invoice. Send invoice first.");

        // Validate payment amount
        if (amount <= 0)
            throw new Exception("Payment amount must be greater than zero");

        if (amount > invoice.Balance)
            throw new Exception($"Payment amount ${amount} exceeds outstanding balance ${invoice.Balance}");

        // Record payment
        invoice.AmountPaid += amount;
        invoice.Balance = invoice.TotalAmount - invoice.AmountPaid;

        // Update status
        if (invoice.Balance <= 0.01m) // Account for rounding
        {
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidDate = paymentDate ?? DateTime.UtcNow;
            invoice.Balance = 0; // Ensure exactly zero

            // Update related loads to Paid status
            var loadIds = invoice.LineItems.Where(li => li.LoadId.HasValue).Select(li => li.LoadId!.Value);
            foreach (var loadId in loadIds)
            {
                var load = await _unitOfWork.Loads.GetByIdAsync(loadId);
                if (load != null)
                {
                    load.Status = LoadStatus.Paid;
                    await _unitOfWork.Loads.UpdateAsync(load);
                }
            }
        }
        else if (invoice.AmountPaid > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        // Record accounting entry if service available
        if (_accountingService != null && invoice.Status == InvoiceStatus.Paid)
        {
            await _accountingService.RecordPaymentAsync(invoice, amount);
        }

        return invoice;
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Gets invoices by status
    /// </summary>
    public async Task<List<Invoice>> GetInvoicesByStatusAsync(InvoiceStatus status)
    {
        return await _unitOfWork.Invoices.GetQueryable()
            .Where(i => i.Status == status)
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets overdue invoices
    /// </summary>
    public async Task<List<Invoice>> GetOverdueInvoicesAsync()
    {
        var today = DateTime.UtcNow.Date;

        return await _unitOfWork.Invoices.GetQueryable()
            .Where(i => i.DueDate < today &&
                       i.Balance > 0 &&
                       i.Status != InvoiceStatus.Void &&
                       i.Status != InvoiceStatus.Paid)
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .OrderBy(i => i.DueDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets invoices for a specific customer
    /// </summary>
    public async Task<List<Invoice>> GetCustomerInvoicesAsync(
        Guid customerId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _unitOfWork.Invoices.GetQueryable()
            .Where(i => i.CustomerId == customerId);

        if (startDate.HasValue)
            query = query.Where(i => i.InvoiceDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(i => i.InvoiceDate <= endDate.Value);

        return await query
            .Include(i => i.LineItems)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets invoice summary statistics for a period
    /// </summary>
    public async Task<InvoiceSummary> GetInvoiceSummaryAsync(
        DateTime startDate,
        DateTime endDate)
    {
        var invoices = await _unitOfWork.Invoices.GetQueryable()
            .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;

        return new InvoiceSummary
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalInvoices = invoices.Count,
            TotalInvoiced = invoices.Where(i => i.Status != InvoiceStatus.Void).Sum(i => i.TotalAmount),
            TotalPaid = invoices.Sum(i => i.AmountPaid),
            TotalOutstanding = invoices.Where(i => i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Paid).Sum(i => i.Balance),
            DraftCount = invoices.Count(i => i.Status == InvoiceStatus.Draft),
            SentCount = invoices.Count(i => i.Status == InvoiceStatus.Sent),
            PaidCount = invoices.Count(i => i.Status == InvoiceStatus.Paid),
            OverdueCount = invoices.Count(i => i.DueDate < today && i.Balance > 0 && i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Paid),
            VoidCount = invoices.Count(i => i.Status == InvoiceStatus.Void)
        };
    }

    #endregion

    #region Aging Reports

    /// <summary>
    /// Generates comprehensive invoice aging report
    /// </summary>
    public async Task<InvoiceAgingReport> GetInvoiceAgingReportAsync()
    {
        var today = DateTime.UtcNow.Date;

        var outstandingInvoices = await _unitOfWork.Invoices.GetQueryable()
            .Where(i => i.Balance > 0 &&
                       i.Status != InvoiceStatus.Void &&
                       i.Status != InvoiceStatus.Draft)
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .ToListAsync();

        var report = new InvoiceAgingReport
        {
            ReportDate = today
        };

        // Create aging buckets
        var buckets = new List<InvoiceAgingBucket>
        {
            new() { Label = "Current (0-30 days)", MinDays = 0, MaxDays = 30 },
            new() { Label = "31-60 days", MinDays = 31, MaxDays = 60 },
            new() { Label = "61-90 days", MinDays = 61, MaxDays = 90 },
            new() { Label = "Over 90 days", MinDays = 91, MaxDays = null }
        };

        // Categorize invoices into buckets
        foreach (var invoice in outstandingInvoices)
        {
            var daysOutstanding = (today - invoice.DueDate).Days;

            var bucket = buckets.FirstOrDefault(b =>
                daysOutstanding >= b.MinDays &&
                (!b.MaxDays.HasValue || daysOutstanding <= b.MaxDays));

            if (bucket != null)
            {
                bucket.InvoiceCount++;
                bucket.TotalAmount += invoice.Balance;
                bucket.Invoices.Add(new InvoiceAgingDetail
                {
                    InvoiceId = invoice.InvoiceId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    CustomerName = invoice.Customer?.CompanyName ?? "Unknown",
                    InvoiceDate = invoice.InvoiceDate,
                    DueDate = invoice.DueDate,
                    TotalAmount = invoice.TotalAmount,
                    Balance = invoice.Balance,
                    DaysOutstanding = daysOutstanding
                });
            }
        }

        report.AgingBuckets = buckets;

        // Create customer summaries
        var customerGroups = outstandingInvoices.GroupBy(i => i.CustomerId);
        foreach (var group in customerGroups)
        {
            var customerInvoices = group.ToList();
            var customer = customerInvoices.First().Customer;

            var summary = new CustomerAgingSummary
            {
                CustomerId = group.Key,
                CustomerName = customer?.CompanyName ?? "Unknown",
                InvoiceCount = customerInvoices.Count
            };

            foreach (var invoice in customerInvoices)
            {
                var daysOut = (today - invoice.DueDate).Days;

                if (daysOut <= 30)
                    summary.Current += invoice.Balance;
                else if (daysOut <= 60)
                    summary.Days1to30 += invoice.Balance;
                else if (daysOut <= 90)
                    summary.Days31to60 += invoice.Balance;
                else
                    summary.Over90Days += invoice.Balance;
            }

            summary.AverageDaysOutstanding = (decimal)customerInvoices.Average(i => (today - i.DueDate).Days);

            report.CustomerSummaries.Add(summary);
        }

        // Sort by risk
        report.CustomerSummaries = report.CustomerSummaries
            .OrderByDescending(c => c.Over90Days)
            .ThenByDescending(c => c.TotalOutstanding)
            .ToList();

        return report;
    }

    /// <summary>
    /// Gets aging for a specific customer
    /// </summary>
    public async Task<CustomerAgingSummary> GetCustomerAgingAsync(Guid customerId)
    {
        var report = await GetInvoiceAgingReportAsync();
        return report.CustomerSummaries.FirstOrDefault(c => c.CustomerId == customerId)
            ?? new CustomerAgingSummary { CustomerId = customerId, CustomerName = "No outstanding invoices" };
    }

    #endregion

    #region Reports & Analysis

    /// <summary>
    /// Gets collection performance metrics
    /// </summary>
    public async Task<CollectionMetrics> GetCollectionMetricsAsync(
        DateTime startDate,
        DateTime endDate)
    {
        var invoices = await _unitOfWork.Invoices.GetQueryable()
            .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
            .ToListAsync();

        var paidInvoices = invoices.Where(i => i.Status == InvoiceStatus.Paid).ToList();

        var metrics = new CollectionMetrics
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalInvoiced = invoices.Where(i => i.Status != InvoiceStatus.Void).Sum(i => i.TotalAmount),
            TotalCollected = invoices.Sum(i => i.AmountPaid),
            CollectionRate = 0,
            AverageDaysToPayment = 0
        };

        if (metrics.TotalInvoiced > 0)
            metrics.CollectionRate = (metrics.TotalCollected / metrics.TotalInvoiced) * 100;

        if (paidInvoices.Any())
        {
            metrics.AverageDaysToPayment = paidInvoices
                .Where(i => i.PaidDate.HasValue)
                .Average(i => (i.PaidDate!.Value - i.InvoiceDate).Days);
        }

        return metrics;
    }

    /// <summary>
    /// Gets top customers by revenue
    /// </summary>
    public async Task<List<CustomerRevenueSummary>> GetTopCustomersByRevenueAsync(
        DateTime startDate,
        DateTime endDate,
        int topCount = 10)
    {
        var invoices = await _unitOfWork.Invoices.GetQueryable()
            .Where(i => i.InvoiceDate >= startDate &&
                       i.InvoiceDate <= endDate &&
                       i.Status != InvoiceStatus.Void)
            .Include(i => i.Customer)
            .ToListAsync();

        var customerGroups = invoices.GroupBy(i => i.CustomerId);

        var summaries = customerGroups.Select(g => new CustomerRevenueSummary
        {
            CustomerId = g.Key,
            CustomerName = g.First().Customer?.CompanyName ?? "Unknown",
            InvoiceCount = g.Count(),
            TotalRevenue = g.Sum(i => i.TotalAmount),
            TotalPaid = g.Sum(i => i.AmountPaid),
            OutstandingBalance = g.Sum(i => i.Balance)
        })
        .OrderByDescending(s => s.TotalRevenue)
        .Take(topCount)
        .ToList();

        return summaries;
    }

    /// <summary>
    /// Identifies customers at credit limit
    /// </summary>
    public async Task<List<CustomerCreditAlert>> GetCreditLimitAlertsAsync()
    {
        var customers = await _unitOfWork.Customers.GetQueryable()
            .Where(c => c.IsActive)
            .ToListAsync();

        var alerts = new List<CustomerCreditAlert>();

        foreach (var customer in customers)
        {
            // Check if customer has credit limit set (stored in Notes or custom field)
            // For now, skip customers without credit tracking
            // TODO: Add CreditLimit field to Customer model

            var outstandingBalance = await _unitOfWork.Invoices.GetQueryable()
                .Where(i => i.CustomerId == customer.CustomerId &&
                           i.Balance > 0 &&
                           i.Status != InvoiceStatus.Void)
                .SumAsync(i => i.Balance);

            // Example: alert if outstanding > $50,000
            if (outstandingBalance > 50000)
            {
                alerts.Add(new CustomerCreditAlert
                {
                    CustomerId = customer.CustomerId,
                    CustomerName = customer.CompanyName,
                    CreditLimit = 50000, // Placeholder
                    OutstandingBalance = outstandingBalance,
                    UtilizationPercent = (outstandingBalance / 50000) * 100,
                    AvailableCredit = 50000 - outstandingBalance,
                    AlertLevel = outstandingBalance >= 50000 ? "Critical" : "Warning"
                });
            }
        }

        return alerts.OrderByDescending(a => a.UtilizationPercent).ToList();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Recalculates invoice totals from line items
    /// </summary>
    private void RecalculateTotals(Invoice invoice)
    {
        invoice.Subtotal = invoice.LineItems.Sum(li => li.LineTotal);
        invoice.TaxAmount = 0; // Tax handling can be added here if needed
        invoice.TotalAmount = invoice.Subtotal + invoice.TaxAmount;
        invoice.Balance = invoice.TotalAmount - invoice.AmountPaid;
    }

    /// <summary>
    /// Calculates due date based on payment terms
    /// </summary>
    private DateTime CalculateDueDate(DateTime invoiceDate, string paymentTerms)
    {
        // Parse payment terms (e.g., "Net30", "Net 30", "NET30")
        var terms = paymentTerms.Replace(" ", "").ToUpper();

        var days = terms switch
        {
            "COD" or "CASHONDELIVERY" => 0,
            "NET7" => 7,
            "NET10" => 10,
            "NET15" => 15,
            "NET30" => 30,
            "NET45" => 45,
            "NET60" => 60,
            _ => 30 // Default to Net30
        };

        return invoiceDate.AddDays(days);
    }

    #endregion
}

#region Supporting DTOs

public class CollectionMetrics
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal CollectionRate { get; set; }
    public double AverageDaysToPayment { get; set; }
}

public class CustomerRevenueSummary
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal PercentageOfTotalRevenue { get; set; }
}

public class CustomerCreditAlert
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal UtilizationPercent { get; set; }
    public decimal AvailableCredit { get; set; }
    public string AlertLevel { get; set; } = string.Empty;
}

#endregion
