namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string value contains only alphanumeric characters and spaces.
/// </summary>
public sealed class AlphaNumericSpaceRule : IRule<string>
{
    #region Public Methods

    /// <summary>
    /// Validates that the string value contains only alphanumeric characters and spaces.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value contains invalid characters; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return value.All(c => char.IsLetterOrDigit(c) || c == ' ')
            ? null
            : $"{displayName} must contain only alphanumeric characters and spaces.";
    }

    #endregion Public Methods
}