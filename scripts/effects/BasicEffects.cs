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
    public bool CanApply()
    {
        return true;
    }

    /// <inheritdoc/>
    public EffectResult Apply()
    {
        return EffectResult.Succeeded(Type, $"Gain {Points} movement ({MoveType})");
    }

    /// <inheritdoc/>
    public EffectPreview Preview()
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
    public bool CanApply()
    {
        return true;
    }

    /// <inheritdoc/>
    public EffectResult Apply()
    {
        return EffectResult.Succeeded(Type, $"Gain {Points} influence");
    }

    /// <inheritdoc/>
    public EffectPreview Preview()
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
    public bool CanApply()
    {
        return true;
    }

    /// <inheritdoc/>
    public EffectResult Apply()
    {
        return EffectResult.Succeeded(Type, $"{AttackType} attack {Value} ({Element})");
    }

    /// <inheritdoc/>
    public EffectPreview Preview()
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
    public bool CanApply()
    {
        return true;
    }

    /// <inheritdoc/>
    public EffectResult Apply()
    {
        string element = ElementalBlock.HasValue ? $" ({ElementalBlock.Value})" : string.Empty;
        return EffectResult.Succeeded(Type, $"Block {Value}{element}");
    }

    /// <inheritdoc/>
    public EffectPreview Preview()
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
    public bool CanApply()
    {
        return true;
    }

    /// <inheritdoc/>
    public EffectResult Apply()
    {
        return EffectResult.Succeeded(Type, $"Heal {Value}");
    }

    /// <inheritdoc/>
    public EffectPreview Preview()
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
    public bool CanApply()
    {
        return Effects.All(e => e.CanApply());
    }

    /// <inheritdoc/>
    public EffectResult Apply()
    {
        string summary = Mode == CompositeMode.Choice
            ? $"Choose one of {Effects.Count} effects"
            : $"Apply all {Effects.Count} effects";
        return EffectResult.Succeeded(Type, summary);
    }

    /// <inheritdoc/>
    public EffectPreview Preview()
    {
        int value = Effects.Sum(e => e.Preview().Value);
        string summary = Mode == CompositeMode.Choice
            ? $"Choose one of {Effects.Count} effects"
            : $"Apply all {Effects.Count} effects";

        return new EffectPreview
        {
            Type = Type,
            CanApply = CanApply(),
            Description = summary,
            Value = value
        };
    }
}
