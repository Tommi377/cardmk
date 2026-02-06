using System;
using Godot;

namespace RealMK;

/// <summary>
/// Main facade for interacting with the game.
/// Provides a clean API for the presentation layer to use.
/// </summary>
public partial class GameManager : Node
{
    private ContentDatabase? _contentDb;
    private EventBus? _eventBus;
    private CommandDispatcher? _dispatcher;
    private MapGenerator? _mapGenerator;
    private DeterministicRandom? _rng;

    [Export] private WorldMap _worldMap = null!;

    /// <summary>
    /// The game's event bus for subscribing to events.
    /// </summary>
    public EventBus? EventBus => _eventBus;

    /// <summary>
    /// The current map state.
    /// </summary>
    public MapState? MapState => _mapGenerator?.Map;

    /// <summary>
    /// The map generator for exploration.
    /// </summary>
    public MapGenerator? MapGenerator => _mapGenerator;

    /// <summary>
    /// Seed used for random number generation.
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

        _eventBus = new EventBus();
        _dispatcher = new CommandDispatcher(_eventBus);
        _rng = new DeterministicRandom(Seed);

        try
        {
            Log.Info("Initializing game...");

            // Create content database and load content
            var loader = new ContentLoader();
            string contentPath = ProjectSettings.GlobalizePath("res://content");

            // Try to load content, but don't fail if it doesn't exist
            if (DirAccess.DirExistsAbsolute(contentPath))
            {
                _contentDb = loader.LoadAll(contentPath, validate: false);
                Log.Debug($"GameManager: Loaded content from {contentPath}");
            }
            else
            {
                Log.Error($"GameManager: Content directory not found at {contentPath}, using empty database");
                _contentDb = new ContentDatabase();
            }

            // Initialize map generator and place starting tile
            InitializeMap();

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
    /// Initializes the map with a starting tile.
    /// </summary>
    private void InitializeMap()
    {
        if (_contentDb == null || _rng == null || _eventBus == null)
        {
            Log.Error("GameManager: Cannot initialize map - dependencies not ready");
            return;
        }

        _mapGenerator = new MapGenerator(_contentDb, _rng);
        var result = _mapGenerator.InitializeMap();

        _worldMap.Initialize(MapState, EventBus);

        if (result is { IsValid: true, Tile: not null })
        {
            // Publish tile placed event
            var evt = new EvtTilePlaced
            {
                EventIndex = 0,
                Timestamp = DateTime.UtcNow.Ticks,
                Tile = result.Tile,
                SpawnedEnemies = result.SpawnedEnemies,
                IsStartingTile = true
            };
            _eventBus.Publish(evt);

            // Publish map initialized event
            var initEvt = new EvtMapInitialized
            {
                EventIndex = 1,
                Timestamp = DateTime.UtcNow.Ticks,
                StartingTileId = result.Tile.TileId,
                CountrysideDeckSize = _mapGenerator.GetDeckCount(TileCategory.Countryside),
                CoreDeckSize = _mapGenerator.GetDeckCount(TileCategory.Core),
                CityDeckSize = _mapGenerator.GetDeckCount(TileCategory.City)
            };
            _eventBus.Publish(initEvt);

            Log.Info($"GameManager: Map initialized with starting tile {result.Tile.Definition.Id}");
        }
        else
        {
            Log.Error($"GameManager: Failed to initialize map: {result.ErrorMessage}");
        }
    }

    /// <summary>
    /// Initializes a WorldMap presentation node with the current map state.
    /// </summary>
    /// <param name="worldMap">The WorldMap node to initialize.</param>
    public void InitializeWorldMap(WorldMap worldMap)
    {
        if (_mapGenerator == null || _eventBus == null)
        {
            Log.Error("GameManager: Cannot initialize WorldMap - MapGenerator or EventBus not ready");
            return;
        }

        worldMap.Initialize(_mapGenerator.Map, _eventBus);
        Log.Debug("GameManager: WorldMap initialized");
    }

    /// <summary>
    /// Explores a new tile at the given edge position.
    /// </summary>
    /// <param name="edgeCoord">An existing hex at the map edge.</param>
    /// <param name="direction">Direction to place the new tile (0-5).</param>
    /// <param name="category">Optional: force a specific tile category.</param>
    /// <returns>Result of the tile placement.</returns>
    public TilePlacementResult? ExploreTile(HexCoord edgeCoord, int direction, TileCategory? category = null)
    {
        if (_mapGenerator == null || _eventBus == null)
        {
            Log.Error("GameManager: Cannot explore tile - MapGenerator or EventBus not ready");
            return null;
        }

        var result = _mapGenerator.ExploreTile(edgeCoord, direction, category);

        if (result.IsValid && result.Tile != null)
        {
            // Publish tile placed event
            var evt = new EvtTilePlaced
            {
                EventIndex = 0, // TODO: proper event indexing
                Timestamp = DateTime.UtcNow.Ticks,
                Tile = result.Tile,
                SpawnedEnemies = result.SpawnedEnemies,
                IsStartingTile = false
            };
            _eventBus.Publish(evt);

            Log.Info($"GameManager: Explored tile {result.Tile.Definition.Id} at direction {direction} from {edgeCoord}");
        }

        return result;
    }

    /// <summary>
    /// Gets all explorable edges on the current map.
    /// </summary>
    public System.Collections.Generic.IReadOnlyList<(HexCoord Position, int Direction)>? GetExplorableEdges()
    {
        return _mapGenerator?.GetExplorableEdges();
    }

    private void ShowLoading(bool show)
    {
        // TODO: Add loading overlay
    }
}
