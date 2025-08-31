using FlowRight.Validation.Rules;

namespace FlowRight.Validation.Validators;

public sealed class EnumerablePropertyValidator<T, TItem> : PropertyValidator<T, IEnumerable<TItem>, EnumerablePropertyValidator<T, TItem>>
{
    #region Internal Constructors

    internal EnumerablePropertyValidator(ValidationBuilder<T> builder, string displayName, IEnumerable<TItem> value)
        : base(builder, displayName, value)
    {
    }

    #endregion Internal Constructors

    #region Public Methods

    public EnumerablePropertyValidator<T, TItem> Count(int min) =>
        AddRule(new CountRule<TItem>(min));

    public EnumerablePropertyValidator<T, TItem> MaxCount(int max) =>
        AddRule(new MaxCountRule<TItem>(max));

    public EnumerablePropertyValidator<T, TItem> MinCount(int min) =>
        AddRule(new MinCountRule<TItem>(min));

    public EnumerablePropertyValidator<T, TItem> Unique(IEqualityComparer<TItem>? comparer = null) =>
        AddRule(new UniqueRule<TItem>(comparer));

    #endregion Public Methods
}