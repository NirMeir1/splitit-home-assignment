using SplititAssignment.Application.Common.Validation;

namespace SplititAssignment.Application.Actors.Validation;

public static class ActorCreateUpdateValidator
{
    public static ValidationResult Validate(string name, string? details, string type, int rank, string source)
    {
        var vr = new ValidationResult();

        if (string.IsNullOrWhiteSpace(name))
            vr.Add("name", "Name is required.");

        if (!string.IsNullOrWhiteSpace(name) && name.Trim().Length > 256)
            vr.Add("name", "Name length must be <= 256.");

        if (string.IsNullOrWhiteSpace(type))
            vr.Add("type", "Type is required.");

        if (string.IsNullOrWhiteSpace(source))
            vr.Add("source", "Source is required.");

        if (rank < 0)
            vr.Add("rank", "Rank must be greater than or equal to 0.");

        return vr;
    }
}


