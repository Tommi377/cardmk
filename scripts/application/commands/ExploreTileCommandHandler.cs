using System;
using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Handles map exploration command execution.
/// </summary>
public sealed class ExploreTileCommandHandler : ICommandHandler<ExploreTileCommand>
{
    private readonly MapGenerator _mapGenerator;
    private readonly IGameClock _clock;
    private readonly IEventIndexProvider _eventIndexes;

    /// <summary>
    /// Creates an explore tile command handler.
    /// </summary>
    public ExploreTileCommandHandler(MapGenerator mapGenerator, IGameClock clock, IEventIndexProvider eventIndexes)
    {
        _mapGenerator = mapGenerator ?? throw new ArgumentNullException(nameof(mapGenerator));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _eventIndexes = eventIndexes ?? throw new ArgumentNullException(nameof(eventIndexes));
    }

    /// <inheritdoc/>
    public ValidationResult Validate(ExploreTileCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (_mapGenerator.Map.TileCount == 0)
        {
            return ValidationResult.Invalid(ValidationErrorCodes.InvalidPhase, "Map must be initialized before exploration");
        }

        return ValidationResult.Success;
    }

    /// <inheritdoc/>
    public IReadOnlyList<IGameEvent> Execute(ExploreTileCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        TilePlacementResult result = _mapGenerator.ExploreTile(command.MacroCoord, command.Category);
        if (!result.IsValid || result.Tile == null)
        {
            throw new InvalidOperationException($"Tile exploration failed: {result.ErrorMessage}");
        }

        return new IGameEvent[]
        {
            new EvtTilePlaced
            {
                EventIndex = _eventIndexes.NextEventIndex(),
                Timestamp = _clock.NowTicks(),
                Tile = result.Tile,
                SpawnedEnemies = result.SpawnedEnemies,
                IsStartingTile = false
            }
        };
    }
}
