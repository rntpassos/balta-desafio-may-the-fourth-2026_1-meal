using OneMeal.Ai.Services;
using OneMeal.Application.Contracts;
using OneMeal.Application.Services;
using OneMeal.Infra.Catalog;

namespace OneMeal.UnitTests;

public sealed class MealSuggestionServiceTests
{
    [Fact]
    public async Task SuggestAsync_PrioritizesRecipeThatFitsTimeAndIngredients()
    {
        var service = new MealSuggestionService(new InMemoryRecipeCatalog(), new LocalRecipeSuggestionComposer());

        var response = await service.SuggestAsync(new SuggestMealRequest
        {
            Ingredients = ["eggs", "spinach", "cheese", "olive oil"],
            AvailableMinutes = 15,
            Goal = "high protein",
            DietaryPreference = "Vegetarian",
        });

        Assert.Equal("Turbo Omelet", response.RecipeName);
        Assert.True(response.UsedFallback);
        Assert.Contains("eggs", response.UsedIngredients);
    }

    [Fact]
    public async Task SuggestAsync_RecognizesPortugueseIngredientsForPastaRecipe()
    {
        var service = new MealSuggestionService(new InMemoryRecipeCatalog(), new LocalRecipeSuggestionComposer());

        var response = await service.SuggestAsync(new SuggestMealRequest
        {
            Ingredients = ["macarrao", "tomate", "alho", "manjericao", "azeite"],
            AvailableMinutes = 15,
            Goal = "conforto",
            DietaryPreference = "Vegetarian",
        });

        Assert.Equal("Nebula Pasta", response.RecipeName);
        Assert.Contains("pasta", response.UsedIngredients);
        Assert.Contains("tomato", response.UsedIngredients);
    }

    [Fact]
    public async Task SuggestAsync_RecognizesAccentedPortugueseIngredients()
    {
        var service = new MealSuggestionService(new InMemoryRecipeCatalog(), new LocalRecipeSuggestionComposer());

        var response = await service.SuggestAsync(new SuggestMealRequest
        {
            Ingredients = ["macarrão", "tomate", "alho", "manjericão", "azeite"],
            AvailableMinutes = 20,
            Goal = "conforto",
            DietaryPreference = "Vegetariano",
        });

        Assert.Equal("Nebula Pasta", response.RecipeName);
        Assert.Contains("pasta", response.UsedIngredients);
        Assert.Contains("garlic", response.UsedIngredients);
    }

    [Fact]
    public async Task SuggestAsync_AppliesCarnivorePenaltyToVegetarianRecipes()
    {
        var service = new MealSuggestionService(new InMemoryRecipeCatalog(), new LocalRecipeSuggestionComposer());

        var response = await service.SuggestAsync(new SuggestMealRequest
        {
            Ingredients = ["frango", "arroz", "alho"],
            AvailableMinutes = 20,
            Goal = "alta proteína",
            DietaryPreference = "Carnívoro",
        });

        Assert.Equal("Orbit Chicken Bowl", response.RecipeName);
    }
}
