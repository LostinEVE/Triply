using System.Text;

namespace Triply.Services;

public static class CsvExportHelper
{
    public static string ToCsv<T>(IEnumerable<T> data, params string[] headers)
    {
        var sb = new StringBuilder();

        // Add headers
        if (headers.Length > 0)
        {
            sb.AppendLine(string.Join(",", headers.Select(EscapeCsvValue)));
        }
        else
        {
            // Auto-generate headers from property names
            var properties = typeof(T).GetProperties();
            sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsvValue(p.Name))));
        }

        // Add data rows
        foreach (var item in data)
        {
            var properties = typeof(T).GetProperties();
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return EscapeCsvValue(value?.ToString() ?? "");
            });

            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    public static async Task<bool> SaveCsvAsync(string csvContent, string fileName, string? directory = null)
    {
        try
        {
            string targetPath;

            if (string.IsNullOrEmpty(directory))
            {
#if WINDOWS
                var downloadsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads");
#elif ANDROID
                var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
#else
                var downloadsPath = FileSystem.AppDataDirectory;
#endif
                targetPath = Path.Combine(downloadsPath ?? FileSystem.AppDataDirectory, fileName);
            }
            else
            {
                targetPath = Path.Combine(directory, fileName);
            }

            // Ensure directory exists
            var dir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(targetPath, csvContent, Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving CSV: {ex.Message}");
            return false;
        }
    }

    private static string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // If value contains comma, quote, or newline, wrap in quotes and escape quotes
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
