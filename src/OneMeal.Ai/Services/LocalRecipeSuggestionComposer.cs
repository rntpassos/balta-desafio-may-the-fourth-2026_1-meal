using OneMeal.Application.Abstractions;
using OneMeal.Application.Contracts;
using OneMeal.Core.Entities;

namespace OneMeal.Ai.Services;

public sealed class LocalRecipeSuggestionComposer : IRecipeSuggestionComposer
{
    public Task<RecipeComposition> ComposeAsync(
        Recipe recipe,
        SuggestMealRequest request,
        IReadOnlyCollection<string> usedIngredients,
        IReadOnlyCollection<string> missingIngredients,
        CancellationToken cancellationToken = default)
    {
        var agendaContext = string.IsNullOrWhiteSpace(request.AgendaContext)
            ? "encaixa no seu proximo bloco livre"
            : $"alinha com o contexto de agenda: {request.AgendaContext.Trim()}";

        var missingClause = missingIngredients.Count == 0
            ? "Voce ja tem todos os ingredientes criticos em maos."
            : $"Faltam apenas {string.Join(", ", missingIngredients)} para completar a execucao.";

        var justification = $"{recipe.Name} foi priorizada porque usa {usedIngredients.Count} ingrediente(s) que voce ja possui, fecha em {recipe.TotalMinutes} minuto(s) e {agendaContext}. {missingClause}";

        return Task.FromResult(new RecipeComposition
        {
            Justification = justification,
            UsedFallback = true,
        });
    }
}
