using Microsoft.Extensions.DependencyInjection;
using OneMeal.Application.Abstractions;
using OneMeal.Infra.Catalog;

namespace OneMeal.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IRecipeCatalog, RuntimeRecipeCatalog>();
        return services;
    }
}
