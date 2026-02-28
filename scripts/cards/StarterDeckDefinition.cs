using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Content definition for a hero starter deck.
/// </summary>
public sealed class StarterDeckDefinition
{
    /// <summary>
    /// Hero associated with this starter deck.
    /// </summary>
    public HeroId HeroId { get; init; }

    /// <summary>
    /// Card entries that compose this starter deck.
    /// </summary>
    public IReadOnlyList<StarterDeckEntry> Entries { get; init; } = [];
}

/// <summary>
/// Single card entry in a starter deck.
/// </summary>
public sealed class StarterDeckEntry
{
    /// <summary>
    /// Card definition id.
    /// </summary>
    public CardId CardId { get; init; }

    /// <summary>
    /// Number of copies in the starter deck.
    /// </summary>
    public int Count { get; init; }
}
