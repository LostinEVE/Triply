# GeolocationService - Complete Implementation Guide

## Overview

The enhanced `GeolocationService` provides robust location tracking with multiple fallbacks, permission handling, caching, and offline support for the Triply trucking management system.

## Features

### ✅ Implemented Features

1. **GPS Location Access** - Uses MAUI Geolocation API
2. **Reverse Geocoding** - Converts coordinates to city/state
3. **Permission Handling** - Requests and checks location permissions
4. **Location Caching** - 10-minute cache to reduce API calls
5. **Offline Support** - Zip code to city/state lookup table (45+ major cities)
6. **Multiple Fallbacks** - GPS → Network → Cached → Manual → Zip Lookup
7. **Cross-Platform** - Works on Android, iOS, Windows, macOS

## API Methods

### 1. GetCurrentLocationAsync()

Gets the current GPS location with full details.

```csharp
LocationInfo? location = await geolocationService.GetCurrentLocationAsync();

if (location != null)
{
    Console.WriteLine($"Location: {location.City}, {location.State}");
    Console.WriteLine($"Coordinates: {location.Latitude}, {location.Longitude}");
    Console.WriteLine($"Source: {location.Source}");
    Console.WriteLine($"Accuracy: {location.Accuracy}m");
}
```

**Returns:** `LocationInfo?` with:
- Latitude, Longitude
- City, State, County, PostalCode
- Timestamp
- Accuracy (in meters)
- Source (GPS, Network, Cached, Manual, ZipCodeLookup)

**Behavior:**
1. Checks cache first (10-minute lifetime)
2. Requests permissions if needed
3. Gets GPS coordinates
4. Reverse geocodes to city/state
5. Caches result
6. Returns cached value on failure

### 2. GetCityStateFromCoordinatesAsync(lat, long)

Converts coordinates to location information.

```csharp
LocationInfo? location = await geolocationService
    .GetCityStateFromCoordinatesAsync(41.8781, -87.6298);

if (location != null)
{
    Console.WriteLine($"{location.City}, {location.State}"); // Chicago, IL
}
```

**Fallback Strategy:**
1. MAUI built-in Geocoding (online)
2. Nominatim OpenStreetMap API (online)
3. Returns null if both fail

### 3. GetLocationFromZipCodeAsync(zipCode)

Offline lookup from local database.

```csharp
LocationInfo? location = await geolocationService
    .GetLocationFromZipCodeAsync("60601");

if (location != null)
{
    Console.WriteLine($"{location.City}, {location.State}"); // Chicago, IL
    Console.WriteLine($"Source: {location.Source}"); // ZipCodeLookup
}
```

**Features:**
- Works completely offline
- 45+ major US trucking hubs pre-seeded
- Includes lat/long for each city
- Accepts 5-digit or 9-digit ZIP codes

### 4. CheckAndRequestPermissionsAsync()

Checks and requests location permissions.

```csharp
bool hasPermission = await geolocationService.CheckAndRequestPermissionsAsync();

if (!hasPermission)
{
    // Show manual entry form
}
```

**Returns:** `true` if permission granted, `false` otherwise

### 5. GetCachedLocation()

Retrieves the last known location from cache.

```csharp
LocationInfo? cached = geolocationService.GetCachedLocation();

if (cached != null)
{
    // Use cached location (< 10 minutes old)
}
```

**Use Case:** Quick access without async call when you know cache is likely valid

### 6. ClearCache()

Clears the location cache.

```csharp
geolocationService.ClearCache();
```

**Use Case:** Force fresh location on next request

## LocationInfo Model

```csharp
public class LocationInfo
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
    GPS = 0,        // Direct GPS hardware
    Network = 1,    // Network-based (WiFi/Cell towers)
    Cached = 2,     // From 10-minute cache
    Manual = 3,     // User entered manually
    ZipCodeLookup = 4  // From offline database
}
```

## Usage Scenarios

### Scenario 1: Record Fuel Purchase (Automatic)

```csharp
@inject IGeolocationService GeolocationService
@inject IUnitOfWork UnitOfWork

private async Task RecordFuelPurchase()
{
    // Get current location automatically
    var location = await GeolocationService.GetCurrentLocationAsync();
    
    var fuelEntry = new FuelEntry
    {
        FuelEntryId = Guid.NewGuid(),
        TruckId = selectedTruckId,
        FuelDate = DateTime.UtcNow,
        Gallons = gallons,
        PricePerGallon = pricePerGallon,
        TotalCost = gallons * pricePerGallon,
        FuelType = FuelType.Diesel,
        City = location?.City,
        State = location?.State,
        Latitude = location?.Latitude,
        Longitude = location?.Longitude
    };

    await UnitOfWork.FuelEntries.AddAsync(fuelEntry);
    await UnitOfWork.SaveChangesAsync();
}
```

### Scenario 2: Manual Entry with Zip Code Lookup

```csharp
private async Task RecordFuelWithZipCode(string zipCode)
{
    // User enters zip code, we look it up offline
    var location = await GeolocationService.GetLocationFromZipCodeAsync(zipCode);
    
    if (location == null)
    {
        // Zip not in database, ask user for city/state
        ShowManualEntryForm();
        return;
    }

    var fuelEntry = new FuelEntry
    {
        // ... other properties
        City = location.City,
        State = location.State,
        Latitude = location.Latitude,
        Longitude = location.Longitude
    };

    await UnitOfWork.FuelEntries.AddAsync(fuelEntry);
    await UnitOfWork.SaveChangesAsync();
}
```

### Scenario 3: Progressive Enhancement

```csharp
private async Task<LocationInfo?> GetLocationWithFallbacks()
{
    // Try 1: GPS
    var location = await GeolocationService.GetCurrentLocationAsync();
    if (location != null && location.Source == LocationSource.GPS)
        return location;

    // Try 2: Cached
    location = GeolocationService.GetCachedLocation();
    if (location != null)
        return location;

    // Try 3: Ask for zip code
    var zipCode = await PromptUserForZipCode();
    if (!string.IsNullOrEmpty(zipCode))
    {
        location = await GeolocationService.GetLocationFromZipCodeAsync(zipCode);
        if (location != null)
            return location;
    }

    // Try 4: Manual entry
    return await PromptUserForCityState();
}
```

### Scenario 4: Permission Handling

```csharp
protected override async Task OnInitializedAsync()
{
    var hasPermission = await GeolocationService.CheckAndRequestPermissionsAsync();
    
    if (!hasPermission)
    {
        // Show message to user
        showManualEntryForm = true;
        message = "Location permission denied. Please enable in device settings or enter manually.";
        return;
    }

    // Permission granted, get location
    var location = await GeolocationService.GetCurrentLocationAsync();
    
    if (location != null)
    {
        currentCity = location.City;
        currentState = location.State;
    }
}
```

### Scenario 5: Reverse Geocoding Existing Coordinates

```csharp
// You have coordinates from a previous load
var load = await UnitOfWork.Loads.GetByIdAsync(loadId);

// Get city/state for those coordinates
var location = await GeolocationService
    .GetCityStateFromCoordinatesAsync(32.7767, -96.7970);

if (location != null)
{
    Console.WriteLine($"Load destination: {location.City}, {location.State}");
}
```

## Platform Configuration

### Android (AndroidManifest.xml)
```xml
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-feature android:name="android.hardware.location" android:required="false" />
<uses-feature android:name="android.hardware.location.gps" android:required="false" />
```

### iOS (Info.plist)
```xml
<key>NSLocationWhenInUseUsageDescription</key>
<string>Triply needs access to your location to automatically record fuel purchases.</string>
<key>NSLocationAlwaysUsageDescription</key>
<string>Triply uses your location to track fuel stops and maintain accurate IFTA records.</string>
```

### Windows & macOS
No additional configuration needed - works automatically.

## Offline Support - Zip Code Database

### Pre-Seeded Cities (45+)

The app includes major trucking hubs:
- **I-80 Corridor**: Chicago, Omaha, Des Moines, Erie
- **I-95 Corridor**: Boston, NYC, Philadelphia, Baltimore, Miami
- **I-10 Corridor**: Jacksonville, Houston, San Antonio, Phoenix, LA
- **I-40 Corridor**: Raleigh, Nashville, Memphis, Oklahoma City, Albuquerque
- **Major Hubs**: Dallas, Atlanta, Denver, Seattle, Kansas City

### Adding More Zip Codes

You can extend the zip code database:

```csharp
public async Task AddCustomZipCodesAsync()
{
    var newZipCodes = new List<ZipCodeLookup>
    {
        new() 
        { 
            ZipCode = "12345", 
            City = "MyCity", 
            State = "MyState", 
            StateAbbr = "MS",
            Latitude = 40.0, 
            Longitude = -80.0 
        }
    };

    await UnitOfWork.ZipCodeLookups.AddRangeAsync(newZipCodes);
    await UnitOfWork.SaveChangesAsync();
}
```

### Bulk Import from CSV

```csharp
public async Task ImportZipCodesFromCsvAsync(string csvPath)
{
    var lines = await File.ReadAllLinesAsync(csvPath);
    var zipCodes = new List<ZipCodeLookup>();

    foreach (var line in lines.Skip(1)) // Skip header
    {
        var parts = line.Split(',');
        zipCodes.Add(new ZipCodeLookup
        {
            ZipCode = parts[0],
            City = parts[1],
            State = parts[2],
            StateAbbr = parts[3],
            Latitude = double.Parse(parts[4]),
            Longitude = double.Parse(parts[5])
        });
    }

    await UnitOfWork.ZipCodeLookups.AddRangeAsync(zipCodes);
    await UnitOfWork.SaveChangesAsync();
}
```

## Caching Strategy

### Cache Lifetime: 10 minutes

**Why 10 minutes?**
- Reduces API calls during continuous use
- Fresh enough for trucking scenarios
- Automatic expiration

### Cache Invalidation

```csharp
// Manual clear
GeolocationService.ClearCache();

// Automatic clear after 10 minutes
// (happens automatically on next GetCurrentLocationAsync call)
```

### When Cache is Used
- ✅ Repeated calls within 10 minutes
- ✅ As fallback when GPS fails
- ✅ When permissions are denied but cache exists

## Error Handling

The service handles all common errors gracefully:

### FeatureNotSupportedException
Device doesn't have GPS hardware → Falls back to cached/manual

### FeatureNotEnabledException
Location services disabled in settings → Returns cached or null

### PermissionException
User denied permission → Returns cached or null

### Network Errors
Reverse geocoding fails → Returns coordinates without city/state

### Database Errors
Zip lookup fails → Returns null (caller should show manual entry)

## Best Practices

### ✅ DO

1. **Check permissions early** in the app lifecycle
2. **Always provide manual entry** as fallback
3. **Display location source** to user (GPS, Cached, Manual)
4. **Refresh location** on critical operations (fuel purchases)
5. **Cache aggressively** to save battery and data

### ❌ DON'T

1. **Don't assume GPS is always available**
2. **Don't make excessive location requests** (use cache)
3. **Don't block UI** - all methods are async
4. **Don't ignore permissions** - always check first
5. **Don't rely solely on online services** - have offline fallback

## Performance Considerations

### Battery Usage
- **10-minute cache** reduces GPS queries
- **Medium accuracy** balances precision vs. battery
- **10-second timeout** prevents hanging

### Data Usage
- **Built-in geocoding** (MAUI) is preferred
- **Nominatim API** only as fallback
- **Offline zip lookup** uses zero data

### Response Times
- **GPS acquisition**: 2-10 seconds
- **Cached lookup**: < 1ms
- **Zip code lookup**: < 50ms (database query)
- **Reverse geocoding**: 1-3 seconds

## Testing

### Manual Testing Checklist

- [ ] Allow location permission → GPS works
- [ ] Deny location permission → Manual entry shown
- [ ] Turn off location services → Cached location used
- [ ] Turn off WiFi/Data → Zip lookup works offline
- [ ] Enter invalid zip code → Error message shown
- [ ] Multiple requests within 10min → Cache used
- [ ] Clear cache → Fresh GPS request

### Unit Test Examples

```csharp
[Test]
public async Task GetCurrentLocation_WithPermission_ReturnsGPSLocation()
{
    // Arrange
    var mockContextFactory = new Mock<IDbContextFactory<TriplyDbContext>>();
    var service = new GeolocationService(mockContextFactory.Object);

    // Act
    var location = await service.GetCurrentLocationAsync();

    // Assert (when GPS available)
    Assert.NotNull(location);
    Assert.Equal(LocationSource.GPS, location.Source);
}

[Test]
public async Task GetLocationFromZipCode_ValidZip_ReturnsLocation()
{
    // Arrange
    var context = CreateInMemoryContext();
    await ZipCodeSeeder.SeedCommonZipCodesAsync(context);
    var factory = CreateDbContextFactory(context);
    var service = new GeolocationService(factory);

    // Act
    var location = await service.GetLocationFromZipCodeAsync("60601");

    // Assert
    Assert.NotNull(location);
    Assert.Equal("Chicago", location.City);
    Assert.Equal("IL", location.State);
    Assert.Equal(LocationSource.ZipCodeLookup, location.Source);
}

[Test]
public void GetCachedLocation_WithinCacheLifetime_ReturnsCachedValue()
{
    // Arrange
    var service = new GeolocationService(mockFactory);
    
    // First call populates cache
    var location1 = await service.GetCurrentLocationAsync();
    
    // Act - Second call within 10 minutes
    var location2 = service.GetCachedLocation();

    // Assert
    Assert.NotNull(location2);
    Assert.Equal(LocationSource.Cached, location2.Source);
}
```

## Integration Examples

### Example 1: Fuel Entry Form

```razor
@page "/add-fuel"
@inject IGeolocationService GeoService

<h3>Add Fuel Entry</h3>

@if (locationInfo != null)
{
    <div class="location-badge">
        📍 @locationInfo.City, @locationInfo.State
        <span class="badge">@locationInfo.Source</span>
    </div>
}
else
{
    <button @onclick="GetLocation">Get My Location</button>
    
    <div class="manual-entry">
        <input @bind="zipCode" placeholder="Enter Zip Code" />
        <button @onclick="LookupZip">Lookup</button>
    </div>
}

@code {
    private LocationInfo? locationInfo;
    private string zipCode = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await GetLocation();
    }

    private async Task GetLocation()
    {
        locationInfo = await GeoService.GetCurrentLocationAsync();
    }

    private async Task LookupZip()
    {
        locationInfo = await GeoService.GetLocationFromZipCodeAsync(zipCode);
    }
}
```

### Example 2: Automatic Fuel Recording Service

```csharp
public class FuelService
{
    private readonly IGeolocationService _geoService;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<FuelEntry> RecordFuelPurchaseAsync(
        string truckId, 
        decimal gallons, 
        decimal price,
        string? manualZipCode = null)
    {
        LocationInfo? location = null;

        // Try automatic location first
        location = await _geoService.GetCurrentLocationAsync();

        // Fallback to zip code if provided and GPS failed
        if (location == null && !string.IsNullOrEmpty(manualZipCode))
        {
            location = await _geoService.GetLocationFromZipCodeAsync(manualZipCode);
        }

        var truck = await _unitOfWork.Trucks.GetByIdAsync(truckId);
        
        var fuelEntry = new FuelEntry
        {
            FuelEntryId = Guid.NewGuid(),
            TruckId = truckId,
            FuelDate = DateTime.UtcNow,
            Odometer = truck.CurrentOdometer,
            Gallons = gallons,
            PricePerGallon = price,
            TotalCost = gallons * price,
            FuelType = FuelType.Diesel,
            City = location?.City ?? "Unknown",
            State = location?.State ?? "Unknown",
            Latitude = location?.Latitude,
            Longitude = location?.Longitude
        };

        await _unitOfWork.FuelEntries.AddAsync(fuelEntry);
        await _unitOfWork.SaveChangesAsync();

        return fuelEntry;
    }
}
```

### Example 3: IFTA Report with Location Verification

```csharp
public class IFTAService
{
    public async Task<IFTAReport> GenerateReportAsync(string truckId, int year, int quarter)
    {
        var fuelEntries = await _unitOfWork.FuelEntries
            .GetQueryable()
            .Where(f => f.TruckId == truckId && 
                       f.IFTA_Quarter == $"Q{quarter}-{year}")
            .ToListAsync();

        // Verify locations for entries without city/state
        foreach (var entry in fuelEntries.Where(e => string.IsNullOrEmpty(e.State)))
        {
            if (entry.Latitude.HasValue && entry.Longitude.HasValue)
            {
                var location = await _geoService.GetCityStateFromCoordinatesAsync(
                    entry.Latitude.Value, 
                    entry.Longitude.Value);

                if (location != null)
                {
                    entry.City = location.City;
                    entry.State = location.State;
                    await _unitOfWork.FuelEntries.UpdateAsync(entry);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();

        // Generate report grouped by state
        var stateGroups = fuelEntries
            .Where(f => !string.IsNullOrEmpty(f.State))
            .GroupBy(f => f.State)
            .Select(g => new IFTAStateData
            {
                State = g.Key!,
                TotalGallons = g.Sum(f => f.Gallons),
                TotalCost = g.Sum(f => f.TotalCost)
            });

        return new IFTAReport { StateData = stateGroups.ToList() };
    }
}
```

### Example 4: Dashboard with Location Stats

```csharp
public async Task<LocationStatistics> GetLocationStatsAsync()
{
    var fuelEntries = await _unitOfWork.FuelEntries
        .GetQueryable()
        .Where(f => f.FuelDate >= DateTime.UtcNow.AddMonths(-3))
        .ToListAsync();

    var stats = new LocationStatistics
    {
        // Count entries by source
        GPSEntries = fuelEntries.Count(f => f.Latitude.HasValue),
        ManualEntries = fuelEntries.Count(f => !f.Latitude.HasValue),
        
        // Most common states
        TopStates = fuelEntries
            .Where(f => !string.IsNullOrEmpty(f.State))
            .GroupBy(f => f.State)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new { State = g.Key, Count = g.Count() })
            .ToList(),

        // Location accuracy
        AverageAccuracy = fuelEntries
            .Where(f => f.Latitude.HasValue)
            .Count()
    };

    return stats;
}
```

## Troubleshooting

### Location Always Returns Null

**Possible Causes:**
1. Location permissions not granted
2. Location services disabled on device
3. Device doesn't have GPS hardware
4. Running in emulator without location simulation

**Solution:**
```csharp
var hasPermission = await GeolocationService.CheckAndRequestPermissionsAsync();
if (!hasPermission)
{
    // Show instructions to enable in device settings
    // Provide manual entry option
}
```

### Reverse Geocoding Returns Null

**Possible Causes:**
1. No internet connection
2. Nominatim API rate limit exceeded
3. Coordinates in ocean or unpopulated area

**Solution:**
- Use zip code lookup as fallback
- Store coordinates even without city/state
- Run batch reverse geocoding later when online

### Cache Not Working

**Check:**
- Cache lifetime (10 minutes)
- Cache cleared manually
- App restarted (cache is in-memory)

**Solution:**
- Implement persistent cache using Preferences:
```csharp
Preferences.Set("lastLocation", JsonSerializer.Serialize(location));
```

## Advanced Features

### Custom Cache Lifetime

Modify in GeolocationService.cs:
```csharp
private readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(5); // Change to 5 minutes
```

### Request High Accuracy

For critical operations:
```csharp
var request = new GeolocationRequest
{
    DesiredAccuracy = GeolocationAccuracy.Best, // Higher accuracy
    Timeout = TimeSpan.FromSeconds(30) // Longer timeout
};
```

### Background Location Tracking

For continuous tracking (requires additional permissions):
```csharp
// iOS Info.plist
<key>NSLocationAlwaysAndWhenInUseUsageDescription</key>
<string>Track location in background for automatic fuel logging</string>

// Enable background updates
<key>UIBackgroundModes</key>
<array>
    <string>location</string>
</array>
```

## Production Recommendations

1. **Expand Zip Code Database**
   - Import complete US zip code dataset (~40,000 entries)
   - Free datasets available: simplemaps.com/data/us-zips

2. **Implement Persistent Cache**
   - Store last location in SecureStorage
   - Survive app restarts

3. **Add Network Detection**
   - Check connectivity before online geocoding
   - Automatically use offline methods when offline

4. **Implement Retry Logic**
   - Retry failed geocoding attempts
   - Queue locations for batch processing

5. **Privacy Compliance**
   - Add privacy policy link
   - Allow users to opt-out of location tracking
   - Store only minimal location data

## Summary

The enhanced GeolocationService provides:
- ✅ Multiple fallback strategies
- ✅ Permission handling
- ✅ Caching for performance
- ✅ Offline zip code lookup
- ✅ Cross-platform support
- ✅ Graceful degradation
- ✅ Manual entry option

Perfect for OTR trucking where connectivity may be intermittent and accurate location tracking is critical for IFTA compliance!
