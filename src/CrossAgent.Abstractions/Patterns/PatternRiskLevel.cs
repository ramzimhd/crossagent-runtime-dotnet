namespace CrossAgent.Abstractions.Patterns;

/// <summary>
/// Classifies the risk profile of an agent pattern. The runtime uses this to
/// reject unsafe configurations (for example, an unbounded ReAct pattern)
/// regardless of policy permissiveness.
/// </summary>
public enum PatternRiskLevel
{
    Low = 0,
    Medium,
    High,
    /// <summary>An unbounded loop. Always rejected by the runtime.</summary>
    Unbounded
}
