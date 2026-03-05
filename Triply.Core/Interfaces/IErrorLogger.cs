namespace Triply.Core.Interfaces;

public interface IErrorLogger
{
    Task LogErrorAsync(Exception exception, string context = "");
    Task LogWarningAsync(string message, string context = "");
    Task LogInfoAsync(string message, string context = "");
    Task<List<string>> GetRecentLogsAsync(int count = 100);
    Task ClearLogsAsync();
}
