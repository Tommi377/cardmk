namespace RealMK;

/// <summary>
/// A key used to look up localized text.
/// </summary>
public readonly record struct LocalizationKey(string Value)
{
    public static implicit operator string(LocalizationKey key) => key.Value;
    public static implicit operator LocalizationKey(string value) => new(value);
    public override string ToString() => Value;

    /// <summary>
    /// Returns true if this key represents an empty or missing localization.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Value);
}
