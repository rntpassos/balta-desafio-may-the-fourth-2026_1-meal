namespace OneMeal.Application.Contracts;

public sealed class RecipeComposition
{
    public required string Justification { get; init; }

    public required bool UsedFallback { get; init; }
}
