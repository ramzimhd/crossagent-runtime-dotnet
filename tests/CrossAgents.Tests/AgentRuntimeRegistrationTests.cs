using CrossAgents.Core;
using CrossAgents.Patterns;
using CrossAgents.Testing;
using Xunit;

namespace CrossAgents.Tests;

public class AgentRuntimeRegistrationTests
{
    [Fact]
    public void RegisterModel_AddsModelToRegistry()
    {
        var runtime = new AgentRuntime();
        var adapter = TestFixtures.EchoAdapter("model-a");

        runtime.RegisterModel(adapter);

        Assert.True(runtime.Models.ContainsKey("model-a"));
        Assert.Same(adapter, runtime.Models["model-a"]);
    }

    [Fact]
    public void RegisterModel_RejectsDuplicateProfileId()
    {
        var runtime = new AgentRuntime();
        runtime.RegisterModel(TestFixtures.EchoAdapter("dup"));

        Assert.Throws<System.InvalidOperationException>(() =>
            runtime.RegisterModel(TestFixtures.EchoAdapter("dup")));
    }

    [Fact]
    public void RegisterPattern_AddsPatternToRegistry()
    {
        var runtime = new AgentRuntime();
        var pattern = new NoToolPattern();

        runtime.RegisterPattern(pattern);

        Assert.Single(runtime.Patterns);
        Assert.Same(pattern, runtime.Patterns[0]);
    }
}
