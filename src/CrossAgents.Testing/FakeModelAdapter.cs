using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrossAgents.Abstractions.Models;
using CrossAgents.Abstractions.Tools;

namespace CrossAgents.Testing;

/// <summary>
/// Deterministic, in-process model adapter for tests and examples. Tests
/// supply a response factory that turns each request into a canned response;
/// the adapter records every call so assertions can inspect what happened.
/// </summary>
public sealed class FakeModelAdapter : IModelAdapter
{
    private readonly Func<ModelRequest, int, ModelResponse> _responder;
    private readonly List<ModelRequest> _calls = new();

    public FakeModelAdapter(ModelProfile profile, Func<ModelRequest, int, ModelResponse> responder)
    {
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _responder = responder ?? throw new ArgumentNullException(nameof(responder));
    }

    /// <summary>Convenience constructor that returns the same content for every call.</summary>
    public FakeModelAdapter(ModelProfile profile, string content, IReadOnlyList<ToolCall>? toolCalls = null)
        : this(profile, (_, _) => new ModelResponse
        {
            Content = content,
            ToolCalls = toolCalls ?? Array.Empty<ToolCall>(),
            FinishReason = ModelFinishReason.Stop
        })
    {
    }

    public ModelProfile Profile { get; }

    public IReadOnlyList<ModelRequest> Calls => _calls;

    public Task<ModelResponse> CompleteAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        _calls.Add(request);
        var response = _responder(request, _calls.Count);
        return Task.FromResult(response);
    }
}
