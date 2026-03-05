namespace Triply.Core.Models;

public class QueuedOperation
{
    public int QueuedOperationId { get; set; }
    public OperationType OperationType { get; set; }
    public string OperationData { get; set; } = string.Empty; // JSON serialized data
    public DateTime QueuedAt { get; set; } = DateTime.Now;
    public DateTime? ProcessedAt { get; set; }
    public OperationStatus Status { get; set; } = OperationStatus.Pending;
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
}

public enum OperationType
{
    SendEmail = 0,
    CloudBackup = 1,
    SyncData = 2
}

public enum OperationStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
