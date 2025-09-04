using Microsoft.AspNetCore.Mvc;
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

        // GET /actors  (filters + pagination) → list items + metadata
        group.MapGet("/", async (
            [AsParameters] ActorQuery query,
            IActorRepository repo,
            CancellationToken ct) =>
        {
            var vr = ActorQueryValidator.Validate(query);
            if (!vr.IsValid) return ErrorResults.Validation(vr.Errors);

            var page = await repo.QueryAsync(query, ct);
            return Results.Ok(new PagedResult<ActorListItemDto>
            {
                Items = page.Items,
                Page = page.Page,
                PageSize = page.PageSize,
                Total = page.Total
            });
        })
        .Produces<PagedResult<ActorListItemDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithOpenApi(op =>
        {
            op.Summary = "List actors (filters, sorting, pagination)";
            op.Parameters = new List<Microsoft.OpenApi.Models.OpenApiParameter>
            {
                new() { Name = "name", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Description = "Filter by name (contains)" },
                new() { Name = "rankMin", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Description = "Min rank (inclusive)" },
                new() { Name = "rankMax", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Description = "Max rank (inclusive)" },
                new() { Name = "page", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Description = "Page (≥1, default 1)" },
                new() { Name = "pageSize", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Description = "Items per page (1..100, default 20)" },
                new() { Name = "sortBy", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Description = "rank|name (default rank)" },
                new() { Name = "sortDir", In = Microsoft.OpenApi.Models.ParameterLocation.Query, Description = "asc|desc (default asc)" },
            };
            return op;
        });

        // GET /actors/{id} → details
        group.MapGet("/{id:guid}", async (Guid id, IActorRepository repo, CancellationToken ct) =>
        {
            var actor = await repo.GetByIdAsync(id, ct);
            return actor is null
                ? ErrorResults.NotFound("Actor not found.")
                : Results.Ok(actor.ToDetailsDto());
        })
        .Produces<ActorDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi(op =>
        {
            op.Summary = "Get actor by id";
            return op;
        });

        // POST /actors → create (201 + Location + full DTO)
        group.MapPost("/", async ([FromBody] ActorCreateUpdateDto dto,
                                  IActorRepository repo,
                                  CancellationToken ct) =>
        {
            var vr = ActorCreateUpdateValidator.Validate(dto.Name, dto.Rank, dto.TopMovies);
            if (!vr.IsValid) return ErrorResults.Validation(vr.Errors);

            if (await repo.RankInUseAsync(dto.Rank, excludingId: null, ct))
                return ErrorResults.Conflict("rank", "Duplicate rank");

            var entity = new Actor { Source = ProviderSource.Imdb }; // default source for manual adds
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

        // PUT /actors/{id} → update (200) or 404/409
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

        // DELETE /actors/{id} → 204 or 404
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