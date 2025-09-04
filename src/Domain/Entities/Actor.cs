namespace SplititAssignment.Domain.Entities;

public sealed class Actor
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public int Rank { get; set; }

    public string? ImageUrl { get; set; }

    public string? KnownFor { get; set; }

    public string? PrimaryProfession { get; set; }

    public List<string> TopMovies { get; set; } = new();

    public ProviderSource Source { get; set; } = ProviderSource.Imdb;

    public string? ExternalId { get; set; }
}