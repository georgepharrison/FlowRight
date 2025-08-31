namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string value ends with a specified suffix.
/// </summary>
public sealed class EndsWithRule : IRule<string>
{
    #region Private Members

    private readonly string _suffix;
    private readonly StringComparison _comparison;

    #endregion Private Members

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the EndsWithRule with the specified suffix and comparison type.
    /// </summary>
    /// <param name="suffix">The suffix that the value must end with.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    public EndsWithRule(string suffix, StringComparison comparison = StringComparison.Ordinal)
    {
        ArgumentNullException.ThrowIfNull(suffix);
        _suffix = suffix;
        _comparison = comparison;
    }

    #endregion Public Constructors

    #region Public Methods

    /// <summary>
    /// Validates that the string value ends with the specified suffix.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value does not end with the suffix; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return $"{displayName} must end with '{_suffix}'.";
        }

        return value.EndsWith(_suffix, _comparison)
            ? null
            : $"{displayName} must end with '{_suffix}'.";
    }

    #endregion Public Methods
}