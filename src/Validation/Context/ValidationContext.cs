namespace FlowRight.Validation.Context;

/// <summary>
/// Concrete implementation of IValidationContext that provides comprehensive validation context
/// for complex validation scenarios with object access, service integration, custom data storage,
/// and hierarchical validation support.
/// </summary>
/// <remarks>
/// <para>
/// This implementation provides a complete validation context system that enables:
/// <list type="bullet">
/// <item><description>Cross-property validation through root object access</description></item>
/// <item><description>External service integration via dependency injection</description></item>
/// <item><description>State sharing through custom data storage</description></item>
/// <item><description>Nested validation with parent-child context relationships</description></item>
/// <item><description>Rule execution tracking for conditional validation</description></item>
/// <item><description>Property path tracking for error reporting and debugging</description></item>
/// </list>
/// </para>
/// <para>
/// The ValidationContext follows an immutable design pattern where child contexts inherit
/// from their parents but cannot modify parent state, ensuring validation isolation and predictability.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create context with services and root object
/// IValidationContext context = ValidationContext.Create(user, serviceProvider);
/// context.SetCustomData("ValidationMode", "Strict");
/// 
/// // Use in ValidationBuilder
/// Result&lt;User&gt; result = new ValidationBuilder&lt;User&gt;(context)
///     .RuleFor(x =&gt; x.Email, request.Email)
///         .Must((email, ctx) =&gt; ValidateEmailWithContext(email, ctx), "Invalid email")
///     .Build(() =&gt; new User(request.Email));
/// </code>
/// </example>
public sealed class ValidationContext : IValidationContext
{
    #region Private Members

    private readonly Dictionary<string, object> _customData;
    private readonly List<string> _executedRules;
    private readonly string _propertyPath;

    #endregion

    #region Private Constructors

    /// <summary>
    /// Initializes a new instance of the ValidationContext class.
    /// </summary>
    /// <param name="rootObject">The root object being validated.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="parent">The parent validation context for hierarchical validation.</param>
    /// <param name="propertyPath">The current property path being validated.</param>
    private ValidationContext(
        object? rootObject = null,
        IServiceProvider? serviceProvider = null,
        IValidationContext? parent = null,
        string propertyPath = "")
    {
        RootObject = rootObject;
        ServiceProvider = serviceProvider;
        Parent = parent;
        _propertyPath = propertyPath;
        _customData = new Dictionary<string, object>();
        _executedRules = [];

        // Inherit custom data from parent context
        if (parent != null)
        {
            foreach (KeyValuePair<string, object> kvp in parent.CustomData)
            {
                _customData[kvp.Key] = kvp.Value;
            }
        }
    }

    #endregion

    #region IValidationContext Implementation

    #region Object Access

    /// <inheritdoc />
    public object? RootObject { get; }

    /// <inheritdoc />
    public T? GetRootObject<T>() where T : class =>
        RootObject as T;

    #endregion

    #region Service Integration

    /// <inheritdoc />
    public IServiceProvider? ServiceProvider { get; }

    /// <inheritdoc />
    public T? GetService<T>() where T : class =>
        ServiceProvider?.GetService(typeof(T)) as T;

    #endregion

    #region Custom Data Storage

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> CustomData =>
        _customData.AsReadOnly();

    /// <inheritdoc />
    public void SetCustomData(string key, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        _customData[key] = value;
    }

    /// <inheritdoc />
    public T? GetCustomData<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_customData.TryGetValue(key, out object? value))
        {
            if (value is T typedValue)
            {
                return typedValue;
            }

            // Attempt type conversion for compatible types
            try
            {
                return (T?)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                // Return default if conversion fails
                return default;
            }
        }

        return default;
    }

    /// <inheritdoc />
    public bool HasCustomData(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _customData.ContainsKey(key);
    }

    /// <inheritdoc />
    public bool RemoveCustomData(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _customData.Remove(key);
    }

    #endregion

    #region Hierarchical Context Support

    /// <inheritdoc />
    public IValidationContext CreateChildContext(object childObject, string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(childObject);

        string childPropertyPath = BuildChildPropertyPath(propertyName);
        
        return new ValidationContext(
            rootObject: RootObject ?? childObject, // Use root object if available, otherwise use child as new root
            serviceProvider: ServiceProvider,
            parent: this,
            propertyPath: childPropertyPath);
    }

    /// <inheritdoc />
    public string GetCurrentPropertyPath() => _propertyPath;

    /// <inheritdoc />
    public IValidationContext? Parent { get; }

    #endregion

    #region Rule Execution Tracking

    /// <inheritdoc />
    public IReadOnlyList<string> GetExecutedRules() =>
        _executedRules.AsReadOnly();

    /// <inheritdoc />
    public void RecordRuleExecution(string ruleIdentifier, string propertyName, bool success)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleIdentifier);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        string ruleRecord = $"{propertyName}.{ruleIdentifier}:{(success ? "Success" : "Failed")}";
        _executedRules.Add(ruleRecord);
    }

    #endregion

    #endregion

    #region Public Static Factory Methods

    /// <summary>
    /// Creates a new validation context with default settings.
    /// </summary>
    /// <returns>A new ValidationContext instance with no root object or service provider.</returns>
    /// <example>
    /// <code>
    /// IValidationContext context = ValidationContext.Create();
    /// context.SetCustomData("ValidationLevel", "Basic");
    /// </code>
    /// </example>
    public static IValidationContext Create() =>
        new ValidationContext();

    /// <summary>
    /// Creates a new validation context with the specified root object.
    /// </summary>
    /// <param name="rootObject">The root object being validated.</param>
    /// <returns>A new ValidationContext instance with the specified root object.</returns>
    /// <example>
    /// <code>
    /// User user = new UserBuilder().Build();
    /// IValidationContext context = ValidationContext.Create(user);
    /// </code>
    /// </example>
    public static IValidationContext Create(object rootObject) =>
        new ValidationContext(rootObject);

    /// <summary>
    /// Creates a new validation context with the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <returns>A new ValidationContext instance with the specified service provider.</returns>
    /// <example>
    /// <code>
    /// IValidationContext context = ValidationContext.Create(serviceProvider: myServiceProvider);
    /// </code>
    /// </example>
    public static IValidationContext Create(IServiceProvider serviceProvider) =>
        new ValidationContext(serviceProvider: serviceProvider);

    /// <summary>
    /// Creates a new validation context with the specified root object and service provider.
    /// </summary>
    /// <param name="rootObject">The root object being validated.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <returns>A new ValidationContext instance with the specified root object and service provider.</returns>
    /// <example>
    /// <code>
    /// User user = new UserBuilder().Build();
    /// IValidationContext context = ValidationContext.Create(user, serviceProvider);
    /// </code>
    /// </example>
    public static IValidationContext Create(object? rootObject = null, IServiceProvider? serviceProvider = null) =>
        new ValidationContext(rootObject, serviceProvider);

    #endregion

    #region Private Methods

    /// <summary>
    /// Builds the property path for a child context.
    /// </summary>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <returns>The full property path from root to the current property.</returns>
    private string BuildChildPropertyPath(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return _propertyPath;
        }

        if (string.IsNullOrEmpty(_propertyPath))
        {
            return propertyName;
        }

        // Handle collection index notation
        if (propertyName.StartsWith('['))
        {
            return $"{_propertyPath}{propertyName}";
        }

        return $"{_propertyPath}.{propertyName}";
    }

    #endregion
}