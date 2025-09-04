namespace SplititAssignment.Application.Actors.Queries;

public sealed class ActorQuery
{
    public string? Name { get; init; }
    public int? RankMin { get; init; }
    public int? RankMax { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "rank";  // rank | name
    public string SortDir { get; init; } = "asc";  // asc | desc
}
