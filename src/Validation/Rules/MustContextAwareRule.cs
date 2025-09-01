using FlowRight.Validation.Context;

namespace FlowRight.Validation.Rules;

/// <summary>
/// A context-aware validation rule that applies a custom condition function with access to validation context.
/// This rule enables complex validation scenarios that require access to root objects, services, or custom data.
/// </summary>
/// <typeparam name="T">The type of value to validate.</typeparam>
/// <param name="condition">The condition function that receives both the value and validation context.</param>
/// <param name="errorMessage">The error message to return when validation fails.</param>
/// <remarks>
/// <para>
/// This rule extends the basic MustRule functionality to provide access to the validation context,
/// enabling validation logic that depends on:
/// <list type="bullet">
/// <item><description>Other properties of the root object being validated</description></item>
/// <item><description>External services accessed through dependency injection</description></item>
/// <item><description>Custom data shared between validation rules</description></item>
/// <item><description>Parent-child relationships in hierarchical validation</description></item>
/// <item><description>Previously executed validation rules and their results</description></item>
/// </list>
/// </para>
/// <para>
/// The condition function should return true if the validation passes, or false if it fails.
/// When the condition returns false, the specified error message will be used as the validation error.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Cross-property validation using root object
/// var rule = new MustContextAwareRule&lt;string&gt;(
///     (email, context) =&gt; 
///     {
///         User? user = context.GetRootObject&lt;User&gt;();
///         return user == null || !string.Equals(email, user.Username, StringComparison.OrdinalIgnoreCase);
///     },
///     "Email cannot be the same as username");
/// 
/// // Service integration validation
/// var serviceRule = new MustContextAwareRule&lt;string&gt;(
///     (value, context) =&gt; 
///     {
///         IValidationService? service = context.GetService&lt;IValidationService&gt;();
///         return service?.IsValid(value) ?? false;
///     },
///     "Value failed service validation");
/// 
/// // Custom data validation
/// var dataRule = new MustContextAwareRule&lt;int&gt;(
///     (age, context) =&gt; 
///     {
///         int maxAge = context.GetCustomData&lt;int&gt;("MaxAllowedAge");
///         return maxAge == 0 || age &lt;= maxAge;
///     },
///     "Age exceeds maximum allowed limit");
/// </code>
/// </example>
public sealed class MustContextAwareRule<T>(Func<T, IValidationContext, bool> condition, string errorMessage) : IContextAwareRule<T>
{
    #region Private Members

    private readonly Func<T, IValidationContext, bool> _condition = condition ?? throw new ArgumentNullException(nameof(condition));
    private readonly string _errorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));

    #endregion

    #region IContextAwareRule<T> Implementation

    /// <summary>
    /// Validates the value using the custom condition function with access to validation context.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <param name="context">The validation context providing access to root object, services, and custom data.</param>
    /// <returns>The error message if the condition fails; otherwise, null.</returns>
    public string? Validate(T value, string displayName, IValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            bool isValid = _condition(value, context);
            return isValid ? null : _errorMessage;
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            // If condition throws an exception, treat as validation failure
            return $"Validation error: {ex.Message}";
        }
    }

    /// <summary>
    /// Validates the value using the standard IRule interface (without context).
    /// This implementation creates a minimal context for compatibility.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>The error message if validation fails; otherwise, null.</returns>
    /// <remarks>
    /// This method provides fallback compatibility with the standard IRule interface.
    /// When called without a context, a minimal empty context is created, which may
    /// limit the functionality of context-dependent validation logic.
    /// </remarks>
    public string? Validate(T value, string displayName)
    {
        IValidationContext context = ValidationContext.Create();
        return Validate(value, displayName, context);
    }

    #endregion
}