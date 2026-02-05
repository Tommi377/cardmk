using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealMK;

/// <summary>
/// Dispatches commands to the rules engine and publishes resulting events.
/// Acts as the main entry point for command execution.
/// </summary>
public sealed class CommandDispatcher
{
    // private readonly RulesEngine _engine;
    private readonly EventBus _eventBus;

    /// <summary>
    /// Creates a new command dispatcher.
    /// </summary>
    /// <param name="engine">The rules engine for validation and execution.</param>
    /// <param name="eventBus">Event bus for publishing events.</param>
    public CommandDispatcher(EventBus eventBus)
    {
        // _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        // _logger = logger ?? LoggerProvider.Current;

        Log.Debug("CommandDispatcher created");
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

        // Validate the command
        // ValidationResult validation = _engine.Validate(command);
        // if (!validation.IsValid)
        // {
        //     _logger.Warning("Command validation failed: {0}", validation.GetErrorSummary());
        //     return CommandResult.Invalid(validation);
        // }

        // Execute the command
        IReadOnlyList<IGameEvent> events;
        try
        {
            Log.Info("TODO: Command execution is not implemented yet, returning empty event list");
            events = [];
            // events = _engine.Execute(command);
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
