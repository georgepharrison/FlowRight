using System.Text.RegularExpressions;

namespace FlowRight.Validation.Rules;

/// <summary>
/// Validation rule that ensures a GUID follows the standard hyphenated format (8-4-4-4-12 characters).
/// This validates the string representation of the GUID is properly formatted.
/// </summary>
/// <example>
/// <code>
/// // Valid format
/// rule.Validate(Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"), "ID") // Returns null (valid)
/// 
/// // Any valid GUID should pass this rule as System.Guid ensures valid format
/// rule.Validate(Guid.NewGuid(), "ID") // Returns null (valid)
/// 
/// // Null GUID
/// rule.Validate(null, "ID") // Returns "ID must be in valid GUID format"
/// </code>
/// </example>
public sealed class GuidValidFormatRule : IRule<Guid?>
{
    #region Private Constants

    private static readonly Regex GuidFormatRegex = new(
        @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    #endregion Private Constants

    #region Public Methods

    /// <inheritdoc />
    public string? Validate(Guid? value, string displayName)
    {
        if (value is null)
            return $"{displayName} is not a valid GUID";

        Guid guid = value.Value;
        string guidString = guid.ToString();

        return GuidFormatRegex.IsMatch(guidString) ? null : $"{displayName} is not a valid GUID";
    }

    #endregion Public Methods
}