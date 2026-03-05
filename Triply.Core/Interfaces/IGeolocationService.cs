using Triply.Core.Models;

namespace Triply.Core.Interfaces;

public interface IGeolocationService
{
    Task<LocationInfo?> GetCurrentLocationAsync();
    Task<LocationInfo?> GetCityStateFromCoordinatesAsync(double latitude, double longitude);
    Task<LocationInfo?> GetLocationFromZipCodeAsync(string zipCode);
    Task<bool> CheckAndRequestPermissionsAsync();
    LocationInfo? GetCachedLocation();
    void ClearCache();
}
