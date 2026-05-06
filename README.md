<p align="center">
  <img src="https://raw.githubusercontent.com/ramzimhd/crossagent-runtime-dotnet/main/logo.png" alt="Cross Agents Runtime" width="160" />
</p>

# Cross Agents Runtime (.NET)

Cross Agents Runtime is a provider-agnostic, model-adaptive agent runtime for building controlled mono-agent and multi-agent systems on .NET. It picks an execution pattern that fits a given task, model, policy, and operational constraints, then runs it inside an audited, bounded session.

This repository contains the .NET implementation of the framework. Implementations in other ecosystems (Python, TypeScript) live in separate repositories so each can follow the conventions and release cadence of its own ecosystem.

## What it is

- A small set of stable contracts (`CrossAgents.Abstractions`) that describe models, tools, memory, patterns, policy, and audit.
- A minimal runtime host (`CrossAgents.Core`) that registers adapters and patterns, selects one for each task, runs it, and surfaces a structured result.
- A first-party set of safe patterns (`CrossAgents.Patterns`): a no-tool single call, a Plan-Execute-Validate flow, a JSON plan skeleton, and a strictly bounded ReAct loop.
- Optional layers for tooling (`CrossAgents.Tooling`) and memory (`CrossAgents.Memory`) that can be plugged in independently.
- Deterministic test doubles (`CrossAgents.Testing`) for pattern and runtime tests with no external dependencies.

## What it isn't

- Not a chatbot framework. Conversations are a use case applications can build on top, not the framework's purpose.
- Not coupled to a single LLM provider. The framework ships no provider adapters in this milestone; applications wire their own through `IModelAdapter`.
- Not a tool calling library. Tooling is an optional module; tasks that don't need tools never see the tooling layer.
- Not a memory store. Memory retrieval is an optional module; the framework does not own a database or vector index.
- Not unbounded. Patterns must declare bounds. Unbounded ReAct configurations are rejected at registration time.

## Core concepts

- **Model adapter**: a thin `IModelAdapter` implementation that talks to one model and reports `ModelCapabilities`.
- **Agent task**: a single unit of work (`AgentTask`) with a type, input, and optional requirements.
- **Pattern**: an `IAgentPattern` plus a `PatternDescriptor` that declares its risk profile, bounds, and dependencies.
- **Pattern selector**: a deterministic chooser that filters patterns by task requirements, model capabilities, and policy, then scores survivors.
- **Policy engine**: an `IPolicyEngine` that translates declarative policies (`AgentPolicy`) into yes/no decisions for selection and tool calls.
- **Audit pipeline**: a per-session buffer that fans events out to `IAuditSink` and surfaces them in the runtime result.

## Minimal example

```csharp
using CrossAgents.Abstractions.Agents;
using CrossAgents.Abstractions.Models;
using CrossAgents.Core;
using CrossAgents.Patterns;
using CrossAgents.Testing;

var runtime = new AgentRuntime(new RuntimeOptions
{
    AuditSink = new InMemoryAuditSink()
});

var profile = new ModelProfile
{
    ProfileId = "demo-echo",
    DisplayName = "Demo echo model",
    Provider = ModelProvider.Custom,
    Capabilities = new ModelCapabilities { ProviderName = "demo", ModelId = "echo", IsLocal = true }
};

runtime
    .RegisterModel(new FakeModelAdapter(profile, "Hello from Cross Agents Runtime."))
    .RegisterPattern(new NoToolPattern())
    .RegisterPattern(new PlanExecuteValidatePattern());

var result = await runtime.RunAsync(
    new AgentTask { TaskId = "demo-1", Input = "Say hello.", RequiresValidation = false },
    profile.ProfileId);

Console.WriteLine($"{result.SelectedPatternId}: {result.Agent?.Output}");
```

A self-contained runnable version of this lives in `examples/CrossAgents.Examples.MinimalRuntime`.

## Package layout

| Package | Purpose | Depends on |
| --- | --- | --- |
| `CrossAgents.Abstractions` | Stable contracts (models, tools, memory, patterns, policy, audit) | – |
| `CrossAgents.Core` | Runtime host, session, selector, default policy engine, audit pipeline | Abstractions |
| `CrossAgents.Patterns` | First-party safe patterns | Abstractions, Core |
| `CrossAgents.Tooling` | Optional tool registry, validator, executor, normalizer | Abstractions |
| `CrossAgents.Memory` | Optional retrieval, ranking, compression, sliding buffer | Abstractions |
| `CrossAgents.Testing` | Deterministic test doubles | Abstractions, Core |

## Design principles

1. **Provider-agnostic**: model behaviour reaches the runtime only through `IModelAdapter` and `ModelCapabilities`.
2. **Model-adaptive**: pattern selection inspects the model's capability profile and degrades safely when something is missing.
3. **Bounded by default**: every shipped pattern declares step counts and risk levels; the runtime rejects unbounded configurations.
4. **Optional middleware**: tooling and memory are separate packages and separate runtime services; they can be omitted entirely.
5. **Auditable**: every session emits a canonical sequence of audit events suitable for compliance and debugging.
6. **Deterministic to test**: `CrossAgents.Testing` ships in-process fakes for every external dependency the framework defines.
7. **Small public surface**: contracts are short, immutable, and documented; framework code never exposes provider-specific types.

## Current status

This is the first milestone. It establishes the contracts, the runtime, three patterns plus a strictly bounded ReAct loop, optional tooling and memory layers, and the test surface. Provider adapters, multi-agent orchestration primitives, distributed session storage, and other extensions are out of scope for this milestone.

## What is intentionally not in this milestone

- No real provider adapters (OpenAI, Anthropic, Bedrock, Ollama, etc.).
- No vector store, embedding, or document extraction implementations.
- No multi-agent orchestration patterns (debate, swarm, voting). The contracts allow them; an implementation will arrive in a later milestone.
- No streaming response surface beyond the boolean capability flag.
- No long-term session persistence.
- No telemetry exporter (open-telemetry, Prometheus, etc.). The audit sink is the integration point.

## Building and testing

```sh
dotnet build CrossAgents.sln -c Release
dotnet test CrossAgents.sln -c Release
```

Tests run entirely in-process and require no credentials or network access.

## Further reading

- [docs/architecture.md](docs/architecture.md) - runtime composition.
- [docs/pattern-selection.md](docs/pattern-selection.md) - selector contract.
- [docs/custom-patterns.md](docs/custom-patterns.md) - implementing `IAgentPattern`.
- [docs/tooling.md](docs/tooling.md) and [docs/memory.md](docs/memory.md) - optional middleware.
- [docs/model-capabilities.md](docs/model-capabilities.md) - capability fields and selector inputs.
- [docs/testing.md](docs/testing.md) - deterministic test doubles.
- [docs/design-references.md](docs/design-references.md) - external agent frameworks studied for pattern shape (no code is copied; provider, channel, and domain features are intentionally out of scope).

## License

MIT. See [LICENSE](LICENSE).
