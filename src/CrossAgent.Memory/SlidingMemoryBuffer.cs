using System;
using System.Collections;
using System.Collections.Generic;

namespace CrossAgent.Memory;

/// <summary>
/// A bounded FIFO buffer used to retain the most recent conversation turns or
/// events for context construction. The buffer is intentionally simple and
/// thread-affine; share at most one buffer per session.
/// </summary>
public sealed class SlidingMemoryBuffer<T> : IReadOnlyCollection<T>
{
    private readonly Queue<T> _items;
    private readonly int _capacity;

    public SlidingMemoryBuffer(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
        }
        _capacity = capacity;
        _items = new Queue<T>(capacity);
    }

    public int Capacity => _capacity;

    public int Count => _items.Count;

    public void Add(T item)
    {
        if (_items.Count == _capacity)
        {
            _items.Dequeue();
        }
        _items.Enqueue(item);
    }

    public void Clear() => _items.Clear();

    public IReadOnlyList<T> Snapshot()
    {
        var array = new T[_items.Count];
        _items.CopyTo(array, 0);
        return array;
    }

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
