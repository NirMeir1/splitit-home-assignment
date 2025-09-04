using SplititAssignment.Domain.Entities;

namespace SplititAssignment.Domain.Abstractions;

public interface IActorProvider
{
    Task<IReadOnlyList<Actor>> FetchAsync(CancellationToken cancellationToken = default);
}
