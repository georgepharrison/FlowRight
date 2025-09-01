namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that applies a custom condition function to validate values.
/// </summary>
/// <typeparam name="T">The type of value to validate.</typeparam>
/// <param name="condition">The condition function to apply.</param>
/// <param name="errorMessage">The error message to return when validation fails.</param>
public sealed class MustRule<T>(Func<T, bool> condition, string errorMessage) : IRule<T>
{
    #region Private Members

    private readonly Func<T, bool> _condition = condition;
    private readonly string _errorMessage = errorMessage;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates the value using the custom condition function.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>The error message if the condition fails; otherwise, null.</returns>
    public string? Validate(T value, string displayName) =>
        _condition(value) ? null : string.Format(System.Globalization.CultureInfo.InvariantCulture, _errorMessage, value, displayName);

    #endregion Public Methods
}