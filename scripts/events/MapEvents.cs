using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Event raised when a tile is placed on the map.
/// </summary>
public sealed class EvtTilePlaced : IGameEvent
{
    /// <inheritdoc/>
    public int EventIndex { get; init; }

    /// <inheritdoc/>
    public long Timestamp { get; init; }

    /// <summary>
    /// The placed tile.
    /// </summary>
    public required MapTile Tile { get; init; }

    /// <summary>
    /// Enemies spawned when the tile was placed.
    /// </summary>
    public required IReadOnlyList<SpawnedEnemyInfo> SpawnedEnemies { get; init; }

    /// <summary>
    /// Whether this was the initial starting tile placement.
    /// </summary>
    public bool IsStartingTile { get; init; }
}

/// <summary>
/// Event raised when a tile is revealed (explored by a player).
/// </summary>
public sealed class EvtTileRevealed : IGameEvent
{
    /// <inheritdoc/>
    public int EventIndex { get; init; }

    /// <inheritdoc/>
    public long Timestamp { get; init; }

    /// <summary>
    /// The tile that was revealed.
    /// </summary>
    public required TileId TileId { get; init; }

    /// <summary>
    /// The player who revealed the tile.
    /// </summary>
    public required PlayerId RevealedBy { get; init; }

    /// <summary>
    /// The hex coordinate where the player triggered the reveal.
    /// </summary>
    public required HexCoord TriggerPosition { get; init; }
}

/// <summary>
/// Event raised when the map is fully initialized.
/// </summary>
public sealed class EvtMapInitialized : IGameEvent
{
    /// <inheritdoc/>
    public int EventIndex { get; init; }

    /// <inheritdoc/>
    public long Timestamp { get; init; }

    /// <summary>
    /// The starting tile that was placed.
    /// </summary>
    public required TileId StartingTileId { get; init; }

    /// <summary>
    /// Number of tiles in the countryside deck.
    /// </summary>
    public int CountrysideDeckSize { get; init; }

    /// <summary>
    /// Number of tiles in the core deck.
    /// </summary>
    public int CoreDeckSize { get; init; }

    /// <summary>
    /// Number of tiles in the city deck.
    /// </summary>
    public int CityDeckSize { get; init; }
}
