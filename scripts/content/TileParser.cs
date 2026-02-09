using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RealMK;

/// <summary>
/// Parses tile definitions from JSON content files.
/// </summary>
public sealed class TileParser
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Parses all tiles from a JSON file.
    /// </summary>
    public IReadOnlyList<TileDefinition> ParseFile(string filePath)
    {
        string json = File.ReadAllText(filePath);
        return ParseJson(json);
    }

    /// <summary>
    /// Parses all tiles from JSON string.
    /// </summary>
    public IReadOnlyList<TileDefinition> ParseJson(string json)
    {
        TileFileDto? fileDto = JsonSerializer.Deserialize<TileFileDto>(json, _jsonOptions);

        if (fileDto?.Tiles == null)
        {
            throw new ContentParseException("Invalid tile file: missing 'tiles' array");
        }

        var results = new List<TileDefinition>();
        foreach (TileDto dto in fileDto.Tiles)
        {
            results.Add(ParseTile(dto));
        }
        return results;
    }

    private TileDefinition ParseTile(TileDto dto)
    {
        if (string.IsNullOrEmpty(dto.Id))
        {
            throw new ContentParseException("Tile is missing required 'id' field");
        }

        var hexes = new Dictionary<HexCoord, TileHexDefinition>();
        if (dto.Hexes != null)
        {
            foreach (HexDto hexDto in dto.Hexes)
            {
                HexCoord coord = new(hexDto.Q ?? 0, hexDto.R ?? 0);
                hexes[coord] = ParseHex(hexDto);
            }
        }

        return new TileDefinition
        {
            Id = new TileDefinitionId(dto.Id),
            Category = ParseTileCategory(dto.Category),
            NameKey = new LocalizationKey(dto.NameKey ?? $"{dto.Id}.name"),
            Hexes = hexes
        };
    }

    private TileHexDefinition ParseHex(HexDto dto)
    {
        return new TileHexDefinition
        {
            Terrain = ParseTerrainType(dto.Terrain),
            LocationType = string.IsNullOrEmpty(dto.LocationType) ? null : ParseLocationType(dto.LocationType),
            SpawnCategory = string.IsNullOrEmpty(dto.SpawnCategory) ? null : ParseEnemyCategory(dto.SpawnCategory),
        };
    }

    private static TileCategory ParseTileCategory(string? category) => category?.ToLowerInvariant() switch
    {
        "starting" => TileCategory.Starting,
        "countryside" => TileCategory.Countryside,
        "core" => TileCategory.Core,
        "city" => TileCategory.City,
        _ => TileCategory.Countryside
    };

    private static TerrainType ParseTerrainType(string? terrain) => terrain?.ToLowerInvariant() switch
    {
        "plains" => TerrainType.Plains,
        "forest" => TerrainType.Forest,
        "hills" => TerrainType.Hills,
        "swamp" => TerrainType.Swamp,
        "wasteland" => TerrainType.Wasteland,
        "desert" => TerrainType.Desert,
        "mountain" => TerrainType.Mountain,
        "lake" => TerrainType.Lake,
        "city" => TerrainType.City,
        _ => TerrainType.Plains
    };
    
    private static LocationType ParseLocationType(string? location) => location?.ToLowerInvariant() switch
    {
        "village" => LocationType.Village,
        "monastery" => LocationType.Monastery,
        "keep" => LocationType.Keep,
        "mage_tower" => LocationType.MageTower,
        "city" => LocationType.City,
        "dungeon" => LocationType.Dungeon,
        "tomb" => LocationType.Tomb,
        "ancient_ruins" => LocationType.AncientRuins,
        "crystal_mine" => LocationType.CrystalMine,
        "magical_glade" => LocationType.MagicalGlade,
        "monster_den" => LocationType.MonsterDen,
        "spawning_ground" => LocationType.SpawningGround,
        _ => LocationType.Village
    };

    private static EnemyCategory ParseEnemyCategory(string? category) => category?.ToLowerInvariant() switch
    {
        "marauding" => EnemyCategory.Marauding,
        "keep" => EnemyCategory.Keep,
        "tower" => EnemyCategory.Tower,
        "dungeon" => EnemyCategory.Dungeon,
        "city" => EnemyCategory.City,
        "draconum" => EnemyCategory.Draconum,
        _ => EnemyCategory.Marauding
    };

    // ─────────────────────────────────────────────────────────────
    // DTOs for JSON deserialization
    // ─────────────────────────────────────────────────────────────

    private sealed class TileFileDto
    {
        [JsonPropertyName("tiles")]
        public List<TileDto>? Tiles { get; set; }
    }

    private sealed class TileDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("nameKey")]
        public string? NameKey { get; set; }

        [JsonPropertyName("hexes")]
        public List<HexDto>? Hexes { get; set; }
    }

    private sealed class HexDto
    {
        [JsonPropertyName("q")]
        public int? Q { get; set; }

        [JsonPropertyName("r")]
        public int? R { get; set; }

        [JsonPropertyName("terrain")]
        public string? Terrain { get; set; }

        [JsonPropertyName("locationType")]
        public string? LocationType { get; set; }

        [JsonPropertyName("spawnCategory")]
        public string? SpawnCategory { get; set; }
    }
}
