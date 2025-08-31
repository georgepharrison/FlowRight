namespace FlowRight.Validation.Rules;

/// <summary>
/// Validation rule that ensures a GUID is not the empty GUID (00000000-0000-0000-0000-000000000000).
/// This provides a more semantic alternative to NotEqual(Guid.Empty) for GUID-specific validation.
/// </summary>
/// <example>
/// <code>
/// // Valid non-empty GUID
/// rule.Validate(Guid.NewGuid(), "ID") // Returns null (valid)
/// 
/// // Invalid empty GUID
/// rule.Validate(Guid.Empty, "ID") // Returns "ID cannot be empty"
/// 
/// // Null GUID
/// rule.Validate(null, "ID") // Returns "ID cannot be empty"
/// </code>
/// </example>
public sealed class GuidNotEmptyRule : IRule<Guid?>
{
    #region Public Methods

    /// <inheritdoc />
    public string? Validate(Guid? value, string displayName)
    {
        if (value is null)
            return $"{displayName} cannot be empty";

        Guid guid = value.Value;
        return guid == Guid.Empty ? $"{displayName} cannot be empty" : null;
    }

    #endregion Public Methods
}