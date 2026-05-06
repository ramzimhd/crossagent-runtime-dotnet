using System.Threading;
using System.Threading.Tasks;
using CrossAgents.Abstractions.Agents;

namespace CrossAgents.Abstractions.Patterns;

/// <summary>
/// An executable agent pattern. Implementations are stateless; per-session state
/// lives only in the <see cref="AgentContext"/> and in services scoped to the call.
/// </summary>
public interface IAgentPattern
{
    /// <summary>Static description used by the selector.</summary>
    PatternDescriptor Descriptor { get; }

    /// <summary>Run the pattern for one task within one session.</summary>
    Task<AgentResult> ExecuteAsync(
        AgentContext context,
        IPatternServices services,
        CancellationToken cancellationToken = default);
}
