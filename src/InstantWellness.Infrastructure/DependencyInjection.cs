using InstantWellness.Application.Geocoding;
using InstantWellness.Application.Tax;
using InstantWellness.Domain.Interfaces;
using InstantWellness.Infrastructure.Data;
using InstantWellness.Infrastructure.Geocoding;
using InstantWellness.Infrastructure.Tax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InstantWellness.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string taxRatesCsvPath,
        string? connectionString = null)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));
            services.AddScoped<IOrderRepository, OrderRepositoryEf>();
        }
        else
        {
            services.AddSingleton<IOrderRepository, OrderRepositoryInMemory>();
        }
        
        // Register HttpClient for Geocoding service
        services.AddHttpClient<IGeocodingService, NominatimGeocodingService>();
        
        // Register Tax service as singleton with CSV file path
        services.AddSingleton<ITaxService>(provider =>
        {
            var geocodingService = provider.GetRequiredService<IGeocodingService>();
            return new TaxService(geocodingService, taxRatesCsvPath);
        });
        
        // Register Order Tax Calculator for lazy evaluation
        services.AddScoped<IOrderTaxCalculator, OrderTaxCalculator>();
        
        return services;
    }
}
