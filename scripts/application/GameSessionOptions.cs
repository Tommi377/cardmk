using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Setup options used when constructing a game session.
/// </summary>
public sealed class GameSessionOptions
{
    /// <summary>
    /// Player setups to initialize in game state.
    /// </summary>
    public IReadOnlyList<PlayerSetup> Players { get; init; } = [];
}

/// <summary>
/// Setup data for a single player.
/// </summary>
public sealed class PlayerSetup
{
    /// <summary>
    /// Player id.
    /// </summary>
    public PlayerId PlayerId { get; init; }

    /// <summary>
    /// Hero id used for starter deck lookup.
    /// </summary>
    public HeroId HeroId { get; init; }

    /// <summary>
    /// Starting world position.
    /// </summary>
    public HexCoord StartPosition { get; init; }
}
