namespace UX.Shared.Contracts;

public record ValidateConfigResultDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
