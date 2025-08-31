namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string does not exceed a specified maximum length.
/// </summary>
/// <param name="maxLength">The maximum allowed length.</param>
public sealed class MaxLengthRule(int maxLength) : IRule<string>
{
    #region Private Members

    private readonly int _maxLength = maxLength;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the string does not exceed the maximum length.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the string exceeds the maximum length; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (value is null)
        {
            return null;
        }

        return value.Length > _maxLength
            ? $"{displayName} must not exceed {_maxLength} characters"
            : null;
    }

    #endregion Public Methods
}