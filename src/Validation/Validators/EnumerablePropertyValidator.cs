using FlowRight.Validation.Builders;
using FlowRight.Validation.Rules;

namespace FlowRight.Validation.Validators;

/// <summary>
/// Provides validation rules for enumerable/collection properties with fluent configuration.
/// </summary>
/// <typeparam name="T">The type containing the property being validated.</typeparam>
/// <typeparam name="TItem">The type of items in the collection.</typeparam>
public sealed class EnumerablePropertyValidator<T, TItem> : PropertyValidator<T, IEnumerable<TItem>, EnumerablePropertyValidator<T, TItem>>
{
    #region Internal Constructors

    internal EnumerablePropertyValidator(ValidationBuilder<T> builder, string displayName, IEnumerable<TItem> value)
        : base(builder, displayName, value)
    {
    }

    #endregion Internal Constructors

    #region Public Methods

    /// <summary>
    /// Validates that the collection contains exactly the specified number of items.
    /// </summary>
    /// <param name="count">The exact number of items required.</param>
    /// <returns>This validator for method chaining.</returns>
    public EnumerablePropertyValidator<T, TItem> Count(int count) =>
        AddRule(new CountRule<TItem>(count));

    /// <summary>
    /// Validates that the collection contains at most the specified number of items.
    /// </summary>
    /// <param name="max">The maximum number of items allowed.</param>
    /// <returns>This validator for method chaining.</returns>
    public EnumerablePropertyValidator<T, TItem> MaxCount(int max) =>
        AddRule(new MaxCountRule<TItem>(max));

    /// <summary>
    /// Validates that the collection contains at least the specified number of items.
    /// </summary>
    /// <param name="min">The minimum number of items required.</param>
    /// <returns>This validator for method chaining.</returns>
    public EnumerablePropertyValidator<T, TItem> MinCount(int min) =>
        AddRule(new MinCountRule<TItem>(min));

    /// <summary>
    /// Validates that all items in the collection are unique.
    /// </summary>
    /// <param name="comparer">Optional equality comparer for determining uniqueness.</param>
    /// <returns>This validator for method chaining.</returns>
    public EnumerablePropertyValidator<T, TItem> Unique(IEqualityComparer<TItem>? comparer = null) =>
        AddRule(new UniqueRule<TItem>(comparer));

    #endregion Public Methods
}