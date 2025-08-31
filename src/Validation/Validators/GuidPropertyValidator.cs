using FlowRight.Validation.Builders;
using FlowRight.Validation.Rules;

namespace FlowRight.Validation.Validators;

/// <summary>
/// Provides fluent validation rules specifically designed for GUID properties, offering comprehensive
/// GUID-specific validation capabilities including empty GUID checks, version validation, and format validation.
/// </summary>
/// <typeparam name="T">The type of object being validated.</typeparam>
/// <example>
/// <code>
/// ValidationBuilder&lt;Entity&gt; builder = new();
/// builder.RuleFor(x =&gt; x.Id, request.Id)
///     .NotEmpty()
///     .NotEqual(Guid.Empty)
///     .Version4()
///     .WithMessage("Please provide a valid entity ID");
///
/// builder.RuleFor(x =&gt; x.ParentId, request.ParentId)
///     .NotNull()
///     .ValidFormat()
///     .WithMessage("Parent ID must be a valid GUID");
/// </code>
/// </example>
public sealed class GuidPropertyValidator<T> : PropertyValidator<T, Guid?, GuidPropertyValidator<T>>
{
    #region Internal Constructors

    /// <summary>
    /// Initializes a new instance of the GuidPropertyValidator class.
    /// </summary>
    /// <param name="builder">The parent validation builder.</param>
    /// <param name="displayName">The display name for the property in error messages.</param>
    /// <param name="value">The GUID value to validate.</param>
    internal GuidPropertyValidator(ValidationBuilder<T> builder, string displayName, Guid? value)
        : base(builder, displayName, value)
    {
    }

    #endregion Internal Constructors

    #region Public Methods


    /// <summary>
    /// Validates that the GUID follows the standard hyphenated format (8-4-4-4-12 characters).
    /// This ensures the GUID string representation is properly formatted.
    /// </summary>
    /// <returns>The GuidPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.Id, request.Id)
    ///     .ValidFormat()
    ///     .WithMessage("ID must be in valid GUID format");
    ///
    /// // Valid: "f47ac10b-58cc-4372-a567-0e02b2c3d479"
    /// // Invalid: Malformed or non-hyphenated formats
    /// </code>
    /// </example>
    public GuidPropertyValidator<T> ValidFormat() =>
        AddRule(new GuidValidFormatRule());


    /// <summary>
    /// Validates that the GUID is not the sequential empty GUID (00000000-0000-0000-0000-000000000000).
    /// This is equivalent to NotEqual(Guid.Empty) but provides a more semantic method name.
    /// </summary>
    /// <returns>The GuidPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.EntityId, request.EntityId)
    ///     .NotEmptyGuid()
    ///     .WithMessage("Entity ID cannot be empty");
    ///
    /// // Valid: Any non-empty GUID
    /// // Invalid: Guid.Empty (00000000-0000-0000-0000-000000000000)
    /// </code>
    /// </example>
    public GuidPropertyValidator<T> NotEmptyGuid() =>
        AddRule(new GuidNotEmptyRule());

    /// <summary>
    /// Validates that the GUID has non-zero timestamp bits, indicating it was not created at epoch time.
    /// This can help identify potentially invalid or placeholder GUIDs.
    /// </summary>
    /// <returns>The GuidPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.CreatedId, request.CreatedId)
    ///     .NonZeroTimestamp()
    ///     .WithMessage("GUID must have a valid timestamp component");
    /// </code>
    /// </example>
    public GuidPropertyValidator<T> NonZeroTimestamp() =>
        AddRule(new GuidNonZeroTimestampRule());

    #endregion Public Methods
}