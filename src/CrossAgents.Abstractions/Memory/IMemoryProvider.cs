using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrossAgents.Abstractions.Memory;

/// <summary>
/// Read-side abstraction over a memory store. The framework does not own the
/// memory store - applications plug in their own (vector DB, keyword index,
/// in-memory list, etc.).
/// </summary>
public interface IMemoryProvider
{
    Task<IReadOnlyList<MemoryItem>> SearchAsync(
        MemoryQuery query,
        CancellationToken cancellationToken = default);
}
