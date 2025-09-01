using System.Text.RegularExpressions;

namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that validates phone numbers in various common formats.
/// </summary>
public sealed class PhoneRule : IRule<string>
{
    #region Private Members

    private static readonly Regex PhoneRegex = new(
        @"^(\+?1[-.\s]?)?\(?([0-9]{3})\)?[-.\s]?([0-9]{3})[-.\s]?([0-9]{4})$|^[0-9]{10}$|^\+[1-9]\d{1,14}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the string value is a valid phone number format.
    /// </summary>
    /// <param name="value">The phone number to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not a valid phone number; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // Remove common separators and check basic digit count for simple validation
        string digitsOnly = new(value.Where(char.IsDigit).ToArray());
        
        // Basic length check - should have at least 10 digits for most formats
        if (digitsOnly.Length < 7)
        {
            return $"{displayName} is not a valid phone number.";
        }

        // Use regex for more comprehensive validation
        if (!PhoneRegex.IsMatch(value))
        {
            return $"{displayName} is not a valid phone number.";
        }

        return null;
    }

    #endregion Public Methods
}