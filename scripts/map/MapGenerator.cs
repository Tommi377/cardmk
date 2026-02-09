using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Manages map generation following Mage Knight rules.
/// Maintains tile decks per category and handles tile placement during exploration.
/// </summary>
public sealed class MapGenerator
{
    private readonly ContentDatabase _content;
    private readonly TilePlacementService _placementService;
    private readonly DeterministicRandom _rng;

    private TileDeck? _countrysideDeck;
    private TileDeck? _coreDeck;
    private TileDeck? _cityDeck;

    /// <summary>
    /// The current map state being generated.
    /// </summary>
    public MapState Map { get; }

    /// <summary>
    /// Creates a new map generator.
    /// </summary>
    /// <param name="content">Content database with tile definitions.</param>
    /// <param name="rng">Deterministic random number generator.</param>
    public MapGenerator(ContentDatabase content, DeterministicRandom rng)
    {
        _content = content ?? throw new ArgumentNullException(nameof(content));
        _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        _placementService = new TilePlacementService();
        Map = new MapState();

        Log.Debug("MapGenerator created");
    }

    /// <summary>
    /// Initializes the map by placing the starting tile and shuffling decks.
    /// </summary>
    /// <returns>Result of the starting tile placement.</returns>
    public TilePlacementResult InitializeMap()
    {
        Log.Info("MapGenerator: Initializing map...");

        // Initialize tile decks
        InitializeDecks();

        // Find and place starting tile
        var startingTiles = _content.GetTilesByCategory(TileCategory.Starting).ToList();
        if (startingTiles.Count == 0)
        {
            Log.Error("MapGenerator: No starting tiles found in content database");
            return TilePlacementResult.Invalid("No starting tiles available");
        }

        // Pick a random starting tile (usually there's only one)
        TileDefinition startingTile = _rng.PickRandom(startingTiles);

        // Place at origin with no rotation
        TilePlacementResult result = _placementService.PlaceTile(Map, startingTile, new HexCoord(0, 0), _rng);

        if (result.IsValid)
        {
            Log.Info($"MapGenerator: Placed starting tile {startingTile.Id} at origin");

            // Place countryside tiles to the NE, NW, and W
            ExploreTile(new HexCoord(0, -1), TileCategory.Countryside); // NW
            ExploreTile(new HexCoord(1, -1), TileCategory.Countryside); // NE
            ExploreTile(new HexCoord(1, 0), TileCategory.Countryside); // E
        }
        else
        {
            Log.Error($"MapGenerator: Failed to place starting tile: {result.ErrorMessage}");
        }

        return result;
    }

    /// <summary>
    /// Explores a new tile at the given edge position.
    /// </summary>
    /// <param name="macroCoord">Coordinate on the map tile scale.</param>
    /// <param name="category">Category of tile to draw. If null, automatically determined.</param>
    /// <returns>Result of the tile placement.</returns>
    public TilePlacementResult ExploreTile(HexCoord macroCoord, TileCategory? category = null)
    {
        Log.Debug($"MapGenerator: Exploring tile at {macroCoord.ToMicroCoord()}({macroCoord}) direction");

        // Determine which deck to draw from
        TileCategory tileCategory = category ?? DetermineTileCategory(macroCoord);
        TileDefinition? tileDef = DrawTile(tileCategory);

        if (tileDef == null)
        {
            Log.Warning($"MapGenerator: No tiles available in {tileCategory} deck");
            return TilePlacementResult.Invalid($"No tiles available in {tileCategory} deck");
        }

        // Place the tile
        var result = _placementService.PlaceTile(
            Map,
            tileDef,
            macroCoord,
            _rng);

        if (result.IsValid)
        {
            Log.Info($"MapGenerator: Explored tile {tileDef.Id} at ({macroCoord})");
        }

        return result;
    }

    /// <summary>
    /// Gets all explorable edges on the current map.
    /// Returns hexes at the map boundary where new tiles can be placed.
    /// </summary>
    public IReadOnlyList<(HexCoord Position, int Direction)> GetExplorableEdges()
    {
        var edges = new List<(HexCoord, int)>();
        var visited = new HashSet<(HexCoord, int)>();

        foreach (var cell in Map.Tiles.Values.SelectMany(t => t.Cells.Values))
        {
            for (int dir = 0; dir < 6; dir++)
            {
                HexCoord neighbor = cell.WorldCoord.Neighbor(dir);
                if (!Map.HasHex(neighbor) && !visited.Contains((cell.WorldCoord, dir)))
                {
                    edges.Add((cell.WorldCoord, dir));
                    visited.Add((cell.WorldCoord, dir));
                }
            }
        }

        return edges;
    }

    /// <summary>
    /// Gets the number of tiles remaining in a deck.
    /// </summary>
    public int GetDeckCount(TileCategory category)
    {
        return category switch
        {
            TileCategory.Countryside => _countrysideDeck?.Count ?? 0,
            TileCategory.Core => _coreDeck?.Count ?? 0,
            TileCategory.City => _cityDeck?.Count ?? 0,
            _ => 0
        };
    }

    /// <summary>
    /// Draws a tile from the specified deck.
    /// </summary>
    /// <param name="category">The category deck to draw from.</param>
    /// <returns>The drawn tile definition, or null if deck is empty.</returns>
    public TileDefinition? DrawTile(TileCategory category)
    {
        return category switch
        {
            TileCategory.Countryside => _countrysideDeck?.Draw(),
            TileCategory.Core => _coreDeck?.Draw(),
            TileCategory.City => _cityDeck?.Draw(),
            _ => null
        };
    }

    /// <summary>
    /// Determines which tile category to use based on distance from origin.
    /// Follows Mage Knight exploration rules:
    /// - Near origin (distance 0-2): Countryside
    /// - Far from origin (distance 3+): Core
    /// </summary>
    private TileCategory DetermineTileCategory(HexCoord macroCoord)
    {
        int distance = macroCoord.DistanceTo(new HexCoord(0, 0));

        // Simple rule: countryside for first 2 rings, then core
        if (distance <= 2)
        {
            // If countryside deck is empty, fall back to core
            if (_countrysideDeck?.Count > 0)
                return TileCategory.Countryside;
        }

        // Default to core tiles for farther exploration
        if (_coreDeck?.Count > 0)
            return TileCategory.Core;

        // Fallback to whatever is available
        if (_countrysideDeck?.Count > 0)
            return TileCategory.Countryside;

        return TileCategory.City;
    }

    /// <summary>
    /// Initializes and shuffles all tile decks.
    /// </summary>
    private void InitializeDecks()
    {
        var countrysideTiles = _content.GetTilesByCategory(TileCategory.Countryside).ToList();
        var coreTiles = _content.GetTilesByCategory(TileCategory.Core).ToList();
        var cityTiles = _content.GetTilesByCategory(TileCategory.City).ToList();

        _countrysideDeck = new TileDeck(countrysideTiles, _rng);
        _coreDeck = new TileDeck(coreTiles, _rng);
        _cityDeck = new TileDeck(cityTiles, _rng);

        Log.Info($"MapGenerator: Initialized decks - Countryside: {_countrysideDeck.Count}, Core: {_coreDeck.Count}, City: {_cityDeck.Count}");
    }

    /// <summary>
    /// A shuffled deck of tiles for a specific category.
    /// </summary>
    private sealed class TileDeck
    {
        private readonly List<TileDefinition> _tiles;
        private int _index;

        public TileDeck(IEnumerable<TileDefinition> tiles, DeterministicRandom rng)
        {
            _tiles = new List<TileDefinition>(tiles);
            rng.Shuffle(_tiles);
            _index = 0;
        }

        /// <summary>
        /// Number of tiles remaining in the deck.
        /// </summary>
        public int Count => _tiles.Count - _index;

        /// <summary>
        /// Draws the next tile from the deck.
        /// </summary>
        /// <returns>The next tile, or null if deck is empty.</returns>
        public TileDefinition? Draw()
        {
            if (_index >= _tiles.Count)
                return null;

            return _tiles[_index++];
        }

        /// <summary>
        /// Peeks at the next tile without drawing it.
        /// </summary>
        public TileDefinition? Peek()
        {
            if (_index >= _tiles.Count)
                return null;

            return _tiles[_index];
        }
    }
}
