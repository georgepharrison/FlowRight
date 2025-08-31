namespace FlowRight.Validation.Rules;

public sealed class InclusiveBetweenRule(int from, int to) : IRule<int>
{
    #region Private Members

    private readonly int _from = from;
    private readonly int _to = to;

    #endregion Private Members

    #region Public Methods

    public string? Validate(int value, string displayName) =>
        value < _from || value > _to ? $"{displayName} must be between {_from} and {_to} (inclusive)" : null;

    #endregion Public Methods
}