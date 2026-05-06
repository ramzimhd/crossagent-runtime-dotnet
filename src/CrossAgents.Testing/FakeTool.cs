using System;
using System.Threading;
using System.Threading.Tasks;
using CrossAgents.Abstractions.Tools;

namespace CrossAgents.Testing;

/// <summary>
/// Test tool that echoes its arguments by default. Tests can supply a custom
/// handler to return any payload or simulate failure.
/// </summary>
public sealed class FakeTool : ITool
{
    private readonly Func<ToolCall, ToolResult> _handler;

    public FakeTool(string name, string description, string parametersJsonSchema, Func<ToolCall, ToolResult>? handler = null)
    {
        Definition = new ToolDefinition
        {
            Name = name,
            Description = description,
            ParametersJsonSchema = parametersJsonSchema
        };
        _handler = handler ?? DefaultHandler;
    }

    public ToolDefinition Definition { get; }

    public Task<ToolResult> InvokeAsync(ToolCall call, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(call);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_handler(call));
    }

    private static ToolResult DefaultHandler(ToolCall call) => new()
    {
        CallId = call.CallId,
        ToolName = call.ToolName,
        Success = true,
        Output = call.ArgumentsJson
    };
}
