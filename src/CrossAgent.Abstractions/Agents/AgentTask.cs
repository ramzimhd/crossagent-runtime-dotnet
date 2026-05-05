using System.Collections.Generic;

namespace CrossAgent.Abstractions.Agents;

/// <summary>
/// The unit of work submitted to <c>AgentRuntime</c>. Tasks are immutable so a
/// single instance can be safely re-used across retries or sessions.
/// </summary>
public sealed record AgentTask
{
    public required string TaskId { get; init; }

    public AgentTaskType Type { get; init; } = AgentTaskType.Generic;

    public required string Input { get; init; }

    /// <summary>If true, only patterns that support tool calling are eligible.</summary>
    public bool RequiresTools { get; init; }

    /// <summary>If true, only patterns that consume an active context are eligible.</summary>
    public bool RequiresMemory { get; init; }

    /// <summary>If true, only patterns that expose a validation phase are eligible.</summary>
    public bool RequiresValidation { get; init; } = true;

    /// <summary>Optional task-level upper bound on bounded patterns. Null defers to policy.</summary>
    public int? MaxSteps { get; init; }

    /// <summary>If non-null, only the listed patterns are eligible for this task.</summary>
    public IReadOnlyCollection<string>? AllowedPatternIds { get; init; }

    /// <summary>If non-null, the listed patterns are forbidden for this task.</summary>
    public IReadOnlyCollection<string>? ForbiddenPatternIds { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();
}
