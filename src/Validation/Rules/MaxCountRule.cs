namespace FlowRight.Validation.Rules;

public sealed class MaxCountRule<TItem>(int maxCount) : IRule<IEnumerable<TItem>>
{
    #region Private Members

    private readonly int _maxCount = maxCount;

    #endregion Private Members

    #region Public Methods

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