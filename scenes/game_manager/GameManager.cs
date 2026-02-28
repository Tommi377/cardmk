using System;
using Godot;

namespace RealMK;

/// <summary>
/// Main facade for interacting with the game.
/// Provides a clean API for the presentation layer to use.
/// </summary>
public partial class GameManager : Node
{
    private IGameSession? _session;

    [Export] private WorldMap _worldMap = null!;

    /// <summary>
    /// The game's event bus for subscribing to events.
    /// </summary>
    public EventBus? EventBus => _session?.EventBus;

    /// <summary>
    /// The current map state.
    /// </summary>
    public MapState? MapState => _session?.MapState;

    /// <summary>
    /// Seed used for deterministic random number generation.
    /// </summary>
    [Export]
    public ulong Seed { get; set; } = 12345;

    public override void _Ready()
    {
        base._Ready();
        InitializeGame();
    }

    private void InitializeGame()
    {
        ShowLoading(true);

        try
        {
            Log.Info("Initializing game...");

            string contentPath = ProjectSettings.GlobalizePath("res://content");
            _session = GameSessionFactory.CreateFromContentPath(contentPath, Seed, validate: true);

            TilePlacementResult result = _session.InitializeMap();
            _worldMap.Initialize(MapState, EventBus);
            if (result is { IsValid: true, Tile: not null })
            {
                Log.Info($"GameManager: Map initialized with starting tile {result.Tile.Definition.Id}");
            }
            else
            {
                Log.Error($"GameManager: Failed to initialize map: {result.ErrorMessage}");
            }

            Log.Info("Game initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to initialize game: {ex.Message}");
            Log.Error(ex.StackTrace ?? "No stack trace");
        }
        finally
        {
            ShowLoading(false);
        }
    }

    /// <summary>
    /// Initializes a WorldMap presentation node with the current map state.
    /// </summary>
    /// <param name="worldMap">The WorldMap node to initialize.</param>
    public void InitializeWorldMap(WorldMap worldMap)
    {
        if (_session == null)
        {
            Log.Error("GameManager: Cannot initialize WorldMap - session not ready");
            return;
        }

        worldMap.Initialize(_session.MapState, _session.EventBus);
        Log.Debug("GameManager: WorldMap initialized");
    }

    /// <summary>
    /// Explores a new tile at the given edge position.
    /// </summary>
    /// <param name="macroCoord">An existing map tile coordinate.</param>
    /// <param name="category">Optional: force a specific tile category.</param>
    /// <returns>Result of the tile placement.</returns>
    public TilePlacementResult? ExploreTile(HexCoord macroCoord, TileCategory? category = null)
    {
        if (_session == null)
        {
            Log.Error("GameManager: Cannot explore tile - session not ready");
            return null;
        }

        TilePlacementResult? result = _session.ExploreTile(macroCoord, category);

        if (result is { IsValid: true, Tile: not null })
        {
            Log.Info($"GameManager: Explored tile {result.Tile.Definition.Id} at {macroCoord}");
        }

        return result;
    }

    /// <summary>
    /// Gets all explorable edges on the current map.
    /// </summary>
    public System.Collections.Generic.IReadOnlyList<(HexCoord Position, int Direction)>? GetExplorableEdges()
    {
        return _session?.GetExplorableEdges();
    }

    private void ShowLoading(bool show)
    {
        // TODO: Add loading overlay
    }
}
