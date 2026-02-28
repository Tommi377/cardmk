using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RealMK;

/// <summary>
/// Parses card definitions from JSON content files.
/// </summary>
public sealed class CardParser
{
    private readonly CardScriptRegistry _scriptRegistry;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Creates a card parser.
    /// </summary>
    /// <param name="scriptRegistry">Script registry used for scripted effect validation.</param>
    public CardParser(CardScriptRegistry? scriptRegistry = null)
    {
        _scriptRegistry = scriptRegistry ?? CardScriptRegistry.Default;
    }

    /// <summary>
    /// Parses all cards from a JSON file.
    /// </summary>
    public IReadOnlyList<CardDefinition> ParseFile(string filePath)
    {
        string json = File.ReadAllText(filePath);
        return ParseJson(json);
    }

    /// <summary>
    /// Parses all cards from JSON string.
    /// </summary>
    public IReadOnlyList<CardDefinition> ParseJson(string json)
    {
        CardFileDto? fileDto = JsonSerializer.Deserialize<CardFileDto>(json, _jsonOptions);

        if (fileDto?.Cards == null)
        {
            throw new ContentParseException("Invalid card file: missing 'cards' array");
        }

        var results = new List<CardDefinition>();
        foreach (CardDto dto in fileDto.Cards)
        {
            results.Add(ParseCard(dto));
        }
        return results;
    }

    private CardDefinition ParseCard(CardDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
        {
            throw new ContentParseException("Card is missing required 'id' field");
        }

        CardType cardType = ParseCardType(dto.Type, dto.Id);
        CardColor color = ParseCardColor(dto.Color, dto.Id);
        HeroId? heroSpecific = null;
        if (!string.IsNullOrWhiteSpace(dto.HeroSpecific))
        {
            heroSpecific = new HeroId(dto.HeroSpecific);
        }

        return new CardDefinition
        {
            Id = new CardId(dto.Id),
            Type = cardType,
            Color = color,
            NameKey = new LocalizationKey(dto.NameKey ?? $"{dto.Id}.name"),
            DescriptionKey = new LocalizationKey(dto.DescriptionKey ?? $"{dto.Id}.desc"),
            BasicEffect = dto.BasicEffect != null ? ParseEffect(dto.BasicEffect, dto.Id, "basicEffect") : null,
            EnhancedEffect = dto.EnhancedEffect != null ? ParseEffect(dto.EnhancedEffect, dto.Id, "enhancedEffect") : null,
            SidewaysEffect = dto.SidewaysEffect != null ? ParseEffect(dto.SidewaysEffect, dto.Id, "sidewaysEffect") : null,
            SidewaysValue = dto.SidewaysValue ?? 1,
            CanBePlacedSideways = dto.CanPlaySideways ?? true,
            HeroSpecific = heroSpecific
        };
    }

    private IEffect ParseEffect(EffectDto dto, string cardId, string path)
    {
        if (string.IsNullOrWhiteSpace(dto.Type))
        {
            throw new ContentParseException($"Card '{cardId}' effect '{path}' is missing required field 'type'");
        }

        return dto.Type.ToLowerInvariant() switch
        {
            "movement" => new MovementEffect
            {
                Points = dto.Value ?? 0,
                MoveType = ParseMovementType(dto.MovementType, cardId, path)
            },
            "influence" => new InfluenceEffect
            {
                Points = dto.Value ?? 0
            },
            "attack" => new AttackEffect
            {
                Value = dto.Value ?? 0,
                AttackType = ParseAttackType(dto.AttackType, cardId, path),
                Element = ParseElement(dto.Element, cardId, path)
            },
            "block" => new BlockEffect
            {
                Value = dto.Value ?? 0,
                ElementalBlock = string.IsNullOrWhiteSpace(dto.Element) ? null : ParseElement(dto.Element, cardId, path)
            },
            "healing" => new HealingEffect
            {
                Value = dto.Value ?? 0
            },
            "composite" => ParseCompositeEffect(dto, cardId, path),
            "script" => ParseScriptedEffect(dto, cardId, path),
            _ => throw new ContentParseException($"Card '{cardId}' effect '{path}' has unknown type '{dto.Type}'")
        };
    }

    private CompositeEffect ParseCompositeEffect(EffectDto dto, string cardId, string path)
    {
        if (dto.Components == null || dto.Components.Count == 0)
        {
            throw new ContentParseException($"Card '{cardId}' composite effect '{path}' must define non-empty 'components'");
        }

        var effects = new List<IEffect>();
        for (int i = 0; i < dto.Components.Count; i++)
        {
            effects.Add(ParseEffect(dto.Components[i], cardId, $"{path}.components[{i}]"));
        }

        return new CompositeEffect
        {
            Effects = effects,
            Mode = ParseCompositeMode(dto.Mode, cardId, path)
        };
    }

    private static CardType ParseCardType(string? type, string cardId) => type?.ToLowerInvariant() switch
    {
        "basic" => CardType.Basic,
        "advanced" => CardType.Advanced,
        "spell" => CardType.Spell,
        "wound" => CardType.Wound,
        "artifact" => CardType.Artifact,
        null or "" => throw new ContentParseException($"Card '{cardId}' is missing required field 'type'"),
        _ => throw new ContentParseException($"Card '{cardId}' has unknown card type '{type}'")
    };

    private static CardColor ParseCardColor(string? color, string cardId) => color?.ToLowerInvariant() switch
    {
        "red" => CardColor.Red,
        "blue" => CardColor.Blue,
        "green" => CardColor.Green,
        "white" => CardColor.White,
        "gold" => CardColor.Gold,
        "none" => CardColor.None,
        null or "" => throw new ContentParseException($"Card '{cardId}' is missing required field 'color'"),
        _ => throw new ContentParseException($"Card '{cardId}' has unknown color '{color}'")
    };

    private static AttackType ParseAttackType(string? type, string cardId, string path) => type?.ToLowerInvariant() switch
    {
        null or "" or "melee" => AttackType.Melee,
        "ranged" => AttackType.Ranged,
        "siege" => AttackType.Siege,
        _ => throw new ContentParseException($"Card '{cardId}' effect '{path}' has unknown attackType '{type}'")
    };

    private static Element ParseElement(string? element, string cardId, string path) => element?.ToLowerInvariant() switch
    {
        null or "" or "physical" => Element.Physical,
        "fire" => Element.Fire,
        "ice" => Element.Ice,
        "coldfire" => Element.ColdFire,
        _ => throw new ContentParseException($"Card '{cardId}' effect '{path}' has unknown element '{element}'")
    };

    private static MovementType ParseMovementType(string? type, string cardId, string path) => type?.ToLowerInvariant() switch
    {
        null or "" or "normal" => MovementType.Normal,
        "flying" => MovementType.Flying,
        "underground" => MovementType.Underground,
        _ => throw new ContentParseException($"Card '{cardId}' effect '{path}' has unknown movementType '{type}'")
    };

    private static CompositeMode ParseCompositeMode(string? mode, string cardId, string path) => mode?.ToLowerInvariant() switch
    {
        null or "" or "all" => CompositeMode.All,
        "choice" => CompositeMode.Choice,
        _ => throw new ContentParseException($"Card '{cardId}' effect '{path}' has unknown mode '{mode}'")
    };

    private ScriptedEffect ParseScriptedEffect(EffectDto dto, string cardId, string path)
    {
        if (string.IsNullOrWhiteSpace(dto.ScriptId))
        {
            throw new ContentParseException($"Card '{cardId}' effect '{path}' of type 'script' is missing required field 'scriptId'");
        }

        if (!_scriptRegistry.IsRegistered(dto.ScriptId))
        {
            throw new ContentParseException($"Card '{cardId}' effect '{path}' references unknown scriptId '{dto.ScriptId}'");
        }

        return new ScriptedEffect
        {
            ScriptId = dto.ScriptId,
            Parameters = dto.Params ?? new Dictionary<string, object>(),
            Registry = _scriptRegistry
        };
    }

    private sealed class CardFileDto
    {
        [JsonPropertyName("cards")]
        public List<CardDto>? Cards { get; set; }
    }

    private sealed class CardDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("color")]
        public string? Color { get; set; }

        [JsonPropertyName("nameKey")]
        public string? NameKey { get; set; }

        [JsonPropertyName("descriptionKey")]
        public string? DescriptionKey { get; set; }

        [JsonPropertyName("heroSpecific")]
        public string? HeroSpecific { get; set; }

        [JsonPropertyName("sidewaysValue")]
        public int? SidewaysValue { get; set; }

        [JsonPropertyName("canPlaySideways")]
        public bool? CanPlaySideways { get; set; }

        [JsonPropertyName("basicEffect")]
        public EffectDto? BasicEffect { get; set; }

        [JsonPropertyName("enhancedEffect")]
        public EffectDto? EnhancedEffect { get; set; }

        [JsonPropertyName("sidewaysEffect")]
        public EffectDto? SidewaysEffect { get; set; }
    }

    private sealed class EffectDto
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("value")]
        public int? Value { get; set; }

        [JsonPropertyName("attackType")]
        public string? AttackType { get; set; }

        [JsonPropertyName("element")]
        public string? Element { get; set; }

        [JsonPropertyName("movementType")]
        public string? MovementType { get; set; }

        [JsonPropertyName("mode")]
        public string? Mode { get; set; }

        [JsonPropertyName("components")]
        public List<EffectDto>? Components { get; set; }

        [JsonPropertyName("scriptId")]
        public string? ScriptId { get; set; }

        [JsonPropertyName("params")]
        public Dictionary<string, object>? Params { get; set; }
    }
}
