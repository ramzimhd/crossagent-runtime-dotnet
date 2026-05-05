using System;
using CrossAgent.Abstractions.Audit;
using CrossAgent.Abstractions.Memory;
using CrossAgent.Abstractions.Policy;
using CrossAgent.Abstractions.Tools;

namespace CrossAgent.Core;

/// <summary>
/// Configuration handed to <see cref="AgentRuntime"/> at construction time. All
/// services are optional except the audit sink; a <see cref="NullAuditSink"/>
/// is assigned when no sink is provided.
/// </summary>
public sealed class RuntimeOptions
{
    /// <summary>Default policy applied to every session unless overridden.</summary>
    public AgentPolicy DefaultPolicy { get; set; } = new();

    /// <summary>Audit sink for the runtime. Required when policy.RequireAudit is true.</summary>
    public IAuditSink? AuditSink { get; set; }

    /// <summary>Optional tooling layer. When null, tool calling is disabled regardless of pattern.</summary>
    public IToolInvoker? Tools { get; set; }

    /// <summary>Optional memory provider. When null, memory is disabled regardless of pattern.</summary>
    public IMemoryProvider? Memory { get; set; }

    /// <summary>Optional pattern preference; the selector inspects this before scoring.</summary>
    public string? PreferredPatternId { get; set; }

    /// <summary>Source of timestamps used for audit events. Defaults to <see cref="TimeProvider.System"/>.</summary>
    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    /// <summary>Source of session identifiers. Defaults to a Guid generator.</summary>
    public Func<string> SessionIdFactory { get; set; } = static () => Guid.NewGuid().ToString("n");
}
