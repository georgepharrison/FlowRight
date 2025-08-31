namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a value is empty or in its default state.
/// </summary>
/// <typeparam name="T">The type of value to validate.</typeparam>
public sealed class EmptyRule<T> : IRule<T>
{
    #region Public Methods

    /// <summary>
    /// Validates that the value is empty or in its default state.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not empty; otherwise, null.</returns>
    public string? Validate(T value, string displayName) =>
        value switch
        {
            null => null,
            string s when string.IsNullOrWhiteSpace(s) => null,
            DateTime dt when dt == default => null,
            Guid g when g == Guid.Empty => null,
            System.Collections.IEnumerable e when !e.Cast<object>().Any() => null,
            _ => $"{displayName} must be empty."
        };

    #endregion Public Methods
}