namespace RealMK;

/// <summary>
/// Session-scoped deterministic ID generator for map entities.
/// </summary>
public sealed class SessionMapIdGenerator : IMapIdGenerator
{
    private int _nextTileId = 1;
    private int _nextEnemyInstanceId = 1;

    /// <inheritdoc/>
    public TileId NextTileId()
    {
        return new TileId(_nextTileId++);
    }

    /// <inheritdoc/>
    public EnemyInstanceId NextEnemyInstanceId()
    {
        return new EnemyInstanceId(_nextEnemyInstanceId++);
    }
}
