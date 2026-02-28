using System;
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
}
