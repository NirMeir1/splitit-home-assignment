namespace SplititAssignment.Application.Common.Validation;

public sealed class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public Dictionary<string, List<string>> Errors { get; } = new();

    public void Add(string field, string message)
    {
        if (!Errors.TryGetValue(field, out var list))
        {
            list = new List<string>();
            Errors[field] = list;
        }
        list.Add(message);
    }
}
