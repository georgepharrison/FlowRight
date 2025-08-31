namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a collection contains at most a specified number of items.
/// </summary>
/// <typeparam name="TItem">The type of items in the collection.</typeparam>
/// <param name="maxCount">The maximum number of items allowed.</param>
public sealed class MaxCountRule<TItem>(int maxCount) : IRule<IEnumerable<TItem>>
{
    #region Private Members

    private readonly int _maxCount = maxCount;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the collection contains at most the maximum number of items.
    /// </summary>
    /// <param name="value">The collection to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the collection exceeds the maximum count; otherwise, null.</returns>
    public string? Validate(IEnumerable<TItem> value, string displayName)
    {
        if (value is null)
        {
            return $"{displayName} must not be null";
        }

        int actualCount = value.Count();
        return actualCount > _maxCount
            ? $"{displayName} must contain at most {_maxCount} items but found {actualCount}"
            : null;
    }

    #endregion Public Methods
}