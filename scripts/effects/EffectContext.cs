using System;
using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Runtime context used while resolving a card effect.
/// </summary>
public sealed class EffectContext
{
    private readonly List<IGameEvent> _scriptEvents = new();

    /// <summary>
    /// Creates a new effect context.
    /// </summary>
    public EffectContext(
        GameState gameState,
        PlayerState player,
        CardDefinition card,
        CardPlayMode mode,
        CardResolutionInput resolutionInput,
        Func<int> nextEventIndex,
        Func<long> nowTicks)
    {
        GameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Card = card ?? throw new ArgumentNullException(nameof(card));
        Mode = mode;
        ResolutionInput = resolutionInput ?? throw new ArgumentNullException(nameof(resolutionInput));
        NextEventIndex = nextEventIndex ?? throw new ArgumentNullException(nameof(nextEventIndex));
        NowTicks = nowTicks ?? throw new ArgumentNullException(nameof(nowTicks));
    }

    /// <summary>
    /// Current game state.
    /// </summary>
    public GameState GameState { get; }

    /// <summary>
    /// Current player resolving the effect.
    /// </summary>
    public PlayerState Player { get; }

    /// <summary>
    /// Card being resolved.
    /// </summary>
    public CardDefinition Card { get; }

    /// <summary>
    /// Card play mode being resolved.
    /// </summary>
    public CardPlayMode Mode { get; }

    /// <summary>
    /// Deterministic resolution input payload.
    /// </summary>
    public CardResolutionInput ResolutionInput { get; }

    /// <summary>
    /// Function that returns the next event index.
    /// </summary>
    public Func<int> NextEventIndex { get; }

    /// <summary>
    /// Function returning current logical timestamp.
    /// </summary>
    public Func<long> NowTicks { get; }

    /// <summary>
    /// Script-produced events collected during effect application.
    /// </summary>
    public IReadOnlyList<IGameEvent> ScriptEvents => _scriptEvents;

    /// <summary>
    /// Adds movement points to the player's turn resource pool.
    /// </summary>
    public void AddMovement(int value)
    {
        Player.TurnResources.Movement += value;
    }

    /// <summary>
    /// Adds attack points to the player's turn resource pool.
    /// </summary>
    public void AddAttack(int value)
    {
        Player.TurnResources.Attack += value;
    }

    /// <summary>
    /// Adds block points to the player's turn resource pool.
    /// </summary>
    public void AddBlock(int value)
    {
        Player.TurnResources.Block += value;
    }

    /// <summary>
    /// Adds influence points to the player's turn resource pool.
    /// </summary>
    public void AddInfluence(int value)
    {
        Player.TurnResources.Influence += value;
    }

    /// <summary>
    /// Adds healing points to the player's turn resource pool.
    /// </summary>
    public void AddHealing(int value)
    {
        Player.TurnResources.Healing += value;
    }

    /// <summary>
    /// Modifies player reputation.
    /// </summary>
    public void AddReputation(int delta)
    {
        Player.Reputation += delta;
    }

    /// <summary>
    /// Appends a script-generated event.
    /// </summary>
    public void AddScriptEvent(IGameEvent gameEvent)
    {
        ArgumentNullException.ThrowIfNull(gameEvent);
        _scriptEvents.Add(gameEvent);
    }
}
