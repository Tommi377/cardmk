using System.Collections.Generic;
using Xunit;

namespace RealMK.Tests;

[Trait("Category", "Unit")]
public class CardPlayPanelHelpersTests
{
    [Fact]
    public void FormatHandLabel_UsesDefinitionAndInstanceIds()
    {
        var card = new CardInstance(new CardInstanceId(12), new CardId("card.march"), new PlayerId(0), CardZone.Hand);

        string label = CardPlayPanelHelpers.FormatHandLabel(card);

        Assert.Equal("card.march | CardInst12", label);
    }

    [Fact]
    public void BuildPlayRequest_Sideways_SetsChoiceAndSidewaysResource()
    {
        PlayCardRequest request = CardPlayPanelHelpers.BuildPlayRequest(
            new PlayerId(0),
            new CardInstanceId(5),
            CardPlayMode.Sideways,
            rootChoiceIndex: 2,
            sidewaysResource: "attack");

        Assert.Equal(new PlayerId(0), request.PlayerId);
        Assert.Equal(new CardInstanceId(5), request.CardInstanceId);
        Assert.Equal(CardPlayMode.Sideways, request.Mode);
        Assert.True(request.ResolutionInput.ChoiceSelections.TryGetValue("root", out int choice));
        Assert.Equal(2, choice);
        Assert.True(request.ResolutionInput.TargetSelections.TryGetValue("sidewaysResource", out string? sideways));
        Assert.Equal("attack", sideways);
    }

    [Fact]
    public void BuildPlayRequest_Basic_DoesNotSetSidewaysResource()
    {
        PlayCardRequest request = CardPlayPanelHelpers.BuildPlayRequest(
            new PlayerId(0),
            new CardInstanceId(5),
            CardPlayMode.Basic,
            rootChoiceIndex: 0,
            sidewaysResource: "movement");

        Assert.Empty(request.ResolutionInput.TargetSelections);
        Assert.True(request.ResolutionInput.ChoiceSelections.ContainsKey("root"));
    }

    [Fact]
    public void TrimEventLog_KeepsMostRecentMaxLines()
    {
        var input = new List<string>();
        for (int i = 0; i < 40; i++)
        {
            input.Add($"line-{i}");
        }

        IReadOnlyList<string> trimmed = CardPlayPanelHelpers.TrimEventLog(input, maxLines: 30);

        Assert.Equal(30, trimmed.Count);
        Assert.Equal("line-10", trimmed[0]);
        Assert.Equal("line-39", trimmed[29]);
    }
}
