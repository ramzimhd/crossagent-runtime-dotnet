namespace CrossAgent.Core;

/// <summary>Categorises a <see cref="RuntimeError"/> for callers that branch on it.</summary>
public enum RuntimeErrorCode
{
    None = 0,
    UnknownModel,
    NoEligiblePattern,
    PatternRejected,
    PatternNotRegistered,
    InvalidConfiguration,
    PolicyDenied,
    Cancelled,
    ExecutionFailed
}

/// <summary>
/// Structured runtime error returned in <c>RuntimeResult</c>. Errors are values,
/// not exceptions; the runtime only throws for programmer mistakes.
/// </summary>
public sealed record RuntimeError
{
    public required RuntimeErrorCode Code { get; init; }
    public required string Message { get; init; }
    public string? Detail { get; init; }
}
