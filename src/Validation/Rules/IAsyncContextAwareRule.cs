using FlowRight.Validation.Context;

namespace FlowRight.Validation.Rules;

/// <summary>
/// Defines an asynchronous validation rule that can access validation context for complex validation scenarios.
/// This interface enables context-aware validation with async operations such as database lookups or web service calls.
/// </summary>
/// <typeparam name="T">The type of value this rule can validate.</typeparam>
/// <remarks>
/// <para>
/// Async context-aware rules enable advanced validation scenarios that require:
/// <list type="bullet">
/// <item><description>Asynchronous operations such as database queries or web service calls</description></item>
/// <item><description>Access to the root object being validated for cross-property validation</description></item>
/// <item><description>Integration with external services through dependency injection</description></item>
/// <item><description>Access to custom data shared between validation rules</description></item>
/// <item><description>Hierarchical validation with parent-child relationships</description></item>
/// <item><description>Conditional validation based on previously executed rules</description></item>
/// </list>
/// </para>
/// <para>
/// Async context-aware rules should be used when validation logic requires asynchronous operations
/// and information beyond the single property value being validated. For simple synchronous validation,
/// use IContextAwareRule&lt;T&gt; or IRule&lt;T&gt; interfaces.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class UniqueEmailRule : IAsyncContextAwareRule&lt;string&gt;
/// {
///     public async Task&lt;string?&gt; ValidateAsync(string email, string displayName, IValidationContext context)
///     {
///         IUserRepository? repository = context.GetService&lt;IUserRepository&gt;();
///         if (repository != null)
///         {
///             bool exists = await repository.EmailExistsAsync(email);
///             if (exists)
///             {
///                 return "Email address is already in use";
///             }
///         }
///         return null; // Validation passed
///     }
/// }
/// </code>
/// </example>
public interface IAsyncContextAwareRule<in T>
{
    /// <summary>
    /// Asynchronously validates the specified value with access to validation context and returns an error message if validation fails.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">The display name for the property being validated, used in error messages.</param>
    /// <param name="context">The validation context providing access to root object, services, and custom data.</param>
    /// <returns>
    /// A task that represents the asynchronous validation operation. The task result contains
    /// an error message if validation fails; otherwise, null indicating validation passed.
    /// </returns>
    /// <remarks>
    /// This method provides full access to the validation context and supports asynchronous operations,
    /// enabling complex validation scenarios that require external data access or web service calls.
    /// The context parameter should not be null when called by the validation framework.
    /// </remarks>
    /// <example>
    /// <code>
    /// public async Task&lt;string?&gt; ValidateAsync(string value, string displayName, IValidationContext context)
    /// {
    ///     // Access services for async external validation
    ///     IEmailValidationService? emailService = context.GetService&lt;IEmailValidationService&gt;();
    ///     if (emailService != null)
    ///     {
    ///         bool isValid = await emailService.ValidateEmailAsync(value);
    ///         if (!isValid)
    ///         {
    ///             return $"{displayName} failed external validation";
    ///         }
    ///     }
    ///     
    ///     // Access custom data for configuration
    ///     int maxRetries = context.GetCustomData&lt;int&gt;("MaxRetries");
    ///     
    ///     return null; // Validation passed
    /// }
    /// </code>
    /// </example>
    Task<string?> ValidateAsync(T value, string displayName, IValidationContext context);
}