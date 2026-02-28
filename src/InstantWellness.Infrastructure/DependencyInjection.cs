using InstantWellness.Domain.Interfaces;
using InstantWellness.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InstantWellness.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IOrderRepository, OrderRepositoryInMemory>();
        services.Configure<TaxCalculationOptions>(configuration.GetSection("TaxCalculation"));
        services.AddSingleton<ITaxCalculationService, TaxCalculationService>();
        services.AddHttpClient("Nominatim", client =>
        {
            client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
            client.DefaultRequestHeaders.Add("User-Agent", "InstantWellness/1.0 (tax calculation)");
        });
        return services;
    }
}
