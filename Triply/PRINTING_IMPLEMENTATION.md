# Printing & Export Implementation - Triply

## ✅ **Components Implemented**

### **1. PrintService** (`IPrintService`)
Full-featured printing service using MAUI APIs:

- ✅ **`PrintPdfAsync()`** - Opens system print dialog/share sheet with PDF
- ✅ **`SavePdfAsync()`** - Saves PDF to Downloads folder (platform-specific)
- ✅ **`SharePdfAsync()`** - Opens share dialog to share PDF via email, messages, etc.
- ✅ **`GetDownloadsFolderPathAsync()`** - Returns platform-specific downloads path

**Platform Behavior:**
- **Windows**: Opens native print dialog or saves to `C:\Users\{User}\Downloads`
- **Android**: Opens share sheet with print option, saves to `/storage/emulated/0/Download`
- **iOS/Mac**: Opens share sheet, saves to Documents/Downloads folder

### **2. ExportButtons Component** (`ExportButtons.razor`)
Reusable export toolbar with multiple options:

**Features:**
- ✅ **Print** - Opens print dialog with generated PDF
- ✅ **Save PDF** - Saves to downloads folder
- ✅ **Share** - Opens system share dialog
- ✅ **Export CSV** - Exports data tables to CSV (optional)
- ✅ **Refresh** - Regenerates PDF/data (optional)

**Parameters:**
```razor
<ExportButtons PdfBytes="@_pdfBytes" 
               FileName="Invoice_INV-001.pdf"
               CsvData="@_csvData"
               CsvFileName="expenses.csv"
               OnExportCsv="@ExportCsvAsync"
               OnRefresh="@RegeneratePdfAsync" />
```

### **3. CSV Export Helper** (`CsvExportHelper`)
Static helper class for exporting data to CSV:

```csharp
// Export any IEnumerable to CSV
var csv = CsvExportHelper.ToCsv(expenses, "Date", "Category", "Amount", "Vendor");

// Save CSV to file
await CsvExportHelper.SaveCsvAsync(csv, "expenses.csv");
```

**Features:**
- ✅ Auto-escapes commas, quotes, and newlines
- ✅ Supports custom headers or auto-generated from properties
- ✅ Platform-specific file saving
- ✅ UTF-8 encoding

---

## **📄 Printable Documents**

All these document types can be printed using the existing `IPdfGenerationService`:

### **1. Invoices** ✅
```csharp
var pdfBytes = await PdfService.GenerateInvoiceAsync(invoice, settings);
await PrintService.PrintPdfAsync(pdfBytes, $"Invoice_{invoice.InvoiceNumber}.pdf");
```

### **2. Financial Reports** ✅
- **Profit & Loss Statement**
  ```csharp
  var pdfBytes = await PdfService.GenerateProfitLossReportAsync(incomeStatement, settings, priorPeriod);
  ```
- **Balance Sheet**
  ```csharp
  var pdfBytes = await PdfService.GenerateBalanceSheetAsync(balanceSheet, settings);
  ```

### **3. Tax Reports** ✅
- **Quarterly Tax Estimate**
  ```csharp
  var pdfBytes = await PdfService.GenerateTaxReportAsync(taxEstimate, deductions, settings);
  ```
- **Annual Tax Summary**
  ```csharp
  var pdfBytes = await PdfService.GenerateAnnualTaxSummaryAsync(projection, settings);
  ```

### **4. IFTA Reports** ✅
```csharp
var pdfBytes = await PdfService.GenerateIFTAReportAsync(report, settings);
```

### **5. Cost Per Mile Reports** ✅
```csharp
var pdfBytes = await PdfService.GenerateCPMReportAsync(report, settings);
```

### **6. Individual Records**
- **Expense Receipts** - PDF generation can be added
- **Maintenance Records** - PDF generation can be added
- **Fuel Entry Summaries** - PDF generation can be added

---

## **🎯 Usage Examples**

### **Example 1: Invoice Detail Page** (Already Implemented)

```razor
@inject IPdfGenerationService PdfService
@inject IPrintService PrintService

<ExportButtons PdfBytes="@_pdfBytes" 
               FileName="@($"Invoice_{_invoice.InvoiceNumber}.pdf")"
               OnRefresh="@GeneratePdfAsync" />

@code {
    private byte[]? _pdfBytes;
    
    private async Task GeneratePdfAsync()
    {
        var settings = await UnitOfWork.CompanySettings.GetQueryable().FirstOrDefaultAsync();
        _pdfBytes = await PdfService.GenerateInvoiceAsync(_invoice, settings);
    }
}
```

### **Example 2: Report Page with CSV Export**

```razor
<ExportButtons PdfBytes="@_pdfBytes" 
               FileName="profit-loss-report.pdf"
               CsvData="@_csvData"
               CsvFileName="profit-loss-data.csv"
               OnExportCsv="@ExportCsvAsync"
               OnRefresh="@GenerateReportAsync" />

@code {
    private byte[]? _pdfBytes;
    private string? _csvData;
    
    private async Task GenerateReportAsync()
    {
        // Generate PDF
        _pdfBytes = await PdfService.GenerateProfitLossReportAsync(report, settings);
        
        // Generate CSV data
        var rows = new List<object>
        {
            new { Account = "Revenue", Amount = 50000 },
            new { Account = "Expenses", Amount = 30000 }
        };
        _csvData = CsvExportHelper.ToCsv(rows);
    }
    
    private async Task ExportCsvAsync()
    {
        if (_csvData == null) return;
        await CsvExportHelper.SaveCsvAsync(_csvData, "profit-loss-data.csv");
    }
}
```

### **Example 3: Expense List with CSV Export**

```razor
<MudButton StartIcon="@Icons.Material.Filled.TableChart"
           OnClick="@ExportExpensesToCsvAsync">
    Export to CSV
</MudButton>

@code {
    private async Task ExportExpensesToCsvAsync()
    {
        var expenses = await UnitOfWork.Expenses.GetQueryable()
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
        
        var csv = CsvExportHelper.ToCsv(
            expenses.Select(e => new
            {
                Date = e.ExpenseDate.ToString("yyyy-MM-dd"),
                Category = e.Category.ToString(),
                Vendor = e.Vendor,
                Amount = e.Amount,
                Description = e.Description
            }),
            "Date", "Category", "Vendor", "Amount", "Description"
        );
        
        var success = await CsvExportHelper.SaveCsvAsync(csv, $"expenses_{DateTime.Now:yyyy-MM-dd}.csv");
        
        if (success)
        {
            Snackbar.Add("CSV exported successfully", Severity.Success);
        }
    }
}
```

---

## **📱 Platform-Specific Behavior**

### **Windows**
- **Print**: Opens native Windows print dialog
- **Save**: Saves to `C:\Users\{User}\Downloads`
- **Share**: Opens Windows share charm

### **Android**
- **Print**: Opens Android print framework with printer selection
- **Save**: Saves to `/storage/emulated/0/Download` (requires storage permission)
- **Share**: Opens Android share sheet (Email, Drive, etc.)

### **iOS/macOS**
- **Print**: Opens iOS print center or macOS print dialog
- **Save**: Saves to `~/Documents/../Downloads`
- **Share**: Opens iOS/macOS share sheet

---

## **🔧 Service Registration**

```csharp
// MauiProgram.cs
builder.Services.AddSingleton<IPrintService, PrintService>();
```

Already added to your app! ✅

---

## **✨ Features**

### **Print Service**
- ✅ Platform-native print dialogs
- ✅ Automatic file management (temp cache cleanup)
- ✅ Error handling with user feedback
- ✅ Downloads folder detection per platform

### **Export Buttons**
- ✅ Loading states with progress indicators
- ✅ Disabled state while processing
- ✅ Conditional rendering (shows only relevant buttons)
- ✅ Snackbar notifications for success/error
- ✅ Icon-based UI (Material Icons)

### **CSV Export**
- ✅ RFC 4180 compliant CSV format
- ✅ Handles commas, quotes, newlines in data
- ✅ UTF-8 encoding for international characters
- ✅ Generic type support (works with any IEnumerable)

---

## **🚀 Next Steps**

### **To add printing to more pages:**

1. **Inject services:**
   ```razor
   @inject IPdfGenerationService PdfService
   @inject IPrintService PrintService
   ```

2. **Add state for PDF:**
   ```csharp
   private byte[]? _pdfBytes;
   ```

3. **Add export buttons:**
   ```razor
   <ExportButtons PdfBytes="@_pdfBytes" 
                  FileName="report.pdf"
                  OnRefresh="@GeneratePdfAsync" />
   ```

4. **Implement PDF generation:**
   ```csharp
   private async Task GeneratePdfAsync()
   {
       _pdfBytes = await PdfService.GenerateSomeReportAsync(...);
   }
   ```

### **Pages that should have export buttons:**
- ✅ Invoice Detail (Already done!)
- ⏳ Profit & Loss Report
- ⏳ Balance Sheet
- ⏳ Tax Reports
- ⏳ IFTA Report
- ⏳ CPM Report
- ⏳ Expense List
- ⏳ Maintenance Records List
- ⏳ Fuel Entry List

---

## **📝 CSV Export Recommendations**

Add CSV export to these data-heavy pages:
- **Expenses** - For accounting software import
- **Fuel Entries** - For IFTA filing
- **Maintenance Records** - For fleet management
- **Load History** - For broker reconciliation
- **Invoice List** - For AR aging reports

---

## **🎉 Summary**

**✅ Complete printing system implemented!**

- Print any PDF-based document
- Save to downloads folder
- Share via system dialogs
- Export tables to CSV
- Reusable components
- Platform-native behavior
- Invoice Detail page fully implemented as example

**Ready to add to all report pages!** 🖨️📄✨
