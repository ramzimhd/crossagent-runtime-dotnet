using System.Collections.Generic;
using CrossAgent.Abstractions.Audit;

namespace CrossAgent.Abstractions.Agents;

/// <summary>
/// The outcome of executing a single pattern within a session. Patterns return
/// this to the runtime; the runtime wraps it in a <c>RuntimeResult</c>.
/// </summary>
public sealed record AgentResult
{
    public required string SessionId { get; init; }

    public required AgentState State { get; init; }

    public string Output { get; init; } = string.Empty;

    /// <summary>True when validation was performed and passed (or when validation was not required).</summary>
    public bool ValidationPassed { get; init; }

    /// <summary>Audit events emitted by this pattern's execution.</summary>
    public IReadOnlyList<AuditEvent> AuditEvents { get; init; } = System.Array.Empty<AuditEvent>();

    /// <summary>Populated when <see cref="State"/> is <see cref="AgentState.Failed"/> or <see cref="AgentState.Rejected"/>.</summary>
    public string? ErrorMessage { get; init; }
}
