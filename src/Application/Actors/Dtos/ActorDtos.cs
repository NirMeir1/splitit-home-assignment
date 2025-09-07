using System.Text.Json.Serialization;

namespace SplititAssignment.Application.Actors.Dtos;

public sealed record ActorListItemDto(Guid Id, string Name);

public sealed class ActorDetailsDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("details")]
    public string Details { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = "Actor";

    [JsonPropertyName("rank")]
    public int Rank { get; init; }

    [JsonPropertyName("source")]
    public string Source { get; init; } = "Imdb";
}

public sealed class ActorUpsertRequestDto
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("details")]
    public string Details { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("rank")]
    public int Rank { get; init; }

    [JsonPropertyName("source")]
    public string Source { get; init; } = string.Empty;
}
