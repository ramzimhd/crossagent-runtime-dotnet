namespace CrossAgent.Abstractions.Patterns;

/// <summary>
/// Static description of a pattern. This is what the selector inspects; patterns
/// themselves are not invoked until one has been chosen.
/// </summary>
public sealed record PatternDescriptor
{
    public required string PatternId { get; init; }

    public required string Name { get; init; }

    public bool RequiresTools { get; init; }

    public bool RequiresMemory { get; init; }

    public bool RequiresNativeToolCalling { get; init; }

    public bool RequiresJsonMode { get; init; }

    public bool SupportsMultiAgent { get; init; }

    /// <summary>True when the pattern enforces an upper bound on its work (steps, time, calls).</summary>
    public bool IsBounded { get; init; }

    /// <summary>Effective maximum step count. Must be greater than zero when <see cref="IsBounded"/> is true.</summary>
    public int MaxSteps { get; init; }

    public PatternRiskLevel RiskLevel { get; init; } = PatternRiskLevel.Low;
}
