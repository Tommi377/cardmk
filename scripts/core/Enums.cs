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