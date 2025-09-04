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
                KnownFor = "Sample Film A",
                Source = ProviderSource.RottenTomatoes,
                ExternalId = "rt-sample-a"
            },
            new()
            {
                Name = "Sample Actor B",
                Rank = 1002,
                KnownFor = "Sample Film B",
                Source = ProviderSource.RottenTomatoes,
                ExternalId = "rt-sample-b"
            }
        };
        return Task.FromResult(list);
    }
}
