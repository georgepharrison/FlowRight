namespace FlowRight.Validation.Rules;

public class NotNullRule<T> : IRule<T>
{
    #region Public Methods

    public string? Validate(T value, string displayName) =>
        value is null ? $"{displayName} must not be null" : null;

    #endregion Public Methods
}