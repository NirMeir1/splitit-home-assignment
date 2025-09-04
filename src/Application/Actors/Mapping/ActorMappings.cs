using SplititAssignment.Application.Actors.Dtos;
using SplititAssignment.Domain.Entities;

namespace SplititAssignment.Application.Actors.Mapping;

public static class ActorMappings
{
    public static ActorListItemDto ToListItemDto(this Actor a)
        => new(a.Id, a.Name);

    public static ActorDetailsDto ToDetailsDto(this Actor a)
        => new()
        {
            Id = a.Id,
            Name = a.Name,
            Rank = a.Rank,
            ImageUrl = a.ImageUrl,
            KnownFor = a.KnownFor,
            PrimaryProfession = a.PrimaryProfession,
            TopMovies = a.TopMovies.AsReadOnly(),
            Source = a.Source.ToString(),
            ExternalId = a.ExternalId
        };

    public static void Apply(this Actor entity, ActorCreateUpdateDto dto)
    {
        entity.Name = dto.Name.Trim();
        entity.Rank = dto.Rank;
        entity.ImageUrl = dto.ImageUrl?.Trim();
        entity.KnownFor = dto.KnownFor?.Trim();
        entity.PrimaryProfession = dto.PrimaryProfession?.Trim();
        entity.TopMovies = dto.TopMovies is null
            ? new List<string>()
            : dto.TopMovies.Where(s => !string.IsNullOrWhiteSpace(s))
                           .Select(s => s.Trim())
                           .ToList();
    }
}
