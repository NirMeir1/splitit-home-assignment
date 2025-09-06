using FluentAssertions;
using SplititAssignment.Application.Actors.Validation;

namespace SplititAssignment.UnitTests.Actors;

public class ActorCreateUpdateValidatorTests
{
    [Fact]
    public void Invalid_WhenNameMissing_OrRankNotPositive()
    {
        var vr = ActorCreateUpdateValidator.Validate("", details: "", type: "Actor", rank: -1, source: "Imdb");
        vr.IsValid.Should().BeFalse();
        vr.Errors.Should().ContainKey("name");
        vr.Errors.Should().ContainKey("rank");
    }

    [Fact]
    public void Valid_WithTrimmedName_AndPositiveRank()
    {
        var vr = ActorCreateUpdateValidator.Validate("  Tom Hanks  ", details: "", type: "Actor", rank: 1, source: "Imdb");
        vr.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Invalid_WhenTypeOrSourceMissing()
    {
        var vr = ActorCreateUpdateValidator.Validate("A", details: null, type: "", rank: 1, source: "");
        vr.IsValid.Should().BeFalse();
        vr.Errors.Should().ContainKey("type");
        vr.Errors.Should().ContainKey("source");
    }
}
