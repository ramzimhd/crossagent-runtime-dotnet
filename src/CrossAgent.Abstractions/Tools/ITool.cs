using System.Threading;
using System.Threading.Tasks;

namespace CrossAgent.Abstractions.Tools;

/// <summary>
/// A single tool that can be exposed to a model. Implementations must be safe to
/// invoke from multiple sessions concurrently and must not throw for ordinary
/// tool failures - they should return a <see cref="ToolResult"/> with
/// <see cref="ToolResult.Success"/> set to false instead.
/// </summary>
public interface ITool
{
    ToolDefinition Definition { get; }

    Task<ToolResult> InvokeAsync(ToolCall call, CancellationToken cancellationToken = default);
}
