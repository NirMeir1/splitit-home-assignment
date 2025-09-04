namespace SplititAssignment.Application.Common.Validation;

public sealed class ValidationException : Exception
{
    public ValidationResult Result { get; }
    public ValidationException(ValidationResult result)
        : base("Validation failed") => Result = result;
}
