namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a value is equal to a specified comparison value.
/// </summary>
/// <typeparam name="T">The type of value to validate.</typeparam>
/// <param name="comparisonValue">The value to compare against.</param>
/// <param name="comparer">Optional equality comparer to use for comparison.</param>
public sealed class EqualRule<T>(T comparisonValue, IEqualityComparer<T>? comparer = null) : IRule<T>
{
    #region Private Members

    private readonly IEqualityComparer<T> _comparer = comparer ?? EqualityComparer<T>.Default;
    private readonly T _comparisonValue = comparisonValue;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the value equals the comparison value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the values are not equal; otherwise, null.</returns>
    public string? Validate(T value, string displayName) =>
        _comparer.Equals(value, _comparisonValue)
            ? null
            : $"{displayName} must be equal to '{_comparisonValue}'";

    #endregion Public Methods
}