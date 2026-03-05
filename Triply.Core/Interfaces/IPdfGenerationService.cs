using Triply.Core.Models;

namespace Triply.Core.Interfaces;

public interface IPdfGenerationService
{
    // Invoice
    Task<byte[]> GenerateInvoiceAsync(Invoice invoice, CompanySettings settings);

    // Financial Reports
    Task<byte[]> GenerateProfitLossReportAsync(
        IncomeStatement incomeStatement, 
        CompanySettings settings,
        IncomeStatement? priorPeriod = null);

    Task<byte[]> GenerateBalanceSheetAsync(
        BalanceSheet balanceSheet,
        CompanySettings settings);

    // Tax Reports
    Task<byte[]> GenerateTaxReportAsync(
        QuarterlyTaxEstimate taxEstimate,
        TaxDeductionSummary deductions,
        CompanySettings settings);

    Task<byte[]> GenerateAnnualTaxSummaryAsync(
        AnnualTaxProjection projection,
        CompanySettings settings);

    // IFTA Report
    Task<byte[]> GenerateIFTAReportAsync(
        IFTAQuarterlyReport iftaReport,
        CompanySettings settings);

    // Maintenance Report
    Task<byte[]> GenerateMaintenanceReportAsync(
        List<MaintenanceRecord> maintenanceRecords,
        Truck truck,
        DateTime startDate,
        DateTime endDate,
        CompanySettings settings);

    // Aging Report
    Task<byte[]> GenerateAgingReportAsync(
        InvoiceAgingReport agingReport,
        CompanySettings settings);

    // Cost Per Mile Report
    Task<byte[]> GenerateCPMReportAsync(
        CPMReport cpmReport,
        CompanySettings settings);
}
