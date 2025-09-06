using FluentAssertions;
using SplititAssignment.Application.Actors.Queries;
using SplititAssignment.Application.Actors.Validation;

namespace SplititAssignment.UnitTests.Actors;

public class ActorQueryValidatorTests
{
    [Fact]
    public void Invalid_PageAndSize()
    {
        var q = new ActorQuery { Page = 0, PageSize = 101 };
        var vr = ActorQueryValidator.Validate(q);
        vr.IsValid.Should().BeFalse();
        vr.Errors.Keys.Should().Contain(new[] { "page", "pageSize" });
    }

    [Fact]
    public void Invalid_RankRange()
    {
        var q = new ActorQuery { RankMin = 10, RankMax = 1 };
        var vr = ActorQueryValidator.Validate(q);
        vr.IsValid.Should().BeFalse();
        vr.Errors.Should().ContainKey("rank");
    }

    [Fact]
    public void Valid_Defaults()
    {
        var q = new ActorQuery();
        var vr = ActorQueryValidator.Validate(q);
        vr.IsValid.Should().BeTrue();
    }
}
