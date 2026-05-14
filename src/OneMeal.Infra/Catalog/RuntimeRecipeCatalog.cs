using OneMeal.Application.Abstractions;
using OneMeal.Core.Entities;

namespace OneMeal.Infra.Catalog;

public sealed class RuntimeRecipeCatalog : IRecipeCatalog
{
    private static readonly IReadOnlyCollection<Recipe> Recipes =
    [
        new(
            "turbo-omelet",
            "Turbo Omelet",
            ["eggs", "spinach", "cheese", "olive oil", "salt"],
            [
                "Whisk the eggs with salt until aerated.",
                "Flash the spinach in olive oil for 90 seconds.",
                "Pour the eggs, add cheese, fold, and finish in under 6 minutes.",
            ],
            new NutritionProfile(390, 29, 4, 27),
            4,
            6,
            "HIGH PROTEIN",
            "Vegetarian"),
        new(
            "nebula-pasta",
            "Nebula Pasta",
            ["pasta", "tomato", "garlic", "basil", "olive oil", "parmesan"],
            [
                "Cook the pasta until al dente.",
                "Build a quick sauce with garlic, tomato, and olive oil.",
                "Combine, finish with basil and parmesan, and serve immediately.",
            ],
            new NutritionProfile(540, 19, 70, 18),
            8,
            12,
            "COMFORT",
            "Vegetarian"),
        new(
            "orbit-bowl",
            "Orbit Chicken Bowl",
            ["chicken breast", "rice", "broccoli", "soy sauce", "garlic", "sesame oil"],
            [
                "Sear the chicken with garlic until golden.",
                "Steam the broccoli while the rice warms through.",
                "Assemble the bowl with a light soy and sesame finish.",
            ],
            new NutritionProfile(510, 38, 41, 16),
            10,
            12,
            "MISSION FUEL",
            "High Protein"),
        new(
            "solar-wrap",
            "Solar Wrap",
            ["tortilla", "chickpeas", "lettuce", "tomato", "yogurt", "lemon"],
            [
                "Mash the chickpeas with lemon and a spoon of yogurt.",
                "Layer the tortilla with lettuce, tomato, and the chickpea mix.",
                "Roll tight and slice into two portable halves.",
            ],
            new NutritionProfile(420, 18, 48, 12),
            7,
            3,
            "LIGHT ORBIT",
            "Vegetarian"),
    ];

    public Task<IReadOnlyCollection<Recipe>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Recipes);
}
