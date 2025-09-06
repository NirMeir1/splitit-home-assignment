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

        // Optional strings
        e.Property(a => a.ExternalId);

        // Store enum as string (readable)
        e.Property(a => a.Source).HasConversion<string>();
    }
}
