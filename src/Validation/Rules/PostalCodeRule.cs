using System.Text.RegularExpressions;

namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that validates postal codes for various countries (US ZIP, Canadian postal, UK postal codes).
/// </summary>
public sealed class PostalCodeRule : IRule<string>
{
    #region Private Members

    private static readonly Regex PostalCodeRegex = new(
        @"^(?:[0-9]{5}(?:-[0-9]{4})?|[A-Za-z][0-9][A-Za-z] ?[0-9][A-Za-z][0-9]|[A-Za-z]{1,2}[0-9][A-Za-z0-9]? ?[0-9][A-Za-z]{2})$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the string value is a valid postal code format (US ZIP, Canadian postal, or UK postal code).
    /// </summary>
    /// <param name="value">The postal code to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not a valid postal code; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (!PostalCodeRegex.IsMatch(value))
        {
            return $"{displayName} is not a valid postal code.";
        }

        return null;
    }

    #endregion Public Methods
}