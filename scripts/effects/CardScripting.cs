using System;
using System.Collections.Generic;
using System.Text.Json;

namespace RealMK;

/// <summary>
/// Handles custom one-off scripted card behavior.
/// </summary>
public interface ICardScriptHandler
{
    /// <summary>
    /// Unique script id that maps content effects to this handler.
    /// </summary>
    string ScriptId { get; }

    /// <summary>
    /// Validates script parameters and resolution input for the current context.
    /// </summary>
    ValidationResult Validate(EffectContext context, IReadOnlyDictionary<string, object> parameters, string path);

    /// <summary>
    /// Applies script behavior to the current context.
    /// </summary>
    IReadOnlyList<IGameEvent> Apply(EffectContext context, IReadOnlyDictionary<string, object> parameters, string path);
}

/// <summary>
/// Explicit registry for all supported card script handlers.
/// </summary>
public sealed class CardScriptRegistry
{
    private readonly Dictionary<string, ICardScriptHandler> _handlers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Default registry used by content loading and runtime execution.
    /// </summary>
    public static CardScriptRegistry Default { get; } = CreateDefault();

    /// <summary>
    /// Registers a handler by its <see cref="ICardScriptHandler.ScriptId"/>.
    /// </summary>
    public void Register(ICardScriptHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handlers[handler.ScriptId] = handler;
    }

    /// <summary>
    /// Checks if a script id is registered.
    /// </summary>
    public bool IsRegistered(string scriptId)
    {
        return _handlers.ContainsKey(scriptId);
    }

    /// <summary>
    /// Tries to resolve a handler for a script id.
    /// </summary>
    public bool TryGet(string scriptId, out ICardScriptHandler? handler)
    {
        return _handlers.TryGetValue(scriptId, out handler);
    }

    private static CardScriptRegistry CreateDefault()
    {
        var registry = new CardScriptRegistry();
        registry.Register(new GainReputationScriptHandler());
        return registry;
    }
}

/// <summary>
/// Scripted effect wrapper resolved through <see cref="CardScriptRegistry"/>.
/// </summary>
public sealed class ScriptedEffect : IEffect
{
    /// <summary>
    /// Script id to execute.
    /// </summary>
    public string ScriptId { get; init; } = string.Empty;

    /// <summary>
    /// Script parameters from content.
    /// </summary>
    public IReadOnlyDictionary<string, object> Parameters { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Registry used for script handler lookup.
    /// </summary>
    public CardScriptRegistry Registry { get; init; } = CardScriptRegistry.Default;

    /// <inheritdoc />
    public EffectType Type => EffectType.Special;

    /// <inheritdoc />
    public ValidationResult Validate(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        if (!Registry.TryGet(ScriptId, out ICardScriptHandler? handler) || handler == null)
        {
            return ValidationResult.Invalid(
                ValidationErrorCodes.UnknownScriptId,
                $"Unknown script id '{ScriptId}' at {path}");
        }

        return handler.Validate(context, Parameters, path);
    }

    /// <inheritdoc />
    public EffectResult Apply(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        if (!Registry.TryGet(ScriptId, out ICardScriptHandler? handler) || handler == null)
        {
            return EffectResult.Failed(Type, $"Unknown script id '{ScriptId}'");
        }

        IReadOnlyList<IGameEvent> events = handler.Apply(context, Parameters, path);
        foreach (IGameEvent gameEvent in events)
        {
            context.AddScriptEvent(gameEvent);
        }

        return EffectResult.Succeeded(Type, $"Executed script '{ScriptId}'");
    }

    /// <inheritdoc />
    public EffectPreview Preview(EffectContext context, CardResolutionInput resolutionInput, string path)
    {
        return new EffectPreview
        {
            Type = Type,
            CanApply = Registry.IsRegistered(ScriptId),
            Description = $"Script: {ScriptId}",
            Value = 0
        };
    }
}

/// <summary>
/// Sample script handler that modifies player reputation.
/// </summary>
public sealed class GainReputationScriptHandler : ICardScriptHandler
{
    /// <inheritdoc />
    public string ScriptId => "script.gain_reputation";

    /// <inheritdoc />
    public ValidationResult Validate(EffectContext context, IReadOnlyDictionary<string, object> parameters, string path)
    {
        return TryReadInt(parameters, "amount", out _)
            ? ValidationResult.Success
            : ValidationResult.Invalid(ValidationErrorCodes.InvalidScriptParameters, $"Script '{ScriptId}' at {path} requires integer 'amount' parameter");
    }

    /// <inheritdoc />
    public IReadOnlyList<IGameEvent> Apply(EffectContext context, IReadOnlyDictionary<string, object> parameters, string path)
    {
        if (!TryReadInt(parameters, "amount", out int amount))
        {
            return [];
        }

        int before = context.Player.Reputation;
        context.AddReputation(amount);

        return new IGameEvent[]
        {
            new EvtPlayerReputationChanged
            {
                EventIndex = context.NextEventIndex(),
                Timestamp = context.NowTicks(),
                PlayerId = context.Player.Id,
                PreviousReputation = before,
                CurrentReputation = context.Player.Reputation
            }
        };
    }

    private static bool TryReadInt(IReadOnlyDictionary<string, object> parameters, string key, out int value)
    {
        value = 0;
        if (!parameters.TryGetValue(key, out object? raw) || raw == null)
        {
            return false;
        }

        if (raw is int intValue)
        {
            value = intValue;
            return true;
        }

        if (raw is long longValue)
        {
            value = (int)longValue;
            return true;
        }

        if (raw is JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.Number && json.TryGetInt32(out int number))
            {
                value = number;
                return true;
            }

            if (json.ValueKind == JsonValueKind.String && int.TryParse(json.GetString(), out int parsed))
            {
                value = parsed;
                return true;
            }
        }

        return false;
    }
}
