# Repository Pattern and Unit of Work Usage Guide

## Generic Repository Pattern

The application uses a generic repository pattern with Unit of Work to abstract data access.

### Basic CRUD Operations

```csharp
// Inject IUnitOfWork
public class MyService
{
    private readonly IUnitOfWork _unitOfWork;

    public MyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Get all records
    public async Task<IEnumerable<Truck>> GetAllTrucks()
    {
        return await _unitOfWork.Trucks.GetAllAsync();
    }

    // Get by ID
    public async Task<Truck?> GetTruck(string truckId)
    {
        return await _unitOfWork.Trucks.GetByIdAsync(truckId);
    }

    // Add new record
    public async Task AddTruck(Truck truck)
    {
        await _unitOfWork.Trucks.AddAsync(truck);
        await _unitOfWork.SaveChangesAsync();
    }

    // Update record
    public async Task UpdateTruck(Truck truck)
    {
        await _unitOfWork.Trucks.UpdateAsync(truck);
        await _unitOfWork.SaveChangesAsync();
    }

    // Delete record
    public async Task DeleteTruck(Truck truck)
    {
        await _unitOfWork.Trucks.DeleteAsync(truck);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

### Using Predicates (Find)

```csharp
// Find active drivers
var activeDrivers = await _unitOfWork.Drivers.FindAsync(d => d.IsActive);

// Find trucks in shop
var trucksInShop = await _unitOfWork.Trucks.FindAsync(t => t.Status == TruckStatus.InShop);

// Find loads by customer
var customerLoads = await _unitOfWork.Loads.FindAsync(l => l.CustomerId == customerId);
```

### Complex Queries with GetQueryable()

```csharp
// Complex query with multiple includes and filtering
var recentLoads = await _unitOfWork.Loads
    .GetQueryable()
    .Include(l => l.Customer)
    .Include(l => l.Truck)
    .Include(l => l.Driver)
    .Where(l => l.PickupDate >= DateTime.UtcNow.AddMonths(-1))
    .OrderByDescending(l => l.PickupDate)
    .ToListAsync();

// Aggregate queries
var totalRevenue = await _unitOfWork.Loads
    .GetQueryable()
    .Where(l => l.Status == LoadStatus.Paid)
    .SumAsync(l => l.TotalAmount);

// Join queries
var driversWithTrucks = await _unitOfWork.Drivers
    .GetQueryable()
    .Include(d => d.AssignedTruck)
    .Where(d => d.AssignedTruckId != null)
    .Select(d => new { d.FirstName, d.LastName, TruckNumber = d.AssignedTruck!.TruckId })
    .ToListAsync();
```

### Transactions

```csharp
public async Task CompleteLoadAndCreateInvoice(Guid loadId)
{
    await using var transaction = await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // Get load
        var load = await _unitOfWork.Loads.GetByIdAsync(loadId);
        if (load == null)
            throw new InvalidOperationException("Load not found");

        // Update load status
        load.Status = LoadStatus.Delivered;
        await _unitOfWork.Loads.UpdateAsync(load);
        
        // Create invoice
        var invoice = new Invoice
        {
            InvoiceId = Guid.NewGuid(),
            CustomerId = load.CustomerId,
            InvoiceDate = DateTime.UtcNow,
            // ... other properties
        };
        
        await _unitOfWork.Invoices.AddAsync(invoice);
        
        // Save all changes
        await _unitOfWork.SaveChangesAsync();
        
        // Commit transaction
        await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### Batch Operations

```csharp
// Add multiple records
var expenses = new List<Expense>
{
    new Expense { /* ... */ },
    new Expense { /* ... */ }
};

await _unitOfWork.Expenses.AddRangeAsync(expenses);
await _unitOfWork.SaveChangesAsync();

// Delete multiple records
var oldRecords = await _unitOfWork.Expenses.FindAsync(e => 
    e.ExpenseDate < DateTime.UtcNow.AddYears(-7));

await _unitOfWork.Expenses.DeleteRangeAsync(oldRecords);
await _unitOfWork.SaveChangesAsync();
```

## Specialized Repositories

While the Unit of Work provides generic access, specialized repositories are still available for complex operations:

```csharp
// Inject specialized repository
public class MyComponent
{
    private readonly ITruckRepository _truckRepository;
    private readonly IFTAReportService _iftaService;

    public MyComponent(ITruckRepository truckRepository, IFTAReportService iftaService)
    {
        _truckRepository = truckRepository;
        _iftaService = iftaService;
    }

    public async Task GenerateTruckReport(string truckId)
    {
        var truck = await _truckRepository.GetTruckByIdAsync(truckId);
        // ... includes all related data
    }
}
```

## Benefits

1. **Reduced Code Duplication**: Common CRUD operations are centralized
2. **Transaction Support**: Easy management of multi-step operations
3. **Testability**: Easy to mock IUnitOfWork and IRepository<T>
4. **Flexibility**: Complex queries via GetQueryable() + specialized repositories for specific needs
5. **Consistency**: Standardized data access patterns across the application
