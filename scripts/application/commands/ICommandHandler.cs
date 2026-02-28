using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Handles validation and execution for a command type.
/// </summary>
public interface ICommandHandler<in TCommand> where TCommand : IGameCommand
{
    /// <summary>
    /// Validates a command.
    /// </summary>
    ValidationResult Validate(TCommand command);

    /// <summary>
    /// Executes a command and returns produced events.
    /// </summary>
    IReadOnlyList<IGameEvent> Execute(TCommand command);
}
