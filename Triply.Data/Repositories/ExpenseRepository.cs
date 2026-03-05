using Microsoft.EntityFrameworkCore;
using Triply.Core.Models;

namespace Triply.Data.Repositories;

public class ExpenseRepository : Repository<Expense>
{
    public ExpenseRepository(TriplyDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Expense>> GetAllExpensesAsync()
    {
        return await _dbSet
            .Include(e => e.Truck)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<Expense?> GetExpenseByIdAsync(Guid expenseId)
    {
        return await _dbSet
            .Include(e => e.Truck)
            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);
    }

    public async Task<IEnumerable<Expense>> GetExpensesByTruckAsync(string truckId)
    {
        return await _dbSet
            .Where(e => e.TruckId == truckId)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<Expense> AddExpenseAsync(Expense expense)
    {
        await AddAsync(expense);
        await _context.SaveChangesAsync();
        return expense;
    }

    public async Task UpdateExpenseAsync(Expense expense)
    {
        await UpdateAsync(expense);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpenseAsync(Guid expenseId)
    {
        var expense = await _dbSet.FindAsync(expenseId);
        if (expense != null)
        {
            await DeleteAsync(expense);
            await _context.SaveChangesAsync();
        }
    }
}
