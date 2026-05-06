namespace CrossAgents.Abstractions.Tools;

/// <summary>
/// The outcome of a tool invocation. Tools must populate either <see cref="Output"/>
/// (on success) or <see cref="Error"/> (on failure) and never both.
/// </summary>
public sealed record ToolResult
{
    public required string CallId { get; init; }

    public required string ToolName { get; init; }

    public bool Success { get; init; }

    /// <summary>Tool output payload, opaque to the runtime (commonly JSON or plain text).</summary>
    public string Output { get; init; } = string.Empty;

    public string? Error { get; init; }
}
