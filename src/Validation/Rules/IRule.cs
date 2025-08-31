namespace FlowRight.Validation.Rules;

/// <summary>
/// Defines a validation rule that can be applied to values of type T, providing error messages when validation fails.
/// </summary>
/// <typeparam name="T">The type of value this rule can validate.</typeparam>
public interface IRule<in T>
{
    #region Public Methods

    /// <summary>
    /// Validates the specified value and returns an error message if validation fails.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">The display name for the property being validated, used in error messages.</param>
    /// <returns>An error message if validation fails; otherwise, null indicating validation passed.</returns>
    string? Validate(T value, string displayName);

    #endregion Public Methods
}