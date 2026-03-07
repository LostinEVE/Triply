using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public class OfflineQueueService : IOfflineQueueService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConnectivityService _connectivityService;

    public event EventHandler<int>? PendingCountChanged;

    public OfflineQueueService(
        IUnitOfWork unitOfWork,
        IConnectivityService connectivityService)
    {
        _unitOfWork = unitOfWork;
        _connectivityService = connectivityService;

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
        // This service only manages the queue
        // Actual processing (like sending emails) is done by the respective services
        // For now, just notify that there are pending operations
        var count = await GetPendingCountAsync();
        PendingCountChanged?.Invoke(this, count);

        await Task.CompletedTask;
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

    private async void OnConnectivityChanged(object? sender, bool isConnected)
    {
        if (isConnected)
        {
            // Just notify that connectivity restored - let other services handle their own queues
            var count = await GetPendingCountAsync();
            PendingCountChanged?.Invoke(this, count);
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
