using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SplititAssignment.Application.Actors.Queries;
using SplititAssignment.Infrastructure.Persistence;
using SplititAssignment.Domain.Entities;

namespace SplititAssignment.UnitTests.Infrastructure;

public class ActorRepositoryTests
{
    private static ActorsDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<ActorsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ActorsDbContext(options);
    }

    [Fact]
    public async Task Query_Paginates_DefaultOrder()
    {
        using var db = NewDb();
        db.Actors.AddRange(
            new Actor { Name = "Charlie", Rank = 3 },
            new Actor { Name = "Alice", Rank = 1 },
            new Actor { Name = "Bob", Rank = 2 }
        );
        await db.SaveChangesAsync();

        var repo = new ActorRepository(db);
        var q = new ActorQuery { Page = 1, PageSize = 2 };

        var page = await repo.QueryAsync(q, CancellationToken.None);
        page.Total.Should().Be(3);
        page.Items.Select(i => i.Name).Should().BeEquivalentTo(new[] { "Alice", "Bob" }, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task RankInUse_Works_WithExclusion()
    {
        using var db = NewDb();
        var a1 = new Actor { Name = "A", Rank = 1 };
        var a2 = new Actor { Name = "B", Rank = 2 };
        db.Actors.AddRange(a1, a2);
        await db.SaveChangesAsync();

        var repo = new ActorRepository(db);
        (await repo.RankInUseAsync(1, null, default)).Should().BeTrue();
        (await repo.RankInUseAsync(1, a1.Id, default)).Should().BeFalse();
    }
}
