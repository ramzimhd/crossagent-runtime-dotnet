using System;
using System.Threading.Tasks;
using CrossAgent.Abstractions.Agents;
using CrossAgent.Abstractions.Models;
using CrossAgent.Core;
using CrossAgent.Patterns;
using CrossAgent.Testing;

namespace CrossAgent.Examples.MinimalRuntime;

/// <summary>
/// Smallest end-to-end demo of the CrossAgent Runtime. The example uses a
/// <see cref="FakeModelAdapter"/> so it never reaches a real LLM provider; the
/// goal is to show the wiring (model registration, pattern registration,
/// pattern selection, audit) rather than model behaviour.
/// </summary>
public static class Program
{
    public static async Task<int> Main()
    {
        var sink = new InMemoryAuditSink();
        var runtime = new AgentRuntime(new RuntimeOptions
        {
            AuditSink = sink
        });

        var profile = new ModelProfile
        {
            ProfileId = "demo-echo",
            DisplayName = "Demo echo model",
            Provider = ModelProvider.Custom,
            Capabilities = new ModelCapabilities
            {
                ProviderName = "demo",
                ModelId = "echo",
                SupportsStreaming = false,
                MaxContextTokens = 8192,
                IsLocal = true
            }
        };

        var adapter = new FakeModelAdapter(profile, (request, callIndex) => new ModelResponse
        {
            Content = callIndex switch
            {
                1 => "1. read input\n2. emit reply",
                2 => "Hello from CrossAgent Runtime.",
                _ => "PASS - response addresses the task."
            },
            FinishReason = ModelFinishReason.Stop
        });

        runtime
            .RegisterModel(adapter)
            .RegisterPattern(new NoToolPattern())
            .RegisterPattern(new PlanExecuteValidatePattern());

        var task = new AgentTask
        {
            TaskId = "demo-1",
            Type = AgentTaskType.Generic,
            Input = "Greet a developer who is just trying out the runtime.",
            RequiresValidation = true
        };

        var result = await runtime.RunAsync(task, profile.ProfileId);

        Console.WriteLine($"Session     : {result.SessionId}");
        Console.WriteLine($"Selected    : {result.SelectedPatternId} on {result.SelectedModelId}");
        Console.WriteLine($"Success     : {result.Success}");
        Console.WriteLine($"Output      : {result.Agent?.Output}");
        Console.WriteLine($"Validation  : {result.Agent?.ValidationPassed}");
        Console.WriteLine();
        Console.WriteLine("Audit trail:");
        foreach (var evt in sink.Events)
        {
            Console.WriteLine($"  [{evt.Timestamp:HH:mm:ss.fff}] {evt.Kind} :: {evt.Message}");
        }

        return result.Success ? 0 : 1;
    }
}
