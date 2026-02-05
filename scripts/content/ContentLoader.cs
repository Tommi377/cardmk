using System;
using System.Collections.Generic;
using System.IO;

namespace RealMK;

/// <summary>
/// Loads game content from JSON files and populates a ContentDatabase.
/// </summary>
public sealed class ContentLoader
{
    private readonly CardParser _cardParser;
    // private readonly EnemyParser _enemyParser;
    // private readonly UnitParser _unitParser;
    // private readonly TileParser _tileParser;
    // private readonly ScenarioParser _scenarioParser;
    // private readonly HeroParser _heroParser;

    /// <summary>
    /// Creates a new content loader with default parsers.
    /// </summary>
    public ContentLoader()
    {
        _cardParser = new CardParser();
        // _enemyParser = new EnemyParser();
        // _unitParser = new UnitParser();
        // _tileParser = new TileParser();
        // _scenarioParser = new ScenarioParser();
        // _heroParser = new HeroParser();
    }

    /// <summary>
    /// Loads all content from the specified content directory.
    /// </summary>
    /// <param name="contentPath">Path to the content directory.</param>
    /// <param name="validate">Whether to validate references after loading.</param>
    /// <returns>A populated ContentDatabase.</returns>
    public ContentDatabase LoadAll(string contentPath, bool validate = true)
    {
        Log.Info($"Loading content from: {contentPath}");

        var database = new ContentDatabase();

        // Load each content type from their directories
        LoadCardsFromDirectory(Path.Combine(contentPath, "cards"), database);
        // LoadEnemiesFromDirectory(Path.Combine(contentPath, "enemies"), database);
        // LoadUnitsFromDirectory(Path.Combine(contentPath, "units"), database);
        // LoadTilesFromDirectory(Path.Combine(contentPath, "tiles"), database);
        // LoadScenariosFromDirectory(Path.Combine(contentPath, "scenarios"), database);
        // LoadHeroesFromDirectory(Path.Combine(contentPath, "heroes"), database);

        Log.Info($"Content loading complete: {database.Cards.Count} cards loaded");
        // Log.Info(
        //     "Content loaded: {0} cards, {1} enemies, {2} units, {3} tiles, {4} scenarios, {5} heroes",
        //     database.Cards.Count,
        //     database.Enemies.Count,
        //     database.Units.Count,
        //     database.Tiles.Count,
        //     database.Scenarios.Count,
        //     database.Heroes.Count);

        // if (validate)
        // {
        //     ValidateContentReferences(database);
        // }

        return database;
    }

    /// <summary>
    /// Loads card definitions from a directory.
    /// </summary>
    public void LoadCardsFromDirectory(string directoryPath, ContentDatabase database)
    {
        if (!Directory.Exists(directoryPath))
        {
            Log.Error($"Cards directory not found: {directoryPath}");
            return;
        }

        foreach (string file in Directory.GetFiles(directoryPath, "*.json"))
        {
            try
            {
                IReadOnlyList<CardDefinition> cards = _cardParser.ParseFile(file);
                database.AddCards(cards);
                Log.Debug($"Loaded {cards.Count} cards from {Path.GetFileName(file)}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load cards from {file}: {ex.Message}");
                throw new ContentParseException($"Failed to parse card file '{file}': {ex.Message}", ex);
            }
        }
    }

    // TODO: Enemies
    /// <summary>
    /// Loads enemy definitions from a directory.
    /// </summary>
    // public void LoadEnemiesFromDirectory(string directoryPath, ContentDatabase database)
    // {
    //     if (!Directory.Exists(directoryPath))
    //     {
    //         LoggerProvider.Current.Debug("Enemies directory not found: {0}", directoryPath);
    //         return;
    //     }
    //
    //     foreach (string file in Directory.GetFiles(directoryPath, "*.json"))
    //     {
    //         try
    //         {
    //             IReadOnlyList<EnemyDefinition> enemies = _enemyParser.ParseFile(file);
    //             database.AddEnemies(enemies);
    //             LoggerProvider.Current.Debug("Loaded {0} enemies from {1}", enemies.Count, Path.GetFileName(file));
    //         }
    //         catch (Exception ex)
    //         {
    //             LoggerProvider.Current.Error("Failed to load enemies from {0}: {1}", file, ex.Message);
    //             throw new ContentParseException($"Failed to parse enemy file '{file}': {ex.Message}", ex);
    //         }
    //     }
    // }

    // TODO: Units
    /// <summary>
    /// Loads unit definitions from a directory.
    /// </summary>
    // public void LoadUnitsFromDirectory(string directoryPath, ContentDatabase database)
    // {
    //     if (!Directory.Exists(directoryPath))
    //     {
    //         LoggerProvider.Current.Debug("Units directory not found: {0}", directoryPath);
    //         return;
    //     }
    //
    //     foreach (string file in Directory.GetFiles(directoryPath, "*.json"))
    //     {
    //         try
    //         {
    //             IReadOnlyList<UnitDefinition> units = _unitParser.ParseFile(file);
    //             database.AddUnits(units);
    //             LoggerProvider.Current.Debug("Loaded {0} units from {1}", units.Count, Path.GetFileName(file));
    //         }
    //         catch (Exception ex)
    //         {
    //             LoggerProvider.Current.Error("Failed to load units from {0}: {1}", file, ex.Message);
    //             throw new ContentParseException($"Failed to parse unit file '{file}': {ex.Message}", ex);
    //         }
    //     }
    // }

    // TODO: Map
    /// <summary>
    /// Loads tile definitions from a directory.
    /// </summary>
    // public void LoadTilesFromDirectory(string directoryPath, ContentDatabase database)
    // {
    //     if (!Directory.Exists(directoryPath))
    //     {
    //         LoggerProvider.Current.Debug("Tiles directory not found: {0}", directoryPath);
    //         return;
    //     }
    //
    //     foreach (string file in Directory.GetFiles(directoryPath, "*.json"))
    //     {
    //         try
    //         {
    //             IReadOnlyList<TileDefinition> tiles = _tileParser.ParseFile(file);
    //             database.AddTiles(tiles);
    //             LoggerProvider.Current.Debug("Loaded {0} tiles from {1}", tiles.Count, Path.GetFileName(file));
    //         }
    //         catch (Exception ex)
    //         {
    //             LoggerProvider.Current.Error("Failed to load tiles from {0}: {1}", file, ex.Message);
    //             throw new ContentParseException($"Failed to parse tile file '{file}': {ex.Message}", ex);
    //         }
    //     }
    // }

    // TODO: Scenarios
    /// <summary>
    /// Loads scenario definitions from a directory.
    /// </summary>
    // public void LoadScenariosFromDirectory(string directoryPath, ContentDatabase database)
    // {
    //     if (!Directory.Exists(directoryPath))
    //     {
    //         LoggerProvider.Current.Debug("Scenarios directory not found: {0}", directoryPath);
    //         return;
    //     }
    //
    //     foreach (string file in Directory.GetFiles(directoryPath, "*.json"))
    //     {
    //         try
    //         {
    //             IReadOnlyList<ScenarioDefinition> scenarios = _scenarioParser.ParseFile(file);
    //             database.AddScenarios(scenarios);
    //             LoggerProvider.Current.Debug("Loaded {0} scenarios from {1}", scenarios.Count, Path.GetFileName(file));
    //         }
    //         catch (Exception ex)
    //         {
    //             LoggerProvider.Current.Error("Failed to load scenarios from {0}: {1}", file, ex.Message);
    //             throw new ContentParseException($"Failed to parse scenario file '{file}': {ex.Message}", ex);
    //         }
    //     }
    // }

    // TODO: Hero
    /// <summary>
    /// Loads hero definitions from a directory.
    /// </summary>
    // public void LoadHeroesFromDirectory(string directoryPath, ContentDatabase database)
    // {
    //     if (!Directory.Exists(directoryPath))
    //     {
    //         LoggerProvider.Current.Debug("Heroes directory not found: {0}", directoryPath);
    //         return;
    //     }
    //
    //     foreach (string file in Directory.GetFiles(directoryPath, "*.json"))
    //     {
    //         try
    //         {
    //             IReadOnlyList<HeroDefinition> heroes = _heroParser.ParseFile(file);
    //             database.AddHeroes(heroes);
    //             LoggerProvider.Current.Debug("Loaded {0} heroes from {1}", heroes.Count, Path.GetFileName(file));
    //         }
    //         catch (Exception ex)
    //         {
    //             LoggerProvider.Current.Error("Failed to load heroes from {0}: {1}", file, ex.Message);
    //             throw new ContentParseException($"Failed to parse hero file '{file}': {ex.Message}", ex);
    //         }
    //     }
    // }

    // TODO: Do this once all loading is implemented, and make it more comprehensive
    /// <summary>
    /// Validates that all content references are valid.
    /// For example, ensures hero starting decks reference existing cards.
    /// </summary>
    /// <param name="database">The database to validate.</param>
    /// <exception cref="ContentValidationException">If validation errors are found.</exception>
    // public void ValidateContentReferences(ContentDatabase database)
    // {
    //     var errors = new List<string>();
    //
    //     // Validate hero starting decks reference existing cards
    //     foreach (HeroDefinition hero in database.Heroes.Values)
    //     {
    //         foreach (CardId cardId in hero.StartingDeck)
    //         {
    //             if (!database.TryGetCard(cardId, out _))
    //             {
    //                 errors.Add($"Hero '{hero.Id}' references unknown card '{cardId}' in starting deck");
    //             }
    //         }
    //     }
    //
    //     // Validate hero-specific cards reference existing heroes
    //     foreach (CardDefinition card in database.Cards.Values)
    //     {
    //         if (card.HeroSpecific.HasValue && !database.TryGetHero(card.HeroSpecific.Value, out _))
    //         {
    //             errors.Add($"Card '{card.Id}' references unknown hero '{card.HeroSpecific}'");
    //         }
    //     }
    //
    //     // Validate enemy summon targets reference existing enemies
    //     foreach (EnemyDefinition enemy in database.Enemies.Values)
    //     {
    //         foreach (EnemyAttack attack in enemy.Attacks)
    //         {
    //             if (attack.IsSummon && attack.SummonTarget.HasValue)
    //             {
    //                 if (!database.TryGetEnemy(attack.SummonTarget.Value, out _))
    //                 {
    //                     errors.Add($"Enemy '{enemy.Id}' summons unknown enemy '{attack.SummonTarget}'");
    //                 }
    //             }
    //         }
    //     }
    //
    //     // Validate scenario starting tiles reference existing tiles
    //     foreach (ScenarioDefinition scenario in database.Scenarios.Values)
    //     {
    //         if (scenario.MapSetup.StartingTileId.HasValue)
    //         {
    //             if (!database.TryGetTile(scenario.MapSetup.StartingTileId.Value, out _))
    //             {
    //                 errors.Add($"Scenario '{scenario.Id}' references unknown starting tile '{scenario.MapSetup.StartingTileId}'");
    //             }
    //         }
    //     }
    //
    //     if (errors.Count > 0)
    //     {
    //         LoggerProvider.Current.Error("Content validation found {0} error(s)", errors.Count);
    //         foreach (string error in errors)
    //         {
    //             LoggerProvider.Current.Error("  - {0}", error);
    //         }
    //         throw new ContentValidationException(errors);
    //     }
    //
    //     LoggerProvider.Current.Info("Content validation passed");
    // }

    /// <summary>
    /// Loads cards directly from JSON string.
    /// </summary>
    public IReadOnlyList<CardDefinition> LoadCardsFromJson(string json)
    {
        return _cardParser.ParseJson(json);
    }

    // TODO: Enemies
    /// <summary>
    /// Loads enemies directly from JSON string.
    /// </summary>
    // public IReadOnlyList<EnemyDefinition> LoadEnemiesFromJson(string json)
    // {
    //     return _enemyParser.ParseJson(json);
    // }

    // TODO: Units
    /// <summary>
    /// Loads units directly from JSON string.
    /// </summary>
    // public IReadOnlyList<UnitDefinition> LoadUnitsFromJson(string json)
    // {
    //     return _unitParser.ParseJson(json);
    // }

    // TODO: Map
    /// <summary>
    /// Loads tiles directly from JSON string.
    /// </summary>
    // public IReadOnlyList<TileDefinition> LoadTilesFromJson(string json)
    // {
    //     return _tileParser.ParseJson(json);
    // }

    // TODO: Scenarios
    /// <summary>
    /// Loads scenarios directly from JSON string.
    /// </summary>
    // public IReadOnlyList<ScenarioDefinition> LoadScenariosFromJson(string json)
    // {
    //     return _scenarioParser.ParseJson(json);
    // }

    // TODO: Hero
    /// <summary>
    /// Loads heroes directly from JSON string.
    /// </summary>
    // public IReadOnlyList<HeroDefinition> LoadHeroesFromJson(string json)
    // {
    //     return _heroParser.ParseJson(json);
    // }
}
