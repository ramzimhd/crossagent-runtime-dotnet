using System;
using System.Collections.Generic;
using CrossAgent.Abstractions.Tools;

namespace CrossAgent.Abstractions.Models;

/// <summary>Reason an adapter returned, in provider-neutral terms.</summary>
public enum ModelFinishReason
{
    Unknown = 0,
    Stop,
    Length,
    ToolCalls,
    ContentFilter,
    Error
}

/// <summary>
/// Provider-neutral response returned from an <see cref="IModelAdapter"/>.
/// Adapters MUST populate <see cref="ToolCalls"/> only when the underlying provider
/// produced tool/function calls; the runtime never infers tool calls from prose.
/// </summary>
public sealed record ModelResponse
{
    public string Content { get; init; } = string.Empty;

    public IReadOnlyList<ToolCall> ToolCalls { get; init; } = Array.Empty<ToolCall>();

    public ModelFinishReason FinishReason { get; init; } = ModelFinishReason.Unknown;

    /// <summary>Optional usage information (input/output tokens, etc.) provided by the adapter.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();
}
