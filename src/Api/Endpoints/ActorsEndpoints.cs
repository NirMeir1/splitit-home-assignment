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

        group.MapGet("/{id:guid}", async (Guid id, IActorRepository repo, HttpContext http, CancellationToken ct) =>
        {
            var actor = await repo.GetByIdAsync(id, ct);
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
            // Example response
            var example = new OpenApiObject
            {
                ["Actor"] = new OpenApiObject
                {
                    ["Id"] = new OpenApiString("00000000-0000-0000-0000-000000000000"),
                    ["Name"] = new OpenApiString("Alice Actor"),
                    ["Details"] = new OpenApiString("Known for Example Work"),
                    ["Type"] = new OpenApiString("Actor"),
                    ["Rank"] = new OpenApiInteger(1),
                    ["Source"] = new OpenApiString("Imdb")
                },
                ["Errors"] = new OpenApiNull(),
                ["StatusCode"] = new OpenApiInteger(200),
                ["TraceId"] = new OpenApiString("00-abc123..."),
                ["IsSuccess"] = new OpenApiBoolean(true)
            };
            if (op.Responses.TryGetValue("200", out var r) && r.Content.TryGetValue("application/json", out var c))
            {
                c.Example = example;
            }
            return op;
        });

        group.MapPost("", async ([FromBody] ActorCreateUpdateDto dto,
                                 IActorRepository repo,
                                 CancellationToken ct) =>
        {
            var vr = ActorCreateUpdateValidator.Validate(dto.Name, dto.Rank, dto.TopMovies);
            if (!vr.IsValid) return ErrorResults.Validation(vr.Errors);

            if (await repo.RankInUseAsync(dto.Rank, excludingId: null, ct))
                return ErrorResults.Conflict("rank", "Duplicate rank");

            var entity = new Actor { Source = ProviderSource.Imdb };
            entity.Apply(dto);

            var created = await repo.AddAsync(entity, ct);
            return Results.Created($"/actors/{created.Id}", created.ToDetailsDto());
        })
        .Produces<ActorDetailsDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict)
        .WithOpenApi(op =>
        {
            op.Summary = "Create actor";
            return op;
        });

        group.MapPut("/{id:guid}", async (Guid id,
                                          [FromBody] ActorCreateUpdateDto dto,
                                          IActorRepository repo,
                                          CancellationToken ct) =>
        {
            var vr = ActorCreateUpdateValidator.Validate(dto.Name, dto.Rank, dto.TopMovies);
            if (!vr.IsValid) return ErrorResults.Validation(vr.Errors);

            var existing = await repo.GetByIdAsync(id, ct);
            if (existing is null) return ErrorResults.NotFound("Actor not found.");

            if (await repo.RankInUseAsync(dto.Rank, excludingId: id, ct))
                return ErrorResults.Conflict("rank", "Duplicate rank");

            var updated = new Actor { Id = id, Source = existing.Source, ExternalId = existing.ExternalId };
            updated.Apply(dto);

            var saved = await repo.UpdateAsync(updated, ct);
            return Results.Ok(saved!.ToDetailsDto());
        })
        .Produces<ActorDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
        .WithOpenApi(op =>
        {
            op.Summary = "Update actor";
            return op;
        });

        group.MapDelete("/{id:guid}", async (Guid id, IActorRepository repo, CancellationToken ct) =>
        {
            var ok = await repo.DeleteAsync(id, ct);
            return ok ? Results.NoContent() : ErrorResults.NotFound("Actor not found.");
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi(op =>
        {
            op.Summary = "Delete actor";
            return op;
        });

        return app;
    }
}
