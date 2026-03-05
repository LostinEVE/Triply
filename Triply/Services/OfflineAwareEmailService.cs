using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public class OfflineAwareEmailService : IEmailService
{
    private readonly EmailService _emailService;
    private readonly IConnectivityService _connectivityService;
    private readonly IOfflineQueueService _queueService;

    public OfflineAwareEmailService(
        EmailService emailService,
        IConnectivityService connectivityService,
        IOfflineQueueService queueService)
    {
        _emailService = emailService;
        _connectivityService = connectivityService;
        _queueService = queueService;
    }

    public async Task SendEmailAsync(string to, string subject, string body, byte[]? attachment = null, string? attachmentName = null)
    {
        if (_connectivityService.IsConnected)
        {
            await _emailService.SendEmailAsync(to, subject, body, attachment, attachmentName);
        }
        else
        {
            await _queueService.QueueEmailAsync(to, subject, body, attachment, attachmentName);
        }
    }

    public async Task<EmailSendResult> SendInvoiceEmailAsync(Invoice invoice, CompanySettings settings)
    {
        if (_connectivityService.IsConnected)
        {
            return await _emailService.SendInvoiceEmailAsync(invoice, settings);
        }
        else
        {
            // Queue the invoice email
            await _queueService.QueueEmailAsync(
                invoice.Customer?.ContactEmail ?? "",
                $"Invoice {invoice.InvoiceNumber}",
                $"Please find attached invoice {invoice.InvoiceNumber}");

            return EmailSendResult.CreateSuccess(DateTime.UtcNow);
        }
    }

    // Pass through to base email service for other methods
    public Task<EmailSendResult> SendPaymentReminderAsync(Invoice invoice, CompanySettings settings)
        => _emailService.SendPaymentReminderAsync(invoice, settings);

    public Task<EmailSendResult> SendPaymentReceiptAsync(Invoice invoice, CompanySettings settings)
        => _emailService.SendPaymentReceiptAsync(invoice, settings);

    public Task<EmailSendResult> TestConnectionAsync(CompanySettings settings)
        => _emailService.TestConnectionAsync(settings);

    public Task<int> QueueEmailAsync(string toEmail, string subject, string bodyHtml, byte[]? attachment = null, string? attachmentFileName = null, Guid? invoiceId = null, string? emailType = null, int priority = 5)
        => _emailService.QueueEmailAsync(toEmail, subject, bodyHtml, attachment, attachmentFileName, invoiceId, emailType, priority);

    public Task ProcessQueueAsync(CompanySettings settings)
        => _emailService.ProcessQueueAsync(settings);

    public Task<List<EmailOutboxQueue>> GetPendingEmailsAsync()
        => _emailService.GetPendingEmailsAsync();

    public Task<EmailSendResult> SendQueuedEmailAsync(EmailOutboxQueue queuedEmail, CompanySettings settings)
        => _emailService.SendQueuedEmailAsync(queuedEmail, settings);
}
