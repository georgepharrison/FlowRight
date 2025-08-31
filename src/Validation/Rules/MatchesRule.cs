using System.Text.RegularExpressions;

namespace FlowRight.Validation.Rules;

public sealed class MatchesRule : IRule<string>
{
    #region Private Members

    private readonly string _pattern;
    private readonly Regex _regex;

    #endregion Private Members

    #region Public Constructors

    public MatchesRule(string pattern, RegexOptions options = RegexOptions.None)
    {
        _pattern = pattern;
        _regex = new Regex(pattern, options);
    }

    public MatchesRule(Regex regex)
    {
        ArgumentNullException.ThrowIfNull(regex);

        _regex = regex;
        _pattern = regex.ToString();
    }

    #endregion Public Constructors

    #region Public Methods

    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return _regex.IsMatch(value)
            ? null
            : $"{displayName} must match the pattern '{_pattern}'.";
    }

    #endregion Public Methods
}