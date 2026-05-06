using System.Collections.Generic;
using CrossAgents.Abstractions.Agents;
using CrossAgents.Abstractions.Audit;

namespace CrossAgents.Core;

/// <summary>
/// Result of <see cref="AgentRuntime.RunAsync"/>. Contains either an
/// <see cref="AgentResult"/> or a <see cref="RuntimeError"/>, never both.
/// </summary>
public sealed record RuntimeResult
{
    public required string SessionId { get; init; }

    public required bool Success { get; init; }

    public AgentResult? Agent { get; init; }

    public RuntimeError? Error { get; init; }

    /// <summary>Audit events captured at the runtime level (selection, dispatch, completion).</summary>
    public IReadOnlyList<AuditEvent> RuntimeAuditEvents { get; init; } = System.Array.Empty<AuditEvent>();

    public string? SelectedPatternId { get; init; }

    public string? SelectedModelId { get; init; }
}
