namespace RealMK;

/// <summary>
/// Simple in-memory event index provider.
/// </summary>
public sealed class SequentialEventIndexProvider : IEventIndexProvider
{
    private int _nextIndex;

    /// <inheritdoc/>
    public int NextEventIndex()
    {
        return _nextIndex++;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        _nextIndex = 0;
    }
}
