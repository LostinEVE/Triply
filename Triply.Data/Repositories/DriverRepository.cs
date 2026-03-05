using Microsoft.EntityFrameworkCore;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Data.Repositories;

public class DriverRepository : Repository<Driver>, IDriverRepository
{
    public DriverRepository(TriplyDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Driver>> GetAllDriversAsync()
    {
        return await _dbSet
            .Include(d => d.AssignedTruck)
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .ToListAsync();
    }

    public async Task<Driver?> GetDriverByIdAsync(Guid driverId)
    {
        return await _dbSet
            .Include(d => d.AssignedTruck)
            .Include(d => d.Loads)
            .FirstOrDefaultAsync(d => d.DriverId == driverId);
    }

    public async Task<Driver> AddDriverAsync(Driver driver)
    {
        await AddAsync(driver);
        await _context.SaveChangesAsync();
        return driver;
    }

    public async Task UpdateDriverAsync(Driver driver)
    {
        await UpdateAsync(driver);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDriverAsync(Guid driverId)
    {
        var driver = await _dbSet.FindAsync(driverId);
        if (driver != null)
        {
            await DeleteAsync(driver);
            await _context.SaveChangesAsync();
        }
    }
}
