namespace RealMK;

/// <summary>
/// Abstraction for time used by game events.
/// </summary>
public interface IGameClock
{
    /// <summary>
    /// Gets the next timestamp tick.
    /// </summary>
    long NowTicks();
}
