using Xunit;

namespace RealMK.Tests;

[Trait("Category", "Unit")]
public class ContentDatabaseTests
{
    [Fact]
    public void Clear_RemovesCardsAndTiles()
    {
        ContentDatabase database = TestDataFactory.CreateContentDatabase();
        Assert.NotEmpty(database.Cards);
        Assert.NotEmpty(database.StarterDecks);
        Assert.NotEmpty(database.Tiles);

        database.Clear();

        Assert.Empty(database.Cards);
        Assert.Empty(database.StarterDecks);
        Assert.Empty(database.Tiles);
    }
}
