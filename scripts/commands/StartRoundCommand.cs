namespace RealMK;

/// <summary>
/// Command that starts a new round.
/// </summary>
public sealed class StartRoundCommand : IGameCommand
{
    /// <inheritdoc />
    public PlayerId PlayerId { get; init; }

    /// <inheritdoc />
    public int SequenceNumber { get; init; }
}
