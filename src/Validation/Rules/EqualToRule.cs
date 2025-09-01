namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string value is equal to a specific expected value.
/// </summary>
/// <param name="expectedValue">The expected string value.</param>
public sealed class EqualToRule(string expectedValue) : IRule<string>
{
    #region Private Members

    private readonly string _expectedValue = expectedValue;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the string value is equal to the expected value.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not equal to the expected value; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        return string.Equals(value, _expectedValue, StringComparison.Ordinal)
            ? null
            : $"{displayName} must be equal to '{_expectedValue}'.";
    }

    #endregion Public Methods
}