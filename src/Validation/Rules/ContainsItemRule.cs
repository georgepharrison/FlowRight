namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a collection contains a specific item.
/// </summary>
/// <typeparam name="TItem">The type of items in the collection.</typeparam>
/// <param name="expectedItem">The item that must be present in the collection.</param>
public sealed class ContainsItemRule<TItem>(TItem expectedItem) : IRule<IEnumerable<TItem>>
{
    #region Private Members

    private readonly TItem _expectedItem = expectedItem;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the collection contains the expected item.
    /// </summary>
    /// <param name="value">The collection to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the collection does not contain the expected item; otherwise, null.</returns>
    public string? Validate(IEnumerable<TItem> value, string displayName)
    {
        if (value is null)
        {
            return $"{displayName} must not be null.";
        }

        return value.Contains(_expectedItem)
            ? null
            : $"{displayName} must contain item '{_expectedItem}'.";
    }

    #endregion Public Methods
}