using FlowRight.Validation.Context;

namespace FlowRight.Validation.Rules;

/// <summary>
/// Defines a validation rule that can access validation context for complex validation scenarios.
/// This interface extends the basic IRule functionality to enable context-aware validation.
/// </summary>
/// <typeparam name="T">The type of value this rule can validate.</typeparam>
/// <remarks>
/// <para>
/// Context-aware rules enable advanced validation scenarios that require:
/// <list type="bullet">
/// <item><description>Access to the root object being validated for cross-property validation</description></item>
/// <item><description>Integration with external services through dependency injection</description></item>
/// <item><description>Access to custom data shared between validation rules</description></item>
/// <item><description>Hierarchical validation with parent-child relationships</description></item>
/// <item><description>Conditional validation based on previously executed rules</description></item>
/// </list>
/// </para>
/// <para>
/// Context-aware rules should be used when validation logic requires information beyond
/// the single property value being validated. For simple property-only validation,
/// the standard IRule&lt;T&gt; interface is more appropriate.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class EmailMustMatchUserNameRule : IContextAwareRule&lt;string&gt;
/// {
///     public string? Validate(string email, string displayName, IValidationContext context)
///     {
///         User? user = context.GetRootObject&lt;User&gt;();
///         if (user != null &amp;&amp; !email.StartsWith(user.UserName))
///         {
///             return "Email must start with username";
///         }
///         return null; // Validation passed
///     }
/// }
/// </code>
/// </example>
public interface IContextAwareRule<in T> : IRule<T>
{
    /// <summary>
    /// Validates the specified value with access to validation context and returns an error message if validation fails.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">The display name for the property being validated, used in error messages.</param>
    /// <param name="context">The validation context providing access to root object, services, and custom data.</param>
    /// <returns>An error message if validation fails; otherwise, null indicating validation passed.</returns>
    /// <remarks>
    /// This method provides full access to the validation context, enabling complex validation scenarios
    /// that require information beyond the single property value. The context parameter should not be null
    /// when called by the validation framework.
    /// </remarks>
    /// <example>
    /// <code>
    /// public string? Validate(string value, string displayName, IValidationContext context)
    /// {
    ///     // Access root object for cross-property validation
    ///     User? rootUser = context.GetRootObject&lt;User&gt;();
    ///     
    ///     // Access services for external validation
    ///     IEmailService? emailService = context.GetService&lt;IEmailService&gt;();
    ///     
    ///     // Access custom data for shared state
    ///     bool strictMode = context.GetCustomData&lt;bool&gt;("StrictValidation");
    ///     
    ///     // Perform context-aware validation logic
    ///     return ValidateWithContext(value, rootUser, emailService, strictMode);
    /// }
    /// </code>
    /// </example>
    string? Validate(T value, string displayName, IValidationContext context);
}