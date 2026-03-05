namespace Triply.Core.Models;

public record LocationInfo
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? County { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public DateTime Timestamp { get; set; }
    public double? Accuracy { get; set; }
    public LocationSource Source { get; set; }
}

public enum LocationSource
{
    GPS = 0,
    Network = 1,
    Cached = 2,
    Manual = 3,
    ZipCodeLookup = 4
}
