namespace FlowRight.Validation.Validators;

public sealed class GuidPropertyValidator<T> : PropertyValidator<T, Guid?, GuidPropertyValidator<T>>
{
    #region Internal Constructors

    internal GuidPropertyValidator(ValidationBuilder<T> builder, string displayName, Guid? value)
        : base(builder, displayName, value)
    {
    }

    #endregion Internal Constructors
}