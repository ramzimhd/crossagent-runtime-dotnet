using System;
using System.Collections.Generic;
using System.Linq;
using CrossAgents.Abstractions.Agents;
using CrossAgents.Abstractions.Models;
using CrossAgents.Abstractions.Patterns;
using CrossAgents.Abstractions.Policy;

namespace CrossAgents.Core;

/// <summary>
/// Selects an <see cref="IAgentPattern"/> for a (task, model, policy) triple.
/// The selector is deterministic: given identical inputs and registration order
/// it always returns the same result.
/// </summary>
public sealed class PatternSelector
{
    private readonly IPolicyEngine _policy;

    public PatternSelector(IPolicyEngine policy)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    }

    /// <summary>
    /// Compute the requirements for a (task, model) pair. Patterns whose descriptors
    /// don't satisfy these requirements are filtered out before scoring.
    /// </summary>
    public static PatternRequirement DeriveRequirements(AgentTask task, ModelProfile model)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(model);

        return new PatternRequirement
        {
            NeedsTools = task.RequiresTools,
            NeedsMemory = task.RequiresMemory,
            NeedsValidation = task.RequiresValidation,
            NeedsJsonMode = task.Type is AgentTaskType.Plan or AgentTaskType.Extract or AgentTaskType.Decision,
            NeedsMultiAgent = false
        };
    }

    public PatternSelectionResult Select(
        AgentTask task,
        ModelProfile model,
        IReadOnlyList<IAgentPattern> patterns,
        string? preferredPatternId = null)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(patterns);

        if (patterns.Count == 0)
        {
            return PatternSelectionResult.None("No patterns are registered.");
        }

        var requirements = DeriveRequirements(task, model);
        var rejections = new List<string>(patterns.Count);

        var candidates = new List<(IAgentPattern Pattern, int Score)>(patterns.Count);
        foreach (var pattern in patterns)
        {
            if (!Satisfies(pattern.Descriptor, requirements, out var requirementError))
            {
                rejections.Add($"{pattern.Descriptor.PatternId}: {requirementError}");
                continue;
            }

            var policyDecision = _policy.EvaluatePatternSelection(task, pattern.Descriptor, model);
            if (!policyDecision.Allowed)
            {
                rejections.Add($"{pattern.Descriptor.PatternId}: {policyDecision.Reason}");
                continue;
            }

            candidates.Add((pattern, Score(pattern.Descriptor, requirements, model)));
        }

        if (candidates.Count == 0)
        {
            return PatternSelectionResult.None(string.Join("; ", rejections));
        }

        if (!string.IsNullOrEmpty(preferredPatternId))
        {
            IAgentPattern? preferredPattern = null;
            foreach (var candidate in candidates)
            {
                if (candidate.Pattern.Descriptor.PatternId == preferredPatternId)
                {
                    preferredPattern = candidate.Pattern;
                    break;
                }
            }

            if (preferredPattern is not null)
            {
                return PatternSelectionResult.Selected(preferredPattern, requirements, rejections);
            }
        }

        var winner = candidates
            .OrderByDescending(c => c.Score)
            .ThenBy(c => c.Pattern.Descriptor.RiskLevel)
            .ThenBy(c => c.Pattern.Descriptor.PatternId, StringComparer.Ordinal)
            .First();

        return PatternSelectionResult.Selected(winner.Pattern, requirements, rejections);
    }

    private static bool Satisfies(PatternDescriptor descriptor, PatternRequirement requirement, out string error)
    {
        if (requirement.NeedsTools && !descriptor.RequiresTools && !descriptor.RequiresNativeToolCalling)
        {
            error = "task requires tools but pattern does not use them";
            return false;
        }

        if (requirement.NeedsMemory && !descriptor.RequiresMemory)
        {
            error = "task requires memory but pattern does not consume an active context";
            return false;
        }

        if (requirement.NeedsMultiAgent && !descriptor.SupportsMultiAgent)
        {
            error = "task requires multi-agent execution but pattern is single-agent";
            return false;
        }

        // NeedsJsonMode is intentionally not a hard reject: tasks that benefit from JSON
        // can still run on a prose-only pattern, just at a lower score.
        error = string.Empty;
        return true;
    }

    private static int Score(PatternDescriptor descriptor, PatternRequirement requirement, ModelProfile model)
    {
        var score = 0;

        if (descriptor.IsBounded) score += 4;
        if (descriptor.RiskLevel == PatternRiskLevel.Low) score += 3;
        else if (descriptor.RiskLevel == PatternRiskLevel.Medium) score += 1;

        if (requirement.NeedsValidation && descriptor.PatternId == KnownPatternIds.PlanExecuteValidate) score += 5;

        if (requirement.NeedsJsonMode && descriptor.RequiresJsonMode) score += 2;

        if (descriptor.RequiresNativeToolCalling && model.Capabilities.SupportsNativeToolCalling) score += 1;

        if (!requirement.NeedsTools && !descriptor.RequiresTools && !descriptor.RequiresNativeToolCalling) score += 2;

        return score;
    }
}

/// <summary>Outcome of <see cref="PatternSelector.Select"/>.</summary>
public sealed record PatternSelectionResult
{
    public required bool HasSelection { get; init; }
    public IAgentPattern? Pattern { get; init; }
    public PatternRequirement? Requirement { get; init; }
    public IReadOnlyList<string> Rejections { get; init; } = Array.Empty<string>();
    public string? Reason { get; init; }

    public static PatternSelectionResult Selected(IAgentPattern pattern, PatternRequirement requirement, IReadOnlyList<string> rejections)
        => new() { HasSelection = true, Pattern = pattern, Requirement = requirement, Rejections = rejections };

    public static PatternSelectionResult None(string reason)
        => new() { HasSelection = false, Reason = reason };
}
