namespace Triply.Core.Models;

public class OperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public Exception? Exception { get; set; }

    public static OperationResult SuccessResult(string message = "Operation completed successfully")
    {
        return new OperationResult
        {
            Success = true,
            Message = message
        };
    }

    public static OperationResult FailureResult(string message, List<string>? errors = null)
    {
        return new OperationResult
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static OperationResult ExceptionResult(Exception ex, string context = "")
    {
        return new OperationResult
        {
            Success = false,
            Message = GetUserFriendlyErrorMessage(ex, context),
            Exception = ex,
            Errors = new List<string> { ex.Message }
        };
    }

    private static string GetUserFriendlyErrorMessage(Exception ex, string context)
    {
        var baseMessage = string.IsNullOrEmpty(context) ? "An error occurred" : $"An error occurred while {context}";

        return ex switch
        {
            UnauthorizedAccessException => $"{baseMessage}: You don't have permission to perform this action.",
            InvalidOperationException => $"{baseMessage}: The operation could not be completed. Please check your data and try again.",
            ArgumentException => $"{baseMessage}: Invalid data provided. Please check your input.",
            System.IO.IOException => $"{baseMessage}: A file system error occurred. Please check file permissions and disk space.",
            TimeoutException => $"{baseMessage}: The operation took too long to complete. Please try again.",
            _ => $"{baseMessage}. Please try again or contact support if the problem persists."
        };
    }
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; set; }

    public static OperationResult<T> SuccessResult(T data, string message = "Operation completed successfully")
    {
        return new OperationResult<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public new static OperationResult<T> FailureResult(string message, List<string>? errors = null)
    {
        return new OperationResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public new static OperationResult<T> ExceptionResult(Exception ex, string context = "")
    {
        return new OperationResult<T>
        {
            Success = false,
            Message = GetUserFriendlyErrorMessage(ex, context),
            Exception = ex,
            Errors = new List<string> { ex.Message }
        };
    }

    private static string GetUserFriendlyErrorMessage(Exception ex, string context)
    {
        var baseMessage = string.IsNullOrEmpty(context) ? "An error occurred" : $"An error occurred while {context}";

        return ex switch
        {
            UnauthorizedAccessException => $"{baseMessage}: You don't have permission to perform this action.",
            InvalidOperationException => $"{baseMessage}: The operation could not be completed. Please check your data and try again.",
            ArgumentException => $"{baseMessage}: Invalid data provided. Please check your input.",
            System.IO.IOException => $"{baseMessage}: A file system error occurred. Please check file permissions and disk space.",
            TimeoutException => $"{baseMessage}: The operation took too long to complete. Please try again.",
            _ => $"{baseMessage}. Please try again or contact support if the problem persists."
        };
    }
}
