using FlowRight.Core.Results;
using FlowRight.Validation.Builders;
using FlowRight.Validation.Rules;
using System.Linq.Expressions;

namespace FlowRight.Validation.Validators;

/// <summary>
/// Abstract base class for all property validators, providing core validation functionality and fluent interface patterns
/// for building complex validation rules. This class enables type-safe validation chaining and integration with the
/// ValidationBuilder&lt;T&gt; framework.
/// </summary>
/// <typeparam name="T">The type of object being validated.</typeparam>
/// <typeparam name="TProp">The type of property being validated.</typeparam>
/// <typeparam name="TRule">The concrete validator type (used for fluent interface return types).</typeparam>
/// <remarks>
/// This class implements the Fluent Interface pattern to enable method chaining and provides a bridge between
/// property-specific validators and the main ValidationBuilder. It manages pending validation rules and applies
/// them when transitioning between properties or building the final result.
/// </remarks>
public abstract class PropertyValidator<T, TProp, TRule>
    where TRule : PropertyValidator<T, TProp, TRule>
{
    #region Private Members

    private readonly ValidationBuilder<T> _builder;
    private readonly string _displayName;
    private readonly List<(IRule<TProp>? rule, string? customMessage, Func<TProp, bool>? condition, bool executed)> _pendingRules = [];
    private readonly TProp _value;

    #endregion Private Members

    #region Internal Constructors

    /// <summary>
    /// Initializes a new instance of the PropertyValidator class.
    /// </summary>
    /// <param name="builder">The parent ValidationBuilder that manages the overall validation process.</param>
    /// <param name="displayName">The display name for this property, used in error messages.</param>
    /// <param name="value">The actual value being validated.</param>
    internal PropertyValidator(ValidationBuilder<T> builder, string displayName, TProp value)
    {
        _builder = builder;
        _displayName = displayName;
        _value = value;
    }

    #endregion Internal Constructors

    #region Public Methods

    /// <summary>
    /// Applies all pending validation rules for this property and builds the final Result&lt;T&gt;.
    /// </summary>
    /// <param name="factory">A factory function to create the validated object when all validations pass.</param>
    /// <returns>A Result&lt;T&gt; containing either the successfully created object or validation errors.</returns>
    /// <remarks>
    /// This method is a convenience shortcut that applies pending rules for the current property
    /// and immediately builds the final result. It's equivalent to calling the ValidationBuilder's
    /// Build method after all property validations are complete.
    /// </remarks>
    public Result<T> Build(Func<T> factory)
    {
        ApplyPendingRules();
        return _builder.Build(factory);
    }

    /// <summary>
    /// Validates that the property value is considered "empty" according to type-specific rules.
    /// </summary>
    /// <returns>The concrete validator type for method chaining.</returns>
    /// <remarks>
    /// Empty validation varies by type:
    /// - Strings: null or empty string
    /// - Collections: null or empty collection
    /// - Nullable types: null value
    /// - Value types: default value
    /// </remarks>
    public TRule Empty()
    {
        return AddRule(new EmptyRule<TProp>());
    }

    /// <summary>
    /// Validates that the property value equals the specified comparison value.
    /// </summary>
    /// <param name="comparisonValue">The value to compare against.</param>
    /// <param name="comparer">Optional equality comparer for comparison.</param>
    /// <returns>The concrete validator type for method chaining.</returns>
    public TRule Equal(TProp comparisonValue, IEqualityComparer<TProp>? comparer = null)
    {
        return AddRule(new EqualRule<TProp>(comparisonValue, comparer));
    }

    /// <summary>
    /// Validates the property using a custom condition function with a specified error message.
    /// </summary>
    /// <param name="condition">A function that returns true if the value is valid.</param>
    /// <param name="errorMessage">The error message to use if validation fails.</param>
    /// <returns>The concrete validator type for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x => x.Username, request.Username)
    ///     .Must(username => !ReservedUsernames.Contains(username), "Username is reserved")
    ///     .Must(username => IsUniqueUsername(username), "Username already exists");
    /// </code>
    /// </example>
    public TRule Must(Func<TProp, bool> condition, string errorMessage)
    {
        return AddRule(new MustRule<TProp>(condition, errorMessage));
    }

    /// <summary>
    /// Validates that the property value is not empty (handles strings, collections, GUIDs, etc.).
    /// </summary>
    /// <returns>The concrete validator type for method chaining.</returns>
    public TRule NotEmpty()
    {
        return AddRule(new NotEmptyRule<TProp>());
    }

    /// <summary>
    /// Validates that the property value does not equal the specified comparison value.
    /// </summary>
    /// <param name="comparisonValue">The value to compare against.</param>
    /// <param name="comparer">Optional equality comparer for comparison.</param>
    /// <returns>The concrete validator type for method chaining.</returns>
    public TRule NotEqual(TProp comparisonValue, IEqualityComparer<TProp>? comparer = null)
    {
        return AddRule(new NotEqualRule<TProp>(comparisonValue, comparer));
    }

    /// <summary>
    /// Validates that the property value is not null.
    /// </summary>
    /// <returns>The concrete validator type for method chaining.</returns>
    public TRule Notnull()
    {
        return AddRule(new NotNullRule<TProp>());
    }

    /// <summary>
    /// Validates that the property value is null.
    /// </summary>
    /// <returns>The concrete validator type for method chaining.</returns>
    public TRule Null()
    {
        return AddRule(new NullRule<TProp>());
    }

    /// <summary>
    /// Creates validation rules for a different property using a Result composition pattern.
    /// </summary>
    /// <typeparam name="TDifferentProp">The type of the different property.</typeparam>
    /// <param name="propertySelector">Expression selecting the property to validate.</param>
    /// <param name="result">The Result containing the property value or errors.</param>
    /// <param name="value">Out parameter for the validated value if successful.</param>
    /// <returns>The validation builder for method chaining.</returns>
    public ValidationBuilder<T> RuleFor<TDifferentProp>(Expression<Func<T, TDifferentProp>> propertySelector, Result<TDifferentProp> result, out TDifferentProp? value) =>
        _builder.RuleFor(propertySelector, result, out value);

    /// <summary>
    /// Creates validation rules for a string property using a fluent interface.
    /// </summary>
    /// <param name="propertySelector">Expression selecting the property to validate.</param>
    /// <param name="value">The string value for the property.</param>
    /// <param name="displayName">Optional display name for validation messages.</param>
    /// <returns>A string property validator for further rule configuration.</returns>
    public StringPropertyValidator<T> RuleFor(Expression<Func<T, string>> propertySelector, string value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    /// <summary>
    /// Creates validation rules for a GUID property using a fluent interface.
    /// </summary>
    /// <param name="propertySelector">Expression selecting the property to validate.</param>
    /// <param name="value">The GUID value for the property.</param>
    /// <param name="displayName">Optional display name for validation messages.</param>
    /// <returns>A GUID property validator for further rule configuration.</returns>
    public GuidPropertyValidator<T> RuleFor(Expression<Func<T, Guid?>> propertySelector, Guid? value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    /// <summary>
    /// Creates validation rules for an integer numeric property using a fluent interface.
    /// </summary>
    /// <param name="propertySelector">Expression selecting the property to validate.</param>
    /// <param name="value">The integer value for the property.</param>
    /// <param name="displayName">Optional display name for validation messages.</param>
    /// <returns>A numeric property validator for further rule configuration.</returns>
    public NumericPropertyValidator<T, int> RuleFor(Expression<Func<T, int>> propertySelector, int value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    /// <summary>
    /// Creates validation rules for a long numeric property using a fluent interface.
    /// </summary>
    /// <param name="propertySelector">Expression selecting the property to validate.</param>
    /// <param name="value">The long value for the property.</param>
    /// <param name="displayName">Optional display name for validation messages.</param>
    /// <returns>A numeric property validator for further rule configuration.</returns>
    public NumericPropertyValidator<T, long> RuleFor(Expression<Func<T, long>> propertySelector, long value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    /// <summary>
    /// Creates validation rules for a decimal numeric property using a fluent interface.
    /// </summary>
    /// <param name="propertySelector">Expression selecting the property to validate.</param>
    /// <param name="value">The decimal value for the property.</param>
    /// <param name="displayName">Optional display name for validation messages.</param>
    /// <returns>A numeric property validator for further rule configuration.</returns>
    public NumericPropertyValidator<T, decimal> RuleFor(Expression<Func<T, decimal>> propertySelector, decimal value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    /// <summary>
    /// Creates validation rules for a double numeric property using a fluent interface.
    /// </summary>
    /// <param name="propertySelector">Expression selecting the property to validate.</param>
    /// <param name="value">The double value for the property.</param>
    /// <param name="displayName">Optional display name for validation messages.</param>
    /// <returns>A numeric property validator for further rule configuration.</returns>
    public NumericPropertyValidator<T, double> RuleFor(Expression<Func<T, double>> propertySelector, double value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    /// <summary>
    /// Creates validation rules for a float numeric property using a fluent interface.
    /// </summary>
    /// <param name="propertySelector">Expression selecting the property to validate.</param>
    /// <param name="value">The float value for the property.</param>
    /// <param name="displayName">Optional display name for validation messages.</param>
    /// <returns>A numeric property validator for further rule configuration.</returns>
    public NumericPropertyValidator<T, float> RuleFor(Expression<Func<T, float>> propertySelector, float value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    /// <summary>
    /// Creates validation rules for a short numeric property using a fluent interface.
    /// </summary>
    /// <param name="propertySelector">Expression selecting the property to validate.</param>
    /// <param name="value">The short value for the property.</param>
    /// <param name="displayName">Optional display name for validation messages.</param>
    /// <returns>A numeric property validator for further rule configuration.</returns>
    public NumericPropertyValidator<T, short> RuleFor(Expression<Func<T, short>> propertySelector, short value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    /// <summary>
    /// Creates validation rules for an enumerable/collection property using a fluent interface.
    /// </summary>
    /// <typeparam name="TItem">The type of items in the collection.</typeparam>
    /// <param name="propertySelector">Expression selecting the property to validate.</param>
    /// <param name="value">The collection value for the property.</param>
    /// <param name="displayName">Optional display name for validation messages.</param>
    /// <returns>An enumerable property validator for further rule configuration.</returns>
    public EnumerablePropertyValidator<T, TItem> RuleFor<TItem>(Expression<Func<T, IEnumerable<TItem>>> propertySelector, IEnumerable<TItem> value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    /// <summary>
    /// Applies a conditional check to the last validation rule, only executing it when the condition is false.
    /// This is the inverse of When().
    /// </summary>
    /// <param name="condition">A function that determines when NOT to apply the previous validation rule.</param>
    /// <returns>The concrete validator type for method chaining.</returns>
    public TRule Unless(Func<TProp, bool> condition) =>
        When(value => !condition(value));

    /// <summary>
    /// Applies a conditional check to the last validation rule, only executing it when the condition is true.
    /// </summary>
    /// <param name="condition">A function that determines whether to apply the previous validation rule.</param>
    /// <returns>The concrete validator type for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x => x.ConfirmPassword, request.ConfirmPassword)
    ///     .Equal(request.Password)
    ///     .When(value => !string.IsNullOrEmpty(request.Password))
    ///     .WithMessage("Passwords must match when password is provided");
    /// </code>
    /// </example>
    public TRule When(Func<TProp, bool> condition)
    {
        UpdateLastRuleCondition(condition);
        return (TRule)this;
    }

    /// <summary>
    /// Overrides the default error message for the last validation rule with a custom message.
    /// </summary>
    /// <param name="customMessage">The custom error message to use instead of the default.</param>
    /// <returns>The concrete validator type for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x => x.Age, request.Age)
    ///     .GreaterThan(0)
    ///     .WithMessage("Character age must be positive - negative ages are not allowed in Shadowrun");
    /// </code>
    /// </example>
    public TRule WithMessage(string customMessage)
    {
        UpdateLastValidationMessage(customMessage);
        return (TRule)this;
    }

    #endregion Public Methods

    #region Protected Methods

    /// <summary>
    /// Adds a validation rule and executes it immediately.
    /// </summary>
    /// <param name="rule">The validation rule to add.</param>
    /// <returns>The concrete validator type for method chaining.</returns>
    protected TRule AddRule(IRule<TProp>? rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        // Execute rule immediately and store result
        string? error = rule.Validate(_value, _displayName);
        bool hasError = error is not null;

        if (hasError)
        {
            _builder.AddError(_displayName, error!);
        }

        // Store rule info for potential condition/message modifications
        _pendingRules.Add((rule, error, null, true));

        return (TRule)this;
    }

    /// <summary>
    /// Updates the condition function for the last added validation rule.
    /// </summary>
    /// <param name="condition">The condition function to apply to the last rule.</param>
    protected void UpdateLastRuleCondition(Func<TProp, bool>? condition)
    {
        if (_pendingRules.Count > 0)
        {
            (IRule<TProp>? rule, string? originalError, _, bool executed) = _pendingRules[^1];

            // Re-evaluate the rule with the condition
            if (condition is not null && !condition(_value))
            {
                // Condition prevents validation - remove any error that was added
                if (originalError is not null)
                {
                    RemoveLastError(_displayName);
                }
            }

            _pendingRules[^1] = (rule, originalError, condition, executed);
        }
    }

    /// <summary>
    /// Updates the custom error message for the last added validation rule.
    /// </summary>
    /// <param name="customMessage">The custom error message to use.</param>
    protected void UpdateLastValidationMessage(string? customMessage)
    {
        if (_pendingRules.Count > 0)
        {
            (IRule<TProp>? rule, string? originalError, Func<TProp, bool>? condition, bool executed) = _pendingRules[^1];

            // Replace the error message if there was an error
            if (originalError is not null)
            {
                ReplaceLastError(_displayName, customMessage ?? originalError);
            }

            _pendingRules[^1] = (rule, originalError, condition, executed);
        }
    }

    #endregion Protected Methods

    #region Private Methods

    private void ApplyPendingRules()
    {
        // With immediate execution, this method now just clears the pending rules
        // since they were already executed when added
        _pendingRules.Clear();
    }

    private void RemoveLastError(string propertyName)
    {
        _builder.RemoveLastError(propertyName);
    }

    private void ReplaceLastError(string propertyName, string newMessage)
    {
        _builder.ReplaceLastError(propertyName, newMessage);
    }

    #endregion Private Methods
}