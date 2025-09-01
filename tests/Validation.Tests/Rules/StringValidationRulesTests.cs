using FlowRight.Validation.Rules;
using Shouldly;

namespace FlowRight.Validation.Tests.Rules;

/// <summary>
/// Comprehensive failing tests for string validation rules that define expected behavior
/// for string validation patterns. These tests follow TDD principles and will initially 
/// fail until the validation rule implementations are complete.
/// 
/// Test Coverage:
/// - AlphaRule - alphabetic characters only
/// - AlphaNumericRule - alphanumeric characters only  
/// - AlphaNumericSpaceRule - alphanumeric + spaces
/// - AlphaNumericDashRule - alphanumeric + dashes
/// - CreditCardRule - credit card number validation
/// - PhoneRule - phone number validation
/// - PostalCodeRule - postal/ZIP code validation
/// - StartsWithRule - string prefix validation
/// - EndsWithRule - string suffix validation
/// - ContainsRule - substring validation
/// - EqualToRule - string equality validation
/// 
/// Current Status: FAILING as expected (TDD Red phase)
/// These failing tests serve as executable specifications for string validation rules.
/// </summary>
public class StringValidationRulesTests
{
    #region AlphaRule Tests

    /// <summary>
    /// Tests for AlphaRule - validates strings contain only alphabetic characters
    /// </summary>
    public class AlphaRuleTests
    {
        [Fact]
        public void Validate_WithAlphabeticString_ShouldReturnNull()
        {
            // Arrange
            AlphaRule rule = new();
            string validValue = "HelloWorld";

            // Act
            string? result = rule.Validate(validValue, "Name");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNumericCharacters_ShouldReturnErrorMessage()
        {
            // Arrange
            AlphaRule rule = new();
            string invalidValue = "Hello123";

            // Act
            string? result = rule.Validate(invalidValue, "Name");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain only alphabetic characters");
        }

        [Fact]
        public void Validate_WithSpecialCharacters_ShouldReturnErrorMessage()
        {
            // Arrange
            AlphaRule rule = new();
            string invalidValue = "Hello-World";

            // Act
            string? result = rule.Validate(invalidValue, "Name");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain only alphabetic characters");
        }

        [Fact]
        public void Validate_WithSpaces_ShouldReturnErrorMessage()
        {
            // Arrange
            AlphaRule rule = new();
            string invalidValue = "Hello World";

            // Act
            string? result = rule.Validate(invalidValue, "Name");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain only alphabetic characters");
        }

        [Fact]
        public void Validate_WithEmptyString_ShouldReturnNull()
        {
            // Arrange
            AlphaRule rule = new();
            string emptyValue = string.Empty;

            // Act
            string? result = rule.Validate(emptyValue, "Name");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullString_ShouldReturnNull()
        {
            // Arrange
            AlphaRule rule = new();

            // Act
            string? result = rule.Validate(null!, "Name");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion AlphaRule Tests

    #region AlphaNumericRule Tests

    /// <summary>
    /// Tests for AlphaNumericRule - validates strings contain only alphanumeric characters
    /// </summary>
    public class AlphaNumericRuleTests
    {
        [Fact]
        public void Validate_WithAlphaNumericString_ShouldReturnNull()
        {
            // Arrange
            AlphaNumericRule rule = new();
            string validValue = "Hello123World";

            // Act
            string? result = rule.Validate(validValue, "Name");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithSpecialCharacters_ShouldReturnErrorMessage()
        {
            // Arrange
            AlphaNumericRule rule = new();
            string invalidValue = "Hello-123";

            // Act
            string? result = rule.Validate(invalidValue, "Name");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain only alphanumeric characters");
        }

        [Fact]
        public void Validate_WithSpaces_ShouldReturnErrorMessage()
        {
            // Arrange
            AlphaNumericRule rule = new();
            string invalidValue = "Hello 123";

            // Act
            string? result = rule.Validate(invalidValue, "Name");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain only alphanumeric characters");
        }

        [Fact]
        public void Validate_WithOnlyLetters_ShouldReturnNull()
        {
            // Arrange
            AlphaNumericRule rule = new();
            string validValue = "HelloWorld";

            // Act
            string? result = rule.Validate(validValue, "Name");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithOnlyNumbers_ShouldReturnNull()
        {
            // Arrange
            AlphaNumericRule rule = new();
            string validValue = "123456";

            // Act
            string? result = rule.Validate(validValue, "Name");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion AlphaNumericRule Tests

    #region AlphaNumericSpaceRule Tests

    /// <summary>
    /// Tests for AlphaNumericSpaceRule - validates strings contain only alphanumeric characters and spaces
    /// </summary>
    public class AlphaNumericSpaceRuleTests
    {
        [Fact]
        public void Validate_WithAlphaNumericAndSpaces_ShouldReturnNull()
        {
            // Arrange
            AlphaNumericSpaceRule rule = new();
            string validValue = "Hello 123 World";

            // Act
            string? result = rule.Validate(validValue, "Full Name");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithSpecialCharacters_ShouldReturnErrorMessage()
        {
            // Arrange
            AlphaNumericSpaceRule rule = new();
            string invalidValue = "Hello-World";

            // Act
            string? result = rule.Validate(invalidValue, "Full Name");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain only alphanumeric characters and spaces");
        }

        [Fact]
        public void Validate_WithPunctuation_ShouldReturnErrorMessage()
        {
            // Arrange
            AlphaNumericSpaceRule rule = new();
            string invalidValue = "Hello, World!";

            // Act
            string? result = rule.Validate(invalidValue, "Full Name");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain only alphanumeric characters and spaces");
        }

        [Fact]
        public void Validate_WithMultipleSpaces_ShouldReturnNull()
        {
            // Arrange
            AlphaNumericSpaceRule rule = new();
            string validValue = "Hello   World   123";

            // Act
            string? result = rule.Validate(validValue, "Full Name");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithLeadingAndTrailingSpaces_ShouldReturnNull()
        {
            // Arrange
            AlphaNumericSpaceRule rule = new();
            string validValue = "  Hello World 123  ";

            // Act
            string? result = rule.Validate(validValue, "Full Name");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion AlphaNumericSpaceRule Tests

    #region AlphaNumericDashRule Tests

    /// <summary>
    /// Tests for AlphaNumericDashRule - validates strings contain only alphanumeric characters and dashes
    /// </summary>
    public class AlphaNumericDashRuleTests
    {
        [Fact]
        public void Validate_WithAlphaNumericAndDashes_ShouldReturnNull()
        {
            // Arrange
            AlphaNumericDashRule rule = new();
            string validValue = "Hello-123-World";

            // Act
            string? result = rule.Validate(validValue, "Identifier");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithSpaces_ShouldReturnErrorMessage()
        {
            // Arrange
            AlphaNumericDashRule rule = new();
            string invalidValue = "Hello 123";

            // Act
            string? result = rule.Validate(invalidValue, "Identifier");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain only alphanumeric characters and dashes");
        }

        [Fact]
        public void Validate_WithUnderscores_ShouldReturnErrorMessage()
        {
            // Arrange
            AlphaNumericDashRule rule = new();
            string invalidValue = "Hello_World";

            // Act
            string? result = rule.Validate(invalidValue, "Identifier");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain only alphanumeric characters and dashes");
        }

        [Fact]
        public void Validate_WithMultipleDashes_ShouldReturnNull()
        {
            // Arrange
            AlphaNumericDashRule rule = new();
            string validValue = "Hello--123--World";

            // Act
            string? result = rule.Validate(validValue, "Identifier");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithLeadingAndTrailingDashes_ShouldReturnNull()
        {
            // Arrange
            AlphaNumericDashRule rule = new();
            string validValue = "-Hello123World-";

            // Act
            string? result = rule.Validate(validValue, "Identifier");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion AlphaNumericDashRule Tests

    #region CreditCardRule Tests

    /// <summary>
    /// Tests for CreditCardRule - validates credit card numbers using Luhn algorithm
    /// </summary>
    public class CreditCardRuleTests
    {
        [Fact]
        public void Validate_WithValidVisaNumber_ShouldReturnNull()
        {
            // Arrange
            CreditCardRule rule = new();
            string validVisa = "4532015112830366"; // Valid test Visa number

            // Act
            string? result = rule.Validate(validVisa, "Credit Card Number");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithValidMasterCardNumber_ShouldReturnNull()
        {
            // Arrange
            CreditCardRule rule = new();
            string validMasterCard = "5555555555554444"; // Valid test MasterCard number

            // Act
            string? result = rule.Validate(validMasterCard, "Credit Card Number");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithInvalidLuhnChecksum_ShouldReturnErrorMessage()
        {
            // Arrange
            CreditCardRule rule = new();
            string invalidNumber = "4532015112830367"; // Invalid checksum

            // Act
            string? result = rule.Validate(invalidNumber, "Credit Card Number");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid credit card number");
        }

        [Fact]
        public void Validate_WithNonNumericCharacters_ShouldReturnErrorMessage()
        {
            // Arrange
            CreditCardRule rule = new();
            string invalidNumber = "4532-0151-1283-0366";

            // Act
            string? result = rule.Validate(invalidNumber, "Credit Card Number");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid credit card number");
        }

        [Fact]
        public void Validate_WithTooShortNumber_ShouldReturnErrorMessage()
        {
            // Arrange
            CreditCardRule rule = new();
            string shortNumber = "453201511";

            // Act
            string? result = rule.Validate(shortNumber, "Credit Card Number");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid credit card number");
        }

        [Fact]
        public void Validate_WithTooLongNumber_ShouldReturnErrorMessage()
        {
            // Arrange
            CreditCardRule rule = new();
            string longNumber = "45320151128303661234567890";

            // Act
            string? result = rule.Validate(longNumber, "Credit Card Number");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid credit card number");
        }
    }

    #endregion CreditCardRule Tests

    #region PhoneRule Tests

    /// <summary>
    /// Tests for PhoneRule - validates phone numbers in various formats
    /// </summary>
    public class PhoneRuleTests
    {
        [Fact]
        public void Validate_WithValidUSPhoneNumber_ShouldReturnNull()
        {
            // Arrange
            PhoneRule rule = new();
            string validPhone = "(555) 123-4567";

            // Act
            string? result = rule.Validate(validPhone, "Phone Number");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithValidInternationalPhoneNumber_ShouldReturnNull()
        {
            // Arrange
            PhoneRule rule = new();
            string validPhone = "+1-555-123-4567";

            // Act
            string? result = rule.Validate(validPhone, "Phone Number");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithDigitsOnly_ShouldReturnNull()
        {
            // Arrange
            PhoneRule rule = new();
            string validPhone = "5551234567";

            // Act
            string? result = rule.Validate(validPhone, "Phone Number");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithInvalidFormat_ShouldReturnErrorMessage()
        {
            // Arrange
            PhoneRule rule = new();
            string invalidPhone = "555-123-456";

            // Act
            string? result = rule.Validate(invalidPhone, "Phone Number");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid phone number");
        }

        [Fact]
        public void Validate_WithLetters_ShouldReturnErrorMessage()
        {
            // Arrange
            PhoneRule rule = new();
            string invalidPhone = "555-CALL-NOW";

            // Act
            string? result = rule.Validate(invalidPhone, "Phone Number");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid phone number");
        }

        [Fact]
        public void Validate_WithTooShortNumber_ShouldReturnErrorMessage()
        {
            // Arrange
            PhoneRule rule = new();
            string shortPhone = "123456";

            // Act
            string? result = rule.Validate(shortPhone, "Phone Number");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid phone number");
        }
    }

    #endregion PhoneRule Tests

    #region PostalCodeRule Tests

    /// <summary>
    /// Tests for PostalCodeRule - validates postal/ZIP codes for different countries
    /// </summary>
    public class PostalCodeRuleTests
    {
        [Fact]
        public void Validate_WithValidUSZipCode_ShouldReturnNull()
        {
            // Arrange
            PostalCodeRule rule = new();
            string validZip = "12345";

            // Act
            string? result = rule.Validate(validZip, "ZIP Code");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithValidUSZipPlusFour_ShouldReturnNull()
        {
            // Arrange
            PostalCodeRule rule = new();
            string validZip = "12345-6789";

            // Act
            string? result = rule.Validate(validZip, "ZIP Code");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithValidCanadianPostalCode_ShouldReturnNull()
        {
            // Arrange
            PostalCodeRule rule = new();
            string validPostal = "K1A 0A6";

            // Act
            string? result = rule.Validate(validPostal, "Postal Code");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithValidUKPostalCode_ShouldReturnNull()
        {
            // Arrange
            PostalCodeRule rule = new();
            string validPostal = "SW1A 1AA";

            // Act
            string? result = rule.Validate(validPostal, "Postal Code");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithInvalidFormat_ShouldReturnErrorMessage()
        {
            // Arrange
            PostalCodeRule rule = new();
            string invalidPostal = "123";

            // Act
            string? result = rule.Validate(invalidPostal, "Postal Code");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid postal code");
        }

        [Fact]
        public void Validate_WithSpecialCharacters_ShouldReturnErrorMessage()
        {
            // Arrange
            PostalCodeRule rule = new();
            string invalidPostal = "12345@#$";

            // Act
            string? result = rule.Validate(invalidPostal, "Postal Code");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("is not a valid postal code");
        }
    }

    #endregion PostalCodeRule Tests

    #region StartsWithRule Tests

    /// <summary>
    /// Tests for StartsWithRule - validates strings start with specific prefix
    /// </summary>
    public class StartsWithRuleTests
    {
        [Fact]
        public void Validate_WithMatchingPrefix_ShouldReturnNull()
        {
            // Arrange
            StartsWithRule rule = new("Hello");
            string validValue = "Hello World";

            // Act
            string? result = rule.Validate(validValue, "Message");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNonMatchingPrefix_ShouldReturnErrorMessage()
        {
            // Arrange
            StartsWithRule rule = new("Hello");
            string invalidValue = "Goodbye World";

            // Act
            string? result = rule.Validate(invalidValue, "Message");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must start with 'Hello'");
        }

        [Fact]
        public void Validate_WithExactMatch_ShouldReturnNull()
        {
            // Arrange
            StartsWithRule rule = new("Hello");
            string validValue = "Hello";

            // Act
            string? result = rule.Validate(validValue, "Message");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithCaseSensitivePrefix_ShouldReturnErrorMessage()
        {
            // Arrange
            StartsWithRule rule = new("Hello");
            string invalidValue = "hello world";

            // Act
            string? result = rule.Validate(invalidValue, "Message");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must start with 'Hello'");
        }

        [Fact]
        public void Validate_WithEmptyString_ShouldReturnErrorMessage()
        {
            // Arrange
            StartsWithRule rule = new("Hello");
            string emptyValue = string.Empty;

            // Act
            string? result = rule.Validate(emptyValue, "Message");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must start with 'Hello'");
        }
    }

    #endregion StartsWithRule Tests

    #region EndsWithRule Tests

    /// <summary>
    /// Tests for EndsWithRule - validates strings end with specific suffix
    /// </summary>
    public class EndsWithRuleTests
    {
        [Fact]
        public void Validate_WithMatchingSuffix_ShouldReturnNull()
        {
            // Arrange
            EndsWithRule rule = new("World");
            string validValue = "Hello World";

            // Act
            string? result = rule.Validate(validValue, "Message");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNonMatchingSuffix_ShouldReturnErrorMessage()
        {
            // Arrange
            EndsWithRule rule = new("World");
            string invalidValue = "Hello Everyone";

            // Act
            string? result = rule.Validate(invalidValue, "Message");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must end with 'World'");
        }

        [Fact]
        public void Validate_WithExactMatch_ShouldReturnNull()
        {
            // Arrange
            EndsWithRule rule = new("World");
            string validValue = "World";

            // Act
            string? result = rule.Validate(validValue, "Message");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithCaseSensitiveSuffix_ShouldReturnErrorMessage()
        {
            // Arrange
            EndsWithRule rule = new("World");
            string invalidValue = "Hello world";

            // Act
            string? result = rule.Validate(invalidValue, "Message");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must end with 'World'");
        }

        [Fact]
        public void Validate_WithEmptyString_ShouldReturnErrorMessage()
        {
            // Arrange
            EndsWithRule rule = new("World");
            string emptyValue = string.Empty;

            // Act
            string? result = rule.Validate(emptyValue, "Message");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must end with 'World'");
        }
    }

    #endregion EndsWithRule Tests

    #region ContainsRule Tests

    /// <summary>
    /// Tests for ContainsRule - validates strings contain specific substring
    /// </summary>
    public class ContainsRuleTests
    {
        [Fact]
        public void Validate_WithContainedSubstring_ShouldReturnNull()
        {
            // Arrange
            ContainsRule rule = new("Test");
            string validValue = "This is a Test message";

            // Act
            string? result = rule.Validate(validValue, "Message");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithoutContainedSubstring_ShouldReturnErrorMessage()
        {
            // Arrange
            ContainsRule rule = new("Test");
            string invalidValue = "This is a message";

            // Act
            string? result = rule.Validate(invalidValue, "Message");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain 'Test'");
        }

        [Fact]
        public void Validate_WithExactMatch_ShouldReturnNull()
        {
            // Arrange
            ContainsRule rule = new("Test");
            string validValue = "Test";

            // Act
            string? result = rule.Validate(validValue, "Message");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithCaseSensitiveSubstring_ShouldReturnErrorMessage()
        {
            // Arrange
            ContainsRule rule = new("Test");
            string invalidValue = "This is a test message";

            // Act
            string? result = rule.Validate(invalidValue, "Message");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain 'Test'");
        }

        [Fact]
        public void Validate_WithMultipleOccurrences_ShouldReturnNull()
        {
            // Arrange
            ContainsRule rule = new("Test");
            string validValue = "Test this Test message";

            // Act
            string? result = rule.Validate(validValue, "Message");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion ContainsRule Tests

    #region EqualToRule Tests

    /// <summary>
    /// Tests for EqualToRule - validates strings are equal to specific value
    /// </summary>
    public class EqualToRuleTests
    {
        [Fact]
        public void Validate_WithEqualValue_ShouldReturnNull()
        {
            // Arrange
            EqualToRule rule = new("ExpectedValue");
            string validValue = "ExpectedValue";

            // Act
            string? result = rule.Validate(validValue, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithDifferentValue_ShouldReturnErrorMessage()
        {
            // Arrange
            EqualToRule rule = new("ExpectedValue");
            string invalidValue = "DifferentValue";

            // Act
            string? result = rule.Validate(invalidValue, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be equal to 'ExpectedValue'");
        }

        [Fact]
        public void Validate_WithCaseSensitiveComparison_ShouldReturnErrorMessage()
        {
            // Arrange
            EqualToRule rule = new("ExpectedValue");
            string invalidValue = "expectedvalue";

            // Act
            string? result = rule.Validate(invalidValue, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be equal to 'ExpectedValue'");
        }

        [Fact]
        public void Validate_WithNullValue_ShouldReturnErrorMessage()
        {
            // Arrange
            EqualToRule rule = new("ExpectedValue");

            // Act
            string? result = rule.Validate(null!, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be equal to 'ExpectedValue'");
        }

        [Fact]
        public void Validate_WithEmptyStringExpected_ShouldReturnNull()
        {
            // Arrange
            EqualToRule rule = new(string.Empty);
            string validValue = string.Empty;

            // Act
            string? result = rule.Validate(validValue, "Field");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion EqualToRule Tests
}