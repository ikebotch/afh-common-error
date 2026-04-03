namespace AFH.Common.Errors.Email.Options;

public sealed class ErrorEmailOptions
{
    public string? FromAddress { get; init; }

    public string? FromDisplayName { get; init; }

    public IReadOnlyCollection<string> ToAddresses { get; init; } = [];

    public IReadOnlyCollection<string> CcAddresses { get; init; } = [];

    public IReadOnlyCollection<string> BccAddresses { get; init; } = [];

    public string SubjectPrefix { get; init; } = "[AFH Error]";

    public bool IncludeDetails { get; init; } = true;
}
