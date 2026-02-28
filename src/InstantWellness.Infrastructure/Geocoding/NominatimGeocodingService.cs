using System.Text.Json;
using InstantWellness.Application.Geocoding;
using InstantWellness.Application.Geocoding.Models;

namespace InstantWellness.Infrastructure.Geocoding;

public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://nominatim.openstreetmap.org";

    public NominatimGeocodingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        // Nominatim requires a User-Agent header
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "InstantWellness/1.0");
        }
    }

    public async Task<LocationInfo?> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/reverse?lat={latitude}&lon={longitude}&format=json";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var nominatimResponse = JsonSerializer.Deserialize<NominatimResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (nominatimResponse?.Address == null)
            {
                return null;
            }

            return new LocationInfo
            {
                State = nominatimResponse.Address.State,
                County = CleanCountyName(nominatimResponse.Address.County),
                City = nominatimResponse.Address.City 
                       ?? nominatimResponse.Address.Town 
                       ?? nominatimResponse.Address.Village 
                       ?? nominatimResponse.Address.Hamlet,
                Country = nominatimResponse.Address.Country
            };
        }
        catch
        {
            return null;
        }
    }

    private static string? CleanCountyName(string? county)
    {
        if (string.IsNullOrWhiteSpace(county))
            return county;

        // Remove " County" suffix if present
        if (county.EndsWith(" County", StringComparison.OrdinalIgnoreCase))
        {
            return county.Substring(0, county.Length - 7).Trim();
        }

        return county;
    }

    private class NominatimResponse
    {
        public NominatimAddress? Address { get; set; }
    }

    private class NominatimAddress
    {
        public string? Road { get; set; }
        public string? Hamlet { get; set; }
        public string? Village { get; set; }
        public string? Town { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
    }
}
