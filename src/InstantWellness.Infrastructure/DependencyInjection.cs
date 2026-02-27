using InstantWellness.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace InstantWellness.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IOrderRepository, OrderRepositoryInMemory>();
        return services;
    }
}
