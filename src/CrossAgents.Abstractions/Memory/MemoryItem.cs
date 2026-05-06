using System.Collections.Generic;

namespace CrossAgents.Abstractions.Memory;

/// <summary>
/// A single retrievable memory record. Score, when supplied, is opaque to the
/// runtime and is used only by ranker implementations.
/// </summary>
public sealed record MemoryItem
{
    public required string Id { get; init; }

    public required string Content { get; init; }

    public double? Score { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();
}
