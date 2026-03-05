using Triply.Core.Models;

namespace Triply.Core.Interfaces;

public interface IEmailService
{
    // Basic email sending
    Task SendEmailAsync(string to, string subject, string body, byte[]? attachment = null, string? attachmentName = null);

    // Invoice-specific emails
    Task<EmailSendResult> SendInvoiceEmailAsync(Invoice invoice, CompanySettings settings);
    Task<EmailSendResult> SendPaymentReminderAsync(Invoice invoice, CompanySettings settings);
    Task<EmailSendResult> SendPaymentReceiptAsync(Invoice invoice, CompanySettings settings);

    // Test & configuration
    Task<EmailSendResult> TestConnectionAsync(CompanySettings settings);

    // Queue management
    Task<int> QueueEmailAsync(
        string toEmail,
        string subject,
        string bodyHtml,
        byte[]? attachment = null,
        string? attachmentFileName = null,
        Guid? invoiceId = null,
        string? emailType = null,
        int priority = 5);

    Task ProcessQueueAsync(CompanySettings settings);
    Task<List<EmailOutboxQueue>> GetPendingEmailsAsync();
    Task<EmailSendResult> SendQueuedEmailAsync(EmailOutboxQueue queuedEmail, CompanySettings settings);
}
