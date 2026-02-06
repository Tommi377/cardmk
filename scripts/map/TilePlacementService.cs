using System;
using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Function delegate for selecting enemies by category.
/// </summary>
/// <param name="category">The enemy category to select from.</param>
/// <param name="rng">Random number generator for selection.</param>
/// <returns>Selected enemy ID.</returns>
public delegate EnemyId EnemySelector(EnemyCategory category, DeterministicRandom rng);

/// <summary>
/// Result of a tile placement operation.
/// </summary>
public sealed class TilePlacementResult
{
    /// <summary>
    /// Whether the placement was successful.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Error message if placement failed.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// The placed tile (null if placement failed).
    /// </summary>
    public MapTile? Tile { get; }

    /// <summary>
    /// Enemies spawned during placement.
    /// </summary>
    public IReadOnlyList<SpawnedEnemyInfo> SpawnedEnemies { get; }

    /// <summary>
    /// Creates a successful placement result.
    /// </summary>
    public TilePlacementResult(MapTile tile, IReadOnlyList<SpawnedEnemyInfo> spawnedEnemies)
    {
        IsValid = true;
        Tile = tile;
        SpawnedEnemies = spawnedEnemies;
        ErrorMessage = null;
    }

    /// <summary>
    /// Creates a failed placement result.
    /// </summary>
    public TilePlacementResult(string errorMessage)
    {
        IsValid = false;
        ErrorMessage = errorMessage;
        Tile = null;
        SpawnedEnemies = [];
    }

    /// <summary>
    /// Creates a failed placement result.
    /// </summary>
    public static TilePlacementResult Invalid(string message) => new(message);
}

/// <summary>
/// Information about a spawned enemy.
/// </summary>
public sealed record SpawnedEnemyInfo(
    EnemyInstanceId InstanceId,
    EnemyId EnemyId,
    HexCoord Location,
    EnemyCategory Category
);

/// <summary>
/// Service for placing tiles on the map and spawning enemies.
/// </summary>
public sealed class TilePlacementService
{
    private static int _nextEnemyInstanceId = 1;

    /// <summary>
    /// Resets the enemy instance ID counter (for testing).
    /// </summary>
    internal static void ResetEnemyIdCounter() => _nextEnemyInstanceId = 1;

    /// <summary>
    /// Places a tile on the map at the specified edge position.
    /// </summary>
    /// <param name="map">The map state to modify.</param>
    /// <param name="definition">The tile definition to place.</param>
    /// <param name="edgeCoord">An existing map coordinate at the edge.</param>
    /// <param name="direction">The direction from edgeCoord where the new tile should be placed.</param>
    /// <param name="rng">Random number generator for spawning enemies.</param>
    /// <param name="enemySelector">Function to select enemies by category (optional).</param>
    /// <returns>Result of the placement operation.</returns>
    public TilePlacementResult PlaceTile(
        MapState map,
        TileDefinition definition,
        HexCoord edgeCoord,
        int direction,
        DeterministicRandom rng,
        EnemySelector? enemySelector = null)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(rng);

        Log.Debug($"TilePlacementService: Placing tile {definition.Id} at edge {edgeCoord} direction {direction}");

        // Validate edge coordinate exists
        if (!map.HasHex(edgeCoord))
        {
            return TilePlacementResult.Invalid($"Edge coordinate {edgeCoord} is not on the map");
        }

        // Calculate the center position for the new tile
        HexCoord newTileCenter = CalculateTileCenter(edgeCoord, direction);

        // Check for overlap with existing tiles
        if (WouldOverlap(map, definition, newTileCenter, direction))
        {
            return TilePlacementResult.Invalid($"Tile would overlap with existing tiles at {newTileCenter}");
        }

        // Determine rotation to align tile with the edge
        int rotation = DetermineRotation(edgeCoord, newTileCenter, direction);

        // Create and place the tile
        var tile = new MapTile(definition, newTileCenter, rotation);
        map.PlaceTile(tile);

        // Spawn enemies
        var spawnedEnemies = SpawnEnemies(tile, rng, enemySelector);

        Log.Info($"TilePlacementService: Placed tile {definition.Id} (TileId={tile.TileId}) at {newTileCenter} with {spawnedEnemies.Count} enemies");

        return new TilePlacementResult(tile, spawnedEnemies);
    }

    /// <summary>
    /// Places a tile directly at a center position (for initial map setup).
    /// </summary>
    /// <param name="map">The map state to modify.</param>
    /// <param name="definition">The tile definition to place.</param>
    /// <param name="centerPosition">The center position for the tile.</param>
    /// <param name="rotation">The rotation to apply (0-5).</param>
    /// <param name="rng">Random number generator for spawning enemies.</param>
    /// <param name="enemySelector">Function to select enemies by category (optional).</param>
    /// <returns>Result of the placement operation.</returns>
    public TilePlacementResult PlaceTileAtCenter(
        MapState map,
        TileDefinition definition,
        HexCoord centerPosition,
        int rotation,
        DeterministicRandom rng,
        EnemySelector? enemySelector = null)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(rng);

        Log.Debug($"TilePlacementService: Placing tile {definition.Id} at center {centerPosition} rotation {rotation}");

        // Check for overlap
        if (WouldOverlap(map, definition, centerPosition, rotation))
        {
            return TilePlacementResult.Invalid($"Tile would overlap with existing tiles at {centerPosition}");
        }

        // Create and place the tile
        var tile = new MapTile(definition, centerPosition, rotation);
        map.PlaceTile(tile);

        // Spawn enemies
        var spawnedEnemies = SpawnEnemies(tile, rng, enemySelector);

        Log.Info($"TilePlacementService: Placed tile {definition.Id} at center {centerPosition} with {spawnedEnemies.Count} enemies");

        return new TilePlacementResult(tile, spawnedEnemies);
    }

    /// <summary>
    /// Reveals a tile and triggers discovery effects.
    /// </summary>
    /// <param name="tile">The tile to reveal.</param>
    public void RevealTile(MapTile tile)
    {
        ArgumentNullException.ThrowIfNull(tile);

        if (!tile.IsRevealed)
        {
            tile.Reveal();
        }
    }

    /// <summary>
    /// Gets all valid placement locations for a new tile from a given edge.
    /// </summary>
    /// <param name="map">The map state.</param>
    /// <param name="edgeCoord">The edge coordinate.</param>
    /// <returns>List of valid (direction, centerPosition) tuples.</returns>
    public IReadOnlyList<(int Direction, HexCoord Center)> GetValidPlacements(MapState map, HexCoord edgeCoord)
    {
        ArgumentNullException.ThrowIfNull(map);

        var validPlacements = new List<(int, HexCoord)>();

        for (int direction = 0; direction < 6; direction++)
        {
            HexCoord neighbor = edgeCoord.Neighbor(direction);

            // Check if this direction leads off the current map
            if (!map.HasHex(neighbor))
            {
                HexCoord center = CalculateTileCenter(edgeCoord, direction);
                validPlacements.Add((direction, center));
            }
        }

        return validPlacements;
    }

    /// <summary>
    /// Calculates the center position for a new tile placed adjacent to an edge.
    /// For 7-hex tiles, the new tile's center is placed 2 hexes away from the edge.
    /// This ensures the tiles connect at their boundaries without overlapping.
    /// </summary>
    /// <param name="edgeCoord">The edge coordinate on the existing map.</param>
    /// <param name="direction">The direction to place the new tile.</param>
    /// <returns>The center position for the new tile.</returns>
    public HexCoord CalculateTileCenter(HexCoord edgeCoord, int direction)
    {
        // For 7-hex tiles (center + 6 surrounding), each tile has a "radius" of 1 from center.
        // When connecting two tiles at their edges, we need to place the new center
        // 2 hexes away from the edge hex to avoid overlap:
        // - 1 hex to reach the boundary of the new tile
        // - 1 hex for the new tile's center
        HexCoord step = edgeCoord.Neighbor(direction) - edgeCoord;
        return edgeCoord + step + step;
    }

    /// <summary>
    /// Determines the rotation to align a tile so it connects properly at the edge.
    /// </summary>
    /// <param name="edgeCoord">The existing edge coordinate.</param>
    /// <param name="newCenter">The center of the new tile.</param>
    /// <param name="direction">The direction from edge to center.</param>
    /// <returns>Rotation value (0-5).</returns>
    public int DetermineRotation(HexCoord edgeCoord, HexCoord newCenter, int direction)
    {
        // The tile should be rotated so its connecting edge faces the existing map
        // Direction is from edge to new center, so the opposite direction should face back
        int oppositeDirection = (direction + 3) % 6;

        // Rotation aligns the tile's "north" to face the connection point
        // This is a simplification - real implementation may need tile-specific logic
        return oppositeDirection;
    }

    /// <summary>
    /// Checks if placing a tile at the given position would overlap with existing tiles.
    /// </summary>
    private bool WouldOverlap(MapState map, TileDefinition definition, HexCoord center, int rotation)
    {
        // Check all hexes the tile would occupy
        foreach (var kvp in definition.Hexes)
        {
            HexCoord localOffset = kvp.Key;
            HexCoord rotatedOffset = RotateOffset(localOffset, rotation);
            HexCoord worldPosition = center + rotatedOffset;

            if (map.HasHex(worldPosition))
            {
                Log.Debug($"TilePlacementService: Overlap detected at {worldPosition}");
                return true;
            }
        }

        // If definition has no hexes defined, check default 7-hex layout
        if (definition.Hexes.Count == 0)
        {
            var defaultOffsets = GetDefaultTileOffsets();
            foreach (HexCoord offset in defaultOffsets)
            {
                HexCoord rotatedOffset = RotateOffset(offset, rotation);
                HexCoord worldPosition = center + rotatedOffset;

                if (map.HasHex(worldPosition))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the default 7-hex tile layout offsets.
    /// </summary>
    private static IReadOnlyList<HexCoord> GetDefaultTileOffsets()
    {
        return [
            new HexCoord(0, 0),   // Center
            new HexCoord(1, 0),   // East
            new HexCoord(0, 1),   // Southeast
            new HexCoord(-1, 1),  // Southwest
            new HexCoord(-1, 0),  // West
            new HexCoord(0, -1),  // Northwest
            new HexCoord(1, -1)   // Northeast
        ];
    }

    /// <summary>
    /// Rotates a hex offset by the specified rotation (0-5).
    /// </summary>
    private static HexCoord RotateOffset(HexCoord offset, int rotation)
    {
        if (rotation == 0) return offset;

        int q = offset.Q;
        int r = offset.R;
        int s = -q - r;

        for (int i = 0; i < rotation; i++)
        {
            int newQ = -r;
            int newR = -s;
            s = -q;
            q = newQ;
            r = newR;
        }

        return new HexCoord(q, r);
    }

    /// <summary>
    /// Spawns enemies on the placed tile based on tile definition.
    /// </summary>
    private IReadOnlyList<SpawnedEnemyInfo> SpawnEnemies(
        MapTile tile,
        DeterministicRandom rng,
        EnemySelector? enemySelector)
    {
        var spawned = new List<SpawnedEnemyInfo>();

        foreach (var kvp in tile.Definition.Hexes)
        {
            TileHexDefinition hexDef = kvp.Value;

            if (hexDef.SpawnCategory.HasValue && hexDef.SpawnCount > 0)
            {
                HexCoord localOffset = kvp.Key;
                HexCoord worldPosition = tile.CenterPosition + RotateOffset(localOffset, tile.Rotation);

                for (int i = 0; i < hexDef.SpawnCount; i++)
                {
                    EnemyId enemyId = SelectEnemyFromCategory(hexDef.SpawnCategory.Value, rng, enemySelector);
                    var instanceId = new EnemyInstanceId(_nextEnemyInstanceId++);

                    spawned.Add(new SpawnedEnemyInfo(
                        instanceId,
                        enemyId,
                        worldPosition,
                        hexDef.SpawnCategory.Value
                    ));

                    Log.Debug($"TilePlacementService: Spawned enemy {instanceId} ({enemyId}) at {worldPosition}");
                }
            }
        }

        return spawned;
    }

    /// <summary>
    /// Selects a random enemy from the given category.
    /// </summary>
    private EnemyId SelectEnemyFromCategory(
        EnemyCategory category,
        DeterministicRandom rng,
        EnemySelector? enemySelector)
    {
        if (enemySelector != null)
        {
            return enemySelector(category, rng);
        }

        // Fallback: generate a placeholder enemy ID
        return new EnemyId($"enemy.{category.ToString().ToLowerInvariant()}.default");
    }
}
