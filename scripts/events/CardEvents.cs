using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Card zone transition information.
/// </summary>
public sealed class CardZoneChange
{
    /// <summary>
    /// Card instance being moved.
    /// </summary>
    public CardInstanceId CardInstanceId { get; init; }

    /// <summary>
    /// Previous zone.
    /// </summary>
    public CardZone From { get; init; }

    /// <summary>
    /// New zone.
    /// </summary>
    public CardZone To { get; init; }
}

/// <summary>
/// Event emitted when a round starts.
/// </summary>
public sealed class EvtRoundStarted : IGameEvent
{
    /// <inheritdoc />
    public int EventIndex { get; init; }

    /// <inheritdoc />
    public long Timestamp { get; init; }

    /// <summary>
    /// Started round number.
    /// </summary>
    public int RoundNumber { get; init; }
}

/// <summary>
/// Event emitted when discard pile is reshuffled into draw pile.
/// </summary>
public sealed class EvtDeckReshuffled : IGameEvent
{
    /// <inheritdoc />
    public int EventIndex { get; init; }

    /// <inheritdoc />
    public long Timestamp { get; init; }

    /// <summary>
    /// Player whose deck was reshuffled.
    /// </summary>
    public PlayerId PlayerId { get; init; }

    /// <summary>
    /// Number of cards moved from discard pile to draw pile.
    /// </summary>
    public int CardCount { get; init; }
}

/// <summary>
/// Event emitted when cards are drawn.
/// </summary>
public sealed class EvtCardsDrawn : IGameEvent
{
    /// <inheritdoc />
    public int EventIndex { get; init; }

    /// <inheritdoc />
    public long Timestamp { get; init; }

    /// <summary>
    /// Player who drew cards.
    /// </summary>
    public PlayerId PlayerId { get; init; }

    /// <summary>
    /// Drawn card ids in draw order.
    /// </summary>
    public IReadOnlyList<CardInstanceId> CardInstanceIds { get; init; } = [];
}

/// <summary>
/// Event emitted when one or more cards move between zones.
/// </summary>
public sealed class EvtCardsMoved : IGameEvent
{
    /// <inheritdoc />
    public int EventIndex { get; init; }

    /// <inheritdoc />
    public long Timestamp { get; init; }

    /// <summary>
    /// Player owning the moved cards.
    /// </summary>
    public PlayerId PlayerId { get; init; }

    /// <summary>
    /// Zone transitions performed by this action.
    /// </summary>
    public IReadOnlyList<CardZoneChange> Changes { get; init; } = [];
}

/// <summary>
/// Event emitted when a card is played.
/// </summary>
public sealed class EvtCardPlayed : IGameEvent
{
    /// <inheritdoc />
    public int EventIndex { get; init; }

    /// <inheritdoc />
    public long Timestamp { get; init; }

    /// <summary>
    /// Player who played the card.
    /// </summary>
    public PlayerId PlayerId { get; init; }

    /// <summary>
    /// Played card instance id.
    /// </summary>
    public CardInstanceId CardInstanceId { get; init; }

    /// <summary>
    /// Played card definition id.
    /// </summary>
    public CardId CardId { get; init; }

    /// <summary>
    /// Play mode used.
    /// </summary>
    public CardPlayMode Mode { get; init; }

    /// <summary>
    /// Effect summary produced by the card.
    /// </summary>
    public string EffectSummary { get; init; } = string.Empty;
}

/// <summary>
/// Event emitted when turn resource totals change.
/// </summary>
public sealed class EvtTurnResourcesChanged : IGameEvent
{
    /// <inheritdoc />
    public int EventIndex { get; init; }

    /// <inheritdoc />
    public long Timestamp { get; init; }

    /// <summary>
    /// Player whose resources changed.
    /// </summary>
    public PlayerId PlayerId { get; init; }

    /// <summary>
    /// Current movement total.
    /// </summary>
    public int Movement { get; init; }

    /// <summary>
    /// Current attack total.
    /// </summary>
    public int Attack { get; init; }

    /// <summary>
    /// Current block total.
    /// </summary>
    public int Block { get; init; }

    /// <summary>
    /// Current influence total.
    /// </summary>
    public int Influence { get; init; }

    /// <summary>
    /// Current healing total.
    /// </summary>
    public int Healing { get; init; }
}

/// <summary>
/// Event emitted when a player ends their turn.
/// </summary>
public sealed class EvtTurnEnded : IGameEvent
{
    /// <inheritdoc />
    public int EventIndex { get; init; }

    /// <inheritdoc />
    public long Timestamp { get; init; }

    /// <summary>
    /// Player whose turn ended.
    /// </summary>
    public PlayerId PlayerId { get; init; }

    /// <summary>
    /// Next active player id, if any.
    /// </summary>
    public PlayerId? NextPlayerId { get; init; }

    /// <summary>
    /// True when turn end completed the round.
    /// </summary>
    public bool RoundEnded { get; init; }
}

/// <summary>
/// Script-generated event for reputation changes.
/// </summary>
public sealed class EvtPlayerReputationChanged : IGameEvent
{
    /// <inheritdoc />
    public int EventIndex { get; init; }

    /// <inheritdoc />
    public long Timestamp { get; init; }

    /// <summary>
    /// Player whose reputation changed.
    /// </summary>
    public PlayerId PlayerId { get; init; }

    /// <summary>
    /// Reputation before change.
    /// </summary>
    public int PreviousReputation { get; init; }

    /// <summary>
    /// Reputation after change.
    /// </summary>
    public int CurrentReputation { get; init; }
}
