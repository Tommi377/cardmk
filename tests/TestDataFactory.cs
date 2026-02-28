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

        db.AddCard(new CardDefinition
        {
            Id = new CardId("card.test.attack"),
            Type = CardType.Basic,
            Color = CardColor.Red,
            NameKey = new LocalizationKey("card.test.attack.name"),
            DescriptionKey = new LocalizationKey("card.test.attack.desc"),
            BasicEffect = new AttackEffect
            {
                Value = 2,
                AttackType = AttackType.Melee,
                Element = Element.Physical
            }
        });

        db.AddCard(new CardDefinition
        {
            Id = new CardId("card.test.flex"),
            Type = CardType.Basic,
            Color = CardColor.Green,
            NameKey = new LocalizationKey("card.test.flex.name"),
            DescriptionKey = new LocalizationKey("card.test.flex.desc"),
            SidewaysValue = 1,
            BasicEffect = new MovementEffect { Points = 1 },
            EnhancedEffect = new MovementEffect { Points = 3 }
        });

        db.AddCard(new CardDefinition
        {
            Id = new CardId("card.test.wound"),
            Type = CardType.Wound,
            Color = CardColor.None,
            NameKey = new LocalizationKey("card.test.wound.name"),
            DescriptionKey = new LocalizationKey("card.test.wound.desc"),
            CanBePlacedSideways = false
        });

        db.AddCard(new CardDefinition
        {
            Id = new CardId("card.test.script"),
            Type = CardType.Basic,
            Color = CardColor.Blue,
            NameKey = new LocalizationKey("card.test.script.name"),
            DescriptionKey = new LocalizationKey("card.test.script.desc"),
            BasicEffect = new ScriptedEffect
            {
                ScriptId = "script.gain_reputation",
                Parameters = new Dictionary<string, object> { ["amount"] = 2 }
            }
        });

        db.AddStarterDeck(new StarterDeckDefinition
        {
            HeroId = new HeroId("hero.default"),
            Entries = new[]
            {
                new StarterDeckEntry { CardId = new CardId("card.test.move"), Count = 6 },
                new StarterDeckEntry { CardId = new CardId("card.test.attack"), Count = 2 },
                new StarterDeckEntry { CardId = new CardId("card.test.flex"), Count = 1 },
                new StarterDeckEntry { CardId = new CardId("card.test.script"), Count = 1 },
                new StarterDeckEntry { CardId = new CardId("card.test.wound"), Count = 1 }
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
