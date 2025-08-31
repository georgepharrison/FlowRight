using System.Text.RegularExpressions;

namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string value is a valid email address format.
/// </summary>
public partial class EmailRule : IRule<string>
{
    #region Private Members

    private static readonly Regex _emailRegex = ValidEmailAddress();

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the string value is a valid email address format.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not a valid email format; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return _emailRegex.IsMatch(value)
            ? null
            : $"{displayName} must be a valid email address.";
    }

    #endregion Public Methods

    #region Private Methods

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex ValidEmailAddress();

    #endregion Private Methods
}