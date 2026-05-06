# Tooling

Tooling is optional. An application that doesn't need tools simply doesn't reference `CrossAgents.Tooling` and doesn't pass an `IToolInvoker` into `RuntimeOptions`. Patterns that don't require tools never see the layer.

## Layer responsibilities

`CrossAgents.Tooling` provides:

- **`ToolRegistry`** - the default `IToolInvoker`. It stores tools by name, normalises calls, validates arguments, and dispatches to the registered `ITool`.
- **`ToolValidator`** - a lightweight JSON Schema check that enforces the canonical "object with required properties" shape. Applications that need richer validation can replace it with their own.
- **`ToolExecutor`** - invokes a single tool and converts unexpected exceptions into a failed `ToolResult`. Tools never crash the session.
- **`ToolCallNormalizer`** - canonicalises a `ToolCall` before validation so trimmed names, missing call ids, and whitespace-only argument bodies are handled consistently.

## Invocation flow

1. A pattern produces a `ToolCall` (typically from a model response).
2. The pattern asks `services.Policy.EvaluateToolCall(task, toolName)`. A denial yields a `ToolCallRejected` audit event and the pattern continues without invoking the tool.
3. The pattern calls `services.Tools.InvokeAsync(toolCall)`.
4. The registry normalises the call, looks up the tool, validates the arguments, and dispatches.
5. The executor runs the tool, catches and converts unexpected exceptions, and returns a `ToolResult`.
6. The pattern records the result and decides what to do next.

## Authoring a tool

Implement `ITool`:

```csharp
public sealed class CalculatorTool : ITool
{
    public ToolDefinition Definition { get; } = new()
    {
        Name = "add",
        Description = "Returns a + b.",
        ParametersJsonSchema = """
        {
          "type": "object",
          "properties": {
            "a": { "type": "number" },
            "b": { "type": "number" }
          },
          "required": ["a", "b"]
        }
        """
    };

    public async Task<ToolResult> InvokeAsync(ToolCall call, CancellationToken cancellationToken = default)
    {
        using var doc = JsonDocument.Parse(call.ArgumentsJson);
        var sum = doc.RootElement.GetProperty("a").GetDouble()
                + doc.RootElement.GetProperty("b").GetDouble();

        return new ToolResult
        {
            CallId = call.CallId,
            ToolName = call.ToolName,
            Success = true,
            Output = sum.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
    }
}
```

Register it on a `ToolRegistry` and pass the registry to `RuntimeOptions.Tools`.

## Policy interaction

Tooling honors `AgentPolicy.AllowTools`, `AllowedTools`, and `ForbiddenTools`. When `AllowTools` is `false`, the runtime hands a `null` invoker to patterns regardless of what the application registered. Patterns that require tools then see `services.Tools == null` and decline cleanly.
