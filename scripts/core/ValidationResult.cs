using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Result of validating a command.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// A successful validation result with no errors.
    /// </summary>
    public static readonly ValidationResult Success = new(true, []);

    /// <summary>
    /// Whether the validation passed.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// List of validation errors (empty if valid).
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>
    /// Creates a validation result.
    /// </summary>
    /// <param name="isValid">Whether validation passed.</param>
    /// <param name="errors">List of errors.</param>
    public ValidationResult(bool isValid, IReadOnlyList<ValidationError> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    /// <summary>
    /// Creates a failed validation result with a single error.
    /// </summary>
    /// <param name="code">Error code.</param>
    /// <param name="message">Error message.</param>
    /// <returns>Failed validation result.</returns>
    public static ValidationResult Invalid(string code, string message)
    {
        return new ValidationResult(false, [new ValidationError(code, message)]);
    }

    /// <summary>
    /// Creates a failed validation result with multiple errors.
    /// </summary>
    /// <param name="errors">List of errors.</param>
    /// <returns>Failed validation result.</returns>
    public static ValidationResult Invalid(IReadOnlyList<ValidationError> errors)
    {
        return new ValidationResult(false, errors);
    }

    /// <summary>
    /// Creates a failed validation result from a builder.
    /// </summary>
    /// <param name="builder">Builder with accumulated errors.</param>
    /// <returns>Validation result (success if no errors).</returns>
    public static ValidationResult FromBuilder(ValidationResultBuilder builder)
    {
        return builder.Build();
    }

    /// <summary>
    /// Gets all error messages concatenated.
    /// </summary>
    public string GetErrorSummary()
    {
        if (IsValid) return string.Empty;
        return string.Join("; ", Errors.Select(e => e.Message));
    }

    public override string ToString()
    {
        return IsValid ? "Valid" : $"Invalid: {GetErrorSummary()}";
    }
}

/// <summary>
/// A single validation error.
/// </summary>
/// <param name="Code">Machine-readable error code.</param>
/// <param name="Message">Human-readable error message.</param>
public sealed record ValidationError(string Code, string Message);

/// <summary>
/// Builder for accumulating validation errors.
/// </summary>
public sealed class ValidationResultBuilder
{
    private readonly List<ValidationError> _errors = new();

    /// <summary>
    /// Whether any errors have been added.
    /// </summary>
    public bool HasErrors => _errors.Count > 0;

    /// <summary>
    /// Add an error to the builder.
    /// </summary>
    /// <param name="code">Error code.</param>
    /// <param name="message">Error message.</param>
    /// <returns>This builder for chaining.</returns>
    public ValidationResultBuilder AddError(string code, string message)
    {
        _errors.Add(new ValidationError(code, message));
        return this;
    }

    /// <summary>
    /// Add an error if the condition is true.
    /// </summary>
    /// <param name="condition">Condition to check.</param>
    /// <param name="code">Error code.</param>
    /// <param name="message">Error message.</param>
    /// <returns>This builder for chaining.</returns>
    public ValidationResultBuilder AddErrorIf(bool condition, string code, string message)
    {
        if (condition)
        {
            _errors.Add(new ValidationError(code, message));
        }
        return this;
    }

    /// <summary>
    /// Add multiple errors from another validation result.
    /// </summary>
    /// <param name="result">Result to merge errors from.</param>
    /// <returns>This builder for chaining.</returns>
    public ValidationResultBuilder Merge(ValidationResult result)
    {
        if (!result.IsValid)
        {
            _errors.AddRange(result.Errors);
        }
        return this;
    }

    /// <summary>
    /// Build the final validation result.
    /// </summary>
    /// <returns>Validation result (success if no errors).</returns>
    public ValidationResult Build()
    {
        return _errors.Count == 0
            ? ValidationResult.Success
            : new ValidationResult(false, _errors.ToArray());
    }
}

/// <summary>
/// Common validation error codes.
/// </summary>
public static class ValidationErrorCodes
{
    /// <summary>Command is from wrong player.</summary>
    public const string WrongPlayer = "WRONG_PLAYER";

    /// <summary>Command sequence number is incorrect.</summary>
    public const string InvalidSequence = "INVALID_SEQUENCE";

    /// <summary>Action is not valid in current game phase.</summary>
    public const string InvalidPhase = "INVALID_PHASE";

    /// <summary>Target is invalid or does not exist.</summary>
    public const string InvalidTarget = "INVALID_TARGET";

    /// <summary>Player does not have required resources.</summary>
    public const string InsufficientResources = "INSUFFICIENT_RESOURCES";

    /// <summary>Card is not in expected location (hand, deck, etc.).</summary>
    public const string CardNotInHand = "CARD_NOT_IN_HAND";

    /// <summary>Cannot play wound cards.</summary>
    public const string CannotPlayWound = "CANNOT_PLAY_WOUND";

    /// <summary>Path is invalid or blocked.</summary>
    public const string InvalidPath = "INVALID_PATH";

    /// <summary>Not enough movement points.</summary>
    public const string InsufficientMovement = "INSUFFICIENT_MOVEMENT";

    /// <summary>Mana die is not available.</summary>
    public const string ManaNotAvailable = "MANA_NOT_AVAILABLE";

    /// <summary>Not in combat.</summary>
    public const string NotInCombat = "NOT_IN_COMBAT";

    /// <summary>Combat phase is incorrect for this action.</summary>
    public const string WrongCombatPhase = "WRONG_COMBAT_PHASE";

    /// <summary>Enemy has already been defeated.</summary>
    public const string EnemyDefeated = "ENEMY_DEFEATED";

    /// <summary>Unit is not available (wounded or exhausted).</summary>
    public const string UnitNotAvailable = "UNIT_NOT_AVAILABLE";

    /// <summary>Resolution input is missing a required choice selection.</summary>
    public const string MissingResolutionChoice = "MISSING_RESOLUTION_CHOICE";

    /// <summary>Resolution input has an invalid choice selection.</summary>
    public const string InvalidResolutionChoice = "INVALID_RESOLUTION_CHOICE";

    /// <summary>Card references a script id that is not registered.</summary>
    public const string UnknownScriptId = "UNKNOWN_SCRIPT_ID";

    /// <summary>Script parameters are missing or invalid.</summary>
    public const string InvalidScriptParameters = "INVALID_SCRIPT_PARAMETERS";
}
