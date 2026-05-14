using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using OneMeal.Application.Abstractions;
using OneMeal.Application.Contracts;
using OneMeal.Core.Entities;

namespace OneMeal.Ai.Services;

/// <summary>
/// Composes recipe suggestions using Ollama local model (gemma4) via REST API.
/// Implements fallback to deterministic logic if Ollama is unavailable or times out.
/// </summary>
public sealed class OllamaRecipeSuggestionComposer : IRecipeSuggestionComposer
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LocalRecipeSuggestionComposer _fallback;
    private const string OllamaModel = "gemma4";
    private static readonly SemaphoreSlim LogFileLock = new(1, 1);
    private static readonly string TranscriptLogPath = Path.Combine(AppContext.BaseDirectory, "logs", "ai-transcript.log");

    public OllamaRecipeSuggestionComposer(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _fallback = new LocalRecipeSuggestionComposer();
    }

    public async Task<RecipeComposition> ComposeAsync(
        Recipe recipe,
        SuggestMealRequest request,
        IReadOnlyCollection<string> usedIngredients,
        IReadOnlyCollection<string> missingIngredients,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(request);

        try
        {
            return await ComposeWithOllamaAsync(prompt, cancellationToken);
        }
        catch (Exception exception)
        {
            await AppendTranscriptAsync(prompt, null, $"FAILURE -> FALLBACK ({exception.GetType().Name}: {exception.Message})", cancellationToken);

            // Graceful fallback to deterministic logic if Ollama fails, times out, or is unavailable
            return await _fallback.ComposeAsync(recipe, request, usedIngredients, missingIngredients, cancellationToken);
        }
    }

    private async Task<RecipeComposition> ComposeWithOllamaAsync(
        string prompt,
        CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("OllamaClient");

        var requestPayload = new OllamaRequest
        {
            Model = OllamaModel,
            Messages = new[]
            {
                new { role = "user", content = prompt }
            },
            Stream = false,
            Temperature = 0.7f
        };

        var jsonContent = JsonSerializer.Serialize(requestPayload);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("/api/chat", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        await AppendTranscriptAsync(prompt, responseText, "SUCCESS", cancellationToken);

        var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(
            responseText,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (ollamaResponse?.Message?.Content == null)
            throw new InvalidOperationException("Empty response from Ollama");

        return new RecipeComposition
        {
            Justification = CleanJustification(ollamaResponse.Message.Content),
            UsedFallback = false
        };
    }

    private static async Task AppendTranscriptAsync(string prompt, string? rawResponse, string status, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(TranscriptLogPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var now = DateTimeOffset.Now;
        var builder = new StringBuilder();
        builder.AppendLine("================================================================================");
        builder.AppendLine($"Timestamp: {now:O}");
        builder.AppendLine($"Status: {status}");
        builder.AppendLine("Model: gemma4");
        builder.AppendLine("--- PROMPT (FULL) ---");
        builder.AppendLine(prompt);
        builder.AppendLine("--- RESPONSE (FULL) ---");
        builder.AppendLine(rawResponse ?? "<null>");
        builder.AppendLine();

        await LogFileLock.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(TranscriptLogPath, builder.ToString(), Encoding.UTF8, cancellationToken);
        }
        finally
        {
            LogFileLock.Release();
        }
    }

    private string BuildPrompt(SuggestMealRequest request)
    {
        var detectedLanguage = DetectInputLanguage(request);
        var responseLanguage = detectedLanguage == "pt" ? "Portuguese" : "English";
        var goal = request.Goal ?? "balanced";
        var userIngredients = request.Ingredients?.Any() == true
            ? string.Join(", ", request.Ingredients)
            : "none";
        var agendaContext = string.IsNullOrWhiteSpace(request.AgendaContext)
            ? "no specific agenda constraint"
            : request.AgendaContext.Trim();

        var dietaryConstraint = string.IsNullOrWhiteSpace(request.DietaryPreference)
            ? "no dietary restrictions"
            : request.DietaryPreference.Trim();

        return $@"You are a helpful meal planning AI. Create a brief, encouraging 1-2 sentence meal suggestion justification based only on the user input below.

User input (must be respected):
Ingredients entered by user: {userIngredients}
Available time entered by user: {request.AvailableMinutes} minutes
Goal entered by user: {goal}
Diet preference entered by user: {dietaryConstraint}
Agenda context entered by user: {agendaContext}
Detected input language: {responseLanguage}

Write your justification in {responseLanguage}. Be concise and encouraging. Mention at least one user input factor explicitly (ingredient, time, goal, dietary preference, or agenda). Do not mention any specific recipe name not present in user input. Just the explanation, no JSON or extras.";
    }

    private static string DetectInputLanguage(SuggestMealRequest request)
    {
        var fullInput = string.Join(" ",
            request.Ingredients ?? [],
            request.Goal ?? string.Empty,
            request.DietaryPreference ?? string.Empty,
            request.AgendaContext ?? string.Empty)
            .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(fullInput))
        {
            return "pt";
        }

        var portugueseHints = new[]
        {
            "ovos", "espinafre", "queijo", "azeite", "arroz", "frango", "tomate", "alho", "manjericao",
            "vegetariano", "carnivoro", "proteina", "janela", "reunioes", "lactose", "sem", "com",
            "rapido", "leve", "almoco", "jantar"
        };

        var englishHints = new[]
        {
            "eggs", "spinach", "cheese", "olive oil", "rice", "chicken", "tomato", "garlic", "basil",
            "vegetarian", "carnivore", "protein", "window", "meetings", "lactose", "without", "with",
            "quick", "light", "lunch", "dinner"
        };

        var ptScore = portugueseHints.Count(fullInput.Contains);
        var enScore = englishHints.Count(fullInput.Contains);

        if (ptScore == enScore)
        {
            // Prefer Portuguese in ties because UI defaults are Portuguese.
            return "pt";
        }

        return ptScore > enScore ? "pt" : "en";
    }

    private string CleanJustification(string rawResponse)
    {
        // Remove common prefixes/artifacts from model output
        var cleaned = rawResponse
            .Replace("```json", "")
            .Replace("```", "")
            .Replace("justification:", "")
            .Replace("Justification:", "")
            .Trim();

        // Limit to reasonable length
        if (cleaned.Length > 300)
            cleaned = cleaned[..300] + "...";

        return cleaned;
    }

    private sealed class OllamaRequest
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("messages")]
        public object[]? Messages { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("temperature")]
        public float? Temperature { get; set; }
    }

    private sealed class OllamaResponse
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("message")]
        public MessageContent? Message { get; set; }
    }

    private sealed class MessageContent
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
