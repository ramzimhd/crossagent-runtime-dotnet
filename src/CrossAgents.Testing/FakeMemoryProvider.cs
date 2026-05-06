using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrossAgents.Abstractions.Memory;

namespace CrossAgents.Testing;

/// <summary>
/// Deterministic in-memory <see cref="IMemoryProvider"/>. Items are returned in
/// the order they were added, filtered by query token overlap.
/// </summary>
public sealed class FakeMemoryProvider : IMemoryProvider
{
    private static readonly char[] TokenSeparators = { ' ', '\t', '\r', '\n', '.', ',', ';', ':', '!', '?' };

    private readonly List<MemoryItem> _items = new();

    public FakeMemoryProvider Add(MemoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
        return this;
    }

    public IReadOnlyList<MemoryItem> Items => _items;

    public Task<IReadOnlyList<MemoryItem>> SearchAsync(MemoryQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();

        var queryTokens = query.Query
            .Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(static t => t.ToLowerInvariant())
            .ToHashSet();

        IReadOnlyList<MemoryItem> matches = _items
            .Where(item => queryTokens.Count == 0 || queryTokens.Any(t => item.Content.Contains(t, StringComparison.OrdinalIgnoreCase)))
            .Take(query.Limit)
            .ToArray();

        return Task.FromResult(matches);
    }
}
