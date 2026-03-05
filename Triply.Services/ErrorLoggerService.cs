using Triply.Core.Interfaces;
using Microsoft.Maui.Storage;

namespace Triply.Services;

public class ErrorLoggerService : IErrorLogger
{
    private readonly string _logFilePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public ErrorLoggerService()
    {
        var logDirectory = Path.Combine(FileSystem.AppDataDirectory, "Logs");
        Directory.CreateDirectory(logDirectory);
        _logFilePath = Path.Combine(logDirectory, $"triply_{DateTime.Now:yyyyMMdd}.log");
    }

    public async Task LogErrorAsync(Exception exception, string context = "")
    {
        var logEntry = FormatLogEntry("ERROR", $"{context}: {exception.Message}\n{exception.StackTrace}");
        await WriteLogAsync(logEntry);
    }

    public async Task LogWarningAsync(string message, string context = "")
    {
        var logEntry = FormatLogEntry("WARNING", $"{context}: {message}");
        await WriteLogAsync(logEntry);
    }

    public async Task LogInfoAsync(string message, string context = "")
    {
        var logEntry = FormatLogEntry("INFO", $"{context}: {message}");
        await WriteLogAsync(logEntry);
    }

    public async Task<List<string>> GetRecentLogsAsync(int count = 100)
    {
        try
        {
            if (!File.Exists(_logFilePath))
                return new List<string>();

            await _semaphore.WaitAsync();
            try
            {
                var lines = await File.ReadAllLinesAsync(_logFilePath);
                return lines.TakeLast(count).ToList();
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task ClearLogsAsync()
    {
        try
        {
            await _semaphore.WaitAsync();
            try
            {
                if (File.Exists(_logFilePath))
                {
                    File.Delete(_logFilePath);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch
        {
            // Ignore errors when clearing logs
        }
    }

    private string FormatLogEntry(string level, string message)
    {
        return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
    }

    private async Task WriteLogAsync(string logEntry)
    {
        try
        {
            await _semaphore.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(_logFilePath, logEntry + Environment.NewLine);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch
        {
            // If we can't write to the log file, silently fail to avoid cascading errors
        }
    }
}
