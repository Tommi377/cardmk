namespace RealMK;

/// <summary>
/// Base interface for all game commands.
/// Commands represent player intentions that can be validated and executed.
/// </summary>
public interface IGameCommand
{
    /// <summary>
    /// The player issuing this command.
    /// </summary>
    PlayerId PlayerId { get; }

    /// <summary>
    /// Sequential command number for replay and validation.
    /// Must match expected sequence to prevent duplicate/out-of-order commands.
    /// </summary>
    int SequenceNumber { get; }
}
