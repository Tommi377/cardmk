using System;
using System.Collections.Generic;

namespace RealMK;



/// <summary>
/// Interface for all card effects.
/// Effects are the executable actions that cards provide.
/// </summary>
public interface IEffect
{
    /// <summary>
    /// The type category of this effect.
    /// </summary>
    EffectType Type { get; }

    /// <summary>
    /// Validates this effect in the given context.
    /// </summary>
    /// <param name="context">The current game context for effect resolution.</param>
    /// <param name="resolutionInput">Deterministic resolution input from the command.</param>
    /// <param name="path">Path for nested effect validation, e.g. root.0.</param>
    /// <returns>Validation result.</returns>
    ValidationResult Validate(EffectContext context, CardResolutionInput resolutionInput, string path);

    /// <summary>
    /// Applies this effect to the game state via the context.
    /// </summary>
    /// <param name="context">The context containing game state and accumulated values.</param>
    /// <param name="resolutionInput">Deterministic resolution input from the command.</param>
    /// <param name="path">Path for nested effect application, e.g. root.0.</param>
    /// <returns>The result of applying this effect.</returns>
    EffectResult Apply(EffectContext context, CardResolutionInput resolutionInput, string path);

    /// <summary>
    /// Generates a preview of what this effect would do without applying it.
    /// Used for UI tooltips and planning.
    /// </summary>
    /// <param name="context">The context for preview calculation.</param>
    /// <param name="resolutionInput">Deterministic resolution input from the command.</param>
    /// <param name="path">Path for nested preview calculation, e.g. root.0.</param>
    /// <returns>A preview showing the potential effect outcome.</returns>
    EffectPreview Preview(EffectContext context, CardResolutionInput resolutionInput, string path);
}

/// <summary>
/// The result of applying an effect.
/// </summary>
public class EffectResult
{
    /// <summary>Whether the effect was successfully applied.</summary>
    public bool Success { get; init; }

    /// <summary>Description of what happened for logging/display.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>The type of effect that was applied.</summary>
    public EffectType EffectType { get; init; }

    /// <summary>Detailed breakdown of how the effect was calculated.</summary>
    public EffectBreakdown? Breakdown { get; init; }

    public static EffectResult Succeeded(EffectType type, string description, EffectBreakdown? breakdown = null) =>
        new() { Success = true, EffectType = type, Description = description, Breakdown = breakdown };

    public static EffectResult Failed(EffectType type, string reason) =>
        new() { Success = false, EffectType = type, Description = reason };
}

/// <summary>
/// Detailed breakdown of effect calculation for explainability.
/// </summary>
public class EffectBreakdown
{
    /// <summary>Base value before modifiers.</summary>
    public int BaseValue { get; init; }

    /// <summary>List of modifiers applied (name, value pairs).</summary>
    public IReadOnlyList<(string Name, int Value)> Modifiers { get; init; } = [];

    /// <summary>Final calculated value after all modifiers.</summary>
    public int FinalValue { get; init; }

    /// <summary>
    /// Returns a human-readable explanation string.
    /// </summary>
    public string ToExplanationString()
    {
        if (Modifiers.Count == 0)
            return $"{BaseValue}";

        var parts = new List<string> { BaseValue.ToString() };
        foreach (var (name, value) in Modifiers)
        {
            string sign = value >= 0 ? "+" : "";
            parts.Add($"{sign}{value} ({name})");
        }
        parts.Add($"= {FinalValue}");

        return string.Join(" ", parts);
    }
}

/// <summary>
/// Preview of an effect for UI display.
/// </summary>
public class EffectPreview
{
    /// <summary>Type of effect being previewed.</summary>
    public EffectType Type { get; init; }

    /// <summary>Whether the effect can currently be applied.</summary>
    public bool CanApply { get; init; }

    /// <summary>Human-readable description of what the effect will do.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>The calculated value the effect would produce.</summary>
    public int Value { get; init; }

    /// <summary>Additional details for complex effects.</summary>
    public IReadOnlyDictionary<string, object>? Details { get; init; }
}

/// <summary>
/// Defines the type of effect for categorization and processing.
/// </summary>
public enum EffectType
{
    /// <summary>Adds movement points to the player's pool.</summary>
    Movement,

    /// <summary>Adds influence points for recruiting or interaction.</summary>
    Influence,

    /// <summary>Provides attack damage in combat.</summary>
    Attack,

    /// <summary>Provides block points to negate enemy attacks.</summary>
    Block,

    /// <summary>Heals wounds or units.</summary>
    Healing,

    /// <summary>Special effects that don't fit other categories.</summary>
    Special,

    /// <summary>Combines multiple effects together.</summary>
    Composite
}
