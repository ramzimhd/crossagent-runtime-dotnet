# Design references

CrossAgent Runtime is the foundation library for an agentic architecture
that gives developers a clean, provider-neutral runtime to build on. The
.NET version is the first port; future ports (Python, TypeScript, others)
will follow. To keep the **shape** of the runtime consistent across ports
without re-deriving it from scratch each time, this document collects
external open-source agent frameworks whose **design patterns** are
relevant to study.

These are reference projects for pattern shape. Not for code. Not for
copy-paste. Not for adopting their provider, channel, or domain features.

## Scope guardrail

Studying these references must not violate any milestone constraint:

- No DocFlow, OCR, invoice, vector, database, or document-processing
  logic enters CrossAgent.
- No provider integration (OpenAI, Anthropic, OpenRouter, Mistral,
  Ollama, Bedrock, Gemini, etc.) enters CrossAgent. Provider adapters
  are a separate concern, written by consumers of the runtime.
- No messaging or channel adapter (Telegram, Discord, Slack, WhatsApp,
  WebRTC, etc.) enters CrossAgent. Those belong above the runtime.
- No secret, credential, or private config enters the repository.
- No AI watermark, generation marker, or assistant trailer is added to
  source, comments, docs, commit messages, or workflow files.

## References

### NousResearch / hermes-agent

- Repository: https://github.com/NousResearch/hermes-agent
- Knowledge endpoint (queryable): https://gitmcp.io/NousResearch/hermes-agent
- Stack: Python (core agent), TypeScript (UI), MCP integration, cron
  scheduler, persistent memory, multi-LLM routing.

Areas of pattern interest for CrossAgent:

| Pattern area | What to extract | What to leave behind |
| --- | --- | --- |
| Agent loop shape | Step lifecycle, halt conditions, bounded recursion | Specific provider clients, OpenRouter routing |
| Tool/skill catalogue | Skill registration, validation, retirement | Tool implementations themselves |
| Memory layering | Short-term context vs durable store separation | Honcho user modeling, FTS5 specifics |
| MCP consumption (future) | How an agent runtime calls an MCP server cleanly | Exposing the runtime as an MCP server is a separate, later milestone |
| Scheduling / cron | The shape of long-running deferred work | The specific cron implementation |

### openclaw / openclaw

- Repository: https://github.com/openclaw/openclaw
- Knowledge endpoint (queryable): https://gitmcp.io/openclaw/openclaw
- Stack: TypeScript / Node.js, multi-channel gateway, sandboxed tool
  execution, voice and Canvas UI surfaces.

Areas of pattern interest for CrossAgent:

| Pattern area | What to extract | What to leave behind |
| --- | --- | --- |
| Sandboxed tool execution | The boundary between tool-call decision and tool-call effect | Docker-specific mechanics |
| Transport / runtime separation | Channel adapters live above the runtime, not inside it | All concrete channel adapters |
| Capability negotiation | How the runtime advertises what a model is allowed to do | UI/Canvas concerns |

## How to use these references

When designing a runtime feature in CrossAgent (any language port):

1. State the feature in CrossAgent's own vocabulary first - what
   abstraction it adds, what contract it changes, what audit events it
   emits. Do not borrow vocabulary from the references prematurely.
2. Skim the corresponding area in the reference projects (via the
   knowledge endpoint or the GitHub repo) for shape, not implementation.
3. Capture only the *shape*: interfaces, lifecycle, error handling,
   invariants. Not file layout, not import lists, not dependencies.
4. Adapt the shape to CrossAgent's idioms. The .NET port follows .NET
   conventions; future ports will follow theirs. The shared layer is
   the conceptual contract, not the code.
5. If the pattern is worth standardising across ports, document it in
   `docs/architecture.md` so all ports converge on it.

## Adding to this list

Add a project here only if all of the following hold:

- It is open-source and can be referenced without licence friction.
- It contains agent-runtime patterns at the correct level of abstraction
  (loop, tools, memory, audit, capability negotiation, scheduling).
- It does not pull CrossAgent toward provider lock-in, domain coupling,
  or feature creep.

Keep the list short. The point of a reference is signal, not surface
area.
