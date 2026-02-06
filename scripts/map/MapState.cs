using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Represents the complete map state with all placed tiles and cells.
/// </summary>
public sealed class MapState
{
    private readonly Dictionary<TileId, MapTile> _tiles;
    private readonly Dictionary<HexCoord, HexCell> _cellsByPosition;
    private readonly Dictionary<HexCoord, TileId> _tileByPosition;

    /// <summary>
    /// Creates an empty map state.
    /// </summary>
    public MapState()
    {
        _tiles = new Dictionary<TileId, MapTile>();
        _cellsByPosition = new Dictionary<HexCoord, HexCell>();
        _tileByPosition = new Dictionary<HexCoord, TileId>();
    }

    /// <summary>
    /// All placed tiles on the map.
    /// </summary>
    public IReadOnlyDictionary<TileId, MapTile> Tiles => _tiles;

    /// <summary>
    /// Number of placed tiles.
    /// </summary>
    public int TileCount => _tiles.Count;

    /// <summary>
    /// Number of total cells on the map.
    /// </summary>
    public int CellCount => _cellsByPosition.Count;

    /// <summary>
    /// Places a tile on the map.
    /// </summary>
    /// <param name="tile">The tile to place.</param>
    /// <returns>True if placed successfully, false if overlapping with existing tiles.</returns>
    public bool PlaceTile(MapTile tile)
    {
        // Check for overlaps
        foreach (HexCoord pos in tile.GetAllPositions())
        {
            if (_cellsByPosition.ContainsKey(pos))
            {
                Log.Warning($"MapState: Cannot place tile {tile.Definition.Id} - overlaps at {pos}");
                return false;
            }
        }

        // Add tile
        _tiles[tile.TileId] = tile;

        // Add all cells
        foreach (var kvp in tile.Cells)
        {
            _cellsByPosition[kvp.Key] = kvp.Value;
            _tileByPosition[kvp.Key] = tile.TileId;
        }

        Log.Info($"MapState: Placed tile {tile.Definition.Id} at {tile.CenterPosition}");
        return true;
    }

    /// <summary>
    /// Checks if a hex coordinate exists on the map.
    /// </summary>
    public bool HasHex(HexCoord position)
    {
        return _cellsByPosition.ContainsKey(position);
    }

    /// <summary>
    /// Gets the cell at the specified position.
    /// </summary>
    public HexCell? GetCell(HexCoord position)
    {
        return _cellsByPosition.GetValueOrDefault(position);
    }

    /// <summary>
    /// Gets the tile containing the specified position.
    /// </summary>
    public MapTile? GetTileAt(HexCoord position)
    {
        if (!_tileByPosition.TryGetValue(position, out TileId tileId))
            return null;

        return _tiles.GetValueOrDefault(tileId);
    }

    /// <summary>
    /// Gets all adjacent positions to the given hex.
    /// </summary>
    public IReadOnlyList<HexCoord> GetAdjacentPositions(HexCoord position)
    {
        var adjacent = new List<HexCoord>();
        foreach (HexCoord neighbor in position.AllNeighbors())
        {
            if (_cellsByPosition.ContainsKey(neighbor))
            {
                adjacent.Add(neighbor);
            }
        }
        return adjacent;
    }

    /// <summary>
    /// Gets positions adjacent to the given hex that are not yet on the map (for tile placement).
    /// </summary>
    public IReadOnlyList<HexCoord> GetAdjacentUnrevealedEdges(HexCoord position)
    {
        var edges = new List<HexCoord>();
        foreach (HexCoord neighbor in position.AllNeighbors())
        {
            if (!_cellsByPosition.ContainsKey(neighbor))
            {
                edges.Add(neighbor);
            }
        }
        return edges;
    }

    /// <summary>
    /// Gets all cells within a specified distance from a position.
    /// </summary>
    public IReadOnlyList<HexCell> GetCellsInRange(HexCoord center, int range)
    {
        var cells = new List<HexCell>();
        foreach (HexCoord pos in GetHexesInRange(center, range))
        {
            if (_cellsByPosition.TryGetValue(pos, out HexCell? cell))
            {
                cells.Add(cell);
            }
        }
        return cells;
    }

    /// <summary>
    /// Gets all positions within a specified distance from a position.
    /// </summary>
    public IReadOnlyList<HexCoord> GetPositionsInRange(HexCoord center, int range)
    {
        var positions = new List<HexCoord>();
        foreach (HexCoord pos in GetHexesInRange(center, range))
        {
            if (_cellsByPosition.ContainsKey(pos))
            {
                positions.Add(pos);
            }
        }
        return positions;
    }

    /// <summary>
    /// Generates all hex coordinates within a range of the center.
    /// </summary>
    private static IEnumerable<HexCoord> GetHexesInRange(HexCoord center, int range)
    {
        for (int q = -range; q <= range; q++)
        {
            for (int r = Math.Max(-range, -q - range); r <= Math.Min(range, -q + range); r++)
            {
                yield return new HexCoord(center.Q + q, center.R + r);
            }
        }
    }

    /// <summary>
    /// Finds a path between two positions.
    /// </summary>
    /// <param name="from">Starting position.</param>
    /// <param name="to">Target position.</param>
    /// <param name="canTraverse">Function to check if a cell can be traversed.</param>
    /// <returns>List of positions forming the path, or empty if no path exists.</returns>
    public IReadOnlyList<HexCoord> FindPath(HexCoord from, HexCoord to, Func<HexCell, bool> canTraverse)
    {
        if (!HasHex(from) || !HasHex(to))
            return [];

        // A* pathfinding
        var openSet = new PriorityQueue<HexCoord, int>();
        var cameFrom = new Dictionary<HexCoord, HexCoord>();
        var gScore = new Dictionary<HexCoord, int> { [from] = 0 };
        var fScore = new Dictionary<HexCoord, int> { [from] = from.DistanceTo(to) };

        openSet.Enqueue(from, fScore[from]);

        while (openSet.Count > 0)
        {
            HexCoord current = openSet.Dequeue();

            if (current == to)
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach (HexCoord neighbor in current.AllNeighbors())
            {
                if (!_cellsByPosition.TryGetValue(neighbor, out HexCell? cell))
                    continue;

                if (!canTraverse(cell))
                    continue;

                int tentativeG = gScore[current] + 1;

                if (!gScore.TryGetValue(neighbor, out int neighborG) || tentativeG < neighborG)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + neighbor.DistanceTo(to);

                    // PriorityQueue doesn't have Contains, so we just enqueue
                    openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        return [];
    }

    /// <summary>
    /// Reconstructs a path from the A* cameFrom dictionary.
    /// </summary>
    private static IReadOnlyList<HexCoord> ReconstructPath(Dictionary<HexCoord, HexCoord> cameFrom, HexCoord current)
    {
        var path = new List<HexCoord> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    /// <summary>
    /// Gets all revealed tiles.
    /// </summary>
    public IEnumerable<MapTile> GetRevealedTiles()
    {
        return _tiles.Values.Where(t => t.IsRevealed);
    }

    /// <summary>
    /// Gets all unrevealed tiles.
    /// </summary>
    public IEnumerable<MapTile> GetUnrevealedTiles()
    {
        return _tiles.Values.Where(t => !t.IsRevealed);
    }

    /// <summary>
    /// Gets all hexes with a specific terrain type.
    /// </summary>
    public IEnumerable<HexCoord> GetHexesWithTerrain(TerrainType terrain)
    {
        return _cellsByPosition
            .Where(kvp => kvp.Value.Terrain == terrain)
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    /// Clears the entire map.
    /// </summary>
    public void Clear()
    {
        _tiles.Clear();
        _cellsByPosition.Clear();
        _tileByPosition.Clear();
        Log.Debug("MapState: Cleared all tiles");
    }
}
