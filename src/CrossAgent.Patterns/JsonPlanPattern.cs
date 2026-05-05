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
/// Skeleton pattern that asks the model for a JSON-encoded plan. Requires the
/// model to advertise JSON mode or JSON Schema support. The skeleton does not
/// itself parse or validate the plan; consumers may layer schema validation
/// on top via a custom pattern or a follow-up step.
/// </summary>
public sealed class JsonPlanPattern : IAgentPattern
{
    public PatternDescriptor Descriptor { get; } = new()
    {
        PatternId = KnownPatternIds.JsonPlan,
        Name = "JSON plan",
        RequiresTools = false,
        RequiresMemory = false,
        RequiresNativeToolCalling = false,
        RequiresJsonMode = true,
        SupportsMultiAgent = false,
        IsBounded = true,
        MaxSteps = 1,
        RiskLevel = PatternRiskLevel.Low
    };

    public async Task<AgentResult> ExecuteAsync(AgentContext context, IPatternServices services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(services);

        await services.Audit.WriteAsync(new AuditEvent
        {
            Timestamp = DateTimeOffset.UtcNow,
            SessionId = context.SessionId,
            Kind = AuditEventKind.StepStarted,
            Message = "json-plan: requesting structured plan"
        }, cancellationToken).ConfigureAwait(false);

        var request = new ModelRequest
        {
            Prompt = $"Produce a JSON object with a 'steps' array describing how to address the task. Do not include any prose.\n\nTask:\n{context.Task.Input}",
            JsonMode = true
        };

        var response = await services.Model.CompleteAsync(request, cancellationToken).ConfigureAwait(false);

        await services.Audit.WriteAsync(new AuditEvent
        {
            Timestamp = DateTimeOffset.UtcNow,
            SessionId = context.SessionId,
            Kind = AuditEventKind.StepCompleted,
            Message = "json-plan: response received"
        }, cancellationToken).ConfigureAwait(false);

        return new AgentResult
        {
            SessionId = context.SessionId,
            State = AgentState.Completed,
            Output = response.Content,
            ValidationPassed = true
        };
    }
}
