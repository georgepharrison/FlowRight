using FlowRight.Core.Results;
using FlowRight.Validation.Rules;
using FlowRight.Validation.Validators;
using System.Linq.Expressions;
using System.Numerics;

namespace FlowRight.Validation.Builders;

/// <summary>
/// A fluent validation builder that integrates with the Result&lt;T&gt; pattern to provide comprehensive validation capabilities
/// with automatic error aggregation and Result&lt;T&gt; composition support.
/// </summary>
/// <typeparam name="T">The type of object being validated and constructed.</typeparam>
/// <example>
/// <para><strong>Basic Usage:</strong></para>
/// <code>
/// Result&lt;Character&gt; result = new ValidationBuilder&lt;Character&gt;()
///     .RuleFor(x =&gt; x.Name, request.Name)
///         .NotEmpty()
///         .MaximumLength(50)
///     .RuleFor(x =&gt; x.Age, request.Age)
///         .GreaterThan(0)
///         .LessThan(120)
///     .Build(() =&gt; new Character(request.Name, request.Age));
/// </code>
/// 
/// <para><strong>Result&lt;T&gt; Composition:</strong></para>
/// <code>
/// Result&lt;Character&gt; result = new ValidationBuilder&lt;Character&gt;()
///     .RuleFor(x =&gt; x.Attributes, AttributeSet.Create(attributes), out AttributeSet? validatedAttributes)
///     .RuleFor(x =&gt; x.Edge, Edge.Create(startingEdge), out Edge? validatedEdge)
///     .Build(() =&gt; new Character(name, validatedAttributes!, validatedEdge!));
/// </code>
/// 
/// <para><strong>Conditional Validation:</strong></para>
/// <code>
/// ValidationBuilder&lt;DicePool&gt; builder = new();
/// return builder
///     .RuleFor(x =&gt; x.Limit, limit)
///         .GreaterThanOrEqualTo(0)
///         .When(value =&gt; !ignoreLimit)
///         .WithMessage("Limit must be specified when not using Edge");
/// </code>
/// </example>
public class ValidationBuilder<T>
{
    #region Private Members

    private readonly Dictionary<string, List<string>> _errors = [];

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Builds the final Result&lt;T&gt; by validating all accumulated rules and constructing the object if validation succeeds.
    /// </summary>
    /// <param name="factory">A factory function to create the validated object when all validations pass.</param>
    /// <returns>
    /// A Result&lt;T&gt; containing either the successfully created object or a validation failure with all accumulated errors.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when factory is null.</exception>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; result = builder.Build(() =&gt; new User(validatedName, validatedEmail));
    /// 
    /// return result.Match(
    ///     onSuccess: user =&gt; Ok(user),
    ///     onValidationException: errors =&gt; BadRequest(errors));
    /// </code>
    /// </example>
    public Result<T> Build(Func<T> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return HasErrors
            ? Result.Failure<T>(GetErrors())
            : Result.Success(factory());
    }

    /// <summary>
    /// Gets all accumulated validation errors as a dictionary mapping property names to error message arrays.
    /// </summary>
    /// <returns>
    /// A dictionary where keys are property names and values are arrays of error messages for that property.
    /// Returns an empty dictionary if no validation errors have been accumulated.
    /// </returns>
    /// <example>
    /// <code>
    /// ValidationBuilder&lt;User&gt; builder = new();
    /// builder.RuleFor(x =&gt; x.Name, "").NotEmpty();
    /// builder.RuleFor(x =&gt; x.Email, "invalid").EmailAddress();
    /// 
    /// Dictionary&lt;string, string[]&gt; errors = builder.GetErrors();
    /// // errors["Name"] contains ["Name is required"]
    /// // errors["Email"] contains ["Email address format is invalid"]
    /// </code>
    /// </example>
    public Dictionary<string, string[]> GetErrors() =>
        _errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());

    /// <summary>
    /// Creates validation rules for a Guid? property using a fluent interface.
    /// </summary>
    /// <param name="propertySelector">An expression selecting the property to validate (e.g., x =&gt; x.Id).</param>
    /// <param name="value">The actual value to be validated.</param>
    /// <param name="displayName">Optional custom display name for error messages. If null, uses the property name from the expression.</param>
    /// <returns>A GuidPropertyValidator&lt;T&gt; for chaining additional validation rules.</returns>
    /// <example>
    /// <code>
    /// ValidationBuilder&lt;Entity&gt; builder = new();
    /// builder.RuleFor(x =&gt; x.Id, request.Id)
    ///     .NotEqual(Guid.Empty)
    ///     .WithMessage("Entity ID cannot be empty");
    /// </code>
    /// </example>
    public GuidPropertyValidator<T> RuleFor(Expression<Func<T, Guid?>> propertySelector, Guid? value, string? displayName = null) =>
        CreateValidator(propertySelector, value, displayName, (builder, display, val) => new GuidPropertyValidator<T>(builder, display, val));

    /// <summary>
    /// Creates validation rules for a string property using a fluent interface.
    /// </summary>
    /// <param name="propertySelector">An expression selecting the property to validate (e.g., x =&gt; x.Name).</param>
    /// <param name="value">The actual string value to be validated.</param>
    /// <param name="displayName">Optional custom display name for error messages. If null, uses the property name from the expression.</param>
    /// <returns>A StringPropertyValidator&lt;T&gt; for chaining additional string-specific validation rules.</returns>
    /// <example>
    /// <code>
    /// ValidationBuilder&lt;User&gt; builder = new();
    /// builder.RuleFor(x =&gt; x.Name, request.Name)
    ///     .NotEmpty()
    ///     .MinimumLength(2)
    ///     .MaximumLength(50)
    ///     .Matches("^[a-zA-Z ]+$")
    ///     .WithMessage("Name must contain only letters and spaces");
    /// </code>
    /// </example>
    public StringPropertyValidator<T> RuleFor(Expression<Func<T, string>> propertySelector, string value, string? displayName = null) =>
        CreateValidator(propertySelector, value, displayName, (builder, display, val) => new StringPropertyValidator<T>(builder, display, val));

    /// <summary>
    /// Integrates Result&lt;T&gt; validation into the validation builder, automatically extracting failures and providing access to successful values.
    /// This is the key method that enables composition of multiple Result&lt;T&gt; operations in a single validation chain.
    /// </summary>
    /// <typeparam name="TProp">The type of property being validated through a Result&lt;T&gt;.</typeparam>
    /// <param name="propertySelector">An expression selecting the property (e.g., x =&gt; x.Attributes).</param>
    /// <param name="result">A Result&lt;TProp&gt; containing either a successful value or validation errors.</param>
    /// <param name="value">Output parameter providing access to the successful value for object construction, or default if validation failed.</param>
    /// <returns>The ValidationBuilder&lt;T&gt; for continued chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method automatically handles all Result&lt;T&gt; failure types:
    /// - Simple errors: Added as single error messages
    /// - Validation exceptions: Merged with existing validation errors
    /// - Security exceptions: Added as security-related errors
    /// - Operation cancelled exceptions: Added as cancellation errors
    /// </para>
    /// <para>
    /// The out parameter allows access to successful validation results for object construction,
    /// enabling patterns like: new Character(name, validatedAttributes!, validatedEdge!)
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Compose multiple Result&lt;T&gt; validations
    /// Result&lt;Character&gt; result = new ValidationBuilder&lt;Character&gt;()
    ///     .RuleFor(x =&gt; x.Attributes, AttributeSet.Create(request.Attributes), out AttributeSet? attributes)
    ///     .RuleFor(x =&gt; x.Edge, Edge.Create(request.StartingEdge), out Edge? edge)
    ///     .RuleFor(x =&gt; x.Health, ConditionMonitor.Create(attributes), out ConditionMonitor? health)
    ///     .Build(() =&gt; new Character(request.Name, attributes!, edge!, health!));
    /// 
    /// // All validation failures are automatically aggregated
    /// // Success values are available through out parameters
    /// </code>
    /// </example>
    public ValidationBuilder<T> RuleFor<TProp>(Expression<Func<T, TProp>> propertySelector, Result<TProp> result, out TProp? value)
    {
        value = result is null
            ? default
            : result.Match(
                onSuccess: successValue => successValue,
                onError: error =>
                {
                    AddError(GetPropertyName(propertySelector), error);
                    return default!;
                },
                onSecurityException: error =>
                {
                    AddError(GetPropertyName(propertySelector), error);
                    return default!;
                },
                onOperationCanceledException: error =>
                {
                    AddError(GetPropertyName(propertySelector), error);
                    return default!;
                },
                onValidationException: failures =>
                {
                    foreach (KeyValuePair<string, string[]> item in failures)
                    {
                        if (!_errors.TryGetValue(item.Key, out List<string>? errors))
                        {
                            _errors[item.Key] = errors ??= [];
                        }
                        errors.AddRange(item.Value);
                    }
                    return default!;
                }
            );

        return this;
    }

    /// <summary>
    /// Creates validation rules for an integer property using a fluent interface.
    /// </summary>
    /// <param name="propertySelector">An expression selecting the property to validate (e.g., x =&gt; x.Age).</param>
    /// <param name="value">The actual integer value to be validated.</param>
    /// <param name="displayName">Optional custom display name for error messages. If null, uses the property name from the expression.</param>
    /// <returns>A NumericPropertyValidator&lt;T, int&gt; for chaining additional numeric validation rules.</returns>
    /// <example>
    /// <code>
    /// ValidationBuilder&lt;User&gt; builder = new();
    /// builder.RuleFor(x =&gt; x.Age, request.Age)
    ///     .GreaterThan(0)
    ///     .LessThan(120)
    ///     .WithMessage("Age must be between 1 and 119");
    /// </code>
    /// </example>
    public NumericPropertyValidator<T, int> RuleFor(Expression<Func<T, int>> propertySelector, int value, string? displayName = null) =>
        CreateNumericValidator(propertySelector, value, displayName);

    public NumericPropertyValidator<T, long> RuleFor(Expression<Func<T, long>> propertySelector, long value, string? displayName = null) =>
        CreateNumericValidator(propertySelector, value, displayName);

    public NumericPropertyValidator<T, decimal> RuleFor(Expression<Func<T, decimal>> propertySelector, decimal value, string? displayName = null) =>
        CreateNumericValidator(propertySelector, value, displayName);

    public NumericPropertyValidator<T, double> RuleFor(Expression<Func<T, double>> propertySelector, double value, string? displayName = null) =>
        CreateNumericValidator(propertySelector, value, displayName);

    public NumericPropertyValidator<T, float> RuleFor(Expression<Func<T, float>> propertySelector, float value, string? displayName = null) =>
        CreateNumericValidator(propertySelector, value, displayName);

    public NumericPropertyValidator<T, short> RuleFor(Expression<Func<T, short>> propertySelector, short value, string? displayName = null) =>
        CreateNumericValidator(propertySelector, value, displayName);

    /// <summary>
    /// Creates validation rules for an enumerable property using a fluent interface.
    /// </summary>
    /// <typeparam name="TItem">The type of items in the enumerable.</typeparam>
    /// <param name="propertySelector">An expression selecting the property to validate (e.g., x =&gt; x.Skills).</param>
    /// <param name="value">The actual enumerable value to be validated.</param>
    /// <param name="displayName">Optional custom display name for error messages. If null, uses the property name from the expression.</param>
    /// <returns>An EnumerablePropertyValidator&lt;T, TItem&gt; for chaining additional collection-specific validation rules.</returns>
    /// <example>
    /// <code>
    /// ValidationBuilder&lt;Character&gt; builder = new();
    /// builder.RuleFor(x =&gt; x.Skills, request.Skills)
    ///     .NotEmpty()
    ///     .MinCount(1)
    ///     .MaxCount(20)
    ///     .Unique()
    ///     .WithMessage("Character must have 1-20 unique skills");
    /// </code>
    /// </example>
    public EnumerablePropertyValidator<T, TItem> RuleFor<TItem>(Expression<Func<T, IEnumerable<TItem>>> propertySelector, IEnumerable<TItem> value, string? displayName = null) =>
        CreateValidator(propertySelector, value, displayName, (builder, display, val) => new EnumerablePropertyValidator<T, TItem>(builder, display, val));

    /// <summary>
    /// Creates validation rules for any property type using a fluent interface. This is the fallback validator for types 
    /// that don't have specialized validators (string, numeric, enumerable, guid).
    /// </summary>
    /// <typeparam name="TProp">The type of property being validated.</typeparam>
    /// <param name="propertySelector">An expression selecting the property to validate (e.g., x =&gt; x.CustomType).</param>
    /// <param name="value">The actual value to be validated.</param>
    /// <param name="displayName">Optional custom display name for error messages. If null, uses the property name from the expression.</param>
    /// <returns>A GenericPropertyValidator&lt;T, TProp&gt; for chaining general validation rules.</returns>
    /// <example>
    /// <code>
    /// ValidationBuilder&lt;Order&gt; builder = new();
    /// builder.RuleFor(x =&gt; x.Status, request.Status)
    ///     .NotNull()
    ///     .Must(status =&gt; status != OrderStatus.Invalid, "Order status cannot be Invalid")
    ///     .Equal(OrderStatus.Pending);
    /// </code>
    /// </example>
    public GenericPropertyValidator<T, TProp> RuleFor<TProp>(Expression<Func<T, TProp>> propertySelector, TProp value, string? displayName = null) =>
        CreateValidator(propertySelector, value, displayName, (builder, display, val) => new GenericPropertyValidator<T, TProp>(builder, display, val));

    #endregion Public Methods

    #region Internal Methods

    /// <summary>
    /// Adds a validation error for the specified property. Used internally by property validators
    /// and for Result&lt;T&gt; integration.
    /// </summary>
    /// <param name="propertyName">The name of the property that failed validation.</param>
    /// <param name="error">The error message describing the validation failure.</param>
    /// <remarks>
    /// This method is internal and used by the validation framework. Multiple errors can be added
    /// for the same property, and they will be accumulated in a list.
    /// </remarks>
    internal void AddError(string propertyName, string error)
    {
        if (!_errors.TryGetValue(propertyName, out List<string>? value))
        {
            value = [];

            _errors[propertyName] = value;
        }

        value.Add(error);
    }

    #endregion Internal Methods

    #region Private Methods

    /// <summary>
    /// Extracts the property name from a lambda expression for use in error messages.
    /// </summary>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="propertySelector">The lambda expression selecting a property (e.g., x =&gt; x.Name).</param>
    /// <returns>The name of the property (e.g., "Name").</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is not a valid property selector.</exception>
    private static string GetPropertyName<TProp>(Expression<Func<T, TProp>> propertySelector) =>
        propertySelector?.Body is MemberExpression member
            ? member.Member.Name
            : throw new ArgumentException("Expression must be a property selector");

    private NumericPropertyValidator<T, TProp> CreateNumericValidator<TProp>(Expression<Func<T, TProp>> propertySelector, TProp value, string? displayName) where TProp : struct, INumber<TProp> =>
        CreateValidator(propertySelector, value, displayName, (builder, display, val) => new NumericPropertyValidator<T, TProp>(builder, display, val));

    private TValidator CreateValidator<TValidator, TProp>(
        Expression<Func<T, TProp>> propertySelector,
        TProp value,
        string? displayName,
        Func<ValidationBuilder<T>, string, TProp, TValidator> factory)
    {
        string propertyName = displayName ?? GetPropertyName(propertySelector);
        return factory(this, propertyName, value);
    }

    #endregion Private Methods

    #region Public Properties

    /// <summary>
    /// Gets a value indicating whether any validation errors have been accumulated.
    /// </summary>
    /// <value>
    /// <c>true</c> if there are validation errors; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property is used internally by the Build method to determine whether to return
    /// a success or failure result. It can also be used externally to check validation state
    /// before calling Build().
    /// </remarks>
    public bool HasErrors =>
        _errors.Count is not 0;

    #endregion Public Properties
}