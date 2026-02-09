namespace RealMK;

/// <summary>
/// Attack types for combat.
/// </summary>
public enum AttackType
{
    Melee,
    Ranged,
    Siege
}

/// <summary>
/// Damage element types.
/// </summary>
public enum Element
{
    Physical,
    Fire,
    Ice,
    ColdFire
}

/// <summary>
/// Movement types affecting terrain traversal.
/// </summary>
public enum MovementType
{
    Normal,
    Flying,
    Underground
}

/// <summary>
/// Day or night phase of the game.
/// </summary>
public enum DayNightPhase
{
    Day,
    Night
}

/// <summary>
/// Terrain types for hex cells.
/// </summary>
public enum TerrainType
{
    Plains,
    Forest,
    Hills,
    Swamp,
    Wasteland,
    Desert,
    Mountain,
    Lake,
    City
}

/// <summary>
/// Enemy category types.
/// </summary>
public enum EnemyCategory
{
    Marauding,
    Keep,
    Tower,
    Dungeon,
    City,
    Draconum
}
