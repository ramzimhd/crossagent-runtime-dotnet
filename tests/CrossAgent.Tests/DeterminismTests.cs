using System.Threading.Tasks;
using CrossAgent.Abstractions.Agents;
using CrossAgent.Core;
using CrossAgent.Patterns;
using CrossAgent.Testing;
using Xunit;

namespace CrossAgent.Tests;

public class DeterminismTests
{
    [Fact]
    public async Task SameTask_ProducesIdenticalOutput_WithFakeAdapter()
    {
        // Asserts the framework requires no external services for unit testing:
        // two runs with the same fake adapter produce the same output.
        var runtime = new AgentRuntime(new RuntimeOptions { AuditSink = new InMemoryAuditSink() });
        runtime.RegisterModel(TestFixtures.EchoAdapter());
        runtime.RegisterPattern(new NoToolPattern());

        var task = new AgentTask { TaskId = "det", Input = "deterministic", RequiresValidation = false };
        var first = await runtime.RunAsync(task, "echo");
        var second = await runtime.RunAsync(task, "echo");

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.Equal(first.Agent!.Output, second.Agent!.Output);
    }
}
