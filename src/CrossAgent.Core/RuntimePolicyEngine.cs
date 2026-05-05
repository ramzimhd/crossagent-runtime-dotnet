using System;
using System.Linq;
using CrossAgent.Abstractions.Agents;
using CrossAgent.Abstractions.Models;
using CrossAgent.Abstractions.Patterns;
using CrossAgent.Abstractions.Policy;

namespace CrossAgent.Core;

/// <summary>
/// Default <see cref="IPolicyEngine"/>. Combines runtime defaults, the active
/// task's allow/forbid lists, and capability gates to produce a single decision.
/// </summary>
public sealed class RuntimePolicyEngine : IPolicyEngine
{
    public RuntimePolicyEngine(AgentPolicy policy)
    {
        Policy = policy ?? throw new ArgumentNullException(nameof(policy));
        if (Policy.MaxSteps <= 0)
        {
            throw new ArgumentException("AgentPolicy.MaxSteps must be greater than zero.", nameof(policy));
        }
    }

    public AgentPolicy Policy { get; }

    public PolicyDecision EvaluatePatternSelection(AgentTask task, PatternDescriptor pattern, ModelProfile model)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(model);

        if (pattern.RiskLevel == PatternRiskLevel.Unbounded)
        {
            return PolicyDecision.Deny($"Pattern '{pattern.PatternId}' is unbounded; rejected by runtime.");
        }

        if (pattern.IsBounded && pattern.MaxSteps <= 0)
        {
            return PolicyDecision.Deny($"Pattern '{pattern.PatternId}' is marked bounded but has no MaxSteps.");
        }

        if (Policy.AllowedPatterns is { Count: > 0 } allow && !allow.Contains(pattern.PatternId))
        {
            return PolicyDecision.Deny($"Pattern '{pattern.PatternId}' is not in the allowed pattern list.");
        }

        if (Policy.ForbiddenPatterns is { Count: > 0 } forbid && forbid.Contains(pattern.PatternId))
        {
            return PolicyDecision.Deny($"Pattern '{pattern.PatternId}' is forbidden by policy.");
        }

        if (task.AllowedPatternIds is { Count: > 0 } taskAllow && !taskAllow.Contains(pattern.PatternId))
        {
            return PolicyDecision.Deny($"Pattern '{pattern.PatternId}' is not in the task's allowed pattern list.");
        }

        if (task.ForbiddenPatternIds is { Count: > 0 } taskForbid && taskForbid.Contains(pattern.PatternId))
        {
            return PolicyDecision.Deny($"Pattern '{pattern.PatternId}' is forbidden by the task.");
        }

        if (pattern.RequiresTools && !Policy.AllowTools)
        {
            return PolicyDecision.Deny($"Pattern '{pattern.PatternId}' requires tools but policy disables tools.");
        }

        if (pattern.RequiresMemory && !Policy.AllowMemory)
        {
            return PolicyDecision.Deny($"Pattern '{pattern.PatternId}' requires memory but policy disables memory.");
        }

        if (pattern.RequiresNativeToolCalling && !model.Capabilities.SupportsNativeToolCalling)
        {
            return PolicyDecision.Deny($"Model '{model.ProfileId}' does not support native tool calling required by '{pattern.PatternId}'.");
        }

        if (pattern.RequiresJsonMode && !(model.Capabilities.SupportsJsonMode || model.Capabilities.SupportsJsonSchema))
        {
            return PolicyDecision.Deny($"Model '{model.ProfileId}' does not support JSON mode required by '{pattern.PatternId}'.");
        }

        if (Policy.RequireValidation && task.RequiresValidation && pattern.PatternId == KnownPatternIds.NoTool && task.Type is AgentTaskType.Validate)
        {
            return PolicyDecision.Deny($"Task '{task.TaskId}' requires validation but pattern '{pattern.PatternId}' does not provide a validation phase.");
        }

        if (task.MaxSteps is int maxSteps && pattern.IsBounded && pattern.MaxSteps > maxSteps)
        {
            return PolicyDecision.Deny($"Pattern '{pattern.PatternId}' MaxSteps ({pattern.MaxSteps}) exceeds task MaxSteps ({maxSteps}).");
        }

        if (pattern.IsBounded && pattern.MaxSteps > Policy.MaxSteps)
        {
            return PolicyDecision.Deny($"Pattern '{pattern.PatternId}' MaxSteps ({pattern.MaxSteps}) exceeds policy MaxSteps ({Policy.MaxSteps}).");
        }

        return PolicyDecision.Allow();
    }

    public PolicyDecision EvaluateToolCall(AgentTask task, string toolName)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return PolicyDecision.Deny("Tool name was empty.");
        }

        if (!Policy.AllowTools)
        {
            return PolicyDecision.Deny("Tool calling is disabled by policy.");
        }

        if (Policy.AllowedTools is { Count: > 0 } allow && !allow.Contains(toolName))
        {
            return PolicyDecision.Deny($"Tool '{toolName}' is not in the allowed tool list.");
        }

        if (Policy.ForbiddenTools is { Count: > 0 } forbid && forbid.Contains(toolName))
        {
            return PolicyDecision.Deny($"Tool '{toolName}' is forbidden by policy.");
        }

        return PolicyDecision.Allow();
    }
}
