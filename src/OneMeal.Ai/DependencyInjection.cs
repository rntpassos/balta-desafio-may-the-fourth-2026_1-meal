using Microsoft.Extensions.DependencyInjection;
using OneMeal.Ai.Services;
using OneMeal.Application.Abstractions;

namespace OneMeal.Ai;

public static class DependencyInjection
{
    public static IServiceCollection AddAi(this IServiceCollection services)
    {
        // Configure HTTP client for Ollama
        services.AddHttpClient("OllamaClient", client =>
        {
            client.BaseAddress = new Uri("http://localhost:11434");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        // Register OllamaRecipeSuggestionComposer with internal fallback
        services.AddSingleton<IRecipeSuggestionComposer, OllamaRecipeSuggestionComposer>();

        return services;
    }
}

