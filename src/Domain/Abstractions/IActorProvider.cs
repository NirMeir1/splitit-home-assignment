public interface IActorProvider
{
    Task<IReadOnlyList<Actor>> FetchAsync(CancellationToken cancellationToken = default);
}
