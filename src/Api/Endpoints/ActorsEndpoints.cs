using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using SplititAssignment.Api.Errors;
using SplititAssignment.Application.Actors.Dtos;
using SplititAssignment.Application.Actors.Mapping;
using SplititAssignment.Application.Actors.Queries;
using SplititAssignment.Application.Actors.Validation;
using SplititAssignment.Application.Common.Pagination;
using SplititAssignment.Domain.Entities;
using SplititAssignment.Domain.Enums;
using SplititAssignment.Infrastructure.Persistence;

namespace SplititAssignment.Api.Endpoints;

public static class ActorsEndpoints
{
    public static IEndpointRouteBuilder MapActorsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/actors").WithTags("Actors");

        group.MapGet("", async (
            [FromQuery] string? name,
            [FromQuery] int? rankMin,
            [FromQuery] int? rankMax,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            IActorRepository repo,
            CancellationToken ct) =>
        {
            var q = new ActorQuery
            {
                Name = name,
                RankMin = rankMin,
                RankMax = rankMax,
                Page = page.GetValueOrDefault(1),
                PageSize = pageSize.GetValueOrDefault(20)
            };

            var vr = ActorQueryValidator.Validate(q);
            if (!vr.IsValid) return ErrorResults.Validation(vr.Errors);

            var pageResult = await repo.QueryAsync(q, ct);
            return Results.Ok(pageResult);
        })
        .Produces<PagedResult<ActorListItemDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithOpenApi(op =>
        {
            op.Summary = "List actors (filters, pagination)";
            op.Parameters = new List<OpenApiParameter>
            {
                new() { Name = "name", In = ParameterLocation.Query, Description = "Filter by name (contains)" },
                new() { Name = "rankMin", In = ParameterLocation.Query, Description = "Min rank (inclusive)" },
                new() { Name = "rankMax", In = ParameterLocation.Query, Description = "Max rank (inclusive)" },
                new() { Name = "page", In = ParameterLocation.Query, Description = "Page (>=1, default 1)" },
                new() { Name = "pageSize", In = ParameterLocation.Query, Description = "Items per page (1..100, default 20)" },
                
            };
            return op;
        });

        group.MapGet("/{id}", async (string id, IActorRepository repo, HttpContext http, CancellationToken ct) =>
        {
            if (!Guid.TryParse(id, out var actorId))
                return ErrorResults.Validation(new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid id format; must be a GUID." }
                });

            var actor = await repo.GetByIdAsync(actorId, ct);
            if (actor is null) return ErrorResults.NotFound("Actor not found.");

            var dto = actor.ToDetailsDto();
            var envelope = new SplititAssignment.Api.Contracts.ActorGetResponse
            {
                Actor = dto,
                Errors = null,
                StatusCode = StatusCodes.Status200OK,
                TraceId = http.TraceIdentifier,
                IsSuccess = true
            };
            return Results.Ok(envelope);
        })
        .Produces<SplititAssignment.Api.Contracts.ActorGetResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi(op =>
        {
            op.Summary = "Get actor by id";
            op.Parameters = new List<OpenApiParameter>
            {
                new() { Name = "id", In = ParameterLocation.Path, Required = true, Description = "Actor id (string GUID)" }
            };
            var example = new OpenApiObject
            {
                ["actor"] = new OpenApiObject
                {
                    ["id"] = new OpenApiString("00000000-0000-0000-0000-000000000000"),
                    ["name"] = new OpenApiString("Alice Actor"),
                    ["details"] = new OpenApiString(""),
                    ["type"] = new OpenApiString("Actor"),
                    ["rank"] = new OpenApiInteger(1),
                    ["source"] = new OpenApiString("Imdb")
                },
                ["errors"] = new OpenApiNull(),
                ["statusCode"] = new OpenApiInteger(200),
                ["traceId"] = new OpenApiString("00-abc123..."),
                ["isSuccess"] = new OpenApiBoolean(true)
            };
            if (op.Responses.TryGetValue("200", out var r) && r.Content.TryGetValue("application/json", out var c))
            {
                c.Example = example;
            }
            return op;
        });

        group.MapPost("/{id}", async (string id,
                                      [FromBody] ActorUpsertRequestDto dto,
                                      IActorRepository repo,
                                      HttpContext http,
                                      CancellationToken ct) =>
        {
            if (!Guid.TryParse(id, out var actorId))
                return ErrorResults.Validation(new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid id format; must be a GUID." }
                });

            var vr = ActorCreateUpdateValidator.Validate(dto.Name, dto.Details, dto.Type, dto.Rank, dto.Source);
            if (!vr.IsValid) return ErrorResults.Validation(vr.Errors);

            if (await repo.RankInUseAsync(dto.Rank, excludingId: null, ct))
                return Results.Problem(title: "Duplicate rank", detail: $"rank {dto.Rank} is already taken", statusCode: StatusCodes.Status409Conflict);

            if (!Enum.TryParse<ProviderSource>(dto.Source, true, out var sourceEnum))
                return ErrorResults.Validation(new Dictionary<string, string[]>
                {
                    ["source"] = new[] { "Invalid source value." }
                });

            var entity = new Actor { Id = actorId, Source = sourceEnum };
            entity.Apply(dto);

            var created = await repo.AddAsync(entity, ct);
            var location = $"/actors/{created.Id}";

            var envelope = new SplititAssignment.Api.Contracts.ActorGetResponse
            {
                Actor = created.ToDetailsDto(),
                Errors = null,
                StatusCode = StatusCodes.Status201Created,
                TraceId = http.TraceIdentifier,
                IsSuccess = true
            };

            return Results.Created(location, envelope);
        })
        .Produces<SplititAssignment.Api.Contracts.ActorGetResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict)
        .WithOpenApi(op =>
        {
            op.Summary = "Create actor";
            op.Parameters = new List<OpenApiParameter>
            {
                new() { Name = "id", In = ParameterLocation.Path, Required = true, Description = "Actor id (string GUID)" }
            };
            if (op.RequestBody is null)
                op.RequestBody = new Microsoft.OpenApi.Models.OpenApiRequestBody();
            op.RequestBody.Content["application/json"] = new Microsoft.OpenApi.Models.OpenApiMediaType
            {
                Example = new Microsoft.OpenApi.Any.OpenApiObject
                {
                    ["name"] = new Microsoft.OpenApi.Any.OpenApiString("Alice Actor"),
                    ["details"] = new Microsoft.OpenApi.Any.OpenApiString("Known for Example Work"),
                    ["type"] = new Microsoft.OpenApi.Any.OpenApiString("Actor"),
                    ["rank"] = new Microsoft.OpenApi.Any.OpenApiInteger(1),
                    ["source"] = new Microsoft.OpenApi.Any.OpenApiString("Imdb")
                }
            };
            return op;
        });

        group.MapPut("/{id}", async (string id,
                                      [FromBody] ActorUpsertRequestDto dto,
                                      IActorRepository repo,
                                      HttpContext http,
                                      CancellationToken ct) =>
        {
            if (!Guid.TryParse(id, out var actorId))
                return ErrorResults.Validation(new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid id format; must be a GUID." }
                });

            var vr = ActorCreateUpdateValidator.Validate(dto.Name, dto.Details, dto.Type, dto.Rank, dto.Source);
            if (!vr.IsValid) return ErrorResults.Validation(vr.Errors);

            var existing = await repo.GetByIdAsync(actorId, ct);
            if (existing is null) return ErrorResults.NotFound("Actor not found.");

            if (await repo.RankInUseAsync(dto.Rank, excludingId: actorId, ct))
                return Results.Problem(title: "Duplicate rank", detail: $"rank {dto.Rank} is already taken", statusCode: StatusCodes.Status409Conflict);

            if (!Enum.TryParse<ProviderSource>(dto.Source, true, out var sourceEnum))
                return ErrorResults.Validation(new Dictionary<string, string[]>
                {
                    ["source"] = new[] { "Invalid source value." }
                });

            var updated = new Actor { Id = actorId, Source = sourceEnum };
            updated.Apply(dto);

            var saved = await repo.UpdateAsync(updated, ct);
            var envelope = new SplititAssignment.Api.Contracts.ActorGetResponse
            {
                Actor = saved!.ToDetailsDto(),
                Errors = null,
                StatusCode = StatusCodes.Status200OK,
                TraceId = http.TraceIdentifier,
                IsSuccess = true
            };
            return Results.Ok(envelope);
        })
        .Produces<SplititAssignment.Api.Contracts.ActorGetResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
        .WithOpenApi(op =>
        {
            op.Summary = "Update actor";
            op.Parameters = new List<OpenApiParameter>
            {
                new() { Name = "id", In = ParameterLocation.Path, Required = true, Description = "Actor id (string GUID)" }
            };
            if (op.RequestBody is null)
                op.RequestBody = new Microsoft.OpenApi.Models.OpenApiRequestBody();
            op.RequestBody.Content["application/json"] = new Microsoft.OpenApi.Models.OpenApiMediaType
            {
                Example = new Microsoft.OpenApi.Any.OpenApiObject
                {
                    ["name"] = new Microsoft.OpenApi.Any.OpenApiString("Alice Actor"),
                    ["details"] = new Microsoft.OpenApi.Any.OpenApiString("Known for Example Work"),
                    ["type"] = new Microsoft.OpenApi.Any.OpenApiString("Actor"),
                    ["rank"] = new Microsoft.OpenApi.Any.OpenApiInteger(2),
                    ["source"] = new Microsoft.OpenApi.Any.OpenApiString("Imdb")
                }
            };
            return op;
        });

        group.MapDelete("/{id}", async (string id, IActorRepository repo, CancellationToken ct) =>
        {
            if (!Guid.TryParse(id, out var actorId))
                return ErrorResults.Validation(new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid id format; must be a GUID." }
                });
            var ok = await repo.DeleteAsync(actorId, ct);
            return ok ? Results.NoContent() : ErrorResults.NotFound("Actor not found.");
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi(op =>
        {
            op.Summary = "Delete actor";
            op.Parameters = new List<OpenApiParameter>
            {
                new() { Name = "id", In = ParameterLocation.Path, Required = true, Description = "Actor id (string GUID)" }
            };
            return op;
        });

        return app;
    }
}
