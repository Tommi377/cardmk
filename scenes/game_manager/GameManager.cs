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
    /// The current game state aggregate.
    /// </summary>
    public GameState? State => _session?.State;

    /// <summary>
    /// Returns true when the game session is initialized.
    /// </summary>
    public bool IsSessionReady => _session != null;

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

    /// <summary>
    /// Starts a new round.
    /// </summary>
    public CommandResult StartRound()
    {
        if (_session == null)
        {
            return SessionUnavailable("Cannot start round - session not ready");
        }

        return _session.StartRound();
    }

    /// <summary>
    /// Draws cards for a player.
    /// </summary>
    /// <param name="playerId">Player drawing cards.</param>
    /// <param name="count">Number of cards to draw.</param>
    public CommandResult DrawCards(PlayerId playerId, int count)
    {
        if (_session == null)
        {
            return SessionUnavailable("Cannot draw cards - session not ready");
        }

        return _session.DrawCards(playerId, count);
    }

    /// <summary>
    /// Plays a card for a player.
    /// </summary>
    /// <param name="request">Play request payload.</param>
    public CommandResult PlayCard(PlayCardRequest request)
    {
        if (_session == null)
        {
            return SessionUnavailable("Cannot play card - session not ready");
        }

        return _session.PlayCard(request);
    }

    /// <summary>
    /// Ends the current turn for a player.
    /// </summary>
    /// <param name="playerId">Player ending their turn.</param>
    public CommandResult EndTurn(PlayerId playerId)
    {
        if (_session == null)
        {
            return SessionUnavailable("Cannot end turn - session not ready");
        }

        return _session.EndTurn(playerId);
    }

    /// <summary>
    /// Gets a player state by id if available.
    /// </summary>
    /// <param name="playerId">Player id.</param>
    /// <returns>Player state or null if unavailable.</returns>
    public PlayerState? GetPlayerState(PlayerId playerId)
    {
        if (_session?.State == null)
        {
            return null;
        }

        return _session.State.TryGetPlayer(playerId, out PlayerState? player) ? player : null;
    }

    private void ShowLoading(bool show)
    {
        // TODO: Add loading overlay
    }

    private static CommandResult SessionUnavailable(string message)
    {
        return CommandResult.Invalid("SESSION_NOT_READY", message);
    }
}
