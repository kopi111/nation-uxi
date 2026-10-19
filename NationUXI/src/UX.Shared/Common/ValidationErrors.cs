namespace UX.Shared.Common;

public class ValidationErrors
{
    public List<string> Errors { get; set; } = new();
    public bool IsValid => Errors.Count == 0;

    public void AddError(string error) => Errors.Add(error);
    public void AddErrors(IEnumerable<string> errors) => Errors.AddRange(errors);
}
