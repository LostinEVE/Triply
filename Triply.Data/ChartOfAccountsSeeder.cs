using Microsoft.EntityFrameworkCore;
using Triply.Core.Models;

namespace Triply.Data;

public static class ChartOfAccountsSeeder
{
    public static async Task SeedChartOfAccountsAsync(TriplyDbContext context)
    {
        if (await context.Accounts.AnyAsync())
            return;

        var accounts = new List<Account>
        {
            // ===== ASSETS =====
            
            // Current Assets
            new() { AccountNumber = "1000", AccountName = "Cash", AccountType = AccountType.Asset, Category = AccountCategory.CurrentAsset, IsSystemAccount = true, Description = "Checking and savings accounts" },
            new() { AccountNumber = "1010", AccountName = "Petty Cash", AccountType = AccountType.Asset, Category = AccountCategory.CurrentAsset, Description = "Cash on hand" },
            new() { AccountNumber = "1100", AccountName = "Accounts Receivable", AccountType = AccountType.Asset, Category = AccountCategory.CurrentAsset, IsSystemAccount = true, Description = "Outstanding invoices" },
            new() { AccountNumber = "1200", AccountName = "Fuel Advances", AccountType = AccountType.Asset, Category = AccountCategory.CurrentAsset, Description = "Prepaid fuel cards" },
            new() { AccountNumber = "1210", AccountName = "Prepaid Insurance", AccountType = AccountType.Asset, Category = AccountCategory.CurrentAsset, Description = "Insurance paid in advance" },
            new() { AccountNumber = "1220", AccountName = "Prepaid Expenses", AccountType = AccountType.Asset, Category = AccountCategory.CurrentAsset, Description = "Other prepaid expenses" },
            
            // Fixed Assets
            new() { AccountNumber = "1500", AccountName = "Trucks", AccountType = AccountType.Asset, Category = AccountCategory.FixedAsset, IsSystemAccount = true, Description = "Tractor units at cost" },
            new() { AccountNumber = "1510", AccountName = "Accumulated Depreciation - Trucks", AccountType = AccountType.Asset, Category = AccountCategory.FixedAsset, Description = "Contra-asset for truck depreciation" },
            new() { AccountNumber = "1520", AccountName = "Trailers", AccountType = AccountType.Asset, Category = AccountCategory.FixedAsset, Description = "Trailers at cost" },
            new() { AccountNumber = "1530", AccountName = "Accumulated Depreciation - Trailers", AccountType = AccountType.Asset, Category = AccountCategory.FixedAsset, Description = "Contra-asset for trailer depreciation" },
            new() { AccountNumber = "1540", AccountName = "Equipment", AccountType = AccountType.Asset, Category = AccountCategory.FixedAsset, Description = "Office equipment, computers, etc." },
            new() { AccountNumber = "1550", AccountName = "Accumulated Depreciation - Equipment", AccountType = AccountType.Asset, Category = AccountCategory.FixedAsset, Description = "Contra-asset for equipment depreciation" },
            
            // Other Assets
            new() { AccountNumber = "1800", AccountName = "Deposits", AccountType = AccountType.Asset, Category = AccountCategory.OtherAsset, Description = "Security deposits" },
            
            // ===== LIABILITIES =====
            
            // Current Liabilities
            new() { AccountNumber = "2000", AccountName = "Accounts Payable", AccountType = AccountType.Liability, Category = AccountCategory.CurrentLiability, IsSystemAccount = true, Description = "Amounts owed to vendors" },
            new() { AccountNumber = "2010", AccountName = "Credit Cards Payable", AccountType = AccountType.Liability, Category = AccountCategory.CurrentLiability, Description = "Business credit card balances" },
            new() { AccountNumber = "2020", AccountName = "Fuel Card Payable", AccountType = AccountType.Liability, Category = AccountCategory.CurrentLiability, Description = "Fuel card balances" },
            new() { AccountNumber = "2100", AccountName = "Sales Tax Payable", AccountType = AccountType.Liability, Category = AccountCategory.CurrentLiability, Description = "Sales tax collected" },
            new() { AccountNumber = "2110", AccountName = "IFTA Payable", AccountType = AccountType.Liability, Category = AccountCategory.CurrentLiability, IsSystemAccount = true, Description = "IFTA fuel tax liability" },
            new() { AccountNumber = "2120", AccountName = "Payroll Taxes Payable", AccountType = AccountType.Liability, Category = AccountCategory.CurrentLiability, Description = "Withholding and employer taxes" },
            new() { AccountNumber = "2200", AccountName = "Current Portion - Truck Loans", AccountType = AccountType.Liability, Category = AccountCategory.CurrentLiability, Description = "Truck loan due within 12 months" },
            
            // Long-Term Liabilities
            new() { AccountNumber = "2500", AccountName = "Truck Loans Payable", AccountType = AccountType.Liability, Category = AccountCategory.LongTermLiability, IsSystemAccount = true, Description = "Long-term truck financing" },
            new() { AccountNumber = "2510", AccountName = "Trailer Loans Payable", AccountType = AccountType.Liability, Category = AccountCategory.LongTermLiability, Description = "Long-term trailer financing" },
            new() { AccountNumber = "2520", AccountName = "Equipment Loans Payable", AccountType = AccountType.Liability, Category = AccountCategory.LongTermLiability, Description = "Long-term equipment financing" },
            
            // ===== EQUITY =====
            
            new() { AccountNumber = "3000", AccountName = "Owner's Equity", AccountType = AccountType.Equity, Category = AccountCategory.OwnersEquity, IsSystemAccount = true, Description = "Owner's capital" },
            new() { AccountNumber = "3010", AccountName = "Owner's Draw", AccountType = AccountType.Equity, Category = AccountCategory.OwnersEquity, Description = "Owner withdrawals" },
            new() { AccountNumber = "3900", AccountName = "Retained Earnings", AccountType = AccountType.Equity, Category = AccountCategory.RetainedEarnings, IsSystemAccount = true, Description = "Accumulated profits/losses" },
            
            // ===== REVENUE =====
            
            new() { AccountNumber = "4000", AccountName = "Freight Revenue", AccountType = AccountType.Revenue, Category = AccountCategory.OperatingRevenue, IsSystemAccount = true, Description = "Revenue from hauling freight" },
            new() { AccountNumber = "4010", AccountName = "Detention Pay", AccountType = AccountType.Revenue, Category = AccountCategory.OperatingRevenue, Description = "Detention and waiting time charges" },
            new() { AccountNumber = "4020", AccountName = "Fuel Surcharge", AccountType = AccountType.Revenue, Category = AccountCategory.OperatingRevenue, Description = "Fuel surcharge revenue" },
            new() { AccountNumber = "4030", AccountName = "Accessorial Charges", AccountType = AccountType.Revenue, Category = AccountCategory.OperatingRevenue, Description = "Other accessorial revenue" },
            new() { AccountNumber = "4900", AccountName = "Other Income", AccountType = AccountType.Revenue, Category = AccountCategory.OtherRevenue, Description = "Miscellaneous income" },
            
            // ===== EXPENSES =====
            
            // Fuel
            new() { AccountNumber = "5000", AccountName = "Fuel - Diesel", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, IsSystemAccount = true, Description = "Diesel fuel purchases" },
            new() { AccountNumber = "5010", AccountName = "Fuel - DEF", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Diesel Exhaust Fluid" },
            
            // Maintenance & Repairs
            new() { AccountNumber = "5100", AccountName = "Maintenance - Preventive", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, IsSystemAccount = true, Description = "Scheduled maintenance" },
            new() { AccountNumber = "5110", AccountName = "Repairs - Truck", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Truck repairs" },
            new() { AccountNumber = "5120", AccountName = "Repairs - Trailer", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Trailer repairs" },
            new() { AccountNumber = "5130", AccountName = "Tires", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, IsSystemAccount = true, Description = "Tire purchases and repairs" },
            
            // Driver Expenses
            new() { AccountNumber = "5200", AccountName = "Driver Wages", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, IsSystemAccount = true, Description = "Driver compensation" },
            new() { AccountNumber = "5210", AccountName = "Driver Per Diem", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Per diem payments to drivers" },
            new() { AccountNumber = "5220", AccountName = "Payroll Taxes", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Employer payroll taxes" },
            
            // Insurance
            new() { AccountNumber = "5300", AccountName = "Truck Insurance", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, IsSystemAccount = true, Description = "Liability and physical damage insurance" },
            new() { AccountNumber = "5310", AccountName = "Cargo Insurance", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Cargo insurance" },
            new() { AccountNumber = "5320", AccountName = "Health Insurance", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Health insurance premiums" },
            
            // Permits & Licenses
            new() { AccountNumber = "5400", AccountName = "Permits & Licenses", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, IsSystemAccount = true, Description = "DOT permits, UCR, etc." },
            new() { AccountNumber = "5410", AccountName = "IFTA Tax", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, IsSystemAccount = true, Description = "IFTA fuel tax expense" },
            new() { AccountNumber = "5420", AccountName = "Heavy Vehicle Use Tax", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Form 2290 tax" },
            
            // Road Expenses
            new() { AccountNumber = "5500", AccountName = "Tolls", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, IsSystemAccount = true, Description = "Road and bridge tolls" },
            new() { AccountNumber = "5510", AccountName = "Parking", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Truck parking fees" },
            new() { AccountNumber = "5520", AccountName = "Scales", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Scale and weigh station fees" },
            new() { AccountNumber = "5530", AccountName = "Lumper Fees", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Loading/unloading fees" },
            
            // Office & Administrative
            new() { AccountNumber = "5600", AccountName = "Office Supplies", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Office supplies and materials" },
            new() { AccountNumber = "5610", AccountName = "Professional Fees", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Accounting, legal fees" },
            new() { AccountNumber = "5620", AccountName = "Software & Subscriptions", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Software subscriptions" },
            new() { AccountNumber = "5630", AccountName = "Telephone & Internet", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Communication expenses" },
            new() { AccountNumber = "5640", AccountName = "Advertising & Marketing", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Marketing costs" },
            
            // Truck Financing
            new() { AccountNumber = "5700", AccountName = "Interest - Truck Loans", AccountType = AccountType.Expense, Category = AccountCategory.OtherExpense, IsSystemAccount = true, Description = "Interest on truck financing" },
            new() { AccountNumber = "5710", AccountName = "Interest - Credit Cards", AccountType = AccountType.Expense, Category = AccountCategory.OtherExpense, Description = "Credit card interest" },
            new() { AccountNumber = "5720", AccountName = "Bank Fees", AccountType = AccountType.Expense, Category = AccountCategory.OtherExpense, Description = "Banking fees" },
            
            // Depreciation
            new() { AccountNumber = "5800", AccountName = "Depreciation - Trucks", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, IsSystemAccount = true, Description = "Truck depreciation expense" },
            new() { AccountNumber = "5810", AccountName = "Depreciation - Trailers", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Trailer depreciation expense" },
            new() { AccountNumber = "5820", AccountName = "Depreciation - Equipment", AccountType = AccountType.Expense, Category = AccountCategory.OperatingExpense, Description = "Equipment depreciation expense" },
            
            // Other Expenses
            new() { AccountNumber = "5900", AccountName = "Miscellaneous Expense", AccountType = AccountType.Expense, Category = AccountCategory.OtherExpense, Description = "Other expenses" },
        };

        await context.Accounts.AddRangeAsync(accounts);
        await context.SaveChangesAsync();
    }
}
