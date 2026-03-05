namespace Triply.Core.Models;

public class TaxCalendar
{
    public int Year { get; set; }
    public List<TaxDeadline> Deadlines { get; set; } = new();
    
    public List<TaxDeadline> GetUpcomingDeadlines(int daysAhead = 60)
    {
        var today = DateTime.UtcNow.Date;
        return Deadlines
            .Where(d => d.DueDate >= today && d.DueDate <= today.AddDays(daysAhead))
            .OrderBy(d => d.DueDate)
            .ToList();
    }
    
    public List<TaxDeadline> GetOverdueDeadlines()
    {
        var today = DateTime.UtcNow.Date;
        return Deadlines
            .Where(d => d.DueDate < today && !d.IsFiled)
            .OrderBy(d => d.DueDate)
            .ToList();
    }
}

public class TaxDeadline
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public TaxDeadlineType Type { get; set; }
    public bool IsFiled { get; set; }
    public DateTime? FiledDate { get; set; }
    public int DaysUntilDue => (DueDate - DateTime.UtcNow.Date).Days;
    public bool IsOverdue => DueDate < DateTime.UtcNow.Date && !IsFiled;
    public string UrgencyLevel
    {
        get
        {
            if (IsOverdue) return "Overdue";
            if (DaysUntilDue <= 7) return "Critical";
            if (DaysUntilDue <= 30) return "Important";
            return "Upcoming";
        }
    }
}

public enum TaxDeadlineType
{
    QuarterlyEstimate = 0,
    IFTA = 1,
    Form2290 = 2,
    AnnualReturn = 3,
    StateReturn = 4,
    UCR = 5, // Unified Carrier Registration
    Other = 99
}

public static class TaxCalendarFactory
{
    public static TaxCalendar Create2024Calendar()
    {
        var calendar = new TaxCalendar { Year = 2024 };
        
        // Quarterly Estimated Tax Payments (Form 1040-ES)
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q1 2024 Estimated Tax",
            Description = "Federal quarterly estimated tax payment for Q1 (Jan-Mar)",
            DueDate = new DateTime(2024, 4, 15),
            Type = TaxDeadlineType.QuarterlyEstimate
        });
        
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q2 2024 Estimated Tax",
            Description = "Federal quarterly estimated tax payment for Q2 (Apr-May)",
            DueDate = new DateTime(2024, 6, 17), // Falls on weekend, moved to Monday
            Type = TaxDeadlineType.QuarterlyEstimate
        });
        
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q3 2024 Estimated Tax",
            Description = "Federal quarterly estimated tax payment for Q3 (Jun-Aug)",
            DueDate = new DateTime(2024, 9, 16),
            Type = TaxDeadlineType.QuarterlyEstimate
        });
        
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q4 2024 Estimated Tax",
            Description = "Federal quarterly estimated tax payment for Q4 (Sep-Dec)",
            DueDate = new DateTime(2025, 1, 15),
            Type = TaxDeadlineType.QuarterlyEstimate
        });
        
        // IFTA Quarterly Returns
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q1 2024 IFTA Return",
            Description = "International Fuel Tax Agreement quarterly report",
            DueDate = new DateTime(2024, 4, 30),
            Type = TaxDeadlineType.IFTA
        });
        
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q2 2024 IFTA Return",
            Description = "International Fuel Tax Agreement quarterly report",
            DueDate = new DateTime(2024, 7, 31),
            Type = TaxDeadlineType.IFTA
        });
        
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q3 2024 IFTA Return",
            Description = "International Fuel Tax Agreement quarterly report",
            DueDate = new DateTime(2024, 10, 31),
            Type = TaxDeadlineType.IFTA
        });
        
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q4 2024 IFTA Return",
            Description = "International Fuel Tax Agreement quarterly report",
            DueDate = new DateTime(2025, 1, 31),
            Type = TaxDeadlineType.IFTA
        });
        
        // Form 2290 - Heavy Vehicle Use Tax
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Form 2290 - HVUT",
            Description = "Heavy Vehicle Use Tax for trucks 55,000+ lbs GVW",
            DueDate = new DateTime(2024, 8, 31),
            Type = TaxDeadlineType.Form2290
        });
        
        // Annual Returns
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Schedule C & Form 1040",
            Description = "Annual federal income tax return",
            DueDate = new DateTime(2025, 4, 15),
            Type = TaxDeadlineType.AnnualReturn
        });
        
        // UCR Registration
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "UCR Registration",
            Description = "Unified Carrier Registration annual registration",
            DueDate = new DateTime(2024, 12, 31),
            Type = TaxDeadlineType.UCR
        });
        
        return calendar;
    }
    
    public static TaxCalendar Create2025Calendar()
    {
        var calendar = new TaxCalendar { Year = 2025 };
        
        // Q4 2024 payments fall in 2025
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q4 2024 Estimated Tax",
            Description = "Federal quarterly estimated tax payment for Q4 2024",
            DueDate = new DateTime(2025, 1, 15),
            Type = TaxDeadlineType.QuarterlyEstimate
        });
        
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q4 2024 IFTA Return",
            Description = "International Fuel Tax Agreement quarterly report",
            DueDate = new DateTime(2025, 1, 31),
            Type = TaxDeadlineType.IFTA
        });
        
        // 2025 Q1
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q1 2025 Estimated Tax",
            Description = "Federal quarterly estimated tax payment for Q1 (Jan-Mar)",
            DueDate = new DateTime(2025, 4, 15),
            Type = TaxDeadlineType.QuarterlyEstimate
        });
        
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "2024 Tax Return",
            Description = "Annual federal income tax return for 2024",
            DueDate = new DateTime(2025, 4, 15),
            Type = TaxDeadlineType.AnnualReturn
        });
        
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q1 2025 IFTA Return",
            Description = "International Fuel Tax Agreement quarterly report",
            DueDate = new DateTime(2025, 4, 30),
            Type = TaxDeadlineType.IFTA
        });
        
        // 2025 Q2
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q2 2025 Estimated Tax",
            Description = "Federal quarterly estimated tax payment for Q2 (Apr-May)",
            DueDate = new DateTime(2025, 6, 16),
            Type = TaxDeadlineType.QuarterlyEstimate
        });
        
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q2 2025 IFTA Return",
            Description = "International Fuel Tax Agreement quarterly report",
            DueDate = new DateTime(2025, 7, 31),
            Type = TaxDeadlineType.IFTA
        });
        
        // 2025 Q3
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Form 2290 - HVUT",
            Description = "Heavy Vehicle Use Tax for trucks 55,000+ lbs GVW",
            DueDate = new DateTime(2025, 8, 31),
            Type = TaxDeadlineType.Form2290
        });
        
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q3 2025 Estimated Tax",
            Description = "Federal quarterly estimated tax payment for Q3 (Jun-Aug)",
            DueDate = new DateTime(2025, 9, 15),
            Type = TaxDeadlineType.QuarterlyEstimate
        });
        
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "Q3 2025 IFTA Return",
            Description = "International Fuel Tax Agreement quarterly report",
            DueDate = new DateTime(2025, 10, 31),
            Type = TaxDeadlineType.IFTA
        });
        
        // UCR
        calendar.Deadlines.Add(new TaxDeadline
        {
            Name = "UCR Registration",
            Description = "Unified Carrier Registration annual registration",
            DueDate = new DateTime(2025, 12, 31),
            Type = TaxDeadlineType.UCR
        });
        
        return calendar;
    }
}
