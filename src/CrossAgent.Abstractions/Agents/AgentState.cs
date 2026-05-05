namespace CrossAgent.Abstractions.Agents;

/// <summary>Lifecycle state of an in-flight agent session.</summary>
public enum AgentState
{
    Pending = 0,
    Planning,
    Executing,
    Validating,
    Completed,
    Failed,
    Rejected
}
