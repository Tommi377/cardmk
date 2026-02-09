using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Represents a placed tile on the map with its cells.
/// </summary>
public sealed class MapTile
{
    private static int _nextTileId = 1;
    private readonly Dictionary<HexCoord, HexCell> _cells;

    /// <summary>
    /// Creates a map tile from a definition placed at a specific position.
    /// </summary>
    /// <param name="definition">The tile definition.</param>
    /// <param name="centerPosition">The center hex coordinate where this tile is placed.</param>
    /// <param name="rotation">Rotation of the tile (0-5, representing 60° increments).</param>
    public MapTile(TileDefinition definition, HexCoord centerPosition, int rotation = 0)
    {
        Definition = definition;
        CenterPosition = centerPosition;
        Rotation = rotation % 6;
        TileId = new TileId(_nextTileId++);
        _cells = new Dictionary<HexCoord, HexCell>();

        // Initialize cells based on tile definition layout
        InitializeCells();

        Log.Debug($"MapTile {definition.Id} (TileId={TileId}) placed at {centerPosition} with rotation {Rotation}");
    }

    /// <summary>
    /// The tile definition.
    /// </summary>
    public TileDefinition Definition { get; }

    /// <summary>
    /// The center hex coordinate of this placed tile.
    /// </summary>
    public HexCoord CenterPosition { get; }

    /// <summary>
    /// Rotation of the tile (0-5, representing 60° increments).
    /// </summary>
    public int Rotation { get; }

    /// <summary>
    /// Whether this tile has been revealed (explored).
    /// </summary>
    public bool IsRevealed { get; private set; }

    /// <summary>
    /// All cells in this tile.
    /// </summary>
    public IReadOnlyDictionary<HexCoord, HexCell> Cells => _cells;

    /// <summary>
    /// Runtime TileId for this placed tile instance.
    /// </summary>
    public TileId TileId { get; }

    /// <summary>
    /// Resets the tile ID counter (for testing only).
    /// </summary>
    internal static void ResetIdCounter() => _nextTileId = 1;

    /// <summary>
    /// Initializes the hex cells for this tile.
    /// </summary>
    private void InitializeCells()
    {
        // Create cells from the tile definition's hexes
        foreach (var kvp in Definition.Hexes)
        {
            HexCoord localCoord = kvp.Key;
            TileHexDefinition hexDef = kvp.Value;

            HexCoord rotatedOffset = RotateOffset(localCoord, Rotation);
            HexCoord worldPosition = CenterPosition + rotatedOffset;

            var cell = new HexCell(
                worldPosition,
                hexDef.Terrain,
                TileId,
                hexDef.LocationType);

            _cells[worldPosition] = cell;
        }

        // If definition has no hexes, create a standard 7-hex layout
        if (_cells.Count == 0)
        {
            CreateDefaultCells();
        }
    }

    /// <summary>
    /// Creates a default 7-hex tile layout when definition has no hexes.
    /// </summary>
    private void CreateDefaultCells()
    {
        var offsets = new List<HexCoord>
        {
            new(0, 0),  // Center
            new(1, 0), new(0, 1), new(-1, 1),  // East neighbors
            new(-1, 0), new(0, -1), new(1, -1)  // West neighbors
        };

        foreach (HexCoord offset in offsets)
        {
            HexCoord rotatedOffset = RotateOffset(offset, Rotation);
            HexCoord worldPosition = CenterPosition + rotatedOffset;

            var cell = new HexCell(
                worldPosition,
                TerrainType.Plains,
                TileId);

            _cells[worldPosition] = cell;
        }
    }

    /// <summary>
    /// Rotates a hex offset by the specified rotation (0-5).
    /// </summary>
    private static HexCoord RotateOffset(HexCoord offset, int rotation)
    {
        if (rotation == 0) return offset;

        // Cube coordinate rotation
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
    /// Gets a cell at the specified world coordinate.
    /// </summary>
    public HexCell? GetCell(HexCoord worldPosition)
    {
        return _cells.TryGetValue(worldPosition, out HexCell? cell) ? cell : null;
    }

    /// <summary>
    /// Checks if this tile contains the specified world coordinate.
    /// </summary>
    public bool ContainsHex(HexCoord worldPosition)
    {
        return _cells.ContainsKey(worldPosition);
    }

    /// <summary>
    /// Reveals this tile.
    /// </summary>
    public void Reveal()
    {
        if (!IsRevealed)
        {
            IsRevealed = true;
            Log.Info($"MapTile {Definition.Id} at {CenterPosition} revealed");
        }
    }

    /// <summary>
    /// Gets all world coordinates occupied by this tile.
    /// </summary>
    public IEnumerable<HexCoord> GetAllPositions()
    {
        return _cells.Keys;
    }
}
