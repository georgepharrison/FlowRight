namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures all items in a collection are unique.
/// </summary>
/// <typeparam name="TItem">The type of items in the collection.</typeparam>
/// <param name="comparer">Optional equality comparer for determining uniqueness.</param>
public sealed class UniqueRule<TItem>(IEqualityComparer<TItem>? comparer = null) : IRule<IEnumerable<TItem>>
{
    #region Private Members

    private readonly IEqualityComparer<TItem>? _comparer = comparer;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that all items in the collection are unique.
    /// </summary>
    /// <param name="value">The collection to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if duplicate items are found; otherwise, null.</returns>
    public string? Validate(IEnumerable<TItem> value, string displayName)
    {
        if (value is null)
        {
            return $"{displayName} must not be null";
        }

        IEnumerable<TItem> items = [.. value];
        IEnumerable<TItem> uniqueItems = _comparer is not null ? [.. items.Distinct(_comparer)] : [.. items.Distinct()];

        return items.Count() != uniqueItems.Count()
            ? $"{displayName} must contain only unique items"
            : null;
    }

    #endregion Public Methods
}