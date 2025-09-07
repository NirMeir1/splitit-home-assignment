using SplititAssignment.Domain.Enums;

namespace SplititAssignment.Domain.Entities;

public sealed class Actor
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;
    public string Type { get; init; } = "Actor";

    public int Rank { get; set; }

    public ProviderSource Source { get; set; } = ProviderSource.Imdb;
}
