namespace FlowRight.Validation.Rules;

/// <summary>
/// Validation rule that ensures a GUID has non-zero timestamp bits, indicating it was not created at epoch time.
/// This can help identify potentially invalid or placeholder GUIDs that may have been generated incorrectly.
/// </summary>
/// <example>
/// <code>
/// // Valid GUID with non-zero timestamp
/// rule.Validate(Guid.NewGuid(), "ID") // Returns null (valid)
/// 
/// // Invalid GUID with all-zero timestamp (very unlikely in practice)
/// rule.Validate(someEpochGuid, "ID") // Returns "ID must have a valid timestamp component"
/// 
/// // Null GUID
/// rule.Validate(null, "ID") // Returns "ID must have a valid timestamp component"
/// </code>
/// </example>
public sealed class GuidNonZeroTimestampRule : IRule<Guid?>
{
    #region Public Methods

    /// <inheritdoc />
    public string? Validate(Guid? value, string displayName)
    {
        if (value is null)
            return $"{displayName} must have a valid timestamp component";

        Guid guid = value.Value;
        if (guid == Guid.Empty)
            return $"{displayName} must have a valid timestamp component";

        // Get the GUID bytes and check timestamp components
        byte[] bytes = guid.ToByteArray();

        // Check if timestamp low, mid, and high fields have any non-zero bits
        // Bytes 0-3: timestamp low
        // Bytes 4-5: timestamp mid  
        // Bytes 6-7: timestamp high (including version bits)
        bool hasNonZeroTimestamp = false;

        for (int i = 0; i < 8; i++)
        {
            if (bytes[i] != 0)
            {
                hasNonZeroTimestamp = true;
                break;
            }
        }

        return hasNonZeroTimestamp ? null : $"{displayName} must have a valid timestamp component";
    }

    #endregion Public Methods
}