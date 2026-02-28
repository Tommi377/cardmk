using System;
using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Default application session that owns command routing and deterministic game orchestration.
/// </summary>
public sealed class GameSession : IGameSession
{
    private readonly CommandDispatcher _dispatcher;
    private readonly MapGenerator _mapGenerator;
    private readonly IEventIndexProvider _eventIndexes;
    private readonly ContentDatabase _content;
    private int _nextCommandSequence;

    /// <summary>
    /// Creates a new game session with default player options.
    /// </summary>
    public GameSession(ContentDatabase contentDatabase, ulong seed, IGameClock? clock = null)
        : this(contentDatabase, seed, CreateDefaultOptions(), clock)
    {
    }

    /// <summary>
    /// Creates a new game session with explicit setup options.
    /// </summary>
    public GameSession(ContentDatabase contentDatabase, ulong seed, GameSessionOptions options, IGameClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(contentDatabase);
        ArgumentNullException.ThrowIfNull(options);

        _content = contentDatabase;
        EventBus = new EventBus();
        _eventIndexes = new SequentialEventIndexProvider();
        IGameClock eventClock = clock ?? new DeterministicGameClock();
        var mapIdGenerator = new SessionMapIdGenerator();

        var rng = new DeterministicRandom(seed);
        _mapGenerator = new MapGenerator(contentDatabase, rng, mapIdGenerator);
        State = new GameState(
            id: new GameId($"game-{seed}"),
            seed: seed,
            players: BuildPlayers(options),
            map: _mapGenerator.Map);

        InitializePlayerDecks();

        var router = new CommandRouter();
        router.Register(new InitializeMapCommandHandler(_mapGenerator, eventClock, _eventIndexes));
        router.Register(new ExploreTileCommandHandler(_mapGenerator, eventClock, _eventIndexes));
        router.Register(new StartRoundCommandHandler(State, eventClock, _eventIndexes));
        router.Register(new DrawCardsCommandHandler(State, eventClock, _eventIndexes));
        router.Register(new PlayCardCommandHandler(State, _content, eventClock, _eventIndexes));
        router.Register(new EndTurnCommandHandler(State, eventClock, _eventIndexes));

        _dispatcher = new CommandDispatcher(EventBus, router);
        _nextCommandSequence = 0;

        Log.Debug("GameSession created");
    }

    /// <inheritdoc />
    public EventBus EventBus { get; }

    /// <inheritdoc />
    public GameState State { get; }

    /// <inheritdoc />
    public MapState MapState => _mapGenerator.Map;

    /// <inheritdoc />
    public ValidationResult Validate(IGameCommand command)
    {
        return _dispatcher.Validate(command);
    }

    /// <inheritdoc />
    public CommandResult Dispatch(IGameCommand command)
    {
        return _dispatcher.Dispatch(command);
    }

    /// <inheritdoc />
    public TilePlacementResult InitializeMap()
    {
        var command = new InitializeMapCommand
        {
            PlayerId = new PlayerId(0),
            SequenceNumber = NextSequence()
        };

        CommandResult result = Dispatch(command);
        if (!result.IsSuccess)
        {
            string summary = result.GetErrorSummary();
            Log.Error($"GameSession: InitializeMap failed: {summary}");
            return TilePlacementResult.Invalid(summary);
        }

        return _mapGenerator.LastPlacementResult ?? TilePlacementResult.Invalid("Map initialization produced no placement result");
    }

    /// <inheritdoc />
    public TilePlacementResult? ExploreTile(HexCoord macroCoord, TileCategory? category = null)
    {
        var command = new ExploreTileCommand
        {
            PlayerId = new PlayerId(0),
            SequenceNumber = NextSequence(),
            MacroCoord = macroCoord,
            Category = category
        };

        CommandResult result = Dispatch(command);
        if (!result.IsSuccess)
        {
            string summary = result.GetErrorSummary();
            Log.Warning($"GameSession: ExploreTile failed: {summary}");
            return TilePlacementResult.Invalid(summary);
        }

        return _mapGenerator.LastPlacementResult;
    }

    /// <inheritdoc />
    public IReadOnlyList<(HexCoord Position, int Direction)> GetExplorableEdges()
    {
        return _mapGenerator.GetExplorableEdges();
    }

    /// <inheritdoc />
    public CommandResult StartRound()
    {
        return Dispatch(new StartRoundCommand
        {
            PlayerId = State.ActivePlayerId,
            SequenceNumber = NextSequence()
        });
    }

    /// <inheritdoc />
    public CommandResult DrawCards(PlayerId playerId, int count)
    {
        return Dispatch(new DrawCardsCommand
        {
            PlayerId = playerId,
            SequenceNumber = NextSequence(),
            Count = count
        });
    }

    /// <inheritdoc />
    public CommandResult PlayCard(PlayCardRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Dispatch(new PlayCardCommand
        {
            PlayerId = request.PlayerId,
            SequenceNumber = NextSequence(),
            CardInstanceId = request.CardInstanceId,
            Mode = request.Mode,
            ResolutionInput = request.ResolutionInput
        });
    }

    /// <inheritdoc />
    public CommandResult EndTurn(PlayerId playerId)
    {
        return Dispatch(new EndTurnCommand
        {
            PlayerId = playerId,
            SequenceNumber = NextSequence()
        });
    }

    private static GameSessionOptions CreateDefaultOptions()
    {
        return new GameSessionOptions
        {
            Players = new[]
            {
                new PlayerSetup
                {
                    PlayerId = new PlayerId(0),
                    HeroId = new HeroId("hero.default"),
                    StartPosition = new HexCoord(0, 0)
                }
            }
        };
    }

    private static IReadOnlyList<PlayerState> BuildPlayers(GameSessionOptions options)
    {
        if (options.Players.Count == 0)
        {
            throw new InvalidOperationException("GameSessionOptions must contain at least one player");
        }

        var players = new List<PlayerState>(options.Players.Count);
        foreach (PlayerSetup setup in options.Players)
        {
            players.Add(new PlayerState(setup.PlayerId, setup.HeroId, setup.StartPosition));
        }

        return players;
    }

    private void InitializePlayerDecks()
    {
        foreach (PlayerState player in State.Players)
        {
            if (!_content.TryGetStarterDeck(player.HeroId, out StarterDeckDefinition? starterDeck) || starterDeck == null)
            {
                throw new InvalidOperationException($"Missing starter deck definition for hero '{player.HeroId}'");
            }

            var cards = new List<CardInstance>();
            foreach (StarterDeckEntry entry in starterDeck.Entries)
            {
                for (int i = 0; i < entry.Count; i++)
                {
                    cards.Add(State.CreateCardInstance(player.Id, entry.CardId, CardZone.DrawPile));
                }
            }

            State.Rng.Shuffle(cards);
            player.InitializeDrawPile(cards);
        }
    }

    private int NextSequence()
    {
        return _nextCommandSequence++;
    }
}
