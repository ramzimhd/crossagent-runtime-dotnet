namespace CrossAgent.Abstractions.Agents;

/// <summary>
/// The kind of work an <see cref="AgentTask"/> represents. The selector uses this
/// hint together with model capabilities and policy to decide which pattern to run.
/// </summary>
public enum AgentTaskType
{
    /// <summary>Generic open-ended task with no special expectations.</summary>
    Generic = 0,
    /// <summary>The task asks for a direct answer to a question.</summary>
    Question,
    /// <summary>The task asks for a structured plan.</summary>
    Plan,
    /// <summary>The task asks for structured extraction from input.</summary>
    Extract,
    /// <summary>The task asks for validation of an existing artifact.</summary>
    Validate,
    /// <summary>The task asks for a deterministic transformation.</summary>
    Transform,
    /// <summary>The task asks for a decision among a fixed set of outcomes.</summary>
    Decision
}
