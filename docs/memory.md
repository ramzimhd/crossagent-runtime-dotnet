# Memory

Memory is optional. The `CrossAgent.Memory` package contributes a small set of utilities that turn an `IMemoryProvider` into an `ActiveContext` patterns can consume; it does not own a database, vector index, or embedding model.

## Components

- **`MemoryRetriever`** - a thin wrapper over `IMemoryProvider` that validates the query and returns the matched items.
- **`MemoryRanker`** - a deterministic ranker. When all items already carry a score it honors them; otherwise it ranks by length-normalised lexical overlap with the query. Ties resolve on item id.
- **`ContextCompressor`** - compresses ranked items into an `ActiveContext` whose total content stays under a soft token budget. Uses a character-based estimator so the compressor has no tokenizer dependency.
- **`SlidingMemoryBuffer<T>`** - a bounded FIFO buffer suitable for retaining recent turns or events. Per session, not shared.

## Why retrieval is not in the runtime

The runtime never queries memory itself. Instead, patterns that need context call into the memory layer they were configured with. That keeps the runtime free of retrieval semantics and lets applications choose the strategy that fits their store - keyword search, vector retrieval, or something else - without forking the runtime.

## Plugging memory in

Implement `IMemoryProvider` for your store:

```csharp
public sealed class MyMemoryProvider : IMemoryProvider
{
    public Task<IReadOnlyList<MemoryItem>> SearchAsync(MemoryQuery query, CancellationToken cancellationToken = default)
    {
        // ... call your store, return MemoryItem instances ...
    }
}
```

Pass the provider to `RuntimeOptions.Memory`. Patterns that consume an `ActiveContext` can then ask the runtime's services for memory and run it through `MemoryRetriever`, `MemoryRanker`, and `ContextCompressor` to build the active window.

## What this milestone deliberately omits

- No vector store implementation.
- No embedding pipeline.
- No persistence semantics. Storage lifetime is owned by the application.
- No automatic summarisation. The compressor truncates by token budget rather than rewriting content.
