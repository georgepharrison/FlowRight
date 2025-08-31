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

    /// <summary>
    /// Validates that the string is a properly formatted URL according to standard URL format rules.
    /// </summary>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.Website, request.Website)
    ///     .Url()
    ///     .WithMessage("Please enter a valid URL");
    ///
    /// // Valid: "https://example.com", "http://www.test.org/path?query=1"
    /// // Invalid: "not-a-url", "ftp://example.com", "example.com"
    /// </code>
    /// </example>
    public StringPropertyValidator<T> Url() =>
        AddRule(new UrlRule());

    /// <summary>
    /// Validates that the string contains the specified substring.
    /// </summary>
    /// <param name="substring">The substring that must be contained in the value.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.Description, request.Description)
    ///     .Contains("important")
    ///     .WithMessage("Description must contain the word 'important'");
    ///
    /// // Case-insensitive search
    /// builder.RuleFor(x =&gt; x.Title, request.Title)
    ///     .Contains("urgent", StringComparison.OrdinalIgnoreCase)
    ///     .WithMessage("Title must contain 'urgent' (case insensitive)");
    /// </code>
    /// </example>
    public StringPropertyValidator<T> Contains(string substring, StringComparison comparison = StringComparison.Ordinal) =>
        AddRule(new ContainsRule(substring, comparison));

    /// <summary>
    /// Validates that the string starts with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix that the value must start with.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.ProductCode, request.ProductCode)
    ///     .StartsWith("PRD-")
    ///     .WithMessage("Product code must start with 'PRD-'");
    ///
    /// // Case-insensitive prefix check
    /// builder.RuleFor(x =&gt; x.Command, request.Command)
    ///     .StartsWith("exec", StringComparison.OrdinalIgnoreCase)
    ///     .WithMessage("Command must start with 'exec'");
    /// </code>
    /// </example>
    public StringPropertyValidator<T> StartsWith(string prefix, StringComparison comparison = StringComparison.Ordinal) =>
        AddRule(new StartsWithRule(prefix, comparison));

    /// <summary>
    /// Validates that the string ends with the specified suffix.
    /// </summary>
    /// <param name="suffix">The suffix that the value must end with.</param>
    /// <param name="comparison">The string comparison type to use.</param>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.FileName, request.FileName)
    ///     .EndsWith(".txt")
    ///     .WithMessage("File must be a text file (.txt)");
    ///
    /// // Case-insensitive suffix check
    /// builder.RuleFor(x =&gt; x.ImagePath, request.ImagePath)
    ///     .EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
    ///     .WithMessage("Image must be a JPEG file");
    /// </code>
    /// </example>
    public StringPropertyValidator<T> EndsWith(string suffix, StringComparison comparison = StringComparison.Ordinal) =>
        AddRule(new EndsWithRule(suffix, comparison));

    /// <summary>
    /// Validates that the string contains only alphabetic characters (letters).
    /// </summary>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.FirstName, request.FirstName)
    ///     .Alpha()
    ///     .WithMessage("First name must contain only letters");
    ///
    /// // Valid: "John", "María", "José"
    /// // Invalid: "John123", "John-Doe", "John Smith"
    /// </code>
    /// </example>
    public StringPropertyValidator<T> Alpha() =>
        AddRule(new AlphaRule());

    /// <summary>
    /// Validates that the string contains only alphanumeric characters (letters and digits).
    /// </summary>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.Username, request.Username)
    ///     .AlphaNumeric()
    ///     .WithMessage("Username must contain only letters and numbers");
    ///
    /// // Valid: "User123", "JohnDoe", "admin"
    /// // Invalid: "user-123", "john_doe", "admin@"
    /// </code>
    /// </example>
    public StringPropertyValidator<T> AlphaNumeric() =>
        AddRule(new AlphaNumericRule());

    /// <summary>
    /// Validates that the string is in uppercase.
    /// </summary>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.CountryCode, request.CountryCode)
    ///     .UpperCase()
    ///     .WithMessage("Country code must be in uppercase");
    ///
    /// // Valid: "US", "CANADA", "UNITED KINGDOM"
    /// // Invalid: "us", "canada", "United Kingdom"
    /// </code>
    /// </example>
    public StringPropertyValidator<T> UpperCase() =>
        AddRule(new UpperCaseRule());

    /// <summary>
    /// Validates that the string is in lowercase.
    /// </summary>
    /// <returns>The StringPropertyValidator&lt;T&gt; for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.RuleFor(x =&gt; x.Username, request.Username)
    ///     .LowerCase()
    ///     .WithMessage("Username must be in lowercase");
    ///
    /// // Valid: "user", "admin", "john.doe"
    /// // Invalid: "User", "ADMIN", "John.Doe"
    /// </code>
    /// </example>
    public StringPropertyValidator<T> LowerCase() =>
        AddRule(new LowerCaseRule());

    #endregion Public Methods
}