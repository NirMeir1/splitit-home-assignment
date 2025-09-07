using SplititAssignment.Domain.Abstractions;
using SplititAssignment.Domain.Entities;
using SplititAssignment.Domain.Enums;

namespace SplititAssignment.Infrastructure.Providers;

public sealed class StubRottenTomatoesProvider : IActorProvider
{
    public Task<IReadOnlyList<Actor>> FetchAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Actor> list = new List<Actor>
        {
            new()
            {
                Name = "Sample Actor A",
                Rank = 1001,
                Source = ProviderSource.RottenTomatoes
            },
            new()
            {
                Name = "Sample Actor B",
                Rank = 1002,
                Source = ProviderSource.RottenTomatoes
            }
        };
        return Task.FromResult(list);
    }
}
