using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrossAgents.Abstractions.Agents;
using CrossAgents.Abstractions.Audit;
using CrossAgents.Abstractions.Models;
using CrossAgents.Abstractions.Patterns;
using CrossAgents.Abstractions.Tools;
using CrossAgents.Core;

namespace CrossAgents.Patterns;

/// <summary>
/// A bounded reason-and-act loop. Every loop iteration is a single model call
/// optionally followed by a tool call; the loop terminates when the model
/// stops requesting tools or when the configured step bound is hit. The pattern
/// requires a registered tool invoker and rejects sessions that don't have one.
/// Unbounded ReAct configurations are rejected by <see cref="BoundedReActOptions.Validate"/>.
/// </summary>
public sealed class BoundedReActPattern : IAgentPattern
{
    private readonly BoundedReActOptions _options;
    private readonly HashSet<string> _allowedTools;

    public BoundedReActPattern(BoundedReActOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        _options = options;
        _allowedTools = new HashSet<string>(options.AllowedTools, StringComparer.Ordinal);
        Descriptor = new PatternDescriptor
        {
            PatternId = KnownPatternIds.BoundedReAct,
            Name = "Bounded ReAct",
            RequiresTools = true,
            RequiresMemory = false,
            RequiresNativeToolCalling = true,
            RequiresJsonMode = false,
            SupportsMultiAgent = false,
            IsBounded = true,
            MaxSteps = options.MaxSteps,
            RiskLevel = PatternRiskLevel.Medium
        };
    }

    public PatternDescriptor Descriptor { get; }

    public async Task<AgentResult> ExecuteAsync(AgentContext context, IPatternServices services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(services);

        if (services.Tools is null)
        {
            return new AgentResult
            {
                SessionId = context.SessionId,
                State = AgentState.Rejected,
                Output = string.Empty,
                ValidationPassed = false,
                ErrorMessage = "BoundedReActPattern requires a tool invoker."
            };
        }

        var toolDefinitions = services.Tools.GetDefinitions()
            .Where(d => _allowedTools.Contains(d.Name))
            .ToArray();

        if (toolDefinitions.Length == 0)
        {
            return new AgentResult
            {
                SessionId = context.SessionId,
                State = AgentState.Rejected,
                Output = string.Empty,
                ValidationPassed = false,
                ErrorMessage = "None of the allowed tools are registered."
            };
        }

        var transcript = context.Task.Input;
        ModelResponse? lastResponse = null;

        for (var step = 1; step <= _options.MaxSteps; step++)
        {
            await services.Audit.WriteAsync(new AuditEvent
            {
                Timestamp = DateTimeOffset.UtcNow,
                SessionId = context.SessionId,
                Kind = AuditEventKind.StepStarted,
                Message = $"react: step {step} of {_options.MaxSteps}"
            }, cancellationToken).ConfigureAwait(false);

            using var stepCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            stepCts.CancelAfter(_options.StepTimeout);

            var request = new ModelRequest
            {
                Prompt = transcript,
                Tools = toolDefinitions
            };

            ModelResponse response;
            try
            {
                response = await services.Model.CompleteAsync(request, stepCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return new AgentResult
                {
                    SessionId = context.SessionId,
                    State = AgentState.Failed,
                    Output = lastResponse?.Content ?? string.Empty,
                    ValidationPassed = false,
                    ErrorMessage = $"Step {step} exceeded the configured timeout of {_options.StepTimeout}."
                };
            }

            lastResponse = response;

            if (response.ToolCalls.Count == 0)
            {
                await services.Audit.WriteAsync(new AuditEvent
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    SessionId = context.SessionId,
                    Kind = AuditEventKind.StepCompleted,
                    Message = $"react: step {step} produced final answer"
                }, cancellationToken).ConfigureAwait(false);

                return new AgentResult
                {
                    SessionId = context.SessionId,
                    State = AgentState.Completed,
                    Output = response.Content,
                    ValidationPassed = true
                };
            }

            foreach (var toolCall in response.ToolCalls)
            {
                if (!_allowedTools.Contains(toolCall.ToolName))
                {
                    await services.Audit.WriteAsync(new AuditEvent
                    {
                        Timestamp = DateTimeOffset.UtcNow,
                        SessionId = context.SessionId,
                        Kind = AuditEventKind.ToolCallRejected,
                        Message = $"react: tool '{toolCall.ToolName}' is not allowed in this session"
                    }, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                var policy = services.Policy.EvaluateToolCall(context.Task, toolCall.ToolName);
                if (!policy.Allowed)
                {
                    await services.Audit.WriteAsync(new AuditEvent
                    {
                        Timestamp = DateTimeOffset.UtcNow,
                        SessionId = context.SessionId,
                        Kind = AuditEventKind.ToolCallRejected,
                        Message = policy.Reason ?? "Tool call rejected by policy."
                    }, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                await services.Audit.WriteAsync(new AuditEvent
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    SessionId = context.SessionId,
                    Kind = AuditEventKind.ToolCallApproved,
                    Message = $"react: invoking tool '{toolCall.ToolName}'"
                }, cancellationToken).ConfigureAwait(false);

                var toolResult = await services.Tools.InvokeAsync(toolCall, cancellationToken).ConfigureAwait(false);

                await services.Audit.WriteAsync(new AuditEvent
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    SessionId = context.SessionId,
                    Kind = AuditEventKind.ToolResultReceived,
                    Message = $"react: tool '{toolCall.ToolName}' returned (success={toolResult.Success})"
                }, cancellationToken).ConfigureAwait(false);

                transcript = string.Concat(
                    transcript,
                    Environment.NewLine,
                    "[tool ", toolCall.ToolName, "] ",
                    toolResult.Success ? toolResult.Output : ("error: " + toolResult.Error));
            }

            await services.Audit.WriteAsync(new AuditEvent
            {
                Timestamp = DateTimeOffset.UtcNow,
                SessionId = context.SessionId,
                Kind = AuditEventKind.StepCompleted,
                Message = $"react: step {step} completed"
            }, cancellationToken).ConfigureAwait(false);
        }

        return new AgentResult
        {
            SessionId = context.SessionId,
            State = AgentState.Failed,
            Output = lastResponse?.Content ?? string.Empty,
            ValidationPassed = false,
            ErrorMessage = $"Bounded ReAct exhausted {_options.MaxSteps} step(s) without a final answer."
        };
    }
}
