using System;
using CrossAgents.Abstractions.Audit;
using CrossAgents.Abstractions.Memory;
using CrossAgents.Abstractions.Models;
using CrossAgents.Abstractions.Patterns;
using CrossAgents.Abstractions.Policy;
using CrossAgents.Abstractions.Tools;

namespace CrossAgents.Core;

/// <summary>
/// Concrete <see cref="IPatternServices"/> handed to a pattern by the runtime.
/// Honors policy gates: memory and tools surface as null when the policy
/// disables them, even if the runtime was configured with concrete instances.
/// </summary>
public sealed class PatternServices : IPatternServices
{
    public PatternServices(
        string sessionId,
        IModelAdapter model,
        IAuditSink audit,
        IPolicyEngine policy,
        AgentPolicy effectivePolicy,
        IToolInvoker? tools,
        IMemoryProvider? memory)
    {
        SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Audit = audit ?? throw new ArgumentNullException(nameof(audit));
        Policy = policy ?? throw new ArgumentNullException(nameof(policy));
        EffectivePolicy = effectivePolicy ?? throw new ArgumentNullException(nameof(effectivePolicy));
        Tools = effectivePolicy.AllowTools ? tools : null;
        Memory = effectivePolicy.AllowMemory ? memory : null;
    }

    public string SessionId { get; }
    public IModelAdapter Model { get; }
    public IToolInvoker? Tools { get; }
    public IMemoryProvider? Memory { get; }
    public IAuditSink Audit { get; }
    public IPolicyEngine Policy { get; }
    public AgentPolicy EffectivePolicy { get; }
}
