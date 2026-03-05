using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public class GeolocationService : IGeolocationService
{
    private readonly IDbContextFactory<Data.TriplyDbContext> _contextFactory;
    private LocationInfo? _cachedLocation;
    private DateTime _cacheExpiration;
    private readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(10);

    public GeolocationService(IDbContextFactory<Data.TriplyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<LocationInfo?> GetCurrentLocationAsync()
    {
        // Check if cached location is still valid
        if (_cachedLocation != null && DateTime.UtcNow < _cacheExpiration)
        {
            return _cachedLocation with { Source = LocationSource.Cached };
        }

        // Check and request permissions
        if (!await CheckAndRequestPermissionsAsync())
        {
            return _cachedLocation; // Return cached if available, otherwise null
        }

        try
        {
            var request = new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            };

            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                var locationInfo = await GetCityStateFromCoordinatesAsync(location.Latitude, location.Longitude);

                if (locationInfo != null)
                {
                    locationInfo.Accuracy = location.Accuracy;
                    locationInfo.Timestamp = location.Timestamp.UtcDateTime;
                    locationInfo.Source = LocationSource.GPS;

                    // Cache the location
                    _cachedLocation = locationInfo;
                    _cacheExpiration = DateTime.UtcNow.Add(_cacheLifetime);

                    return locationInfo;
                }
            }
        }
        catch (FeatureNotSupportedException)
        {
            // Geolocation not supported on device
        }
        catch (FeatureNotEnabledException)
        {
            // Geolocation is disabled
        }
        catch (PermissionException)
        {
            // Permission not granted
        }
        catch (Exception)
        {
            // Other error occurred
        }

        return _cachedLocation;
    }

    public async Task<LocationInfo?> GetCityStateFromCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            // Try MAUI built-in geocoding first
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);
            var placemark = placemarks?.FirstOrDefault();

            if (placemark != null)
            {
                return new LocationInfo
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    City = placemark.Locality,
                    State = placemark.AdminArea,
                    County = placemark.SubAdminArea,
                    PostalCode = placemark.PostalCode,
                    Country = placemark.CountryName,
                    Timestamp = DateTime.UtcNow,
                    Source = LocationSource.Network
                };
            }
        }
        catch (Exception)
        {
            // Fallback to Nominatim API if built-in fails
            try
            {
                return await GetLocationFromNominatimAsync(latitude, longitude);
            }
            catch
            {
                // Both methods failed
            }
        }

        return null;
    }

    public async Task<LocationInfo?> GetLocationFromZipCodeAsync(string zipCode)
    {
        if (string.IsNullOrWhiteSpace(zipCode))
            return null;

        // Clean zip code (remove spaces, keep only first 5 digits)
        var cleanZip = new string(zipCode.Where(char.IsDigit).Take(5).ToArray());

        if (cleanZip.Length != 5)
            return null;

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var zipData = await context.ZipCodeLookups
                .AsNoTracking()
                .FirstOrDefaultAsync(z => z.ZipCode == cleanZip);

            if (zipData != null)
            {
                return new LocationInfo
                {
                    Latitude = zipData.Latitude,
                    Longitude = zipData.Longitude,
                    City = zipData.City,
                    State = zipData.StateAbbr,
                    County = zipData.County,
                    PostalCode = zipData.ZipCode,
                    Country = "USA",
                    Timestamp = DateTime.UtcNow,
                    Source = LocationSource.ZipCodeLookup
                };
            }
        }
        catch
        {
            // Database access failed
        }

        return null;
    }

    public async Task<bool> CheckAndRequestPermissionsAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
                return true;

            if (status == PermissionStatus.Denied || status == PermissionStatus.Disabled)
                return false;

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            return status == PermissionStatus.Granted;
        }
        catch
        {
            return false;
        }
    }

    public LocationInfo? GetCachedLocation()
    {
        if (_cachedLocation != null && DateTime.UtcNow < _cacheExpiration)
        {
            return _cachedLocation with { Source = LocationSource.Cached };
        }

        return null;
    }

    public void ClearCache()
    {
        _cachedLocation = null;
        _cacheExpiration = DateTime.MinValue;
    }

    private async Task<LocationInfo?> GetLocationFromNominatimAsync(double latitude, double longitude)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "TriplyTruckingApp/1.0");

            var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&zoom=18&addressdetails=1";
            var response = await httpClient.GetStringAsync(url);

            // Simple JSON parsing (or use System.Text.Json for production)
            if (response.Contains("\"city\"") || response.Contains("\"town\"") || response.Contains("\"village\""))
            {
                // Basic parsing - in production, use proper JSON deserialization
                var city = ExtractJsonValue(response, "city") 
                          ?? ExtractJsonValue(response, "town") 
                          ?? ExtractJsonValue(response, "village");
                var state = ExtractJsonValue(response, "state");
                var county = ExtractJsonValue(response, "county");
                var postcode = ExtractJsonValue(response, "postcode");

                if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(state))
                {
                    return new LocationInfo
                    {
                        Latitude = latitude,
                        Longitude = longitude,
                        City = city,
                        State = state,
                        County = county,
                        PostalCode = postcode,
                        Country = "USA",
                        Timestamp = DateTime.UtcNow,
                        Source = LocationSource.Network
                    };
                }
            }
        }
        catch
        {
            // Nominatim API failed
        }

        return null;
    }

    private static string? ExtractJsonValue(string json, string key)
    {
        try
        {
            var searchKey = $"\"{key}\":\"";
            var startIndex = json.IndexOf(searchKey);
            if (startIndex == -1)
                return null;

            startIndex += searchKey.Length;
            var endIndex = json.IndexOf("\"", startIndex);
            if (endIndex == -1)
                return null;

            return json.Substring(startIndex, endIndex - startIndex);
        }
        catch
        {
            return null;
        }
    }
}
