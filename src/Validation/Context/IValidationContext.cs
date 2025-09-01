namespace FlowRight.Validation.Context;

/// <summary>
/// Provides validation context for complex validation scenarios, enabling access to parent objects,
/// services, custom data storage, and hierarchical validation support.
/// </summary>
/// <remarks>
/// <para>
/// The ValidationContext enables sophisticated validation scenarios by providing:
/// <list type="bullet">
/// <item><description>Access to root object being validated for cross-property validation</description></item>
/// <item><description>Service provider integration for external dependency validation</description></item>
/// <item><description>Custom data storage for sharing state between validation rules</description></item>
/// <item><description>Hierarchical validation support with parent-child context relationships</description></item>
/// <item><description>Rule execution tracking for conditional validation logic</description></item>
/// <item><description>Property path tracking for nested object validation</description></item>
/// </list>
/// </para>
/// <para>
/// This interface is particularly useful for validation scenarios that require:
/// <list type="bullet">
/// <item><description>Business rule validation that depends on multiple properties</description></item>
/// <item><description>External service integration (database lookups, API calls)</description></item>
/// <item><description>Conditional validation based on previous validation results</description></item>
/// <item><description>Complex nested object validation with context propagation</description></item>
/// <item><description>State sharing between validation rules within the same validation session</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Context-aware validation with service integration
/// IValidationContext context = ValidationContext.Create(user, serviceProvider);
/// context.SetCustomData("MaxRetryAttempts", 3);
/// 
/// Result&lt;User&gt; result = new ValidationBuilder&lt;User&gt;(context)
///     .RuleFor(x =&gt; x.Email, request.Email)
///         .MustAsync(async (email, ctx) =&gt; 
///         {
///             IEmailService? emailService = ctx.GetService&lt;IEmailService&gt;();
///             return emailService != null &amp;&amp; await emailService.IsValidAsync(email);
///         }, "Email validation failed")
///     .RuleFor(x =&gt; x.Username, request.Username)
///         .Must((username, ctx) =&gt; 
///         {
///             User? rootUser = ctx.GetRootObject&lt;User&gt;();
///             return rootUser != null &amp;&amp; !string.Equals(username, rootUser.Email, StringComparison.OrdinalIgnoreCase);
///         }, "Username cannot be the same as email")
///     .Build(() =&gt; new User(request.Email, request.Username));
/// </code>
/// </example>
public interface IValidationContext
{
    #region Object Access

    /// <summary>
    /// Gets the root object being validated, if available.
    /// </summary>
    /// <value>
    /// The root object being validated, or null if no root object was provided during context creation.
    /// </value>
    /// <remarks>
    /// The root object provides access to the entire object being validated, enabling cross-property
    /// validation scenarios where one property's validation depends on the values of other properties.
    /// </remarks>
    object? RootObject { get; }

    /// <summary>
    /// Gets the root object being validated as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the root object to.</typeparam>
    /// <returns>The root object cast to type T, or null if the root object is null or not of type T.</returns>
    /// <example>
    /// <code>
    /// User? user = context.GetRootObject&lt;User&gt;();
    /// if (user != null)
    /// {
    ///     // Access user properties for validation logic
    ///     return someProperty != user.SomeOtherProperty;
    /// }
    /// </code>
    /// </example>
    T? GetRootObject<T>() where T : class;

    #endregion

    #region Service Integration

    /// <summary>
    /// Gets the service provider for dependency injection, if available.
    /// </summary>
    /// <value>
    /// The service provider instance, or null if no service provider was provided during context creation.
    /// </value>
    /// <remarks>
    /// The service provider enables validation rules to access external dependencies such as
    /// repositories, web services, or other business services required for validation logic.
    /// </remarks>
    IServiceProvider? ServiceProvider { get; }

    /// <summary>
    /// Gets a service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>The service instance if found, or null if the service is not available or no service provider exists.</returns>
    /// <example>
    /// <code>
    /// IEmailValidationService? emailService = context.GetService&lt;IEmailValidationService&gt;();
    /// if (emailService != null)
    /// {
    ///     return await emailService.IsValidEmailAsync(email);
    /// }
    /// </code>
    /// </example>
    T? GetService<T>() where T : class;

    #endregion

    #region Custom Data Storage

    /// <summary>
    /// Gets the custom data dictionary for storing validation state.
    /// </summary>
    /// <value>
    /// A read-only dictionary containing custom data set during validation.
    /// </value>
    /// <remarks>
    /// Custom data provides a mechanism for storing and sharing state between validation rules
    /// within the same validation session. This is useful for caching expensive calculations
    /// or sharing computed values across multiple validation rules.
    /// </remarks>
    IReadOnlyDictionary<string, object> CustomData { get; }

    /// <summary>
    /// Sets custom data in the validation context.
    /// </summary>
    /// <param name="key">The key to identify the data.</param>
    /// <param name="value">The value to store.</param>
    /// <remarks>
    /// Custom data is stored for the lifetime of the validation context and can be accessed
    /// by any validation rule within the same validation session.
    /// </remarks>
    /// <example>
    /// <code>
    /// context.SetCustomData("DatabaseUser", currentUser);
    /// context.SetCustomData("MaxAllowedAge", 65);
    /// </code>
    /// </example>
    void SetCustomData(string key, object value);

    /// <summary>
    /// Gets custom data from the validation context.
    /// </summary>
    /// <typeparam name="T">The type of data to retrieve.</typeparam>
    /// <param name="key">The key identifying the data.</param>
    /// <returns>The data value cast to type T, or the default value of T if the key is not found or cannot be cast.</returns>
    /// <example>
    /// <code>
    /// int maxAge = context.GetCustomData&lt;int&gt;("MaxAllowedAge");
    /// User? dbUser = context.GetCustomData&lt;User&gt;("DatabaseUser");
    /// </code>
    /// </example>
    T? GetCustomData<T>(string key);

    /// <summary>
    /// Determines whether the validation context contains custom data with the specified key.
    /// </summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>true if the context contains data with the specified key; otherwise, false.</returns>
    bool HasCustomData(string key);

    /// <summary>
    /// Removes custom data from the validation context.
    /// </summary>
    /// <param name="key">The key identifying the data to remove.</param>
    /// <returns>true if the data was found and removed; otherwise, false.</returns>
    bool RemoveCustomData(string key);

    #endregion

    #region Hierarchical Context Support

    /// <summary>
    /// Creates a child validation context for nested object validation.
    /// </summary>
    /// <param name="childObject">The child object being validated.</param>
    /// <param name="propertyName">The name of the property containing the child object.</param>
    /// <returns>A new validation context that inherits from the current context.</returns>
    /// <remarks>
    /// Child contexts inherit custom data and service providers from their parent context,
    /// enabling consistent validation behavior across nested object hierarchies.
    /// </remarks>
    /// <example>
    /// <code>
    /// IValidationContext childContext = parentContext.CreateChildContext(user.Profile, "Profile");
    /// // Child context has access to parent's custom data and services
    /// </code>
    /// </example>
    IValidationContext CreateChildContext(object childObject, string? propertyName = null);

    /// <summary>
    /// Gets the current property path being validated.
    /// </summary>
    /// <returns>The full property path from the root object to the current property being validated.</returns>
    /// <remarks>
    /// Property paths use dot notation for nested properties (e.g., "User.Profile.Bio") and
    /// bracket notation for collection indices (e.g., "User.Roles[0].Name").
    /// </remarks>
    /// <example>
    /// <code>
    /// string path = context.GetCurrentPropertyPath(); // Returns "User.Profile.Bio"
    /// </code>
    /// </example>
    string GetCurrentPropertyPath();

    /// <summary>
    /// Gets the parent validation context, if this is a child context.
    /// </summary>
    /// <value>
    /// The parent validation context, or null if this is a root context.
    /// </value>
    IValidationContext? Parent { get; }

    #endregion

    #region Rule Execution Tracking

    /// <summary>
    /// Gets a read-only list of validation rules that have been executed in this context.
    /// </summary>
    /// <returns>A list of rule identifiers representing the validation rules that have been executed.</returns>
    /// <remarks>
    /// Rule execution tracking enables conditional validation logic where the execution of
    /// subsequent rules can depend on which previous rules have been executed and their results.
    /// </remarks>
    /// <example>
    /// <code>
    /// IReadOnlyList&lt;string&gt; executedRules = context.GetExecutedRules();
    /// bool nameValidated = executedRules.Any(rule =&gt; rule.Contains("Name.NotEmpty"));
    /// </code>
    /// </example>
    IReadOnlyList<string> GetExecutedRules();

    /// <summary>
    /// Records that a validation rule has been executed.
    /// </summary>
    /// <param name="ruleIdentifier">A unique identifier for the validation rule.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="success">Whether the validation rule passed or failed.</param>
    /// <remarks>
    /// This method is typically called internally by the validation framework to track
    /// rule execution for conditional validation scenarios.
    /// </remarks>
    void RecordRuleExecution(string ruleIdentifier, string propertyName, bool success);

    #endregion
}