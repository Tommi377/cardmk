namespace RealMK;

/// <summary>
/// Strongly-typed ID for a game instance.
/// </summary>
public readonly record struct GameId(string Value)
{
    public static implicit operator string(GameId id) => id.Value;
    public static implicit operator GameId(string value) => new(value);
    public override string ToString() => Value;
}

/// <summary>
/// Strongly-typed ID for a player.
/// </summary>
public readonly record struct PlayerId(int Value)
{
    public static implicit operator int(PlayerId id) => id.Value;
    public static implicit operator PlayerId(int value) => new(value);
    public override string ToString() => $"Player{Value}";
}

/// <summary>
/// Strongly-typed ID for a card definition.
/// </summary>
public readonly record struct CardId(string Value)
{
    public static implicit operator string(CardId id) => id.Value;
    public static implicit operator CardId(string value) => new(value);
    public override string ToString() => Value;
}

/// <summary>
/// Strongly-typed ID for a specific card instance in play.
/// </summary>
public readonly record struct CardInstanceId(int Value)
{
    public static implicit operator int(CardInstanceId id) => id.Value;
    public static implicit operator CardInstanceId(int value) => new(value);
    public override string ToString() => $"CardInst{Value}";
}