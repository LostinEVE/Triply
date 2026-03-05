namespace Triply.Core.Models;

public class IFTAQuarterlyReport
{
    public int Year { get; set; }
    public int Quarter { get; set; }
    public string QuarterLabel => $"Q{Quarter} {Year}";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Fleet Information
    public string? VIN { get; set; }
    public string? TruckId { get; set; }
    
    // State-by-State Data
    public List<IFTAStateData> StateData { get; set; } = new();
    
    // Summary Totals
    public int TotalMiles => StateData.Sum(s => s.MilesDriven);
    public decimal TotalGallons => StateData.Sum(s => s.GallonsPurchased);
    public decimal TotalTaxOwed => StateData.Sum(s => s.NetTaxOwed);
    public decimal TotalTaxCredit => StateData.Sum(s => s.NetTaxCredit);
    public decimal NetIFTATax => TotalTaxOwed - TotalTaxCredit;
    
    // Filing Information
    public DateTime FilingDeadline { get; set; }
    public bool IsFiled { get; set; }
    public DateTime? FiledDate { get; set; }
    
    // Average MPG
    public double FleetMPG => TotalGallons > 0 ? (double)TotalMiles / (double)TotalGallons : 0;
}

public class IFTAStateData
{
    public string State { get; set; } = string.Empty;
    public string StateFullName { get; set; } = string.Empty;
    
    // Miles
    public int MilesDriven { get; set; }
    public int TaxableMiles { get; set; }
    public int NonTaxableMiles { get; set; }
    
    // Fuel
    public decimal GallonsPurchased { get; set; }
    public decimal TaxPaidOnPurchase { get; set; }
    
    // Tax Rates
    public decimal StateFuelTaxRate { get; set; } // cents per gallon
    
    // Calculations
    public decimal TaxableGallons => MilesDriven > 0 && FleetMPG > 0 ? 
        MilesDriven / (decimal)FleetMPG : 0;
    
    public double FleetMPG { get; set; } // Passed from parent report
    
    public decimal TaxOwed => TaxableGallons * (StateFuelTaxRate / 100);
    public decimal TaxPaid => TaxPaidOnPurchase;
    public decimal NetTaxOwed => Math.Max(0, TaxOwed - TaxPaid);
    public decimal NetTaxCredit => Math.Max(0, TaxPaid - TaxOwed);
    
    // Status
    public bool IsJurisdiction { get; set; } // Base jurisdiction or not
}

public class IFTATaxRates
{
    public static Dictionary<string, decimal> GetStateFuelTaxRates()
    {
        // 2024 diesel fuel tax rates (cents per gallon) - Updated annually
        return new Dictionary<string, decimal>
        {
            { "AL", 27.0m }, { "AK", 8.95m }, { "AZ", 26.0m }, { "AR", 24.5m },
            { "CA", 53.5m }, { "CO", 20.5m }, { "CT", 49.2m }, { "DE", 22.0m },
            { "FL", 23.9m }, { "GA", 32.6m }, { "HI", 16.0m }, { "ID", 32.0m },
            { "IL", 54.5m }, { "IN", 52.0m }, { "IA", 32.5m }, { "KS", 26.0m },
            { "KY", 27.7m }, { "LA", 20.0m }, { "ME", 31.2m }, { "MD", 36.1m },
            { "MA", 24.0m }, { "MI", 28.6m }, { "MN", 28.5m }, { "MS", 18.4m },
            { "MO", 17.0m }, { "MT", 32.75m }, { "NE", 27.4m }, { "NV", 27.75m },
            { "NH", 22.2m }, { "NJ", 35.0m }, { "NM", 21.88m }, { "NY", 45.74m },
            { "NC", 36.2m }, { "ND", 23.0m }, { "OH", 47.0m }, { "OK", 20.0m },
            { "OR", 36.0m }, { "PA", 75.2m }, { "RI", 35.0m }, { "SC", 26.75m },
            { "SD", 30.0m }, { "TN", 27.4m }, { "TX", 20.0m }, { "UT", 31.4m },
            { "VT", 30.0m }, { "VA", 27.5m }, { "WA", 49.4m }, { "WV", 35.7m },
            { "WI", 32.9m }, { "WY", 24.0m }, { "DC", 23.5m }
        };
    }
    
    public static Dictionary<string, string> GetStateFullNames()
    {
        return new Dictionary<string, string>
        {
            { "AL", "Alabama" }, { "AK", "Alaska" }, { "AZ", "Arizona" }, { "AR", "Arkansas" },
            { "CA", "California" }, { "CO", "Colorado" }, { "CT", "Connecticut" }, { "DE", "Delaware" },
            { "FL", "Florida" }, { "GA", "Georgia" }, { "HI", "Hawaii" }, { "ID", "Idaho" },
            { "IL", "Illinois" }, { "IN", "Indiana" }, { "IA", "Iowa" }, { "KS", "Kansas" },
            { "KY", "Kentucky" }, { "LA", "Louisiana" }, { "ME", "Maine" }, { "MD", "Maryland" },
            { "MA", "Massachusetts" }, { "MI", "Michigan" }, { "MN", "Minnesota" }, { "MS", "Mississippi" },
            { "MO", "Missouri" }, { "MT", "Montana" }, { "NE", "Nebraska" }, { "NV", "Nevada" },
            { "NH", "New Hampshire" }, { "NJ", "New Jersey" }, { "NM", "New Mexico" }, { "NY", "New York" },
            { "NC", "North Carolina" }, { "ND", "North Dakota" }, { "OH", "Ohio" }, { "OK", "Oklahoma" },
            { "OR", "Oregon" }, { "PA", "Pennsylvania" }, { "RI", "Rhode Island" }, { "SC", "South Carolina" },
            { "SD", "South Dakota" }, { "TN", "Tennessee" }, { "TX", "Texas" }, { "UT", "Utah" },
            { "VT", "Vermont" }, { "VA", "Virginia" }, { "WA", "Washington" }, { "WV", "West Virginia" },
            { "WI", "Wisconsin" }, { "WY", "Wyoming" }, { "DC", "District of Columbia" }
        };
    }
}
