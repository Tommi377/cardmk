namespace RealMK;

/// <summary>
/// Deterministic monotonic clock for event timestamps.
/// </summary>
public sealed class DeterministicGameClock : IGameClock
{
    private long _current;

    /// <summary>
    /// Creates a deterministic clock.
    /// </summary>
    public DeterministicGameClock(long startTicks = 0)
    {
        _current = startTicks;
    }

    /// <inheritdoc/>
    public long NowTicks()
    {
        return _current++;
    }
}
