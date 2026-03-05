using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Triply.Core.Enums;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public class EmailService : IEmailService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfGenerationService _pdfService;

    public EmailService(IUnitOfWork unitOfWork, IPdfGenerationService pdfService)
    {
        _unitOfWork = unitOfWork;
        _pdfService = pdfService;
    }

    #region Basic Email Sending

    public async Task SendEmailAsync(string to, string subject, string body, byte[]? attachment = null, string? attachmentName = null)
    {
        var settings = await GetCompanySettingsAsync();

        if (settings == null || !IsEmailConfigured(settings))
            throw new Exception("Email settings not configured. Please configure SMTP settings in Company Settings.");

        await SendEmailInternalAsync(to, null, subject, body, attachment, attachmentName, settings);
    }

    #endregion

    #region Invoice Emails

    public async Task<EmailSendResult> SendInvoiceEmailAsync(Invoice invoice, CompanySettings settings)
    {
        try
        {
            // Validate
            if (string.IsNullOrEmpty(invoice.Customer?.ContactEmail))
                return EmailSendResult.CreateFailure($"Customer {invoice.Customer?.CompanyName} has no email address");

            if (!IsEmailConfigured(settings))
                return EmailSendResult.CreateFailure("Email settings not configured");

            // Generate PDF
            var pdfBytes = await _pdfService.GenerateInvoiceAsync(invoice, settings);

            // Create email body
            var htmlBody = CreateInvoiceEmailBody(invoice, settings);
            var subject = $"Invoice {invoice.InvoiceNumber} from {settings.CompanyName}";

            // Try to send
            if (await IsOnlineAsync())
            {
                await SendEmailInternalAsync(
                    invoice.Customer.ContactEmail,
                    invoice.Customer.CompanyName,
                    subject,
                    htmlBody,
                    pdfBytes,
                    $"Invoice-{invoice.InvoiceNumber}.pdf",
                    settings
                );

                // Update invoice status
                invoice.Status = InvoiceStatus.Sent;
                invoice.SentDate = DateTime.UtcNow;
                await _unitOfWork.Invoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                return EmailSendResult.CreateSuccess(DateTime.UtcNow);
            }
            else
            {
                // Queue for later
                var queueId = await QueueEmailAsync(
                    invoice.Customer.ContactEmail,
                    subject,
                    htmlBody,
                    pdfBytes,
                    $"Invoice-{invoice.InvoiceNumber}.pdf",
                    invoice.InvoiceId,
                    "Invoice",
                    priority: 3
                );

                return EmailSendResult.CreateSuccess(DateTime.UtcNow, queueId);
            }
        }
        catch (Exception ex)
        {
            return EmailSendResult.CreateFailure($"Failed to send invoice email: {ex.Message}");
        }
    }

    public async Task<EmailSendResult> SendPaymentReminderAsync(Invoice invoice, CompanySettings settings)
    {
        try
        {
            if (string.IsNullOrEmpty(invoice.Customer?.ContactEmail))
                return EmailSendResult.CreateFailure("Customer has no email address");

            if (!IsEmailConfigured(settings))
                return EmailSendResult.CreateFailure("Email settings not configured");

            // Generate PDF
            var pdfBytes = await _pdfService.GenerateInvoiceAsync(invoice, settings);

            // Create reminder email
            var htmlBody = CreatePaymentReminderBody(invoice, settings);
            var subject = $"Payment Reminder: Invoice {invoice.InvoiceNumber} - Due {invoice.DueDate:MM/dd/yyyy}";

            if (await IsOnlineAsync())
            {
                await SendEmailInternalAsync(
                    invoice.Customer.ContactEmail,
                    invoice.Customer.CompanyName,
                    subject,
                    htmlBody,
                    pdfBytes,
                    $"Invoice-{invoice.InvoiceNumber}.pdf",
                    settings
                );

                return EmailSendResult.CreateSuccess(DateTime.UtcNow);
            }
            else
            {
                var queueId = await QueueEmailAsync(
                    invoice.Customer.ContactEmail,
                    subject,
                    htmlBody,
                    pdfBytes,
                    $"Invoice-{invoice.InvoiceNumber}.pdf",
                    invoice.InvoiceId,
                    "Reminder",
                    priority: 2
                );

                return EmailSendResult.CreateSuccess(DateTime.UtcNow, queueId);
            }
        }
        catch (Exception ex)
        {
            return EmailSendResult.CreateFailure($"Failed to send payment reminder: {ex.Message}");
        }
    }

    public async Task<EmailSendResult> SendPaymentReceiptAsync(Invoice invoice, CompanySettings settings)
    {
        try
        {
            if (string.IsNullOrEmpty(invoice.Customer?.ContactEmail))
                return EmailSendResult.CreateFailure("Customer has no email address");

            if (!IsEmailConfigured(settings))
                return EmailSendResult.CreateFailure("Email settings not configured");

            // Generate PDF
            var pdfBytes = await _pdfService.GenerateInvoiceAsync(invoice, settings);

            // Create receipt email
            var htmlBody = CreatePaymentReceiptBody(invoice, settings);
            var subject = $"Payment Received - Invoice {invoice.InvoiceNumber}";

            if (await IsOnlineAsync())
            {
                await SendEmailInternalAsync(
                    invoice.Customer.ContactEmail,
                    invoice.Customer.CompanyName,
                    subject,
                    htmlBody,
                    pdfBytes,
                    $"Receipt-{invoice.InvoiceNumber}.pdf",
                    settings
                );

                return EmailSendResult.CreateSuccess(DateTime.UtcNow);
            }
            else
            {
                var queueId = await QueueEmailAsync(
                    invoice.Customer.ContactEmail,
                    subject,
                    htmlBody,
                    pdfBytes,
                    $"Receipt-{invoice.InvoiceNumber}.pdf",
                    invoice.InvoiceId,
                    "Receipt",
                    priority: 4
                );

                return EmailSendResult.CreateSuccess(DateTime.UtcNow, queueId);
            }
        }
        catch (Exception ex)
        {
            return EmailSendResult.CreateFailure($"Failed to send payment receipt: {ex.Message}");
        }
    }

    #endregion

    #region Email Queue Management

    public async Task<int> QueueEmailAsync(
        string toEmail,
        string subject,
        string bodyHtml,
        byte[]? attachment = null,
        string? attachmentFileName = null,
        Guid? invoiceId = null,
        string? emailType = null,
        int priority = 5)
    {
        var queueItem = new EmailOutboxQueue
        {
            ToEmail = toEmail,
            Subject = subject,
            BodyHtml = bodyHtml,
            AttachmentData = attachment,
            AttachmentFileName = attachmentFileName,
            AttachmentContentType = "application/pdf",
            InvoiceId = invoiceId,
            EmailType = emailType,
            Priority = priority,
            Status = EmailQueueStatus.Pending,
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.EmailOutboxQueue.AddAsync(queueItem);
        await _unitOfWork.SaveChangesAsync();

        return queueItem.QueueId;
    }

    public async Task ProcessQueueAsync(CompanySettings settings)
    {
        if (!IsEmailConfigured(settings))
            return;

        if (!await IsOnlineAsync())
            return;

        var pendingEmails = await GetPendingEmailsAsync();

        foreach (var email in pendingEmails.OrderBy(e => e.Priority).ThenBy(e => e.CreatedDate))
        {
            await SendQueuedEmailAsync(email, settings);
        }
    }

    public async Task<List<EmailOutboxQueue>> GetPendingEmailsAsync()
    {
        return await _unitOfWork.EmailOutboxQueue.GetQueryable()
            .Where(e => e.Status == EmailQueueStatus.Pending && e.AttemptCount < 5)
            .OrderBy(e => e.Priority)
            .ThenBy(e => e.CreatedDate)
            .Take(50)
            .ToListAsync();
    }

    public async Task<EmailSendResult> SendQueuedEmailAsync(EmailOutboxQueue queuedEmail, CompanySettings settings)
    {
        try
        {
            queuedEmail.Status = EmailQueueStatus.Sending;
            queuedEmail.AttemptCount++;
            queuedEmail.LastAttemptDate = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            await SendEmailInternalAsync(
                queuedEmail.ToEmail,
                queuedEmail.ToName,
                queuedEmail.Subject,
                queuedEmail.BodyHtml,
                queuedEmail.AttachmentData,
                queuedEmail.AttachmentFileName,
                settings
            );

            queuedEmail.Status = EmailQueueStatus.Sent;
            queuedEmail.SentDate = DateTime.UtcNow;
            queuedEmail.ErrorMessage = null;
            await _unitOfWork.SaveChangesAsync();

            // Update related invoice if applicable
            if (queuedEmail.InvoiceId.HasValue && queuedEmail.EmailType == "Invoice")
            {
                var invoice = await _unitOfWork.Invoices.GetByIdAsync(queuedEmail.InvoiceId.Value);
                if (invoice != null && invoice.Status == InvoiceStatus.Draft)
                {
                    invoice.Status = InvoiceStatus.Sent;
                    invoice.SentDate = DateTime.UtcNow;
                    await _unitOfWork.Invoices.UpdateAsync(invoice);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            return EmailSendResult.CreateSuccess(DateTime.UtcNow, queuedEmail.QueueId);
        }
        catch (Exception ex)
        {
            queuedEmail.Status = queuedEmail.AttemptCount >= 5 ? EmailQueueStatus.Failed : EmailQueueStatus.Pending;
            queuedEmail.ErrorMessage = ex.Message;
            await _unitOfWork.SaveChangesAsync();

            return EmailSendResult.CreateFailure(ex.Message, queuedEmail.QueueId);
        }
    }

    #endregion

    #region Test & Configuration

    public async Task<EmailSendResult> TestConnectionAsync(CompanySettings settings)
    {
        try
        {
            if (!IsEmailConfigured(settings))
                return EmailSendResult.CreateFailure("Email settings not configured");

            using var client = new SmtpClient();

            await client.ConnectAsync(
                settings.SMTPServer,
                settings.SMTPPort,
                settings.UseSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.None
            );

            if (!string.IsNullOrEmpty(settings.SMTPUsername) && !string.IsNullOrEmpty(settings.SMTPPassword))
            {
                await client.AuthenticateAsync(settings.SMTPUsername, settings.SMTPPassword);
            }

            await client.DisconnectAsync(true);

            return EmailSendResult.CreateSuccess(DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            return EmailSendResult.CreateFailure($"SMTP connection failed: {ex.Message}");
        }
    }

    #endregion

    #region Internal Methods

    private async Task SendEmailInternalAsync(
        string toEmail,
        string? toName,
        string subject,
        string htmlBody,
        byte[]? attachment,
        string? attachmentFileName,
        CompanySettings settings)
    {
        var message = new MimeMessage();

        // From
        message.From.Add(new MailboxAddress(
            settings.FromName ?? settings.CompanyName,
            settings.FromEmail ?? settings.Email
        ));

        // To
        message.To.Add(new MailboxAddress(toName ?? "", toEmail));

        // Subject
        message.Subject = subject;

        // Body
        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        // Attachment
        if (attachment != null && !string.IsNullOrEmpty(attachmentFileName))
        {
            builder.Attachments.Add(attachmentFileName, attachment, ContentType.Parse("application/pdf"));
        }

        message.Body = builder.ToMessageBody();

        // Send
        using var client = new SmtpClient();

        await client.ConnectAsync(
            settings.SMTPServer,
            settings.SMTPPort,
            settings.UseSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.None
        );

        if (!string.IsNullOrEmpty(settings.SMTPUsername) && !string.IsNullOrEmpty(settings.SMTPPassword))
        {
            await client.AuthenticateAsync(settings.SMTPUsername, settings.SMTPPassword);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private string CreateInvoiceEmailBody(Invoice invoice, CompanySettings settings)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2c3e50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .invoice-details {{ background-color: white; padding: 20px; margin: 20px 0; border-left: 4px solid #3498db; }}
        .amount-due {{ font-size: 24px; color: #e74c3c; font-weight: bold; }}
        .due-date {{ font-size: 18px; color: #e67e22; font-weight: bold; }}
        .footer {{ background-color: #ecf0f1; padding: 20px; text-align: center; font-size: 12px; color: #7f8c8d; }}
        .button {{ background-color: #3498db; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0; }}
        table {{ width: 100%; border-collapse: collapse; }}
        td {{ padding: 8px 0; }}
        .label {{ font-weight: bold; color: #7f8c8d; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{settings.CompanyName}</h1>
            <p>{settings.Phone} | {settings.Email}</p>
        </div>

        <div class='content'>
            <h2>Invoice {invoice.InvoiceNumber}</h2>
            <p>Dear {invoice.Customer.CompanyName},</p>
            <p>Thank you for your business! Please find your invoice attached to this email.</p>

            <div class='invoice-details'>
                <table>
                    <tr>
                        <td class='label'>Invoice Number:</td>
                        <td>{invoice.InvoiceNumber}</td>
                    </tr>
                    <tr>
                        <td class='label'>Invoice Date:</td>
                        <td>{invoice.InvoiceDate:MMMM d, yyyy}</td>
                    </tr>
                    <tr>
                        <td class='label'>Due Date:</td>
                        <td class='due-date'>{invoice.DueDate:MMMM d, yyyy}</td>
                    </tr>
                    <tr>
                        <td class='label'>Amount Due:</td>
                        <td class='amount-due'>${invoice.Balance:N2}</td>
                    </tr>
                </table>
            </div>

            <h3>Payment Instructions</h3>
            <p>Please make checks payable to: <strong>{settings.CompanyName}</strong></p>
            <p>Mail to:<br/>
            {settings.Address}<br/>
            {settings.City}, {settings.State} {settings.Zip}</p>

            <p>For questions about this invoice, please contact us at {settings.Email} or {settings.Phone}.</p>

            {(!string.IsNullOrEmpty(settings.EmailSignature) ? $"<div style='margin-top: 30px;'>{settings.EmailSignature}</div>" : "")}
        </div>

        <div class='footer'>
            <p>{settings.CompanyName}</p>
            <p>DOT# {settings.DOTNumber} | MC# {settings.MCNumber}</p>
            <p>{settings.Address}, {settings.City}, {settings.State} {settings.Zip}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string CreatePaymentReminderBody(Invoice invoice, CompanySettings settings)
    {
        var daysOverdue = (DateTime.UtcNow - invoice.DueDate).Days;
        var urgencyMessage = daysOverdue switch
        {
            <= 7 => "We wanted to send you a friendly reminder",
            <= 30 => "This is a reminder",
            _ => "This is an urgent reminder"
        };

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #e67e22; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .invoice-details {{ background-color: white; padding: 20px; margin: 20px 0; border-left: 4px solid #e74c3c; }}
        .amount-due {{ font-size: 28px; color: #c0392b; font-weight: bold; }}
        .overdue {{ font-size: 16px; color: #e74c3c; font-weight: bold; }}
        .footer {{ background-color: #ecf0f1; padding: 20px; text-align: center; font-size: 12px; color: #7f8c8d; }}
        table {{ width: 100%; border-collapse: collapse; }}
        td {{ padding: 8px 0; }}
        .label {{ font-weight: bold; color: #7f8c8d; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⚠️ Payment Reminder</h1>
            <p>{settings.CompanyName}</p>
        </div>

        <div class='content'>
            <h2>Invoice {invoice.InvoiceNumber}</h2>
            <p>Dear {invoice.Customer.CompanyName},</p>
            <p>{urgencyMessage} that we have not yet received payment for the following invoice:</p>

            <div class='invoice-details'>
                <table>
                    <tr>
                        <td class='label'>Invoice Number:</td>
                        <td>{invoice.InvoiceNumber}</td>
                    </tr>
                    <tr>
                        <td class='label'>Invoice Date:</td>
                        <td>{invoice.InvoiceDate:MMMM d, yyyy}</td>
                    </tr>
                    <tr>
                        <td class='label'>Due Date:</td>
                        <td class='overdue'>{invoice.DueDate:MMMM d, yyyy} ({daysOverdue} days overdue)</td>
                    </tr>
                    <tr>
                        <td class='label'>Amount Due:</td>
                        <td class='amount-due'>${invoice.Balance:N2}</td>
                    </tr>
                </table>
            </div>

            <p>If you have already sent payment, please disregard this notice. Otherwise, please submit payment at your earliest convenience.</p>

            <h3>Payment Instructions</h3>
            <p>Please make checks payable to: <strong>{settings.CompanyName}</strong></p>
            <p>Mail to:<br/>
            {settings.Address}<br/>
            {settings.City}, {settings.State} {settings.Zip}</p>

            <p>If you have any questions or concerns about this invoice, please contact us immediately at {settings.Email} or {settings.Phone}.</p>

            <p>Thank you for your prompt attention to this matter.</p>

            {(!string.IsNullOrEmpty(settings.EmailSignature) ? $"<div style='margin-top: 30px;'>{settings.EmailSignature}</div>" : "")}
        </div>

        <div class='footer'>
            <p>{settings.CompanyName}</p>
            <p>DOT# {settings.DOTNumber} | MC# {settings.MCNumber}</p>
            <p>{settings.Address}, {settings.City}, {settings.State} {settings.Zip}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string CreatePaymentReceiptBody(Invoice invoice, CompanySettings settings)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #27ae60; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .payment-details {{ background-color: white; padding: 20px; margin: 20px 0; border-left: 4px solid #27ae60; }}
        .amount-paid {{ font-size: 28px; color: #27ae60; font-weight: bold; }}
        .footer {{ background-color: #ecf0f1; padding: 20px; text-align: center; font-size: 12px; color: #7f8c8d; }}
        table {{ width: 100%; border-collapse: collapse; }}
        td {{ padding: 8px 0; }}
        .label {{ font-weight: bold; color: #7f8c8d; }}
        .checkmark {{ font-size: 48px; color: #27ae60; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='checkmark'>✓</div>
            <h1>Payment Received</h1>
            <p>{settings.CompanyName}</p>
        </div>

        <div class='content'>
            <h2>Thank You!</h2>
            <p>Dear {invoice.Customer.CompanyName},</p>
            <p>We have received your payment. Thank you for your business!</p>

            <div class='payment-details'>
                <table>
                    <tr>
                        <td class='label'>Invoice Number:</td>
                        <td>{invoice.InvoiceNumber}</td>
                    </tr>
                    <tr>
                        <td class='label'>Payment Date:</td>
                        <td>{invoice.PaidDate?.ToString("MMMM d, yyyy") ?? DateTime.UtcNow.ToString("MMMM d, yyyy")}</td>
                    </tr>
                    <tr>
                        <td class='label'>Amount Paid:</td>
                        <td class='amount-paid'>${invoice.AmountPaid:N2}</td>
                    </tr>
                    <tr>
                        <td class='label'>Payment Status:</td>
                        <td><strong style='color: #27ae60;'>PAID IN FULL</strong></td>
                    </tr>
                </table>
            </div>

            <p>Please keep this email as your receipt for payment. A copy of the invoice is attached for your records.</p>

            <p>We appreciate your business and look forward to serving you again!</p>

            <p>If you have any questions, please contact us at {settings.Email} or {settings.Phone}.</p>

            {(!string.IsNullOrEmpty(settings.EmailSignature) ? $"<div style='margin-top: 30px;'>{settings.EmailSignature}</div>" : "")}
        </div>

        <div class='footer'>
            <p>{settings.CompanyName}</p>
            <p>DOT# {settings.DOTNumber} | MC# {settings.MCNumber}</p>
            <p>{settings.Address}, {settings.City}, {settings.State} {settings.Zip}</p>
        </div>
    </div>
</body>
</html>";
    }

    private bool IsEmailConfigured(CompanySettings settings)
    {
        return !string.IsNullOrEmpty(settings.SMTPServer) &&
               !string.IsNullOrEmpty(settings.FromEmail) &&
               settings.SMTPPort > 0;
    }

    private async Task<CompanySettings?> GetCompanySettingsAsync()
    {
        return await _unitOfWork.CompanySettings.GetQueryable().FirstOrDefaultAsync();
    }

    private async Task<bool> IsOnlineAsync()
    {
        // Simple connectivity check
        try
        {
            using var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            var response = await client.GetAsync("https://www.google.com");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
