using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Represents a placed hex cell on the game map.
/// Contains both static terrain data and runtime state.
/// </summary>
public sealed class HexCell
{
    /// <summary>
    /// World coordinate of this cell on the map.
    /// </summary>
    public HexCoord WorldCoord { get; init; }

    /// <summary>
    /// Terrain type of this cell.
    /// </summary>
    public TerrainType Terrain { get; init; }

    /// <summary>
    /// Location ID if this cell contains a location. Null otherwise.
    /// </summary>
    public LocationId? LocationId { get; init; }

    /// <summary>
    /// Whether this cell is coastal (adjacent to water).
    /// </summary>
    public bool IsCoastal { get; init; }

    /// <summary>
    /// The tile this cell belongs to.
    /// </summary>
    public TileId TileId { get; init; }

    // ─────────────────────────────────────────────────────────────
    // Runtime state
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Runtime state for location at this cell (if any).
    /// </summary>
    public LocationState? Location { get; set; }

    /// <summary>
    /// Enemies currently at this cell.
    /// </summary>
    public List<EnemyInstanceId> Enemies { get; init; } = [];

    /// <summary>
    /// Players currently at this cell.
    /// </summary>
    public List<PlayerId> Players { get; init; } = [];

    // ─────────────────────────────────────────────────────────────
    // Computed properties
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if this cell has a location.
    /// </summary>
    public bool HasLocation => LocationId.HasValue;

    /// <summary>
    /// Returns true if there are enemies at this cell.
    /// </summary>
    public bool HasEnemies => Enemies.Count > 0;

    /// <summary>
    /// Returns true if there are players at this cell.
    /// </summary>
    public bool HasPlayers => Players.Count > 0;

    /// <summary>
    /// Returns true if this terrain is normally impassable.
    /// </summary>
    public bool IsImpassable => Terrain is TerrainType.Mountain or TerrainType.Lake or TerrainType.Ocean;

    /// <summary>
    /// Creates a new HexCell with the specified properties.
    /// </summary>
    public HexCell(HexCoord worldCoord, TerrainType terrain, TileId tileId,
        LocationId? locationId = null, bool isCoastal = false)
    {
        WorldCoord = worldCoord;
        Terrain = terrain;
        TileId = tileId;
        LocationId = locationId;
        IsCoastal = isCoastal;
    }

    public override string ToString() =>
        $"HexCell({WorldCoord}, {Terrain}{(HasLocation ? $", {LocationId}" : "")})";
}

/// <summary>
/// Runtime state for a location on the map.
/// </summary>
public sealed class LocationState
{
    /// <summary>
    /// Whether this location has been interacted with.
    /// </summary>
    public bool IsInteracted { get; set; }

    /// <summary>
    /// Whether this location is conquered/controlled.
    /// </summary>
    public bool IsConquered { get; set; }

    /// <summary>
    /// Player who controls this location. Null if uncontrolled.
    /// </summary>
    public PlayerId? ControllerId { get; set; }

    /// <summary>
    /// Available units for recruitment (for villages/keeps).
    /// </summary>
    public List<UnitId> AvailableUnits { get; init; } = new();

    /// <summary>
    /// Available advanced actions (for monasteries).
    /// </summary>
    public List<CardId> AvailableAdvancedActions { get; init; } = new();

    /// <summary>
    /// Remaining uses if location is limited (e.g., healing fountain).
    /// </summary>
    public int? RemainingUses { get; set; }
}

/// <summary>
/// Types of locations that can appear on the map.
/// </summary>
public enum LocationType
{
    /// <summary>Village - recruit regular units.</summary>
    Village,

    /// <summary>Monastery - gain advanced actions or healing.</summary>
    Monastery,

    /// <summary>Keep - recruit elite units, may have defenders.</summary>
    Keep,

    /// <summary>Mage Tower - gain spells, may have defenders.</summary>
    MageTower,

    /// <summary>Dungeon - challenge for artifacts.</summary>
    Dungeon,

    /// <summary>Ancient Ruins - special challenges.</summary>
    AncientRuins,

    /// <summary>City - final objective locations.</summary>
    City,

    /// <summary>Portal - special movement location.</summary>
    Portal,

    /// <summary>Crystal Mine - gain crystals.</summary>
    CrystalMine,

    /// <summary>Spawning Ground - enemy respawn point.</summary>
    SpawningGround
}

/// <summary>
/// Defines a location type that can appear on the map.
/// </summary>
public sealed class LocationDefinition
{
    /// <summary>
    /// Unique identifier for this location type.
    /// </summary>
    public LocationId Id { get; init; }

    /// <summary>
    /// Type of location.
    /// </summary>
    public LocationType Type { get; init; }

    /// <summary>
    /// Localization key for location name.
    /// </summary>
    public LocalizationKey NameKey { get; init; }

    /// <summary>
    /// Enemy categories that defend this location (if any).
    /// </summary>
    public IReadOnlyList<EnemyCategory> Defenders { get; init; } = [];

    /// <summary>
    /// Number of defenders to spawn.
    /// </summary>
    public int DefenderCount { get; init; }

    /// <summary>
    /// Whether this location is safe (no rampaging enemies).
    /// </summary>
    public bool IsSafe { get; init; }

    /// <summary>
    /// Influence bonus/penalty for interaction.
    /// </summary>
    public int InfluenceModifier { get; init; }
}
