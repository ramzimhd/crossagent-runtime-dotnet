using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrossAgents.Abstractions.Memory;

namespace CrossAgents.Memory;

/// <summary>
/// Thin wrapper around an <see cref="IMemoryProvider"/> that enforces query
/// validation and surfaces a stable retrieve method patterns can call. Patterns
/// must use a retriever rather than calling the provider directly so cross-cutting
/// concerns (limits, normalization) live in one place.
/// </summary>
public sealed class MemoryRetriever
{
    private readonly IMemoryProvider _provider;

    public MemoryRetriever(IMemoryProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public async Task<IReadOnlyList<MemoryItem>> RetrieveAsync(MemoryQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.Query))
        {
            return Array.Empty<MemoryItem>();
        }
        if (query.Limit <= 0)
        {
            return Array.Empty<MemoryItem>();
        }

        return await _provider.SearchAsync(query, cancellationToken).ConfigureAwait(false) ?? Array.Empty<MemoryItem>();
    }
}
