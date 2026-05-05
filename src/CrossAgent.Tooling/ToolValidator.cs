using System;
using System.Collections.Generic;
using System.Text.Json;
using CrossAgent.Abstractions.Tools;

namespace CrossAgent.Tooling;

/// <summary>Outcome of validating a <see cref="ToolCall"/> against a <see cref="ToolDefinition"/>.</summary>
public sealed record ToolValidationResult
{
    public required bool IsValid { get; init; }
    public string? Reason { get; init; }
    public IReadOnlyList<string> Issues { get; init; } = Array.Empty<string>();

    public static ToolValidationResult Valid() => new() { IsValid = true };
    public static ToolValidationResult Invalid(string reason, IReadOnlyList<string>? issues = null) =>
        new() { IsValid = false, Reason = reason, Issues = issues ?? Array.Empty<string>() };
}

/// <summary>
/// Lightweight validator for tool call arguments. The validator parses
/// <see cref="ToolCall.ArgumentsJson"/> as JSON and, when the tool's parameter
/// schema is the canonical "object" form with declared properties, checks
/// presence of required keys and basic type compatibility. It is intentionally
/// permissive: applications that need full JSON Schema validation should plug
/// in a richer implementation.
/// </summary>
public sealed class ToolValidator
{
    public ToolValidationResult Validate(ToolDefinition definition, ToolCall call)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(call);

        if (!string.Equals(definition.Name, call.ToolName, StringComparison.Ordinal))
        {
            return ToolValidationResult.Invalid($"Call targets '{call.ToolName}' but definition is for '{definition.Name}'.");
        }

        JsonDocument argDoc;
        try
        {
            argDoc = JsonDocument.Parse(string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson);
        }
        catch (JsonException ex)
        {
            return ToolValidationResult.Invalid("Arguments are not valid JSON.", new[] { ex.Message });
        }

        using (argDoc)
        {
            JsonDocument schemaDoc;
            try
            {
                schemaDoc = JsonDocument.Parse(definition.ParametersJsonSchema);
            }
            catch (JsonException)
            {
                return argDoc.RootElement.ValueKind == JsonValueKind.Object
                    ? ToolValidationResult.Valid()
                    : ToolValidationResult.Invalid("Arguments must be a JSON object.");
            }

            using (schemaDoc)
            {
                var schemaRoot = schemaDoc.RootElement;
                var argRoot = argDoc.RootElement;

                if (argRoot.ValueKind != JsonValueKind.Object)
                {
                    return ToolValidationResult.Invalid("Arguments must be a JSON object.");
                }

                if (schemaRoot.ValueKind != JsonValueKind.Object)
                {
                    return ToolValidationResult.Valid();
                }

                if (schemaRoot.TryGetProperty("required", out var required) &&
                    required.ValueKind == JsonValueKind.Array)
                {
                    var issues = new List<string>();
                    foreach (var requiredKey in required.EnumerateArray())
                    {
                        if (requiredKey.ValueKind != JsonValueKind.String) continue;
                        var name = requiredKey.GetString();
                        if (name is null) continue;
                        if (!argRoot.TryGetProperty(name, out _))
                        {
                            issues.Add($"missing required property '{name}'");
                        }
                    }
                    if (issues.Count > 0)
                    {
                        return ToolValidationResult.Invalid("Required properties are missing.", issues);
                    }
                }

                if (schemaRoot.TryGetProperty("properties", out var propertiesElement) &&
                    propertiesElement.ValueKind == JsonValueKind.Object)
                {
                    var issues = new List<string>();
                    foreach (var arg in argRoot.EnumerateObject())
                    {
                        if (!propertiesElement.TryGetProperty(arg.Name, out var propSchema)) continue;
                        if (propSchema.ValueKind != JsonValueKind.Object) continue;
                        if (!propSchema.TryGetProperty("type", out var typeElement)) continue;
                        if (typeElement.ValueKind != JsonValueKind.String) continue;
                        var declared = typeElement.GetString();
                        if (!IsCompatible(declared, arg.Value.ValueKind))
                        {
                            issues.Add($"property '{arg.Name}' expected '{declared}' but got '{arg.Value.ValueKind}'");
                        }
                    }
                    if (issues.Count > 0)
                    {
                        return ToolValidationResult.Invalid("One or more properties have incompatible types.", issues);
                    }
                }
            }
        }

        return ToolValidationResult.Valid();
    }

    private static bool IsCompatible(string? declaredType, JsonValueKind kind) => declaredType switch
    {
        "string" => kind is JsonValueKind.String,
        "number" or "integer" => kind is JsonValueKind.Number,
        "boolean" => kind is JsonValueKind.True or JsonValueKind.False,
        "array" => kind is JsonValueKind.Array,
        "object" => kind is JsonValueKind.Object,
        "null" => kind is JsonValueKind.Null,
        _ => true
    };
}
