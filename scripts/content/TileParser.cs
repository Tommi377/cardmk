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
        if (string.IsNullOrWhiteSpace(dto.Id))
        {
            throw new ContentParseException("Tile is missing required 'id' field");
        }

        var hexes = new Dictionary<HexCoord, TileHexDefinition>();
        if (dto.Hexes != null)
        {
            for (int i = 0; i < dto.Hexes.Count; i++)
            {
                HexDto hexDto = dto.Hexes[i];
                HexCoord coord = new(hexDto.Q ?? 0, hexDto.R ?? 0);
                if (hexes.ContainsKey(coord))
                {
                    throw new ContentParseException($"Tile '{dto.Id}' has duplicate hex coordinate {coord}");
                }

                hexes[coord] = ParseHex(hexDto, dto.Id, i);
            }
        }

        return new TileDefinition
        {
            Id = new TileDefinitionId(dto.Id),
            Category = ParseTileCategory(dto.Category, dto.Id),
            NameKey = new LocalizationKey(dto.NameKey ?? $"{dto.Id}.name"),
            Hexes = hexes
        };
    }

    private TileHexDefinition ParseHex(HexDto dto, string tileId, int index)
    {
        return new TileHexDefinition
        {
            Terrain = ParseTerrainType(dto.Terrain, tileId, index),
            LocationType = string.IsNullOrWhiteSpace(dto.LocationType) ? null : ParseLocationType(dto.LocationType, tileId, index),
            SpawnCategory = string.IsNullOrWhiteSpace(dto.SpawnCategory) ? null : ParseEnemyCategory(dto.SpawnCategory, tileId, index),
        };
    }

    private static TileCategory ParseTileCategory(string? category, string tileId) => category?.ToLowerInvariant() switch
    {
        "starting" => TileCategory.Starting,
        "countryside" => TileCategory.Countryside,
        "core" => TileCategory.Core,
        "city" => TileCategory.City,
        null or "" => throw new ContentParseException($"Tile '{tileId}' is missing required field 'category'"),
        _ => throw new ContentParseException($"Tile '{tileId}' has unknown category '{category}'")
    };

    private static TerrainType ParseTerrainType(string? terrain, string tileId, int index) => terrain?.ToLowerInvariant() switch
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
        null or "" => throw new ContentParseException($"Tile '{tileId}' hex[{index}] is missing required field 'terrain'"),
        _ => throw new ContentParseException($"Tile '{tileId}' hex[{index}] has unknown terrain '{terrain}'")
    };

    private static LocationType ParseLocationType(string? location, string tileId, int index) => location?.ToLowerInvariant() switch
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
        _ => throw new ContentParseException($"Tile '{tileId}' hex[{index}] has unknown locationType '{location}'")
    };

    private static EnemyCategory ParseEnemyCategory(string? category, string tileId, int index) => category?.ToLowerInvariant() switch
    {
        "marauding" => EnemyCategory.Marauding,
        "keep" => EnemyCategory.Keep,
        "tower" => EnemyCategory.Tower,
        "dungeon" => EnemyCategory.Dungeon,
        "city" => EnemyCategory.City,
        "draconum" => EnemyCategory.Draconum,
        _ => throw new ContentParseException($"Tile '{tileId}' hex[{index}] has unknown spawnCategory '{category}'")
    };

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
