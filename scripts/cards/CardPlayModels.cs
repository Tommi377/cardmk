using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Zone where a card instance currently resides.
/// </summary>
public enum CardZone
{
    DrawPile,
    Hand,
    PlayArea,
    DiscardPile,
    Removed
}

/// <summary>
/// Mode used to play a card.
/// </summary>
public enum CardPlayMode
{
    Basic,
    Enhanced,
    Sideways
}

/// <summary>
/// Deterministic input selections provided when resolving a played card.
/// </summary>
public sealed class CardResolutionInput
{
    /// <summary>
    /// Choice selections keyed by effect path, e.g. "root", "root.1".
    /// </summary>
    public IReadOnlyDictionary<string, int> ChoiceSelections { get; init; } = new Dictionary<string, int>();

    /// <summary>
    /// Optional targeting selections keyed by target token.
    /// </summary>
    public IReadOnlyDictionary<string, string> TargetSelections { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets a selected index for a choice path.
    /// </summary>
    public bool TryGetChoice(string path, out int index)
    {
        return ChoiceSelections.TryGetValue(path, out index);
    }
}

/// <summary>
/// Typed request model for playing a card through the session API.
/// </summary>
public sealed class PlayCardRequest
{
    /// <summary>
    /// Player issuing the play.
    /// </summary>
    public PlayerId PlayerId { get; init; }

    /// <summary>
    /// Card instance to play.
    /// </summary>
    public CardInstanceId CardInstanceId { get; init; }

    /// <summary>
    /// Play mode for this card.
    /// </summary>
    public CardPlayMode Mode { get; init; }

    /// <summary>
    /// Deterministic resolution inputs (choices/targets).
    /// </summary>
    public CardResolutionInput ResolutionInput { get; init; } = new();
}
