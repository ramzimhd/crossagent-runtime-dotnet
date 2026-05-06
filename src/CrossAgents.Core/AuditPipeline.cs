using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrossAgents.Abstractions.Audit;

namespace CrossAgents.Core;

/// <summary>
/// Buffers audit events for one session, fans them out to a sink, and exposes
/// them for inclusion in the final result. The pipeline is single-session and
/// single-threaded by design; sessions get their own pipeline instance.
/// </summary>
public sealed class AuditPipeline
{
    private readonly IAuditSink _sink;
    private readonly TimeProvider _time;
    private readonly string _sessionId;
    private readonly List<AuditEvent> _captured = new();

    public AuditPipeline(IAuditSink sink, TimeProvider time, string sessionId)
    {
        _sink = sink ?? throw new ArgumentNullException(nameof(sink));
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
    }

    public IReadOnlyList<AuditEvent> Captured => _captured;

    public Task EmitAsync(AuditEventKind kind, string message, IReadOnlyDictionary<string, string>? properties = null, CancellationToken cancellationToken = default)
    {
        var evt = new AuditEvent
        {
            Timestamp = _time.GetUtcNow(),
            SessionId = _sessionId,
            Kind = kind,
            Message = message,
            Properties = properties ?? new Dictionary<string, string>()
        };
        _captured.Add(evt);
        return _sink.WriteAsync(evt, cancellationToken);
    }

    /// <summary>
    /// Returns an <see cref="IAuditSink"/> that writes through this pipeline so events
    /// emitted by patterns are captured in the same buffer as runtime-level events.
    /// </summary>
    public IAuditSink AsSink() => new PipelineSink(this);

    private sealed class PipelineSink : IAuditSink
    {
        private readonly AuditPipeline _pipeline;
        public PipelineSink(AuditPipeline pipeline) => _pipeline = pipeline;

        public Task WriteAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(auditEvent);
            // Re-stamp session and timestamp through the pipeline to preserve invariants.
            return _pipeline.EmitAsync(auditEvent.Kind, auditEvent.Message, auditEvent.Properties, cancellationToken);
        }
    }
}
