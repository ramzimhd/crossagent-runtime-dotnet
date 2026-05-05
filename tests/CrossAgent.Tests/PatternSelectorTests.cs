using System.Collections.Generic;
using CrossAgent.Abstractions.Agents;
using CrossAgent.Abstractions.Patterns;
using CrossAgent.Abstractions.Policy;
using CrossAgent.Core;
using CrossAgent.Patterns;
using Xunit;

namespace CrossAgent.Tests;

public class PatternSelectorTests
{
    [Fact]
    public void Select_PrefersNoToolPattern_WhenValidationIsNotRequired()
    {
        var policy = new RuntimePolicyEngine(new AgentPolicy { RequireValidation = false });
        var selector = new PatternSelector(policy);
        var patterns = new IAgentPattern[] { new PlanExecuteValidatePattern(), new NoToolPattern() };

        var task = new AgentTask { TaskId = "t-1", Input = "answer", RequiresValidation = false };
        var profile = TestFixtures.EchoProfile();

        var result = selector.Select(task, profile, patterns);

        Assert.True(result.HasSelection);
        Assert.Equal(KnownPatternIds.NoTool, result.Pattern!.Descriptor.PatternId);
    }

    [Fact]
    public void Select_PrefersPlanExecuteValidate_WhenValidationIsRequired()
    {
        var policy = new RuntimePolicyEngine(new AgentPolicy());
        var selector = new PatternSelector(policy);
        var patterns = new IAgentPattern[] { new NoToolPattern(), new PlanExecuteValidatePattern() };

        var task = new AgentTask { TaskId = "t-2", Input = "validate this", RequiresValidation = true };
        var profile = TestFixtures.EchoProfile();

        var result = selector.Select(task, profile, patterns);

        Assert.True(result.HasSelection);
        Assert.Equal(KnownPatternIds.PlanExecuteValidate, result.Pattern!.Descriptor.PatternId);
    }

    [Fact]
    public void Select_RespectsTaskAllowList()
    {
        var policy = new RuntimePolicyEngine(new AgentPolicy());
        var selector = new PatternSelector(policy);
        var patterns = new IAgentPattern[] { new NoToolPattern(), new PlanExecuteValidatePattern() };

        var task = new AgentTask
        {
            TaskId = "t-3",
            Input = "x",
            RequiresValidation = true,
            AllowedPatternIds = new HashSet<string> { KnownPatternIds.NoTool }
        };

        var result = selector.Select(task, TestFixtures.EchoProfile(), patterns);

        Assert.True(result.HasSelection);
        Assert.Equal(KnownPatternIds.NoTool, result.Pattern!.Descriptor.PatternId);
    }

    [Fact]
    public void Select_FiltersByModelCapabilities()
    {
        var policy = new RuntimePolicyEngine(new AgentPolicy { RequireValidation = false });
        var selector = new PatternSelector(policy);
        var patterns = new IAgentPattern[] { new NoToolPattern(), new JsonPlanPattern() };
        var task = new AgentTask { TaskId = "t-4", Input = "x", RequiresValidation = false };

        var profileWithoutJson = TestFixtures.EchoProfile("noj", jsonMode: false);
        var resultWithoutJson = selector.Select(task, profileWithoutJson, patterns);
        Assert.True(resultWithoutJson.HasSelection);
        Assert.Equal(KnownPatternIds.NoTool, resultWithoutJson.Pattern!.Descriptor.PatternId);

        var profileWithJson = TestFixtures.EchoProfile("withj", jsonMode: true);
        var resultWithJson = selector.Select(task, profileWithJson, patterns);
        Assert.True(resultWithJson.HasSelection);
        // With JSON-capable model, the JSON plan still scores below NoTool when JSON isn't required by the task,
        // but when the task type asks for a Plan, JSON capability becomes relevant. Verify the capable model unlocks JsonPlan.
        var planTask = new AgentTask
        {
            TaskId = "t-4-plan",
            Input = "plan",
            Type = AgentTaskType.Plan,
            RequiresValidation = false
        };
        var planResult = selector.Select(planTask, profileWithJson, patterns);
        Assert.True(planResult.HasSelection);
        Assert.Equal(KnownPatternIds.JsonPlan, planResult.Pattern!.Descriptor.PatternId);
    }
}
