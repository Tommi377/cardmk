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
    /// <param name="macroCoord">Coordinates where to place the hexes in macro coordinates</param>
    /// <param name="rng">Random number generator for spawning enemies.</param>
    /// <param name="enemySelector">Function to select enemies by category (optional).</param>
    /// <param name="rotation">The rotation of the map tile on placement.</param>
    /// <returns>Result of the placement operation.</returns>
    public TilePlacementResult PlaceTile(
        MapState map,
        TileDefinition definition,
        HexCoord macroCoord,
        DeterministicRandom rng,
        EnemySelector? enemySelector = null,
        int rotation = 0)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(rng);

        HexCoord tileCenter = macroCoord.ToMicroCoord();
        
        Log.Debug($"TilePlacementService: Placing tile {definition.Id} at {tileCenter}({macroCoord}) with rotation {rotation}");

        // Check for overlap with existing tiles
        if (WouldOverlap(map, definition, macroCoord.ToMicroCoord(), rotation))
        {
            return TilePlacementResult.Invalid($"Tile would overlap with existing tiles at {tileCenter}");
        }

        // Create and place the tile
        var tile = new MapTile(definition, tileCenter, rotation);
        map.PlaceTile(tile);

        // Spawn enemies
        var spawnedEnemies = SpawnEnemies(tile, rng, enemySelector);

        Log.Info($"TilePlacementService: Placed tile {definition.Id} (TileId={tile.TileId}) at {tileCenter}({macroCoord}) with {spawnedEnemies.Count} enemies");

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
    /// Checks if placing a tile at the given position would overlap with existing tiles.
    /// </summary>
    private bool WouldOverlap(MapState map, TileDefinition definition, HexCoord center, int rotation)
    {
        // Check all hexes the tile would occupy
        foreach (var kvp in definition.Hexes)
        {
            HexCoord localOffset = kvp.Key;
            HexCoord rotatedOffset = localOffset.RotateOffset(rotation);
            HexCoord worldPosition = center + rotatedOffset;

            if (map.HasHex(worldPosition))
            {
                Log.Debug($"TilePlacementService: Overlap detected at {worldPosition}");
                return true;
            }
        }
        return false;
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

            if (hexDef.SpawnCategory.HasValue)
            {
                HexCoord localOffset = kvp.Key;
                HexCoord worldPosition = tile.CenterPosition + localOffset.RotateOffset(tile.Rotation);

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
