using OneMeal.Core.Entities;

namespace OneMeal.Application.Abstractions;

public interface IRecipeCatalog
{
    Task<IReadOnlyCollection<Recipe>> ListAsync(CancellationToken cancellationToken = default);
}
