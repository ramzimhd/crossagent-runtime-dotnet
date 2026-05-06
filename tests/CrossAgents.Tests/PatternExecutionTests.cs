using System.Threading.Tasks;
using CrossAgents.Abstractions.Agents;
using CrossAgents.Abstractions.Policy;
using CrossAgents.Patterns;
using CrossAgents.Testing;
using Xunit;

namespace CrossAgents.Tests;

public class PatternExecutionTests
{
    [Fact]
    public async Task NoToolPattern_ProducesDeterministicOutput()
    {
        var harness = new PatternTestHarness(TestFixtures.EchoAdapter())
        {
            Policy = new AgentPolicy { RequireValidation = false }
        };
        var task = new AgentTask { TaskId = "no-tool", Input = "ECHO ME", RequiresValidation = false };

        var result = await harness.RunAsync(new NoToolPattern(), task);

        Assert.Equal(AgentState.Completed, result.State);
        Assert.Equal("ECHO ME", result.Output);
    }

    [Fact]
    public async Task PlanExecuteValidatePattern_RunsThreePhasesAndReportsValidation()
    {
        var adapter = TestFixtures.ScriptedAdapter("scripted",
            "step 1; step 2; step 3",
            "the answer is 42",
            "PASS - looks complete");

        var harness = new PatternTestHarness(adapter);
        var task = new AgentTask { TaskId = "pev", Input = "Compute the answer.", RequiresValidation = true };

        var result = await harness.RunAsync(new PlanExecuteValidatePattern(), task);

        Assert.Equal(AgentState.Completed, result.State);
        Assert.Equal("the answer is 42", result.Output);
        Assert.True(result.ValidationPassed);
        Assert.Equal(3, adapter.Calls.Count);
    }

    [Fact]
    public async Task PlanExecuteValidatePattern_FlagsValidationFailure_WhenValidatorReportsFail()
    {
        var adapter = TestFixtures.ScriptedAdapter("scripted",
            "plan",
            "answer",
            "FAIL - missing detail");

        var harness = new PatternTestHarness(adapter);
        var task = new AgentTask { TaskId = "pev-fail", Input = "Compute.", RequiresValidation = true };

        var result = await harness.RunAsync(new PlanExecuteValidatePattern(), task);

        Assert.Equal(AgentState.Completed, result.State);
        Assert.False(result.ValidationPassed);
    }
}
