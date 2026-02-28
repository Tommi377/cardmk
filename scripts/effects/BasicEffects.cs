using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Whether composite effects execute all sub-effects or allow choosing one.
/// </summary>
public enum CompositeMode
{
    All,
    Choice
}

/// <summary>
/// Movement card effect.
/// </summary>
public sealed class MovementEffect : IEffect
{
    /// <summary>
    /// Movement points provided by this effect.
    /// </summary>
    public int Points { get; init; }

    /// <summary>
    /// Movement type.
    /// </summary>
    public MovementType MoveType { get; init; } = MovementType.Normal;

    /// <inheritdoc/>
    public EffectType Type => EffectType.Movement;

    /// <inheritdoc/>
    public ValidationResult Validate(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        return ValidationResult.Success;
    }

    /// <inheritdoc/>
    public EffectResult Apply(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        context.AddMovement(Points);
        return EffectResult.Succeeded(Type, $"Gain {Points} movement ({MoveType})");
    }

    /// <inheritdoc/>
    public EffectPreview Preview(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        return new EffectPreview
        {
            Type = Type,
            CanApply = true,
            Description = $"Gain {Points} movement ({MoveType})",
            Value = Points
        };
    }
}

/// <summary>
/// Influence card effect.
/// </summary>
public sealed class InfluenceEffect : IEffect
{
    /// <summary>
    /// Influence points provided by this effect.
    /// </summary>
    public int Points { get; init; }

    /// <inheritdoc/>
    public EffectType Type => EffectType.Influence;

    /// <inheritdoc/>
    public ValidationResult Validate(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        return ValidationResult.Success;
    }

    /// <inheritdoc/>
    public EffectResult Apply(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        context.AddInfluence(Points);
        return EffectResult.Succeeded(Type, $"Gain {Points} influence");
    }

    /// <inheritdoc/>
    public EffectPreview Preview(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        return new EffectPreview
        {
            Type = Type,
            CanApply = true,
            Description = $"Gain {Points} influence",
            Value = Points
        };
    }
}

/// <summary>
/// Attack card effect.
/// </summary>
public sealed class AttackEffect : IEffect
{
    /// <summary>
    /// Attack strength value.
    /// </summary>
    public int Value { get; init; }

    /// <summary>
    /// Attack type (melee, ranged, siege).
    /// </summary>
    public AttackType AttackType { get; init; } = AttackType.Melee;

    /// <summary>
    /// Attack element.
    /// </summary>
    public Element Element { get; init; } = Element.Physical;

    /// <inheritdoc/>
    public EffectType Type => EffectType.Attack;

    /// <inheritdoc/>
    public ValidationResult Validate(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        return ValidationResult.Success;
    }

    /// <inheritdoc/>
    public EffectResult Apply(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        context.AddAttack(Value);
        return EffectResult.Succeeded(Type, $"{AttackType} attack {Value} ({Element})");
    }

    /// <inheritdoc/>
    public EffectPreview Preview(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        return new EffectPreview
        {
            Type = Type,
            CanApply = true,
            Description = $"{AttackType} attack {Value} ({Element})",
            Value = Value
        };
    }
}

/// <summary>
/// Block card effect.
/// </summary>
public sealed class BlockEffect : IEffect
{
    /// <summary>
    /// Block value.
    /// </summary>
    public int Value { get; init; }

    /// <summary>
    /// Optional element restriction for block.
    /// </summary>
    public Element? ElementalBlock { get; init; }

    /// <inheritdoc/>
    public EffectType Type => EffectType.Block;

    /// <inheritdoc/>
    public ValidationResult Validate(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        return ValidationResult.Success;
    }

    /// <inheritdoc/>
    public EffectResult Apply(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        context.AddBlock(Value);
        string element = ElementalBlock.HasValue ? $" ({ElementalBlock.Value})" : string.Empty;
        return EffectResult.Succeeded(Type, $"Block {Value}{element}");
    }

    /// <inheritdoc/>
    public EffectPreview Preview(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        string element = ElementalBlock.HasValue ? $" ({ElementalBlock.Value})" : string.Empty;
        return new EffectPreview
        {
            Type = Type,
            CanApply = true,
            Description = $"Block {Value}{element}",
            Value = Value
        };
    }
}

/// <summary>
/// Healing card effect.
/// </summary>
public sealed class HealingEffect : IEffect
{
    /// <summary>
    /// Healing amount.
    /// </summary>
    public int Value { get; init; }

    /// <inheritdoc/>
    public EffectType Type => EffectType.Healing;

    /// <inheritdoc/>
    public ValidationResult Validate(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        return ValidationResult.Success;
    }

    /// <inheritdoc/>
    public EffectResult Apply(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        context.AddHealing(Value);
        return EffectResult.Succeeded(Type, $"Heal {Value}");
    }

    /// <inheritdoc/>
    public EffectPreview Preview(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        return new EffectPreview
        {
            Type = Type,
            CanApply = true,
            Description = $"Heal {Value}",
            Value = Value
        };
    }
}

/// <summary>
/// Composite effect made of multiple child effects.
/// </summary>
public sealed class CompositeEffect : IEffect
{
    /// <summary>
    /// Child effects.
    /// </summary>
    public IReadOnlyList<IEffect> Effects { get; init; } = [];

    /// <summary>
    /// Composite execution mode.
    /// </summary>
    public CompositeMode Mode { get; init; } = CompositeMode.All;

    /// <inheritdoc/>
    public EffectType Type => EffectType.Composite;

    /// <inheritdoc/>
    public ValidationResult Validate(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        if (Mode == CompositeMode.Choice)
        {
            if (!resolutionInput.TryGetChoice(path, out int selectedIndex))
            {
                return ValidationResult.Invalid(
                    ValidationErrorCodes.MissingResolutionChoice,
                    $"Missing choice selection for '{path}'");
            }

            if (selectedIndex < 0 || selectedIndex >= Effects.Count)
            {
                return ValidationResult.Invalid(
                    ValidationErrorCodes.InvalidResolutionChoice,
                    $"Choice index {selectedIndex} is out of range for '{path}'");
            }

            return Effects[selectedIndex].Validate(context, resolutionInput, $"{path}.{selectedIndex}");
        }

        var builder = new ValidationResultBuilder();
        for (int i = 0; i < Effects.Count; i++)
        {
            builder.Merge(Effects[i].Validate(context, resolutionInput, $"{path}.{i}"));
        }

        return builder.Build();
    }

    /// <inheritdoc/>
    public EffectResult Apply(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        if (Mode == CompositeMode.Choice)
        {
            int selectedIndex = resolutionInput.ChoiceSelections[path];
            return Effects[selectedIndex].Apply(context, resolutionInput, $"{path}.{selectedIndex}");
        }

        var descriptions = new List<string>(Effects.Count);
        for (int i = 0; i < Effects.Count; i++)
        {
            EffectResult result = Effects[i].Apply(context, resolutionInput, $"{path}.{i}");
            if (!result.Success)
            {
                return result;
            }

            descriptions.Add(result.Description);
        }

        string summary = Mode == CompositeMode.Choice
            ? $"Choose one of {Effects.Count} effects"
            : $"Apply all {Effects.Count} effects";
        string description = descriptions.Count == 0 ? summary : $"{summary}: {string.Join(", ", descriptions)}";
        return EffectResult.Succeeded(Type, description);
    }

    /// <inheritdoc/>
    public EffectPreview Preview(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        int value;
        if (Mode == CompositeMode.Choice && resolutionInput.TryGetChoice(path, out int selectedIndex) && selectedIndex >= 0 && selectedIndex < Effects.Count)
        {
            value = Effects[selectedIndex].Preview(context, resolutionInput, $"{path}.{selectedIndex}").Value;
        }
        else
        {
            value = 0;
            for (int i = 0; i < Effects.Count; i++)
            {
                value += Effects[i].Preview(context, resolutionInput, $"{path}.{i}").Value;
            }
        }

        string summary = Mode == CompositeMode.Choice
            ? $"Choose one of {Effects.Count} effects"
            : $"Apply all {Effects.Count} effects";

        return new EffectPreview
        {
            Type = Type,
            CanApply = Validate(context, resolutionInput, path).IsValid,
            Description = summary,
            Value = value
        };
    }
}
