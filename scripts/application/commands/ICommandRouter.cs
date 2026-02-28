using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Routes commands to matching typed handlers.
/// </summary>
public interface ICommandRouter
{
    /// <summary>
    /// Validates a command against its handler.
    /// </summary>
    ValidationResult Validate(IGameCommand command);

    /// <summary>
    /// Executes a command and returns produced events.
    /// </summary>
    IReadOnlyList<IGameEvent> Execute(IGameCommand command);
}
