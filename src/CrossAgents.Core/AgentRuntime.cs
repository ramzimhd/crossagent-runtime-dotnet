using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrossAgents.Abstractions.Agents;
using CrossAgents.Abstractions.Audit;
using CrossAgents.Abstractions.Memory;
using CrossAgents.Abstractions.Models;
using CrossAgents.Abstractions.Patterns;
using CrossAgents.Abstractions.Policy;
using CrossAgents.Abstractions.Tools;

namespace CrossAgents.Core;

/// <summary>
/// The host that owns the registered models, patterns, services, and policy.
/// Applications instantiate one runtime, register what they need, and call
/// <see cref="RunAsync"/> per task.
/// </summary>
public sealed class AgentRuntime
{
    private readonly Dictionary<string, IModelAdapter> _models = new(StringComparer.Ordinal);
    private readonly List<IAgentPattern> _patterns = new();
    private readonly RuntimeOptions _options;
    private readonly RuntimePolicyEngine _policy;
    private readonly PatternSelector _selector;

    public AgentRuntime(RuntimeOptions? options = null)
    {
        _options = options ?? new RuntimeOptions();
        _options.AuditSink ??= NullAuditSink.Instance;
        _policy = new RuntimePolicyEngine(_options.DefaultPolicy);
        _selector = new PatternSelector(_policy);
    }

    /// <summary>The active policy engine. Exposed for inspection and testing.</summary>
    public IPolicyEngine Policy => _policy;

    /// <summary>The configured tool layer or null when none was supplied.</summary>
    public IToolInvoker? Tools => _options.Tools;

    /// <summary>The configured memory provider or null when none was supplied.</summary>
    public IMemoryProvider? Memory => _options.Memory;

    /// <summary>Registered models keyed by profile id.</summary>
    public IReadOnlyDictionary<string, IModelAdapter> Models => _models;

    /// <summary>Registered patterns in registration order.</summary>
    public IReadOnlyList<IAgentPattern> Patterns => _patterns;

    public AgentRuntime RegisterModel(IModelAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(adapter);
        if (string.IsNullOrWhiteSpace(adapter.Profile.ProfileId))
        {
            throw new ArgumentException("ModelAdapter.Profile.ProfileId must be non-empty.", nameof(adapter));
        }

        if (!_models.TryAdd(adapter.Profile.ProfileId, adapter))
        {
            throw new InvalidOperationException($"A model with id '{adapter.Profile.ProfileId}' is already registered.");
        }

        return this;
    }

    public AgentRuntime RegisterPattern(IAgentPattern pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ValidatePatternDescriptor(pattern.Descriptor);
        _patterns.Add(pattern);
        return this;
    }

    /// <summary>Run a task using a registered model. The selector chooses the pattern.</summary>
    public Task<RuntimeResult> RunAsync(AgentTask task, string modelProfileId, CancellationToken cancellationToken = default)
        => RunAsync(task, modelProfileId, taskPolicy: null, cancellationToken);

    /// <summary>Run a task using a registered model and an overriding policy for this session.</summary>
    public async Task<RuntimeResult> RunAsync(AgentTask task, string modelProfileId, AgentPolicy? taskPolicy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(modelProfileId);

        var sessionId = _options.SessionIdFactory();
        var pipeline = new AuditPipeline(_options.AuditSink!, _options.TimeProvider, sessionId);

        await pipeline.EmitAsync(AuditEventKind.SessionStarted, "Session started.", Property("sessionId", sessionId), cancellationToken).ConfigureAwait(false);
        await pipeline.EmitAsync(AuditEventKind.TaskReceived, $"Task '{task.TaskId}' of type '{task.Type}'.", Property("taskId", task.TaskId), cancellationToken).ConfigureAwait(false);

        if (!_models.TryGetValue(modelProfileId, out var model))
        {
            return await FailAsync(pipeline, sessionId, RuntimeErrorCode.UnknownModel, $"No model is registered with profile id '{modelProfileId}'.").ConfigureAwait(false);
        }

        await pipeline.EmitAsync(AuditEventKind.ModelSelected, $"Model '{model.Profile.ProfileId}' selected.", Property("modelId", model.Profile.ProfileId), cancellationToken).ConfigureAwait(false);

        var effectivePolicy = taskPolicy ?? _options.DefaultPolicy;
        var policyEngine = ReferenceEquals(effectivePolicy, _options.DefaultPolicy)
            ? (IPolicyEngine)_policy
            : new RuntimePolicyEngine(effectivePolicy);
        var selector = ReferenceEquals(policyEngine, _policy) ? _selector : new PatternSelector(policyEngine);

        var selection = selector.Select(task, model.Profile, _patterns, _options.PreferredPatternId);
        var selectedPattern = selection.Pattern;
        if (!selection.HasSelection || selectedPattern is null)
        {
            await pipeline.EmitAsync(AuditEventKind.PolicyRejected, $"No pattern selected. {selection.Reason}", properties: null, cancellationToken).ConfigureAwait(false);
            return new RuntimeResult
            {
                SessionId = sessionId,
                Success = false,
                Error = new RuntimeError
                {
                    Code = RuntimeErrorCode.NoEligiblePattern,
                    Message = "No eligible pattern.",
                    Detail = selection.Reason
                },
                RuntimeAuditEvents = pipeline.Captured,
                SelectedModelId = model.Profile.ProfileId
            };
        }

        await pipeline.EmitAsync(AuditEventKind.PatternSelected, $"Pattern '{selectedPattern.Descriptor.PatternId}' selected.", Property("patternId", selectedPattern.Descriptor.PatternId), cancellationToken).ConfigureAwait(false);

        var session = new AgentSession(this, sessionId, task, model, selectedPattern, policyEngine, effectivePolicy, pipeline);
        var agentResult = await session.RunAsync(cancellationToken).ConfigureAwait(false);

        var success = agentResult.State == AgentState.Completed;
        await pipeline.EmitAsync(success ? AuditEventKind.SessionCompleted : AuditEventKind.SessionFailed,
            success ? "Session completed." : $"Session failed: {agentResult.ErrorMessage ?? "unknown error"}.",
            Property("sessionId", sessionId),
            cancellationToken).ConfigureAwait(false);

        return new RuntimeResult
        {
            SessionId = sessionId,
            Success = success,
            Agent = agentResult,
            Error = success ? null : new RuntimeError
            {
                Code = RuntimeErrorCode.ExecutionFailed,
                Message = agentResult.ErrorMessage ?? "Pattern reported a non-completed state.",
                Detail = agentResult.State.ToString()
            },
            RuntimeAuditEvents = pipeline.Captured,
            SelectedPatternId = selectedPattern.Descriptor.PatternId,
            SelectedModelId = model.Profile.ProfileId
        };
    }

    private static void ValidatePatternDescriptor(PatternDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (string.IsNullOrWhiteSpace(descriptor.PatternId))
        {
            throw new ArgumentException("PatternDescriptor.PatternId must be non-empty.");
        }
        if (descriptor.RiskLevel == PatternRiskLevel.Unbounded)
        {
            throw new ArgumentException(
                $"Unbounded patterns are not permitted. Pattern '{descriptor.PatternId}' was registered with RiskLevel.Unbounded.",
                nameof(descriptor));
        }
        if (descriptor.IsBounded && descriptor.MaxSteps <= 0)
        {
            throw new ArgumentException(
                $"Bounded pattern '{descriptor.PatternId}' must declare MaxSteps > 0.",
                nameof(descriptor));
        }
    }

    private static async Task<RuntimeResult> FailAsync(AuditPipeline pipeline, string sessionId, RuntimeErrorCode code, string message)
    {
        await pipeline.EmitAsync(AuditEventKind.SessionFailed, message).ConfigureAwait(false);
        return new RuntimeResult
        {
            SessionId = sessionId,
            Success = false,
            Error = new RuntimeError { Code = code, Message = message },
            RuntimeAuditEvents = pipeline.Captured
        };
    }

    private static IReadOnlyDictionary<string, string> Property(string key, string value)
        => new Dictionary<string, string>(StringComparer.Ordinal) { [key] = value };
}
