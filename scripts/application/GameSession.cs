using System;
using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Default application session that owns command routing and map orchestration.
/// </summary>
public sealed class GameSession : IGameSession
{
    private readonly CommandDispatcher _dispatcher;
    private readonly MapGenerator _mapGenerator;
    private readonly IEventIndexProvider _eventIndexes;
    private int _nextCommandSequence;

    /// <summary>
    /// Creates a new game session.
    /// </summary>
    public GameSession(ContentDatabase contentDatabase, ulong seed, IGameClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(contentDatabase);

        EventBus = new EventBus();
        _eventIndexes = new SequentialEventIndexProvider();
        IGameClock eventClock = clock ?? new DeterministicGameClock();
        var mapIdGenerator = new SessionMapIdGenerator();

        var rng = new DeterministicRandom(seed);
        _mapGenerator = new MapGenerator(contentDatabase, rng, mapIdGenerator);

        var router = new CommandRouter();
        router.Register(new InitializeMapCommandHandler(_mapGenerator, eventClock, _eventIndexes));
        router.Register(new ExploreTileCommandHandler(_mapGenerator, eventClock, _eventIndexes));

        _dispatcher = new CommandDispatcher(EventBus, router);
        _nextCommandSequence = 0;

        Log.Debug("GameSession created");
    }

    /// <inheritdoc/>
    public EventBus EventBus { get; }

    /// <inheritdoc/>
    public MapState MapState => _mapGenerator.Map;

    /// <inheritdoc/>
    public TilePlacementResult InitializeMap()
    {
        var command = new InitializeMapCommand
        {
            PlayerId = new PlayerId(0),
            SequenceNumber = NextSequence()
        };

        CommandResult result = _dispatcher.Dispatch(command);
        if (!result.IsSuccess)
        {
            string summary = result.GetErrorSummary();
            Log.Error($"GameSession: InitializeMap failed: {summary}");
            return TilePlacementResult.Invalid(summary);
        }

        return _mapGenerator.LastPlacementResult ?? TilePlacementResult.Invalid("Map initialization produced no placement result");
    }

    /// <inheritdoc/>
    public TilePlacementResult? ExploreTile(HexCoord macroCoord, TileCategory? category = null)
    {
        var command = new ExploreTileCommand
        {
            PlayerId = new PlayerId(0),
            SequenceNumber = NextSequence(),
            MacroCoord = macroCoord,
            Category = category
        };

        CommandResult result = _dispatcher.Dispatch(command);
        if (!result.IsSuccess)
        {
            string summary = result.GetErrorSummary();
            Log.Warning($"GameSession: ExploreTile failed: {summary}");
            return TilePlacementResult.Invalid(summary);
        }

        return _mapGenerator.LastPlacementResult;
    }

    /// <inheritdoc/>
    public IReadOnlyList<(HexCoord Position, int Direction)> GetExplorableEdges()
    {
        return _mapGenerator.GetExplorableEdges();
    }

    private int NextSequence()
    {
        return _nextCommandSequence++;
    }
}
