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
    /// Current game state aggregate.
    /// </summary>
    GameState State { get; }

    /// <summary>
    /// Current map state.
    /// </summary>
    MapState MapState { get; }

    /// <summary>
    /// Validates a command without executing it.
    /// </summary>
    ValidationResult Validate(IGameCommand command);

    /// <summary>
    /// Dispatches a command through the command pipeline.
    /// </summary>
    CommandResult Dispatch(IGameCommand command);

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

    /// <summary>
    /// Starts a new round.
    /// </summary>
    CommandResult StartRound();

    /// <summary>
    /// Draws cards for the specified player.
    /// </summary>
    CommandResult DrawCards(PlayerId playerId, int count);

    /// <summary>
    /// Plays a card for the specified player.
    /// </summary>
    CommandResult PlayCard(PlayCardRequest request);

    /// <summary>
    /// Ends the specified player's turn.
    /// </summary>
    CommandResult EndTurn(PlayerId playerId);
}
