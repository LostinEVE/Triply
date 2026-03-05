using Microsoft.EntityFrameworkCore;
using Triply.Core.Models;

namespace Triply.Data;

public static class ZipCodeSeeder
{
    public static async Task SeedCommonZipCodesAsync(TriplyDbContext context)
    {
        if (await context.ZipCodeLookups.AnyAsync())
            return;

        // Seed with common US trucking hub cities
        var zipCodes = new List<ZipCodeLookup>
        {
            // Major Trucking Hubs
            new() { ZipCode = "60601", City = "Chicago", State = "Illinois", StateAbbr = "IL", County = "Cook", Latitude = 41.8781, Longitude = -87.6298 },
            new() { ZipCode = "75201", City = "Dallas", State = "Texas", StateAbbr = "TX", County = "Dallas", Latitude = 32.7767, Longitude = -96.7970 },
            new() { ZipCode = "90001", City = "Los Angeles", State = "California", StateAbbr = "CA", County = "Los Angeles", Latitude = 34.0522, Longitude = -118.2437 },
            new() { ZipCode = "30301", City = "Atlanta", State = "Georgia", StateAbbr = "GA", County = "Fulton", Latitude = 33.7490, Longitude = -84.3880 },
            new() { ZipCode = "77001", City = "Houston", State = "Texas", StateAbbr = "TX", County = "Harris", Latitude = 29.7604, Longitude = -95.3698 },
            new() { ZipCode = "10001", City = "New York", State = "New York", StateAbbr = "NY", County = "New York", Latitude = 40.7128, Longitude = -74.0060 },
            new() { ZipCode = "85001", City = "Phoenix", State = "Arizona", StateAbbr = "AZ", County = "Maricopa", Latitude = 33.4484, Longitude = -112.0740 },
            new() { ZipCode = "19101", City = "Philadelphia", State = "Pennsylvania", StateAbbr = "PA", County = "Philadelphia", Latitude = 39.9526, Longitude = -75.1652 },
            new() { ZipCode = "33101", City = "Miami", State = "Florida", StateAbbr = "FL", County = "Miami-Dade", Latitude = 25.7617, Longitude = -80.1918 },
            new() { ZipCode = "98101", City = "Seattle", State = "Washington", StateAbbr = "WA", County = "King", Latitude = 47.6062, Longitude = -122.3321 },
            new() { ZipCode = "63101", City = "St. Louis", State = "Missouri", StateAbbr = "MO", County = "St. Louis City", Latitude = 38.6270, Longitude = -90.1994 },
            new() { ZipCode = "80201", City = "Denver", State = "Colorado", StateAbbr = "CO", County = "Denver", Latitude = 39.7392, Longitude = -104.9903 },
            new() { ZipCode = "37201", City = "Nashville", State = "Tennessee", StateAbbr = "TN", County = "Davidson", Latitude = 36.1627, Longitude = -86.7816 },
            new() { ZipCode = "28201", City = "Charlotte", State = "North Carolina", StateAbbr = "NC", County = "Mecklenburg", Latitude = 35.2271, Longitude = -80.8431 },
            new() { ZipCode = "46201", City = "Indianapolis", State = "Indiana", StateAbbr = "IN", County = "Marion", Latitude = 39.7684, Longitude = -86.1581 },
            new() { ZipCode = "43201", City = "Columbus", State = "Ohio", StateAbbr = "OH", County = "Franklin", Latitude = 39.9612, Longitude = -82.9988 },
            new() { ZipCode = "53201", City = "Milwaukee", State = "Wisconsin", StateAbbr = "WI", County = "Milwaukee", Latitude = 43.0389, Longitude = -87.9065 },
            new() { ZipCode = "64101", City = "Kansas City", State = "Missouri", StateAbbr = "MO", County = "Jackson", Latitude = 39.0997, Longitude = -94.5786 },
            new() { ZipCode = "38101", City = "Memphis", State = "Tennessee", StateAbbr = "TN", County = "Shelby", Latitude = 35.1495, Longitude = -90.0490 },
            new() { ZipCode = "70112", City = "New Orleans", State = "Louisiana", StateAbbr = "LA", County = "Orleans", Latitude = 29.9511, Longitude = -90.0715 },
            
            // I-80 Corridor
            new() { ZipCode = "68102", City = "Omaha", State = "Nebraska", StateAbbr = "NE", County = "Douglas", Latitude = 41.2565, Longitude = -95.9345 },
            new() { ZipCode = "50309", City = "Des Moines", State = "Iowa", StateAbbr = "IA", County = "Polk", Latitude = 41.5868, Longitude = -93.6250 },
            new() { ZipCode = "43215", City = "Columbus", State = "Ohio", StateAbbr = "OH", County = "Franklin", Latitude = 39.9612, Longitude = -82.9988 },
            new() { ZipCode = "16501", City = "Erie", State = "Pennsylvania", StateAbbr = "PA", County = "Erie", Latitude = 42.1292, Longitude = -80.0851 },
            
            // I-95 Corridor
            new() { ZipCode = "04101", City = "Portland", State = "Maine", StateAbbr = "ME", County = "Cumberland", Latitude = 43.6591, Longitude = -70.2568 },
            new() { ZipCode = "02101", City = "Boston", State = "Massachusetts", StateAbbr = "MA", County = "Suffolk", Latitude = 42.3601, Longitude = -71.0589 },
            new() { ZipCode = "06101", City = "Hartford", State = "Connecticut", StateAbbr = "CT", County = "Hartford", Latitude = 41.7658, Longitude = -72.6734 },
            new() { ZipCode = "21201", City = "Baltimore", State = "Maryland", StateAbbr = "MD", County = "Baltimore City", Latitude = 39.2904, Longitude = -76.6122 },
            new() { ZipCode = "23219", City = "Richmond", State = "Virginia", StateAbbr = "VA", County = "Richmond City", Latitude = 37.5407, Longitude = -77.4360 },
            new() { ZipCode = "27601", City = "Raleigh", State = "North Carolina", StateAbbr = "NC", County = "Wake", Latitude = 35.7796, Longitude = -78.6382 },
            new() { ZipCode = "29401", City = "Charleston", State = "South Carolina", StateAbbr = "SC", County = "Charleston", Latitude = 32.7765, Longitude = -79.9311 },
            new() { ZipCode = "31401", City = "Savannah", State = "Georgia", StateAbbr = "GA", County = "Chatham", Latitude = 32.0809, Longitude = -81.0912 },
            new() { ZipCode = "32801", City = "Orlando", State = "Florida", StateAbbr = "FL", County = "Orange", Latitude = 28.5383, Longitude = -81.3792 },
            
            // I-10 Corridor
            new() { ZipCode = "32301", City = "Tallahassee", State = "Florida", StateAbbr = "FL", County = "Leon", Latitude = 30.4383, Longitude = -84.2807 },
            new() { ZipCode = "36101", City = "Montgomery", State = "Alabama", StateAbbr = "AL", County = "Montgomery", Latitude = 32.3668, Longitude = -86.3000 },
            new() { ZipCode = "39530", City = "Biloxi", State = "Mississippi", StateAbbr = "MS", County = "Harrison", Latitude = 30.3960, Longitude = -88.8853 },
            new() { ZipCode = "70801", City = "Baton Rouge", State = "Louisiana", StateAbbr = "LA", County = "East Baton Rouge", Latitude = 30.4515, Longitude = -91.1871 },
            new() { ZipCode = "78201", City = "San Antonio", State = "Texas", StateAbbr = "TX", County = "Bexar", Latitude = 29.4241, Longitude = -98.4936 },
            new() { ZipCode = "79901", City = "El Paso", State = "Texas", StateAbbr = "TX", County = "El Paso", Latitude = 31.7619, Longitude = -106.4850 },
            new() { ZipCode = "85701", City = "Tucson", State = "Arizona", StateAbbr = "AZ", County = "Pima", Latitude = 32.2226, Longitude = -110.9747 },
            
            // I-40 Corridor
            new() { ZipCode = "27101", City = "Winston-Salem", State = "North Carolina", StateAbbr = "NC", County = "Forsyth", Latitude = 36.0999, Longitude = -80.2442 },
            new() { ZipCode = "37402", City = "Chattanooga", State = "Tennessee", StateAbbr = "TN", County = "Hamilton", Latitude = 35.0456, Longitude = -85.3097 },
            new() { ZipCode = "72201", City = "Little Rock", State = "Arkansas", StateAbbr = "AR", County = "Pulaski", Latitude = 34.7465, Longitude = -92.2896 },
            new() { ZipCode = "73102", City = "Oklahoma City", State = "Oklahoma", StateAbbr = "OK", County = "Oklahoma", Latitude = 35.4676, Longitude = -97.5164 },
            new() { ZipCode = "79101", City = "Amarillo", State = "Texas", StateAbbr = "TX", County = "Potter", Latitude = 35.2220, Longitude = -101.8313 },
            new() { ZipCode = "87102", City = "Albuquerque", State = "New Mexico", StateAbbr = "NM", County = "Bernalillo", Latitude = 35.0844, Longitude = -106.6504 },
            new() { ZipCode = "86001", City = "Flagstaff", State = "Arizona", StateAbbr = "AZ", County = "Coconino", Latitude = 35.1983, Longitude = -111.6513 },
        };

        await context.ZipCodeLookups.AddRangeAsync(zipCodes);
        await context.SaveChangesAsync();
    }
}
