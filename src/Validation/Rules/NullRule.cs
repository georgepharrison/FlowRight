namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a value is null.
/// </summary>
/// <typeparam name="T">The type of value to validate.</typeparam>
public sealed class NullRule<T> : IRule<T>
{
    #region Public Methods

    /// <summary>
    /// Validates that the value is null.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not null; otherwise, null.</returns>
    public string? Validate(T value, string displayName) =>
        value is not null ? $"{displayName} must be null" : null;

    #endregion Public Methods
}