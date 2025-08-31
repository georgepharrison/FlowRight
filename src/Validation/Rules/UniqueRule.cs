namespace FlowRight.Validation.Rules;

public sealed class UniqueRule<TItem>(IEqualityComparer<TItem>? comparer = null) : IRule<IEnumerable<TItem>>
{
    #region Private Members

    private readonly IEqualityComparer<TItem>? _comparer = comparer;

    #endregion Private Members

    #region Public Methods

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