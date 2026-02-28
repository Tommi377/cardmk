using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Represents a player's complete state during a game.
/// </summary>
public sealed class PlayerState
{
    private readonly List<CardInstance> _drawPile;
    private readonly List<CardInstance> _hand;
    private readonly List<CardInstance> _playArea;
    private readonly List<CardInstance> _discardPile;

    // TODO: Units, Skills, Crystals management
    // private readonly List<UnitInstance> _units;
    // private readonly List<SkillId> _skills;
    // private readonly Dictionary<ManaType, int> _crystals;

    /// <summary>
    /// Creates a new player state.
    /// </summary>
    public PlayerState(PlayerId id, HeroId heroId, HexCoord startPosition)
    {
        Id = id;
        HeroId = heroId;
        Position = startPosition;
        _drawPile = new List<CardInstance>();
        _hand = new List<CardInstance>();
        _playArea = new List<CardInstance>();
        _discardPile = new List<CardInstance>();
        TurnResources = new TurnResourcePool();

        // Default starting values
        Level = 1;
        Fame = 0;
        Reputation = 0;
        Armor = 2;
        HandLimit = 5;
        UnitSlots = 1;
        IsEliminated = false;
        InfluenceBonus = 0;

        Log.Debug($"PlayerState created for player {id} with hero {heroId}");
    }

    /// <summary>
    /// Unique identifier for this player.
    /// </summary>
    public PlayerId Id { get; }

    /// <summary>
    /// The hero this player is using.
    /// </summary>
    public HeroId HeroId { get; }

    /// <summary>
    /// Current position on the map.
    /// </summary>
    public HexCoord Position { get; set; }

    /// <summary>
    /// Cards currently in draw pile (top of deck is at the end of this list).
    /// </summary>
    public IReadOnlyList<CardInstance> DrawPile => _drawPile;

    /// <summary>
    /// Cards currently in hand.
    /// </summary>
    public IReadOnlyList<CardInstance> Hand => _hand;

    /// <summary>
    /// Cards currently in the play area.
    /// </summary>
    public IReadOnlyList<CardInstance> PlayArea => _playArea;

    /// <summary>
    /// Cards currently in discard pile.
    /// </summary>
    public IReadOnlyList<CardInstance> DiscardPile => _discardPile;

    // TODO: Units, Skills, Crystals management
    /// <summary>
    /// Units the player controls.
    /// </summary>
    // public IReadOnlyList<UnitInstance> Units => _units;

    /// <summary>
    /// Skills the player has acquired.
    /// </summary>
    // public IReadOnlyList<SkillId> Skills => _skills;

    /// <summary>
    /// Crystals the player has stored (by mana type).
    /// </summary>
    // public IReadOnlyDictionary<ManaType, int> Crystals => _crystals;

    /// <summary>
    /// Player's current level (1-10).
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Fame points accumulated.
    /// </summary>
    public int Fame { get; set; }

    /// <summary>
    /// Reputation value (-7 to +7).
    /// </summary>
    public int Reputation { get; set; }

    /// <summary>
    /// Base armor value.
    /// </summary>
    public int Armor { get; set; }

    /// <summary>
    /// Maximum hand size.
    /// </summary>
    public int HandLimit { get; set; }

    /// <summary>
    /// Number of unit slots available.
    /// </summary>
    public int UnitSlots { get; set; }

    /// <summary>
    /// Whether this player has been eliminated from the game.
    /// </summary>
    public bool IsEliminated { get; set; }

    /// <summary>
    /// Bonus to all influence actions.
    /// </summary>
    public int InfluenceBonus { get; set; }

    /// <summary>
    /// Resource pool accumulated by played cards this turn.
    /// </summary>
    public TurnResourcePool TurnResources { get; }

    /// <summary>
    /// Whether the player has attacked this turn.
    /// </summary>
    public bool HasAttackedThisTurn { get; set; }

    /// <summary>
    /// Whether the player has taken mana from the source this turn.
    /// </summary>
    public bool HasTakenManaThisTurn { get; private set; }

    /// <summary>
    /// Marks that the player has taken mana this turn.
    /// </summary>
    public void MarkManaTaken()
    {
        HasTakenManaThisTurn = true;
        Log.Debug($"Player {Id}: Marked mana taken this turn");
    }

    #region Deck and Hand Management

    /// <summary>
    /// Clears and initializes draw pile with provided cards.
    /// </summary>
    public void InitializeDrawPile(IEnumerable<CardInstance> cards)
    {
        ArgumentNullException.ThrowIfNull(cards);

        _drawPile.Clear();
        _hand.Clear();
        _playArea.Clear();
        _discardPile.Clear();

        foreach (CardInstance card in cards)
        {
            card.Zone = CardZone.DrawPile;
            _drawPile.Add(card);
        }

        Log.Debug($"Player {Id}: Initialized draw pile with {_drawPile.Count} cards");
    }

    /// <summary>
    /// Adds a card to draw pile.
    /// </summary>
    public void AddToDrawPile(CardInstance card)
    {
        ArgumentNullException.ThrowIfNull(card);
        card.Zone = CardZone.DrawPile;
        _drawPile.Add(card);
    }

    /// <summary>
    /// Draws one card from draw pile into hand.
    /// </summary>
    public CardInstance? DrawOne()
    {
        if (_drawPile.Count == 0)
        {
            return null;
        }

        int topIndex = _drawPile.Count - 1;
        CardInstance drawn = _drawPile[topIndex];
        _drawPile.RemoveAt(topIndex);
        drawn.Zone = CardZone.Hand;
        _hand.Add(drawn);
        return drawn;
    }

    /// <summary>
    /// Draws up to <paramref name="count"/> cards from draw pile into hand.
    /// </summary>
    public IReadOnlyList<CardInstance> DrawCards(int count)
    {
        if (count <= 0)
        {
            return [];
        }

        var drawn = new List<CardInstance>(count);
        for (int i = 0; i < count; i++)
        {
            CardInstance? card = DrawOne();
            if (card == null)
            {
                break;
            }

            drawn.Add(card);
        }

        return drawn;
    }

    /// <summary>
    /// Tries to find a card in hand by instance id.
    /// </summary>
    public bool TryGetCardInHand(CardInstanceId cardInstanceId, out CardInstance? card)
    {
        card = _hand.FirstOrDefault(c => c.Id == cardInstanceId);
        return card != null;
    }

    /// <summary>
    /// Moves a card from hand to play area.
    /// </summary>
    public bool PlayCard(CardInstance card)
    {
        ArgumentNullException.ThrowIfNull(card);
        if (!_hand.Remove(card))
        {
            return false;
        }

        card.Zone = CardZone.PlayArea;
        _playArea.Add(card);
        Log.Debug($"Player {Id}: Played card {card.Id}");
        return true;
    }

    /// <summary>
    /// Removes a card from play area and discards it.
    /// </summary>
    public bool DiscardFromPlayArea(CardInstance card)
    {
        ArgumentNullException.ThrowIfNull(card);
        if (!_playArea.Remove(card))
        {
            return false;
        }

        card.Zone = CardZone.DiscardPile;
        _discardPile.Add(card);
        return true;
    }

    /// <summary>
    /// Moves all hand and play area cards to discard pile.
    /// </summary>
    public IReadOnlyList<CardZoneChange> DiscardHandAndPlayArea()
    {
        var moved = new List<CardZoneChange>(_hand.Count + _playArea.Count);

        foreach (CardInstance card in _hand)
        {
            card.Zone = CardZone.DiscardPile;
            _discardPile.Add(card);
            moved.Add(new CardZoneChange
            {
                CardInstanceId = card.Id,
                From = CardZone.Hand,
                To = CardZone.DiscardPile
            });
        }

        foreach (CardInstance card in _playArea)
        {
            card.Zone = CardZone.DiscardPile;
            _discardPile.Add(card);
            moved.Add(new CardZoneChange
            {
                CardInstanceId = card.Id,
                From = CardZone.PlayArea,
                To = CardZone.DiscardPile
            });
        }

        _hand.Clear();
        _playArea.Clear();
        return moved;
    }

    /// <summary>
    /// Shuffles discard pile into draw pile.
    /// </summary>
    /// <returns>Number of cards moved from discard to draw pile.</returns>
    public int ReshuffleDiscardIntoDraw(DeterministicRandom rng)
    {
        ArgumentNullException.ThrowIfNull(rng);
        if (_discardPile.Count == 0)
        {
            return 0;
        }

        foreach (CardInstance card in _discardPile)
        {
            card.Zone = CardZone.DrawPile;
            _drawPile.Add(card);
        }

        _discardPile.Clear();
        rng.Shuffle(_drawPile);
        return _drawPile.Count;
    }

    /// <summary>
    /// Adds a card directly into hand.
    /// </summary>
    public void AddToHand(CardInstance card)
    {
        ArgumentNullException.ThrowIfNull(card);
        card.Zone = CardZone.Hand;
        _hand.Add(card);
        Log.Debug($"Player {Id}: Added card {card.Id} to hand");
    }

    /// <summary>
    /// Removes a card from hand.
    /// </summary>
    public bool RemoveFromHand(CardInstance card)
    {
        ArgumentNullException.ThrowIfNull(card);
        bool removed = _hand.Remove(card);
        if (removed)
        {
            Log.Debug($"Player {Id}: Removed card {card.Id} from hand");
        }
        return removed;
    }

    /// <summary>
    /// Removes a specific card from play area.
    /// </summary>
    public bool RemoveFromPlayArea(CardInstance card)
    {
        ArgumentNullException.ThrowIfNull(card);
        bool removed = _playArea.Remove(card);
        if (removed)
        {
            Log.Debug($"Player {Id}: Removed card {card.Id} from play area");
        }
        return removed;
    }

    /// <summary>
    /// Discards all cards from play area to discard pile.
    /// </summary>
    public void CleanUpPlayArea()
    {
        foreach (CardInstance card in _playArea)
        {
            card.Zone = CardZone.DiscardPile;
            _discardPile.Add(card);
        }

        _playArea.Clear();
        Log.Debug($"Player {Id}: Cleaned up play area");
    }

    /// <summary>
    /// Discards the entire hand to discard pile.
    /// </summary>
    public void DiscardHand()
    {
        foreach (CardInstance card in _hand)
        {
            card.Zone = CardZone.DiscardPile;
            _discardPile.Add(card);
        }

        _hand.Clear();
        Log.Debug($"Player {Id}: Discarded entire hand");
    }

    #endregion

    #region Turn Management

    /// <summary>
    /// Resets turn-based state at the start of a new turn.
    /// </summary>
    public void StartTurn()
    {
        HasAttackedThisTurn = false;
        HasTakenManaThisTurn = false;
        TurnResources.Clear();
        Log.Debug($"Player {Id}: Turn started");
    }

    /// <summary>
    /// Performs end-of-turn cleanup.
    /// </summary>
    public void EndTurn()
    {
        DiscardHandAndPlayArea();
        TurnResources.Clear();
        Log.Debug($"Player {Id}: Turn ended");
    }

    #endregion

    /// <summary>
    /// Calculates the effective influence for interactions.
    /// </summary>
    public int CalculateInfluence(int baseInfluence)
    {
        return baseInfluence + InfluenceBonus + Reputation;
    }
}
