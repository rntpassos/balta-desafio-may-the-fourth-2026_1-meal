namespace OneMeal.Application.Contracts;

public sealed class MealSuggestionResponse
{
    public required string RecipeId { get; init; }

    public required string RecipeName { get; init; }

    public required string Badge { get; init; }

    public required string DietaryProfile { get; init; }

    public required int TotalMinutes { get; init; }

    public required string Justification { get; init; }

    public required IReadOnlyCollection<string> UsedIngredients { get; init; }

    public required IReadOnlyCollection<string> MissingIngredients { get; init; }

    public required IReadOnlyCollection<string> Steps { get; init; }

    public required NutritionSummary Nutrition { get; init; }

    public required bool UsedFallback { get; init; }
}

public sealed class NutritionSummary
{
    public required int Calories { get; init; }

    public required int ProteinGrams { get; init; }

    public required int CarbsGrams { get; init; }

    public required int FatGrams { get; init; }
}
