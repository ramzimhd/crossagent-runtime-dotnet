using System.Collections.Generic;

namespace CrossAgents.Abstractions.Tools;

/// <summary>
/// Constraints applied to tool usage within a single session.
/// </summary>
public sealed record ToolPolicy
{
    /// <summary>If non-null, only tools whose names appear here may be invoked.</summary>
    public IReadOnlyCollection<string>? AllowedTools { get; init; }

    /// <summary>Tools whose names appear here may never be invoked.</summary>
    public IReadOnlyCollection<string>? ForbiddenTools { get; init; }

    /// <summary>Maximum number of tool calls a single session may issue.</summary>
    public int MaxCallsPerSession { get; init; } = 16;
}
