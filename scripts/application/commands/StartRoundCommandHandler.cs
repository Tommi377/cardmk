using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Handles round start command execution.
/// </summary>
public sealed class StartRoundCommandHandler : ICommandHandler<StartRoundCommand>
{
    private readonly GameState _state;
    private readonly IGameClock _clock;
    private readonly IEventIndexProvider _eventIndexes;

    /// <summary>
    /// Creates a start-round command handler.
    /// </summary>
    public StartRoundCommandHandler(GameState state, IGameClock clock, IEventIndexProvider eventIndexes)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _eventIndexes = eventIndexes ?? throw new ArgumentNullException(nameof(eventIndexes));
    }

    /// <inheritdoc />
    public ValidationResult Validate(StartRoundCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (_state.Players.Count == 0)
        {
            return ValidationResult.Invalid(ValidationErrorCodes.InvalidPhase, "Cannot start round without players");
        }

        bool validPhase = _state.GamePhase == GamePhase.Setup || _state.CurrentPhase == TurnPhase.RoundEnd || _state.CurrentPhase == TurnPhase.RoundStart;
        if (!validPhase)
        {
            return ValidationResult.Invalid(ValidationErrorCodes.InvalidPhase, $"Cannot start round during phase {_state.CurrentPhase}");
        }

        return ValidationResult.Success;
    }

    /// <inheritdoc />
    public IReadOnlyList<IGameEvent> Execute(StartRoundCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (_state.GamePhase == GamePhase.Setup)
        {
            _state.Start();
        }
        else
        {
            _state.AdvanceRound();
        }

        PlayerState firstActive = _state.GetActivePlayers().First();
        _state.SetActivePlayer(firstActive.Id);
        _state.SetPhase(TurnPhase.PlayerAction);

        long now = _clock.NowTicks();
        var events = new List<IGameEvent>
        {
            new EvtRoundStarted
            {
                EventIndex = _eventIndexes.NextEventIndex(),
                Timestamp = now,
                RoundNumber = _state.RoundNumber
            }
        };

        foreach (PlayerState player in _state.Players)
        {
            player.StartTurn();

            int reshuffled = player.ReshuffleDiscardIntoDraw(_state.Rng);
            if (reshuffled > 0)
            {
                events.Add(new EvtDeckReshuffled
                {
                    EventIndex = _eventIndexes.NextEventIndex(),
                    Timestamp = now,
                    PlayerId = player.Id,
                    CardCount = reshuffled
                });
            }

            int drawCount = Math.Max(0, player.HandLimit - player.Hand.Count);
            IReadOnlyList<CardInstance> drawn = player.DrawCards(drawCount);
            if (drawn.Count == 0)
            {
                continue;
            }

            events.Add(new EvtCardsDrawn
            {
                EventIndex = _eventIndexes.NextEventIndex(),
                Timestamp = now,
                PlayerId = player.Id,
                CardInstanceIds = drawn.Select(c => c.Id).ToArray()
            });

            events.Add(new EvtCardsMoved
            {
                EventIndex = _eventIndexes.NextEventIndex(),
                Timestamp = now,
                PlayerId = player.Id,
                Changes = drawn.Select(c => new CardZoneChange
                {
                    CardInstanceId = c.Id,
                    From = CardZone.DrawPile,
                    To = CardZone.Hand
                }).ToArray()
            });
        }

        return events;
    }
}
