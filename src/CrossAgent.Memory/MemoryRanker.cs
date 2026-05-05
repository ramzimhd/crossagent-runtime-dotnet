using System;
using System.Collections.Generic;
using System.Linq;
using CrossAgent.Abstractions.Memory;

namespace CrossAgent.Memory;

/// <summary>
/// Deterministic ranker. When all items already carry a score the ranker honors
/// it; otherwise items are ranked by length-normalised lexical overlap with the
/// query. Ties resolve on item id so ranking is reproducible across runs.
/// </summary>
public sealed class MemoryRanker
{
    public IReadOnlyList<MemoryItem> Rank(string query, IReadOnlyList<MemoryItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
        {
            return items;
        }

        if (items.All(static i => i.Score is not null))
        {
            return items
                .OrderByDescending(static i => i.Score!.Value)
                .ThenBy(static i => i.Id, StringComparer.Ordinal)
                .ToArray();
        }

        var queryTokens = Tokenize(query);
        if (queryTokens.Count == 0)
        {
            return items;
        }

        return items
            .Select(item => (Item: item, Score: ComputeOverlap(queryTokens, item.Content)))
            .OrderByDescending(static t => t.Score)
            .ThenBy(static t => t.Item.Id, StringComparer.Ordinal)
            .Select(t => t.Item with { Score = t.Score })
            .ToArray();
    }

    private static double ComputeOverlap(IReadOnlySet<string> queryTokens, string content)
    {
        var contentTokens = Tokenize(content);
        if (contentTokens.Count == 0)
        {
            return 0d;
        }
        var hits = 0;
        foreach (var token in contentTokens)
        {
            if (queryTokens.Contains(token))
            {
                hits++;
            }
        }
        return hits / (double)contentTokens.Count;
    }

    private static HashSet<string> Tokenize(string text)
    {
        var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(text))
        {
            return tokens;
        }
        var span = text.AsSpan();
        var start = -1;
        for (var i = 0; i < span.Length; i++)
        {
            var c = span[i];
            var isLetterOrDigit = char.IsLetterOrDigit(c);
            if (isLetterOrDigit && start < 0)
            {
                start = i;
            }
            else if (!isLetterOrDigit && start >= 0)
            {
                tokens.Add(text.Substring(start, i - start));
                start = -1;
            }
        }
        if (start >= 0)
        {
            tokens.Add(text.Substring(start));
        }
        return tokens;
    }
}
