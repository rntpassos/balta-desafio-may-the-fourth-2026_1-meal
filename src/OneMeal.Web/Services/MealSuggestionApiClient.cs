using System.Net.Http.Json;
using OneMeal.Application.Contracts;
using OneMeal.Web.Models;

namespace OneMeal.Web.Services;

public sealed class MealSuggestionApiClient(HttpClient httpClient)
{
	public string ApiBaseUrl => httpClient.BaseAddress?.ToString() ?? "(base url nao configurada)";

    public async Task<MealSuggestionResponse?> SuggestAsync(MissionFormModel form, CancellationToken cancellationToken = default)
    {
        var request = new SuggestMealRequest
        {
            Ingredients = form.Ingredients.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
            AvailableMinutes = form.AvailableMinutes,
            Goal = form.Goal,
            DietaryPreference = form.DietaryPreference,
            AgendaContext = form.AgendaContext,
        };

        using var response = await httpClient.PostAsJsonAsync("api/meal-suggestions", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MealSuggestionResponse>(cancellationToken);
    }
}
