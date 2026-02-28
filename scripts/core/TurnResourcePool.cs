namespace RealMK;

/// <summary>
/// Resource pool accumulated from cards during a turn.
/// </summary>
public sealed class TurnResourcePool
{
    /// <summary>
    /// Movement points accumulated this turn.
    /// </summary>
    public int Movement { get; set; }

    /// <summary>
    /// Attack points accumulated this turn.
    /// </summary>
    public int Attack { get; set; }

    /// <summary>
    /// Block points accumulated this turn.
    /// </summary>
    public int Block { get; set; }

    /// <summary>
    /// Influence points accumulated this turn.
    /// </summary>
    public int Influence { get; set; }

    /// <summary>
    /// Healing points accumulated this turn.
    /// </summary>
    public int Healing { get; set; }

    /// <summary>
    /// Clears all turn resources.
    /// </summary>
    public void Clear()
    {
        Movement = 0;
        Attack = 0;
        Block = 0;
        Influence = 0;
        Healing = 0;
    }
}
