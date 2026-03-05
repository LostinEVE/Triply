namespace Triply.Core.Models;

public class EmailOutboxQueue
{
    public int QueueId { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public byte[]? AttachmentData { get; set; }
    public string? AttachmentFileName { get; set; }
    public string? AttachmentContentType { get; set; }
    
    // Related entities
    public Guid? InvoiceId { get; set; }
    public string? EmailType { get; set; } // "Invoice", "Reminder", "Receipt", etc.
    
    // Status tracking
    public EmailQueueStatus Status { get; set; } = EmailQueueStatus.Pending;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? SentDate { get; set; }
    public int AttemptCount { get; set; } = 0;
    public DateTime? LastAttemptDate { get; set; }
    public string? ErrorMessage { get; set; }
    public int Priority { get; set; } = 5; // 1 = highest, 10 = lowest
}

public enum EmailQueueStatus
{
    Pending = 0,
    Sending = 1,
    Sent = 2,
    Failed = 3,
    Cancelled = 4
}

public class EmailSendResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentDate { get; set; }
    public int QueueId { get; set; }
    
    public static EmailSendResult CreateSuccess(DateTime sentDate, int queueId = 0)
    {
        return new EmailSendResult
        {
            Success = true,
            SentDate = sentDate,
            QueueId = queueId
        };
    }
    
    public static EmailSendResult CreateFailure(string errorMessage, int queueId = 0)
    {
        return new EmailSendResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            QueueId = queueId
        };
    }
}
