using Microsoft.EntityFrameworkCore;
using Triply.Core.Models;

namespace Triply.Data.Repositories;

public class FuelEntryRepository : Repository<FuelEntry>
{
    public FuelEntryRepository(TriplyDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<FuelEntry>> GetFuelEntriesByTruckAsync(string truckId)
    {
        return await _dbSet
            .Include(f => f.Truck)
            .Include(f => f.Driver)
            .Where(f => f.TruckId == truckId)
            .OrderByDescending(f => f.FuelDate)
            .ToListAsync();
    }

    public async Task<FuelEntry?> GetFuelEntryByIdAsync(Guid fuelEntryId)
    {
        return await _dbSet
            .Include(f => f.Truck)
            .Include(f => f.Driver)
            .FirstOrDefaultAsync(f => f.FuelEntryId == fuelEntryId);
    }

    public async Task<FuelEntry> AddFuelEntryAsync(FuelEntry fuelEntry)
    {
        fuelEntry.IFTA_Quarter = CalculateIFTAQuarter(fuelEntry.FuelDate);
        await AddAsync(fuelEntry);
        await _context.SaveChangesAsync();
        return fuelEntry;
    }

    public async Task UpdateFuelEntryAsync(FuelEntry fuelEntry)
    {
        fuelEntry.IFTA_Quarter = CalculateIFTAQuarter(fuelEntry.FuelDate);
        await UpdateAsync(fuelEntry);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFuelEntryAsync(Guid fuelEntryId)
    {
        var fuelEntry = await _dbSet.FindAsync(fuelEntryId);
        if (fuelEntry != null)
        {
            await DeleteAsync(fuelEntry);
            await _context.SaveChangesAsync();
        }
    }

    private static string CalculateIFTAQuarter(DateTime date)
    {
        var quarter = (date.Month - 1) / 3 + 1;
        return $"Q{quarter}-{date.Year}";
    }
}
