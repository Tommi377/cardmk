namespace RealMK;

/// <summary>
/// Command to play a card from hand.
/// </summary>
public sealed class PlayCardCommand : IGameCommand
{
    /// <inheritdoc />
    public PlayerId PlayerId { get; init; }

    /// <inheritdoc />
    public int SequenceNumber { get; init; }

    /// <summary>
    /// Card instance to play.
    /// </summary>
    public CardInstanceId CardInstanceId { get; init; }

    /// <summary>
    /// Selected play mode.
    /// </summary>
    public CardPlayMode Mode { get; init; }

    /// <summary>
    /// Deterministic resolution inputs for this play.
    /// </summary>
    public CardResolutionInput ResolutionInput { get; init; } = new();
}
