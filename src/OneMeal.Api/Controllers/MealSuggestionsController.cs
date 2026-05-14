using Microsoft.AspNetCore.Mvc;
using OneMeal.Application.Abstractions;
using OneMeal.Application.Contracts;

namespace OneMeal.Api.Controllers;

[ApiController]
[Route("api/meal-suggestions")]
public sealed class MealSuggestionsController(IMealSuggestionService mealSuggestionService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(MealSuggestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MealSuggestionResponse>> Post([FromBody] SuggestMealRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await mealSuggestionService.SuggestAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [exception.ParamName ?? "request"] = [exception.Message],
            }));
        }
    }
}
