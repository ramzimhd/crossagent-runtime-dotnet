namespace CrossAgents.Abstractions.Patterns;

/// <summary>
/// Computed requirements for a single (task, model) pair. The selector compares
/// these requirements against a candidate <see cref="PatternDescriptor"/>.
/// </summary>
public sealed record PatternRequirement
{
    public bool NeedsTools { get; init; }
    public bool NeedsMemory { get; init; }
    public bool NeedsJsonMode { get; init; }
    public bool NeedsValidation { get; init; }
    public bool NeedsMultiAgent { get; init; }
}
