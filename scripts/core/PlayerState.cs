using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Represents a player's complete state during a game.
/// </summary>
public sealed class PlayerState
{
    private readonly List<CardInstance> _hand;
    private readonly List<CardInstance> _playArea;
    
    // TODO: Units, Skills, Crystals management
    // private readonly List<UnitInstance> _units;
    // private readonly List<SkillId> _skills;
    // private readonly Dictionary<ManaType, int> _crystals;

    /// <summary>
    /// Creates a new player state.
    /// </summary>
    public PlayerState(PlayerId id, string heroId, HexCoord startPosition)
    {
        Id = id;
        HeroId = heroId;
        Position = startPosition;
        // TODO: Deck
        // Deck = new DeckState();
        _hand = new List<CardInstance>();
        _playArea = new List<CardInstance>();
        
        // TODO: Units, Skills, Crystals management
        // _units = new List<UnitInstance>();
        // _skills = new List<SkillId>();
        // _crystals = new Dictionary<ManaType, int>();
        // PersonalMana = new ManaPool();

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
    public string HeroId { get; }

    /// <summary>
    /// Current position on the map.
    /// </summary>
    public HexCoord Position { get; set; }

    // TODO: Deck
    /// <summary>
    /// The player's deck state.
    /// </summary>
    // public DeckState Deck { get; }

    /// <summary>
    /// Cards currently in hand.
    /// </summary>
    public IReadOnlyList<CardInstance> Hand => _hand;

    /// <summary>
    /// Cards currently in the play area (being used this turn).
    /// </summary>
    public IReadOnlyList<CardInstance> PlayArea => _playArea;

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
    /// Personal mana pool available for use.
    /// </summary>
    // public ManaPool PersonalMana { get; }

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

    // TODO: Deck
    /// <summary>
    /// Number of wounds the player has (computed from deck).
    /// </summary>
    // public int WoundCount => Deck.WoundCount;

    /// <summary>
    /// Movement points available this turn.
    /// </summary>
    public int MovementPoints { get; set; }

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

    #region Hand Management

    /// <summary>
    /// Adds a card to the player's hand.
    /// </summary>
    public void AddToHand(CardInstance card)
    {
        _hand.Add(card);
        Log.Debug($"Player {Id}: Added card {card.Id} to hand");
    }

    /// <summary>
    /// Removes a card from the player's hand.
    /// </summary>
    /// <returns>True if the card was in hand and removed.</returns>
    public bool RemoveFromHand(CardInstance card)
    {
        bool removed = _hand.Remove(card);
        if (removed)
            Log.Debug($"Player {Id}: Removed card {card.Id} from hand");
        return removed;
    }

    /// <summary>
    /// Moves a card from hand to play area.
    /// </summary>
    public bool PlayCard(CardInstance card)
    {
        if (!_hand.Remove(card))
            return false;

        _playArea.Add(card);
        Log.Debug($"Player {Id}: Played card {card.Id}");
        return true;
    }

    /// <summary>
    /// Discards all cards from play area to discard pile.
    /// </summary>
    public void CleanUpPlayArea()
    {
        foreach (CardInstance card in _playArea)
        {
            // TODO: Deck
            // Deck.Discard(card);
        }
        _playArea.Clear();
        Log.Debug($"Player {Id}: Cleaned up play area");
    }

    /// <summary>
    /// Removes a specific card from the play area.
    /// </summary>
    /// <returns>True if the card was in the play area and removed.</returns>
    public bool RemoveFromPlayArea(CardInstance card)
    {
        bool removed = _playArea.Remove(card);
        if (removed)
            Log.Debug($"Player {Id}: Removed card {card.Id} from play area");
        return removed;
    }

    /// <summary>
    /// Discards the entire hand to the discard pile.
    /// </summary>
    public void DiscardHand()
    {
        foreach (CardInstance card in _hand)
        {
            // TODO: Deck
            // Deck.Discard(card);
        }
        _hand.Clear();
        Log.Debug($"Player {Id}: Discarded entire hand");
    }

    #endregion

    #region Unit Management

    // TODO: Unit
    /// <summary>
    /// Recruits a unit.
    /// </summary>
    // public bool RecruitUnit(UnitInstance unit)
    // {
    //     if (_units.Count >= UnitSlots)
    //     {
    //         LoggerProvider.Current.Warning("Player {0}: Cannot recruit unit, no slots available", Id);
    //         return false;
    //     }
    //
    //     _units.Add(unit);
    //     LoggerProvider.Current.Debug("Player {0}: Recruited unit {1}", Id, unit.DefinitionId);
    //     return true;
    // }

    /// <summary>
    /// Removes a unit from the player's control.
    /// </summary>
    // public bool DisbandUnit(UnitInstance unit)
    // {
    //     bool removed = _units.Remove(unit);
    //     if (removed)
    //         LoggerProvider.Current.Debug("Player {0}: Disbanded unit {1}", Id, unit.DefinitionId);
    //     return removed;
    // }

    /// <summary>
    /// Gets all ready (unwounded) units.
    /// </summary>
    // public IEnumerable<UnitInstance> GetReadyUnits()
    // {
    //     return _units.Where(u => u.IsReady);
    // }

    /// <summary>
    /// Gets all wounded units.
    /// </summary>
    // public IEnumerable<UnitInstance> GetWoundedUnits()
    // {
    //     return _units.Where(u => u.IsWounded);
    // }

    #endregion

    #region Skill Management

    // TODO: Skills
    /// <summary>
    /// Adds a skill to the player.
    /// </summary>
    // public void AddSkill(SkillId skill)
    // {
    //     if (!_skills.Contains(skill))
    //     {
    //         _skills.Add(skill);
    //         LoggerProvider.Current.Debug("Player {0}: Acquired skill {1}", Id, skill);
    //     }
    // }

    /// <summary>
    /// Checks if the player has a specific skill.
    /// </summary>
    // public bool HasSkill(SkillId skill)
    // {
    //     return _skills.Contains(skill);
    // }

    #endregion

    #region Crystal Management

    // TODO: Inventory / Crystals
    /// <summary>
    /// Adds a crystal of the specified mana type.
    /// </summary>
    // public void AddCrystal(ManaType type, int count = 1)
    // {
    //     if (count <= 0) return;
    //
    //     if (_crystals.ContainsKey(type))
    //         _crystals[type] += count;
    //     else
    //         _crystals[type] = count;
    //
    //     LoggerProvider.Current.Debug("Player {0}: Added {1} {2} crystal(s)", Id, count, type);
    // }

    /// <summary>
    /// Spends a crystal of the specified type.
    /// </summary>
    /// <returns>True if successful.</returns>
    // public bool SpendCrystal(ManaType type, int count = 1)
    // {
    //     if (!_crystals.TryGetValue(type, out int available) || available < count)
    //         return false;
    //
    //     _crystals[type] -= count;
    //     if (_crystals[type] == 0)
    //         _crystals.Remove(type);
    //
    //     LoggerProvider.Current.Debug("Player {0}: Spent {1} {2} crystal(s)", Id, count, type);
    //     return true;
    // }

    /// <summary>
    /// Uses (spends) a crystal of the specified type for mana.
    /// Alias for SpendCrystal.
    /// </summary>
    /// <returns>True if successful.</returns>
    // public bool UseCrystal(ManaType type)
    // {
    //     return SpendCrystal(type, 1);
    // }

    /// <summary>
    /// Gets the number of crystals of a specific type.
    /// </summary>
    // public int GetCrystalCount(ManaType type)
    // {
    //     return _crystals.TryGetValue(type, out int count) ? count : 0;
    // }

    #endregion

    #region Turn Management

    /// <summary>
    /// Resets turn-based state at the start of a new turn.
    /// </summary>
    public void StartTurn()
    {
        MovementPoints = 0;
        HasAttackedThisTurn = false;
        HasTakenManaThisTurn = false;
        // TODO: Mana Tokens
        // PersonalMana.Clear();
        Log.Debug($"Player {Id}: Turn started");
    }

    /// <summary>
    /// Performs end-of-turn cleanup.
    /// </summary>
    public void EndTurn()
    {
        CleanUpPlayArea();
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
 
