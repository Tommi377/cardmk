using Godot;
using Xunit;
using RealMK;

namespace RealMK.Tests;

/// <summary>
/// Tests for WorldMap coordinate conversion and atlas mapping.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Category", "Godot")]
public class WorldMapTests
{
    // -- GetLocationAtlasCoord tests --

    [Theory]
    [InlineData(LocationType.Village, 0, 1)]
    [InlineData(LocationType.Monastery, 1, 1)]
    public void GetLocationAtlasCoord_ReturnsCorrectCoordinates(LocationType locationType, int expectedX, int expectedY)
    {
        Vector2I result = WorldMap.GetLocationAtlasCoord(locationType);

        Assert.Equal(new Vector2I(expectedX, expectedY), result);
    }

    // -- HexToTileMapCoord / TileMapCoordToHex roundtrip tests --

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, -1)]
    [InlineData(3, -2)]
    [InlineData(-2, 4)]
    [InlineData(0, 1)]
    [InlineData(0, -1)]
    [InlineData(5, 3)]
    [InlineData(-3, -3)]
    public void HexToTileMapCoord_Roundtrip_PreservesCoordinates(int q, int r)
    {
        var hex = new HexCoord(q, r);

        Vector2I tileCoord = WorldMap.HexToTileMapCoord(hex);
        HexCoord result = WorldMap.TileMapCoordToHex(tileCoord);

        Assert.Equal(hex, result);
    }

    [Fact]
    public void HexToTileMapCoord_Origin_ReturnsZero()
    {
        var origin = new HexCoord(0, 0);

        Vector2I result = WorldMap.HexToTileMapCoord(origin);

        Assert.Equal(new Vector2I(0, 0), result);
    }
}
