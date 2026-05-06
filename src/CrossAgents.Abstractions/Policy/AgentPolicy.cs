using System.Collections.Generic;

namespace CrossAgents.Abstractions.Policy;

/// <summary>
/// Application-supplied constraints that apply to a session. Policies are
/// declarative: the runtime evaluates them, the patterns consume the result.
/// </summary>
public sealed record AgentPolicy
{
    /// <summary>If non-null, only patterns whose ids appear here may be selected.</summary>
    public IReadOnlyCollection<string>? AllowedPatterns { get; init; }

    /// <summary>Patterns whose ids appear here may never be selected.</summary>
    public IReadOnlyCollection<string>? ForbiddenPatterns { get; init; }

    /// <summary>If non-null, only tools whose names appear here may be invoked.</summary>
    public IReadOnlyCollection<string>? AllowedTools { get; init; }

    /// <summary>Tools whose names appear here may never be invoked.</summary>
    public IReadOnlyCollection<string>? ForbiddenTools { get; init; }

    /// <summary>Upper bound on bounded patterns. Must be greater than zero.</summary>
    public int MaxSteps { get; init; } = 8;

    /// <summary>When true, every session must produce audit events.</summary>
    public bool RequireAudit { get; init; } = true;

    /// <summary>When true, only patterns that include validation phases are eligible.</summary>
    public bool RequireValidation { get; init; } = true;

    /// <summary>When false, memory is unavailable to patterns regardless of session services.</summary>
    public bool AllowMemory { get; init; } = true;

    /// <summary>When false, tool calling is disabled regardless of session services.</summary>
    public bool AllowTools { get; init; } = true;
}
