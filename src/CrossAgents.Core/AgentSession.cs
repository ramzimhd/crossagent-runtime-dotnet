using System;
using System.Threading;
using System.Threading.Tasks;
using CrossAgents.Abstractions.Agents;
using CrossAgents.Abstractions.Models;
using CrossAgents.Abstractions.Patterns;
using CrossAgents.Abstractions.Policy;

namespace CrossAgents.Core;

/// <summary>
/// One execution of one task. Sessions are short-lived: created, run, returned
/// to the runtime, then discarded. They are not thread-safe and are not reused.
/// </summary>
public sealed class AgentSession
{
    private readonly AgentRuntime _owner;
    private readonly AuditPipeline _audit;
    private readonly IPolicyEngine _policy;
    private readonly AgentPolicy _effectivePolicy;
    private readonly IModelAdapter _model;
    private readonly IAgentPattern _pattern;
    private readonly AgentTask _task;

    internal AgentSession(
        AgentRuntime owner,
        string sessionId,
        AgentTask task,
        IModelAdapter model,
        IAgentPattern pattern,
        IPolicyEngine policy,
        AgentPolicy effectivePolicy,
        AuditPipeline audit)
    {
        _owner = owner;
        SessionId = sessionId;
        _task = task;
        _model = model;
        _pattern = pattern;
        _policy = policy;
        _effectivePolicy = effectivePolicy;
        _audit = audit;
    }

    public string SessionId { get; }

    internal async Task<AgentResult> RunAsync(CancellationToken cancellationToken)
    {
        var context = new AgentContext
        {
            SessionId = SessionId,
            Task = _task,
            Model = _model.Profile,
            ActiveContext = null
        };

        var services = new PatternServices(
            SessionId,
            _model,
            _audit.AsSink(),
            _policy,
            _effectivePolicy,
            _owner.Tools,
            _owner.Memory);

        try
        {
            return await _pattern.ExecuteAsync(context, services, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return new AgentResult
            {
                SessionId = SessionId,
                State = AgentState.Failed,
                ValidationPassed = false,
                ErrorMessage = "Cancelled."
            };
        }
        catch (Exception ex)
        {
            return new AgentResult
            {
                SessionId = SessionId,
                State = AgentState.Failed,
                ValidationPassed = false,
                ErrorMessage = $"{ex.GetType().Name}: {ex.Message}"
            };
        }
    }
}
