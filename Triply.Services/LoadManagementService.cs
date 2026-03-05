using Triply.Core.Interfaces;
using Triply.Core.Models;
using Triply.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Triply.Services;

public class LoadManagementService
{
    private readonly IUnitOfWork _unitOfWork;

    public LoadManagementService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Load> CreateLoadAsync(Load load)
    {
        await _unitOfWork.Loads.AddAsync(load);
        await _unitOfWork.SaveChangesAsync();
        return load;
    }

    public async Task<Invoice> CreateInvoiceFromLoadAsync(Guid loadId)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            var load = await _unitOfWork.Loads
                .GetQueryable()
                .Include(l => l.Customer)
                .FirstOrDefaultAsync(l => l.LoadId == loadId);

            if (load == null)
                throw new InvalidOperationException("Load not found");

            var settings = await _unitOfWork.CompanySettings
                .GetQueryable()
                .FirstOrDefaultAsync();

            if (settings == null)
                throw new InvalidOperationException("Company settings not configured");

            var invoiceNumber = $"{settings.InvoicePrefix}-{DateTime.UtcNow.Year}-{settings.NextInvoiceNumber:D4}";
            settings.NextInvoiceNumber++;
            await _unitOfWork.CompanySettings.UpdateAsync(settings);

            var invoice = new Invoice
            {
                InvoiceId = Guid.NewGuid(),
                InvoiceNumber = invoiceNumber,
                CustomerId = load.CustomerId,
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                Status = InvoiceStatus.Draft
            };

            var lineItem = new InvoiceLineItem
            {
                InvoiceId = invoice.InvoiceId,
                LoadId = load.LoadId,
                Description = $"Load {load.LoadNumber}: {load.PickupCity}, {load.PickupState} to {load.DeliveryCity}, {load.DeliveryState}",
                Quantity = 1,
                UnitPrice = load.TotalAmount,
                LineTotal = load.TotalAmount
            };

            invoice.LineItems.Add(lineItem);
            invoice.Subtotal = load.TotalAmount;
            invoice.TaxAmount = 0;
            invoice.TotalAmount = load.TotalAmount;
            invoice.Balance = load.TotalAmount;

            await _unitOfWork.Invoices.AddAsync(invoice);

            load.Status = LoadStatus.Invoiced;
            await _unitOfWork.Loads.UpdateAsync(load);

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

    public async Task<IEnumerable<Load>> GetUnassignedLoadsAsync()
    {
        return await _unitOfWork.Loads.FindAsync(l => 
            l.DriverId == null || l.TruckId == null);
    }

    public async Task AssignLoadToTruckAndDriverAsync(Guid loadId, string truckId, Guid driverId)
    {
        var load = await _unitOfWork.Loads.GetByIdAsync(loadId);
        if (load == null)
            throw new InvalidOperationException("Load not found");

        load.TruckId = truckId;
        load.DriverId = driverId;
        load.Status = LoadStatus.Booked;

        await _unitOfWork.Loads.UpdateAsync(load);
        await _unitOfWork.SaveChangesAsync();
    }
}
