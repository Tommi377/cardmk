using Godot;

namespace RealMK;

/// <summary>
/// A TileMapLayer that highlights the hex cell currently under the mouse cursor.
/// Enable or disable highlighting via <see cref="HighlightingEnabled"/>.
/// </summary>
public partial class HighlightLayer : TileMapLayer
{
    /// <summary>
    /// Atlas source ID in the shared TileSet.
    /// </summary>
    [Export] private int AtlasSourceId = 0;

    /// <summary>
    /// Atlas coordinate of the highlight sprite.
    /// </summary>
    [Export] private Vector2I HighlightAtlasCoord = new(0, 2);

    private Vector2I? _currentCell;
    private bool _highlightingEnabled = true;
    private MapState? _mapState;

    /// <summary>
    /// Sets the MapState used to determine which cells are valid for highlighting.
    /// </summary>
    public void SetMapState(MapState mapState)
    {
        _mapState = mapState;
    }

    /// <summary>
    /// Gets or sets whether mouse-based highlighting is active.
    /// Disabling clears any existing highlight.
    /// </summary>
    public bool HighlightingEnabled
    {
        get => _highlightingEnabled;
        set
        {
            if (_highlightingEnabled == value) return;
            _highlightingEnabled = value;
            if (!_highlightingEnabled)
            {
                ClearHighlight();
            }
        }
    }

    /// <summary>
    /// The tile-map cell coordinate that is currently highlighted, or null if none.
    /// </summary>
    public Vector2I? CurrentCell => _currentCell;

    public override void _Process(double delta)
    {
        if (!_highlightingEnabled) return;

        Vector2 mouseLocal = GetLocalMousePosition();
        Vector2I cell = LocalToMap(mouseLocal);

        if (_currentCell.HasValue && _currentCell.Value == cell) return;

        // Check whether the cell exists in MapState
        HexCoord hex = WorldMap.TileMapCoordToHex(cell);
        bool cellExists = _mapState?.GetCell(hex) != null;

        // Erase previous highlight tile
        if (_currentCell.HasValue)
        {
            EraseCell(_currentCell.Value);
            _currentCell = null;
        }

        // Only highlight cells that exist on the map
        if (!cellExists) return;

        _currentCell = cell;
        SetCell(cell, AtlasSourceId, HighlightAtlasCoord);
    }

    /// <summary>
    /// Clears the current highlight, if any.
    /// </summary>
    public void ClearHighlight()
    {
        if (_currentCell.HasValue)
        {
            EraseCell(_currentCell.Value);
            _currentCell = null;
        }
    }
}