namespace FlowRight.Validation.Rules;

public sealed class MinLengthRule(int minLength) : IRule<string>
{
    #region Private Members

    private readonly int _minLength = minLength;

    #endregion Private Members

    #region Public Methods

    public string? Validate(string value, string displayName)
    {
        if (value is null)
        {
            return null;
        }

        return value.Length < _minLength
            ? $"{displayName} must be at least {_minLength} characters"
            : null;
    }

    #endregion Public Methods
}