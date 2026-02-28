namespace RealMK;

/// <summary>
/// Provides monotonic event indexes for deterministic event ordering.
/// </summary>
public interface IEventIndexProvider
{
    /// <summary>
    /// Gets the next event index.
    /// </summary>
    int NextEventIndex();

    /// <summary>
    /// Resets event indexing to zero.
    /// </summary>
    void Reset();
}
