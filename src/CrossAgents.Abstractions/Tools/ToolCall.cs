namespace CrossAgents.Abstractions.Tools;

/// <summary>
/// A model's request to invoke a tool. <see cref="ArgumentsJson"/> is the raw JSON
/// produced by the model and is validated by the tooling layer before invocation.
/// </summary>
public sealed record ToolCall
{
    public required string CallId { get; init; }

    public required string ToolName { get; init; }

    public required string ArgumentsJson { get; init; }
}
