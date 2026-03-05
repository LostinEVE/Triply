using Microsoft.EntityFrameworkCore;
using Triply.Core.Models;

namespace Triply.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(TriplyDbContext context)
    {
        await context.Database.MigrateAsync();

        // Seed default company settings
        if (!await context.CompanySettings.AnyAsync())
        {
            var defaultSettings = new CompanySettings
            {
                CompanyName = "Your Trucking Company",
                InvoicePrefix = "INV",
                NextInvoiceNumber = 1,
                DefaultPaymentTerms = "Net30",
                FederalTaxRate = 21.00m,
                StateTaxRate = 0.00m,
                SelfEmploymentTaxRate = 15.30m,
                FiscalYearStart = 1
            };

            context.CompanySettings.Add(defaultSettings);
            await context.SaveChangesAsync();
        }

        // Seed zip code lookup data for offline support
        await ZipCodeSeeder.SeedCommonZipCodesAsync(context);

        // Seed chart of accounts
        await ChartOfAccountsSeeder.SeedChartOfAccountsAsync(context);
    }
}
