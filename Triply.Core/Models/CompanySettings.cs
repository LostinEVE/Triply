namespace Triply.Core.Models;

public class CompanySettings
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? DBA { get; set; }
    public string? DOTNumber { get; set; }
    public string? MCNumber { get; set; }
    public string? EIN { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public byte[]? LogoImage { get; set; }
    public string InvoicePrefix { get; set; } = "INV";
    public int NextInvoiceNumber { get; set; } = 1;
    public string DefaultPaymentTerms { get; set; } = "Net30";
    public decimal FederalTaxRate { get; set; }
    public decimal StateTaxRate { get; set; }
    public decimal SelfEmploymentTaxRate { get; set; }
    public int FiscalYearStart { get; set; } = 1;

    // Email Settings
    public string? SMTPServer { get; set; }
    public int SMTPPort { get; set; } = 587;
    public string? SMTPUsername { get; set; }
    public string? SMTPPassword { get; set; }
    public bool UseSSL { get; set; } = true;
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? EmailSignature { get; set; }
    public bool EnableEmailNotifications { get; set; } = true;
}
