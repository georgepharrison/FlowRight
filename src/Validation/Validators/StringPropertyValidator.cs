using FlowRight.Validation.Builders;
using FlowRight.Validation.Rules;
using System.Text.RegularExpressions;

namespace FlowRight.Validation.Validators;

/// <summary>
/// Provides fluent validation rules specifically designed for string properties, offering comprehensive
/// string-specific validation capabilities including length checks, pattern matching, and email validation.
/// </summary>
/// <typeparam name="T">The type of object being validated.</typeparam>
/// <example>
/// <code>
/// ValidationBuilder&lt;User&gt; builder = new();
/// builder.RuleFor(x =&gt; x.Email, request.Email)
///     .NotEmpty()
///     .EmailAddress()
///     .MaximumLength(255)
///     .WithMessage("Please provide a valid email address");
///
/// builder.RuleFor(x =&gt; x.Password, request.Password)
///     .NotEmpty()
///     .MinimumLength(8)
///     .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$")
///     .WithMessage("Password must contain at least one lowercase, uppercase, and digit");
/// </code>
/// </example>
public sealed class StringPropertyValidator<T> : PropertyValidator<T, string, StringPropertyValidator<T>>
{
    #region Internal Constructors

    /// <summary>
    /// Initializes a new instance of the StringPropertyValidator class.
    /// </summary>
    /// <param name="builder">The parent validation builder.</param>
    /// <param name="displayName">The display name for the property in error messages.</param>
    /// <param name="value">The string value to validate.</param>
    internal StringPropertyValidator(ValidationBuilder<T> builder, string displayName, string value)
        : base(builder, displayName, value)
    {
    }

    #endregion Internal Constructors

    #region Public Methods

    /// <summary>
    /// Validates that the string is a properly formatted email address according to standard email format rules.
    /// </summary>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.Email, request.Email)
    ///     .EmailAddress()
    ///     .WithMessage("Please enter a valid email address");
    ///
    /// // Valid: "user@example.com", "test.email+tag@domain.co.uk"
    /// // Invalid: "invalid-email", "@domain.com", "user@"
    /// </code>
    /// </example>
    public StringPropertyValidator<T> EmailAddress() =>
        AddRule(new EmailRule());

    /// <summary>
    /// Validates that the string has exactly the specified length.
    /// </summary>
    /// <param name="length">The exact length the string must have.</param>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.CountryCode, request.CountryCode)
    ///     .ExactLength(2)
    ///     .WithMessage("Country code must be exactly 2 characters");
    ///
    /// // Valid: "US", "CA", "GB"
    /// // Invalid: "USA", "C", ""
    /// </code>
    /// </example>
    public StringPropertyValidator<T> ExactLength(int length) =>
        AddRule(new ExactLengthRule(length));

    /// <summary>
    /// Validates that the string length is between the specified minimum and maximum values (inclusive).
    /// </summary>
    /// <param name="minLength">The minimum allowed length (inclusive).</param>
    /// <param name="maxLength">The maximum allowed length (inclusive).</param>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.Username, request.Username)
    ///     .Length(3, 20)
    ///     .WithMessage("Username must be between 3 and 20 characters");
    ///
    /// // Valid: "abc", "username123", "abcdefghijklmnopqrst"
    /// // Invalid: "ab", "verylongusernamethatexceedslimit"
    /// </code>
    /// </example>
    public StringPropertyValidator<T> Length(int minLength, int maxLength) =>
        AddRule(new LengthRule(minLength, maxLength));

    /// <summary>
    /// Validates that the string matches the specified regular expression pattern.
    /// </summary>
    /// <param name="pattern">The regular expression pattern to match against.</param>
    /// <param name="options">Optional regex options to modify pattern matching behavior.</param>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// // Phone number validation
    /// builder.RuleFor(x =&gt; x.PhoneNumber, request.PhoneNumber)
    ///     .Matches(@"^\+?[1-9]\d{1,14}$")
    ///     .WithMessage("Please enter a valid phone number");
    ///
    /// // Case-insensitive pattern matching
    /// builder.RuleFor(x =&gt; x.ProductCode, request.ProductCode)
    ///     .Matches(@"^[A-Z]{2}\d{4}$", RegexOptions.IgnoreCase)
    ///     .WithMessage("Product code must be 2 letters followed by 4 digits");
    /// </code>
    /// </example>
    public StringPropertyValidator<T> Matches(string pattern, RegexOptions options = RegexOptions.None) =>
        AddRule(new MatchesRule(pattern, options));

    /// <summary>
    /// Validates that the string length does not exceed the specified maximum.
    /// </summary>
    /// <param name="max">The maximum allowed length (inclusive).</param>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.Description, request.Description)
    ///     .MaximumLength(500)
    ///     .WithMessage("Description cannot exceed 500 characters");
    ///
    /// // Database field length constraints
    /// builder.RuleFor(x =&gt; x.Title, request.Title)
    ///     .MaximumLength(100)
    ///     .WithMessage("Title is too long for database storage");
    /// </code>
    /// </example>
    public StringPropertyValidator<T> MaximumLength(int max) =>
        AddRule(new MaxLengthRule(max));

    /// <summary>
    /// Validates that the string length meets or exceeds the specified minimum.
    /// </summary>
    /// <param name="min">The minimum required length (inclusive).</param>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.Password, request.Password)
    ///     .MinimumLength(8)
    ///     .WithMessage("Password must be at least 8 characters long");
    ///
    /// // Security requirements
    /// builder.RuleFor(x =&gt; x.ApiKey, request.ApiKey)
    ///     .MinimumLength(32)
    ///     .WithMessage("API key is too short to be secure");
    /// </code>
    /// </example>
    public StringPropertyValidator<T> MinimumLength(int min) =>
        AddRule(new MinLengthRule(min));

    #endregion Public Methods
}