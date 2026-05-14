namespace OneMeal.Application.Contracts;

public sealed class SuggestMealRequest
{
    public IReadOnlyCollection<string> Ingredients { get; init; } = [];

    public int AvailableMinutes { get; init; }

    public string? Goal { get; init; }

    public string? DietaryPreference { get; init; }

    public string? AgendaContext { get; init; }
}
