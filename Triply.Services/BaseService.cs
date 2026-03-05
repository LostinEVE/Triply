using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public abstract class BaseService
{
    protected readonly IErrorLogger _errorLogger;

    protected BaseService(IErrorLogger errorLogger)
    {
        _errorLogger = errorLogger;
    }

    protected async Task<OperationResult> ExecuteWithErrorHandlingAsync(
        Func<Task> action,
        string context = "")
    {
        try
        {
            await action();
            return OperationResult.SuccessResult();
        }
        catch (Exception ex)
        {
            await _errorLogger.LogErrorAsync(ex, context);
            return OperationResult.ExceptionResult(ex, context);
        }
    }

    protected async Task<OperationResult<T>> ExecuteWithErrorHandlingAsync<T>(
        Func<Task<T>> action,
        string context = "")
    {
        try
        {
            var result = await action();
            return OperationResult<T>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogErrorAsync(ex, context);
            return OperationResult<T>.ExceptionResult(ex, context);
        }
    }

    protected async Task<OperationResult> ValidateAndExecuteAsync<TModel>(
        TModel model,
        FluentValidation.IValidator<TModel> validator,
        Func<Task> action,
        string context = "")
    {
        try
        {
            var validationResult = await validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                await _errorLogger.LogWarningAsync($"Validation failed: {string.Join(", ", errors)}", context);
                return OperationResult.FailureResult("Validation failed. Please check your input.", errors);
            }

            await action();
            return OperationResult.SuccessResult();
        }
        catch (Exception ex)
        {
            await _errorLogger.LogErrorAsync(ex, context);
            return OperationResult.ExceptionResult(ex, context);
        }
    }

    protected async Task<OperationResult<T>> ValidateAndExecuteAsync<TModel, T>(
        TModel model,
        FluentValidation.IValidator<TModel> validator,
        Func<Task<T>> action,
        string context = "")
    {
        try
        {
            var validationResult = await validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                await _errorLogger.LogWarningAsync($"Validation failed: {string.Join(", ", errors)}", context);
                return OperationResult<T>.FailureResult("Validation failed. Please check your input.", errors);
            }

            var result = await action();
            return OperationResult<T>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            await _errorLogger.LogErrorAsync(ex, context);
            return OperationResult<T>.ExceptionResult(ex, context);
        }
    }
}
