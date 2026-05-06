using System;
using System.Collections.Generic;

namespace CrossAgents.Patterns;

/// <summary>
/// Configuration for <see cref="BoundedReActPattern"/>. The pattern intentionally has no
/// no-argument constructor: every safety bound (max steps, allowed tools, timeout,
/// audit requirement) must be supplied. Misconfigured options are rejected by the
/// pattern's constructor so unbounded ReAct is impossible to instantiate.
/// </summary>
public sealed class BoundedReActOptions
{
    public required int MaxSteps { get; init; }

    public required IReadOnlyCollection<string> AllowedTools { get; init; }

    public required TimeSpan StepTimeout { get; init; }

    /// <summary>When true (the default) the pattern requires an audit sink and emits per-step events.</summary>
    public bool RequireAudit { get; init; } = true;

    /// <summary>When true, the pattern stops the moment the model returns no tool calls.</summary>
    public bool StopWhenModelEmitsNoToolCalls { get; init; } = true;

    public void Validate()
    {
        if (MaxSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxSteps), MaxSteps, "BoundedReActOptions.MaxSteps must be greater than zero. Unbounded ReAct is rejected by design.");
        }

        if (AllowedTools is null || AllowedTools.Count == 0)
        {
            throw new ArgumentException("BoundedReActOptions.AllowedTools must contain at least one tool. Unbounded ReAct is rejected by design.", nameof(AllowedTools));
        }

        if (StepTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(StepTimeout), StepTimeout, "BoundedReActOptions.StepTimeout must be greater than zero.");
        }

        if (!RequireAudit)
        {
            throw new ArgumentException("BoundedReActOptions.RequireAudit must be true; bounded ReAct execution requires audit logging.", nameof(RequireAudit));
        }
    }
}
