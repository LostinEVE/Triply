# Triply - OTR Trucking Management System

A comprehensive .NET 10 MAUI Hybrid Blazor application for managing over-the-road trucking operations.

## Solution Structure

```
Triply/
├── Triply/                     # Main MAUI Blazor application
├── Triply.Core/                # Models, Interfaces, Enums
├── Triply.Data/                # EF Core, Repositories, Migrations
└── Triply.Services/            # Business Logic Services
```

## Features

### Core Functionality
- **Fleet Management**: Track trucks, maintenance, and fuel consumption
- **Driver Management**: CDL tracking, pay rates, and assignments
- **Load Management**: Book, track, and complete freight loads
- **Customer Management**: Customer details and billing information
- **Invoicing**: Generate and track invoices with line items
- **Expense Tracking**: Categorize and track all business expenses
- **IFTA Reporting**: Automated quarterly fuel tax reporting
- **Tax Management**: Track tax payments and deadlines

### Technology Stack
- **.NET 10**: Latest .NET framework
- **MAUI Blazor**: Cross-platform UI with Blazor components
- **MudBlazor**: Material Design component library
- **Entity Framework Core 10**: SQLite database with Code-First approach
- **CommunityToolkit.Maui**: Additional device features
- **QuestPDF**: Professional PDF generation for invoices
- **MailKit**: Email sending capabilities
- **Geolocation Services**: Auto-location for fuel entries

## Architecture Patterns

### Repository Pattern with Unit of Work

The application implements a robust repository pattern with Unit of Work:

#### Generic Repository (`IRepository<T>`)
```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    IQueryable<T> GetQueryable();
}
```

#### Unit of Work (`IUnitOfWork`)
Provides centralized access to all repositories and transaction management:
```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<Truck> Trucks { get; }
    IRepository<Driver> Drivers { get; }
    IRepository<Load> Loads { get; }
    // ... other repositories
    
    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

#### Usage Example
```csharp
public class MyService
{
    private readonly IUnitOfWork _unitOfWork;

    public MyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateInvoiceWithTransaction()
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            var invoice = new Invoice { /* ... */ };
            await _unitOfWork.Invoices.AddAsync(invoice);
            
            // Update related load
            var load = await _unitOfWork.Loads.GetByIdAsync(loadId);
            load.Status = LoadStatus.Invoiced;
            await _unitOfWork.Loads.UpdateAsync(load);
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

## Data Models

### Truck
- Custom TruckId (e.g., "TRUCK-001")
- Vehicle details (Make, Model, Year, VIN)
- Registration and licensing
- Purchase information
- Current odometer
- Status tracking

### Driver
- Personal and contact information
- CDL details and expiration tracking
- Truck assignment
- Pay rate and type (per mile, percentage, hourly, salary)
- Active status

### Customer
- Company and contact information
- Billing address
- Payment terms
- Active status

### Load
- Load number and tracking
- Pickup and delivery locations with dates/times
- Distance, rate, and total amount
- Status workflow (Booked → InTransit → Delivered → Invoiced → Paid)
- POD (Proof of Delivery) document storage
- Customer, truck, and driver assignments

### Invoice
- Auto-generated invoice numbers
- Line items with load references
- Tax calculations
- Payment tracking
- Status workflow

### Expense
- Categorized expense tracking
- Receipt image storage
- Tax deductibility
- Truck association (optional)

### FuelEntry
- Detailed fuel purchase tracking
- GPS location capture
- IFTA quarter auto-calculation
- MPG tracking
- Receipt storage

### MaintenanceRecord
- Maintenance type categorization
- Cost breakdown (labor/parts)
- Warranty tracking
- Next service due tracking
- Document storage

### TaxPayment
- Federal, State, IFTA, 2290, UCR tracking
- Quarter and year organization
- Due date and payment tracking

### CompanySettings
- Company information (DOT#, MC#, EIN)
- Invoice settings and numbering
- Tax rates
- Branding (logo)

## Services

### LoadManagementService
Handles complex load operations including:
- Creating loads
- Generating invoices from loads
- Assigning trucks and drivers
- Transaction management

### IFTAReportService
Automated IFTA quarterly reporting:
- Fuel consumption by state
- Mileage calculations
- Quarter-over-quarter comparisons

### TruckingBusinessService
Business analytics and operations:
- Dashboard statistics
- Profit calculations
- Available driver lookup
- Maintenance scheduling

### GeolocationService
Location services using MAUI Essentials:
- Current location retrieval
- Reverse geocoding for city/state

### PdfGenerationService
Professional PDF generation with QuestPDF:
- Invoice PDFs with company branding
- Detailed formatting and styling

### EmailService
Email functionality using MailKit:
- Invoice delivery
- Attachment support

## Database

### SQLite with EF Core
- Local storage in app data directory
- Code-First migrations
- Design-time factory for migrations
- Automatic initialization on first run

### Running Migrations
```bash
# Create new migration
dotnet ef migrations add MigrationName --project Triply.Data\Triply.Data.csproj

# Update database (done automatically on app start)
dotnet ef database update --project Triply.Data\Triply.Data.csproj
```

## Dependency Injection Configuration

All services and repositories are registered in `MauiProgram.cs`:

```csharp
// Database
builder.Services.AddDbContext<TriplyDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Specialized Repositories (optional, for complex queries)
builder.Services.AddScoped<ITruckRepository, TruckRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

// Business Services
builder.Services.AddScoped<LoadManagementService>();
builder.Services.AddScoped<IFTAReportService>();
builder.Services.AddScoped<TruckingBusinessService>();

// Infrastructure Services
builder.Services.AddSingleton<IGeolocationService, GeolocationService>();
builder.Services.AddScoped<IPdfGenerationService, PdfGenerationService>();
builder.Services.AddTransient<IEmailService, EmailService>();
```

## Getting Started

1. **Build the solution:**
   ```bash
   dotnet build
   ```

2. **The database will be automatically created and initialized on first run**

3. **Company Settings**: On first launch, default company settings are created. Update them through the UI.

4. **Add your first truck, driver, customer, and load!**

## Development Guidelines

### Using Unit of Work
Prefer `IUnitOfWork` for standard CRUD operations and complex transactions:
```csharp
// Inject IUnitOfWork
private readonly IUnitOfWork _unitOfWork;

// Simple query
var trucks = await _unitOfWork.Trucks.GetAllAsync();

// Complex query
var activeTrucks = await _unitOfWork.Trucks
    .GetQueryable()
    .Include(t => t.Drivers)
    .Where(t => t.Status == TruckStatus.Active)
    .ToListAsync();
```

### Using Specialized Repositories
Use specialized repositories when you need complex includes or business logic:
```csharp
// Inject specialized repository
private readonly ITruckRepository _truckRepository;

// Gets truck with all related data preloaded
var truck = await _truckRepository.GetTruckByIdAsync("TRUCK-001");
```

### Transactions
Always use transactions for multi-step operations:
```csharp
await using var transaction = await _unitOfWork.BeginTransactionAsync();
try
{
    // Multiple operations
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

## Platform Support

- ✅ Android
- ✅ iOS
- ✅ macOS (Catalyst)
- ✅ Windows

## License

Community/MIT (Update as needed)

## Next Steps

1. Configure MudBlazor theme in `MainLayout.razor`
2. Create Blazor pages for each entity
3. Implement authentication/authorization
4. Configure SMTP settings for email
5. Add data validation
6. Implement reporting dashboards
7. Add backup/export functionality

See `REPOSITORY_PATTERN_GUIDE.md` for detailed usage examples.
