using Microsoft.EntityFrameworkCore;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Data.Repositories;

public class TruckRepository : Repository<Truck>, ITruckRepository
{
    public TruckRepository(TriplyDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Truck>> GetAllTrucksAsync()
    {
        return await _dbSet
            .Include(t => t.Drivers)
            .OrderBy(t => t.TruckId)
            .ToListAsync();
    }

    public async Task<Truck?> GetTruckByIdAsync(string truckId)
    {
        return await _dbSet
            .Include(t => t.Drivers)
            .Include(t => t.Loads)
            .Include(t => t.Expenses)
            .Include(t => t.FuelEntries)
            .Include(t => t.MaintenanceRecords)
            .FirstOrDefaultAsync(t => t.TruckId == truckId);
    }

    public async Task<Truck> AddTruckAsync(Truck truck)
    {
        truck.DateAdded = DateTime.UtcNow;
        await AddAsync(truck);
        await _context.SaveChangesAsync();
        return truck;
    }

    public async Task UpdateTruckAsync(Truck truck)
    {
        truck.LastModified = DateTime.UtcNow;
        await UpdateAsync(truck);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTruckAsync(string truckId)
    {
        var truck = await _dbSet.FindAsync(truckId);
        if (truck != null)
        {
            await DeleteAsync(truck);
            await _context.SaveChangesAsync();
        }
    }
}
