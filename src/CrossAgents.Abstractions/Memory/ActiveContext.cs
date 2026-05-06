using System.Collections.Generic;

namespace CrossAgents.Abstractions.Memory;

/// <summary>
/// The compressed and ranked working context produced by the memory layer.
/// Patterns consume this to ground a model call; they do not query memory directly.
/// </summary>
public sealed record ActiveContext
{
    public required IReadOnlyList<MemoryItem> Items { get; init; }

    /// <summary>Estimated token count of the rendered context. Zero means unknown.</summary>
    public int EstimatedTokens { get; init; }
}
