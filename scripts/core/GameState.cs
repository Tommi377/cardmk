using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Overall game phase.
/// </summary>
public enum GamePhase
{
    Setup,
    InProgress,
    Ended
}

/// <summary>
/// Phase of a round's turn structure.
/// </summary>
public enum TurnPhase
{
    RoundStart,
    PlayerAction,
    Combat,
    TurnEnd,
    RoundEnd
}

/// <summary>
/// Result of a completed game.
/// </summary>
public sealed class VictoryResult
{
    /// <summary>
    /// Whether the game was won.
    /// </summary>
    public bool IsVictory { get; init; }

    /// <summary>
    /// The winning player (if any).
    /// </summary>
    public PlayerId? WinnerId { get; init; }

    /// <summary>
    /// Final scores for all players.
    /// </summary>
    public IReadOnlyDictionary<PlayerId, int> FinalScores { get; init; }
        = new Dictionary<PlayerId, int>();

    /// <summary>
    /// Description of how the game ended.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// Root aggregate for the complete game state.
/// </summary>
public sealed class GameState
{
    private readonly List<PlayerState> _players;
    private readonly Dictionary<CardInstanceId, CardInstance> _cardInstances;
    // private readonly List<CardInstance> _offerRow;
    // private readonly List<CardInstance> _unitOffer;
    // private readonly List<CardInstance> _advancedActionPool;
    // private readonly List<CardInstance> _spellPool;
    // private readonly List<CardInstance> _artifactPool;
    private int _commandIndex;
    private int _nextCardInstanceId;

    /// <summary>
    /// Creates a new game state.
    /// </summary>
    public GameState(
        GameId id,
        ulong seed,
        // ScenarioId scenarioId,
        // GameSettings settings,
        IEnumerable<PlayerState> players,
        MapState? map = null)
    {
        Id = id;
        Seed = seed;
        // ScenarioId = scenarioId;
        // Settings = settings;
        _players = new List<PlayerState>(players);
        _cardInstances = new Dictionary<CardInstanceId, CardInstance>();

        Rng = new DeterministicRandom(seed);
        Map = map ?? new MapState();
        
        // TODO: Mana
        // ManaSource = new ManaSource(_players.Count);

        // TODO: Pools
        // _offerRow = new List<CardInstance>();
        // _unitOffer = new List<CardInstance>();
        // _advancedActionPool = new List<CardInstance>();
        // _spellPool = new List<CardInstance>();
        // _artifactPool = new List<CardInstance>();

        // TODO: Day Night Cycle
        // DayNight = DayNightPhase.Day;
        
        RoundNumber = 0;
        CurrentPhase = TurnPhase.RoundStart;
        ActivePlayerId = _players.FirstOrDefault()?.Id ?? new PlayerId(0);
        GamePhase = GamePhase.Setup;
        _commandIndex = 0;
        _nextCardInstanceId = 1;

        Log.Info($"GameState created: Id={id}, Seed={seed}, Scenario=NOT_IMPLEMENTED, Players={_players.Count}");
    }

    /// <summary>
    /// Unique identifier for this game.
    /// </summary>
    public GameId Id { get; }

    /// <summary>
    /// Seed used for deterministic random number generation.
    /// </summary>
    public ulong Seed { get; }

    /// <summary>
    /// The scenario being played.
    /// </summary>
    public int ScenarioId { get; }

    // TODO: Game Settings and Variants
    /// <summary>
    /// Game settings.
    /// </summary>
    // public GameSettings Settings { get; }

    /// <summary>
    /// Deterministic random number generator.
    /// </summary>
    public DeterministicRandom Rng { get; }

    /// <summary>
    /// Current round number (1-based).
    /// </summary>
    public int RoundNumber { get; private set; }

    // TODO: Day Night Cycle
    /// <summary>
    /// Current day/night phase.
    /// </summary>
    // public DayNightPhase DayNight { get; private set; }

    /// <summary>
    /// Current turn phase.
    /// </summary>
    public TurnPhase CurrentPhase { get; private set; }

    /// <summary>
    /// The currently active player.
    /// </summary>
    public PlayerId ActivePlayerId { get; private set; }

    /// <summary>
    /// The map state.
    /// </summary>
    public MapState Map { get; }

    // TODO: Mana
    /// <summary>
    /// The shared mana source (dice).
    /// </summary>
    // public ManaSource ManaSource { get; }

    /// <summary>
    /// All players in the game.
    /// </summary>
    public IReadOnlyList<PlayerState> Players => _players;

    /// <summary>
    /// All known card instances in this game.
    /// </summary>
    public IReadOnlyDictionary<CardInstanceId, CardInstance> CardInstances => _cardInstances;

    // TODO: Pools
    /// <summary>
    /// Cards available in the offer row.
    /// </summary>
    // public IReadOnlyList<CardInstance> OfferRow => _offerRow;

    /// <summary>
    /// Units available for recruitment.
    /// </summary>
    // public IReadOnlyList<CardInstance> UnitOffer => _unitOffer;

    /// <summary>
    /// Pool of advanced actions.
    /// </summary>
    // public IReadOnlyList<CardInstance> AdvancedActionPool => _advancedActionPool;

    /// <summary>
    /// Pool of spells.
    /// </summary>
    // public IReadOnlyList<CardInstance> SpellPool => _spellPool;

    /// <summary>
    /// Pool of artifacts.
    /// </summary>
    // public IReadOnlyList<CardInstance> ArtifactPool => _artifactPool;

    // TODO: Combat
    /// <summary>
    /// Active combat (if any).
    /// </summary>
    // public CombatState? ActiveCombat { get; private set; }

    /// <summary>
    /// Current command index for replay ordering.
    /// </summary>
    public int CommandIndex => _commandIndex;

    /// <summary>
    /// Overall game phase.
    /// </summary>
    public GamePhase GamePhase { get; private set; }

    /// <summary>
    /// Victory result (if game has ended).
    /// </summary>
    public VictoryResult? Victory { get; private set; }

    #region Player Access

    /// <summary>
    /// Gets a player by ID.
    /// </summary>
    /// <exception cref="InvalidOperationException">If player not found.</exception>
    public PlayerState GetPlayer(PlayerId id)
    {
        PlayerState? player = _players.FirstOrDefault(p => p.Id == id);
        if (player == null)
            throw new InvalidOperationException($"Player {id} not found");
        return player;
    }

    /// <summary>
    /// Tries to get a player by ID.
    /// </summary>
    public bool TryGetPlayer(PlayerId id, out PlayerState? player)
    {
        player = _players.FirstOrDefault(p => p.Id == id);
        return player != null;
    }

    /// <summary>
    /// Gets the currently active player.
    /// </summary>
    public PlayerState GetActivePlayer()
    {
        return GetPlayer(ActivePlayerId);
    }

    /// <summary>
    /// Gets all non-eliminated players.
    /// </summary>
    public IEnumerable<PlayerState> GetActivePlayers()
    {
        return _players.Where(p => !p.IsEliminated);
    }

    #endregion

    #region Card Instances

    /// <summary>
    /// Registers a card instance in the global game index.
    /// </summary>
    public void RegisterCardInstance(CardInstance card)
    {
        ArgumentNullException.ThrowIfNull(card);
        _cardInstances[card.Id] = card;
    }

    /// <summary>
    /// Creates and registers a new card instance owned by the specified player.
    /// </summary>
    public CardInstance CreateCardInstance(PlayerId ownerId, CardId definitionId, CardZone zone = CardZone.DrawPile)
    {
        var id = new CardInstanceId(_nextCardInstanceId++);
        var card = new CardInstance(id, definitionId, ownerId, zone);
        RegisterCardInstance(card);
        return card;
    }

    /// <summary>
    /// Tries to get a card instance by id.
    /// </summary>
    public bool TryGetCardInstance(CardInstanceId id, out CardInstance? card)
    {
        return _cardInstances.TryGetValue(id, out card);
    }

    /// <summary>
    /// Gets a card instance by id.
    /// </summary>
    public CardInstance GetCardInstance(CardInstanceId id)
    {
        if (!_cardInstances.TryGetValue(id, out CardInstance? card))
        {
            throw new InvalidOperationException($"Card instance {id} not found");
        }

        return card;
    }

    #endregion

    #region State Transitions

    /// <summary>
    /// Advances to the next round.
    /// </summary>
    public void AdvanceRound()
    {
        RoundNumber++;

        // TODO: Day Night Cycle
        // Update day/night cycle
        // int cycleLength = Settings.DayRoundsPerCycle + Settings.NightRoundsPerCycle;
        // int positionInCycle = (RoundNumber - 1) % cycleLength;
        //
        // if (positionInCycle < Settings.DayRoundsPerCycle)
        // {
        //     if (DayNight != DayNightPhase.Day)
        //     {
        //         DayNight = DayNightPhase.Day;
        //         ManaSource.DisableBlackMana();
        //         LoggerProvider.Current.Info("Day phase begins (Round {0})", RoundNumber);
        //     }
        // }
        // else
        // {
        //     if (DayNight != DayNightPhase.Night)
        //     {
        //         DayNight = DayNightPhase.Night;
        //         ManaSource.EnableBlackMana();
        //         LoggerProvider.Current.Info("Night phase begins (Round {0})", RoundNumber);
        //     }
        // }

        // TODO: Mana
        // Reroll mana source
        // ManaSource.Reroll(Rng);

        CurrentPhase = TurnPhase.RoundStart;
        Log.Info($"Round {RoundNumber} started");
    }

    /// <summary>
    /// Sets the current turn phase.
    /// </summary>
    public void SetPhase(TurnPhase phase)
    {
        TurnPhase oldPhase = CurrentPhase;
        CurrentPhase = phase;
        Log.Debug($"Phase transition: {oldPhase} → {phase}");
    }

    /// <summary>
    /// Sets the active player.
    /// </summary>
    public void SetActivePlayer(PlayerId playerId)
    {
        if (!TryGetPlayer(playerId, out _))
            throw new InvalidOperationException($"Player {playerId} not found");

        PlayerId oldPlayer = ActivePlayerId;
        ActivePlayerId = playerId;
        Log.Debug($"Active player changed: {oldPlayer} → {playerId}");
    }

    /// <summary>
    /// Advances to the next player in turn order.
    /// </summary>
    /// <returns>True if moved to next player, false if round should end.</returns>
    public bool AdvanceToNextPlayer()
    {
        var activePlayers = GetActivePlayers().ToList();
        if (activePlayers.Count == 0) return false;

        int currentIndex = activePlayers.FindIndex(p => p.Id == ActivePlayerId);
        int nextIndex = (currentIndex + 1) % activePlayers.Count;

        // If we've wrapped around, round should end
        if (nextIndex <= currentIndex)
        {
            return false;
        }

        SetActivePlayer(activePlayers[nextIndex].Id);
        return true;
    }

    // TODO: Day Night Cycle
    /// <summary>
    /// Toggles between Day and Night phases.
    /// </summary>
    // public void ToggleDayNight()
    // {
    //     if (DayNight == DayNightPhase.Day)
    //     {
    //         DayNight = DayNightPhase.Night;
    //         ManaSource.EnableBlackMana();
    //         LoggerProvider.Current.Info("Toggled to Night phase");
    //     }
    //     else
    //     {
    //         DayNight = DayNightPhase.Day;
    //         ManaSource.DisableBlackMana();
    //         LoggerProvider.Current.Info("Toggled to Day phase");
    //     }
    // }

    /// <summary>
    /// Starts the game.
    /// </summary>
    public void Start()
    {
        if (GamePhase != GamePhase.Setup)
            throw new InvalidOperationException("Game has already started");

        GamePhase = GamePhase.InProgress;
        AdvanceRound();
        Log.Info("Game started");
    }

    /// <summary>
    /// Ends the game with a victory result.
    /// </summary>
    public void End(VictoryResult result)
    {
        GamePhase = GamePhase.Ended;
        Victory = result;
        Log.Info($"Game ended: {result.Description}");
    }

    #endregion

    #region Combat Management

    /// TODO: Combat
    /// <summary>
    /// Starts combat at the specified location.
    /// </summary>
    // public void StartCombat(PlayerId attackerId, HexCoord location, IEnumerable<EnemyDefinition> enemies)
    // {
    //     var enemyInstances = enemies.Select(EnemyInstance.FromDefinition).ToList();
    //     ActiveCombat = new CombatState
    //     {
    //         AttackingPlayerId = attackerId,
    //         Location = location,
    //         Enemies = enemyInstances
    //     };
    //     SetPhase(TurnPhase.Combat);
    //     Log.Info($"Combat started at {{location}} with {enemyInstances.Count} enemies");
    // }

    /// <summary>
    /// Starts combat at the specified location with pre-created enemy instances.
    /// </summary>
    // public void StartCombat(PlayerId attackerId, HexCoord location, IReadOnlyList<EnemyInstance> enemyInstances)
    // {
    //     ActiveCombat = new CombatState
    //     {
    //         AttackingPlayerId = attackerId,
    //         Location = location,
    //         Enemies = enemyInstances
    //     };
    //     SetPhase(TurnPhase.Combat);
    //     Log.Info($"Combat started at {location} with {enemyInstances.Count} enemies");
    // }

    /// <summary>
    /// Ends the current combat.
    /// </summary>
    // public void EndCombat()
    // {
    //     ActiveCombat = null;
    //     SetPhase(TurnPhase.PlayerAction);
    //     Log.Info("Combat ended");
    // }

    #endregion

    #region Pool Management

    // TODO: Pool management
    /// <summary>
    /// Adds a card to the offer row.
    /// </summary>
    // public void AddToOfferRow(CardInstance card)
    // {
    //     _offerRow.Add(card);
    // }

    /// <summary>
    /// Removes a card from the offer row.
    /// </summary>
    // public bool RemoveFromOfferRow(CardInstance card)
    // {
    //     return _offerRow.Remove(card);
    // }

    // TODO: Units
    /// <summary>
    /// Adds a card to the unit offer.
    /// </summary>
    // public void AddToUnitOffer(CardInstance card)
    // {
    //     _unitOffer.Add(card);
    // }

    /// <summary>
    /// Removes a card from the unit offer.
    /// </summary>
    // public bool RemoveFromUnitOffer(CardInstance card)
    // {
    //     return _unitOffer.Remove(card);
    // }

    // TODO: Action Pool
    /// <summary>
    /// Initializes the advanced action pool.
    /// </summary>
    // public void InitializeAdvancedActionPool(IEnumerable<CardInstance> cards)
    // {
    //     _advancedActionPool.Clear();
    //     _advancedActionPool.AddRange(cards);
    // }

    // TODO: Spells 
    /// <summary>
    /// Initializes the spell pool.
    /// </summary>
    // public void InitializeSpellPool(IEnumerable<CardInstance> cards)
    // {
    //     _spellPool.Clear();
    //     _spellPool.AddRange(cards);
    // }

    // TODO: Artifacts
    /// <summary>
    /// Initializes the artifact pool.
    /// </summary>
    // public void InitializeArtifactPool(IEnumerable<CardInstance> cards)
    // {
    //     _artifactPool.Clear();
    //     _artifactPool.AddRange(cards);
    // }

    #endregion

    #region Command Tracking

    /// <summary>
    /// Gets and increments the next event/command index.
    /// </summary>
    public int NextEventIndex()
    {
        return _commandIndex++;
    }

    /// <summary>
    /// Resets the command index (for testing/replay).
    /// </summary>
    public void ResetCommandIndex()
    {
        _commandIndex = 0;
    }

    #endregion

    /// <summary>
    /// Checks if the game has ended.
    /// </summary>
    public bool IsGameOver => GamePhase == GamePhase.Ended;

    /// <summary>
    /// Checks if the game is in progress.
    /// </summary>
    public bool IsInProgress => GamePhase == GamePhase.InProgress;

    // TODO: Day Night Cycle
    /// <summary>
    /// Checks if it's currently night.
    /// </summary>
    // public bool IsNight => DayNight == DayNightPhase.Night;
}

# region Combat

/// <summary>
/// Active combat state (when in combat).
/// </summary>
// public sealed class CombatState
// {
//     /// <summary>
//     /// The player engaged in combat.
//     /// </summary>
//     public PlayerId AttackingPlayerId { get; init; }
//
//     /// <summary>
//     /// Position where combat is occurring.
//     /// </summary>
//     public HexCoord Location { get; init; }
//
//     /// <summary>
//     /// Enemies involved in the combat.
//     /// </summary>
//     public IReadOnlyList<EnemyInstance> Enemies { get; init; } = [];
//
//     /// <summary>
//     /// Current phase of combat.
//     /// </summary>
//     public CombatPhase Phase { get; set; } = CombatPhase.RangedAttack;
//
//     /// <summary>
//     /// Block accumulated for this combat.
//     /// </summary>
//     public int AccumulatedBlock { get; set; }
//
//     /// <summary>
//     /// Attack accumulated for this combat.
//     /// </summary>
//     public int AccumulatedAttack { get; set; }
// }

// TODO: Combat
/// <summary>
/// Enemy instance in combat.
/// </summary>
// public sealed class EnemyInstance
// {
//     /// <summary>
//     /// The enemy definition.
//     /// </summary>
//     public required EnemyDefinition Definition { get; init; }
//
//     /// <summary>
//     /// Current health of the enemy.
//     /// </summary>
//     public int CurrentHealth { get; set; }
//
//     /// <summary>
//     /// Whether this enemy has been defeated.
//     /// </summary>
//     public bool IsDefeated => CurrentHealth <= 0;
//
//     /// <summary>
//     /// Creates an enemy instance from a definition.
//     /// </summary>
//     public static EnemyInstance FromDefinition(EnemyDefinition definition)
//     {
//         return new EnemyInstance
//         {
//             Definition = definition,
//             CurrentHealth = definition.Armor
//         };
//     }
// }

/// <summary>
/// Combat phase.
/// </summary>
public enum CombatPhase
{
    RangedAttack,
    Block,
    AssignDamage,
    Attack,
    Resolution
}

# endregion
