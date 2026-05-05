using System;
using System.Text.Json;
using CrossAgent.Abstractions.Tools;

namespace CrossAgent.Tooling;

/// <summary>
/// Canonicalises a <see cref="ToolCall"/> so downstream validators and tools see
/// consistent input. Specifically: trims tool names, defaults missing call ids,
/// and rewrites empty argument payloads to the empty object literal "{}".
/// </summary>
public sealed class ToolCallNormalizer
{
    public ToolCall Normalize(ToolCall call)
    {
        ArgumentNullException.ThrowIfNull(call);

        var name = call.ToolName?.Trim() ?? string.Empty;
        var callId = string.IsNullOrWhiteSpace(call.CallId) ? Guid.NewGuid().ToString("n") : call.CallId.Trim();
        var args = string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson.Trim();

        // Best-effort canonical JSON: re-serialise valid JSON to drop whitespace; leave invalid input
        // untouched so the validator can surface the parse error.
        try
        {
            using var doc = JsonDocument.Parse(args);
            args = JsonSerializer.Serialize(doc.RootElement);
        }
        catch (JsonException)
        {
            // Pass through; ToolValidator will reject it.
        }

        return new ToolCall
        {
            CallId = callId,
            ToolName = name,
            ArgumentsJson = args
        };
    }
}
