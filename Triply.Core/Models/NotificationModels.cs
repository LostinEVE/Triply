namespace Triply.Core.Models;

public class NotificationItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public NotificationType Type { get; set; }
    public NotificationSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? ActionUrl { get; set; }
    public object? RelatedData { get; set; }
    public bool IsRead { get; set; }
}

public enum NotificationType
{
    InvoiceOverdue = 0,
    InvoiceDueSoon = 1,
    MaintenanceDue = 2,
    MaintenanceOverdue = 3,
    DocumentExpiring = 4,
    DocumentExpired = 5,
    TaxPaymentDue = 6,
    IFTAFilingDue = 7,
    General = 8
}

public enum NotificationSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Success = 3
}

public class NotificationSummary
{
    public int TotalCount { get; set; }
    public int InvoicesOverdue { get; set; }
    public int InvoicesDueSoon { get; set; }
    public int MaintenanceDue { get; set; }
    public int MaintenanceOverdue { get; set; }
    public int DocumentsExpiring { get; set; }
    public int DocumentsExpired { get; set; }
    public int TaxPaymentsDue { get; set; }
    public int IFTAFilingsDue { get; set; }
    
    public int CriticalCount => InvoicesOverdue + MaintenanceOverdue + DocumentsExpired;
    public int WarningCount => InvoicesDueSoon + MaintenanceDue + DocumentsExpiring + TaxPaymentsDue + IFTAFilingsDue;
}

public class NotificationSettings
{
    public int NotificationSettingsId { get; set; }
    public bool EnableNotifications { get; set; } = true;
    public bool EnableInvoiceAlerts { get; set; } = true;
    public bool EnableMaintenanceAlerts { get; set; } = true;
    public bool EnableDocumentAlerts { get; set; } = true;
    public bool EnableTaxAlerts { get; set; } = true;
    public bool EnableIFTAAlerts { get; set; } = true;
    public int InvoiceDueWarningDays { get; set; } = 7;
    public int MaintenanceDueWarningDays { get; set; } = 7;
    public int DocumentExpiryWarningDays { get; set; } = 30;
    public int CheckIntervalMinutes { get; set; } = 60;
    public bool EnableLocalNotifications { get; set; } = false;
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }
}
