using FlowRight.Validation.Builders;
using FlowRight.Validation.Rules;
using System.Numerics;

namespace FlowRight.Validation.Validators;

/// <summary>
/// Provides fluent validation rules specifically designed for numeric properties, offering comprehensive
/// numeric comparison and range validation capabilities for any type implementing INumber&lt;T&gt;.
/// </summary>
/// <typeparam name="T">The type of object being validated.</typeparam>
/// <typeparam name="TNumeric">The numeric type being validated (int, long, decimal, double, float, short, etc.).</typeparam>
/// <example>
/// <code>
/// // Integer validation
/// ValidationBuilder&lt;Character&gt; builder = new();
/// builder.RuleFor(x =&gt; x.Age, request.Age)
///     .GreaterThan(0)
///     .LessThanOrEqualTo(1000)
///     .WithMessage("Character age must be between 1 and 1000");
///
/// // Decimal validation with precision
/// builder.RuleFor(x =&gt; x.Price, request.Price)
///     .GreaterThanOrEqualTo(0.01m)
///     .LessThan(1000000m)
///     .PrecisionScale(10, 2)
///     .WithMessage("Price must be positive with up to 2 decimal places");
///
/// // Range validation
/// builder.RuleFor(x =&gt; x.DiceCount, dicePool.TotalDice)
///     .InclusiveBetween(1, 100)
///     .WithMessage("Dice pool must contain 1-100 dice");
/// </code>
/// </example>
public sealed class NumericPropertyValidator<T, TNumeric> : PropertyValidator<T, TNumeric, NumericPropertyValidator<T, TNumeric>>
    where TNumeric : struct, INumber<TNumeric>
{
    #region Internal Constructors

    /// <summary>
    /// Initializes a new instance of the NumericPropertyValidator class.
    /// </summary>
    /// <param name="builder">The parent validation builder.</param>
    /// <param name="displayName">The display name for the property in error messages.</param>
    /// <param name="value">The numeric value to validate.</param>
    internal NumericPropertyValidator(ValidationBuilder<T> builder, string displayName, TNumeric value)
        : base(builder, displayName, value)
    {
    }

    #endregion Internal Constructors

    #region Public Methods

    /// <summary>
    /// Validates that the numeric value is strictly between the specified bounds (exclusive).
    /// </summary>
    /// <param name="from">The lower bound (exclusive).</param>
    /// <param name="to">The upper bound (exclusive).</param>
    /// <returns>The NumericPropertyValidator&lt;T, TNumeric&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.Percentage, request.Percentage)
    ///     .ExclusiveBetween(0, 100)
    ///     .WithMessage("Percentage must be between 0 and 100 (exclusive)");
    ///
    /// // Valid: 0.1, 50, 99.9
    /// // Invalid: 0, 100, -1, 101
    /// </code>
    /// </example>
    public NumericPropertyValidator<T, TNumeric> ExclusiveBetween(TNumeric from, TNumeric to) =>
        AddRule(new ExclusiveBetweenRule<TNumeric>(from, to));

    /// <summary>
    /// Validates that the numeric value is greater than the specified comparison value.
    /// </summary>
    /// <param name="valueToCompare">The value to compare against.</param>
    /// <returns>The NumericPropertyValidator&lt;T, TNumeric&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.Price, request.Price)
    ///     .GreaterThan(0)
    ///     .WithMessage("Price must be positive");
    ///
    /// builder.RuleFor(x =&gt; x.Rating, request.Rating)
    ///     .GreaterThan(0.0)
    ///     .WithMessage("Rating must be above zero");
    /// </code>
    /// </example>
    public NumericPropertyValidator<T, TNumeric> GreaterThan(TNumeric valueToCompare) =>
        AddRule(new GreaterThanRule<TNumeric>(valueToCompare));

    /// <summary>
    /// Validates that the numeric value is greater than or equal to the specified comparison value.
    /// </summary>
    /// <param name="valueToCompare">The value to compare against.</param>
    /// <returns>The NumericPropertyValidator&lt;T, TNumeric&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.MinimumAge, request.MinimumAge)
    ///     .GreaterThanOrEqualTo(18)
    ///     .WithMessage("Minimum age must be 18 or older");
    ///
    /// builder.RuleFor(x =&gt; x.Balance, account.Balance)
    ///     .GreaterThanOrEqualTo(0.00m)
    ///     .WithMessage("Account balance cannot be negative");
    /// </code>
    /// </example>
    public NumericPropertyValidator<T, TNumeric> GreaterThanOrEqualTo(TNumeric valueToCompare) =>
        AddRule(new GreaterThanOrEqualToRule<TNumeric>(valueToCompare));

    /// <summary>
    /// Validates that the numeric value is between the specified bounds (inclusive).
    /// </summary>
    /// <param name="from">The lower bound (inclusive).</param>
    /// <param name="to">The upper bound (inclusive).</param>
    /// <returns>The NumericPropertyValidator&lt;T, TNumeric&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.DiceRoll, roll.Value)
    ///     .InclusiveBetween(1, 6)
    ///     .WithMessage("Dice roll must be between 1 and 6");
    ///
    /// builder.RuleFor(x =&gt; x.Attribute, character.Strength)
    ///     .InclusiveBetween(1, 12)
    ///     .WithMessage("Shadowrun attributes range from 1 to 12");
    /// </code>
    /// </example>
    public NumericPropertyValidator<T, TNumeric> InclusiveBetween(int from, int to) =>
        AddRule(new InclusiveBetweenRule(from, to) as IRule<TNumeric>);

    /// <summary>
    /// Validates that the numeric value is less than the specified comparison value.
    /// </summary>
    /// <param name="valueToCompare">The value to compare against.</param>
    /// <returns>The NumericPropertyValidator&lt;T, TNumeric&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.MaxUsers, request.MaxUsers)
    ///     .LessThan(10000)
    ///     .WithMessage("Maximum users must be under 10,000");
    ///
    /// builder.RuleFor(x =&gt; x.Discount, request.Discount)
    ///     .LessThan(1.0)
    ///     .WithMessage("Discount must be less than 100%");
    /// </code>
    /// </example>
    public NumericPropertyValidator<T, TNumeric> LessThan(TNumeric valueToCompare) =>
        AddRule(new LessThanRule<TNumeric>(valueToCompare));

    /// <summary>
    /// Validates that the numeric value is less than or equal to the specified comparison value.
    /// </summary>
    /// <param name="valueToCompare">The value to compare against.</param>
    /// <returns>The NumericPropertyValidator&lt;T, TNumeric&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.MaxRetries, request.MaxRetries)
    ///     .LessThanOrEqualTo(5)
    ///     .WithMessage("Maximum retries cannot exceed 5");
    ///
    /// builder.RuleFor(x =&gt; x.CompletionRate, task.CompletionRate)
    ///     .LessThanOrEqualTo(1.0)
    ///     .WithMessage("Completion rate cannot exceed 100%");
    /// </code>
    /// </example>
    public NumericPropertyValidator<T, TNumeric> LessThanOrEqualTo(TNumeric valueToCompare) =>
        AddRule(new LessThanOrEqualToRule<TNumeric>(valueToCompare));

    /// <summary>
    /// Validates that the numeric value conforms to the specified precision and scale requirements,
    /// typically used for decimal types to ensure database compatibility.
    /// </summary>
    /// <param name="precision">The maximum number of digits (total digits).</param>
    /// <param name="scale">The maximum number of decimal places.</param>
    /// <returns>The NumericPropertyValidator&lt;T, TNumeric&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// // Currency validation: up to 10 digits total, 2 decimal places
    /// builder.RuleFor(x =&gt; x.Price, request.Price)
    ///     .PrecisionScale(10, 2)
    ///     .WithMessage("Price must have at most 8 whole digits and 2 decimal places");
    ///
    /// // Percentage with high precision: up to 5 digits total, 3 decimal places
    /// builder.RuleFor(x =&gt; x.InterestRate, request.InterestRate)
    ///     .PrecisionScale(5, 3)
    ///     .WithMessage("Interest rate precision is too high");
    ///
    /// // Valid examples for PrecisionScale(10, 2):
    /// // 12345678.90, 0.01, 99999999.99
    /// // Invalid: 123456789.123 (too many decimal places), 12345678901 (too many total digits)
    /// </code>
    /// </example>
    public NumericPropertyValidator<T, TNumeric> PrecisionScale(int precision, int scale) =>
        AddRule(new PrecisionScaleRule<TNumeric>(precision, scale));

    #endregion Public Methods
}