# Unit of Work Pattern in Triply - Complete Guide

## What is Unit of Work?

The Unit of Work pattern maintains a list of objects affected by a business transaction and coordinates the writing out of changes and the resolution of concurrency problems.

## Key Benefits

1. **Single Transaction Scope**: All repository operations within a Unit of Work share the same database context
2. **Atomic Operations**: Either all changes succeed or all fail
3. **Reduced Database Roundtrips**: Changes are batched
4. **Simplified Testing**: Mock one interface instead of many repositories
5. **Cleaner Code**: Less dependency injection noise

## Architecture

```
Service/Controller
    ↓
IUnitOfWork
    ↓
TriplyDbContext (shared across all repositories)
    ↓
Database
```

## IUnitOfWork Interface

```csharp
public interface IUnitOfWork : IDisposable
{
    // Repository Access
    IRepository<Truck> Trucks { get; }
    IRepository<Driver> Drivers { get; }
    IRepository<Customer> Customers { get; }
    IRepository<Load> Loads { get; }
    IRepository<Invoice> Invoices { get; }
    IRepository<InvoiceLineItem> InvoiceLineItems { get; }
    IRepository<Expense> Expenses { get; }
    IRepository<FuelEntry> FuelEntries { get; }
    IRepository<MaintenanceRecord> MaintenanceRecords { get; }
    IRepository<TaxPayment> TaxPayments { get; }
    IRepository<CompanySettings> CompanySettings { get; }

    // Persistence
    Task<int> SaveChangesAsync();
    
    // Transaction Management
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

## Generic Repository Interface

```csharp
public interface IRepository<T> where T : class
{
    // Retrieval
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> GetQueryable();  // For complex queries
    
    // Modification
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);
}
```

## Usage Patterns

### Pattern 1: Simple CRUD

```csharp
public class TruckListPage : ComponentBase
{
    [Inject] private IUnitOfWork UnitOfWork { get; set; } = null!;

    private List<Truck> trucks = new();

    protected override async Task OnInitializedAsync()
    {
        trucks = (await UnitOfWork.Trucks.GetAllAsync()).ToList();
    }

    private async Task AddTruck(Truck newTruck)
    {
        newTruck.DateAdded = DateTime.UtcNow;
        await UnitOfWork.Trucks.AddAsync(newTruck);
        await UnitOfWork.SaveChangesAsync();
        
        trucks = (await UnitOfWork.Trucks.GetAllAsync()).ToList();
    }
}
```

### Pattern 2: Filtering with Predicates

```csharp
// Find active drivers without assigned loads
var availableDrivers = await UnitOfWork.Drivers.FindAsync(d => 
    d.IsActive && d.Loads.All(l => l.Status != LoadStatus.InTransit));

// Find overdue invoices
var overdueInvoices = await UnitOfWork.Invoices.FindAsync(i =>
    i.Status != InvoiceStatus.Paid && 
    i.DueDate < DateTime.UtcNow);

// Find trucks needing maintenance
var trucksNeedingService = await UnitOfWork.Trucks.FindAsync(t =>
    t.Status == TruckStatus.Active &&
    t.CurrentOdometer >= 500000);
```

### Pattern 3: Complex Queries with GetQueryable()

```csharp
public async Task<List<LoadWithDetails>> GetRecentLoadsAsync()
{
    return await UnitOfWork.Loads
        .GetQueryable()
        .Include(l => l.Customer)
        .Include(l => l.Truck)
        .Include(l => l.Driver)
        .Where(l => l.PickupDate >= DateTime.UtcNow.AddMonths(-1))
        .OrderByDescending(l => l.PickupDate)
        .Select(l => new LoadWithDetails
        {
            LoadNumber = l.LoadNumber,
            CustomerName = l.Customer.CompanyName,
            TruckNumber = l.Truck != null ? l.Truck.TruckId : "Unassigned",
            DriverName = l.Driver != null ? $"{l.Driver.FirstName} {l.Driver.LastName}" : "Unassigned",
            Revenue = l.TotalAmount
        })
        .ToListAsync();
}
```

### Pattern 4: Multiple Entity Updates (Transaction)

```csharp
public class LoadService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task CompleteLoadAsync(Guid loadId)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // 1. Get the load
            var load = await _unitOfWork.Loads.GetByIdAsync(loadId);
            if (load == null) throw new Exception("Load not found");

            // 2. Update load status
            load.Status = LoadStatus.Delivered;
            load.DeliveryDate = DateTime.UtcNow;
            await _unitOfWork.Loads.UpdateAsync(load);

            // 3. Update truck odometer
            var truck = await _unitOfWork.Trucks.GetByIdAsync(load.TruckId!);
            if (truck != null)
            {
                truck.CurrentOdometer += load.Miles;
                truck.LastModified = DateTime.UtcNow;
                await _unitOfWork.Trucks.UpdateAsync(truck);
            }

            // 4. Create driver pay expense
            if (load.DriverId.HasValue)
            {
                var driver = await _unitOfWork.Drivers.GetByIdAsync(load.DriverId.Value);
                if (driver != null)
                {
                    var driverPay = CalculateDriverPay(driver, load);
                    var expense = new Expense
                    {
                        ExpenseId = Guid.NewGuid(),
                        TruckId = load.TruckId,
                        ExpenseDate = DateTime.UtcNow,
                        Category = ExpenseCategory.DriverPay,
                        Amount = driverPay,
                        Description = $"Driver pay for Load {load.LoadNumber}"
                    };
                    await _unitOfWork.Expenses.AddAsync(expense);
                }
            }

            // Save all changes as one transaction
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private decimal CalculateDriverPay(Driver driver, Load load)
    {
        return driver.PayType switch
        {
            PayType.PerMile => driver.PayRate ?? 0 * load.Miles,
            PayType.Percentage => load.TotalAmount * (driver.PayRate ?? 0) / 100,
            _ => driver.PayRate ?? 0
        };
    }
}
```

### Pattern 5: Batch Operations

```csharp
// Add multiple fuel entries at once
var fuelEntries = new List<FuelEntry>
{
    new FuelEntry { TruckId = "TRUCK-001", /* ... */ },
    new FuelEntry { TruckId = "TRUCK-002", /* ... */ }
};

await UnitOfWork.FuelEntries.AddRangeAsync(fuelEntries);
await UnitOfWork.SaveChangesAsync();

// Delete old records
var oldExpenses = await UnitOfWork.Expenses.FindAsync(e => 
    e.ExpenseDate < DateTime.UtcNow.AddYears(-7));

await UnitOfWork.Expenses.DeleteRangeAsync(oldExpenses);
await UnitOfWork.SaveChangesAsync();
```

### Pattern 6: Aggregations and Analytics

```csharp
public class AnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<MonthlyReport> GetMonthlyReportAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var totalRevenue = await _unitOfWork.Loads
            .GetQueryable()
            .Where(l => l.Status == LoadStatus.Paid &&
                       l.DeliveryDate >= startDate &&
                       l.DeliveryDate < endDate)
            .SumAsync(l => l.TotalAmount);

        var totalExpenses = await _unitOfWork.Expenses
            .GetQueryable()
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate < endDate)
            .SumAsync(e => e.Amount);

        var fuelCost = await _unitOfWork.FuelEntries
            .GetQueryable()
            .Where(f => f.FuelDate >= startDate && f.FuelDate < endDate)
            .SumAsync(f => f.TotalCost);

        var maintenanceCost = await _unitOfWork.MaintenanceRecords
            .GetQueryable()
            .Where(m => m.MaintenanceDate >= startDate && m.MaintenanceDate < endDate)
            .SumAsync(m => m.TotalCost);

        return new MonthlyReport
        {
            Year = year,
            Month = month,
            Revenue = totalRevenue,
            Expenses = totalExpenses + fuelCost + maintenanceCost,
            NetProfit = totalRevenue - (totalExpenses + fuelCost + maintenanceCost)
        };
    }
}
```

## When to Use What?

### Use IUnitOfWork When:
- ✅ Simple CRUD operations
- ✅ Multiple entity updates in one transaction
- ✅ Building complex queries with LINQ
- ✅ Aggregations and analytics
- ✅ Batch operations
- ✅ Testing (easy to mock)

### Use Specialized Repositories When:
- ✅ Need pre-configured complex includes
- ✅ Domain-specific query logic
- ✅ Business rule encapsulation
- ✅ Calculated fields (e.g., GenerateInvoiceNumberAsync)

### Both Work Together!
```csharp
public class InvoicingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInvoiceRepository _invoiceRepository;

    public async Task ProcessInvoice(Guid invoiceId)
    {
        // Use specialized repository for complex includes
        var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
        
        // Use Unit of Work for related updates
        await _unitOfWork.Invoices.UpdateAsync(invoice);
        
        var loads = await _unitOfWork.Loads.FindAsync(l => 
            l.InvoiceLineItems.Any(li => li.InvoiceId == invoiceId));
            
        foreach (var load in loads)
        {
            load.Status = LoadStatus.Invoiced;
            await _unitOfWork.Loads.UpdateAsync(load);
        }
        
        await _unitOfWork.SaveChangesAsync();
    }
}
```

## Testing with Unit of Work

```csharp
[Test]
public async Task CreateInvoice_UpdatesNextInvoiceNumber()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var settings = new CompanySettings { NextInvoiceNumber = 1 };
    
    mockUnitOfWork.Setup(u => u.CompanySettings.GetQueryable())
        .Returns(new[] { settings }.AsQueryable());
    
    var service = new InvoiceService(mockUnitOfWork.Object);

    // Act
    await service.CreateInvoiceAsync(/* ... */);

    // Assert
    Assert.Equal(2, settings.NextInvoiceNumber);
    mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
}
```

## Performance Tips

1. **Use AsNoTracking() for read-only queries**:
   ```csharp
   var trucks = await UnitOfWork.Trucks
       .GetQueryable()
       .AsNoTracking()
       .ToListAsync();
   ```

2. **Batch SaveChanges calls**:
   ```csharp
   // ❌ Bad - Multiple saves
   foreach (var expense in expenses)
   {
       await UnitOfWork.Expenses.AddAsync(expense);
       await UnitOfWork.SaveChangesAsync(); // DON'T DO THIS
   }

   // ✅ Good - Single save
   await UnitOfWork.Expenses.AddRangeAsync(expenses);
   await UnitOfWork.SaveChangesAsync();
   ```

3. **Use projections for large datasets**:
   ```csharp
   var summaries = await UnitOfWork.Loads
       .GetQueryable()
       .Select(l => new { l.LoadNumber, l.TotalAmount })
       .ToListAsync();
   ```

## Common Scenarios

### Scenario: Record Fuel Purchase with Location
```csharp
public async Task RecordFuelAsync(string truckId, decimal gallons, decimal price)
{
    var location = await _geolocationService.GetCurrentLocationAsync();
    
    var fuelEntry = new FuelEntry
    {
        FuelEntryId = Guid.NewGuid(),
        TruckId = truckId,
        FuelDate = DateTime.UtcNow,
        Gallons = gallons,
        PricePerGallon = price,
        TotalCost = gallons * price,
        Latitude = location?.Latitude,
        Longitude = location?.Longitude
    };

    await UnitOfWork.FuelEntries.AddAsync(fuelEntry);
    
    // Also create expense record
    var expense = new Expense
    {
        ExpenseId = Guid.NewGuid(),
        TruckId = truckId,
        ExpenseDate = DateTime.UtcNow,
        Category = ExpenseCategory.Fuel,
        Amount = gallons * price
    };

    await UnitOfWork.Expenses.AddAsync(expense);
    await UnitOfWork.SaveChangesAsync(); // Both saved together
}
```

### Scenario: Generate and Send Invoice
```csharp
public async Task GenerateAndSendInvoiceAsync(Guid customerId, List<Guid> loadIds)
{
    await using var transaction = await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // Get customer
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        
        // Get loads
        var loads = await _unitOfWork.Loads
            .GetQueryable()
            .Where(l => loadIds.Contains(l.LoadId))
            .ToListAsync();

        // Create invoice
        var settings = await _unitOfWork.CompanySettings
            .GetQueryable()
            .FirstOrDefaultAsync();

        var invoice = new Invoice
        {
            InvoiceId = Guid.NewGuid(),
            InvoiceNumber = $"{settings!.InvoicePrefix}-{DateTime.UtcNow.Year}-{settings.NextInvoiceNumber:D4}",
            CustomerId = customerId,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30)
        };

        // Add line items
        foreach (var load in loads)
        {
            var lineItem = new InvoiceLineItem
            {
                InvoiceId = invoice.InvoiceId,
                LoadId = load.LoadId,
                Description = $"Load {load.LoadNumber}",
                Quantity = 1,
                UnitPrice = load.TotalAmount,
                LineTotal = load.TotalAmount
            };
            invoice.LineItems.Add(lineItem);

            // Update load status
            load.Status = LoadStatus.Invoiced;
            await _unitOfWork.Loads.UpdateAsync(load);
        }

        invoice.Subtotal = invoice.LineItems.Sum(li => li.LineTotal);
        invoice.TotalAmount = invoice.Subtotal;
        invoice.Balance = invoice.TotalAmount;

        await _unitOfWork.Invoices.AddAsync(invoice);

        // Update settings
        settings.NextInvoiceNumber++;
        await _unitOfWork.CompanySettings.UpdateAsync(settings);

        // Commit all changes
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();

        // Generate and send PDF
        var pdfBytes = await _pdfService.GenerateInvoiceAsync(invoice, settings);
        await _emailService.SendEmailAsync(
            customer.ContactEmail!,
            $"Invoice {invoice.InvoiceNumber}",
            "Please find your invoice attached.",
            pdfBytes,
            $"{invoice.InvoiceNumber}.pdf");

        invoice.Status = InvoiceStatus.Sent;
        invoice.SentDate = DateTime.UtcNow;
        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### Scenario: Dashboard with Multiple Aggregations
```csharp
public async Task<DashboardData> GetDashboardAsync()
{
    var today = DateTime.UtcNow.Date;
    var thisMonth = new DateTime(today.Year, today.Month, 1);

    return new DashboardData
    {
        // Counts
        TotalTrucks = await _unitOfWork.Trucks.GetQueryable().CountAsync(),
        ActiveDrivers = await _unitOfWork.Drivers.GetQueryable().CountAsync(d => d.IsActive),
        ActiveLoads = await _unitOfWork.Loads.GetQueryable().CountAsync(l => l.Status == LoadStatus.InTransit),
        
        // Financial
        MonthlyRevenue = await _unitOfWork.Loads
            .GetQueryable()
            .Where(l => l.Status == LoadStatus.Paid && l.DeliveryDate >= thisMonth)
            .SumAsync(l => l.TotalAmount),

        MonthlyExpenses = await _unitOfWork.Expenses
            .GetQueryable()
            .Where(e => e.ExpenseDate >= thisMonth)
            .SumAsync(e => e.Amount),

        UnpaidInvoiceAmount = await _unitOfWork.Invoices
            .GetQueryable()
            .Where(i => i.Status != InvoiceStatus.Paid)
            .SumAsync(i => i.Balance),

        // Alerts
        TrucksInShop = await _unitOfWork.Trucks.GetQueryable().CountAsync(t => t.Status == TruckStatus.InShop),
        
        ExpiringCDLs = await _unitOfWork.Drivers
            .GetQueryable()
            .Where(d => d.CDLExpiration <= DateTime.UtcNow.AddMonths(2))
            .CountAsync()
    };
}
```

## Best Practices

### ✅ DO

1. **Inject IUnitOfWork** instead of individual repositories when possible
2. **Use transactions** for operations affecting multiple entities
3. **Dispose properly** - IUnitOfWork is IDisposable (use `using` or `await using`)
4. **Batch operations** - Use AddRangeAsync/DeleteRangeAsync
5. **Leverage GetQueryable()** for complex LINQ queries

### ❌ DON'T

1. **Don't call SaveChanges multiple times** in a loop
2. **Don't mix Unit of Work instances** for related operations
3. **Don't forget to commit/rollback** transactions
4. **Don't load entire tables** - use filtering and projection
5. **Don't track entities** for read-only operations (use AsNoTracking)

## Troubleshooting

### Issue: Changes not persisted
**Solution**: Make sure you call `await _unitOfWork.SaveChangesAsync()`

### Issue: Concurrency conflicts
**Solution**: Implement optimistic concurrency with RowVersion:
```csharp
entity.Property(e => e.RowVersion).IsRowVersion();
```

### Issue: Memory leaks
**Solution**: Always dispose Unit of Work:
```csharp
await using var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
```

### Issue: Transaction deadlocks
**Solution**: Keep transactions short and commit quickly

## Migration to Unit of Work from Direct DbContext

If you have existing code using DbContext directly:

### Before:
```csharp
public class OldService
{
    private readonly TriplyDbContext _context;

    public async Task DoSomething()
    {
        var trucks = await _context.Trucks.ToListAsync();
        _context.Trucks.Add(newTruck);
        await _context.SaveChangesAsync();
    }
}
```

### After:
```csharp
public class NewService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task DoSomething()
    {
        var trucks = await _unitOfWork.Trucks.GetAllAsync();
        await _unitOfWork.Trucks.AddAsync(newTruck);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

## Summary

The Unit of Work pattern provides:
- ✅ Transaction support
- ✅ Reduced coupling
- ✅ Better testability
- ✅ Cleaner code
- ✅ Consistent data access

Use it as your primary data access pattern with specialized repositories for complex domain logic!
