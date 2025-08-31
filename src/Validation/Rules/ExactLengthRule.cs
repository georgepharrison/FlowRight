namespace FlowRight.Validation.Rules;

public sealed class ExactLengthRule(int length) : IRule<string>
{
    #region Private Members

    private readonly int _length = length;

    #endregion Private Members

    #region Public Methods

    public string? Validate(string value, string displayName)
    {
        if (value is null)
        {
            return null;
        }

        return value.Length != _length
            ? $"{displayName} must be exactly {_length} characters long"
            : null;
    }

    #endregion Public Methods
}