namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a GUID value matches a specific expected GUID.
/// </summary>
/// <param name="expectedGuid">The expected GUID value.</param>
public sealed class SpecificGuidValueRule(Guid expectedGuid) : IRule<Guid?>
{
    #region Private Members

    private readonly Guid _expectedGuid = expectedGuid;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the GUID value matches the expected GUID.
    /// </summary>
    /// <param name="value">The GUID value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value does not match the expected GUID; otherwise, null.</returns>
    public string? Validate(Guid? value, string displayName)
    {
        if (value is null)
        {
            return $"{displayName} must be equal to '{_expectedGuid}'.";
        }

        return value.Value == _expectedGuid
            ? null
            : $"{displayName} must be equal to '{_expectedGuid}'.";
    }

    #endregion Public Methods
}