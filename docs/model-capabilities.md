# Model capabilities

Models reach the runtime through `IModelAdapter`. Each adapter exposes a `ModelProfile` whose `ModelCapabilities` declare what the model can do. The selector reads those flags; patterns rely on them to know what is safe to ask for.

## Capability flags

| Flag | Meaning |
| --- | --- |
| `SupportsNativeToolCalling` | The model exposes a first-class tool calling protocol. Required by patterns whose descriptor sets `RequiresNativeToolCalling = true`. |
| `SupportsJsonMode` | The model can be constrained to return JSON. Required by patterns whose descriptor sets `RequiresJsonMode = true`. |
| `SupportsJsonSchema` | The model accepts a JSON Schema and is constrained to satisfy it. Treated as a stronger form of JSON mode by the default scorer. |
| `SupportsVision` | Image inputs are allowed. Reserved for future patterns that consume images. |
| `SupportsStreaming` | The model can stream tokens incrementally. Used by streaming-aware adapters; the framework itself does not stream responses. |
| `MaxContextTokens` | Soft information used by memory compression and adapter-side guards. Zero means unspecified. |
| `IsLocal` | The model executes on the same machine as the runtime. Useful for policies that prefer local processing. |
| `ProviderName` / `ModelId` | Free-form provenance fields. Useful for audit and observability. |

## Authoring an adapter

1. Implement `IModelAdapter` with a `ModelProfile` populated from your provider's metadata.
2. Translate `ModelRequest` into a provider-specific call. Map `Tools` only when the model supports tool calls; otherwise drop them.
3. Translate the provider response into `ModelResponse`. Populate `ToolCalls` only when the provider produced explicit tool calls; never infer tool calls from prose.
4. Map provider stop reasons to `ModelFinishReason`.
5. Surface usage data via `ModelResponse.Metadata` so audit sinks can record token usage without coupling to a provider.

## Capability-aware degradation

The default policy engine refuses to route a task to a pattern that requires a capability the chosen model does not advertise. Applications that need to override that behaviour can implement a custom `IPolicyEngine` and pass a different scorer through a custom selector. In normal usage, register multiple models with different capabilities and let the selector pick a capable model and pattern together.
