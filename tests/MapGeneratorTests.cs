using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RealMK.Tests;

[Trait("Category", "Unit")]
public class MapGeneratorTests
{
    /// <summary>
    /// Creates a test content database with sample tiles.
    /// </summary>
    private static ContentDatabase CreateTestContent()
    {
        var db = new ContentDatabase();

        // Add a starting tile
        db.AddTile(new TileDefinition
        {
            Id = new TileDefinitionId("tile.starting_test"),
            Category = TileCategory.Starting,
            NameKey = new LocalizationKey("tile.starting_test.name"),
            Hexes = CreateStandardHexLayout(TerrainType.Plains)
        });

        // Add countryside tiles
        db.AddTile(new TileDefinition
        {
            Id = new TileDefinitionId("tile.countryside_01"),
            Category = TileCategory.Countryside,
            NameKey = new LocalizationKey("tile.countryside_01.name"),
            Hexes = CreateStandardHexLayout(TerrainType.Forest)
        });

        db.AddTile(new TileDefinition
        {
            Id = new TileDefinitionId("tile.countryside_02"),
            Category = TileCategory.Countryside,
            NameKey = new LocalizationKey("tile.countryside_02.name"),
            Hexes = CreateStandardHexLayout(TerrainType.Hills)
        });

        db.AddTile(new TileDefinition
        {
            Id = new TileDefinitionId("tile.countryside_03"),
            Category = TileCategory.Countryside,
            NameKey = new LocalizationKey("tile.countryside_03.name"),
            Hexes = CreateStandardHexLayout(TerrainType.Plains)
        });

        db.AddTile(new TileDefinition
        {
            Id = new TileDefinitionId("tile.countryside_04"),
            Category = TileCategory.Countryside,
            NameKey = new LocalizationKey("tile.countryside_04.name"),
            Hexes = CreateStandardHexLayout(TerrainType.Desert)
        });

        db.AddTile(new TileDefinition
        {
            Id = new TileDefinitionId("tile.countryside_05"),
            Category = TileCategory.Countryside,
            NameKey = new LocalizationKey("tile.countryside_05.name"),
            Hexes = CreateStandardHexLayout(TerrainType.Wasteland)
        });

        db.AddTile(new TileDefinition
        {
            Id = new TileDefinitionId("tile.countryside_06"),
            Category = TileCategory.Countryside,
            NameKey = new LocalizationKey("tile.countryside_06.name"),
            Hexes = CreateStandardHexLayout(TerrainType.Forest)
        });

        // Add core tiles
        db.AddTile(new TileDefinition
        {
            Id = new TileDefinitionId("tile.core_01"),
            Category = TileCategory.Core,
            NameKey = new LocalizationKey("tile.core_01.name"),
            Hexes = CreateStandardHexLayout(TerrainType.Mountain)
        });

        // Add city tile
        db.AddTile(new TileDefinition
        {
            Id = new TileDefinitionId("tile.city_01"),
            Category = TileCategory.City,
            NameKey = new LocalizationKey("tile.city_01.name"),
            Hexes = CreateStandardHexLayout(TerrainType.City)
        });

        return db;
    }

    /// <summary>
    /// Creates a standard 7-hex tile layout with the given terrain.
    /// </summary>
    private static Dictionary<HexCoord, TileHexDefinition> CreateStandardHexLayout(TerrainType terrain)
    {
        var hexes = new Dictionary<HexCoord, TileHexDefinition>
        {
            [new HexCoord(0, 0)] = new TileHexDefinition { Terrain = terrain },
            [new HexCoord(1, 0)] = new TileHexDefinition { Terrain = terrain },
            [new HexCoord(0, 1)] = new TileHexDefinition { Terrain = terrain },
            [new HexCoord(-1, 1)] = new TileHexDefinition { Terrain = terrain },
            [new HexCoord(-1, 0)] = new TileHexDefinition { Terrain = terrain },
            [new HexCoord(0, -1)] = new TileHexDefinition { Terrain = terrain },
            [new HexCoord(1, -1)] = new TileHexDefinition { Terrain = terrain }
        };
        return hexes;
    }

    [Fact]
    public void InitializeMap_PlacesStartingTileAtOrigin()
    {
        // Arrange
        var content = CreateTestContent();
        var rng = new DeterministicRandom(42);
        var generator = new MapGenerator(content, rng);

        // Act
        var result = generator.InitializeMap();

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.Tile);
        Assert.Equal(new HexCoord(0, 0), result.Tile.CenterPosition);
        Assert.Equal(TileCategory.Starting, result.Tile.Definition.Category);
    }

    [Fact]
    public void InitializeMap_CreatesDecksFromContent()
    {
        // Arrange
        var content = CreateTestContent();
        var rng = new DeterministicRandom(42);
        var generator = new MapGenerator(content, rng);

        // Act
        generator.InitializeMap();

        // Assert - InitializeMap places 3 countryside tiles (NE, NW, W), 6 total - 3 = 3 remaining
        Assert.Equal(3, generator.GetDeckCount(TileCategory.Countryside));
        Assert.Equal(1, generator.GetDeckCount(TileCategory.Core));
        Assert.Equal(1, generator.GetDeckCount(TileCategory.City));
    }

    [Fact]
    public void InitializeMap_MapContainsStartingTileHexes()
    {
        // Arrange
        var content = CreateTestContent();
        var rng = new DeterministicRandom(42);
        var generator = new MapGenerator(content, rng);

        // Act
        generator.InitializeMap();

        // Assert - Starting tile (7 hexes) + 3 countryside tiles (21 hexes) = 28 total
        Assert.True(generator.Map.HasHex(new HexCoord(0, 0)));
        Assert.Equal(28, generator.Map.CellCount);
    }

    [Fact]
    public void DrawTile_DecrementsDeckCount()
    {
        // Arrange
        var content = CreateTestContent();
        var rng = new DeterministicRandom(42);
        var generator = new MapGenerator(content, rng);
        generator.InitializeMap();

        int initialCount = generator.GetDeckCount(TileCategory.Countryside);

        // Act
        var tile = generator.DrawTile(TileCategory.Countryside);

        // Assert
        Assert.NotNull(tile);
        Assert.Equal(initialCount - 1, generator.GetDeckCount(TileCategory.Countryside));
    }

    [Fact]
    public void DrawTile_ReturnsNullWhenDeckEmpty()
    {
        // Arrange
        var content = CreateTestContent();
        var rng = new DeterministicRandom(42);
        var generator = new MapGenerator(content, rng);
        generator.InitializeMap();

        // Drain the core deck (only 1 tile)
        generator.DrawTile(TileCategory.Core);

        // Act
        var tile = generator.DrawTile(TileCategory.Core);

        // Assert
        Assert.Null(tile);
        Assert.Equal(0, generator.GetDeckCount(TileCategory.Core));
    }

    [Fact]
    public void ExploreTile_PlacesTileAdjacentToEdge()
    {
        // Arrange
        var content = CreateTestContent();
        var rng = new DeterministicRandom(42);
        var generator = new MapGenerator(content, rng);
        generator.InitializeMap();

        // Get a valid explorable edge from the generator
        var explorableEdges = generator.GetExplorableEdges();
        Assert.NotEmpty(explorableEdges);

        var (edgeHex, direction) = explorableEdges[0];

        // Act
        var result = generator.ExploreTile(edgeHex, direction, TileCategory.Countryside);

        // Assert
        Assert.True(result.IsValid, $"ExploreTile failed: {result.ErrorMessage}");
        Assert.NotNull(result.Tile);
        Assert.True(generator.Map.TileCount > 1);
    }

    [Fact]
    public void ExploreTile_FailsForInvalidEdgeCoord()
    {
        // Arrange
        var content = CreateTestContent();
        var rng = new DeterministicRandom(42);
        var generator = new MapGenerator(content, rng);
        generator.InitializeMap();

        // Try to explore from a hex not on the map
        HexCoord invalidHex = new(100, 100);

        // Act
        var result = generator.ExploreTile(invalidHex, 0, TileCategory.Countryside);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("not on the map", result.ErrorMessage);
    }

    [Fact]
    public void ExploreTile_FailsWhenDirectionHasExistingTile()
    {
        // Arrange
        var content = CreateTestContent();
        var rng = new DeterministicRandom(42);
        var generator = new MapGenerator(content, rng);
        generator.InitializeMap();

        // The center hex has neighbors already placed (other hexes of the starting tile)
        HexCoord centerHex = new(0, 0);

        // Act - try to place in a direction that already has a hex
        var result = generator.ExploreTile(centerHex, 0, TileCategory.Countryside);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("does not lead off the map", result.ErrorMessage);
    }

    [Fact]
    public void GetExplorableEdges_ReturnsEdgeHexesWithUnexploredDirections()
    {
        // Arrange
        var content = CreateTestContent();
        var rng = new DeterministicRandom(42);
        var generator = new MapGenerator(content, rng);
        generator.InitializeMap();

        // Act
        var edges = generator.GetExplorableEdges();

        // Assert
        Assert.NotEmpty(edges);
        foreach (var (position, direction) in edges)
        {
            // Verify position is on the map
            Assert.True(generator.Map.HasHex(position));
            // Verify direction leads off the map
            Assert.False(generator.Map.HasHex(position.Neighbor(direction)));
        }
    }

    [Fact]
    public void DeckShuffling_IsDeterministic()
    {
        // Arrange
        var content = CreateTestContent();
        var rng1 = new DeterministicRandom(42);
        var rng2 = new DeterministicRandom(42);

        var generator1 = new MapGenerator(content, rng1);
        var generator2 = new MapGenerator(content, rng2);

        // Act
        generator1.InitializeMap();
        generator2.InitializeMap();

        var tile1 = generator1.DrawTile(TileCategory.Countryside);
        var tile2 = generator2.DrawTile(TileCategory.Countryside);

        // Assert - same seed should produce same tile order
        Assert.NotNull(tile1);
        Assert.NotNull(tile2);
        Assert.Equal(tile1.Id, tile2.Id);
    }
}
