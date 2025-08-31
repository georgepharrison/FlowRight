namespace FlowRight.Validation.Rules;

public sealed class LengthRule(int min, int max) : IRule<string>
{
    #region Private Members

    private readonly int _max = max;
    private readonly int _min = min;

    #endregion Private Members

    #region Public Methods

    public string? Validate(string value, string displayName)
    {
        if (value is null)
        {
            return null;
        }

        if (value.Length < _min)
        {
            return $"{displayName} must be at least {_min} characters";
        }

        if (value.Length > _max)
        {
            return $"{displayName} must not exceed {_max} characters";
        }

        return null;
    }

    #endregion Public Methods
}