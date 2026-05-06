using System;
using System.Collections.Generic;

namespace CrossAgents.Abstractions.Audit;

/// <summary>
/// A single audit event. Events are immutable, deterministic, and free of model
/// content payloads by design - sinks may attach additional context if needed.
/// </summary>
public sealed record AuditEvent
{
    public required DateTimeOffset Timestamp { get; init; }

    public required string SessionId { get; init; }

    public required AuditEventKind Kind { get; init; }

    public string Message { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, string> Properties { get; init; } =
        new Dictionary<string, string>();
}
