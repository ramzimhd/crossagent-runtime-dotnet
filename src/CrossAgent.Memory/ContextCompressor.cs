using System;
using System.Collections.Generic;
using CrossAgent.Abstractions.Memory;

namespace CrossAgent.Memory;

/// <summary>
/// Compresses ranked memory items into an <see cref="ActiveContext"/> that fits
/// within a soft token budget. The compressor uses a simple character-based
/// estimator (~4 chars per token) to keep behaviour deterministic without
/// pulling in a tokenizer.
/// </summary>
public sealed class ContextCompressor
{
    private const int CharsPerToken = 4;

    private readonly int _maxTokens;

    public ContextCompressor(int maxTokens = 2048)
    {
        if (maxTokens <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxTokens), "Token budget must be positive.");
        }
        _maxTokens = maxTokens;
    }

    public ActiveContext Compress(IReadOnlyList<MemoryItem> ranked)
    {
        ArgumentNullException.ThrowIfNull(ranked);
        var kept = new List<MemoryItem>(ranked.Count);
        var totalChars = 0;
        var budget = _maxTokens * CharsPerToken;

        foreach (var item in ranked)
        {
            var size = item.Content.Length;
            if (totalChars + size > budget && kept.Count > 0)
            {
                break;
            }
            kept.Add(item);
            totalChars += size;
        }

        var estimatedTokens = (totalChars + CharsPerToken - 1) / CharsPerToken;
        return new ActiveContext { Items = kept, EstimatedTokens = estimatedTokens };
    }
}
