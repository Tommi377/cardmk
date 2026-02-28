namespace RealMK;

/// <summary>
/// Command to explore a map tile at the provided macro coordinate.
/// </summary>
public sealed class ExploreTileCommand : IGameCommand
{
    /// <inheritdoc/>
    public PlayerId PlayerId { get; init; }

    /// <inheritdoc/>
    public int SequenceNumber { get; init; }

    /// <summary>
    /// Macro coordinate where the tile should be placed.
    /// </summary>
    public HexCoord MacroCoord { get; init; }

    /// <summary>
    /// Optional forced tile category.
    /// </summary>
    public TileCategory? Category { get; init; }
}
