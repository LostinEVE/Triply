using Triply.Core.Interfaces;

namespace Triply.Services;

public class PrintService : IPrintService
{
    public async Task<bool> PrintPdfAsync(byte[] pdfBytes, string fileName)
    {
        try
        {
            // Save PDF to temporary location
            var tempPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(tempPath, pdfBytes);

            // On desktop platforms, use share which opens print dialog
            // On mobile, share allows printing via system share sheet
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Print {fileName}",
                File = new ShareFile(tempPath)
            });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error printing PDF: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SavePdfAsync(byte[] pdfBytes, string fileName, string? directory = null)
    {
        try
        {
            string targetPath;

            if (string.IsNullOrEmpty(directory))
            {
                // Use Downloads folder if available, otherwise use Documents
                var downloadsPath = await GetDownloadsFolderPathAsync();
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

            await File.WriteAllBytesAsync(targetPath, pdfBytes);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving PDF: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SharePdfAsync(byte[] pdfBytes, string fileName)
    {
        try
        {
            // Save to cache first
            var tempPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(tempPath, pdfBytes);

            // Share the file
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Share {fileName}",
                File = new ShareFile(tempPath)
            });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sharing PDF: {ex.Message}");
            return false;
        }
    }

    public Task<string?> GetDownloadsFolderPathAsync()
    {
        try
        {
            // Platform-specific downloads folder
            string? downloadsPath = null;

#if ANDROID
            downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
                Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
#elif IOS || MACCATALYST
            downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "..",
                "Downloads");
#elif WINDOWS
            downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");
#endif

            return Task.FromResult(downloadsPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting downloads folder: {ex.Message}");
            return Task.FromResult<string?>(null);
        }
    }
}
