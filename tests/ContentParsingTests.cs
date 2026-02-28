using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace RealMK.Tests;

[Trait("Category", "Unit")]
public class ContentParsingTests
{
    [Fact]
    public void CardParser_UnknownEffectType_ThrowsContentParseException()
    {
        string json = """
        {
          "cards": [
            {
              "id": "card.invalid",
              "type": "basic",
              "color": "green",
              "basicEffect": { "type": "teleport", "value": 2 }
            }
          ]
        }
        """;

        var parser = new CardParser();

        Assert.Throws<ContentParseException>(() => parser.ParseJson(json));
    }

    [Fact]
    public void TileParser_UnknownTerrain_ThrowsContentParseException()
    {
        string json = """
        {
          "tiles": [
            {
              "id": "tile.invalid",
              "category": "starting",
              "hexes": [
                { "q": 0, "r": 0, "terrain": "lava" }
              ]
            }
          ]
        }
        """;

        var parser = new TileParser();

        Assert.Throws<ContentParseException>(() => parser.ParseJson(json));
    }

    [Fact]
    public void ContentLoader_ValidationEnabled_ThrowsForInvalidTileLayout()
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), $"realmk-content-{Guid.NewGuid():N}");
        string cardsDir = Path.Combine(tempRoot, "cards");
        string tilesDir = Path.Combine(tempRoot, "tiles");
        Directory.CreateDirectory(cardsDir);
        Directory.CreateDirectory(tilesDir);

        try
        {
            File.WriteAllText(Path.Combine(cardsDir, "cards.json"), """
            {
              "cards": [
                {
                  "id": "card.valid",
                  "type": "basic",
                  "color": "green",
                  "basicEffect": { "type": "movement", "value": 2 }
                }
              ]
            }
            """);

            File.WriteAllText(Path.Combine(tilesDir, "tiles.json"), """
            {
              "tiles": [
                {
                  "id": "tile.invalid",
                  "category": "starting",
                  "hexes": [
                    { "q": 1, "r": 0, "terrain": "plains" }
                  ]
                }
              ]
            }
            """);

            var loader = new ContentLoader();
            Assert.Throws<ContentValidationException>(() => loader.LoadAll(tempRoot, validate: true));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void CardParser_UnknownScriptId_ThrowsContentParseException()
    {
        string json = """
        {
          "cards": [
            {
              "id": "card.invalid_script",
              "type": "basic",
              "color": "green",
              "basicEffect": {
                "type": "script",
                "scriptId": "script.does_not_exist",
                "params": { "amount": 1 }
              }
            }
          ]
        }
        """;

        var parser = new CardParser();

        Assert.Throws<ContentParseException>(() => parser.ParseJson(json));
    }

    [Fact]
    public void DeckParser_ValidDeck_ParsesEntries()
    {
        string json = """
        {
          "starterDecks": [
            {
              "heroId": "hero.default",
              "entries": [
                { "cardId": "card.test.move", "count": 2 },
                { "cardId": "card.test.attack", "count": 1 }
              ]
            }
          ]
        }
        """;

        var parser = new DeckParser();
        IReadOnlyList<StarterDeckDefinition> decks = parser.ParseJson(json);

        Assert.Single(decks);
        Assert.Equal(new HeroId("hero.default"), decks[0].HeroId);
        Assert.Equal(2, decks[0].Entries.Count);
    }

    [Fact]
    public void ContentValidation_InvalidStarterDeckCardReference_Throws()
    {
        var db = new ContentDatabase();
        db.AddCard(new CardDefinition
        {
            Id = new CardId("card.valid"),
            Type = CardType.Basic,
            Color = CardColor.Green,
            NameKey = new LocalizationKey("card.valid.name"),
            DescriptionKey = new LocalizationKey("card.valid.desc"),
            BasicEffect = new MovementEffect { Points = 2 }
        });

        db.AddTile(new TileDefinition
        {
            Id = new TileDefinitionId("tile.starting"),
            Category = TileCategory.Starting,
            NameKey = new LocalizationKey("tile.starting.name"),
            Hexes = new Dictionary<HexCoord, TileHexDefinition>
            {
                [new HexCoord(0, 0)] = new() { Terrain = TerrainType.Plains }
            }
        });

        db.AddStarterDeck(new StarterDeckDefinition
        {
            HeroId = new HeroId("hero.default"),
            Entries = new[]
            {
                new StarterDeckEntry { CardId = new CardId("card.unknown"), Count = 1 }
            }
        });

        var validator = new ContentValidationService();
        Assert.Throws<ContentValidationException>(() => validator.Validate(db));
    }
}
