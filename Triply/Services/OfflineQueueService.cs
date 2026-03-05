using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public class OfflineQueueService : IOfflineQueueService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConnectivityService _connectivityService;
    private readonly IEmailService _emailService;
    private bool _isProcessing;

    public event EventHandler<int>? PendingCountChanged;

    public OfflineQueueService(
        IUnitOfWork unitOfWork,
        IConnectivityService connectivityService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _connectivityService = connectivityService;
        _emailService = emailService;

        // Subscribe to connectivity changes
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
    }

    public async Task QueueEmailAsync(string toEmail, string subject, string body, byte[]? attachment = null, string? attachmentName = null)
    {
        var emailData = new EmailQueueData
        {
            ToEmail = toEmail,
            Subject = subject,
            Body = body,
            Attachment = attachment,
            AttachmentName = attachmentName
        };

        await QueueOperationAsync(OperationType.SendEmail, emailData);
    }

    public async Task QueueOperationAsync(OperationType operationType, object data)
    {
        var operation = new QueuedOperation
        {
            OperationType = operationType,
            OperationData = JsonSerializer.Serialize(data),
            QueuedAt = DateTime.Now,
            Status = OperationStatus.Pending
        };

        await _unitOfWork.QueuedOperations.AddAsync(operation);
        await _unitOfWork.SaveChangesAsync();

        // Notify count changed
        var count = await GetPendingCountAsync();
        PendingCountChanged?.Invoke(this, count);

        // Try to process if online
        if (_connectivityService.IsConnected)
        {
            _ = Task.Run(ProcessQueueAsync);
        }
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await _unitOfWork.QueuedOperations.GetQueryable()
            .CountAsync(q => q.Status == OperationStatus.Pending || q.Status == OperationStatus.Processing);
    }

    public async Task<List<QueuedOperation>> GetPendingOperationsAsync()
    {
        return await _unitOfWork.QueuedOperations.GetQueryable()
            .Where(q => q.Status == OperationStatus.Pending || q.Status == OperationStatus.Processing)
            .OrderBy(q => q.QueuedAt)
            .ToListAsync();
    }

    public async Task ProcessQueueAsync()
    {
        if (_isProcessing || !_connectivityService.IsConnected)
            return;

        try
        {
            _isProcessing = true;

            var pendingOperations = await _unitOfWork.QueuedOperations.GetQueryable()
                .Where(q => q.Status == OperationStatus.Pending)
                .OrderBy(q => q.QueuedAt)
                .ToListAsync();

            foreach (var operation in pendingOperations)
            {
                // Check if still connected
                if (!_connectivityService.IsConnected)
                    break;

                try
                {
                    operation.Status = OperationStatus.Processing;
                    await _unitOfWork.QueuedOperations.UpdateAsync(operation);
                    await _unitOfWork.SaveChangesAsync();

                    await ProcessOperationAsync(operation);

                    operation.Status = OperationStatus.Completed;
                    operation.ProcessedAt = DateTime.Now;
                    await _unitOfWork.QueuedOperations.UpdateAsync(operation);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    operation.RetryCount++;
                    operation.ErrorMessage = ex.Message;

                    if (operation.RetryCount >= operation.MaxRetries)
                    {
                        operation.Status = OperationStatus.Failed;
                    }
                    else
                    {
                        operation.Status = OperationStatus.Pending;
                    }

                    await _unitOfWork.QueuedOperations.UpdateAsync(operation);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            // Notify count changed
            var count = await GetPendingCountAsync();
            PendingCountChanged?.Invoke(this, count);
        }
        finally
        {
            _isProcessing = false;
        }
    }

    public async Task ClearCompletedAsync()
    {
        var completedOperations = await _unitOfWork.QueuedOperations.GetQueryable()
            .Where(q => q.Status == OperationStatus.Completed)
            .ToListAsync();

        foreach (var operation in completedOperations)
        {
            await _unitOfWork.QueuedOperations.DeleteAsync(operation);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task ProcessOperationAsync(QueuedOperation operation)
    {
        switch (operation.OperationType)
        {
            case OperationType.SendEmail:
                await ProcessEmailAsync(operation);
                break;

            case OperationType.CloudBackup:
                // TODO: Implement cloud backup
                break;

            case OperationType.SyncData:
                // TODO: Implement data sync
                break;

            default:
                throw new NotSupportedException($"Operation type {operation.OperationType} is not supported");
        }
    }

    private async Task ProcessEmailAsync(QueuedOperation operation)
    {
        var emailData = JsonSerializer.Deserialize<EmailQueueData>(operation.OperationData);
        if (emailData == null)
            throw new InvalidOperationException("Invalid email data");

        await _emailService.SendEmailAsync(
            emailData.ToEmail,
            emailData.Subject,
            emailData.Body,
            emailData.Attachment,
            emailData.AttachmentName);
    }

    private async void OnConnectivityChanged(object? sender, bool isConnected)
    {
        if (isConnected)
        {
            // Process queue when connectivity is restored
            await ProcessQueueAsync();
        }
    }

    private class EmailQueueData
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public byte[]? Attachment { get; set; }
        public string? AttachmentName { get; set; }
    }
}
