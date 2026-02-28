using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Handles end-turn command execution.
/// </summary>
public sealed class EndTurnCommandHandler : ICommandHandler<EndTurnCommand>
{
    private readonly GameState _state;
    private readonly IGameClock _clock;
    private readonly IEventIndexProvider _eventIndexes;

    /// <summary>
    /// Creates an end-turn command handler.
    /// </summary>
    public EndTurnCommandHandler(GameState state, IGameClock clock, IEventIndexProvider eventIndexes)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _eventIndexes = eventIndexes ?? throw new ArgumentNullException(nameof(eventIndexes));
    }

    /// <inheritdoc />
    public ValidationResult Validate(EndTurnCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (_state.ActivePlayerId != command.PlayerId)
        {
            return ValidationResult.Invalid(ValidationErrorCodes.WrongPlayer, "Only active player can end turn");
        }

        return ValidationResult.Success;
    }

    /// <inheritdoc />
    public IReadOnlyList<IGameEvent> Execute(EndTurnCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        PlayerState player = _state.GetPlayer(command.PlayerId);
        long now = _clock.NowTicks();
        var events = new List<IGameEvent>();

        IReadOnlyList<CardZoneChange> discarded = player.DiscardHandAndPlayArea();
        player.TurnResources.Clear();

        if (discarded.Count > 0)
        {
            events.Add(new EvtCardsMoved
            {
                    EventIndex = _eventIndexes.NextEventIndex(),
                    Timestamp = now,
                    PlayerId = player.Id,
                    Changes = discarded.ToArray()
                });
        }

        int drawCount = Math.Max(0, player.HandLimit - player.Hand.Count);
        IReadOnlyList<CardInstance> drawn = player.DrawCards(drawCount);
        if (drawn.Count > 0)
        {
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

        bool advanced = _state.AdvanceToNextPlayer();
        PlayerId? nextPlayer = advanced ? _state.ActivePlayerId : null;

        if (advanced)
        {
            _state.GetPlayer(_state.ActivePlayerId).StartTurn();
            _state.SetPhase(TurnPhase.PlayerAction);
        }
        else
        {
            _state.SetPhase(TurnPhase.RoundEnd);
        }

        events.Add(new EvtTurnEnded
        {
            EventIndex = _eventIndexes.NextEventIndex(),
            Timestamp = now,
            PlayerId = player.Id,
            NextPlayerId = nextPlayer,
            RoundEnded = !advanced
        });

        return events;
    }
}
