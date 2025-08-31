namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string value is in uppercase.
/// </summary>
public sealed class UpperCaseRule : IRule<string>
{
    #region Public Methods

    /// <summary>
    /// Validates that the string value is in uppercase.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not in uppercase; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return value.Equals(value.ToUpperInvariant(), StringComparison.Ordinal)
            ? null
            : $"{displayName} must be in uppercase.";
    }

    #endregion Public Methods
}