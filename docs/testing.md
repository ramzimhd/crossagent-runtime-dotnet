# Testing

`CrossAgents.Testing` ships deterministic test doubles for everything the framework defines.

## Doubles

- **`FakeModelAdapter`** - in-process `IModelAdapter` whose responses come from a function or a fixed string. Records every request so tests can assert against it.
- **`FakeMemoryProvider`** - in-memory `IMemoryProvider` that filters items by query token overlap.
- **`FakeTool`** - `ITool` that returns whatever the test wires up, defaulting to echoing the arguments.
- **`InMemoryAuditSink`** - retains every audit event so tests can assert kinds, ordering, and metadata.
- **`PatternTestHarness`** - exercises a single `IAgentPattern` against an adapter without going through the runtime selector.
- **`DeterministicClock`** - a `TimeProvider` that advances only when the test calls `Advance`.

## Writing a pattern test

```csharp
[Fact]
public async Task Pattern_ProducesExpectedOutput()
{
    var harness = new PatternTestHarness(TestFixtures.EchoAdapter())
    {
        Policy = new AgentPolicy { RequireValidation = false }
    };

    var task = new AgentTask { TaskId = "t-1", Input = "hello", RequiresValidation = false };
    var result = await harness.RunAsync(new NoToolPattern(), task);

    Assert.Equal(AgentState.Completed, result.State);
    Assert.Equal("hello", result.Output);
}
```

## Writing a runtime test

```csharp
[Fact]
public async Task Runtime_PicksAndRunsAPattern()
{
    var sink = new InMemoryAuditSink();
    var runtime = new AgentRuntime(new RuntimeOptions { AuditSink = sink });
    runtime.RegisterModel(new FakeModelAdapter(profile, "PASS - ok"));
    runtime.RegisterPattern(new NoToolPattern());

    var task = new AgentTask { TaskId = "rt-1", Input = "x", RequiresValidation = false };
    var result = await runtime.RunAsync(task, profile.ProfileId);

    Assert.True(result.Success);
    Assert.Equal(KnownPatternIds.NoTool, result.SelectedPatternId);
    Assert.Contains(sink.Events, e => e.Kind == AuditEventKind.SessionCompleted);
}
```

## What the test layer does not do

- It never reaches a network. Tests using only `CrossAgents.Testing` cannot accidentally hit a provider.
- It never reads credentials. The framework defines no credential surface.
- It does not generate non-deterministic data. Identifiers are supplied by the application or constructed deterministically by the doubles.
