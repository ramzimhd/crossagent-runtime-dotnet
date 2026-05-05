using System;
using System.Threading;
using System.Threading.Tasks;
using CrossAgent.Abstractions.Agents;
using CrossAgent.Abstractions.Audit;
using CrossAgent.Abstractions.Models;
using CrossAgent.Abstractions.Patterns;
using CrossAgent.Core;

namespace CrossAgent.Patterns;

/// <summary>
/// Three-phase pattern: a planning call produces a plan, an execution call
/// produces output following that plan, and a validation call inspects the
/// output. Phases are explicit and audited so applications can reason about
/// what happened in each session.
/// </summary>
public sealed class PlanExecuteValidatePattern : IAgentPattern
{
    public PatternDescriptor Descriptor { get; } = new()
    {
        PatternId = KnownPatternIds.PlanExecuteValidate,
        Name = "Plan-Execute-Validate",
        RequiresTools = false,
        RequiresMemory = false,
        RequiresNativeToolCalling = false,
        RequiresJsonMode = false,
        SupportsMultiAgent = false,
        IsBounded = true,
        MaxSteps = 3,
        RiskLevel = PatternRiskLevel.Low
    };

    public async Task<AgentResult> ExecuteAsync(AgentContext context, IPatternServices services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(services);

        var graph = new ExecutionGraph()
            .AddStep("plan")
            .AddStep("execute")
            .AddStep("validate");

        var planResponse = await RunStepAsync(services, context, graph.Steps[0],
            $"Produce a short, ordered plan for the following task. Do not solve it yet.\n\nTask:\n{context.Task.Input}",
            cancellationToken).ConfigureAwait(false);

        var executeResponse = await RunStepAsync(services, context, graph.Steps[1],
            $"Plan:\n{planResponse.Content}\n\nUsing the plan above, produce the final answer.\n\nTask:\n{context.Task.Input}",
            cancellationToken).ConfigureAwait(false);

        var validateResponse = await RunStepAsync(services, context, graph.Steps[2],
            $"Review the answer below for completeness against the task. Reply with the single word 'PASS' or 'FAIL' followed by a one-line reason.\n\nTask:\n{context.Task.Input}\n\nAnswer:\n{executeResponse.Content}",
            cancellationToken).ConfigureAwait(false);

        var validationPassed = validateResponse.Content.TrimStart().StartsWith("PASS", StringComparison.OrdinalIgnoreCase);

        await services.Audit.WriteAsync(new AuditEvent
        {
            Timestamp = DateTimeOffset.UtcNow,
            SessionId = context.SessionId,
            Kind = validationPassed ? AuditEventKind.ValidationPassed : AuditEventKind.ValidationFailed,
            Message = validateResponse.Content
        }, cancellationToken).ConfigureAwait(false);

        return new AgentResult
        {
            SessionId = context.SessionId,
            State = AgentState.Completed,
            Output = executeResponse.Content,
            ValidationPassed = validationPassed
        };
    }

    private static async Task<ModelResponse> RunStepAsync(IPatternServices services, AgentContext context, string stepName, string prompt, CancellationToken cancellationToken)
    {
        await services.Audit.WriteAsync(new AuditEvent
        {
            Timestamp = DateTimeOffset.UtcNow,
            SessionId = context.SessionId,
            Kind = AuditEventKind.StepStarted,
            Message = $"plan-execute-validate: {stepName} started"
        }, cancellationToken).ConfigureAwait(false);

        var response = await services.Model.CompleteAsync(new ModelRequest { Prompt = prompt }, cancellationToken).ConfigureAwait(false);

        await services.Audit.WriteAsync(new AuditEvent
        {
            Timestamp = DateTimeOffset.UtcNow,
            SessionId = context.SessionId,
            Kind = AuditEventKind.StepCompleted,
            Message = $"plan-execute-validate: {stepName} completed"
        }, cancellationToken).ConfigureAwait(false);

        return response;
    }
}
