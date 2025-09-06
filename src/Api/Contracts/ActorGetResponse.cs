using System.Text.Json.Serialization;
using SplititAssignment.Application.Actors.Dtos;

namespace SplititAssignment.Api.Contracts;

public sealed class ActorGetResponse
{
    [JsonPropertyName("actor")]
    public required ActorDetailsDto Actor { get; init; }

    [JsonPropertyName("errors")]
    public object? Errors { get; init; } = null;

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; init; }

    [JsonPropertyName("traceId")]
    public string TraceId { get; init; } = string.Empty;

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }
}
