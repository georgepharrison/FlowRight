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
            return $"{displayName} must have non-zero timestamp";

        Guid guid = value.Value;
        if (guid == Guid.Empty)
            return $"{displayName} must have non-zero timestamp";

        // Get the GUID bytes and check timestamp components
        byte[] bytes = guid.ToByteArray();

        // Check if timestamp low, mid, and high fields have any non-zero bits
        // Bytes 0-3: timestamp low
        // Bytes 4-5: timestamp mid  
        // Bytes 6-7: timestamp high (including version bits)
        
        // Check each timestamp component separately
        // Low (bytes 0-3): must have at least one non-zero byte
        bool hasNonZeroLow = bytes[0] != 0 || bytes[1] != 0 || bytes[2] != 0 || bytes[3] != 0;
        // Mid (bytes 4-5): must have at least one non-zero byte
        bool hasNonZeroMid = bytes[4] != 0 || bytes[5] != 0;
        // High (bytes 6-7): must have at least one non-zero byte
        bool hasNonZeroHigh = bytes[6] != 0 || bytes[7] != 0;

        return (hasNonZeroLow && hasNonZeroMid && hasNonZeroHigh) ? null : $"{displayName} must have non-zero timestamp";
    }

    #endregion Public Methods
}