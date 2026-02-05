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

/// <summary>
/// Strongly-typed ID for an enemy definition.
/// </summary>
public readonly record struct EnemyId(string Value)
{
    public static implicit operator string(EnemyId id) => id.Value;
    public static implicit operator EnemyId(string value) => new(value);
    public override string ToString() => Value;
}

/// <summary>
/// Strongly-typed ID for a specific enemy instance on the map.
/// </summary>
public readonly record struct EnemyInstanceId(int Value)
{
    public static implicit operator int(EnemyInstanceId id) => id.Value;
    public static implicit operator EnemyInstanceId(int value) => new(value);
    public override string ToString() => $"EnemyInst{Value}";
}

/// <summary>
/// Strongly-typed ID for a placed map tile instance.
/// </summary>
public readonly record struct TileId(int Value)
{
    public static implicit operator int(TileId id) => id.Value;
    public static implicit operator TileId(int value) => new(value);
    public override string ToString() => $"Tile{Value}";
}

/// <summary>
/// Strongly-typed ID for a tile definition.
/// </summary>
public readonly record struct TileDefinitionId(string Value)
{
    public static implicit operator string(TileDefinitionId id) => id.Value;
    public static implicit operator TileDefinitionId(string value) => new(value);
    public override string ToString() => Value;
}

/// <summary>
/// Strongly-typed ID for a location on the map (village, monastery, etc.).
/// </summary>
public readonly record struct LocationId(string Value)
{
    public static implicit operator string(LocationId id) => id.Value;
    public static implicit operator LocationId(string value) => new(value);
    public override string ToString() => Value;
}

/// <summary>
/// Strongly-typed ID for a unit definition.
/// </summary>
public readonly record struct UnitId(string Value)
{
    public static implicit operator string(UnitId id) => id.Value;
    public static implicit operator UnitId(string value) => new(value);
    public override string ToString() => Value;
}

/// <summary>
/// Strongly-typed ID for a specific recruited unit instance.
/// </summary>
public readonly record struct UnitInstanceId(int Value)
{
    public static implicit operator int(UnitInstanceId id) => id.Value;
    public static implicit operator UnitInstanceId(int value) => new(value);
    public override string ToString() => $"UnitInst{Value}";
}

/// <summary>
/// Strongly-typed ID for a scenario definition.
/// </summary>
public readonly record struct ScenarioId(string Value)
{
    public static implicit operator string(ScenarioId id) => id.Value;
    public static implicit operator ScenarioId(string value) => new(value);
    public override string ToString() => Value;
}

/// <summary>
/// Strongly-typed ID for a hero definition.
/// </summary>
public readonly record struct HeroId(string Value)
{
    public static implicit operator string(HeroId id) => id.Value;
    public static implicit operator HeroId(string value) => new(value);
    public override string ToString() => Value;
}

/// <summary>
/// Strongly-typed ID for a skill definition.
/// </summary>
public readonly record struct SkillId(string Value)
{
    public static implicit operator string(SkillId id) => id.Value;
    public static implicit operator SkillId(string value) => new(value);
    public override string ToString() => Value;
}

/// <summary>
/// Strongly-typed ID for a combat instance.
/// </summary>
public readonly record struct CombatId(int Value)
{
    public static implicit operator int(CombatId id) => id.Value;
    public static implicit operator CombatId(int value) => new(value);
    public override string ToString() => $"Combat{Value}";
}
