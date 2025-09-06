using SplititAssignment.Application.Actors.Queries;
using SplititAssignment.Application.Common.Validation;

namespace SplititAssignment.Application.Actors.Validation;

public static class ActorQueryValidator
{
    public static ValidationResult Validate(ActorQuery q)
    {
        var vr = new ValidationResult();

        if (q.Page < 1) vr.Add("page", "Page must be ≥ 1.");
        if (q.PageSize < 1 || q.PageSize > 100) vr.Add("pageSize", "PageSize must be between 1 and 100.");

        

        if (q.RankMin is not null && q.RankMax is not null && q.RankMin > q.RankMax)
            vr.Add("rank", "rankMin must be ≤ rankMax.");

        if (!string.IsNullOrWhiteSpace(q.Name) && q.Name.Trim().Length > 256)
            vr.Add("name", "Name filter length must be ≤ 256.");

        return vr;
    }
}
