using FluentValidation;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

/// <summary>
/// Enhanced invoice service wrapper with comprehensive error handling, validation, and notifications
/// </summary>
public class InvoiceServiceEnhanced : BaseService
{
    private readonly InvoiceService _invoiceService;
    private readonly IValidator<Invoice> _invoiceValidator;
    private readonly IToastService _toastService;

    public InvoiceServiceEnhanced(
        InvoiceService invoiceService,
        IValidator<Invoice> invoiceValidator,
        IErrorLogger errorLogger,
        IToastService toastService) : base(errorLogger)
    {
        _invoiceService = invoiceService;
        _invoiceValidator = invoiceValidator;
        _toastService = toastService;
    }

    public async Task<OperationResult<Invoice>> CreateInvoiceAsync(
        Guid customerId,
        List<InvoiceLineItem> lineItems,
        DateTime? invoiceDate = null,
        string? notes = null)
    {
        var result = await ExecuteWithErrorHandlingAsync(
            async () => await _invoiceService.CreateInvoiceAsync(customerId, lineItems, invoiceDate, notes),
            "creating invoice");

        if (result.Success && result.Data != null)
        {
            // Validate the created invoice
            var validationResult = await _invoiceValidator.ValidateAsync(result.Data);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                _toastService.ShowWarning("Invoice created but has validation warnings");
                return OperationResult<Invoice>.FailureResult("Invoice validation failed", errors);
            }

            _toastService.ShowSuccess($"Invoice {result.Data.InvoiceNumber} created successfully");
            await _errorLogger.LogInfoAsync($"Invoice {result.Data.InvoiceNumber} created", "InvoiceService");
        }
        else
        {
            _toastService.ShowError(result.Message);
        }

        return result;
    }

    public async Task<OperationResult<Invoice>> CreateInvoiceFromLoadsAsync(
        Guid customerId,
        List<Guid> loadIds,
        DateTime? invoiceDate = null,
        string? notes = null)
    {
        var result = await ExecuteWithErrorHandlingAsync(
            async () => await _invoiceService.CreateInvoiceFromLoadsAsync(customerId, loadIds, invoiceDate, notes),
            "creating invoice from loads");

        if (result.Success && result.Data != null)
        {
            _toastService.ShowSuccess($"Invoice {result.Data.InvoiceNumber} created successfully from {loadIds.Count} load(s)");
            await _errorLogger.LogInfoAsync($"Invoice {result.Data.InvoiceNumber} created from {loadIds.Count} loads", "InvoiceService");
        }
        else
        {
            _toastService.ShowError(result.Message);
        }

        return result;
    }

    public async Task<OperationResult<Invoice>> UpdateInvoiceAsync(
        Guid invoiceId,
        List<InvoiceLineItem>? lineItems = null,
        DateTime? dueDate = null,
        string? notes = null)
    {
        var result = await ExecuteWithErrorHandlingAsync(
            async () => await _invoiceService.UpdateInvoiceAsync(invoiceId, lineItems, dueDate, notes),
            "updating invoice");

        if (result.Success && result.Data != null)
        {
            _toastService.ShowSuccess($"Invoice {result.Data.InvoiceNumber} updated successfully");
            await _errorLogger.LogInfoAsync($"Invoice {result.Data.InvoiceNumber} updated", "InvoiceService");
        }
        else
        {
            _toastService.ShowError(result.Message);
        }

        return result;
    }

    public async Task<OperationResult<Invoice>> SendInvoiceAsync(Guid invoiceId)
    {
        var result = await ExecuteWithErrorHandlingAsync(
            async () => await _invoiceService.SendInvoiceAsync(invoiceId),
            "sending invoice");

        if (result.Success && result.Data != null)
        {
            _toastService.ShowSuccess($"Invoice {result.Data.InvoiceNumber} sent successfully");
            await _errorLogger.LogInfoAsync($"Invoice {result.Data.InvoiceNumber} sent", "InvoiceService");
        }
        else
        {
            _toastService.ShowError(result.Message);
        }

        return result;
    }

    public async Task<OperationResult<Invoice>> VoidInvoiceAsync(Guid invoiceId, string reason)
    {
        // Note: Confirmation dialog should be handled in the UI layer
        var result = await ExecuteWithErrorHandlingAsync(
            async () => await _invoiceService.VoidInvoiceAsync(invoiceId, reason),
            "voiding invoice");

        if (result.Success && result.Data != null)
        {
            _toastService.ShowSuccess($"Invoice {result.Data.InvoiceNumber} voided successfully");
            await _errorLogger.LogInfoAsync($"Invoice {result.Data.InvoiceNumber} voided: {reason}", "InvoiceService");
        }
        else
        {
            _toastService.ShowError(result.Message);
        }

        return result;
    }

    public async Task<OperationResult<Invoice>> RecordPaymentAsync(
        Guid invoiceId,
        decimal amount,
        DateTime? paymentDate = null,
        string? paymentMethod = null,
        string? referenceNumber = null)
    {
        var result = await ExecuteWithErrorHandlingAsync(
            async () => await _invoiceService.RecordPaymentAsync(invoiceId, amount, paymentDate, paymentMethod, referenceNumber),
            "recording payment");

        if (result.Success && result.Data != null)
        {
            _toastService.ShowSuccess($"Payment of ${amount:N2} recorded successfully for invoice {result.Data.InvoiceNumber}");
            await _errorLogger.LogInfoAsync($"Payment of ${amount:N2} recorded for invoice {result.Data.InvoiceNumber}", "InvoiceService");
        }
        else
        {
            _toastService.ShowError(result.Message);
        }

        return result;
    }

    public async Task<OperationResult<string>> GenerateInvoiceNumberAsync()
    {
        return await ExecuteWithErrorHandlingAsync(
            async () => await _invoiceService.GenerateInvoiceNumberAsync(),
            "generating invoice number");
    }
}
