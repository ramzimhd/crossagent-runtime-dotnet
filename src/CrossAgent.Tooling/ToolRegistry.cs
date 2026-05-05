using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrossAgent.Abstractions.Tools;

namespace CrossAgent.Tooling;

/// <summary>
/// Default <see cref="IToolInvoker"/>. Looks up tools by name, normalises and validates calls,
/// and dispatches to the registered <see cref="ITool"/>. Registration is by name and is
/// case-sensitive to match how providers report tool calls.
/// </summary>
public sealed class ToolRegistry : IToolInvoker
{
    private readonly Dictionary<string, ITool> _tools = new(StringComparer.Ordinal);
    private readonly ToolValidator _validator;
    private readonly ToolExecutor _executor;
    private readonly ToolCallNormalizer _normalizer;

    public ToolRegistry()
        : this(new ToolValidator(), new ToolExecutor(), new ToolCallNormalizer()) { }

    public ToolRegistry(ToolValidator validator, ToolExecutor executor, ToolCallNormalizer normalizer)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
    }

    /// <summary>Number of tools currently registered.</summary>
    public int Count => _tools.Count;

    public ToolRegistry Register(ITool tool)
    {
        ArgumentNullException.ThrowIfNull(tool);
        if (string.IsNullOrWhiteSpace(tool.Definition.Name))
        {
            throw new ArgumentException("Tool.Definition.Name must be non-empty.", nameof(tool));
        }
        if (!_tools.TryAdd(tool.Definition.Name, tool))
        {
            throw new InvalidOperationException($"A tool named '{tool.Definition.Name}' is already registered.");
        }
        return this;
    }

    public bool TryGet(string toolName, out ITool? tool)
    {
        if (!string.IsNullOrWhiteSpace(toolName) && _tools.TryGetValue(toolName, out var found))
        {
            tool = found;
            return true;
        }
        tool = null;
        return false;
    }

    public IReadOnlyList<ToolDefinition> GetDefinitions()
    {
        var list = new List<ToolDefinition>(_tools.Count);
        foreach (var tool in _tools.Values)
        {
            list.Add(tool.Definition);
        }
        return list;
    }

    public async Task<ToolResult> InvokeAsync(ToolCall call, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(call);
        var normalized = _normalizer.Normalize(call);

        if (!_tools.TryGetValue(normalized.ToolName, out var tool))
        {
            return new ToolResult
            {
                CallId = normalized.CallId,
                ToolName = normalized.ToolName,
                Success = false,
                Error = $"Unknown tool '{normalized.ToolName}'."
            };
        }

        var validation = _validator.Validate(tool.Definition, normalized);
        if (!validation.IsValid)
        {
            return new ToolResult
            {
                CallId = normalized.CallId,
                ToolName = normalized.ToolName,
                Success = false,
                Error = validation.Reason ?? "Invalid arguments."
            };
        }

        return await _executor.ExecuteAsync(tool, normalized, cancellationToken).ConfigureAwait(false);
    }
}
