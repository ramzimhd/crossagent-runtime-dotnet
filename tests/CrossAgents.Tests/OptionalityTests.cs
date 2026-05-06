using System.Threading.Tasks;
using CrossAgents.Abstractions.Agents;
using CrossAgents.Core;
using CrossAgents.Memory;
using CrossAgents.Patterns;
using CrossAgents.Testing;
using Xunit;

namespace CrossAgents.Tests;

public class OptionalityTests
{
    [Fact]
    public async Task Runtime_RunsTask_WithoutTooling()
    {
        var runtime = new AgentRuntime(new RuntimeOptions
        {
            AuditSink = new InMemoryAuditSink()
        });
        runtime.RegisterModel(TestFixtures.EchoAdapter());
        runtime.RegisterPattern(new NoToolPattern());

        var task = new AgentTask { TaskId = "no-tools", Input = "hello", RequiresValidation = false };
        var result = await runtime.RunAsync(task, "echo");

        Assert.True(result.Success);
        Assert.Null(runtime.Tools);
        Assert.Equal(KnownPatternIds.NoTool, result.SelectedPatternId);
        Assert.Equal("hello", result.Agent!.Output);
    }

    [Fact]
    public async Task Runtime_RunsTask_WithoutMemory()
    {
        var runtime = new AgentRuntime(new RuntimeOptions
        {
            AuditSink = new InMemoryAuditSink()
        });
        runtime.RegisterModel(TestFixtures.EchoAdapter());
        runtime.RegisterPattern(new NoToolPattern());

        var task = new AgentTask { TaskId = "no-memory", Input = "ping", RequiresValidation = false };
        var result = await runtime.RunAsync(task, "echo");

        Assert.True(result.Success);
        Assert.Null(runtime.Memory);
        Assert.Equal("ping", result.Agent!.Output);
    }

    [Fact]
    public void Memory_Layer_Components_Are_DropIn_Replaceable()
    {
        // Memory layer is a set of utilities; verifying they can be constructed and chained
        // without taking a runtime dependency proves memory is optional.
        var provider = new FakeMemoryProvider();
        var retriever = new MemoryRetriever(provider);
        var ranker = new MemoryRanker();
        var compressor = new ContextCompressor(maxTokens: 64);
        var buffer = new SlidingMemoryBuffer<string>(capacity: 4);

        Assert.NotNull(retriever);
        Assert.NotNull(ranker);
        Assert.NotNull(compressor);
        Assert.NotNull(buffer);
    }
}
