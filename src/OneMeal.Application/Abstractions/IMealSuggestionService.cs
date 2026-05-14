using OneMeal.Application.Contracts;

namespace OneMeal.Application.Abstractions;

public interface IMealSuggestionService
{
    Task<MealSuggestionResponse> SuggestAsync(SuggestMealRequest request, CancellationToken cancellationToken = default);
}
