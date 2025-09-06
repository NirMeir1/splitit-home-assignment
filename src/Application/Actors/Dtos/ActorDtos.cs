namespace SplititAssignment.Application.Actors.Dtos;

public sealed record ActorListItemDto(Guid Id, string Name);

public sealed class ActorDetailsDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Rank { get; init; }
    public string? ImageUrl { get; init; }
    public string? KnownFor { get; init; }
    public string? PrimaryProfession { get; init; }
    public IReadOnlyList<string> TopMovies { get; init; } = Array.Empty<string>();
    public string Source { get; init; } = "Imdb"; // string representation of enum
    public string? ExternalId { get; init; }
}

public sealed class ActorCreateUpdateDto
{
    public string Name { get; init; } = string.Empty;
    public int Rank { get; init; }
    public string? ImageUrl { get; init; }
    public string? KnownFor { get; init; }
    public string? PrimaryProfession { get; init; }
    public List<string>? TopMovies { get; init; } // optional in request
}