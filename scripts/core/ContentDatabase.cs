using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;


/// <summary>
/// Central database containing all game content definitions.
/// Populated from content JSON files at startup.
/// </summary>
public sealed class ContentDatabase
{
    private readonly Dictionary<CardId, CardDefinition> _cards = new();
    // private readonly Dictionary<EnemyId, EnemyDefinition> _enemies = new();
    // private readonly Dictionary<UnitId, UnitDefinition> _units = new();
    // private readonly Dictionary<TileDefinitionId, TileDefinition> _tiles = new();
    // private readonly Dictionary<ScenarioId, ScenarioDefinition> _scenarios = new();
    // private readonly Dictionary<HeroId, HeroDefinition> _heroes = new();

    /// <summary>
    /// Creates an empty content database.
    /// </summary>
    public ContentDatabase()
    {
        Log.Debug("ContentDatabase created (empty)");
    }

    #region Cards

    /// <summary>
    /// All card definitions.
    /// </summary>
    public IReadOnlyDictionary<CardId, CardDefinition> Cards => _cards;

    /// <summary>
    /// Gets a card definition by ID.
    /// </summary>
    /// <exception cref="KeyNotFoundException">If card not found.</exception>
    public CardDefinition GetCard(CardId id)
    {
        if (!_cards.TryGetValue(id, out CardDefinition? card))
            throw new KeyNotFoundException($"Card '{id}' not found in content database");
        return card;
    }

    /// <summary>
    /// Tries to get a card definition by ID.
    /// </summary>
    public bool TryGetCard(CardId id, out CardDefinition? card)
    {
        return _cards.TryGetValue(id, out card);
    }

    /// <summary>
    /// Adds a card definition to the database.
    /// </summary>
    public void AddCard(CardDefinition card)
    {
        ArgumentNullException.ThrowIfNull(card);
        _cards[card.Id] = card;
        Log.Trace($"Added card: {card.Id}");
    }

    /// <summary>
    /// Adds multiple card definitions to the database.
    /// </summary>
    public void AddCards(IEnumerable<CardDefinition> cards)
    {
        foreach (CardDefinition card in cards)
        {
            AddCard(card);
        }
    }

    /// <summary>
    /// Gets all cards of a specific type.
    /// </summary>
    public IEnumerable<CardDefinition> GetCardsByType(CardType type)
    {
        return _cards.Values.Where(c => c.Type == type);
    }

    /// <summary>
    /// Gets all cards of a specific color.
    /// </summary>
    public IEnumerable<CardDefinition> GetCardsByColor(CardColor color)
    {
        return _cards.Values.Where(c => c.Color == color);
    }

    #endregion

    #region Enemies

    // TODO: Enemies
    /// <summary>
    /// All enemy definitions.
    /// </summary>
    // public IReadOnlyDictionary<EnemyId, EnemyDefinition> Enemies => _enemies;

    /// <summary>
    /// Gets an enemy definition by ID.
    /// </summary>
    /// <exception cref="KeyNotFoundException">If enemy not found.</exception>
    // public EnemyDefinition GetEnemy(EnemyId id)
    // {
    //     if (!_enemies.TryGetValue(id, out EnemyDefinition? enemy))
    //         throw new KeyNotFoundException($"Enemy '{id}' not found in content database");
    //     return enemy;
    // }

    /// <summary>
    /// Tries to get an enemy definition by ID.
    /// </summary>
    // public bool TryGetEnemy(EnemyId id, out EnemyDefinition? enemy)
    // {
    //     return _enemies.TryGetValue(id, out enemy);
    // }

    /// <summary>
    /// Adds an enemy definition to the database.
    /// </summary>
    // public void AddEnemy(EnemyDefinition enemy)
    // {
    //     ArgumentNullException.ThrowIfNull(enemy);
    //     _enemies[enemy.Id] = enemy;
    //     LoggerProvider.Current.Trace("Added enemy: {0}", enemy.Id);
    // }

    /// <summary>
    /// Adds multiple enemy definitions to the database.
    /// </summary>
    // public void AddEnemies(IEnumerable<EnemyDefinition> enemies)
    // {
    //     foreach (EnemyDefinition enemy in enemies)
    //     {
    //         AddEnemy(enemy);
    //     }
    // }

    /// <summary>
    /// Gets all enemies of a specific category.
    /// </summary>
    // public IEnumerable<EnemyDefinition> GetEnemiesByCategory(EnemyCategory category)
    // {
    //     return _enemies.Values.Where(e => e.Category == category);
    // }

    #endregion

    #region Units

    // TODO: Units
    /// <summary>
    /// All unit definitions.
    /// </summary>
    // public IReadOnlyDictionary<UnitId, UnitDefinition> Units => _units;

    /// <summary>
    /// Gets a unit definition by ID.
    /// </summary>
    /// <exception cref="KeyNotFoundException">If unit not found.</exception>
    // public UnitDefinition GetUnit(UnitId id)
    // {
    //     if (!_units.TryGetValue(id, out UnitDefinition? unit))
    //         throw new KeyNotFoundException($"Unit '{id}' not found in content database");
    //     return unit;
    // }

    /// <summary>
    /// Tries to get a unit definition by ID.
    /// </summary>
    // public bool TryGetUnit(UnitId id, out UnitDefinition? unit)
    // {
    //     return _units.TryGetValue(id, out unit);
    // }

    /// <summary>
    /// Adds a unit definition to the database.
    /// </summary>
    // public void AddUnit(UnitDefinition unit)
    // {
    //     ArgumentNullException.ThrowIfNull(unit);
    //     _units[unit.Id] = unit;
    //     LoggerProvider.Current.Trace("Added unit: {0}", unit.Id);
    // }

    /// <summary>
    /// Adds multiple unit definitions to the database.
    /// </summary>
    // public void AddUnits(IEnumerable<UnitDefinition> units)
    // {
    //     foreach (UnitDefinition unit in units)
    //     {
    //         AddUnit(unit);
    //     }
    // }

    /// <summary>
    /// Gets all units of a specific level.
    /// </summary>
    // public IEnumerable<UnitDefinition> GetUnitsByLevel(UnitLevel level)
    // {
    //     return _units.Values.Where(u => u.Level == level);
    // }

    #endregion

    #region Tiles

    // TODO: Map
    /// <summary>
    /// All tile definitions.
    /// </summary>
    // public IReadOnlyDictionary<TileDefinitionId, TileDefinition> Tiles => _tiles;

    /// <summary>
    /// Gets a tile definition by ID.
    /// </summary>
    /// <exception cref="KeyNotFoundException">If tile not found.</exception>
    // public TileDefinition GetTile(TileDefinitionId id)
    // {
    //     if (!_tiles.TryGetValue(id, out TileDefinition? tile))
    //         throw new KeyNotFoundException($"Tile '{id}' not found in content database");
    //     return tile;
    // }

    /// <summary>
    /// Tries to get a tile definition by ID.
    /// </summary>
    // public bool TryGetTile(TileDefinitionId id, out TileDefinition? tile)
    // {
    //     return _tiles.TryGetValue(id, out tile);
    // }

    /// <summary>
    /// Adds a tile definition to the database.
    /// </summary>
    // public void AddTile(TileDefinition tile)
    // {
    //     ArgumentNullException.ThrowIfNull(tile);
    //     _tiles[tile.Id] = tile;
    //     LoggerProvider.Current.Trace("Added tile: {0}", tile.Id);
    // }

    /// <summary>
    /// Adds multiple tile definitions to the database.
    /// </summary>
    // public void AddTiles(IEnumerable<TileDefinition> tiles)
    // {
    //     foreach (TileDefinition tile in tiles)
    //     {
    //         AddTile(tile);
    //     }
    // }

    /// <summary>
    /// Gets all tiles of a specific category.
    /// </summary>
    // public IEnumerable<TileDefinition> GetTilesByCategory(TileCategory category)
    // {
    //     return _tiles.Values.Where(t => t.Category == category);
    // }

    #endregion

    #region Scenarios

    // TODO: Scenarios
    /// <summary>
    /// All scenario definitions.
    /// </summary>
    // public IReadOnlyDictionary<ScenarioId, ScenarioDefinition> Scenarios => _scenarios;

    /// <summary>
    /// Gets a scenario definition by ID.
    /// </summary>
    /// <exception cref="KeyNotFoundException">If scenario not found.</exception>
    // public ScenarioDefinition GetScenario(ScenarioId id)
    // {
    //     if (!_scenarios.TryGetValue(id, out ScenarioDefinition? scenario))
    //         throw new KeyNotFoundException($"Scenario '{id}' not found in content database");
    //     return scenario;
    // }

    /// <summary>
    /// Tries to get a scenario definition by ID.
    /// </summary>
    // public bool TryGetScenario(ScenarioId id, out ScenarioDefinition? scenario)
    // {
    //     return _scenarios.TryGetValue(id, out scenario);
    // }

    /// <summary>
    /// Adds a scenario definition to the database.
    /// </summary>
    // public void AddScenario(ScenarioDefinition scenario)
    // {
    //     ArgumentNullException.ThrowIfNull(scenario);
    //     _scenarios[scenario.Id] = scenario;
    //     LoggerProvider.Current.Trace("Added scenario: {0}", scenario.Id);
    // }

    /// <summary>
    /// Adds multiple scenario definitions to the database.
    /// </summary>
    // public void AddScenarios(IEnumerable<ScenarioDefinition> scenarios)
    // {
    //     foreach (ScenarioDefinition scenario in scenarios)
    //     {
    //         AddScenario(scenario);
    //     }
    // }

    #endregion

    #region Heroes
    
    // TODO: Heroes
    /// <summary>
    /// All hero definitions.
    /// </summary>
    // public IReadOnlyDictionary<HeroId, HeroDefinition> Heroes => _heroes;

    /// <summary>
    /// Gets a hero definition by ID.
    /// </summary>
    /// <exception cref="KeyNotFoundException">If hero not found.</exception>
    // public HeroDefinition GetHero(HeroId id)
    // {
    //     if (!_heroes.TryGetValue(id, out HeroDefinition? hero))
    //         throw new KeyNotFoundException($"Hero '{id}' not found in content database");
    //     return hero;
    // }

    /// <summary>
    /// Tries to get a hero definition by ID.
    /// </summary>
    // public bool TryGetHero(HeroId id, out HeroDefinition? hero)
    // {
    //     return _heroes.TryGetValue(id, out hero);
    // }

    /// <summary>
    /// Adds a hero definition to the database.
    /// </summary>
    // public void AddHero(HeroDefinition hero)
    // {
    //     ArgumentNullException.ThrowIfNull(hero);
    //     _heroes[hero.Id] = hero;
    //     LoggerProvider.Current.Trace("Added hero: {0}", hero.Id);
    // }

    /// <summary>
    /// Adds multiple hero definitions to the database.
    /// </summary>

    #endregion

    #region Summary

    /// <summary>
    /// Gets a summary of the content database.
    /// </summary>
    public string GetSummary() {
        return $"ContentDatabase: {_cards.Count} cards";
        // return $"ContentDatabase: {_cards.Count} cards, {_enemies.Count} enemies, " +
        //        $"{_units.Count} units, {_tiles.Count} tiles, {_scenarios.Count} scenarios, " +
        //        $"{_heroes.Count} heroes";
    }

    /// <summary>
    /// Clears all content from the database.
    /// </summary>
    public void Clear()
    {
        _cards.Clear();
        // _enemies.Clear();
        // _units.Clear();
        // _tiles.Clear();
        // _scenarios.Clear();
        // _heroes.Clear();
        Log.Debug("ContentDatabase cleared");
    }

    #endregion
}
