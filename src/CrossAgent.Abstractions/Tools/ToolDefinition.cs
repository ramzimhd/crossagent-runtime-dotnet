namespace CrossAgent.Abstractions.Tools;

/// <summary>
/// Public description of a tool a model may invoke. The framework treats
/// <see cref="ParametersJsonSchema"/> as opaque text; concrete tooling layers are
/// responsible for validating arguments against it.
/// </summary>
public sealed record ToolDefinition
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    /// <summary>JSON Schema for the tool's argument object. Must be a valid JSON object schema.</summary>
    public required string ParametersJsonSchema { get; init; }
}
