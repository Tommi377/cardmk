using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Validates loaded content definitions after parsing.
/// </summary>
public sealed class ContentValidationService
{
    private readonly CardScriptRegistry _scriptRegistry;

    /// <summary>
    /// Creates a content validation service.
    /// </summary>
    public ContentValidationService(CardScriptRegistry? scriptRegistry = null)
    {
        _scriptRegistry = scriptRegistry ?? CardScriptRegistry.Default;
    }

    /// <summary>
    /// Validates content database invariants.
    /// </summary>
    /// <exception cref="ContentValidationException">Thrown when one or more validation errors are found.</exception>
    public void Validate(ContentDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);

        var errors = new List<string>();

        foreach (CardDefinition card in database.Cards.Values)
        {
            bool hasAnyEffect = card.BasicEffect != null || card.EnhancedEffect != null || card.SidewaysEffect != null;
            if (!card.IsWound && !hasAnyEffect)
            {
                errors.Add($"Card '{card.Id}' must define at least one effect");
            }

            if (card.SidewaysValue < 0)
            {
                errors.Add($"Card '{card.Id}' has invalid negative sidewaysValue {card.SidewaysValue}");
            }

            ValidateEffectScriptReferences(card.BasicEffect, card.Id, "basicEffect", errors);
            ValidateEffectScriptReferences(card.EnhancedEffect, card.Id, "enhancedEffect", errors);
            ValidateEffectScriptReferences(card.SidewaysEffect, card.Id, "sidewaysEffect", errors);
        }

        foreach (StarterDeckDefinition starterDeck in database.StarterDecks.Values)
        {
            if (starterDeck.Entries.Count == 0)
            {
                errors.Add($"Starter deck for hero '{starterDeck.HeroId}' must contain at least one entry");
            }

            foreach (StarterDeckEntry entry in starterDeck.Entries)
            {
                if (entry.Count <= 0)
                {
                    errors.Add($"Starter deck for hero '{starterDeck.HeroId}' contains invalid count {entry.Count} for card '{entry.CardId}'");
                }

                if (!database.TryGetCard(entry.CardId, out _))
                {
                    errors.Add($"Starter deck for hero '{starterDeck.HeroId}' references unknown card '{entry.CardId}'");
                }
            }
        }

        foreach (TileDefinition tile in database.Tiles.Values)
        {
            if (tile.Hexes.Count == 0)
            {
                errors.Add($"Tile '{tile.Id}' must contain at least one hex");
            }

            if (!tile.Hexes.ContainsKey(new HexCoord(0, 0)))
            {
                errors.Add($"Tile '{tile.Id}' must contain center hex (0, 0)");
            }
        }

        if (errors.Count > 0)
        {
            throw new ContentValidationException(errors);
        }

        Log.Info($"Content validation passed ({database.Cards.Count} cards, {database.Tiles.Count} tiles)");
    }

    private void ValidateEffectScriptReferences(IEffect? effect, CardId cardId, string path, List<string> errors)
    {
        if (effect == null)
        {
            return;
        }

        if (effect is ScriptedEffect scripted)
        {
            if (!_scriptRegistry.IsRegistered(scripted.ScriptId))
            {
                errors.Add($"Card '{cardId}' effect '{path}' references unknown script id '{scripted.ScriptId}'");
            }

            return;
        }

        if (effect is CompositeEffect composite)
        {
            for (int i = 0; i < composite.Effects.Count; i++)
            {
                ValidateEffectScriptReferences(composite.Effects[i], cardId, $"{path}.components[{i}]", errors);
            }
        }
    }
}
