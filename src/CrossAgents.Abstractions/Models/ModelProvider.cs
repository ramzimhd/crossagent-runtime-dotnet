namespace CrossAgents.Abstractions.Models;

/// <summary>
/// Identifies the broad category of model provider behind a <see cref="ModelProfile"/>.
/// The framework does not ship adapters for any specific provider; this enum exists only
/// so policy and selection logic can reason about provenance.
/// </summary>
public enum ModelProvider
{
    Unknown = 0,
    OpenAI,
    Anthropic,
    Google,
    Mistral,
    AzureOpenAI,
    Bedrock,
    Local,
    Custom
}
