namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a value is less than or equal to a specified comparison value.
/// </summary>
/// <typeparam name="T">The type of value to validate.</typeparam>
/// <param name="valueToCompare">The value to compare against.</param>
public sealed class LessThanOrEqualToRule<T>(T valueToCompare) : IRule<T>
{
    #region Private Members

    private readonly T _valueToCompare = valueToCompare;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the value is less than or equal to the comparison value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is greater than the comparison value; otherwise, null.</returns>
    public string? Validate(T value, string displayName)
    {
        if (value is null)
        {
            return null;
        }

        return Comparer<T>.Default.Compare(_valueToCompare, value) <= 0
            ? null
            : $"{displayName} must be less than or equal to {_valueToCompare}";
    }

    #endregion Public Methods
}