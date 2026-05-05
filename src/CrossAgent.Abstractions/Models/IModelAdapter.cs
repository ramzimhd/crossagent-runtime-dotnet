using System.Threading;
using System.Threading.Tasks;

namespace CrossAgent.Abstractions.Models;

/// <summary>
/// A provider-neutral entry point to a single model. Implementations are expected to be
/// thread-safe and stateless aside from any caching they choose to perform internally.
/// </summary>
public interface IModelAdapter
{
    /// <summary>The profile this adapter exposes. Used by the runtime for discovery and selection.</summary>
    ModelProfile Profile { get; }

    /// <summary>Send a single request and return a single response.</summary>
    Task<ModelResponse> CompleteAsync(ModelRequest request, CancellationToken cancellationToken = default);
}
