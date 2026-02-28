using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Helper methods for card play panel view logic.
/// </summary>
public static class CardPlayPanelHelpers
{
    /// <summary>
    /// Formats a hand card label for debug display.
    /// </summary>
    /// <param name="card">Card instance to format.</param>
    /// <returns>Debug label containing definition and instance ids.</returns>
    public static string FormatHandLabel(CardInstance card)
    {
        ArgumentNullException.ThrowIfNull(card);
        return $"{card.DefinitionId} | {card.Id}";
    }

    /// <summary>
    /// Builds a deterministic play-card request from UI values.
    /// </summary>
    /// <param name="playerId">Player issuing the play.</param>
    /// <param name="cardInstanceId">Card instance to play.</param>
    /// <param name="mode">Play mode.</param>
    /// <param name="rootChoiceIndex">Root choice index for composite effects.</param>
    /// <param name="sidewaysResource">Optional sideways resource target.</param>
    /// <returns>Play card request.</returns>
    public static PlayCardRequest BuildPlayRequest(
        PlayerId playerId,
        CardInstanceId cardInstanceId,
        CardPlayMode mode,
        int rootChoiceIndex,
        string? sidewaysResource)
    {
        var choiceSelections = new Dictionary<string, int>
        {
            ["root"] = rootChoiceIndex
        };

        var targetSelections = new Dictionary<string, string>();
        if (mode == CardPlayMode.Sideways && !string.IsNullOrWhiteSpace(sidewaysResource))
        {
            targetSelections["sidewaysResource"] = sidewaysResource;
        }

        return new PlayCardRequest
        {
            PlayerId = playerId,
            CardInstanceId = cardInstanceId,
            Mode = mode,
            ResolutionInput = new CardResolutionInput
            {
                ChoiceSelections = choiceSelections,
                TargetSelections = targetSelections
            }
        };
    }

    /// <summary>
    /// Trims event log lines to a maximum number of most recent entries.
    /// </summary>
    /// <param name="lines">Input log lines.</param>
    /// <param name="maxLines">Maximum number of lines to keep.</param>
    /// <returns>Trimmed log lines.</returns>
    public static IReadOnlyList<string> TrimEventLog(IEnumerable<string> lines, int maxLines = 30)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (maxLines <= 0)
        {
            return [];
        }

        string[] all = lines.ToArray();
        if (all.Length <= maxLines)
        {
            return all;
        }

        return all.Skip(all.Length - maxLines).ToArray();
    }
}
