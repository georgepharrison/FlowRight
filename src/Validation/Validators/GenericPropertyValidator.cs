using FlowRight.Validation.Builders;

namespace FlowRight.Validation.Validators;

/// <summary>
/// Property validator for generic types that provides basic validation operations.
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
/// <typeparam name="TProp">The property type being validated.</typeparam>
public sealed class GenericPropertyValidator<T, TProp> : PropertyValidator<T, TProp, GenericPropertyValidator<T, TProp>>
{
    #region Internal Constructors

    internal GenericPropertyValidator(ValidationBuilder<T> builder, string displayName, TProp value)
        : base(builder, displayName, value)
    {
    }

    #endregion Internal Constructors
}