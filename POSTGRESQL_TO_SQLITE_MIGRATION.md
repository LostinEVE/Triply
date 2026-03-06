# PostgreSQL to SQLite Migration Guide

## Common Issues When Importing PostgreSQL Data to SQLite

If you're importing data from a PostgreSQL backup into Triply's SQLite database, you may encounter compatibility issues. Here's how to address them:

### 1. **Data Type Differences**

#### Boolean Values
- **PostgreSQL**: Uses `true`/`false` or `t`/`f`
- **SQLite**: Uses `1`/`0` or `true`/`false` (stored as integers)
- **Solution**: The import will automatically handle boolean conversion

#### Dates and Times
- **PostgreSQL**: Stores timestamps with timezone info
- **SQLite**: Stores as strings in ISO 8601 format
- **Solution**: Ensure dates are in format: `YYYY-MM-DD HH:MM:SS`

#### Decimal/Numeric
- **PostgreSQL**: Has precise NUMERIC type
- **SQLite**: Stores as REAL (floating point)
- **Solution**: May lose some precision on very large numbers

### 2. **Primary Key Strategies**

#### Auto-increment IDs
- **PostgreSQL**: Uses SERIAL or SEQUENCE
- **SQLite**: Uses AUTOINCREMENT
- **Issue**: PostgreSQL sequences may have gaps or start at different numbers
- **Solution**: The import preserves existing IDs

#### UUIDs/GUIDs
- **PostgreSQL**: Native UUID type
- **SQLite**: Stored as TEXT or BLOB
- **Solution**: Convert UUIDs to string format

### 3. **Common Errors and Solutions**

#### Error: "JSON parsing failed"
**Cause**: PostgreSQL exports may include extra fields or different formatting

**Solution**:
```json
// Ensure your JSON structure matches:
{
  "Trucks": [...],
  "Drivers": [...],
  "Customers": [...],
  // etc.
}
```

#### Error: "Failed to import X record (ID: Y)"
**Cause**: Data type mismatch or constraint violation

**Solutions**:
1. Check for NULL values in required fields
2. Verify foreign key relationships are valid
3. Ensure ID fields are properly formatted

#### Error: "Foreign key constraint failed"
**Cause**: Referenced records don't exist yet

**Solution**: Import order matters! The app imports in this order:
1. CompanySettings
2. Trucks
3. Drivers
4. Customers
5. Loads
6. Expenses
7. FuelEntries
8. MaintenanceRecords
9. Invoices
10. InvoiceLineItems
11. TaxPayments

### 4. **Data Sanitization Checklist**

Before importing PostgreSQL data:

- [ ] Remove PostgreSQL-specific metadata fields
- [ ] Convert boolean `true`/`false` strings if needed
- [ ] Ensure all dates are in ISO 8601 format
- [ ] Remove any NULL or undefined values for required fields
- [ ] Verify all GUIDs are properly formatted
- [ ] Check that all foreign key references exist
- [ ] Remove any array or JSON column data (not supported in basic SQLite)

### 5. **Viewing Detailed Error Messages**

The import now provides detailed error messages including:
- Which record failed to import
- The specific error message
- The ID of the problematic record

Watch for these messages in the import result dialog.

### 6. **Manual Data Conversion**

If automatic import fails, you may need to manually convert the data:

#### Using Python:
```python
import json

# Load PostgreSQL export
with open('postgres_backup.json') as f:
    data = json.load(f)

# Convert booleans
for table in ['Trucks', 'Drivers', 'Customers']:
    if table in data:
        for record in data[table]:
            for key, value in record.items():
                if value == 'true' or value == 't':
                    record[key] = True
                elif value == 'false' or value == 'f':
                    record[key] = False

# Save converted data
with open('sqlite_ready.json', 'w') as f:
    json.dump(data, f, indent=2)
```

#### Using PowerShell:
```powershell
# Read the JSON file
$data = Get-Content -Path 'postgres_backup.json' | ConvertFrom-Json

# Convert the data (add your conversion logic here)

# Save back to JSON
$data | ConvertTo-Json -Depth 10 | Out-File 'sqlite_ready.json'
```

### 7. **Testing Your Import**

1. **Start Small**: Try importing just one table at a time
2. **Use Test Data**: Test with a small subset first
3. **Check Logs**: Look at the detailed error messages
4. **Verify Results**: Check that imported data looks correct

### 8. **Getting Help**

If you continue to have issues:

1. **Check the error message** - It will tell you which record and table failed
2. **Examine that specific record** in your JSON file
3. **Look for**:
   - Missing required fields
   - Invalid data types
   - Broken foreign key relationships
   - PostgreSQL-specific formatting

### 9. **Alternative: Database Migration Tools**

For large datasets or complex migrations, consider using:
- **pgloader** - PostgreSQL to SQLite migration tool
- **SQLite Manager** - GUI tool for data manipulation
- **DB Browser for SQLite** - Visual database editor

These tools can handle automatic type conversion and provide better error handling for large migrations.

## Need More Help?

If you're still having trouble, please:
1. Note the exact error message
2. Identify which record/table is failing
3. Check the JSON structure of that record
4. Consult the error logs for more details
