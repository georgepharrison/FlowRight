namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string value contains only alphanumeric characters.
/// </summary>
public sealed class AlphaNumericRule : IRule<string>
{
    #region Public Methods

    /// <summary>
    /// Validates that the string value contains only alphanumeric characters (letters and digits).
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value contains non-alphanumeric characters; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return value.All(char.IsLetterOrDigit)
            ? null
            : $"{displayName} must contain only alphanumeric characters.";
    }

    #endregion Public Methods
}