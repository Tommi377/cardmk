namespace RealMK;

/// <summary>
/// Command to end a player's turn.
/// </summary>
public sealed class EndTurnCommand : IGameCommand
{
    /// <inheritdoc />
    public PlayerId PlayerId { get; init; }

    /// <inheritdoc />
    public int SequenceNumber { get; init; }
}
