using Microsoft.EntityFrameworkCore.Storage;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly TriplyDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy initialization of repositories
    private IRepository<Truck>? _trucks;
    private IRepository<Driver>? _drivers;
    private IRepository<Customer>? _customers;
    private IRepository<Load>? _loads;
    private IRepository<Invoice>? _invoices;
    private IRepository<InvoiceLineItem>? _invoiceLineItems;
    private IRepository<Expense>? _expenses;
    private IRepository<FuelEntry>? _fuelEntries;
    private IRepository<MaintenanceRecord>? _maintenanceRecords;
    private IRepository<TaxPayment>? _taxPayments;
    private IRepository<CompanySettings>? _companySettings;
    private IRepository<ZipCodeLookup>? _zipCodeLookups;
    private IRepository<EmailOutboxQueue>? _emailOutboxQueue;
    private IRepository<QueuedOperation>? _queuedOperations;
    private IRepository<NotificationSettings>? _notificationSettings;

    public UnitOfWork(TriplyDbContext context)
    {
        _context = context;
    }

    // Repository properties with lazy initialization
    public IRepository<Truck> Trucks =>
        _trucks ??= new Repositories.Repository<Truck>(_context);

    public IRepository<Driver> Drivers =>
        _drivers ??= new Repositories.Repository<Driver>(_context);

    public IRepository<Customer> Customers =>
        _customers ??= new Repositories.Repository<Customer>(_context);

    public IRepository<Load> Loads =>
        _loads ??= new Repositories.Repository<Load>(_context);

    public IRepository<Invoice> Invoices =>
        _invoices ??= new Repositories.Repository<Invoice>(_context);

    public IRepository<InvoiceLineItem> InvoiceLineItems =>
        _invoiceLineItems ??= new Repositories.Repository<InvoiceLineItem>(_context);

    public IRepository<Expense> Expenses =>
        _expenses ??= new Repositories.Repository<Expense>(_context);

    public IRepository<FuelEntry> FuelEntries =>
        _fuelEntries ??= new Repositories.Repository<FuelEntry>(_context);

    public IRepository<MaintenanceRecord> MaintenanceRecords =>
        _maintenanceRecords ??= new Repositories.Repository<MaintenanceRecord>(_context);

    public IRepository<TaxPayment> TaxPayments =>
        _taxPayments ??= new Repositories.Repository<TaxPayment>(_context);

    public IRepository<CompanySettings> CompanySettings =>
        _companySettings ??= new Repositories.Repository<CompanySettings>(_context);

    public IRepository<ZipCodeLookup> ZipCodeLookups =>
        _zipCodeLookups ??= new Repositories.Repository<ZipCodeLookup>(_context);

    public IRepository<EmailOutboxQueue> EmailOutboxQueue =>
        _emailOutboxQueue ??= new Repositories.Repository<EmailOutboxQueue>(_context);

    public IRepository<QueuedOperation> QueuedOperations =>
        _queuedOperations ??= new Repositories.Repository<QueuedOperation>(_context);

    public IRepository<NotificationSettings> NotificationSettings =>
        _notificationSettings ??= new Repositories.Repository<NotificationSettings>(_context);

    // Unit of Work methods
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        return _transaction;
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        try
        {
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
