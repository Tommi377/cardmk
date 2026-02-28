using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RealMK.Tests;

[Trait("Category", "Unit")]
public class CardSystemTests
{
    [Fact]
    public void StartRound_DrawsToHandLimit_AndPublishesRoundEvents()
    {
        IGameSession session = new GameSession(TestDataFactory.CreateContentDatabase(), seed: 123);

        CommandResult result = session.StartRound();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Events);
        Assert.Contains(result.Events!, e => e is EvtRoundStarted);
        Assert.Contains(result.Events!, e => e is EvtCardsDrawn);
        Assert.Equal(5, session.State.GetPlayer(new PlayerId(0)).Hand.Count);
        Assert.Equal(TurnPhase.PlayerAction, session.State.CurrentPhase);
    }

    [Fact]
    public void DrawCards_EmptyDrawPile_IsNoOp()
    {
        IGameSession session = new GameSession(TestDataFactory.CreateContentDatabase(), seed: 1);
        Assert.True(session.StartRound().IsSuccess);

        PlayerState player = session.State.GetPlayer(new PlayerId(0));
        player.InitializeDrawPile([]);

        CommandResult draw = session.DrawCards(player.Id, 3);
        Assert.True(draw.IsSuccess);
        Assert.NotNull(draw.Events);
        Assert.Empty(draw.Events!);
    }

    [Fact]
    public void PlayCard_Basic_MovesToPlayAreaAndAddsResources()
    {
        IGameSession session = new GameSession(TestDataFactory.CreateContentDatabase(), seed: 2);
        Assert.True(session.StartRound().IsSuccess);
        PlayerState player = session.State.GetPlayer(new PlayerId(0));

        CardInstance card = session.State.CreateCardInstance(player.Id, new CardId("card.test.move"), CardZone.Hand);
        player.AddToHand(card);

        CommandResult play = session.PlayCard(new PlayCardRequest
        {
            PlayerId = player.Id,
            CardInstanceId = card.Id,
            Mode = CardPlayMode.Basic
        });

        Assert.True(play.IsSuccess);
        Assert.Equal(CardZone.PlayArea, card.Zone);
        Assert.Equal(2, player.TurnResources.Movement);
        Assert.Contains(play.Events!, e => e is EvtCardPlayed);
        Assert.Contains(play.Events!, e => e is EvtTurnResourcesChanged);
    }

    [Fact]
    public void PlayCard_Enhanced_UsesEnhancedEffect()
    {
        IGameSession session = new GameSession(TestDataFactory.CreateContentDatabase(), seed: 3);
        Assert.True(session.StartRound().IsSuccess);
        PlayerState player = session.State.GetPlayer(new PlayerId(0));

        CardInstance card = session.State.CreateCardInstance(player.Id, new CardId("card.test.flex"), CardZone.Hand);
        player.AddToHand(card);

        CommandResult play = session.PlayCard(new PlayCardRequest
        {
            PlayerId = player.Id,
            CardInstanceId = card.Id,
            Mode = CardPlayMode.Enhanced
        });

        Assert.True(play.IsSuccess);
        Assert.Equal(3, player.TurnResources.Movement);
    }

    [Fact]
    public void PlayCard_Sideways_UsesDefaultResourceSelection()
    {
        IGameSession session = new GameSession(TestDataFactory.CreateContentDatabase(), seed: 4);
        Assert.True(session.StartRound().IsSuccess);
        PlayerState player = session.State.GetPlayer(new PlayerId(0));

        CardInstance card = session.State.CreateCardInstance(player.Id, new CardId("card.test.flex"), CardZone.Hand);
        player.AddToHand(card);

        CommandResult play = session.PlayCard(new PlayCardRequest
        {
            PlayerId = player.Id,
            CardInstanceId = card.Id,
            Mode = CardPlayMode.Sideways,
            ResolutionInput = new CardResolutionInput
            {
                TargetSelections = new Dictionary<string, string>
                {
                    ["sidewaysResource"] = "attack"
                }
            }
        });

        Assert.True(play.IsSuccess);
        Assert.Equal(1, player.TurnResources.Attack);
    }

    [Fact]
    public void PlayCard_Wound_IsRejected()
    {
        IGameSession session = new GameSession(TestDataFactory.CreateContentDatabase(), seed: 5);
        Assert.True(session.StartRound().IsSuccess);
        PlayerState player = session.State.GetPlayer(new PlayerId(0));

        CardInstance wound = session.State.CreateCardInstance(player.Id, new CardId("card.test.wound"), CardZone.Hand);
        player.AddToHand(wound);

        CommandResult play = session.PlayCard(new PlayCardRequest
        {
            PlayerId = player.Id,
            CardInstanceId = wound.Id,
            Mode = CardPlayMode.Basic
        });

        Assert.False(play.IsSuccess);
        Assert.Contains(play.Errors!, e => e.Code == ValidationErrorCodes.CannotPlayWound);
    }

    [Fact]
    public void PlayCard_ScriptEffect_ChangesReputation_AndPublishesScriptEvent()
    {
        IGameSession session = new GameSession(TestDataFactory.CreateContentDatabase(), seed: 6);
        Assert.True(session.StartRound().IsSuccess);
        PlayerState player = session.State.GetPlayer(new PlayerId(0));

        int before = player.Reputation;
        CardInstance scriptCard = session.State.CreateCardInstance(player.Id, new CardId("card.test.script"), CardZone.Hand);
        player.AddToHand(scriptCard);

        CommandResult play = session.PlayCard(new PlayCardRequest
        {
            PlayerId = player.Id,
            CardInstanceId = scriptCard.Id,
            Mode = CardPlayMode.Basic
        });

        Assert.True(play.IsSuccess);
        Assert.Equal(before + 2, player.Reputation);
        Assert.Contains(play.Events!, e => e is EvtPlayerReputationChanged);
    }

    [Fact]
    public void PlayCard_CompositeChoiceWithoutSelection_IsRejected()
    {
        ContentDatabase content = TestDataFactory.CreateContentDatabase();
        content.AddCard(new CardDefinition
        {
            Id = new CardId("card.test.choice"),
            Type = CardType.Basic,
            Color = CardColor.Blue,
            NameKey = new LocalizationKey("card.test.choice.name"),
            DescriptionKey = new LocalizationKey("card.test.choice.desc"),
            BasicEffect = new CompositeEffect
            {
                Mode = CompositeMode.Choice,
                Effects = new IEffect[]
                {
                    new MovementEffect { Points = 1 },
                    new AttackEffect { Value = 1, AttackType = AttackType.Melee, Element = Element.Physical }
                }
            }
        });

        IGameSession session = new GameSession(content, seed: 10);
        Assert.True(session.StartRound().IsSuccess);
        PlayerState player = session.State.GetPlayer(new PlayerId(0));

        CardInstance choiceCard = session.State.CreateCardInstance(player.Id, new CardId("card.test.choice"), CardZone.Hand);
        player.AddToHand(choiceCard);

        CommandResult play = session.PlayCard(new PlayCardRequest
        {
            PlayerId = player.Id,
            CardInstanceId = choiceCard.Id,
            Mode = CardPlayMode.Basic
        });

        Assert.False(play.IsSuccess);
        Assert.Contains(play.Errors!, e => e.Code == ValidationErrorCodes.MissingResolutionChoice);
    }

    [Fact]
    public void SequenceIsEnforced_AcrossMixedCommands()
    {
        IGameSession session = new GameSession(TestDataFactory.CreateContentDatabase(), seed: 7);
        Assert.True(session.InitializeMap().IsValid);
        Assert.True(session.StartRound().IsSuccess);

        CommandResult outOfOrder = session.Dispatch(new DrawCardsCommand
        {
            PlayerId = new PlayerId(0),
            SequenceNumber = 99,
            Count = 1
        });

        Assert.False(outOfOrder.IsSuccess);
        Assert.Contains(outOfOrder.Errors!, e => e.Code == ValidationErrorCodes.InvalidSequence);
    }

    [Fact]
    public void EndTurn_SinglePlayer_TransitionsToRoundEnd()
    {
        IGameSession session = new GameSession(TestDataFactory.CreateContentDatabase(), seed: 8);
        Assert.True(session.StartRound().IsSuccess);

        CommandResult endTurn = session.EndTurn(new PlayerId(0));

        Assert.True(endTurn.IsSuccess);
        Assert.Equal(TurnPhase.RoundEnd, session.State.CurrentPhase);
        EvtTurnEnded evt = Assert.IsType<EvtTurnEnded>(endTurn.Events!.Last());
        Assert.True(evt.RoundEnded);
        Assert.Null(evt.NextPlayerId);
    }
}
