# Architecture

CrossAgent Runtime is structured around a small core that depends only on stable contracts. Every other concern - tooling, memory, patterns, test doubles - lives in its own assembly and is an optional dependency for an application.

## Layers

```
+-----------------------------------------------------------+
|  Application                                              |
|    - constructs AgentRuntime                              |
|    - registers IModelAdapter implementations              |
|    - registers IAgentPattern implementations              |
|    - calls runtime.RunAsync(task, modelId)                |
+-----------------------------------------------------------+
|  CrossAgent.Patterns      CrossAgent.Tooling   .Memory    |
|    - first-party patterns   - tool registry    - retrieval|
|    - bounded by design      - validator        - ranker   |
|                              - executor         - compress|
+-----------------------------------------------------------+
|  CrossAgent.Core                                          |
|    - AgentRuntime         - PatternSelector               |
|    - AgentSession         - RuntimePolicyEngine           |
|    - AuditPipeline        - PatternServices               |
+-----------------------------------------------------------+
|  CrossAgent.Abstractions  (stable contracts only)         |
|    - IModelAdapter, ModelProfile, ModelCapabilities       |
|    - IAgentPattern, PatternDescriptor, AgentTask, ...     |
|    - ITool, IToolInvoker, ToolDefinition, ToolPolicy      |
|    - IMemoryProvider, MemoryItem, ActiveContext           |
|    - IPolicyEngine, AgentPolicy, PolicyDecision           |
|    - IAuditSink, AuditEvent                               |
+-----------------------------------------------------------+
```

The dependency direction always flows downward: Patterns depend on Core and Abstractions, Tooling and Memory depend only on Abstractions. Core depends only on Abstractions.

## Session lifecycle

1. The application calls `runtime.RunAsync(task, modelId)`.
2. The runtime creates a session id, opens an `AuditPipeline`, and emits `SessionStarted` and `TaskReceived`.
3. The runtime resolves the model by id (emits `ModelSelected`).
4. The pattern selector filters registered patterns by requirements, asks the policy engine, scores survivors, and returns one (`PatternSelected`).
5. An `AgentSession` constructs `PatternServices` honoring the effective policy (memory and tools become null if the policy disables them) and dispatches to the pattern.
6. The pattern emits step events through the audit sink and returns an `AgentResult`.
7. The runtime emits `SessionCompleted` (or `SessionFailed`), wraps the agent result in a `RuntimeResult`, and returns.

## Determinism guarantees

The selector orders patterns by score, then risk level, then pattern id. Given identical registration order and identical inputs, the selector returns the same pattern on every run. Audit timestamps come from a `TimeProvider` so tests can substitute a deterministic clock.

## Extension points

- **Custom model adapters** implement `IModelAdapter` and `ModelProfile`/`ModelCapabilities`. Adapters do not need to be registered as services - they are passed directly to `runtime.RegisterModel`.
- **Custom patterns** implement `IAgentPattern` and a `PatternDescriptor`. The descriptor is the contract the selector reasons about; the pattern itself only runs once selected.
- **Custom policy engines** implement `IPolicyEngine` to enforce non-default rules. Applications that only need the built-in rules use `RuntimePolicyEngine`.
- **Custom audit sinks** implement `IAuditSink`. Sinks should be safe under concurrent writers; the framework uses one pipeline per session, so contention is per-application.
- **Custom tool invokers** implement `IToolInvoker`. The default is `ToolRegistry` from `CrossAgent.Tooling`.

## Concurrency model

Sessions are independent. The runtime is thread-safe for `RunAsync` calls; pattern instances must be safe to invoke concurrently from different sessions. Per-session state lives only in the `AgentContext` and the services constructed in `AgentSession`.
