using FlowRight.Validation.Context;

namespace FlowRight.Validation.Rules;

/// <summary>
/// An asynchronous context-aware validation rule that applies a custom condition function with access to validation context.
/// This rule enables complex validation scenarios that require async operations and access to root objects, services, or custom data.
/// </summary>
/// <typeparam name="T">The type of value to validate.</typeparam>
/// <param name="condition">The async condition function that receives both the value and validation context.</param>
/// <param name="errorMessage">The error message to return when validation fails.</param>
/// <remarks>
/// <para>
/// This rule extends validation capabilities to support asynchronous operations while providing access to the validation context.
/// It enables validation logic that depends on:
/// <list type="bullet">
/// <item><description>Asynchronous database queries or web service calls</description></item>
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
/// // Async database validation with context
/// var rule = new MustAsyncContextAwareRule&lt;string&gt;(
///     async (email, context) =&gt; 
///     {
///         IUserRepository? repository = context.GetService&lt;IUserRepository&gt;();
///         if (repository != null)
///         {
///             return !await repository.EmailExistsAsync(email);
///         }
///         return false;
///     },
///     "Email address is already in use");
/// 
/// // Async web service validation with cross-property check
/// var serviceRule = new MustAsyncContextAwareRule&lt;string&gt;(
///     async (value, context) =&gt; 
///     {
///         User? user = context.GetRootObject&lt;User&gt;();
///         IValidationService? service = context.GetService&lt;IValidationService&gt;();
///         
///         if (user != null &amp;&amp; service != null)
///         {
///             return await service.ValidateWithUserContextAsync(value, user.Id);
///         }
///         return false;
///     },
///     "Value failed external validation service");
/// 
/// // Async validation with custom data and retry logic
/// var retryRule = new MustAsyncContextAwareRule&lt;string&gt;(
///     async (value, context) =&gt; 
///     {
///         int maxRetries = context.GetCustomData&lt;int&gt;("MaxRetries");
///         IExternalService? service = context.GetService&lt;IExternalService&gt;();
///         
///         for (int i = 0; i &lt;= maxRetries; i++)
///         {
///             try
///             {
///                 return await service?.ValidateAsync(value) ?? false;
///             }
///             catch when (i &lt; maxRetries)
///             {
///                 await Task.Delay(TimeSpan.FromMilliseconds(100 * (i + 1)));
///             }
///         }
///         return false;
///     },
///     "External validation failed after retries");
/// </code>
/// </example>
public sealed class MustAsyncContextAwareRule<T>(Func<T, IValidationContext, Task<bool>> condition, string errorMessage) : IAsyncContextAwareRule<T>
{
    #region Private Members

    private readonly Func<T, IValidationContext, Task<bool>> _condition = condition ?? throw new ArgumentNullException(nameof(condition));
    private readonly string _errorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));

    #endregion

    #region IAsyncContextAwareRule<T> Implementation

    /// <summary>
    /// Asynchronously validates the value using the custom condition function with access to validation context.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <param name="context">The validation context providing access to root object, services, and custom data.</param>
    /// <returns>
    /// A task that represents the asynchronous validation operation. The task result contains
    /// an error message if validation fails; otherwise, null indicating validation passed.
    /// </returns>
    public async Task<string?> ValidateAsync(T value, string displayName, IValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            bool isValid = await _condition(value, context);
            return isValid ? null : _errorMessage;
        }
        catch (Exception ex) when (ex is not ArgumentNullException and not TaskCanceledException and not OperationCanceledException)
        {
            // If condition throws an exception, treat as validation failure
            return $"Async validation error: {ex.Message}";
        }
    }

    #endregion
}