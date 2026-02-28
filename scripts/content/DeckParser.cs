using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RealMK;

/// <summary>
/// Parses starter deck definitions from JSON content files.
/// </summary>
public sealed class DeckParser
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Parses all starter decks from a JSON file.
    /// </summary>
    public IReadOnlyList<StarterDeckDefinition> ParseFile(string filePath)
    {
        string json = File.ReadAllText(filePath);
        return ParseJson(json);
    }

    /// <summary>
    /// Parses all starter decks from JSON content.
    /// </summary>
    public IReadOnlyList<StarterDeckDefinition> ParseJson(string json)
    {
        DeckFileDto? file = JsonSerializer.Deserialize<DeckFileDto>(json, _jsonOptions);
        if (file?.StarterDecks == null)
        {
            throw new ContentParseException("Invalid deck file: missing 'starterDecks' array");
        }

        var results = new List<StarterDeckDefinition>(file.StarterDecks.Count);
        foreach (StarterDeckDto dto in file.StarterDecks)
        {
            results.Add(ParseDeck(dto));
        }

        return results;
    }

    private static StarterDeckDefinition ParseDeck(StarterDeckDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.HeroId))
        {
            throw new ContentParseException("Starter deck is missing required field 'heroId'");
        }

        if (dto.Entries == null || dto.Entries.Count == 0)
        {
            throw new ContentParseException($"Starter deck '{dto.HeroId}' must define non-empty 'entries'");
        }

        var entries = new List<StarterDeckEntry>(dto.Entries.Count);
        foreach (StarterDeckEntryDto entry in dto.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.CardId))
            {
                throw new ContentParseException($"Starter deck '{dto.HeroId}' contains entry missing required field 'cardId'");
            }

            if (entry.Count <= 0)
            {
                throw new ContentParseException($"Starter deck '{dto.HeroId}' entry '{entry.CardId}' has invalid count {entry.Count}");
            }

            entries.Add(new StarterDeckEntry
            {
                CardId = new CardId(entry.CardId),
                Count = entry.Count
            });
        }

        return new StarterDeckDefinition
        {
            HeroId = new HeroId(dto.HeroId),
            Entries = entries
        };
    }

    private sealed class DeckFileDto
    {
        [JsonPropertyName("starterDecks")]
        public List<StarterDeckDto>? StarterDecks { get; set; }
    }

    private sealed class StarterDeckDto
    {
        [JsonPropertyName("heroId")]
        public string? HeroId { get; set; }

        [JsonPropertyName("entries")]
        public List<StarterDeckEntryDto>? Entries { get; set; }
    }

    private sealed class StarterDeckEntryDto
    {
        [JsonPropertyName("cardId")]
        public string? CardId { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
