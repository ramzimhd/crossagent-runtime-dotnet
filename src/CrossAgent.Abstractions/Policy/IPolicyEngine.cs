using CrossAgent.Abstractions.Agents;
using CrossAgent.Abstractions.Models;
using CrossAgent.Abstractions.Patterns;

namespace CrossAgent.Abstractions.Policy;

/// <summary>
/// Application-replaceable evaluator for policy rules. The runtime ships with a
/// default implementation; advanced applications may substitute their own.
/// </summary>
public interface IPolicyEngine
{
    /// <summary>The active policy used by this engine.</summary>
    AgentPolicy Policy { get; }

    /// <summary>Decide whether a candidate pattern may be selected for the given task and model.</summary>
    PolicyDecision EvaluatePatternSelection(AgentTask task, PatternDescriptor pattern, ModelProfile model);

    /// <summary>Decide whether a tool call is permitted for the given task.</summary>
    PolicyDecision EvaluateToolCall(AgentTask task, string toolName);
}
