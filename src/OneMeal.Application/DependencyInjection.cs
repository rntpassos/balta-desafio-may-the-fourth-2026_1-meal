using Microsoft.Extensions.DependencyInjection;
using OneMeal.Application.Abstractions;
using OneMeal.Application.Services;

namespace OneMeal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMealSuggestionService, MealSuggestionService>();
        return services;
    }
}
