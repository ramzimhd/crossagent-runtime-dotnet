using System;
using System.Threading;
using System.Threading.Tasks;
using CrossAgents.Abstractions.Tools;

namespace CrossAgents.Tooling;

/// <summary>
/// Runs a single, already-validated tool call against an <see cref="ITool"/>. The executor
/// catches and converts unexpected tool exceptions into a failed <see cref="ToolResult"/>.
/// </summary>
public sealed class ToolExecutor
{
    public async Task<ToolResult> ExecuteAsync(ITool tool, ToolCall call, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tool);
        ArgumentNullException.ThrowIfNull(call);

        try
        {
            var result = await tool.InvokeAsync(call, cancellationToken).ConfigureAwait(false);
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new ToolResult
            {
                CallId = call.CallId,
                ToolName = call.ToolName,
                Success = false,
                Output = string.Empty,
                Error = $"{ex.GetType().Name}: {ex.Message}"
            };
        }
    }
}
