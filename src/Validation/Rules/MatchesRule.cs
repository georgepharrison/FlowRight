using System.Text.RegularExpressions;

namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string value matches a specified regular expression pattern.
/// </summary>
public sealed class MatchesRule : IRule<string>
{
    #region Private Members

    private readonly string _pattern;
    private readonly Regex _regex;

    #endregion Private Members

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the MatchesRule with a pattern and options.
    /// </summary>
    /// <param name="pattern">The regular expression pattern.</param>
    /// <param name="options">Optional regex options.</param>
    public MatchesRule(string pattern, RegexOptions options = RegexOptions.None)
    {
        _pattern = pattern;
        _regex = new Regex(pattern, options);
    }

    /// <summary>
    /// Initializes a new instance of the MatchesRule with a compiled regex.
    /// </summary>
    /// <param name="regex">The compiled regular expression.</param>
    public MatchesRule(Regex regex)
    {
        ArgumentNullException.ThrowIfNull(regex);

        _regex = regex;
        _pattern = regex.ToString();
    }

    #endregion Public Constructors

    #region Public Methods

    /// <summary>
    /// Validates that the string value matches the regular expression pattern.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value does not match the pattern; otherwise, null.</returns>
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