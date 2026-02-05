using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;



/// <summary>
/// Result of executing a command.
/// </summary>
public sealed class CommandResult
{
    /// <summary>
    /// Whether the command was executed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Validation errors if the command was invalid. Null if successful.
    /// </summary>
    public IReadOnlyList<ValidationError>? Errors { get; }

    /// <summary>
    /// Events produced by executing the command. Null if failed.
    /// </summary>
    public IReadOnlyList<IGameEvent>? Events { get; }

    private CommandResult(bool isSuccess, IReadOnlyList<ValidationError>? errors, IReadOnlyList<IGameEvent>? events)
    {
        IsSuccess = isSuccess;
        Errors = errors;
        Events = events;
    }

    /// <summary>
    /// Create a successful command result with the produced events.
    /// </summary>
    /// <param name="events">Events produced by the command.</param>
    /// <returns>Successful command result.</returns>
    public static CommandResult Success(IReadOnlyList<IGameEvent> events)
    {
        return new CommandResult(true, null, events);
    }

    /// <summary>
    /// Create a successful command result with a single event.
    /// </summary>
    /// <param name="gameEvent">Event produced by the command.</param>
    /// <returns>Successful command result.</returns>
    public static CommandResult Success(IGameEvent gameEvent)
    {
        return new CommandResult(true, null, [gameEvent]);
    }

    /// <summary>
    /// Create a successful command result with no events.
    /// </summary>
    /// <returns>Successful command result.</returns>
    public static CommandResult SuccessNoEvents()
    {
        return new CommandResult(true, null, []);
    }

    /// <summary>
    /// Create a failed command result from a validation result.
    /// </summary>
    /// <param name="validationResult">Validation result with errors.</param>
    /// <returns>Failed command result.</returns>
    public static CommandResult Invalid(ValidationResult validationResult)
    {
        return new CommandResult(false, validationResult.Errors, null);
    }

    /// <summary>
    /// Create a failed command result with a single error.
    /// </summary>
    /// <param name="code">Error code.</param>
    /// <param name="message">Error message.</param>
    /// <returns>Failed command result.</returns>
    public static CommandResult Invalid(string code, string message)
    {
        return new CommandResult(false, new[] { new ValidationError(code, message) }, null);
    }

    /// <summary>
    /// Create a failed command result with multiple errors.
    /// </summary>
    /// <param name="errors">Validation errors.</param>
    /// <returns>Failed command result.</returns>
    public static CommandResult Invalid(IReadOnlyList<ValidationError> errors)
    {
        return new CommandResult(false, errors, null);
    }

    /// <summary>
    /// Gets a summary of all error messages.
    /// </summary>
    public string GetErrorSummary()
    {
        if (IsSuccess || Errors == null) return string.Empty;
        return string.Join("; ", Errors.Select(e => e.Message));
    }

    public override string ToString()
    {
        if (IsSuccess)
        {
            return $"Success: {Events?.Count ?? 0} event(s)";
        }
        return $"Failed: {GetErrorSummary()}";
    }
}
