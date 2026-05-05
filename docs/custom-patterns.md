# Custom patterns

Custom patterns are first-class citizens of the runtime. Implementing one is the recommended way to encode application-specific control flow.

## Minimum implementation

```csharp
public sealed class GreetingPattern : IAgentPattern
{
    public PatternDescriptor Descriptor { get; } = new()
    {
        PatternId = "greeting",
        Name = "Greeting",
        IsBounded = true,
        MaxSteps = 1,
        RiskLevel = PatternRiskLevel.Low
    };

    public async Task<AgentResult> ExecuteAsync(
        AgentContext context,
        IPatternServices services,
        CancellationToken cancellationToken = default)
    {
        var response = await services.Model.CompleteAsync(
            new ModelRequest { Prompt = $"Greet the user. Context: {context.Task.Input}" },
            cancellationToken);

        return new AgentResult
        {
            SessionId = context.SessionId,
            State = AgentState.Completed,
            Output = response.Content,
            ValidationPassed = true
        };
    }
}
```

Register it: `runtime.RegisterPattern(new GreetingPattern());`.

## Designing the descriptor

The `PatternDescriptor` is what the selector reasons about. Match it to your implementation:

- Set `RequiresTools` only if the pattern fails without a tool invoker.
- Set `RequiresMemory` only if the pattern fails without an active context.
- Set `RequiresNativeToolCalling` only if the pattern needs the model to emit explicit tool calls.
- Set `RequiresJsonMode` only if the pattern asks the model for JSON output.
- Set `IsBounded` to true and provide a meaningful `MaxSteps`. Unbounded risk levels are rejected at registration.

## Phases and audit

Patterns are encouraged to emit step events through `services.Audit`. Use `AuditEventKind.StepStarted` and `AuditEventKind.StepCompleted` for each phase, and `ValidationPassed` / `ValidationFailed` when a phase performs validation. Use `ToolCallRequested`, `ToolCallApproved`, `ToolCallRejected`, and `ToolResultReceived` for any tool interaction.

## Honoring policy

When the pattern proposes a tool call, ask `services.Policy.EvaluateToolCall(task, toolName)` first. If it returns a denial, emit `ToolCallRejected` with the policy reason and continue. Never invoke a tool the policy rejected.

When the pattern needs memory, check `services.Memory` for null before calling. Null indicates the policy disabled memory or the application did not configure a provider; either way, the pattern must degrade gracefully.

## Multi-agent considerations

Multi-agent patterns are valid as long as they remain bounded. Set `SupportsMultiAgent = true` on the descriptor. Each sub-agent should run inside its own `AgentContext` derived from the parent so audit trails remain attributable.
