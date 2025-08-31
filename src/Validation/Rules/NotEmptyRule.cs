namespace FlowRight.Validation.Rules;

/// <summary>
/// Validation rule that ensures a value is not considered "empty" according to type-specific logic.
/// Handles nulls, empty strings, default values, empty GUIDs, and empty collections.
/// </summary>
/// <typeparam name="T">The type of value being validated.</typeparam>
/// <example>
/// <code>
/// // String validation
/// rule.Validate("", "Name") // Returns "Name must not be empty"
/// rule.Validate("   ", "Name") // Returns "Name must not be empty" (whitespace)
/// rule.Validate("Value", "Name") // Returns null (valid)
/// 
/// // GUID validation
/// rule.Validate(Guid.Empty, "ID") // Returns "ID must not be empty"
/// rule.Validate(Guid.NewGuid(), "ID") // Returns null (valid)
/// 
/// // Collection validation
/// rule.Validate(new List&lt;string&gt;(), "Items") // Returns "Items must not be empty"
/// rule.Validate(new[] { "item" }, "Items") // Returns null (valid)
/// </code>
/// </example>
public sealed class NotEmptyRule<T> : IRule<T>
{
    #region Public Methods

    /// <inheritdoc />
    public string? Validate(T value, string displayName) =>
        value switch
        {
            null => $"{displayName} must not be empty",
            string s when string.IsNullOrWhiteSpace(s) => $"{displayName} must not be empty",
            DateTime dt when dt == default => $"{displayName} must not be empty",
            Guid g when g == Guid.Empty => $"{displayName} must not be empty",
            System.Collections.IEnumerable e when !e.Cast<object>().Any() => $"{displayName} must not be empty",
            _ => null
        };

    #endregion Public Methods
}