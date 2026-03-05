using Microsoft.EntityFrameworkCore;
using Triply.Core.Models;

namespace Triply.Data.Repositories;

public class MaintenanceRepository : Repository<MaintenanceRecord>
{
    public MaintenanceRepository(TriplyDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetMaintenanceByTruckAsync(string truckId)
    {
        return await _dbSet
            .Include(m => m.Truck)
            .Where(m => m.TruckId == truckId)
            .OrderByDescending(m => m.MaintenanceDate)
            .ToListAsync();
    }

    public async Task<MaintenanceRecord?> GetMaintenanceByIdAsync(Guid maintenanceId)
    {
        return await _dbSet
            .Include(m => m.Truck)
            .FirstOrDefaultAsync(m => m.MaintenanceId == maintenanceId);
    }

    public async Task<MaintenanceRecord> AddMaintenanceAsync(MaintenanceRecord maintenance)
    {
        await AddAsync(maintenance);
        await _context.SaveChangesAsync();
        return maintenance;
    }

    public async Task UpdateMaintenanceAsync(MaintenanceRecord maintenance)
    {
        await UpdateAsync(maintenance);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMaintenanceAsync(Guid maintenanceId)
    {
        var maintenance = await _dbSet.FindAsync(maintenanceId);
        if (maintenance != null)
        {
            await DeleteAsync(maintenance);
            await _context.SaveChangesAsync();
        }
    }
}
