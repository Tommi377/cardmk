using System;

namespace RealMK;

/// <summary>
/// The current location/state of a card instance.
/// </summary>
public enum CardState
{
    InDeck,
    InHand,
    InPlayArea,
    InDiscard,
    Removed
}

/// <summary>
/// Represents a specific card instance in play.
/// Each physical copy of a card definition gets its own instance.
/// </summary>
public sealed class CardInstance
{
    /// <summary>
    /// Unique identifier for this specific card instance.
    /// </summary>
    public CardInstanceId Id { get; init; }

    /// <summary>
    /// Reference to the card definition (template) for this instance.
    /// </summary>
    public CardId DefinitionId { get; init; }

    /// <summary>
    /// The player who owns this card.
    /// </summary>
    public PlayerId OwnerId { get; init; }

    /// <summary>
    /// Current state/location of this card.
    /// </summary>
    public CardState State { get; set; }

    /// <summary>
    /// Returns true if this card instance is a wound card.
    /// Determined by checking if the DefinitionId matches the wound card pattern.
    /// </summary>
    public bool IsWound => DefinitionId.Value.StartsWith("card.wound", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a new card instance with the specified properties.
    /// </summary>
    public CardInstance(CardInstanceId id, CardId definitionId, PlayerId ownerId, CardState initialState = CardState.InDeck)
    {
        Id = id;
        DefinitionId = definitionId;
        OwnerId = ownerId;
        State = initialState;
    }

    /// <summary>
    /// Returns true if this card can currently be played (is in hand and not a wound).
    /// </summary>
    public bool CanBePlayed => State == CardState.InHand && !IsWound;

    /// <summary>
    /// Returns true if this card can be discarded (is in hand or play area).
    /// </summary>
    public bool CanBeDiscarded => State is CardState.InHand or CardState.InPlayArea;

    public override string ToString() => $"CardInstance({Id}, {DefinitionId}, {State})";
}