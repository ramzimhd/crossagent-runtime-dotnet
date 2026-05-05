using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrossAgent.Abstractions.Tools;

/// <summary>
/// Abstraction over the tooling layer used by the runtime and by patterns. This
/// keeps the core decoupled from any concrete <c>ToolRegistry</c> implementation
/// so applications can substitute their own.
/// </summary>
public interface IToolInvoker
{
    /// <summary>Returns true and the tool when a tool with the given name is registered.</summary>
    bool TryGet(string toolName, out ITool? tool);

    /// <summary>Definitions for every registered tool, in registration order.</summary>
    IReadOnlyList<ToolDefinition> GetDefinitions();

    /// <summary>
    /// Validate the call, invoke the tool, and return the result. Implementations must
    /// reject unknown tools and invalid arguments by returning a failed <see cref="ToolResult"/>
    /// rather than throwing.
    /// </summary>
    Task<ToolResult> InvokeAsync(ToolCall call, CancellationToken cancellationToken = default);
}
