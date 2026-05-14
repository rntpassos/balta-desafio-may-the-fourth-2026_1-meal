using OneMeal.Application.Contracts;
using OneMeal.Core.Entities;

namespace OneMeal.Application.Abstractions;

public interface IRecipeSuggestionComposer
{
    Task<RecipeComposition> ComposeAsync(
        Recipe recipe,
        SuggestMealRequest request,
        IReadOnlyCollection<string> usedIngredients,
        IReadOnlyCollection<string> missingIngredients,
        CancellationToken cancellationToken = default);
}
