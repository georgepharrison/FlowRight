namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string has an exact specified length.
/// </summary>
/// <param name="length">The exact length the string must have.</param>
public sealed class ExactLengthRule(int length) : IRule<string>
{
    #region Private Members

    private readonly int _length = length;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the string has the exact specified length.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the string length does not match; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (value is null)
        {
            return null;
        }

        return value.Length != _length
            ? $"{displayName} must be exactly {_length} characters long"
            : null;
    }

    #endregion Public Methods
}