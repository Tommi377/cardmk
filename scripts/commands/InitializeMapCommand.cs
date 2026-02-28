namespace RealMK;

/// <summary>
/// Command to initialize the map and place the starting tile.
/// </summary>
public sealed class InitializeMapCommand : IGameCommand
{
    /// <inheritdoc/>
    public PlayerId PlayerId { get; init; }

    /// <inheritdoc/>
    public int SequenceNumber { get; init; }
}
