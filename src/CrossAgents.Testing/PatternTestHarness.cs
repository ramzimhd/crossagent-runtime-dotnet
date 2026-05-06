using System;
using System.Threading;
using System.Threading.Tasks;
using CrossAgents.Abstractions.Agents;
using CrossAgents.Abstractions.Memory;
using CrossAgents.Abstractions.Models;
using CrossAgents.Abstractions.Patterns;
using CrossAgents.Abstractions.Policy;
using CrossAgents.Abstractions.Tools;
using CrossAgents.Core;

namespace CrossAgents.Testing;

/// <summary>
/// Lightweight harness for exercising a single <see cref="IAgentPattern"/> in
/// isolation, without going through the full <see cref="AgentRuntime"/> path.
/// Useful for unit-testing pattern logic.
/// </summary>
public sealed class PatternTestHarness
{
    public PatternTestHarness(IModelAdapter model)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
    }

    public IModelAdapter Model { get; }
    public IToolInvoker? Tools { get; set; }
    public IMemoryProvider? Memory { get; set; }
    public InMemoryAuditSink Audit { get; } = new();
    public AgentPolicy Policy { get; set; } = new();

    public async Task<AgentResult> RunAsync(IAgentPattern pattern, AgentTask task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(task);

        var policyEngine = new RuntimePolicyEngine(Policy);
        var sessionId = Guid.NewGuid().ToString("n");
        var context = new AgentContext
        {
            SessionId = sessionId,
            Task = task,
            Model = Model.Profile
        };
        var services = new PatternServices(sessionId, Model, Audit, policyEngine, Policy, Tools, Memory);
        return await pattern.ExecuteAsync(context, services, cancellationToken).ConfigureAwait(false);
    }
}
