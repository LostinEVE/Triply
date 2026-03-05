namespace Triply.Core.Models;

public class ZipCodeLookup
{
    public int Id { get; set; }
    public string ZipCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string StateAbbr { get; set; } = string.Empty;
    public string? County { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
