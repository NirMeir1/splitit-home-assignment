namespace SplititAssignment.Application.Actors.Queries;

public sealed class ActorQuery
{
    public string? Name { get; set; }
    public int? RankMin { get; set; }
    public int? RankMax { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
