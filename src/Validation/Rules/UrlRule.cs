using System.Text.RegularExpressions;

namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a string value is a valid URL format.
/// </summary>
public partial class UrlRule : IRule<string>
{
    #region Private Members

    private static readonly Regex _urlRegex = ValidUrlPattern();

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the string value is a valid URL format.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not a valid URL format; otherwise, null.</returns>
    public string? Validate(string value, string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return _urlRegex.IsMatch(value) && Uri.TryCreate(value, UriKind.Absolute, out Uri? _)
            ? null
            : $"{displayName} must be a valid URL.";
    }

    #endregion Public Methods

    #region Private Methods

    [GeneratedRegex(@"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex ValidUrlPattern();

    #endregion Private Methods
}