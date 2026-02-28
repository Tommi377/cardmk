using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RealMK.Tests;

[Trait("Category", "Unit")]
public class DeterminismTests
{
    [Fact]
    public void SameSeedAndCommandStream_ProducesSameEventSequenceAndMapSignature()
    {
        var first = RunScenario(seed: 99);
        var second = RunScenario(seed: 99);

        Assert.Equal(first.EventSignatures, second.EventSignatures);
        Assert.Equal(first.MapSignature, second.MapSignature);
    }

    [Fact]
    public void EventIndexes_AreMonotonicAndGapFree()
    {
        var run = RunScenario(seed: 7);

        Assert.Equal(new[] { 0, 1, 2 }, run.EventIndexes);
    }

    private static (List<string> EventSignatures, int[] EventIndexes, string MapSignature) RunScenario(ulong seed)
    {
        IGameSession session = new GameSession(TestDataFactory.CreateContentDatabase(), seed);

        var events = new List<IGameEvent>();
        session.EventBus.Subscribe<EvtTilePlaced>(e => events.Add(e));
        session.EventBus.Subscribe<EvtMapInitialized>(e => events.Add(e));

        TilePlacementResult init = session.InitializeMap();
        Assert.True(init.IsValid);

        TilePlacementResult? explore = session.ExploreTile(new HexCoord(4, 0), TileCategory.Core);
        Assert.NotNull(explore);
        Assert.True(explore!.IsValid);

        List<string> eventSignatures = events.Select(e =>
        {
            if (e is EvtTilePlaced tp)
            {
                return $"{e.EventIndex}:{e.Timestamp}:tile:{tp.Tile.Definition.Id}:{tp.IsStartingTile}";
            }

            if (e is EvtMapInitialized mi)
            {
                return $"{e.EventIndex}:{e.Timestamp}:init:{mi.StartingTileId}:{mi.CountrysideDeckSize}:{mi.CoreDeckSize}:{mi.CityDeckSize}";
            }

            return $"{e.EventIndex}:{e.Timestamp}:{e.GetType().Name}";
        }).ToList();

        int[] indexes = events.Select(e => e.EventIndex).ToArray();

        string mapSignature = string.Join("|", session.MapState.Tiles.Values
            .OrderBy(t => t.TileId.Value)
            .Select(t => $"{t.TileId.Value}:{t.Definition.Id}:{t.CenterPosition.Q},{t.CenterPosition.R}:{t.Rotation}"));

        return (eventSignatures, indexes, mapSignature);
    }
}
