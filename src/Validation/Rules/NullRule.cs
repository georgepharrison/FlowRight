namespace FlowRight.Validation.Rules;

public sealed class NullRule<T> : IRule<T>
{
    #region Public Methods

    public string? Validate(T value, string displayName) =>
        value is not null ? $"{displayName} must be null" : null;

    #endregion Public Methods
}