namespace FlowRight.Validation.Rules;

public sealed class LessThanRule<T>(T valueToCompare) : IRule<T>
{
    #region Private Members

    private readonly T _valueToCompare = valueToCompare;

    #endregion Private Members

    #region Public Methods

    public string? Validate(T value, string displayName)
    {
        if (value is null)
        {
            return null;
        }

        return Comparer<T>.Default.Compare(_valueToCompare, value) < 0
            ? null
            : $"{displayName} must be less than {_valueToCompare}";
    }

    #endregion Public Methods
}