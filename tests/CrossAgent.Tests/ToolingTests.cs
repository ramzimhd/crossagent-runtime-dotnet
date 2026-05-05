using System.Threading.Tasks;
using CrossAgent.Abstractions.Tools;
using CrossAgent.Testing;
using CrossAgent.Tooling;
using Xunit;

namespace CrossAgent.Tests;

public class ToolingTests
{
    [Fact]
    public async Task Registry_RejectsUnknownTool()
    {
        var registry = new ToolRegistry();

        var result = await registry.InvokeAsync(new ToolCall
        {
            CallId = "1",
            ToolName = "does-not-exist",
            ArgumentsJson = "{}"
        });

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Contains("Unknown tool", result.Error!);
    }

    [Fact]
    public async Task Registry_RejectsInvalidArguments_WhenSchemaRequiresProperties()
    {
        const string schema = """
        { "type": "object", "properties": { "n": { "type": "number" } }, "required": ["n"] }
        """;
        var registry = new ToolRegistry();
        registry.Register(new FakeTool("add", "Adds 1 to n.", schema));

        var missing = await registry.InvokeAsync(new ToolCall
        {
            CallId = "1",
            ToolName = "add",
            ArgumentsJson = "{}"
        });
        Assert.False(missing.Success);
        Assert.Contains("Required properties", missing.Error);

        var wrongType = await registry.InvokeAsync(new ToolCall
        {
            CallId = "2",
            ToolName = "add",
            ArgumentsJson = "{ \"n\": \"not-a-number\" }"
        });
        Assert.False(wrongType.Success);
        Assert.Contains("incompatible types", wrongType.Error);

        var ok = await registry.InvokeAsync(new ToolCall
        {
            CallId = "3",
            ToolName = "add",
            ArgumentsJson = "{ \"n\": 5 }"
        });
        Assert.True(ok.Success);
    }

    [Fact]
    public async Task Registry_RejectsMalformedJson()
    {
        var registry = new ToolRegistry();
        registry.Register(new FakeTool("noop", "noop", "{ \"type\": \"object\" }"));

        var result = await registry.InvokeAsync(new ToolCall
        {
            CallId = "1",
            ToolName = "noop",
            ArgumentsJson = "{ this is not json"
        });

        Assert.False(result.Success);
    }
}
