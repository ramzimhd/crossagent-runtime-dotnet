using System.Collections.Generic;

namespace CrossAgents.Abstractions.Models;

/// <summary>
/// A registered model description. The <see cref="ProfileId"/> is the stable identifier
/// applications use to request a specific model when running a task; the
/// <see cref="Capabilities"/> drive pattern selection.
/// </summary>
public sealed record ModelProfile
{
    public required string ProfileId { get; init; }

    public required string DisplayName { get; init; }

    public ModelProvider Provider { get; init; } = ModelProvider.Unknown;

    public required ModelCapabilities Capabilities { get; init; }

    /// <summary>Optional free-form metadata (deployment region, tier, owner, etc.).</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();
}
