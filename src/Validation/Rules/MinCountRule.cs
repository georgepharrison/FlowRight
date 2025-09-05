namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a collection contains at least a specified number of items.
/// </summary>
/// <typeparam name="TItem">The type of items in the collection.</typeparam>
/// <param name="minCount">The minimum number of items required.</param>
public sealed class MinCountRule<TItem>(int minCount) : IRule<IEnumerable<TItem>>
{
    #region Private Members

    private readonly int _minCount = minCount;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the collection contains at least the minimum number of items.
    /// </summary>
    /// <param name="value">The collection to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the collection has fewer than the minimum count; otherwise, null.</returns>
    public string? Validate(IEnumerable<TItem> value, string displayName)
    {
        if (value is null)
        {
            return $"{displayName} must not be null";
        }

        int actualCount = value.Count();
        return actualCount < _minCount
            ? $"{displayName} must contain at least {_minCount} items but found {actualCount}"
            : null;
    }

    #endregion Public Methods
}