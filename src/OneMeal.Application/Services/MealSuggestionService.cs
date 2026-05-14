using System.Globalization;
using System.Text;
using OneMeal.Application.Abstractions;
using OneMeal.Application.Contracts;
using OneMeal.Core.Entities;

namespace OneMeal.Application.Services;

public sealed class MealSuggestionService(IRecipeCatalog recipeCatalog, IRecipeSuggestionComposer composer) : IMealSuggestionService
{
    private static readonly IReadOnlyDictionary<string, string> IngredientAliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["egg"] = "eggs",
            ["eggs"] = "eggs",
            ["ovo"] = "eggs",
            ["ovos"] = "eggs",
            ["spinach"] = "spinach",
            ["espinafre"] = "spinach",
            ["cheese"] = "cheese",
            ["queijo"] = "cheese",
            ["olive oil"] = "olive oil",
            ["azeite"] = "olive oil",
            ["salt"] = "salt",
            ["sal"] = "salt",
            ["pasta"] = "pasta",
            ["macarrao"] = "pasta",
            ["macarrao integral"] = "pasta",
            ["tomato"] = "tomato",
            ["tomatoes"] = "tomato",
            ["tomate"] = "tomato",
            ["garlic"] = "garlic",
            ["alho"] = "garlic",
            ["basil"] = "basil",
            ["manjericao"] = "basil",
            ["parmesan"] = "parmesan",
            ["parmesao"] = "parmesan",
            ["rice"] = "rice",
            ["arroz"] = "rice",
            ["chicken"] = "chicken breast",
            ["chicken breast"] = "chicken breast",
            ["frango"] = "chicken breast",
            ["peito de frango"] = "chicken breast",
            ["carne moida"] = "ground beef",
            ["broccoli"] = "broccoli",
            ["brocolis"] = "broccoli",
            ["pimentao"] = "bell pepper",
            ["soy sauce"] = "soy sauce",
            ["shoyu"] = "soy sauce",
            ["sesame oil"] = "sesame oil",
            ["oleo de gergelim"] = "sesame oil",
            ["tortilla"] = "tortilla",
            ["rap10"] = "tortilla",
            ["chickpeas"] = "chickpeas",
            ["grao de bico"] = "chickpeas",
            ["lettuce"] = "lettuce",
            ["alface"] = "lettuce",
            ["yogurt"] = "yogurt",
            ["iogurte"] = "yogurt",
            ["lemon"] = "lemon",
            ["limao"] = "lemon",
        };

    public async Task<MealSuggestionResponse> SuggestAsync(SuggestMealRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Ingredients.Count == 0)
        {
            throw new ArgumentException("At least one ingredient is required.", nameof(request));
        }

        if (request.AvailableMinutes <= 0)
        {
            throw new ArgumentException("Available minutes must be greater than zero.", nameof(request));
        }

        var normalizedIngredients = request.Ingredients
            .Select(Normalize)
            .Where(static ingredient => !string.IsNullOrWhiteSpace(ingredient))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var recipes = await recipeCatalog.ListAsync(cancellationToken);

        var rankedRecipes = recipes
            .Select(recipe => new
            {
                Recipe = recipe,
                UsedIngredients = recipe.Ingredients
                    .Where(ingredient => normalizedIngredients.Contains(Normalize(ingredient), StringComparer.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                MissingIngredients = recipe.Ingredients
                    .Where(ingredient => !normalizedIngredients.Contains(Normalize(ingredient), StringComparer.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
            })
            .Select(candidate => new
            {
                candidate.Recipe,
                candidate.UsedIngredients,
                candidate.MissingIngredients,
                Score = Score(candidate.Recipe, request, candidate.UsedIngredients, candidate.MissingIngredients),
            })
            .OrderByDescending(static candidate => candidate.Score)
            .ThenBy(candidate => candidate.Recipe.TotalMinutes)
            .ToArray();

        var bestCandidate = rankedRecipes.FirstOrDefault() ?? throw new InvalidOperationException("No recipes are available in the catalog.");

        var composition = await composer.ComposeAsync(
            bestCandidate.Recipe,
            request,
            bestCandidate.UsedIngredients,
            bestCandidate.MissingIngredients,
            cancellationToken);

        return new MealSuggestionResponse
        {
            RecipeId = bestCandidate.Recipe.Id,
            RecipeName = bestCandidate.Recipe.Name,
            Badge = bestCandidate.Recipe.Badge,
            DietaryProfile = bestCandidate.Recipe.DietaryProfile,
            TotalMinutes = bestCandidate.Recipe.TotalMinutes,
            Justification = composition.Justification,
            UsedIngredients = bestCandidate.UsedIngredients,
            MissingIngredients = bestCandidate.MissingIngredients,
            Steps = bestCandidate.Recipe.Steps,
            Nutrition = new NutritionSummary
            {
                Calories = bestCandidate.Recipe.Nutrition.Calories,
                ProteinGrams = bestCandidate.Recipe.Nutrition.ProteinGrams,
                CarbsGrams = bestCandidate.Recipe.Nutrition.CarbsGrams,
                FatGrams = bestCandidate.Recipe.Nutrition.FatGrams,
            },
            UsedFallback = composition.UsedFallback,
        };
    }

    private static string Normalize(string value)
    {
        var normalized = NormalizeText(value);

        if (IngredientAliases.TryGetValue(normalized, out var canonicalIngredient))
        {
            return canonicalIngredient;
        }

        return normalized;
    }

    private static int Score(Recipe recipe, SuggestMealRequest request, IReadOnlyCollection<string> usedIngredients, IReadOnlyCollection<string> missingIngredients)
    {
        var score = usedIngredients.Count * 20;

        if (recipe.TotalMinutes <= request.AvailableMinutes)
        {
            score += 30;
        }
        else
        {
            score -= (recipe.TotalMinutes - request.AvailableMinutes) * 5;
        }

        score -= missingIngredients.Count * 7;

        score += ScoreDietaryCompatibility(recipe, request);

        if (!string.IsNullOrWhiteSpace(request.Goal))
        {
            var normalizedGoal = NormalizeText(request.Goal);

            if ((normalizedGoal.Contains("protein", StringComparison.OrdinalIgnoreCase) || normalizedGoal.Contains("proteina", StringComparison.OrdinalIgnoreCase))
                && recipe.Nutrition.ProteinGrams >= 25)
            {
                score += 15;
            }

            if ((normalizedGoal.Contains("leve", StringComparison.OrdinalIgnoreCase) || normalizedGoal.Contains("light", StringComparison.OrdinalIgnoreCase))
                && recipe.Nutrition.Calories <= 450)
            {
                score += 10;
            }

            if ((normalizedGoal.Contains("conforto", StringComparison.OrdinalIgnoreCase) || normalizedGoal.Contains("comfort", StringComparison.OrdinalIgnoreCase))
                && (recipe.Badge.Contains("COMFORT", StringComparison.OrdinalIgnoreCase) || recipe.Nutrition.CarbsGrams >= 55))
            {
                score += 10;
            }
        }

        return score;
    }

    private static int ScoreDietaryCompatibility(Recipe recipe, SuggestMealRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DietaryPreference))
        {
            return 0;
        }

        var preference = NormalizeText(request.DietaryPreference);
        var profile = NormalizeText(recipe.DietaryProfile);

        var isVegetarianRecipe = profile.Contains("vegetarian", StringComparison.OrdinalIgnoreCase) || profile.Contains("vegetariano", StringComparison.OrdinalIgnoreCase);
        var isHighProteinRecipe = profile.Contains("high protein", StringComparison.OrdinalIgnoreCase);

        if (preference.Contains("vegetarian", StringComparison.OrdinalIgnoreCase) || preference.Contains("vegetariano", StringComparison.OrdinalIgnoreCase))
        {
            return isVegetarianRecipe ? 25 : -20;
        }

        if (preference.Contains("carnivoro", StringComparison.OrdinalIgnoreCase) || preference.Contains("carnivore", StringComparison.OrdinalIgnoreCase))
        {
            // Strongly avoid vegetarian options for carnivore preference.
            if (isVegetarianRecipe)
            {
                return -35;
            }

            return isHighProteinRecipe ? 20 : 5;
        }

        // Fallback contains-based matching for other preference words.
        return profile.Contains(preference, StringComparison.OrdinalIgnoreCase) ? 20 : 0;
    }

    private static string NormalizeText(string value)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        var formD = trimmed.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(formD.Length);

        foreach (var character in formD)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
