namespace FlowRight.Validation.Rules;

public sealed class NotEqualRule<T>(T comparisonValue, IEqualityComparer<T>? comparer = null) : IRule<T>
{
    #region Private Members

    private readonly IEqualityComparer<T> _comparer = comparer ?? EqualityComparer<T>.Default;
    private readonly T _comparisonValue = comparisonValue;

    #endregion Private Members

    #region Public Methods

    public string? Validate(T value, string displayName) =>
        !_comparer.Equals(value, _comparisonValue)
            ? null
            : $"{displayName} must not be equal to '{_comparisonValue}'";

    #endregion Public Methods
}