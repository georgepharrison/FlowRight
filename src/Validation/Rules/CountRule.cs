namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a collection contains an exact number of items.
/// </summary>
/// <typeparam name="TItem">The type of items in the collection.</typeparam>
/// <param name="expectedCount">The exact number of items the collection must contain.</param>
public sealed class CountRule<TItem>(int expectedCount) : IRule<IEnumerable<TItem>>
{
    #region Private Members

    private readonly int _expectedCount = expectedCount;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the collection contains exactly the expected number of items.
    /// </summary>
    /// <param name="value">The collection to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the collection does not contain the expected count; otherwise, null.</returns>
    public string? Validate(IEnumerable<TItem> value, string displayName)
    {
        if (value is null)
        {
            return $"{displayName} must not be null.";
        }

        int actualCount = value.Count();

        return actualCount != _expectedCount
            ? $"{displayName} must contain exactly {_expectedCount} items, but found {actualCount}."
            : null;
    }

    #endregion Public Methods
}