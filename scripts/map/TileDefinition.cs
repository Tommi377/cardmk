using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Category of map tile determining content and placement rules.
/// </summary>
public enum TileCategory
{
    /// <summary>Starting tile where players begin.</summary>
    Starting,

    /// <summary>Countryside tiles with villages and basic locations.</summary>
    Countryside,

    /// <summary>Core tiles with keeps, towers, and dungeons.</summary>
    Core,

    /// <summary>City tiles with the final objectives.</summary>
    City
}

/// <summary>
/// Defines the static data for a map tile type.
/// Each tile is a 7-hex arrangement loaded from content.
/// </summary>
public sealed class TileDefinition
{
    /// <summary>
    /// Unique identifier for this tile type, e.g., "tile.countryside_01".
    /// </summary>
    public TileDefinitionId Id { get; init; }

    /// <summary>
    /// Category determining when/where this tile can be placed.
    /// </summary>
    public TileCategory Category { get; init; }

    /// <summary>
    /// Localization key for the tile's display name (if any).
    /// </summary>
    public LocalizationKey NameKey { get; init; }

    /// <summary>
    /// Hex cell definitions within this tile.
    /// Key is local coordinate (relative to tile center at 0,0).
    /// </summary>
    public IReadOnlyDictionary<HexCoord, TileHexDefinition> Hexes { get; init; }
        = new Dictionary<HexCoord, TileHexDefinition>();

    /// <summary>
    /// Gets the hex definition at the specified local coordinate.
    /// </summary>
    public TileHexDefinition? GetHex(HexCoord localCoord)
    {
        return Hexes.GetValueOrDefault(localCoord);
    }

    /// <summary>
    /// Returns all local coordinates that have hexes in this tile.
    /// </summary>
    public IEnumerable<HexCoord> GetAllLocalCoords() => Hexes.Keys;
}

/// <summary>
/// Defines a single hex within a tile definition.
/// </summary>
public sealed class TileHexDefinition
{
    /// <summary>
    /// Terrain type for this hex.
    /// </summary>
    public TerrainType Terrain { get; init; }

    /// <summary>
    /// Location ID if this hex contains a location (village, keep, etc.).
    /// </summary>
    public LocationId? LocationId { get; init; }

    /// <summary>
    /// Enemy category that spawns here when tile is revealed.
    /// </summary>
    public EnemyCategory? SpawnCategory { get; init; }

    /// <summary>
    /// Returns true if this hex has a location.
    /// </summary>
    public bool HasLocation => LocationId.HasValue;

    /// <summary>
    /// Returns true if enemies spawn here.
    /// </summary>
    public bool HasSpawn => SpawnCategory.HasValue;
}
