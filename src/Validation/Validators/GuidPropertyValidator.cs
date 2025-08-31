using FlowRight.Validation.Builders;

namespace FlowRight.Validation.Validators;

/// <summary>
/// Provides validation rules for GUID properties with fluent configuration.
/// </summary>
/// <typeparam name="T">The type containing the property being validated.</typeparam>
public sealed class GuidPropertyValidator<T> : PropertyValidator<T, Guid?, GuidPropertyValidator<T>>
{
    #region Internal Constructors

    internal GuidPropertyValidator(ValidationBuilder<T> builder, string displayName, Guid? value)
        : base(builder, displayName, value)
    {
    }

    #endregion Internal Constructors
}