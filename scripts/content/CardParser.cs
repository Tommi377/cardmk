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
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

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
        if (string.IsNullOrEmpty(dto.Id))
        {
            throw new ContentParseException("Card is missing required 'id' field");
        }

        return new CardDefinition
        {
            Id = new CardId(dto.Id),
            Type = ParseCardType(dto.Type),
            Color = ParseCardColor(dto.Color),
            NameKey = new LocalizationKey(dto.NameKey ?? $"{dto.Id}.name"),
            DescriptionKey = new LocalizationKey(dto.DescriptionKey ?? $"{dto.Id}.desc"),
            BasicEffect = dto.BasicEffect != null ? ParseEffect(dto.BasicEffect) : null,
            EnhancedEffect = dto.EnhancedEffect != null ? ParseEffect(dto.EnhancedEffect) : null,
            SidewaysEffect = dto.SidewaysEffect != null ? ParseEffect(dto.SidewaysEffect) : null,
            SidewaysValue = dto.SidewaysValue ?? 1,
            CanBePlacedSideways = dto.CanPlaySideways ?? true,
            HeroSpecific = string.IsNullOrEmpty(dto.HeroSpecific) ? null! : new HeroId(dto.HeroSpecific)
        };
    }

    private IEffect ParseEffect(EffectDto dto)
    {
        // TODO: Effects
        return null!;
        // return dto.Type?.ToLowerInvariant() switch
        // {
        //     "movement" => new MovementEffect
        //     {
        //         Points = dto.Value ?? 0,
        //         MoveType = ParseMovementType(dto.MovementType)
        //     },
        //     "influence" => new InfluenceEffect
        //     {
        //         Points = dto.Value ?? 0
        //     },
        //     "attack" => new AttackEffect
        //     {
        //         Value = dto.Value ?? 0,
        //         AttackType = ParseAttackType(dto.AttackType),
        //         Element = ParseElement(dto.Element)
        //     },
        //     "block" => new BlockEffect
        //     {
        //         Value = dto.Value ?? 0,
        //         ElementalBlock = string.IsNullOrEmpty(dto.Element) ? null : ParseElement(dto.Element)
        //     },
        //     "healing" => new HealingEffect
        //     {
        //         Value = dto.Value ?? 0
        //     },
        //     "composite" => ParseCompositeEffect(dto),
        //     _ => throw new ContentParseException($"Unknown effect type: {dto.Type}")
        // };
    }

    // private CompositeEffect ParseCompositeEffect(EffectDto dto)
    // {
    //     if (dto.Components == null || dto.Components.Count == 0)
    //     {
    //         throw new ContentParseException("Composite effect must have 'components' array");
    //     }
    //
    //     var effects = new List<IEffect>();
    //     foreach (EffectDto componentDto in dto.Components)
    //     {
    //         effects.Add(ParseEffect(componentDto));
    //     }
    //
    //     return new CompositeEffect
    //     {
    //         Effects = effects,
    //         Mode = dto.Mode?.ToLowerInvariant() == "choice"
    //             ? CompositeMode.Choice
    //             : CompositeMode.All
    //     };
    // }

    private static CardType ParseCardType(string? type) => type?.ToLowerInvariant() switch
    {
        "basic" => CardType.Basic,
        "advanced" => CardType.Advanced,
        "spell" => CardType.Spell,
        "wound" => CardType.Wound,
        "artifact" => CardType.Artifact,
        _ => CardType.Basic
    };

    private static CardColor ParseCardColor(string? color) => color?.ToLowerInvariant() switch
    {
        "red" => CardColor.Red,
        "blue" => CardColor.Blue,
        "green" => CardColor.Green,
        "white" => CardColor.White,
        "gold" => CardColor.Gold,
        "none" => CardColor.None,
        _ => CardColor.None
    };

    private static AttackType ParseAttackType(string? type) => type?.ToLowerInvariant() switch
    {
        "melee" => AttackType.Melee,
        "ranged" => AttackType.Ranged,
        "siege" => AttackType.Siege,
        _ => AttackType.Melee
    };

    private static Element ParseElement(string? element) => element?.ToLowerInvariant() switch
    {
        "physical" => Element.Physical,
        "fire" => Element.Fire,
        "ice" => Element.Ice,
        "coldfire" => Element.ColdFire,
        _ => Element.Physical
    };

    private static MovementType ParseMovementType(string? type) => type?.ToLowerInvariant() switch
    {
        "normal" => MovementType.Normal,
        "flying" => MovementType.Flying,
        "underground" => MovementType.Underground,
        _ => MovementType.Normal
    };

    // ─────────────────────────────────────────────────────────────
    // DTOs for JSON deserialization
    // ─────────────────────────────────────────────────────────────

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
    }
}

