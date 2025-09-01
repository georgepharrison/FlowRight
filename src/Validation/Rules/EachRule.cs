namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that validates each item in a collection using a nested validation rule.
/// </summary>
/// <typeparam name="TItem">The type of items in the collection.</typeparam>
/// <param name="nestedRule">The rule to apply to each item in the collection.</param>
public sealed class EachRule<TItem>(IRule<TItem> nestedRule) : IRule<IEnumerable<TItem>>
{
    #region Private Members

    private readonly IRule<TItem> _nestedRule = nestedRule;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates each item in the collection using the nested rule.
    /// </summary>
    /// <param name="value">The collection to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if any item fails validation; otherwise, null.</returns>
    public string? Validate(IEnumerable<TItem> value, string displayName)
    {
        if (value is null)
        {
            return $"{displayName} must not be null.";
        }

        int index = 0;
        foreach (TItem item in value)
        {
            string? error = _nestedRule.Validate(item, $"Item[{index}]");
            if (error is not null)
            {
                return $"{displayName} contains invalid items.";
            }
            index++;
        }

        return null;
    }

    #endregion Public Methods
}