using System.Collections.Generic;

namespace RealMK.Tests;

internal static class TestDataFactory
{
    public static ContentDatabase CreateContentDatabase()
    {
        var db = new ContentDatabase();

        db.AddCard(new CardDefinition
        {
            Id = new CardId("card.test.move"),
            Type = CardType.Basic,
            Color = CardColor.Green,
            NameKey = new LocalizationKey("card.test.move.name"),
            DescriptionKey = new LocalizationKey("card.test.move.desc"),
            BasicEffect = new MovementEffect
            {
                Points = 2,
                MoveType = MovementType.Normal
            }
        });

        db.AddTile(CreateTile("tile.starting_01", TileCategory.Starting));
        db.AddTile(CreateTile("tile.countryside_01", TileCategory.Countryside));
        db.AddTile(CreateTile("tile.countryside_02", TileCategory.Countryside));
        db.AddTile(CreateTile("tile.countryside_03", TileCategory.Countryside));
        db.AddTile(CreateTile("tile.countryside_04", TileCategory.Countryside));
        db.AddTile(CreateTile("tile.core_01", TileCategory.Core));
        db.AddTile(CreateTile("tile.core_02", TileCategory.Core));
        db.AddTile(CreateTile("tile.city_01", TileCategory.City));

        return db;
    }

    private static TileDefinition CreateTile(string id, TileCategory category)
    {
        return new TileDefinition
        {
            Id = new TileDefinitionId(id),
            Category = category,
            NameKey = new LocalizationKey($"{id}.name"),
            Hexes = CreateHexes()
        };
    }

    private static IReadOnlyDictionary<HexCoord, TileHexDefinition> CreateHexes()
    {
        return new Dictionary<HexCoord, TileHexDefinition>
        {
            [new HexCoord(0, 0)] = new() { Terrain = TerrainType.Plains },
            [new HexCoord(1, -1)] = new() { Terrain = TerrainType.Forest },
            [new HexCoord(1, 0)] = new() { Terrain = TerrainType.Hills },
            [new HexCoord(0, 1)] = new() { Terrain = TerrainType.Swamp },
            [new HexCoord(-1, 1)] = new() { Terrain = TerrainType.Plains },
            [new HexCoord(-1, 0)] = new() { Terrain = TerrainType.Forest },
            [new HexCoord(0, -1)] = new() { Terrain = TerrainType.Hills }
        };
    }
}
