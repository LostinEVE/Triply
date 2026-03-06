# Database Import Guide

## How to Import Data from Your Other App

Your app now has a fully functional database import system. Here's how to use it:

### Step 1: Prepare Your Backup File

Your backup file should be one of these formats:
- **TAR.GZ file** (`.tar.gz` or `.tgz`) containing a JSON file with your data
  - Common naming patterns like `dir.tar.gz` or `backup.tar.gz` are supported
- **ZIP file** containing a JSON file with your data
- **JSON file** directly with your data structure

The JSON file should have this structure:

```json
{
  "CompanySettings": [...],
  "Trucks": [...],
  "Drivers": [...],
  "Customers": [...],
  "Loads": [...],
  "Expenses": [...],
  "FuelEntries": [...],
  "MaintenanceRecords": [...],
  "Invoices": [...],
  "InvoiceLineItems": [...],
  "TaxPayments": [...]
}
```

### Step 2: Access the Import Feature

1. Open your Triply app
2. Navigate to **Settings → Data Management**
3. Look for the **Import Data** card (blue header)

### Step 3: Import Your Data

1. Click **"Select Backup File"** button
2. Choose your `.tar.gz`, `.tgz`, `.zip`, or `.json` file from your device
3. You'll see the file name and size displayed
4. Click **"Import Data"** button
5. Wait for the import to complete (you'll see a progress indicator)
6. Review the results showing how many records were imported for each category

### Features

✅ **Duplicate Protection**: Records with existing IDs are automatically skipped to prevent duplicates  
✅ **Transaction Safety**: All imports happen within a database transaction - if anything fails, nothing is imported  
✅ **Detailed Results**: See exactly how many of each type of record was imported  
✅ **Format Flexibility**: Supports TAR.GZ, ZIP archives and direct JSON files  
✅ **Large File Support**: Can handle files up to 100MB

### What Gets Imported

- ✓ Trucks and Equipment
- ✓ Drivers
- ✓ Customers
- ✓ Loads and Deliveries
- ✓ Expenses
- ✓ Fuel Entries
- ✓ Maintenance Records
- ✓ Invoices and Line Items
- ✓ Tax Payments
- ✓ Company Settings

### Troubleshooting

**"No JSON data file found in the archive"**
- Make sure your TAR.GZ or ZIP file contains a `.json` file
- The JSON file can be named `data.json`, `backup.json`, or any name ending in `.json`

**"Unsupported file format"**
- Only `.tar.gz`, `.tgz`, `.zip` and `.json` files are supported
- Make sure your file has the correct extension

**Can't select the file on mobile (Android/iOS)**
- The app now uses the native file picker on mobile devices
- Make sure your file is saved in a location accessible to apps (Downloads, Documents, etc.)
- On Android, you may need to grant storage permissions
- On iOS, make sure the file is in iCloud Drive or the Files app
- Try using the "Browse" option in the file picker to navigate to your file

**Import fails partway through**
- The import uses transactions, so partial imports won't corrupt your database
- Check the error message for specific details
- Ensure your JSON structure matches the expected format

### Important Notes

⚠️ **Backup First**: Always create a backup before importing data  
⚠️ **Data Merge**: Import merges with existing data (doesn't delete existing records)  
⚠️ **IDs Matter**: Make sure your backup has valid ID fields for each record type  

### File Size Limit

The current maximum file size is **100MB**. For larger imports, consider:
- Splitting your data into multiple files
- Compressing your JSON file into a ZIP archive
- Contacting support for assistance with very large datasets

---

## For Developers

The import functionality is implemented in:
- **Service**: `Triply.Services/DataImportExportService.cs`
- **UI**: `Triply/Components/Pages/DataManagement.razor`
- **Interface**: `IDataImportExportService`

To customize or extend the import process, modify the `DataImportExportService` class.
