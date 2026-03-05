using Triply.Core.Models;

namespace Triply.Core.Interfaces;

public interface IOfflineQueueService
{
    Task QueueEmailAsync(string toEmail, string subject, string body, byte[]? attachment = null, string? attachmentName = null);
    Task QueueOperationAsync(OperationType operationType, object data);
    Task<int> GetPendingCountAsync();
    Task<List<QueuedOperation>> GetPendingOperationsAsync();
    Task ProcessQueueAsync();
    Task ClearCompletedAsync();
    event EventHandler<int>? PendingCountChanged;
}
