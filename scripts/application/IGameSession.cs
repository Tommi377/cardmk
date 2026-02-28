namespace RealMK;

/// <summary>
/// Application facade for game orchestration.
/// </summary>
public interface IGameSession
{
    /// <summary>
    /// Event bus for presentation subscriptions.
    /// </summary>
    EventBus EventBus { get; }

    /// <summary>
    /// Current map state.
    /// </summary>
    MapState MapState { get; }

    /// <summary>
    /// Initializes the map.
    /// </summary>
    TilePlacementResult InitializeMap();

    /// <summary>
    /// Explores a tile at a macro coordinate.
    /// </summary>
    TilePlacementResult? ExploreTile(HexCoord macroCoord, TileCategory? category = null);

    /// <summary>
    /// Gets all explorable map edges.
    /// </summary>
    System.Collections.Generic.IReadOnlyList<(HexCoord Position, int Direction)> GetExplorableEdges();
}
