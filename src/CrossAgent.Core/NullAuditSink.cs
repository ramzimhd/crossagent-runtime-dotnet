using System.Threading;
using System.Threading.Tasks;
using CrossAgent.Abstractions.Audit;

namespace CrossAgent.Core;

/// <summary>An audit sink that discards events. Useful for tests and policy-disabled audit.</summary>
public sealed class NullAuditSink : IAuditSink
{
    public static NullAuditSink Instance { get; } = new();

    private NullAuditSink() { }

    public Task WriteAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
