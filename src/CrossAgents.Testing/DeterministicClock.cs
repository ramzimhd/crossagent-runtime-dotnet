using System;

namespace CrossAgents.Testing;

/// <summary>
/// A <see cref="TimeProvider"/> that advances only when explicitly stepped.
/// Useful for deterministic audit timestamps.
/// </summary>
public sealed class DeterministicClock : TimeProvider
{
    private DateTimeOffset _now;

    public DeterministicClock(DateTimeOffset start)
    {
        _now = start;
    }

    public DeterministicClock()
        : this(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)) { }

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan interval)
    {
        if (interval < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must not be negative.");
        }
        _now += interval;
    }
}
