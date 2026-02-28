using System;
using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Dispatches commands to the rules engine and publishes resulting events.
/// Acts as the main entry point for command execution.
/// </summary>
public sealed class CommandDispatcher
{
    private readonly ICommandRouter _router;
    private readonly EventBus _eventBus;
    private int _expectedSequenceNumber;

    /// <summary>
    /// Creates a new command dispatcher.
    /// </summary>
    /// <param name="router">Command router for validation and execution.</param>
    /// <param name="eventBus">Event bus for publishing events.</param>
    public CommandDispatcher(EventBus eventBus, ICommandRouter router, int initialSequenceNumber = 0)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _router = router ?? throw new ArgumentNullException(nameof(router));
        _expectedSequenceNumber = initialSequenceNumber;

        Log.Debug("CommandDispatcher created");
    }

    /// <summary>
    /// Validates a command without executing it.
    /// </summary>
    public ValidationResult Validate(IGameCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.SequenceNumber != _expectedSequenceNumber)
        {
            return ValidationResult.Invalid(
                ValidationErrorCodes.InvalidSequence,
                $"Invalid command sequence {command.SequenceNumber}, expected {_expectedSequenceNumber}");
        }

        return _router.Validate(command);
    }

    /// <summary>
    /// Dispatches a command for validation and execution.
    /// </summary>
    /// <param name="command">The command to dispatch.</param>
    /// <returns>Result indicating success or failure with events.</returns>
    public CommandResult Dispatch(IGameCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        Log.Debug($"Dispatching command: {command.GetType().Name} (seq={command.SequenceNumber})");

        ValidationResult validation = Validate(command);
        if (!validation.IsValid)
        {
            Log.Warning($"Command validation failed: {validation.GetErrorSummary()}");
            return CommandResult.Invalid(validation);
        }

        // Execute the command
        IReadOnlyList<IGameEvent> events;
        try
        {
            events = _router.Execute(command);
        }
        catch (Exception ex)
        {
            Log.Error($"Command execution failed: {ex.Message}");
            return CommandResult.Invalid("EXECUTION_ERROR", $"Command execution failed: {ex.Message}");
        }

        // Publish events
        foreach (IGameEvent gameEvent in events)
        {
            Log.Trace($"Publishing event: {gameEvent.GetType().Name} (idx={gameEvent.EventIndex})");
            _eventBus.Publish(gameEvent);
        }

        _expectedSequenceNumber++;
        Log.Debug($"Command succeeded, published {events.Count} events");
        return CommandResult.Success(events);
    }

    /// <summary>
    /// Gets a preview of what a command would do without executing it.
    /// </summary>
    /// <param name="command">The command to preview.</param>
    /// <returns>Preview of the command's effects.</returns>
    // public ActionPreview Preview(IGameCommand command)
    // {
    //     ArgumentNullException.ThrowIfNull(command);
    //     return _engine.Preview(command);
    // }

    /// <summary>
    /// Gets all legal commands for a player.
    /// </summary>
    /// <param name="playerId">The player to get commands for.</param>
    /// <returns>List of legal commands.</returns>
    // public IReadOnlyList<IGameCommand> GetLegalCommands(PlayerId playerId)
    // {
    //     return _engine.EnumerateLegalCommands(playerId);
    // }

    /// <summary>
    /// Validates a command without executing it.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>Validation result.</returns>
    // public ValidationResult Validate(IGameCommand command)
    // {
    //     ArgumentNullException.ThrowIfNull(command);
    //     return _engine.Validate(command);
    // }

    /// <summary>
    /// Computes the current state hash.
    /// </summary>
    /// <returns>64-bit hash of the game state.</returns>
    // public ulong GetStateHash()
    // {
    //     return _engine.ComputeStateHash();
    // }

    /// <summary>
    /// Gets the underlying rules engine.
    /// </summary>
    // public RulesEngine Engine => _engine;
}
