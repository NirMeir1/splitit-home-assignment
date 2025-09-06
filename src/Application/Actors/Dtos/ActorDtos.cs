using System.Text.Json.Serialization;

namespace SplititAssignment.Application.Actors.Dtos;

public sealed record ActorListItemDto(Guid Id, string Name);

public sealed class ActorDetailsDto
{
    [JsonPropertyName("Id")]
    public Guid Id { get; init; }

    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("Details")]
    public string Details { get; init; } = string.Empty;

    [JsonPropertyName("Type")]
    public string Type { get; init; } = "Actor";

    [JsonPropertyName("Rank")]
    public int Rank { get; init; }

    [JsonPropertyName("Source")]
    public string Source { get; init; } = "Imdb"; // enum string
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
