namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a value is between two bounds (exclusive).
/// </summary>
/// <typeparam name="T">The type of value to validate.</typeparam>
/// <param name="from">The lower bound (exclusive).</param>
/// <param name="to">The upper bound (exclusive).</param>
public sealed class ExclusiveBetweenRule<T>(T from, T to) : IRule<T>
{
    #region Private Members

    private readonly T _from = from;
    private readonly T _to = to;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the value is between the bounds (exclusive).
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not between the bounds; otherwise, null.</returns>
    public string? Validate(T value, string displayName)
    {
        if (value is null)
        {
            return null;
        }

        Comparer<T> comparer = Comparer<T>.Default;
        return comparer.Compare(value, _from) <= 0 || comparer.Compare(value, _to) >= 0 
            ? $"{displayName} must be between {_from} and {_to} (exclusive)" 
            : null;
    }

    #endregion Public Methods
}