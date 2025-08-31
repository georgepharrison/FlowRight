namespace FlowRight.Validation.Rules;

public sealed class EmptyRule<T> : IRule<T>
{
    #region Public Methods

    public string? Validate(T value, string displayName) =>
        value switch
        {
            null => null,
            string s when string.IsNullOrWhiteSpace(s) => null,
            DateTime dt when dt == default => null,
            Guid g when g == Guid.Empty => null,
            System.Collections.IEnumerable e when !e.Cast<object>().Any() => null,
            _ => $"{displayName} must be empty."
        };

    #endregion Public Methods
}