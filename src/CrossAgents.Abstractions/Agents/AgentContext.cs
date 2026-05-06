using System.Collections.Generic;
using CrossAgents.Abstractions.Memory;
using CrossAgents.Abstractions.Models;

namespace CrossAgents.Abstractions.Agents;

/// <summary>
/// Read-only execution context handed to a pattern. Patterns receive a context
/// per session and never share mutable state through it.
/// </summary>
public sealed record AgentContext
{
    public required string SessionId { get; init; }

    public required AgentTask Task { get; init; }

    public required ModelProfile Model { get; init; }

    /// <summary>Optional active context produced by the memory layer.</summary>
    public ActiveContext? ActiveContext { get; init; }

    /// <summary>Free-form session-scoped properties applications may attach.</summary>
    public IReadOnlyDictionary<string, object> Properties { get; init; } =
        new Dictionary<string, object>();
}
