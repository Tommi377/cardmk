using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Handles play-card command execution.
/// </summary>
public sealed class PlayCardCommandHandler : ICommandHandler<PlayCardCommand>
{
    private readonly GameState _state;
    private readonly ContentDatabase _content;
    private readonly IGameClock _clock;
    private readonly IEventIndexProvider _eventIndexes;

    /// <summary>
    /// Creates a play-card command handler.
    /// </summary>
    public PlayCardCommandHandler(GameState state, ContentDatabase content, IGameClock clock, IEventIndexProvider eventIndexes)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _content = content ?? throw new ArgumentNullException(nameof(content));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _eventIndexes = eventIndexes ?? throw new ArgumentNullException(nameof(eventIndexes));
    }

    /// <inheritdoc />
    public ValidationResult Validate(PlayCardCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        var builder = new ValidationResultBuilder();

        if (_state.ActivePlayerId != command.PlayerId)
        {
            builder.AddError(ValidationErrorCodes.WrongPlayer, "Only active player can play cards");
            return builder.Build();
        }

        PlayerState player = _state.GetPlayer(command.PlayerId);
        if (!player.TryGetCardInHand(command.CardInstanceId, out CardInstance? cardInstance) || cardInstance == null)
        {
            builder.AddError(ValidationErrorCodes.CardNotInHand, $"Card instance {command.CardInstanceId} is not in hand");
            return builder.Build();
        }

        CardDefinition cardDefinition = _content.GetCard(cardInstance.DefinitionId);
        if (cardDefinition.IsWound)
        {
            builder.AddError(ValidationErrorCodes.CannotPlayWound, $"Card '{cardDefinition.Id}' is a wound and cannot be played");
            return builder.Build();
        }

        IEffect? selectedEffect = GetSelectedEffect(cardDefinition, command.Mode);
        if (selectedEffect == null && command.Mode != CardPlayMode.Sideways)
        {
            builder.AddError(ValidationErrorCodes.InvalidTarget, $"Card '{cardDefinition.Id}' cannot be played in mode {command.Mode}");
            return builder.Build();
        }

        if (command.Mode == CardPlayMode.Sideways && !cardDefinition.CanBePlacedSideways)
        {
            builder.AddError(ValidationErrorCodes.InvalidTarget, $"Card '{cardDefinition.Id}' cannot be played sideways");
            return builder.Build();
        }

        var context = CreateEffectContext(player, cardDefinition, command.Mode, command.ResolutionInput);
        if (selectedEffect != null)
        {
            builder.Merge(selectedEffect.Validate(context, command.ResolutionInput, "root"));
        }
        else
        {
            builder.Merge(ValidateDefaultSideways(command, cardDefinition));
        }

        return builder.Build();
    }

    /// <inheritdoc />
    public IReadOnlyList<IGameEvent> Execute(PlayCardCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        PlayerState player = _state.GetPlayer(command.PlayerId);
        CardInstance cardInstance = _state.GetCardInstance(command.CardInstanceId);
        CardDefinition cardDefinition = _content.GetCard(cardInstance.DefinitionId);

        if (!player.PlayCard(cardInstance))
        {
            throw new InvalidOperationException($"Card instance {cardInstance.Id} could not be moved to play area");
        }

        var context = CreateEffectContext(player, cardDefinition, command.Mode, command.ResolutionInput);
        IEffect? selectedEffect = GetSelectedEffect(cardDefinition, command.Mode);
        string effectSummary;

        if (selectedEffect != null)
        {
            EffectResult effectResult = selectedEffect.Apply(context, command.ResolutionInput, "root");
            if (!effectResult.Success)
            {
                throw new InvalidOperationException($"Card effect failed: {effectResult.Description}");
            }

            effectSummary = effectResult.Description;
        }
        else
        {
            effectSummary = ApplyDefaultSideways(context, command.ResolutionInput, cardDefinition.SidewaysValue);
        }

        long now = _clock.NowTicks();
        var events = new List<IGameEvent>
        {
            new EvtCardsMoved
            {
                EventIndex = _eventIndexes.NextEventIndex(),
                Timestamp = now,
                PlayerId = player.Id,
                Changes = new[]
                {
                    new CardZoneChange
                    {
                        CardInstanceId = cardInstance.Id,
                        From = CardZone.Hand,
                        To = CardZone.PlayArea
                    }
                }
            },
            new EvtCardPlayed
            {
                EventIndex = _eventIndexes.NextEventIndex(),
                Timestamp = now,
                PlayerId = player.Id,
                CardInstanceId = cardInstance.Id,
                CardId = cardDefinition.Id,
                Mode = command.Mode,
                EffectSummary = effectSummary
            },
            new EvtTurnResourcesChanged
            {
                EventIndex = _eventIndexes.NextEventIndex(),
                Timestamp = now,
                PlayerId = player.Id,
                Movement = player.TurnResources.Movement,
                Attack = player.TurnResources.Attack,
                Block = player.TurnResources.Block,
                Influence = player.TurnResources.Influence,
                Healing = player.TurnResources.Healing
            }
        };

        events.AddRange(context.ScriptEvents);
        return events.OrderBy(e => e.EventIndex).ToArray();
    }

    private EffectContext CreateEffectContext(PlayerState player, CardDefinition card, CardPlayMode mode, CardResolutionInput input)
    {
        return new EffectContext(
            _state,
            player,
            card,
            mode,
            input,
            _eventIndexes.NextEventIndex,
            _clock.NowTicks);
    }

    private static IEffect? GetSelectedEffect(CardDefinition cardDefinition, CardPlayMode mode)
    {
        return mode switch
        {
            CardPlayMode.Basic => cardDefinition.BasicEffect,
            CardPlayMode.Enhanced => cardDefinition.EnhancedEffect,
            CardPlayMode.Sideways => cardDefinition.SidewaysEffect,
            _ => null
        };
    }

    private static ValidationResult ValidateDefaultSideways(PlayCardCommand command, CardDefinition cardDefinition)
    {
        if (command.ResolutionInput.TargetSelections.TryGetValue("sidewaysResource", out string? resource) &&
            IsValidSidewaysResource(resource))
        {
            return ValidationResult.Success;
        }

        return ValidationResult.Invalid(
            ValidationErrorCodes.InvalidResolutionChoice,
            $"Card '{cardDefinition.Id}' sideways play requires targetSelections['sidewaysResource'] set to movement/attack/block/influence");
    }

    private static string ApplyDefaultSideways(EffectContext context, CardResolutionInput resolutionInput, int value)
    {
        string resource = resolutionInput.TargetSelections["sidewaysResource"];
        switch (resource.ToLowerInvariant())
        {
            case "movement":
                context.AddMovement(value);
                return $"Gain {value} movement (sideways)";
            case "attack":
                context.AddAttack(value);
                return $"Gain {value} attack (sideways)";
            case "block":
                context.AddBlock(value);
                return $"Gain {value} block (sideways)";
            case "influence":
                context.AddInfluence(value);
                return $"Gain {value} influence (sideways)";
            default:
                throw new InvalidOperationException($"Invalid sideways resource '{resource}'");
        }
    }

    private static bool IsValidSidewaysResource(string? resource)
    {
        if (string.IsNullOrWhiteSpace(resource))
        {
            return false;
        }

        return resource.Equals("movement", StringComparison.OrdinalIgnoreCase) ||
               resource.Equals("attack", StringComparison.OrdinalIgnoreCase) ||
               resource.Equals("block", StringComparison.OrdinalIgnoreCase) ||
               resource.Equals("influence", StringComparison.OrdinalIgnoreCase);
    }
}
