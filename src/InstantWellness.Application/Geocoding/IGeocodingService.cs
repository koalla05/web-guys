using InstantWellness.Application.Geocoding.Models;

namespace InstantWellness.Application.Geocoding;

public interface IGeocodingService
{
    Task<LocationInfo?> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
}
