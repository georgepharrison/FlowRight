using System.Text.RegularExpressions;

namespace FlowRight.Validation.Rules;

public partial class EmailRule : IRule<string>
{
    #region Private Members

    private static readonly Regex _emailRegex = ValidEmailAddress();

    #endregion Private Members

    #region Public Methods

    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return _emailRegex.IsMatch(value)
            ? null
            : $"{displayName} must be a valid email address.";
    }

    #endregion Public Methods

    #region Private Methods

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex ValidEmailAddress();

    #endregion Private Methods
}