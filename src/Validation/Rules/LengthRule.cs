namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string length is within specified bounds.
/// </summary>
/// <param name="min">The minimum allowed length.</param>
/// <param name="max">The maximum allowed length.</param>
public sealed class LengthRule(int min, int max) : IRule<string>
{
    #region Private Members

    private readonly int _max = max;
    private readonly int _min = min;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the string length is within the specified bounds.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the length is outside bounds; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (value is null)
        {
            return null;
        }

        if (value.Length < _min)
        {
            return $"{displayName} must be at least {_min} characters";
        }

        if (value.Length > _max)
        {
            return $"{displayName} must not exceed {_max} characters";
        }

        return null;
    }

    #endregion Public Methods
}