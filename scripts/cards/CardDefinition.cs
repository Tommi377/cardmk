namespace RealMK;

/// <summary>
/// Defines the static data for a card type.
/// Loaded from content JSON, immutable at runtime.
/// </summary>
public sealed class CardDefinition
{
    /// <summary>
    /// Unique identifier for this card, e.g., "card.swiftness".
    /// </summary>
    public CardId Id { get; init; }

    /// <summary>
    /// The category of card (Basic, Advanced, Spell, Wound, Artifact).
    /// </summary>
    public CardType Type { get; init; }

    /// <summary>
    /// The card's color for mana matching (Red, Blue, Green, White, Gold, None).
    /// </summary>
    public CardColor Color { get; init; }

    /// <summary>
    /// Localization key for the card's display name.
    /// </summary>
    public LocalizationKey NameKey { get; init; }

    /// <summary>
    /// Localization key for the card's description text.
    /// </summary>
    public LocalizationKey DescriptionKey { get; init; }

    /// <summary>
    /// The effect when played normally (without mana).
    /// </summary>
    public IEffect? BasicEffect { get; init; }

    /// <summary>
    /// The enhanced effect when powered with matching mana. Null if card has no enhanced mode.
    /// </summary>
    public IEffect? EnhancedEffect { get; init; }

    /// <summary>
    /// The universal weak effect when played sideways. Null if card cannot be played sideways.
    /// </summary>
    public IEffect? SidewaysEffect { get; init; }

    /// <summary>
    /// The value granted when played sideways (usually 1 for basic, 2 for advanced).
    /// </summary>
    public int SidewaysValue { get; init; } = 1;

    /// <summary>
    /// Whether this card can be played sideways for a generic effect.
    /// </summary>
    public bool CanBePlacedSideways { get; init; } = true;

    /// <summary>
    /// If set, this card is only available to a specific hero. Null means available to all.
    /// </summary>
    public HeroId? HeroSpecific { get; init; }

    /// <summary>
    /// Returns true if this is a wound card.
    /// </summary>
    public bool IsWound => Type == CardType.Wound;

    /// <summary>
    /// Returns the mana type required to power this card's enhanced effect.
    /// Returns null for colorless cards or wounds.
    /// </summary>
    public ManaType? RequiredMana => Color switch
    {
        CardColor.Red => ManaType.Red,
        CardColor.Blue => ManaType.Blue,
        CardColor.Green => ManaType.Green,
        CardColor.White => ManaType.White,
        CardColor.Gold => ManaType.Gold,
        _ => null
    };
}

/// <summary>
/// Card color for basic and advanced action cards.
/// </summary>
public enum CardColor
{
    Red,
    Blue,
    Green,
    White,
    Gold,
    None
}

/// <summary>
/// Card type categories.
/// </summary>
public enum CardType
{
    Basic,
    Advanced,
    Spell,
    Wound,
    Artifact
}
