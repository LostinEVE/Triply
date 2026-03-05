using Microsoft.EntityFrameworkCore;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Data.Repositories;

public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(TriplyDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
    {
        return await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
                .ThenInclude(li => li.Load)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId)
    {
        return await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
                .ThenInclude(li => li.Load)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
    }

    public async Task<Invoice> AddInvoiceAsync(Invoice invoice)
    {
        await AddAsync(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task UpdateInvoiceAsync(Invoice invoice)
    {
        await UpdateAsync(invoice);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _dbSet.FindAsync(invoiceId);
        if (invoice != null)
        {
            await DeleteAsync(invoice);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var settings = await _context.CompanySettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            return $"INV-{DateTime.UtcNow.Year}-0001";
        }

        var invoiceNumber = $"{settings.InvoicePrefix}-{DateTime.UtcNow.Year}-{settings.NextInvoiceNumber:D4}";
        settings.NextInvoiceNumber++;
        await _context.SaveChangesAsync();

        return invoiceNumber;
    }
}
