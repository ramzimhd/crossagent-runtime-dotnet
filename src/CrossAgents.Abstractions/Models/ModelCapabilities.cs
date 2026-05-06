namespace CrossAgents.Abstractions.Models;

/// <summary>
/// Declarative capability descriptor for a model. The runtime uses these flags to
/// decide which patterns are eligible for a given task and model combination.
/// All values default to a conservative "unsupported" state so unknown models
/// degrade safely.
/// </summary>
public sealed record ModelCapabilities
{
    /// <summary>The provider name as reported by the adapter (free-form, e.g. "openai", "anthropic").</summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>The provider-specific model identifier (e.g. "gpt-4o-mini", "claude-3-5-sonnet").</summary>
    public string ModelId { get; init; } = string.Empty;

    /// <summary>True when the model exposes a first-class tool/function calling interface.</summary>
    public bool SupportsNativeToolCalling { get; init; }

    /// <summary>True when the model can be constrained to return JSON-only output.</summary>
    public bool SupportsJsonMode { get; init; }

    /// <summary>True when the model accepts a JSON Schema and is constrained to satisfy it.</summary>
    public bool SupportsJsonSchema { get; init; }

    /// <summary>True when the model accepts image inputs.</summary>
    public bool SupportsVision { get; init; }

    /// <summary>True when the model exposes incremental streaming responses.</summary>
    public bool SupportsStreaming { get; init; }

    /// <summary>Maximum number of context tokens the model can accept. Zero means unspecified.</summary>
    public int MaxContextTokens { get; init; }

    /// <summary>True when the model executes locally (e.g. an on-device runtime).</summary>
    public bool IsLocal { get; init; }
}
