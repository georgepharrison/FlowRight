namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string value is in lowercase.
/// </summary>
public sealed class LowerCaseRule : IRule<string>
{
    #region Public Methods

    /// <summary>
    /// Validates that the string value is in lowercase.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not in lowercase; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return value.Equals(value.ToLowerInvariant(), StringComparison.Ordinal)
            ? null
            : $"{displayName} must be in lowercase.";
    }

    #endregion Public Methods
}