using Microsoft.EntityFrameworkCore;
using SplititAssignment.Application.Actors.Dtos;
using SplititAssignment.Application.Actors.Mapping;
using SplititAssignment.Application.Actors.Queries;
using SplititAssignment.Application.Common.Pagination;
using SplititAssignment.Domain.Entities;

namespace SplititAssignment.Infrastructure.Persistence;

public interface IActorRepository
{
    Task<bool> AnyAsync(CancellationToken ct);
    Task<bool> RankInUseAsync(int rank, Guid? excludingId, CancellationToken ct);

    Task<PagedResult<ActorListItemDto>> QueryAsync(ActorQuery q, CancellationToken ct);
    Task<Actor?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<Actor> AddAsync(Actor actor, CancellationToken ct);
    Task<Actor?> UpdateAsync(Actor actor, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

public sealed class ActorRepository : IActorRepository
{
    private readonly ActorsDbContext _db;

    public ActorRepository(ActorsDbContext db) => _db = db;

    public Task<bool> AnyAsync(CancellationToken ct)
        => _db.Actors.AsNoTracking().AnyAsync(ct);

    public Task<Actor?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Actors.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Actor> AddAsync(Actor actor, CancellationToken ct)
    {
        _db.Actors.Add(actor);
        await _db.SaveChangesAsync(ct);
        return actor;
    }

    public async Task<Actor?> UpdateAsync(Actor actor, CancellationToken ct)
    {
        var existing = await _db.Actors.FirstOrDefaultAsync(a => a.Id == actor.Id, ct);
        if (existing is null) return null;

        existing.Name = actor.Name;
        existing.Rank = actor.Rank;
        existing.ImageUrl = actor.ImageUrl;
        existing.KnownFor = actor.KnownFor;
        existing.PrimaryProfession = actor.PrimaryProfession;
        existing.TopMovies = actor.TopMovies;
        existing.Source = actor.Source;
        existing.ExternalId = actor.ExternalId;

        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.Actors.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (entity is null) return false;
        _db.Actors.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<bool> RankInUseAsync(int rank, Guid? excludingId, CancellationToken ct)
        => _db.Actors.AsNoTracking()
            .AnyAsync(a => a.Rank == rank && (excludingId == null || a.Id != excludingId.Value), ct);

    public async Task<PagedResult<ActorListItemDto>> QueryAsync(ActorQuery q, CancellationToken ct)
    {
        var query = _db.Actors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Name))
        {
            var term = q.Name.Trim();
            query = query.Where(a => a.Name.Contains(term));
        }

        if (q.RankMin is not null)
            query = query.Where(a => a.Rank >= q.RankMin);

        if (q.RankMax is not null)
            query = query.Where(a => a.Rank <= q.RankMax);

        // Default ordering for stable pagination (by rank ascending)
        query = query.OrderBy(a => a.Rank);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(a => a.ToListItemDto())
            .ToListAsync(ct);

        return new PagedResult<ActorListItemDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            Total = total
        };
    }
}
