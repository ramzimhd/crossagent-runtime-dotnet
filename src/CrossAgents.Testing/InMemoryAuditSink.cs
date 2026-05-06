using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrossAgents.Abstractions.Audit;

namespace CrossAgents.Testing;

/// <summary>Audit sink that retains every event in order. Useful for assertions.</summary>
public sealed class InMemoryAuditSink : IAuditSink
{
    private readonly List<AuditEvent> _events = new();

    public IReadOnlyList<AuditEvent> Events => _events;

    public Task WriteAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        _events.Add(auditEvent);
        return Task.CompletedTask;
    }
}
