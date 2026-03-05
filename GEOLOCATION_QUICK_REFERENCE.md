# GeolocationService - Quick Reference

## ⚡ Quick Start

```csharp
@inject IGeolocationService GeoService

// Get current location
var location = await GeoService.GetCurrentLocationAsync();
if (location != null)
{
    Console.WriteLine($"{location.City}, {location.State}");
}
```

## 📍 Core Methods

| Method | Returns | Use Case |
|--------|---------|----------|
| `GetCurrentLocationAsync()` | `LocationInfo?` | Get GPS location with city/state |
| `GetCityStateFromCoordinatesAsync(lat, lng)` | `LocationInfo?` | Reverse geocode coordinates |
| `GetLocationFromZipCodeAsync(zip)` | `LocationInfo?` | Offline zip → city/state lookup |
| `CheckAndRequestPermissionsAsync()` | `bool` | Check/request location permission |
| `GetCachedLocation()` | `LocationInfo?` | Get last location (< 10 min) |
| `ClearCache()` | `void` | Force fresh location next time |

## 🎯 Common Patterns

### Pattern 1: Auto-Location with Fallback
```csharp
var location = await GeoService.GetCurrentLocationAsync()
              ?? GeoService.GetCachedLocation()
              ?? await GeoService.GetLocationFromZipCodeAsync(userZip);
```

### Pattern 2: Permission Check
```csharp
if (await GeoService.CheckAndRequestPermissionsAsync())
{
    var location = await GeoService.GetCurrentLocationAsync();
}
else
{
    // Show manual entry form
}
```

### Pattern 3: Zip Lookup
```csharp
var location = await GeoService.GetLocationFromZipCodeAsync("60601");
// Returns: Chicago, IL (works offline!)
```

## 📊 LocationInfo Properties

```csharp
public class LocationInfo
{
    double Latitude, Longitude
    string? City, State, County, PostalCode, Country
    DateTime Timestamp
    double? Accuracy (meters)
    LocationSource Source
}
```

## 🔄 Location Sources

| Source | Description |
|--------|-------------|
| `GPS` | Direct from GPS hardware |
| `Network` | WiFi/Cell tower triangulation |
| `Cached` | From 10-minute cache |
| `Manual` | User entered |
| `ZipCodeLookup` | Offline database (45+ cities) |

## 🗺️ Pre-Seeded Zip Codes

Major trucking hubs included:
- Chicago (60601), Dallas (75201), LA (90001)
- Atlanta (30301), Houston (77001), NYC (10001)
- Phoenix (85001), Miami (33101), Seattle (98101)
- ... 35+ more cities along I-80, I-95, I-10, I-40

## 🔒 Permissions Required

### Android
```xml
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
```

### iOS
```xml
<key>NSLocationWhenInUseUsageDescription</key>
<string>Your description here</string>
```

## 💡 Tips

- ✅ Cache lasts 10 minutes
- ✅ Works completely offline with zip codes
- ✅ Nominatim fallback for reverse geocoding
- ✅ Handles all permission scenarios
- ✅ Cross-platform (Android, iOS, Windows, macOS)

## 🚀 Example: Record Fuel Entry

```csharp
// Automatic location
var location = await GeoService.GetCurrentLocationAsync();

var fuel = new FuelEntry
{
    City = location?.City,
    State = location?.State,
    Latitude = location?.Latitude,
    Longitude = location?.Longitude
};

await UnitOfWork.FuelEntries.AddAsync(fuel);
await UnitOfWork.SaveChangesAsync();
```

## 📖 Full Documentation

See `GEOLOCATION_SERVICE_GUIDE.md` for complete details, examples, and troubleshooting.
