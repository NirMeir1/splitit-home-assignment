namespace SplititAssignment.Api.Errors;

public static class ErrorResults
{
    public static IResult Validation(object details, string? message = null)
        => Results.Json(new
        {
            code = "validation_error",
            message = message ?? "One or more validation errors occurred.",
            details
        }, statusCode: StatusCodes.Status400BadRequest);

    public static IResult NotFound(string? message = null)
        => Results.Json(new
        {
            code = "not_found",
            message = message ?? "Resource not found."
        }, statusCode: StatusCodes.Status404NotFound);

    public static IResult Conflict(string field, string error, string? message = null)
        => Results.Json(new
        {
            code = "conflict",
            message = message ?? "Conflict.",
            details = new Dictionary<string, string[]>
            {
                [field] = new[] { error }
            }
        }, statusCode: StatusCodes.Status409Conflict);
}