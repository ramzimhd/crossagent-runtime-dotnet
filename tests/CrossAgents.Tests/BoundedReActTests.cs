using System;
using System.Collections.Generic;
using CrossAgents.Patterns;
using Xunit;

namespace CrossAgents.Tests;

public class BoundedReActTests
{
    [Fact]
    public void UnboundedMaxSteps_IsRejected()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new BoundedReActPattern(new BoundedReActOptions
        {
            MaxSteps = 0,
            AllowedTools = new[] { "noop" },
            StepTimeout = TimeSpan.FromSeconds(5)
        }));
        Assert.Contains("Unbounded ReAct is rejected by design", ex.Message);
    }

    [Fact]
    public void EmptyAllowedTools_IsRejected()
    {
        var ex = Assert.Throws<ArgumentException>(() => new BoundedReActPattern(new BoundedReActOptions
        {
            MaxSteps = 4,
            AllowedTools = Array.Empty<string>(),
            StepTimeout = TimeSpan.FromSeconds(5)
        }));
        Assert.Contains("at least one tool", ex.Message);
    }

    [Fact]
    public void NegativeTimeout_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoundedReActPattern(new BoundedReActOptions
        {
            MaxSteps = 4,
            AllowedTools = new HashSet<string> { "noop" },
            StepTimeout = TimeSpan.Zero
        }));
    }

    [Fact]
    public void DisabledAudit_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => new BoundedReActPattern(new BoundedReActOptions
        {
            MaxSteps = 4,
            AllowedTools = new[] { "noop" },
            StepTimeout = TimeSpan.FromSeconds(5),
            RequireAudit = false
        }));
    }

    [Fact]
    public void Bounded_Configuration_Succeeds()
    {
        var pattern = new BoundedReActPattern(new BoundedReActOptions
        {
            MaxSteps = 3,
            AllowedTools = new[] { "echo" },
            StepTimeout = TimeSpan.FromSeconds(2)
        });

        Assert.Equal(3, pattern.Descriptor.MaxSteps);
        Assert.True(pattern.Descriptor.IsBounded);
    }
}
