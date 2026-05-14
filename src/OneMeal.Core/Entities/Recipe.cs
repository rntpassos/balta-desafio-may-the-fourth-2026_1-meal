namespace OneMeal.Core.Entities;

public sealed record Recipe(
    string Id,
    string Name,
    IReadOnlyList<string> Ingredients,
    IReadOnlyList<string> Steps,
    NutritionProfile Nutrition,
    int PrepMinutes,
    int CookMinutes,
    string Badge,
    string DietaryProfile)
{
    public int TotalMinutes => PrepMinutes + CookMinutes;
}
