using System.Linq;
using System.Threading.Tasks;
using CrossAgent.Abstractions.Agents;
using CrossAgent.Abstractions.Audit;
using CrossAgent.Core;
using CrossAgent.Patterns;
using CrossAgent.Testing;
using Xunit;

namespace CrossAgent.Tests;

public class AuditEventTests
{
    [Fact]
    public async Task Runtime_EmitsCanonicalAuditEvents()
    {
        var sink = new InMemoryAuditSink();
        var runtime = new AgentRuntime(new RuntimeOptions
        {
            AuditSink = sink
        });
        runtime.RegisterModel(TestFixtures.EchoAdapter());
        runtime.RegisterPattern(new NoToolPattern());

        var task = new AgentTask { TaskId = "audit", Input = "hi", RequiresValidation = false };
        var result = await runtime.RunAsync(task, "echo");

        Assert.True(result.Success);

        var kinds = sink.Events.Select(e => e.Kind).ToList();
        Assert.Contains(AuditEventKind.SessionStarted, kinds);
        Assert.Contains(AuditEventKind.TaskReceived, kinds);
        Assert.Contains(AuditEventKind.ModelSelected, kinds);
        Assert.Contains(AuditEventKind.PatternSelected, kinds);
        Assert.Contains(AuditEventKind.SessionCompleted, kinds);

        // Audit events also surface in the result payload.
        Assert.NotEmpty(result.RuntimeAuditEvents);
        Assert.All(result.RuntimeAuditEvents, e => Assert.Equal(result.SessionId, e.SessionId));
    }
}
