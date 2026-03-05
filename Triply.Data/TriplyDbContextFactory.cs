using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Triply.Data;

public class TriplyDbContextFactory : IDesignTimeDbContextFactory<TriplyDbContext>
{
    public TriplyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TriplyDbContext>();
        optionsBuilder.UseSqlite("Data Source=triply.db");

        return new TriplyDbContext(optionsBuilder.Options);
    }
}
