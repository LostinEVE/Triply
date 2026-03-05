# Triply OTR Trucking Management System - Architecture Overview

## ✅ Completed Setup

### Solution Structure (4 Projects)
```
Triply.sln
├── Triply (MAUI Blazor) - Main application
├── Triply.Core (Class Library) - Models, Interfaces, Enums
├── Triply.Data (Class Library) - EF Core, Repositories, Migrations
└── Triply.Services (Class Library) - Business Logic
```

### Technology Stack Configured
- ✅ .NET 10 MAUI Hybrid Blazor
- ✅ SQLite with Entity Framework Core 10
- ✅ MudBlazor UI Components
- ✅ CommunityToolkit.Maui
- ✅ Geolocation Services (MAUI Essentials)
- ✅ QuestPDF for PDF generation
- ✅ MailKit for email

### Data Models (11 Entities)
1. ✅ **Truck** - Fleet management with custom IDs
2. ✅ **Driver** - CDL tracking, pay rates, assignments
3. ✅ **Customer** - Contact and billing information
4. ✅ **Load** - Freight tracking with pickup/delivery
5. ✅ **Invoice** - Billing with line items
6. ✅ **InvoiceLineItem** - Invoice details
7. ✅ **Expense** - Categorized expense tracking
8. ✅ **FuelEntry** - Fuel purchases with IFTA support
9. ✅ **MaintenanceRecord** - Service history
10. ✅ **TaxPayment** - Tax obligation tracking
11. ✅ **CompanySettings** - Configuration (singleton)

### Enums Defined
- ✅ TruckStatus, PayType, LoadStatus, RateType
- ✅ InvoiceStatus, ExpenseCategory, PaymentMethod
- ✅ FuelType, MaintenanceType, TaxType, TaxPaymentStatus

### Repository Pattern Implementation

#### Generic Repository Pattern
```csharp
IRepository<T>
└── Repository<T> (base implementation)
    ├── TruckRepository
    ├── DriverRepository
    ├── LoadRepository
    ├── InvoiceRepository
    ├── CustomerRepository
    ├── ExpenseRepository
    ├── FuelEntryRepository
    └── MaintenanceRepository
```

#### Unit of Work Pattern
```csharp
IUnitOfWork
└── UnitOfWork
    ├── Provides access to all repositories
    ├── Transaction management
    ├── Centralized SaveChanges
    └── IDisposable pattern
```

### Services Layer

#### Business Services
- ✅ **LoadManagementService** - Complex load operations, invoice creation
- ✅ **IFTAReportService** - Quarterly fuel tax reporting
- ✅ **TruckingBusinessService** - Analytics, profit calculations, dashboard stats

#### Infrastructure Services
- ✅ **GeolocationService** - GPS location and geocoding
- ✅ **PdfGenerationService** - Invoice PDF generation with QuestPDF
- ✅ **EmailService** - SMTP email with attachments

### Database Configuration

#### EF Core Setup
- ✅ TriplyDbContext with all 11 DbSets
- ✅ Complete fluent API configuration
- ✅ Proper relationships and cascading behaviors
- ✅ Decimal precision for financial fields
- ✅ String length constraints
- ✅ Design-time DbContextFactory for migrations

#### Migration System
- ✅ Initial migration created: `20260305013853_InitialCreate.cs`
- ✅ Auto-initialization on app startup
- ✅ Default CompanySettings seeded

### Dependency Injection

All configured in `MauiProgram.cs`:

```csharp
// Database
AddDbContext<TriplyDbContext> with SQLite

// Unit of Work (Primary pattern)
IUnitOfWork → UnitOfWork

// Specialized Repositories (For complex queries)
ITruckRepository → TruckRepository
IDriverRepository → DriverRepository
ILoadRepository → LoadRepository
IInvoiceRepository → InvoiceRepository
+ Non-interface repositories (CustomerRepository, ExpenseRepository, etc.)

// Business Services
LoadManagementService
IFTAReportService  
TruckingBusinessService

// Infrastructure Services
IGeolocationService → GeolocationService (Singleton)
IPdfGenerationService → PdfGenerationService (Scoped)
IEmailService → EmailService (Transient)
```

## Key Design Decisions

### Why Unit of Work?
1. **Transaction Management**: Multi-entity operations in single transaction
2. **Consistency**: Single SaveChanges for all repositories
3. **Testability**: Easy to mock entire data layer
4. **Performance**: Reuse repository instances per request

### Why Specialized Repositories?
While Unit of Work provides generic access, specialized repositories:
1. Pre-configure complex includes
2. Implement business-specific queries
3. Provide strongly-typed methods
4. Encapsulate query logic

### Hybrid Approach Benefits
- Use `IUnitOfWork` for simple CRUD and transactions
- Use specialized repositories for complex domain queries
- Both patterns work together seamlessly

## Usage Examples

### Example 1: Simple CRUD with Unit of Work
```csharp
@inject IUnitOfWork UnitOfWork

// Get all active trucks
var trucks = await UnitOfWork.Trucks
    .FindAsync(t => t.Status == TruckStatus.Active);

// Add new truck
var truck = new Truck { TruckId = "TRUCK-001", /* ... */ };
await UnitOfWork.Trucks.AddAsync(truck);
await UnitOfWork.SaveChangesAsync();
```

### Example 2: Complex Query with GetQueryable
```csharp
var recentLoads = await UnitOfWork.Loads
    .GetQueryable()
    .Include(l => l.Customer)
    .Include(l => l.Truck)
    .Include(l => l.Driver)
    .Where(l => l.PickupDate >= DateTime.UtcNow.AddMonths(-1))
    .OrderByDescending(l => l.PickupDate)
    .ToListAsync();
```

### Example 3: Transaction with Multiple Entities
```csharp
await using var transaction = await UnitOfWork.BeginTransactionAsync();

try
{
    // Create invoice
    var invoice = new Invoice { /* ... */ };
    await UnitOfWork.Invoices.AddAsync(invoice);
    
    // Update load status
    var load = await UnitOfWork.Loads.GetByIdAsync(loadId);
    load.Status = LoadStatus.Invoiced;
    await UnitOfWork.Loads.UpdateAsync(load);
    
    // Update company settings
    var settings = await UnitOfWork.CompanySettings
        .GetQueryable()
        .FirstOrDefaultAsync();
    settings.NextInvoiceNumber++;
    await UnitOfWork.CompanySettings.UpdateAsync(settings);
    
    await UnitOfWork.SaveChangesAsync();
    await UnitOfWork.CommitTransactionAsync();
}
catch
{
    await UnitOfWork.RollbackTransactionAsync();
    throw;
}
```

### Example 4: Using Specialized Repository
```csharp
@inject ITruckRepository TruckRepository

// Gets truck with ALL related data preloaded
var truck = await TruckRepository.GetTruckByIdAsync("TRUCK-001");
// Includes: Drivers, Loads, Expenses, FuelEntries, MaintenanceRecords
```

### Example 5: Business Service with Unit of Work
```csharp
public class LoadManagementService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Invoice> CreateInvoiceFromLoadAsync(Guid loadId)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            var load = await _unitOfWork.Loads
                .GetQueryable()
                .Include(l => l.Customer)
                .FirstOrDefaultAsync(l => l.LoadId == loadId);

            // Generate invoice number
            var settings = await _unitOfWork.CompanySettings
                .GetQueryable()
                .FirstOrDefaultAsync();

            var invoiceNumber = $"{settings.InvoicePrefix}-{DateTime.UtcNow.Year}-{settings.NextInvoiceNumber:D4}";
            settings.NextInvoiceNumber++;

            // Create invoice
            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                CustomerId = load.CustomerId,
                // ... other properties
            };

            await _unitOfWork.Invoices.AddAsync(invoice);
            load.Status = LoadStatus.Invoiced;
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return invoice;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

## Database Schema Highlights

### Relationships
- Truck → Drivers (One-to-Many)
- Truck → Loads (One-to-Many)
- Truck → Expenses (One-to-Many)
- Truck → FuelEntries (One-to-Many)
- Truck → MaintenanceRecords (One-to-Many)
- Driver → Truck (Many-to-One, nullable)
- Driver → Loads (One-to-Many)
- Customer → Loads (One-to-Many)
- Customer → Invoices (One-to-Many)
- Load → InvoiceLineItems (One-to-Many)
- Invoice → InvoiceLineItems (One-to-Many)

### Cascade Behaviors
- Truck deletion → Cascades to FuelEntries and MaintenanceRecords
- Truck deletion → Sets null on Drivers, Loads, Expenses
- Customer deletion → Restricted (must not have loads/invoices)
- Invoice deletion → Cascades to InvoiceLineItems

## Next Development Steps

1. **UI Components**
   - Create Blazor pages for each entity
   - Implement MudBlazor DataGrids
   - Add forms with validation

2. **Dashboard**
   - Use TruckingBusinessService.GetDashboardStatsAsync()
   - Charts for revenue, expenses, profit
   - Upcoming maintenance alerts
   - Expiring CDLs

3. **Reports**
   - IFTA quarterly reports
   - Profit/loss statements
   - Truck performance metrics
   - Driver pay summaries

4. **Configuration**
   - Settings page for CompanySettings
   - SMTP configuration for emails
   - Backup/restore functionality

5. **Security**
   - User authentication
   - Role-based access control
   - Data encryption for sensitive info

## File Locations

### Models
`Triply.Core/Models/`
- Truck.cs, Driver.cs, Customer.cs, Load.cs
- Invoice.cs, InvoiceLineItem.cs
- Expense.cs, FuelEntry.cs, MaintenanceRecord.cs
- TaxPayment.cs, CompanySettings.cs

### Enums
`Triply.Core/Enums/TripStatus.cs` (contains all enums)

### Interfaces
`Triply.Core/Interfaces/`
- IRepository.cs, IUnitOfWork.cs, IRepositories.cs
- IGeolocationService.cs, IPdfGenerationService.cs, IEmailService.cs

### Repositories
`Triply.Data/Repositories/`
- Repository.cs (base generic)
- TruckRepository.cs, DriverRepository.cs, LoadRepository.cs
- InvoiceRepository.cs, CustomerRepository.cs
- ExpenseRepository.cs, FuelEntryRepository.cs, MaintenanceRepository.cs

### Services
`Triply.Services/`
- LoadManagementService.cs
- IFTAReportService.cs
- TruckingBusinessService.cs
- GeolocationService.cs
- PdfGenerationService.cs
- EmailService.cs

### Data Layer
`Triply.Data/`
- TriplyDbContext.cs
- TriplyDbContextFactory.cs (for migrations)
- DatabaseInitializer.cs
- Migrations/ (auto-generated)

## Documentation
- `README.md` - Project overview
- `REPOSITORY_PATTERN_GUIDE.md` - Detailed usage patterns
- `ARCHITECTURE_SUMMARY.md` - This file

---

**The application is now fully configured and ready for UI development!**
