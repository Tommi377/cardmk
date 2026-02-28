using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Validates loaded content definitions after parsing.
/// </summary>
public sealed class ContentValidationService
{
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
}
