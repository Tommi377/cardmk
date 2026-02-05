using System;
using System.Collections.Generic;
using System.Linq;

namespace RealMK;

/// <summary>
/// Exception thrown when content parsing fails.
/// </summary>
public class ContentParseException : Exception
{
    /// <summary>
    /// Creates a new parse exception with the specified message.
    /// </summary>
    public ContentParseException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new parse exception with the specified message and inner exception.
    /// </summary>
    public ContentParseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when content validation fails.
/// </summary>
public class ContentValidationException : Exception
{
    /// <summary>
    /// The validation errors that were found.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Creates a new validation exception with the specified errors.
    /// </summary>
    public ContentValidationException(IEnumerable<string> errors)
        : base($"Content validation failed with {errors.Count()} error(s)")
    {
        Errors = errors.ToList();
    }

    /// <summary>
    /// Creates a new validation exception with a single error.
    /// </summary>
    public ContentValidationException(string error)
        : base($"Content validation failed: {error}")
    {
        Errors = [error];
    }
}
