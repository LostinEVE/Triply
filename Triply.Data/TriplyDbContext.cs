using Microsoft.EntityFrameworkCore;
using Triply.Core.Models;

namespace Triply.Data;

public class TriplyDbContext : DbContext
{
    public TriplyDbContext(DbContextOptions<TriplyDbContext> options) : base(options)
    {
    }

    public DbSet<Truck> Trucks { get; set; } = null!;
    public DbSet<Driver> Drivers { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Load> Loads { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<FuelEntry> FuelEntries { get; set; } = null!;
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; } = null!;
    public DbSet<TaxPayment> TaxPayments { get; set; } = null!;
    public DbSet<CompanySettings> CompanySettings { get; set; } = null!;
    public DbSet<ZipCodeLookup> ZipCodeLookups { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
    public DbSet<JournalEntryLine> JournalEntryLines { get; set; } = null!;
    public DbSet<AccountingPeriod> AccountingPeriods { get; set; } = null!;
    public DbSet<BankReconciliation> BankReconciliations { get; set; } = null!;
    public DbSet<EmailOutboxQueue> EmailOutboxQueue { get; set; } = null!;
    public DbSet<QueuedOperation> QueuedOperations { get; set; } = null!;
    public DbSet<NotificationSettings> NotificationSettings { get; set; } = null!;
    public DbSet<Subscription> Subscriptions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Subscription configuration
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.UserId);
        });

        // Truck configuration
        modelBuilder.Entity<Truck>(entity =>
        {
            entity.HasKey(e => e.TruckId);
            entity.Property(e => e.TruckId).HasMaxLength(50);
            entity.Property(e => e.Make).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
            entity.Property(e => e.VIN).HasMaxLength(17);
            entity.Property(e => e.LicensePlate).HasMaxLength(20);
            entity.Property(e => e.LicensePlateState).HasMaxLength(2);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DateAdded).IsRequired();
        });

        // Driver configuration
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.DriverId);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CDLNumber).HasMaxLength(50);
            entity.Property(e => e.CDLState).HasMaxLength(2);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.PayRate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AssignedTruckId).HasMaxLength(50);

            entity.HasOne(e => e.AssignedTruck)
                .WithMany(e => e.Drivers)
                .HasForeignKey(e => e.AssignedTruckId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContactName).HasMaxLength(100);
            entity.Property(e => e.ContactEmail).HasMaxLength(255);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.BillingAddress).HasMaxLength(200);
            entity.Property(e => e.BillingCity).HasMaxLength(100);
            entity.Property(e => e.BillingState).HasMaxLength(2);
            entity.Property(e => e.BillingZip).HasMaxLength(10);
            entity.Property(e => e.PaymentTerms).HasMaxLength(50);
        });

        // Load configuration
        modelBuilder.Entity<Load>(entity =>
        {
            entity.HasKey(e => e.LoadId);
            entity.Property(e => e.LoadNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TruckId).HasMaxLength(50);
            entity.Property(e => e.Rate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Customer)
                .WithMany(e => e.Loads)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Truck)
                .WithMany(e => e.Loads)
                .HasForeignKey(e => e.TruckId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Driver)
                .WithMany(e => e.Loads)
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Invoice configuration
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId);
            entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AmountPaid).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Customer)
                .WithMany(e => e.Invoices)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // InvoiceLineItem configuration
        modelBuilder.Entity<InvoiceLineItem>(entity =>
        {
            entity.HasKey(e => e.LineItemId);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Invoice)
                .WithMany(e => e.LineItems)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Load)
                .WithMany(e => e.InvoiceLineItems)
                .HasForeignKey(e => e.LoadId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Expense configuration
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.ExpenseId);
            entity.Property(e => e.TruckId).HasMaxLength(50);
            entity.Property(e => e.Vendor).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxCategory).HasMaxLength(100);

            entity.HasOne(e => e.Truck)
                .WithMany(e => e.Expenses)
                .HasForeignKey(e => e.TruckId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // FuelEntry configuration
        modelBuilder.Entity<FuelEntry>(entity =>
        {
            entity.HasKey(e => e.FuelEntryId);
            entity.Property(e => e.TruckId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Gallons).HasColumnType("decimal(18,3)");
            entity.Property(e => e.PricePerGallon).HasColumnType("decimal(18,3)");
            entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TruckStop).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(2);
            entity.Property(e => e.FuelCardLast4).HasMaxLength(4);
            entity.Property(e => e.IFTA_Quarter).HasMaxLength(10);

            entity.HasOne(e => e.Truck)
                .WithMany(e => e.FuelEntries)
                .HasForeignKey(e => e.TruckId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Driver)
                .WithMany(e => e.FuelEntries)
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // MaintenanceRecord configuration
        modelBuilder.Entity<MaintenanceRecord>(entity =>
        {
            entity.HasKey(e => e.MaintenanceId);
            entity.Property(e => e.TruckId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Vendor).HasMaxLength(200);
            entity.Property(e => e.LaborCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PartsCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Truck)
                .WithMany(e => e.MaintenanceRecords)
                .HasForeignKey(e => e.TruckId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TaxPayment configuration
        modelBuilder.Entity<TaxPayment>(entity =>
        {
            entity.HasKey(e => e.TaxPaymentId);
            entity.Property(e => e.AmountDue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AmountPaid).HasColumnType("decimal(18,2)");
        });

        // CompanySettings configuration
        modelBuilder.Entity<CompanySettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DBA).HasMaxLength(200);
            entity.Property(e => e.DOTNumber).HasMaxLength(50);
            entity.Property(e => e.MCNumber).HasMaxLength(50);
            entity.Property(e => e.EIN).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(2);
            entity.Property(e => e.Zip).HasMaxLength(10);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Website).HasMaxLength(255);
            entity.Property(e => e.InvoicePrefix).HasMaxLength(10);
            entity.Property(e => e.DefaultPaymentTerms).HasMaxLength(50);
            entity.Property(e => e.FederalTaxRate).HasColumnType("decimal(5,2)");
            entity.Property(e => e.StateTaxRate).HasColumnType("decimal(5,2)");
            entity.Property(e => e.SelfEmploymentTaxRate).HasColumnType("decimal(5,2)");
        });

        // ZipCodeLookup configuration
        modelBuilder.Entity<ZipCodeLookup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ZipCode);
            entity.Property(e => e.ZipCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StateAbbr).IsRequired().HasMaxLength(2);
            entity.Property(e => e.County).HasMaxLength(100);
        });

        // Account configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId);
            entity.HasIndex(e => e.AccountNumber).IsUnique();
            entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.AccountName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.ParentAccount)
                .WithMany(e => e.SubAccounts)
                .HasForeignKey(e => e.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // JournalEntry configuration
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.JournalEntryId);
            entity.HasIndex(e => e.EntryNumber).IsUnique();
            entity.HasIndex(e => e.EntryDate);
            entity.Property(e => e.EntryNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.SourceDocument).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
        });

        // JournalEntryLine configuration
        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.HasKey(e => e.JournalEntryLineId);
            entity.Property(e => e.DebitAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreditAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.JournalEntry)
                .WithMany(e => e.Lines)
                .HasForeignKey(e => e.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Account)
                .WithMany(e => e.JournalEntryLines)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AccountingPeriod configuration
        modelBuilder.Entity<AccountingPeriod>(entity =>
        {
            entity.HasKey(e => e.PeriodId);
            entity.HasIndex(e => new { e.Year, e.Month, e.Quarter }).IsUnique();
            entity.Property(e => e.ClosedBy).HasMaxLength(100);
        });

        // BankReconciliation configuration
        modelBuilder.Entity<BankReconciliation>(entity =>
        {
            entity.HasKey(e => e.ReconciliationId);
            entity.Property(e => e.StatementBeginningBalance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.StatementEndingBalance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.BookBeginningBalance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.BookEndingBalance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EmailOutboxQueue configuration
        modelBuilder.Entity<EmailOutboxQueue>(entity =>
        {
            entity.HasKey(e => e.QueueId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.Status, e.Priority, e.CreatedDate });
            entity.Property(e => e.ToEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ToName).HasMaxLength(200);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
            entity.Property(e => e.EmailType).HasMaxLength(50);
            entity.Property(e => e.AttachmentFileName).HasMaxLength(200);
            entity.Property(e => e.AttachmentContentType).HasMaxLength(100);
        });
    }
}

