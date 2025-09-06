using System.Text.Json.Serialization;
using SplititAssignment.Application.Actors.Dtos;

namespace SplititAssignment.Api.Contracts;

public sealed class ActorGetResponse
{
    [JsonPropertyName("Actor")]
    public required ActorDetailsDto Actor { get; init; }

    [JsonPropertyName("Errors")]
    public object? Errors { get; init; } = null;

    [JsonPropertyName("StatusCode")]
    public int StatusCode { get; init; }

    [JsonPropertyName("TraceId")]
    public string TraceId { get; init; } = string.Empty;

    [JsonPropertyName("IsSuccess")]
    public bool IsSuccess { get; init; }
}

