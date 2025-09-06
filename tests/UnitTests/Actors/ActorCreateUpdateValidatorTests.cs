using FluentAssertions;
using SplititAssignment.Application.Actors.Validation;

namespace SplititAssignment.UnitTests.Actors;

public class ActorCreateUpdateValidatorTests
{
    [Fact]
    public void Invalid_WhenNameMissing_OrRankNotPositive()
    {
        var vr = ActorCreateUpdateValidator.Validate("", 0, null);
        vr.IsValid.Should().BeFalse();
        vr.Errors.Should().ContainKey("name");
        vr.Errors.Should().ContainKey("rank");
    }

    [Fact]
    public void Valid_WithTrimmedName_AndPositiveRank()
    {
        var vr = ActorCreateUpdateValidator.Validate("  Tom Hanks  ", 1, new() { "A", "B" });
        vr.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Invalid_WhenTopMoviesContainsEmpty()
    {
        var vr = ActorCreateUpdateValidator.Validate("A", 1, new() { "X", " " });
        vr.IsValid.Should().BeFalse();
        vr.Errors.Should().ContainKey("topMovies");
    }
}