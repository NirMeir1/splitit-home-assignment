using SplititAssignment.Application.Actors.Dtos;
using SplititAssignment.Domain.Entities;

namespace SplititAssignment.Application.Actors.Mapping;

public static class ActorMapping
{
    public static ActorListItemDto ToListItemDto(this Actor a)
        => new ActorListItemDto(a.Id, a.Name);

    public static ActorDetailsDto ToDetailsDto(this Actor a)
        => new ActorDetailsDto
        {
            Id = a.Id,
            Name = a.Name,
            Details = !string.IsNullOrWhiteSpace(a.KnownFor) ? a.KnownFor! : (a.PrimaryProfession ?? string.Empty),
            Type = "Actor",
            Rank = a.Rank,
            Source = a.Source.ToString() // enum -> string
        };

    public static void Apply(this Actor target, ActorCreateUpdateDto dto)
    {
        target.Name = dto.Name.Trim();
        target.Rank = dto.Rank;
        target.ImageUrl = dto.ImageUrl;
        target.KnownFor = dto.KnownFor;
        target.PrimaryProfession = dto.PrimaryProfession;
        target.TopMovies = dto.TopMovies ?? new List<string>();
    }
}
