using System;

namespace RealMK;

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
    public CardZone Zone { get; set; }

    /// <summary>
    /// Returns true if this card instance is a wound card.
    /// Determined by checking if the DefinitionId matches the wound card pattern.
    /// </summary>
    public bool IsWound => DefinitionId.Value.StartsWith("card.wound", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a new card instance with the specified properties.
    /// </summary>
    public CardInstance(CardInstanceId id, CardId definitionId, PlayerId ownerId, CardZone initialZone = CardZone.DrawPile)
    {
        Id = id;
        DefinitionId = definitionId;
        OwnerId = ownerId;
        Zone = initialZone;
    }

    /// <summary>
    /// Returns true if this card can currently be played (is in hand and not a wound).
    /// </summary>
    public bool CanBePlayed => Zone == CardZone.Hand && !IsWound;

    /// <summary>
    /// Returns true if this card can be discarded (is in hand or play area).
    /// </summary>
    public bool CanBeDiscarded => Zone is CardZone.Hand or CardZone.PlayArea;

    public override string ToString() => $"CardInstance({Id}, {DefinitionId}, {Zone})";
}
