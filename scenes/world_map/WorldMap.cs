using Godot;
using System;
using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Presentation layer for the game world map.
/// Renders MapState to a TileMapLayer and manages visual markers.
/// </summary>
public partial class WorldMap : Node2D
{
    /// <summary>
    /// Reference to the TileMapLayer for terrain rendering.
    /// </summary>
    [Export]
    public TileMapLayer? TileMapLayer { get; set; }

    private MapState? _mapState;
    private EventBus? _eventBus;
    private readonly Dictionary<HexCoord, Node2D> _locationMarkers = new();
    private readonly Dictionary<HexCoord, Node2D> _enemyMarkers = new();

    // Atlas source ID in the TileSet
    private const int AtlasSourceId = 0;

    /// <summary>
    /// Initializes the WorldMap with a MapState and EventBus.
    /// </summary>
    /// <param name="mapState">The map state to render.</param>
    /// <param name="eventBus">Event bus for subscribing to map events.</param>
    public void Initialize(MapState? mapState, EventBus? eventBus)
    {
        _mapState = mapState ?? throw new ArgumentNullException(nameof(mapState));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        // Subscribe to tile placement events
        _eventBus.Subscribe<EvtTilePlaced>(OnTilePlaced);

        Log.Debug("WorldMap initialized");

        // Render the initial map state
        RenderFullMap();
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        _eventBus?.Unsubscribe<EvtTilePlaced>(OnTilePlaced);
    }

    /// <summary>
    /// Renders the entire map from scratch.
    /// </summary>
    public void RenderFullMap()
    {
        if (_mapState == null || TileMapLayer == null)
        {
            Log.Error("WorldMap: Cannot render - MapState or TileMapLayer is null");
            return;
        }

        Log.Debug($"WorldMap: Rendering full map with {_mapState.CellCount} cells");

        // Clear existing tiles and markers
        TileMapLayer.Clear();
        ClearMarkers();

        // Render all cells
        foreach (var tile in _mapState.Tiles.Values)
        {
            RenderTile(tile);
        }
    }

    /// <summary>
    /// Renders a single map tile.
    /// </summary>
    private void RenderTile(MapTile tile)
    {
        if (TileMapLayer == null) return;

        foreach (var kvp in tile.Cells)
        {
            HexCoord worldCoord = kvp.Key;
            HexCell cell = kvp.Value;

            // Convert HexCoord to TileMapLayer coords
            Vector2I tileCoord = HexToTileMapCoord(worldCoord);

            // Get atlas coords for this terrain type
            Vector2I atlasCoord = GetTerrainAtlasCoord(cell.Terrain);

            // Set the tile
            TileMapLayer.SetCell(tileCoord, AtlasSourceId, atlasCoord);

            // Add markers for locations and enemies
            if (cell.HasLocation)
            {
                AddLocationMarker(worldCoord, cell.LocationId!.Value);
            }

            if (cell.HasEnemies)
            {
                AddEnemyMarker(worldCoord, cell.Enemies.Count);
            }
        }
    }

    /// <summary>
    /// Converts a HexCoord to TileMapLayer cell coordinates.
    /// The TileMapLayer uses offset coordinates, while HexCoord uses axial.
    /// </summary>
    public static Vector2I HexToTileMapCoord(HexCoord hex)
    {
        // Convert axial (q, r) to offset coordinates for pointy-top hexes
        // Using odd-r offset: offset_col = q + (r - (r & 1)) / 2, offset_row = r
        int col = hex.Q + (hex.R - (hex.R & 1)) / 2;
        int row = hex.R;
        return new Vector2I(col, row);
    }

    /// <summary>
    /// Converts TileMapLayer cell coordinates back to HexCoord.
    /// </summary>
    public static HexCoord TileMapCoordToHex(Vector2I tileCoord)
    {
        // Convert offset (col, row) to axial (q, r) for pointy-top hexes
        int r = tileCoord.Y;
        int q = tileCoord.X - (r - (r & 1)) / 2;
        return new HexCoord(q, r);
    }

    /// <summary>
    /// Gets the atlas coordinates for a terrain type.
    /// Maps TerrainType to the tile index in the atlas.
    /// </summary>
    private static Vector2I GetTerrainAtlasCoord(TerrainType terrain)
    {
        // Atlas is a horizontal strip, so Y is always 0
        int atlasX = terrain switch
        {
            TerrainType.Plains => 0,
            TerrainType.Forest => 1,
            TerrainType.Hills => 2,
            TerrainType.Swamp => 3,
            TerrainType.Wasteland => 4,
            TerrainType.Desert => 5,
            TerrainType.Mountain => 6,
            TerrainType.Lake => 7,
            TerrainType.Ocean => 7,  // Reuse lake for ocean
            TerrainType.City => 0,   // Fallback to plains for city
            _ => 0
        };

        return new Vector2I(atlasX, 0);
    }

    /// <summary>
    /// Gets the world pixel position for a hex coordinate.
    /// </summary>
    public Vector2 HexToWorldPosition(HexCoord hex)
    {
        if (TileMapLayer == null)
        {
            Log.Warning("WorldMap: TileMapLayer is null, returning zero position");
            return Vector2.Zero;
        }

        Vector2I tileCoord = HexToTileMapCoord(hex);
        return TileMapLayer.MapToLocal(tileCoord);
    }

    /// <summary>
    /// Adds a location marker at the specified hex.
    /// </summary>
    private void AddLocationMarker(HexCoord coord, LocationId locationId)
    {
        if (_locationMarkers.ContainsKey(coord)) return;

        // Create a simple visual marker for locations
        var marker = new Sprite2D
        {
            Position = HexToWorldPosition(coord),
            Modulate = new Color(1, 1, 0, 0.8f), // Yellow tint
            ZIndex = 1
        };

        // Add a label showing the location type
        var label = new Label
        {
            Text = "★",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Position = new Vector2(-10, -20)
        };
        label.AddThemeColorOverride("font_color", new Color(1, 0.8f, 0));
        label.AddThemeFontSizeOverride("font_size", 24);
        marker.AddChild(label);

        AddChild(marker);
        _locationMarkers[coord] = marker;

        Log.Trace($"WorldMap: Added location marker at {coord} for {locationId}");
    }

    /// <summary>
    /// Adds an enemy marker at the specified hex.
    /// </summary>
    private void AddEnemyMarker(HexCoord coord, int count)
    {
        // Remove existing marker if present
        if (_enemyMarkers.TryGetValue(coord, out Node2D? existing))
        {
            existing.QueueFree();
            _enemyMarkers.Remove(coord);
        }

        if (count <= 0) return;

        // Create a simple visual marker for enemies
        var marker = new Node2D
        {
            Position = HexToWorldPosition(coord),
            ZIndex = 2
        };

        var label = new Label
        {
            Text = $"⚔{count}",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Position = new Vector2(-15, 10)
        };
        label.AddThemeColorOverride("font_color", new Color(1, 0.2f, 0.2f));
        label.AddThemeFontSizeOverride("font_size", 18);
        marker.AddChild(label);

        AddChild(marker);
        _enemyMarkers[coord] = marker;

        Log.Trace($"WorldMap: Added enemy marker at {coord} with {count} enemies");
    }

    /// <summary>
    /// Clears all markers from the map.
    /// </summary>
    private void ClearMarkers()
    {
        foreach (var marker in _locationMarkers.Values)
        {
            marker.QueueFree();
        }
        _locationMarkers.Clear();

        foreach (var marker in _enemyMarkers.Values)
        {
            marker.QueueFree();
        }
        _enemyMarkers.Clear();
    }

    /// <summary>
    /// Handles tile placement events to incrementally update the visual.
    /// </summary>
    private void OnTilePlaced(EvtTilePlaced evt)
    {
        Log.Debug($"WorldMap: Received EvtTilePlaced for tile {evt.Tile.Definition.Id}");
        RenderTile(evt.Tile);
    }

    /// <summary>
    /// Updates enemy markers when enemies are added or removed.
    /// </summary>
    public void UpdateEnemyMarker(HexCoord coord, int newCount)
    {
        AddEnemyMarker(coord, newCount);
    }

    /// <summary>
    /// Removes a location marker (e.g., when location is cleared).
    /// </summary>
    public void RemoveLocationMarker(HexCoord coord)
    {
        if (_locationMarkers.TryGetValue(coord, out Node2D? marker))
        {
            marker.QueueFree();
            _locationMarkers.Remove(coord);
        }
    }
}
