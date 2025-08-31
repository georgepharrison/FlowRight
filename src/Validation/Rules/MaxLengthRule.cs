namespace FlowRight.Validation.Rules;

public sealed class MaxLengthRule(int maxLength) : IRule<string>

{
    #region Private Members

    private readonly int _maxLength = maxLength;

    #endregion Private Members

    #region Public Methods

    public string? Validate(string value, string displayName)
    {
        if (value is null)
        {
            return null;
        }

        return value.Length > _maxLength
            ? $"{displayName} must not exceed {_maxLength} characters"
            : null;
    }

    #endregion Public Methods
}