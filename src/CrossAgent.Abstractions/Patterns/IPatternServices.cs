using CrossAgent.Abstractions.Audit;
using CrossAgent.Abstractions.Memory;
using CrossAgent.Abstractions.Models;
using CrossAgent.Abstractions.Policy;
using CrossAgent.Abstractions.Tools;

namespace CrossAgent.Abstractions.Patterns;

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
