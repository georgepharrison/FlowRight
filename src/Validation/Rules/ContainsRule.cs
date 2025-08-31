namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string value contains a specified substring.
/// </summary>
public sealed class ContainsRule : IRule<string>
{
    #region Private Members

    private readonly string _substring;
    private readonly StringComparison _comparison;

    #endregion Private Members

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the ContainsRule with the specified substring and comparison type.
    /// </summary>
    /// <param name="substring">The substring that must be contained in the value.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    public ContainsRule(string substring, StringComparison comparison = StringComparison.Ordinal)
    {
        ArgumentNullException.ThrowIfNull(substring);
        _substring = substring;
        _comparison = comparison;
    }

    #endregion Public Constructors

    #region Public Methods

    /// <summary>
    /// Validates that the string value contains the specified substring.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value does not contain the substring; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return $"{displayName} must contain '{_substring}'.";
        }

        return value.Contains(_substring, _comparison)
            ? null
            : $"{displayName} must contain '{_substring}'.";
    }

    #endregion Public Methods
}