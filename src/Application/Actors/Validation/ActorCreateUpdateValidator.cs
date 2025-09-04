using SplititAssignment.Application.Common.Validation;

namespace SplititAssignment.Application.Actors.Validation;

public static class ActorCreateUpdateValidator
{
    public static ValidationResult Validate(string name, int rank, List<string>? topMovies)
    {
        var vr = new ValidationResult();

        if (string.IsNullOrWhiteSpace(name))
            vr.Add("name", "Name is required.");

        if (!string.IsNullOrWhiteSpace(name) && name.Trim().Length > 256)
            vr.Add("name", "Name length must be ≤ 256.");

        if (rank <= 0)
            vr.Add("rank", "Rank must be greater than 0.");

        if (topMovies is not null && topMovies.Any(m => string.IsNullOrWhiteSpace(m)))
            vr.Add("topMovies", "TopMovies must not contain empty titles.");

        return vr;
    }
}
