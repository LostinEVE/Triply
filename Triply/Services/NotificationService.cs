using Microsoft.EntityFrameworkCore;
using Triply.Core.Enums;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private System.Threading.Timer? _periodicTimer;
    private readonly List<NotificationItem> _notifications = new();
    private NotificationSettings? _cachedSettings;

    public event EventHandler<NotificationSummary>? NotificationsUpdated;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<NotificationSummary> GetNotificationSummaryAsync()
    {
        var notifications = await GetAllNotificationsAsync();
        
        return new NotificationSummary
        {
            TotalCount = notifications.Count,
            InvoicesOverdue = notifications.Count(n => n.Type == NotificationType.InvoiceOverdue),
            InvoicesDueSoon = notifications.Count(n => n.Type == NotificationType.InvoiceDueSoon),
            MaintenanceDue = notifications.Count(n => n.Type == NotificationType.MaintenanceDue),
            MaintenanceOverdue = notifications.Count(n => n.Type == NotificationType.MaintenanceOverdue),
            DocumentsExpiring = notifications.Count(n => n.Type == NotificationType.DocumentExpiring),
            DocumentsExpired = notifications.Count(n => n.Type == NotificationType.DocumentExpired),
            TaxPaymentsDue = notifications.Count(n => n.Type == NotificationType.TaxPaymentDue),
            IFTAFilingsDue = notifications.Count(n => n.Type == NotificationType.IFTAFilingDue)
        };
    }

    public Task<List<NotificationItem>> GetAllNotificationsAsync()
    {
        return Task.FromResult(_notifications.OrderByDescending(n => n.CreatedAt).ToList());
    }

    public Task<List<NotificationItem>> GetUnreadNotificationsAsync()
    {
        return Task.FromResult(_notifications.Where(n => !n.IsRead).OrderByDescending(n => n.CreatedAt).ToList());
    }

    public async Task<NotificationSettings> GetSettingsAsync()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        var settings = await _unitOfWork.NotificationSettings.GetQueryable().FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new NotificationSettings();
            await _unitOfWork.NotificationSettings.AddAsync(settings);
            await _unitOfWork.SaveChangesAsync();
        }

        _cachedSettings = settings;
        return settings;
    }

    public async Task SaveSettingsAsync(NotificationSettings settings)
    {
        await _unitOfWork.NotificationSettings.UpdateAsync(settings);
        await _unitOfWork.SaveChangesAsync();
        _cachedSettings = settings;

        // Restart periodic check with new interval
        if (settings.EnableNotifications)
        {
            StopPeriodicCheck();
            StartPeriodicCheck();
        }
        else
        {
            StopPeriodicCheck();
        }
    }

    public Task MarkAsReadAsync(string notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
        }
        return Task.CompletedTask;
    }

    public Task MarkAllAsReadAsync()
    {
        foreach (var notification in _notifications)
        {
            notification.IsRead = true;
        }
        return Task.CompletedTask;
    }

    public Task<int> GetUnreadCountAsync()
    {
        return Task.FromResult(_notifications.Count(n => !n.IsRead));
    }

    public async Task CheckAndNotifyAsync()
    {
        var settings = await GetSettingsAsync();
        if (!settings.EnableNotifications)
            return;

        _notifications.Clear();
        var today = DateTime.Today;

        // Check invoices
        if (settings.EnableInvoiceAlerts)
        {
            await CheckInvoiceNotificationsAsync(today, settings.InvoiceDueWarningDays);
        }

        // Check maintenance
        if (settings.EnableMaintenanceAlerts)
        {
            await CheckMaintenanceNotificationsAsync(today, settings.MaintenanceDueWarningDays);
        }

        // Check documents (this would need document models - placeholder for now)
        if (settings.EnableDocumentAlerts)
        {
            await CheckDocumentNotificationsAsync(today, settings.DocumentExpiryWarningDays);
        }

        // Check tax payments
        if (settings.EnableTaxAlerts)
        {
            await CheckTaxNotificationsAsync(today);
        }

        // Check IFTA
        if (settings.EnableIFTAAlerts)
        {
            await CheckIFTANotificationsAsync(today);
        }

        // Notify subscribers
        var summary = await GetNotificationSummaryAsync();
        NotificationsUpdated?.Invoke(this, summary);
    }

    private async Task CheckInvoiceNotificationsAsync(DateTime today, int warningDays)
    {
        var invoices = await _unitOfWork.Invoices.GetQueryable()
            .Include(i => i.Customer)
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync();

        foreach (var invoice in invoices)
        {
            if (invoice.DueDate < today)
            {
                // Overdue
                var daysOverdue = (today - invoice.DueDate).Days;
                _notifications.Add(new NotificationItem
                {
                    Type = NotificationType.InvoiceOverdue,
                    Severity = NotificationSeverity.Error,
                    Title = $"Invoice {invoice.InvoiceNumber} Overdue",
                    Message = $"{invoice.Customer?.CompanyName}: ${invoice.TotalAmount:N2} - {daysOverdue} days overdue",
                    ActionUrl = $"/invoices/{invoice.InvoiceId}",
                    RelatedData = invoice
                });
            }
            else if (invoice.DueDate <= today.AddDays(warningDays))
            {
                // Due soon
                var daysUntilDue = (invoice.DueDate - today).Days;
                _notifications.Add(new NotificationItem
                {
                    Type = NotificationType.InvoiceDueSoon,
                    Severity = NotificationSeverity.Warning,
                    Title = $"Invoice {invoice.InvoiceNumber} Due Soon",
                    Message = $"{invoice.Customer?.CompanyName}: ${invoice.TotalAmount:N2} - Due in {daysUntilDue} days",
                    ActionUrl = $"/invoices/{invoice.InvoiceId}",
                    RelatedData = invoice
                });
            }
        }
    }

    private async Task CheckMaintenanceNotificationsAsync(DateTime today, int warningDays)
    {
        var maintenanceRecords = await _unitOfWork.MaintenanceRecords.GetQueryable()
            .Include(m => m.Truck)
            .Where(m => m.NextDueDate != null || m.NextDueOdometer != null)
            .ToListAsync();

        foreach (var record in maintenanceRecords)
        {
            var truckId = record.Truck?.TruckId ?? record.TruckId;

            // Check by date
            if (record.NextDueDate.HasValue)
            {
                if (record.NextDueDate.Value < today)
                {
                    var daysOverdue = (today - record.NextDueDate.Value).Days;
                    _notifications.Add(new NotificationItem
                    {
                        Type = NotificationType.MaintenanceOverdue,
                        Severity = NotificationSeverity.Error,
                        Title = $"Maintenance Overdue - {truckId}",
                        Message = $"{record.Type}: {daysOverdue} days overdue",
                        ActionUrl = $"/trucks/{record.TruckId}/maintenance",
                        RelatedData = record
                    });
                }
                else if (record.NextDueDate.Value <= today.AddDays(warningDays))
                {
                    var daysUntilDue = (record.NextDueDate.Value - today).Days;
                    _notifications.Add(new NotificationItem
                    {
                        Type = NotificationType.MaintenanceDue,
                        Severity = NotificationSeverity.Warning,
                        Title = $"Maintenance Due Soon - {truckId}",
                        Message = $"{record.Type}: Due in {daysUntilDue} days",
                        ActionUrl = $"/trucks/{record.TruckId}/maintenance",
                        RelatedData = record
                    });
                }
            }

            // Check by odometer (would need current truck odometer)
            if (record.NextDueOdometer.HasValue && record.Truck != null)
            {
                // Placeholder - would need to track current odometer
                // For now, we'll check if we have recent fuel entries or loads to estimate
                var currentOdometer = await EstimateCurrentOdometerAsync(record.TruckId);
                if (currentOdometer >= record.NextDueOdometer.Value)
                {
                    _notifications.Add(new NotificationItem
                    {
                        Type = NotificationType.MaintenanceOverdue,
                        Severity = NotificationSeverity.Error,
                        Title = $"Maintenance Due - {truckId}",
                        Message = $"{record.Type}: At {currentOdometer} miles (due at {record.NextDueOdometer:N0})",
                        ActionUrl = $"/trucks/{record.TruckId}/maintenance",
                        RelatedData = record
                    });
                }
            }
        }
    }

    private async Task<int> EstimateCurrentOdometerAsync(string truckId)
    {
        // Get the latest odometer reading from maintenance records or fuel entries
        var latestMaintenance = await _unitOfWork.MaintenanceRecords.GetQueryable()
            .Where(m => m.TruckId == truckId)
            .OrderByDescending(m => m.MaintenanceDate)
            .FirstOrDefaultAsync();

        return latestMaintenance?.Odometer ?? 0;
    }

    private Task CheckDocumentNotificationsAsync(DateTime today, int warningDays)
    {
        // Placeholder for document expiration checks
        // Would need Driver licenses, truck registrations, permits, etc.
        // Example structure:
        // - CDL expiration
        // - Medical card expiration
        // - Truck registration
        // - Operating permits
        
        return Task.CompletedTask;
    }

    private Task CheckTaxNotificationsAsync(DateTime today)
    {
        // Check for quarterly estimated tax payments
        var currentQuarter = (today.Month - 1) / 3 + 1;
        var quarterlyDueDates = new Dictionary<int, DateTime>
        {
            { 1, new DateTime(today.Year, 4, 15) },
            { 2, new DateTime(today.Year, 6, 15) },
            { 3, new DateTime(today.Year, 9, 15) },
            { 4, new DateTime(today.Year + 1, 1, 15) }
        };

        foreach (var dueDate in quarterlyDueDates.Values)
        {
            if (dueDate >= today && dueDate <= today.AddDays(14))
            {
                var quarter = quarterlyDueDates.First(kvp => kvp.Value == dueDate).Key;
                _notifications.Add(new NotificationItem
                {
                    Type = NotificationType.TaxPaymentDue,
                    Severity = NotificationSeverity.Warning,
                    Title = $"Q{quarter} Estimated Tax Payment Due",
                    Message = $"Quarterly estimated tax payment due on {dueDate:MM/dd/yyyy}",
                    ActionUrl = "/reports/tax"
                });
            }
        }

        return Task.CompletedTask;
    }

    private Task CheckIFTANotificationsAsync(DateTime today)
    {
        // Check for IFTA quarterly filing deadlines
        var currentQuarter = (today.Month - 1) / 3 + 1;
        var iftaDueDates = new Dictionary<int, DateTime>
        {
            { 1, new DateTime(today.Year, 4, 30) },
            { 2, new DateTime(today.Year, 7, 31) },
            { 3, new DateTime(today.Year, 10, 31) },
            { 4, new DateTime(today.Year + 1, 1, 31) }
        };

        foreach (var kvp in iftaDueDates)
        {
            if (kvp.Value >= today && kvp.Value <= today.AddDays(14))
            {
                _notifications.Add(new NotificationItem
                {
                    Type = NotificationType.IFTAFilingDue,
                    Severity = NotificationSeverity.Warning,
                    Title = $"Q{kvp.Key} IFTA Filing Due",
                    Message = $"IFTA quarterly report due on {kvp.Value:MM/dd/yyyy}",
                    ActionUrl = "/reports/ifta"
                });
            }
        }

        return Task.CompletedTask;
    }

    public void StartPeriodicCheck()
    {
        var task = Task.Run(async () =>
        {
            var settings = await GetSettingsAsync();
            var interval = TimeSpan.FromMinutes(settings.CheckIntervalMinutes);
            
            _periodicTimer = new System.Threading.Timer(
                async _ => await CheckAndNotifyAsync(),
                null,
                TimeSpan.Zero,
                interval);
        });
    }

    public void StopPeriodicCheck()
    {
        _periodicTimer?.Dispose();
        _periodicTimer = null;
    }
}
