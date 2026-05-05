using System.Threading;
using System.Threading.Tasks;

namespace CrossAgent.Abstractions.Audit;

/// <summary>
/// Destination for audit events. Implementations must be safe to call from
/// multiple sessions concurrently. Sinks must not throw for ordinary write
/// failures; degrade by dropping the event and surfacing diagnostics elsewhere.
/// </summary>
public interface IAuditSink
{
    Task WriteAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}
