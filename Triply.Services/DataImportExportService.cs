using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Triply.Core.Interfaces;
using Triply.Core.Models;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Readers;

namespace Triply.Services;

public interface IDataImportExportService
{
    Task<ImportResult> ImportFromArchiveAsync(Stream archiveStream, string fileName);
    Task<ImportResult> ImportFromJsonAsync(Stream jsonStream);
    Task<ExportResult> ExportToZipAsync();
    Task<ExportResult> ExportToJsonAsync();
}

public class DataImportExportService : IDataImportExportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DataImportExportService> _logger;

    public DataImportExportService(IUnitOfWork unitOfWork, ILogger<DataImportExportService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ImportResult> ImportFromArchiveAsync(Stream archiveStream, string fileName)
    {
        var result = new ImportResult();

        try
        {
            // Determine archive type from file extension
            if (fileName.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase))
            {
                return await ImportFromTarGzAsync(archiveStream);
            }
            else if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return await ImportFromZipAsync(archiveStream);
            }
            else
            {
                result.Success = false;
                result.Message = "Unsupported archive format. Please use .zip, .tar.gz, or .tgz files.";
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from archive file");
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
            return result;
        }
    }

    private async Task<ImportResult> ImportFromTarGzAsync(Stream tarGzStream)
    {
        var result = new ImportResult();

        try
        {
            using var archive = TarArchive.Open(tarGzStream);

            _logger.LogInformation($"Found {archive.Entries.Count(e => !e.IsDirectory)} files in tar.gz archive");

            // First, try to find files with known extensions
            foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
            {
                _logger.LogInformation($"Checking file: {entry.Key}");

                // Check for JSON, DAT, or TXT files (PostgreSQL dumps often use .dat)
                if (entry.Key.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
                    entry.Key.EndsWith(".dat", StringComparison.OrdinalIgnoreCase) ||
                    entry.Key.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation($"Attempting to import from: {entry.Key}");

                    using var entryStream = entry.OpenEntryStream();
                    using var memoryStream = new MemoryStream();
                    await entryStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    // Try to detect if it's JSON by peeking at the first character
                    using var reader = new StreamReader(memoryStream, leaveOpen: true);
                    var firstChar = (char)reader.Peek();
                    memoryStream.Position = 0;

                    if (firstChar == '{' || firstChar == '[')
                    {
                        // Looks like JSON, try to import it
                        _logger.LogInformation($"File {entry.Key} appears to be JSON");
                        return await ImportFromJsonAsync(memoryStream);
                    }
                    else
                    {
                        _logger.LogWarning($"File {entry.Key} doesn't appear to be JSON (first char: '{firstChar}')");
                    }
                }
            }

            // If no suitable file found, list all files in the archive
            var fileList = string.Join(", ", archive.Entries.Where(e => !e.IsDirectory).Select(e => e.Key));
            _logger.LogError($"No importable file found. Files in archive: {fileList}");

            result.Success = false;
            result.Message = $"No JSON data file found in the tar.gz archive.\n\nFiles found: {fileList}\n\nSupported formats: .json, .dat, .txt (must contain JSON data)";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from tar.gz file");
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
            return result;
        }
    }

    private async Task<ImportResult> ImportFromZipAsync(Stream zipStream)
    {
        var result = new ImportResult();

        try
        {
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

            _logger.LogInformation($"Found {archive.Entries.Count} files in zip archive");

            // Look for JSON, DAT, or TXT files
            foreach (var entry in archive.Entries.Where(e => !string.IsNullOrEmpty(e.Name)))
            {
                _logger.LogInformation($"Checking file: {entry.FullName}");

                if (entry.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
                    entry.Name.EndsWith(".dat", StringComparison.OrdinalIgnoreCase) ||
                    entry.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation($"Attempting to import from: {entry.Name}");

                    using var entryStream = entry.Open();
                    using var memoryStream = new MemoryStream();
                    await entryStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    // Try to detect if it's JSON
                    using var reader = new StreamReader(memoryStream, leaveOpen: true);
                    var firstChar = (char)reader.Peek();
                    memoryStream.Position = 0;

                    if (firstChar == '{' || firstChar == '[')
                    {
                        _logger.LogInformation($"File {entry.Name} appears to be JSON");
                        return await ImportFromJsonAsync(memoryStream);
                    }
                    else
                    {
                        _logger.LogWarning($"File {entry.Name} doesn't appear to be JSON (first char: '{firstChar}')");
                    }
                }
            }

            // If no suitable file found, list all files
            var fileList = string.Join(", ", archive.Entries.Where(e => !string.IsNullOrEmpty(e.Name)).Select(e => e.Name));
            _logger.LogError($"No importable file found. Files in archive: {fileList}");

            result.Success = false;
            result.Message = $"No JSON data file found in the zip archive.\n\nFiles found: {fileList}\n\nSupported formats: .json, .dat, .txt (must contain JSON data)";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from zip file");
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
            return result;
        }
    }

    public async Task<ImportResult> ImportFromJsonAsync(Stream jsonStream)
    {
        var result = new ImportResult();

        try
        {
            _logger.LogInformation("Starting JSON import...");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var backupData = await JsonSerializer.DeserializeAsync<BackupData>(jsonStream, options);

            if (backupData == null)
            {
                result.Success = false;
                result.Message = "Failed to deserialize backup data. The JSON file may be empty or corrupted.";
                _logger.LogError("Deserialization returned null");
                return result;
            }

            _logger.LogInformation("JSON deserialized successfully");
            _logger.LogInformation($"Found: {backupData.Trucks?.Count ?? 0} trucks, {backupData.Drivers?.Count ?? 0} drivers, {backupData.Customers?.Count ?? 0} customers");

            // Start a transaction
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Import data in the correct order to respect foreign key constraints
                _logger.LogInformation("Importing CompanySettings...");
                await ImportCompanySettingsAsync(backupData.CompanySettings, result);

                _logger.LogInformation("Importing Trucks...");
                await ImportTrucksAsync(backupData.Trucks, result);

                _logger.LogInformation("Importing Drivers...");
                await ImportDriversAsync(backupData.Drivers, result);

                _logger.LogInformation("Importing Customers...");
                await ImportCustomersAsync(backupData.Customers, result);

                _logger.LogInformation("Importing Loads...");
                await ImportLoadsAsync(backupData.Loads, result);

                _logger.LogInformation("Importing Expenses...");
                await ImportExpensesAsync(backupData.Expenses, result);

                _logger.LogInformation("Importing FuelEntries...");
                await ImportFuelEntriesAsync(backupData.FuelEntries, result);

                _logger.LogInformation("Importing MaintenanceRecords...");
                await ImportMaintenanceRecordsAsync(backupData.MaintenanceRecords, result);

                _logger.LogInformation("Importing Invoices...");
                await ImportInvoicesAsync(backupData.Invoices, result);

                _logger.LogInformation("Importing InvoiceLineItems...");
                await ImportInvoiceLineItemsAsync(backupData.InvoiceLineItems, result);

                _logger.LogInformation("Importing TaxPayments...");
                await ImportTaxPaymentsAsync(backupData.TaxPayments, result);

                _logger.LogInformation("Saving changes...");
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Transaction committed successfully");

                result.Success = true;
                result.Message = $"Successfully imported {result.TotalRecordsImported} records.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during import, rolling back transaction");
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON parsing error");
            result.Success = false;
            result.Message = $"JSON parsing failed: {jsonEx.Message}\n\nThe file may be from PostgreSQL. Please ensure it's properly formatted for SQLite.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from JSON");
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}\n\nStack trace: {ex.StackTrace}";
        }

        return result;
    }

    private async Task ImportCompanySettingsAsync(List<CompanySettings>? items, ImportResult result)
    {
        if (items == null || !items.Any())
        {
            _logger.LogInformation("No CompanySettings to import");
            return;
        }

        _logger.LogInformation($"Importing {items.Count} CompanySettings records");

        foreach (var item in items)
        {
            try
            {
                var existing = await _unitOfWork.CompanySettings.GetByIdAsync(item.Id);
                if (existing == null)
                {
                    await _unitOfWork.CompanySettings.AddAsync(item);
                    result.CompanySettingsImported++;
                }
                else
                {
                    _logger.LogInformation($"Skipping duplicate CompanySettings with ID {item.Id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error importing CompanySettings with ID {item.Id}: {ex.Message}");
                throw new Exception($"Failed to import CompanySettings record (ID: {item.Id}). Error: {ex.Message}", ex);
            }
        }
    }

    private async Task ImportTrucksAsync(List<Truck>? items, ImportResult result)
    {
        if (items == null || !items.Any())
        {
            _logger.LogInformation("No Trucks to import");
            return;
        }

        _logger.LogInformation($"Importing {items.Count} Truck records");

        foreach (var item in items)
        {
            try
            {
                var existing = await _unitOfWork.Trucks.GetByIdAsync(item.TruckId);
                if (existing == null)
                {
                    await _unitOfWork.Trucks.AddAsync(item);
                    result.TrucksImported++;
                }
                else
                {
                    _logger.LogInformation($"Skipping duplicate Truck with ID {item.TruckId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error importing Truck with ID {item.TruckId}: {ex.Message}");
                throw new Exception($"Failed to import Truck record (ID: {item.TruckId}). Error: {ex.Message}", ex);
            }
        }
    }

    private async Task ImportDriversAsync(List<Driver>? items, ImportResult result)
    {
        if (items == null || !items.Any())
        {
            _logger.LogInformation("No Drivers to import");
            return;
        }

        _logger.LogInformation($"Importing {items.Count} Driver records");

        foreach (var item in items)
        {
            try
            {
                var existing = await _unitOfWork.Drivers.GetByIdAsync(item.DriverId);
                if (existing == null)
                {
                    await _unitOfWork.Drivers.AddAsync(item);
                    result.DriversImported++;
                }
                else
                {
                    _logger.LogInformation($"Skipping duplicate Driver with ID {item.DriverId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error importing Driver with ID {item.DriverId}: {ex.Message}");
                throw new Exception($"Failed to import Driver record (ID: {item.DriverId}). Error: {ex.Message}", ex);
            }
        }
    }

    private async Task ImportCustomersAsync(List<Customer>? items, ImportResult result)
    {
        if (items == null || !items.Any()) return;

        foreach (var item in items)
        {
            var existing = await _unitOfWork.Customers.GetByIdAsync(item.CustomerId);
            if (existing == null)
            {
                await _unitOfWork.Customers.AddAsync(item);
                result.CustomersImported++;
            }
        }
    }

    private async Task ImportLoadsAsync(List<Load>? items, ImportResult result)
    {
        if (items == null || !items.Any()) return;

        foreach (var item in items)
        {
            var existing = await _unitOfWork.Loads.GetByIdAsync(item.LoadId);
            if (existing == null)
            {
                await _unitOfWork.Loads.AddAsync(item);
                result.LoadsImported++;
            }
        }
    }

    private async Task ImportExpensesAsync(List<Expense>? items, ImportResult result)
    {
        if (items == null || !items.Any()) return;

        foreach (var item in items)
        {
            var existing = await _unitOfWork.Expenses.GetByIdAsync(item.ExpenseId);
            if (existing == null)
            {
                await _unitOfWork.Expenses.AddAsync(item);
                result.ExpensesImported++;
            }
        }
    }

    private async Task ImportFuelEntriesAsync(List<FuelEntry>? items, ImportResult result)
    {
        if (items == null || !items.Any()) return;

        foreach (var item in items)
        {
            var existing = await _unitOfWork.FuelEntries.GetByIdAsync(item.FuelEntryId);
            if (existing == null)
            {
                await _unitOfWork.FuelEntries.AddAsync(item);
                result.FuelEntriesImported++;
            }
        }
    }

    private async Task ImportMaintenanceRecordsAsync(List<MaintenanceRecord>? items, ImportResult result)
    {
        if (items == null || !items.Any()) return;

        foreach (var item in items)
        {
            var existing = await _unitOfWork.MaintenanceRecords.GetByIdAsync(item.MaintenanceId);
            if (existing == null)
            {
                await _unitOfWork.MaintenanceRecords.AddAsync(item);
                result.MaintenanceRecordsImported++;
            }
        }
    }

    private async Task ImportInvoicesAsync(List<Invoice>? items, ImportResult result)
    {
        if (items == null || !items.Any()) return;

        foreach (var item in items)
        {
            var existing = await _unitOfWork.Invoices.GetByIdAsync(item.InvoiceId);
            if (existing == null)
            {
                await _unitOfWork.Invoices.AddAsync(item);
                result.InvoicesImported++;
            }
        }
    }

    private async Task ImportInvoiceLineItemsAsync(List<InvoiceLineItem>? items, ImportResult result)
    {
        if (items == null || !items.Any()) return;

        foreach (var item in items)
        {
            var existing = await _unitOfWork.InvoiceLineItems.GetByIdAsync(item.LineItemId);
            if (existing == null)
            {
                await _unitOfWork.InvoiceLineItems.AddAsync(item);
                result.InvoiceLineItemsImported++;
            }
        }
    }

    private async Task ImportTaxPaymentsAsync(List<TaxPayment>? items, ImportResult result)
    {
        if (items == null || !items.Any()) return;

        foreach (var item in items)
        {
            var existing = await _unitOfWork.TaxPayments.GetByIdAsync(item.TaxPaymentId);
            if (existing == null)
            {
                await _unitOfWork.TaxPayments.AddAsync(item);
                result.TaxPaymentsImported++;
            }
        }
    }

    public async Task<ExportResult> ExportToZipAsync()
    {
        // TODO: Implement export to zip
        throw new NotImplementedException();
    }

    public async Task<ExportResult> ExportToJsonAsync()
    {
        // TODO: Implement export to JSON
        throw new NotImplementedException();
    }
}

public class BackupData
{
    public List<CompanySettings>? CompanySettings { get; set; }
    public List<Truck>? Trucks { get; set; }
    public List<Driver>? Drivers { get; set; }
    public List<Customer>? Customers { get; set; }
    public List<Load>? Loads { get; set; }
    public List<Expense>? Expenses { get; set; }
    public List<FuelEntry>? FuelEntries { get; set; }
    public List<MaintenanceRecord>? MaintenanceRecords { get; set; }
    public List<Invoice>? Invoices { get; set; }
    public List<InvoiceLineItem>? InvoiceLineItems { get; set; }
    public List<TaxPayment>? TaxPayments { get; set; }
}

public class ImportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TrucksImported { get; set; }
    public int DriversImported { get; set; }
    public int CustomersImported { get; set; }
    public int LoadsImported { get; set; }
    public int ExpensesImported { get; set; }
    public int FuelEntriesImported { get; set; }
    public int MaintenanceRecordsImported { get; set; }
    public int InvoicesImported { get; set; }
    public int InvoiceLineItemsImported { get; set; }
    public int TaxPaymentsImported { get; set; }
    public int CompanySettingsImported { get; set; }

    public int TotalRecordsImported =>
        TrucksImported + DriversImported + CustomersImported + LoadsImported +
        ExpensesImported + FuelEntriesImported + MaintenanceRecordsImported +
        InvoicesImported + InvoiceLineItemsImported + TaxPaymentsImported +
        CompanySettingsImported;
}

public class ExportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public byte[]? Data { get; set; }
    public string FileName { get; set; } = string.Empty;
}
