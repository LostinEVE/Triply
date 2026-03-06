namespace Triply.Core.Interfaces;

public interface IPrintService
{
    Task<bool> PrintPdfAsync(byte[] pdfBytes, string fileName);
    Task<bool> SavePdfAsync(byte[] pdfBytes, string fileName, string? directory = null);
    Task<bool> SharePdfAsync(byte[] pdfBytes, string fileName);
    Task<string?> GetDownloadsFolderPathAsync();
}
