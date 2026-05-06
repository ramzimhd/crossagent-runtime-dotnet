using System.Collections.Generic;

namespace CrossAgents.Abstractions.Memory;

/// <summary>Search request issued to a memory provider.</summary>
public sealed record MemoryQuery
{
    public required string Query { get; init; }

    public int Limit { get; init; } = 8;

    /// <summary>Optional metadata equality filters applied by the provider.</summary>
    public IReadOnlyDictionary<string, string> Filters { get; init; } =
        new Dictionary<string, string>();
}
