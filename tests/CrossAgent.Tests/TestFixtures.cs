using System;
using CrossAgent.Abstractions.Models;
using CrossAgent.Testing;

namespace CrossAgent.Tests;

internal static class TestFixtures
{
    public static ModelProfile EchoProfile(
        string id = "echo",
        bool toolCalling = false,
        bool jsonMode = false) => new()
    {
        ProfileId = id,
        DisplayName = id,
        Provider = ModelProvider.Custom,
        Capabilities = new ModelCapabilities
        {
            ProviderName = "test",
            ModelId = id,
            SupportsNativeToolCalling = toolCalling,
            SupportsJsonMode = jsonMode,
            SupportsStreaming = false,
            MaxContextTokens = 8192,
            IsLocal = true
        }
    };

    public static FakeModelAdapter EchoAdapter(string id = "echo")
        => new(EchoProfile(id), (req, _) => new ModelResponse
        {
            Content = req.Prompt,
            FinishReason = ModelFinishReason.Stop
        });

    public static FakeModelAdapter ScriptedAdapter(string id, params string[] responses)
    {
        var index = 0;
        return new FakeModelAdapter(EchoProfile(id), (_, _) =>
        {
            var content = responses[Math.Min(index, responses.Length - 1)];
            index++;
            return new ModelResponse
            {
                Content = content,
                FinishReason = ModelFinishReason.Stop
            };
        });
    }

}
