using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Handles draw-cards command execution.
/// </summary>
public sealed class DrawCardsCommandHandler : ICommandHandler<DrawCardsCommand>
{
    private readonly GameState _state;
    private readonly IGameClock _clock;
    private readonly IEventIndexProvider _eventIndexes;

    /// <summary>
    /// Creates a draw-cards command handler.
    /// </summary>
    public DrawCardsCommandHandler(GameState state, IGameClock clock, IEventIndexProvider eventIndexes)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _eventIndexes = eventIndexes ?? throw new ArgumentNullException(nameof(eventIndexes));
    }

    /// <inheritdoc />
    public ValidationResult Validate(DrawCardsCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Count <= 0)
        {
            return ValidationResult.Invalid(ValidationErrorCodes.InvalidTarget, "Draw count must be positive");
        }

        if (!_state.TryGetPlayer(command.PlayerId, out _))
        {
            return ValidationResult.Invalid(ValidationErrorCodes.InvalidTarget, $"Player {command.PlayerId} not found");
        }

        if (_state.ActivePlayerId != command.PlayerId)
        {
            return ValidationResult.Invalid(ValidationErrorCodes.WrongPlayer, "Only active player can draw cards");
        }

        return ValidationResult.Success;
    }

    /// <inheritdoc />
    public IReadOnlyList<IGameEvent> Execute(DrawCardsCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        PlayerState player = _state.GetPlayer(command.PlayerId);

        IReadOnlyList<CardInstance> drawn = player.DrawCards(command.Count);
        if (drawn.Count == 0)
        {
            return [];
        }

        long now = _clock.NowTicks();
        return new IGameEvent[]
        {
            new EvtCardsDrawn
            {
                EventIndex = _eventIndexes.NextEventIndex(),
                Timestamp = now,
                PlayerId = player.Id,
                CardInstanceIds = drawn.Select(c => c.Id).ToArray()
            },
            new EvtCardsMoved
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
            }
        };
    }
}
