using System;
using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Handles map initialization command execution.
/// </summary>
public sealed class InitializeMapCommandHandler : ICommandHandler<InitializeMapCommand>
{
    private readonly MapGenerator _mapGenerator;
    private readonly IGameClock _clock;
    private readonly IEventIndexProvider _eventIndexes;

    /// <summary>
    /// Creates a map initialization command handler.
    /// </summary>
    public InitializeMapCommandHandler(MapGenerator mapGenerator, IGameClock clock, IEventIndexProvider eventIndexes)
    {
        _mapGenerator = mapGenerator ?? throw new ArgumentNullException(nameof(mapGenerator));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _eventIndexes = eventIndexes ?? throw new ArgumentNullException(nameof(eventIndexes));
    }

    /// <inheritdoc/>
    public ValidationResult Validate(InitializeMapCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (_mapGenerator.Map.TileCount > 0)
        {
            return ValidationResult.Invalid(ValidationErrorCodes.InvalidPhase, "Map has already been initialized");
        }

        return ValidationResult.Success;
    }

    /// <inheritdoc/>
    public IReadOnlyList<IGameEvent> Execute(InitializeMapCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        TilePlacementResult result = _mapGenerator.InitializeMap();
        if (!result.IsValid || result.Tile == null)
        {
            throw new InvalidOperationException($"Map initialization failed: {result.ErrorMessage}");
        }

        long now = _clock.NowTicks();

        return new IGameEvent[]
        {
            new EvtTilePlaced
            {
                EventIndex = _eventIndexes.NextEventIndex(),
                Timestamp = now,
                Tile = result.Tile,
                SpawnedEnemies = result.SpawnedEnemies,
                IsStartingTile = true
            },
            new EvtMapInitialized
            {
                EventIndex = _eventIndexes.NextEventIndex(),
                Timestamp = now,
                StartingTileId = result.Tile.TileId,
                CountrysideDeckSize = _mapGenerator.GetDeckCount(TileCategory.Countryside),
                CoreDeckSize = _mapGenerator.GetDeckCount(TileCategory.Core),
                CityDeckSize = _mapGenerator.GetDeckCount(TileCategory.City)
            }
        };
    }
}
