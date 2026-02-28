using Xunit;

namespace RealMK.Tests;

[Trait("Category", "Unit")]
public class CommandDispatcherTests
{
    [Fact]
    public void Dispatch_InitializeAndExplore_UsesRegisteredHandlers()
    {
        ContentDatabase content = TestDataFactory.CreateContentDatabase();
        var rng = new DeterministicRandom(123);
        var mapGenerator = new MapGenerator(content, rng, new SessionMapIdGenerator());
        var clock = new DeterministicGameClock();
        var indexes = new SequentialEventIndexProvider();

        var router = new CommandRouter();
        router.Register(new InitializeMapCommandHandler(mapGenerator, clock, indexes));
        router.Register(new ExploreTileCommandHandler(mapGenerator, clock, indexes));

        var bus = new EventBus();
        var dispatcher = new CommandDispatcher(bus, router);

        CommandResult init = dispatcher.Dispatch(new InitializeMapCommand { PlayerId = new PlayerId(0), SequenceNumber = 0 });
        CommandResult explore = dispatcher.Dispatch(new ExploreTileCommand
        {
            PlayerId = new PlayerId(0),
            SequenceNumber = 1,
            MacroCoord = new HexCoord(4, 0),
            Category = TileCategory.Core
        });

        Assert.True(init.IsSuccess);
        Assert.True(explore.IsSuccess);
        Assert.NotNull(init.Events);
        Assert.NotNull(explore.Events);
        Assert.Equal(2, init.Events!.Count);
        Assert.Single(explore.Events!);
    }
}
