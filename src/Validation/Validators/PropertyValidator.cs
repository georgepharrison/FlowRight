using FlowRight.Validation.Rules;
using FlowRight.Core.Results;
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
    private readonly List<(IRule<TProp>? rule, string? customMessage, Func<TProp, bool>? condition)> _pendingRules = [];
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
        _pendingRules.Add((new EmptyRule<TProp>(), null, null));
        return (TRule)this;
    }

    public TRule Equal(TProp comparisonValue, IEqualityComparer<TProp>? comparer = null)
    {
        _pendingRules.Add((new EqualRule<TProp>(comparisonValue, comparer), null, null));
        return (TRule)this;
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
        _pendingRules.Add((new MustRule<TProp>(condition, errorMessage), null, null));
        return (TRule)this;
    }

    public TRule NotEmpty()
    {
        _pendingRules.Add((new NotEmptyRule<TProp>(), null, null));
        return (TRule)this;
    }

    public TRule NotEqual(TProp comparisonValue, IEqualityComparer<TProp>? comparer = null)
    {
        _pendingRules.Add((new NotEqualRule<TProp>(comparisonValue, comparer), null, null));
        return (TRule)this;
    }

    public TRule Notnull()
    {
        _pendingRules.Add((new NotNullRule<TProp>(), null, null));
        return (TRule)this;
    }

    public TRule Null()
    {
        _pendingRules.Add((new NullRule<TProp>(), null, null));
        return (TRule)this;
    }

    public ValidationBuilder<T> RuleFor<TDifferentProp>(Expression<Func<T, TDifferentProp>> propertySelector, Result<TDifferentProp> result, out TDifferentProp? value) =>
        _builder.RuleFor(propertySelector, result, out value);

    public StringPropertyValidator<T> RuleFor(Expression<Func<T, string>> propertySelector, string value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public GuidPropertyValidator<T> RuleFor(Expression<Func<T, Guid?>> propertySelector, Guid? value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public NumericPropertyValidator<T, int> RuleFor(Expression<Func<T, int>> propertySelector, int value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public NumericPropertyValidator<T, long> RuleFor(Expression<Func<T, long>> propertySelector, long value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public NumericPropertyValidator<T, decimal> RuleFor(Expression<Func<T, decimal>> propertySelector, decimal value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public NumericPropertyValidator<T, double> RuleFor(Expression<Func<T, double>> propertySelector, double value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public NumericPropertyValidator<T, float> RuleFor(Expression<Func<T, float>> propertySelector, float value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public NumericPropertyValidator<T, short> RuleFor(Expression<Func<T, short>> propertySelector, short value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

    public EnumerablePropertyValidator<T, TItem> RuleFor<TItem>(Expression<Func<T, IEnumerable<TItem>>> propertySelector, IEnumerable<TItem> value, string? displayName = null)
    {
        ApplyPendingRules();
        return _builder.RuleFor(propertySelector, value, displayName);
    }

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

    protected TRule AddRule(IRule<TProp>? rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        _pendingRules.Add((rule, null, null));

        return (TRule)this;
    }

    protected void UpdateLastRuleCondition(Func<TProp, bool>? condition)
    {
        if (_pendingRules.Count > 0)
        {
            (IRule<TProp>? rule, string? message, _) = _pendingRules[^1];
            _pendingRules[^1] = (rule, message, condition);
        }
    }

    protected void UpdateLastValidationMessage(string? customMessage)
    {
        if (_pendingRules.Count > 0)
        {
            (IRule<TProp>? rule, _, Func<TProp, bool>? condition) = _pendingRules[^1];
            _pendingRules[^1] = (rule, customMessage, condition);
        }
    }

    #endregion Protected Methods

    #region Private Methods

    private void ApplyPendingRules()
    {
        foreach ((IRule<TProp>? rule, string? customMessage, Func<TProp, bool>? condition) in _pendingRules)
        {
            if (rule is not null)
            {
                if (condition is not null && !condition(_value))
                {
                    continue;
                }

                string? error = rule.Validate(_value, _displayName);

                if (error is not null)
                {
                    string finalMessage = customMessage ?? error;

                    _builder.AddError(_displayName, finalMessage);
                }
            }
        }
        _pendingRules.Clear();
    }

    #endregion Private Methods
}