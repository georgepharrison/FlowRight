namespace FlowRight.Validation.Rules;

public sealed class CountRule<TItem>(int expectedCount) : IRule<IEnumerable<TItem>>
{
    #region Private Members

    private readonly int _expectedCount = expectedCount;

    #endregion Private Members

    #region Public Methods

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