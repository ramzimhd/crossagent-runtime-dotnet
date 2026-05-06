using System;
using System.Threading;
using System.Threading.Tasks;
using CrossAgents.Abstractions.Agents;
using CrossAgents.Abstractions.Audit;
using CrossAgents.Abstractions.Models;
using CrossAgents.Abstractions.Patterns;
using CrossAgents.Core;

namespace CrossAgents.Patterns;

/// <summary>
/// Single model call with no tool use, no memory, and no validation phase. The
/// safest possible pattern; suitable for direct question answering or
/// transformations that don't require external context.
/// </summary>
public sealed class NoToolPattern : IAgentPattern
{
    public PatternDescriptor Descriptor { get; } = new()
    {
        PatternId = KnownPatternIds.NoTool,
        Name = "No-tool single call",
        RequiresTools = false,
        RequiresMemory = false,
        RequiresNativeToolCalling = false,
        RequiresJsonMode = false,
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
            Message = "no-tool: single call"
        }, cancellationToken).ConfigureAwait(false);

        var request = new ModelRequest
        {
            Prompt = context.Task.Input
        };

        var response = await services.Model.CompleteAsync(request, cancellationToken).ConfigureAwait(false);

        await services.Audit.WriteAsync(new AuditEvent
        {
            Timestamp = DateTimeOffset.UtcNow,
            SessionId = context.SessionId,
            Kind = AuditEventKind.StepCompleted,
            Message = "no-tool: single call completed"
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
