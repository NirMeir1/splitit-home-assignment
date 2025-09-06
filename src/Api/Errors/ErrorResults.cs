namespace SplititAssignment.Api.Errors;

public static class ErrorResults
{
    public static IResult Validation(Dictionary<string, List<string>> details, string? message = null)
        => Results.ValidationProblem(details.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()), title: message ?? "One or more validation errors occurred.");

    public static IResult Validation(Dictionary<string, string[]> details, string? message = null)
        => Results.ValidationProblem(details, title: message ?? "One or more validation errors occurred.");

    public static IResult NotFound(string? message = null)
        => Results.Json(new
        {
            code = "not_found",
            message = message ?? "Resource not found."
        }, statusCode: StatusCodes.Status404NotFound);

    public static IResult Conflict(string field, string error, string? message = null)
        => Results.Problem(title: message ?? "Conflict.", detail: $"{field} {error}", statusCode: StatusCodes.Status409Conflict);
}
