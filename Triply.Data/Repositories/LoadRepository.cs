using Microsoft.EntityFrameworkCore;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Data.Repositories;

public class LoadRepository : Repository<Load>, ILoadRepository
{
    public LoadRepository(TriplyDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Load>> GetAllLoadsAsync()
    {
        return await _dbSet
            .Include(l => l.Customer)
            .Include(l => l.Truck)
            .Include(l => l.Driver)
            .OrderByDescending(l => l.PickupDate)
            .ToListAsync();
    }

    public async Task<Load?> GetLoadByIdAsync(Guid loadId)
    {
        return await _dbSet
            .Include(l => l.Customer)
            .Include(l => l.Truck)
            .Include(l => l.Driver)
            .Include(l => l.InvoiceLineItems)
            .FirstOrDefaultAsync(l => l.LoadId == loadId);
    }

    public async Task<Load> AddLoadAsync(Load load)
    {
        await AddAsync(load);
        await _context.SaveChangesAsync();
        return load;
    }

    public async Task UpdateLoadAsync(Load load)
    {
        await UpdateAsync(load);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteLoadAsync(Guid loadId)
    {
        var load = await _dbSet.FindAsync(loadId);
        if (load != null)
        {
            await DeleteAsync(load);
            await _context.SaveChangesAsync();
        }
    }
}
