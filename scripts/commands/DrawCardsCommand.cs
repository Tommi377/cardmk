namespace RealMK;

/// <summary>
/// Command to draw cards from draw pile to hand.
/// </summary>
public sealed class DrawCardsCommand : IGameCommand
{
    /// <inheritdoc />
    public PlayerId PlayerId { get; init; }

    /// <inheritdoc />
    public int SequenceNumber { get; init; }

    /// <summary>
    /// Number of cards requested.
    /// </summary>
    public int Count { get; init; }
}
