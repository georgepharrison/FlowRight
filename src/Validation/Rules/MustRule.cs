namespace FlowRight.Validation.Rules;

public sealed class MustRule<T>(Func<T, bool> condition, string errorMessage) : IRule<T>
{
    #region Private Members

    private readonly Func<T, bool> _condition = condition;
    private readonly string _errorMessage = errorMessage;

    #endregion Private Members

    #region Public Methods

    public string? Validate(T value, string displayName) =>
        _condition(value) ? null : _errorMessage;

    #endregion Public Methods
}