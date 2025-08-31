namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a value is not null.
/// </summary>
/// <typeparam name="T">The type of value to validate.</typeparam>
public class NotNullRule<T> : IRule<T>
{
    #region Public Methods

    /// <summary>
    /// Validates that the value is not null.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is null; otherwise, null.</returns>
    public string? Validate(T value, string displayName) =>
        value is null ? $"{displayName} must not be null" : null;

    #endregion Public Methods
}