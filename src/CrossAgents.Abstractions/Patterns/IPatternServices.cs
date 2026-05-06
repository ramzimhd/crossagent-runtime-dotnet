using CrossAgents.Abstractions.Audit;
using CrossAgents.Abstractions.Memory;
using CrossAgents.Abstractions.Models;
using CrossAgents.Abstractions.Policy;
using CrossAgents.Abstractions.Tools;

namespace CrossAgents.Abstractions.Patterns;

/// <summary>
/// Services exposed to a pattern during execution. Patterns must not reach
/// outside this surface; doing so would couple them to a particular runtime build.
/// </summary>
public interface IPatternServices
{
    string SessionId { get; }
    IModelAdapter Model { get; }
    IToolInvoker? Tools { get; }
    IMemoryProvider? Memory { get; }
    IAuditSink Audit { get; }
    IPolicyEngine Policy { get; }
    AgentPolicy EffectivePolicy { get; }
}
