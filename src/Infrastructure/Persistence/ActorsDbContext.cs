using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SplititAssignment.Domain.Entities;

namespace SplititAssignment.Infrastructure.Persistence;

public sealed class ActorsDbContext : DbContext
{
    public ActorsDbContext(DbContextOptions<ActorsDbContext> options) : base(options) { }

    public DbSet<Actor> Actors => Set<Actor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Actor>();

        e.HasKey(a => a.Id);

        e.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(256);

        e.Property(a => a.Rank)
            .IsRequired();

        // Map List<string> TopMovies via string column for deterministic InMemory behavior
        // + Add ValueComparer so EF can detect changes correctly.
        e.Property(a => a.TopMovies)
            .HasConversion(
                v => string.Join("||", v ?? new List<string>()),
                v => string.IsNullOrWhiteSpace(v)
                        ? new List<string>()
                        : v.Split("||", StringSplitOptions.RemoveEmptyEntries).ToList()
            );

        var listComparer = new ValueComparer<List<string>>(
            (l1, l2) =>
                ReferenceEquals(l1, l2) ||
                (l1 != null && l2 != null && l1.SequenceEqual(l2)),
            l => l == null ? 0 : l.Aggregate(0, (acc, v) => HashCode.Combine(acc, v.GetHashCode())),
            l => l == null ? new List<string>() : l.ToList()
        );

        e.Property(a => a.TopMovies).Metadata.SetValueComparer(listComparer);


        // Optional strings
        e.Property(a => a.ImageUrl);
        e.Property(a => a.KnownFor);
        e.Property(a => a.PrimaryProfession);
        e.Property(a => a.ExternalId);

        // Store enum as string (readable)
        e.Property(a => a.Source).HasConversion<string>();
    }
}