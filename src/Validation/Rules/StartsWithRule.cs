namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string value starts with a specified prefix.
/// </summary>
public sealed class StartsWithRule : IRule<string>
{
    #region Private Members

    private readonly string _prefix;
    private readonly StringComparison _comparison;

    #endregion Private Members

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the StartsWithRule with the specified prefix and comparison type.
    /// </summary>
    /// <param name="prefix">The prefix that the value must start with.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    public StartsWithRule(string prefix, StringComparison comparison = StringComparison.Ordinal)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        _prefix = prefix;
        _comparison = comparison;
    }

    #endregion Public Constructors

    #region Public Methods

    /// <summary>
    /// Validates that the string value starts with the specified prefix.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value does not start with the prefix; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return $"{displayName} must start with '{_prefix}'.";
        }

        return value.StartsWith(_prefix, _comparison)
            ? null
            : $"{displayName} must start with '{_prefix}'.";
    }

    #endregion Public Methods
}