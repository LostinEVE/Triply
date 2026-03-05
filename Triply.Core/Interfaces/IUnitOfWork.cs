using Microsoft.EntityFrameworkCore.Storage;
using Triply.Core.Models;

namespace Triply.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repository properties
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
    IRepository<ZipCodeLookup> ZipCodeLookups { get; }
    IRepository<EmailOutboxQueue> EmailOutboxQueue { get; }
    IRepository<QueuedOperation> QueuedOperations { get; }
    IRepository<NotificationSettings> NotificationSettings { get; }

    // Unit of Work methods
    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
