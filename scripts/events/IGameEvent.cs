namespace RealMK;

/// <summary>
/// Base interface for all game events.
/// Events represent state changes that have occurred in the game.
/// </summary>
public interface IGameEvent
{
    /// <summary>
    /// Sequential index of this event within the game.
    /// Used for replay and synchronization.
    /// </summary>
    int EventIndex { get; }

    /// <summary>
    /// Timestamp when the event occurred (game ticks or real time).
    /// </summary>
    long Timestamp { get; }
}
