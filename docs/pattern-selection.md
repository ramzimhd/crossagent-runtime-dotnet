# Pattern selection

The selector turns a `(task, model, registered patterns, policy)` quadruple into a single chosen pattern, or a structured rejection if nothing fits.

## Inputs

- **AgentTask**: type, input, requirement flags (`RequiresTools`, `RequiresMemory`, `RequiresValidation`), optional allow / forbid lists, and an optional max-steps ceiling.
- **ModelProfile**: capability flags and provider information.
- **Registered patterns**: ordered list of `IAgentPattern` instances, each carrying a `PatternDescriptor`.
- **AgentPolicy**: applied via the `IPolicyEngine` for both pattern selection and tool calls.

## Filtering

A pattern is filtered out if:

- its descriptor cannot satisfy a hard requirement (tools, memory, multi-agent);
- the policy engine denies selection (allow/forbid lists, capability mismatches, unbounded risk, max-step ceilings).

Soft signals (for example, JSON mode preference) influence scoring rather than filtering.

## Scoring

Surviving patterns are scored. The default scorer rewards:

- bounded patterns (preferred over loose loops);
- lower risk levels;
- the Plan-Execute-Validate pattern when the task requires validation;
- patterns that declare JSON mode when the task type benefits from structured output;
- native tool calling alignment with the model's capability;
- absence of tooling needs when the task does not need tools.

Ties are broken first by lower risk level, then by ascending pattern id for stability.

A `PatternSelector` honors `RuntimeOptions.PreferredPatternId` when the preferred pattern survives filtering and policy.

## Rejections

The selector never throws. When no pattern survives, the runtime returns a `RuntimeResult` with `Success = false`, a `RuntimeError` with code `NoEligiblePattern`, and a message that lists which patterns were rejected and why. Audit events include a `PolicyRejected` entry summarising the reason.

## Custom patterns and the selector

Custom patterns are first-class participants. To make them eligible:

1. Provide a `PatternDescriptor` with truthful flags. Lying about tool or memory needs will cause the runtime to dispatch into a pattern that cannot succeed.
2. Set `IsBounded` to `true` and pick `RiskLevel` honestly. Unbounded risk levels are rejected at registration.
3. If the pattern requires JSON mode or native tool calling, mark it explicitly so the selector can match it to capable models.
