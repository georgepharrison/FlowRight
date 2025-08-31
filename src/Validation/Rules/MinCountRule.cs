namespace FlowRight.Validation.Rules;

public sealed class MinCountRule<TItem>(int minCount) : IRule<IEnumerable<TItem>>
{
    #region Private Members

    private readonly int _minCount = minCount;

    #endregion Private Members

    #region Public Methods

    public string? Validate(IEnumerable<TItem> value, string displayName)
    {
        if (value is null)
        {
            return $"{displayName} must not be null";
        }

        int actualCount = value.Count();
        return actualCount < _minCount
            ? $"{displayName} must contain at least {_minCount} items but count {actualCount}"
            : null;
    }

    #endregion Public Methods
}