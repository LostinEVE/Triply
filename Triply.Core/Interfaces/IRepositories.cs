using Triply.Core.Models;

namespace Triply.Core.Interfaces;

public interface ITruckRepository
{
    Task<IEnumerable<Truck>> GetAllTrucksAsync();
    Task<Truck?> GetTruckByIdAsync(string truckId);
    Task<Truck> AddTruckAsync(Truck truck);
    Task UpdateTruckAsync(Truck truck);
    Task DeleteTruckAsync(string truckId);
}

public interface IDriverRepository
{
    Task<IEnumerable<Driver>> GetAllDriversAsync();
    Task<Driver?> GetDriverByIdAsync(Guid driverId);
    Task<Driver> AddDriverAsync(Driver driver);
    Task UpdateDriverAsync(Driver driver);
    Task DeleteDriverAsync(Guid driverId);
}

public interface ILoadRepository
{
    Task<IEnumerable<Load>> GetAllLoadsAsync();
    Task<Load?> GetLoadByIdAsync(Guid loadId);
    Task<Load> AddLoadAsync(Load load);
    Task UpdateLoadAsync(Load load);
    Task DeleteLoadAsync(Guid loadId);
}

public interface IInvoiceRepository
{
    Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
    Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId);
    Task<Invoice> AddInvoiceAsync(Invoice invoice);
    Task UpdateInvoiceAsync(Invoice invoice);
    Task DeleteInvoiceAsync(Guid invoiceId);
    Task<string> GenerateInvoiceNumberAsync();
}
