using Microsoft.EntityFrameworkCore;
using Triply.Core.Models;

namespace Triply.Data.Repositories;

public class CustomerRepository : Repository<Customer>
{
    public CustomerRepository(TriplyDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _dbSet
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(Guid customerId)
    {
        return await _dbSet
            .Include(c => c.Loads)
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);
    }

    public async Task<Customer> AddCustomerAsync(Customer customer)
    {
        await AddAsync(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        await UpdateAsync(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCustomerAsync(Guid customerId)
    {
        var customer = await _dbSet.FindAsync(customerId);
        if (customer != null)
        {
            await DeleteAsync(customer);
            await _context.SaveChangesAsync();
        }
    }
}
