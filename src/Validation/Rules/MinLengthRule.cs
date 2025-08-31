namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string meets a minimum length requirement.
/// </summary>
/// <param name="minLength">The minimum required length.</param>
public sealed class MinLengthRule(int minLength) : IRule<string>
{
    #region Private Members

    private readonly int _minLength = minLength;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the string meets the minimum length requirement.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the string is shorter than the minimum length; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (value is null)
        {
            return null;
        }

        return value.Length < _minLength
            ? $"{displayName} must be at least {_minLength} characters"
            : null;
    }

    #endregion Public Methods
}