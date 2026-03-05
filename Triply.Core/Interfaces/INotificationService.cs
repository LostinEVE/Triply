using Triply.Core.Models;

namespace Triply.Core.Interfaces;

public interface INotificationService
{
    Task<NotificationSummary> GetNotificationSummaryAsync();
    Task<List<NotificationItem>> GetAllNotificationsAsync();
    Task<List<NotificationItem>> GetUnreadNotificationsAsync();
    Task<NotificationSettings> GetSettingsAsync();
    Task SaveSettingsAsync(NotificationSettings settings);
    Task MarkAsReadAsync(string notificationId);
    Task MarkAllAsReadAsync();
    Task<int> GetUnreadCountAsync();
    Task CheckAndNotifyAsync();
    void StartPeriodicCheck();
    void StopPeriodicCheck();
    event EventHandler<NotificationSummary>? NotificationsUpdated;
}
