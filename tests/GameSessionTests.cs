using System.Collections.Generic;
using Xunit;

namespace RealMK.Tests;

[Trait("Category", "Unit")]
public class GameSessionTests
{
    [Fact]
    public void InitializeMap_PublishesStartingEventsWithMonotonicIndexes()
    {
        ContentDatabase content = TestDataFactory.CreateContentDatabase();
        IGameSession session = new GameSession(content, seed: 42);

        var events = new List<IGameEvent>();
        session.EventBus.Subscribe<EvtTilePlaced>(e => events.Add(e));
        session.EventBus.Subscribe<EvtMapInitialized>(e => events.Add(e));

        TilePlacementResult result = session.InitializeMap();

        Assert.True(result.IsValid);
        Assert.NotNull(result.Tile);
        Assert.Equal(2, events.Count);
        Assert.IsType<EvtTilePlaced>(events[0]);
        Assert.IsType<EvtMapInitialized>(events[1]);
        Assert.Equal(0, events[0].EventIndex);
        Assert.Equal(1, events[1].EventIndex);
        Assert.True(events[1].Timestamp >= events[0].Timestamp);
        Assert.Equal(4, session.MapState.TileCount);
    }

    [Fact]
    public void ExploreTile_UsesCommandPipelineAndContinuesEventIndexes()
    {
        ContentDatabase content = TestDataFactory.CreateContentDatabase();
        IGameSession session = new GameSession(content, seed: 1);

        var placed = new List<EvtTilePlaced>();
        session.EventBus.Subscribe<EvtTilePlaced>(e => placed.Add(e));

        TilePlacementResult init = session.InitializeMap();
        Assert.True(init.IsValid);

        TilePlacementResult? explore = session.ExploreTile(new HexCoord(4, 0), TileCategory.Core);

        Assert.NotNull(explore);
        Assert.True(explore!.IsValid);
        Assert.NotNull(explore.Tile);
        Assert.Equal(2, placed.Count);
        Assert.Equal(0, placed[0].EventIndex);
        Assert.Equal(2, placed[1].EventIndex);
    }
}
