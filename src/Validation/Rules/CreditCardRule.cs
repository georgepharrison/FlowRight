namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that validates credit card numbers using the Luhn algorithm.
/// </summary>
public sealed class CreditCardRule : IRule<string>
{
    #region Public Methods

    /// <summary>
    /// Validates that the string value is a valid credit card number using the Luhn algorithm.
    /// </summary>
    /// <param name="value">The credit card number to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not a valid credit card number; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // Check if all characters are numeric
        if (!value.All(char.IsDigit))
        {
            return $"{displayName} is not a valid credit card number.";
        }

        // Check length (most credit cards are between 13-19 digits, but commonly 16)
        if (value.Length < 13 || value.Length > 19)
        {
            return $"{displayName} is not a valid credit card number.";
        }

        // Validate using Luhn algorithm
        if (!IsValidLuhn(value))
        {
            return $"{displayName} is not a valid credit card number.";
        }

        return null;
    }

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// Validates a number using the Luhn algorithm.
    /// </summary>
    /// <param name="number">The number to validate.</param>
    /// <returns>True if the number passes Luhn validation; otherwise, false.</returns>
    private static bool IsValidLuhn(string number)
    {
        int sum = 0;
        bool isEven = false;

        // Process digits from right to left
        for (int i = number.Length - 1; i >= 0; i--)
        {
            int digit = number[i] - '0';

            if (isEven)
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit = digit / 10 + digit % 10;
                }
            }

            sum += digit;
            isEven = !isEven;
        }

        return sum % 10 == 0;
    }

    #endregion Private Methods
}