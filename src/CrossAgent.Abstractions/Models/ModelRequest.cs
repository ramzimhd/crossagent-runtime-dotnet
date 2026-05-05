using System.Collections.Generic;
using CrossAgent.Abstractions.Tools;

namespace CrossAgent.Abstractions.Models;

/// <summary>
/// Provider-neutral request submitted to an <see cref="IModelAdapter"/>.
/// Adapters translate this into the provider-specific transport.
/// </summary>
public sealed record ModelRequest
{
    public required string Prompt { get; init; }

    /// <summary>Optional system or role prefix.</summary>
    public string? System { get; init; }

    /// <summary>Tools advertised to the model. May be empty when tool use is disabled.</summary>
    public IReadOnlyList<ToolDefinition> Tools { get; init; } = Array.Empty<ToolDefinition>();

    /// <summary>If set, the response is expected to validate against this JSON schema.</summary>
    public string? JsonSchema { get; init; }

    /// <summary>If true, the response should be JSON; ignored if the model lacks the capability.</summary>
    public bool JsonMode { get; init; }

    public int? MaxOutputTokens { get; init; }

    public double? Temperature { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();
}
