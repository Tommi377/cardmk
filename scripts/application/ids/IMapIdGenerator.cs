namespace RealMK;

/// <summary>
/// Provides deterministic runtime IDs for map entities.
/// </summary>
public interface IMapIdGenerator
{
    /// <summary>
    /// Gets the next placed tile ID.
    /// </summary>
    TileId NextTileId();

    /// <summary>
    /// Gets the next enemy instance ID.
    /// </summary>
    EnemyInstanceId NextEnemyInstanceId();
}
