using FlowRight.Validation.Rules;
using Shouldly;

namespace FlowRight.Validation.Tests.Rules;

/// <summary>
/// Comprehensive failing tests for ExactLengthRule that define expected behavior
/// for exact string length validation. These tests follow TDD principles and will initially 
/// fail until the ExactLengthRule implementation is complete.
/// 
/// Test Coverage:
/// - Valid strings with exact length
/// - Invalid strings too short
/// - Invalid strings too long
/// - Edge cases (null values, empty strings, zero length)
/// - Boundary conditions
/// - Error message validation
/// 
/// Current Status: FAILING as expected (TDD Red phase)
/// These failing tests serve as executable specifications for ExactLengthRule.
/// </summary>
public class ExactLengthRuleTests
{
    #region Constructor Tests

    /// <summary>
    /// Tests for ExactLengthRule constructor behavior
    /// </summary>
    public class Constructor
    {
        [Fact]
        public void Constructor_WithPositiveLength_ShouldCreateRule()
        {
            // Arrange & Act
            ExactLengthRule rule = new(10);

            // Assert
            rule.ShouldNotBeNull();
        }

        [Fact]
        public void Constructor_WithZeroLength_ShouldCreateRule()
        {
            // Arrange & Act
            ExactLengthRule rule = new(0);

            // Assert
            rule.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(-100)]
        public void Constructor_WithNegativeLength_ShouldCreateRule(int negativeLength)
        {
            // Arrange & Act
            ExactLengthRule rule = new(negativeLength);

            // Assert
            rule.ShouldNotBeNull();
        }
    }

    #endregion Constructor Tests

    #region Valid Length Tests

    /// <summary>
    /// Tests for strings with the exact required length
    /// </summary>
    public class ValidLengthTests
    {
        [Fact]
        public void Validate_WithExactLength_ShouldReturnNull()
        {
            // Arrange
            ExactLengthRule rule = new(5);
            string validValue = "Hello";

            // Act
            string? result = rule.Validate(validValue, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Theory]
        [InlineData(1, "A")]
        [InlineData(3, "ABC")]
        [InlineData(10, "1234567890")]
        [InlineData(20, "12345678901234567890")]
        public void Validate_WithVariousExactLengths_ShouldReturnNull(int length, string validValue)
        {
            // Arrange
            ExactLengthRule rule = new(length);

            // Act
            string? result = rule.Validate(validValue, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithZeroLengthAndEmptyString_ShouldReturnNull()
        {
            // Arrange
            ExactLengthRule rule = new(0);
            string emptyValue = string.Empty;

            // Act
            string? result = rule.Validate(emptyValue, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithSpecialCharacters_ShouldReturnNull()
        {
            // Arrange
            ExactLengthRule rule = new(10);
            string specialChars = "Hello@#$%^";

            // Act
            string? result = rule.Validate(specialChars, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithUnicodeCharacters_ShouldReturnErrorMessage()
        {
            // Arrange
            ExactLengthRule rule = new(6);
            string unicodeValue = "Héllo🙂"; // This string has a different length than expected

            // Act
            string? result = rule.Validate(unicodeValue, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Field must be exactly 6 characters long");
        }

        [Fact]
        public void Validate_WithWhitespaceOnly_ShouldReturnNull()
        {
            // Arrange
            ExactLengthRule rule = new(5);
            string whitespaceValue = "     ";

            // Act
            string? result = rule.Validate(whitespaceValue, "Field");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion Valid Length Tests

    #region Invalid Length Tests

    /// <summary>
    /// Tests for strings that don't have the exact required length
    /// </summary>
    public class InvalidLengthTests
    {
        [Fact]
        public void Validate_WithTooShortString_ShouldReturnErrorMessage()
        {
            // Arrange
            ExactLengthRule rule = new(10);
            string shortValue = "Hello";

            // Act
            string? result = rule.Validate(shortValue, "Username");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Username must be exactly 10 characters long");
        }

        [Fact]
        public void Validate_WithTooLongString_ShouldReturnErrorMessage()
        {
            // Arrange
            ExactLengthRule rule = new(5);
            string longValue = "HelloWorld";

            // Act
            string? result = rule.Validate(longValue, "Code");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Code must be exactly 5 characters long");
        }

        [Theory]
        [InlineData(5, "Hi", "Field")]
        [InlineData(5, "Hello World", "Name")]
        [InlineData(10, "", "Password")]
        [InlineData(3, "AB", "Code")]
        [InlineData(3, "ABCD", "Code")]
        public void Validate_WithWrongLength_ShouldReturnCorrectErrorMessage(int expectedLength, string value, string displayName)
        {
            // Arrange
            ExactLengthRule rule = new(expectedLength);

            // Act
            string? result = rule.Validate(value, displayName);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe($"{displayName} must be exactly {expectedLength} characters long");
        }

        [Fact]
        public void Validate_WithEmptyStringWhenLengthRequired_ShouldReturnErrorMessage()
        {
            // Arrange
            ExactLengthRule rule = new(5);
            string emptyValue = string.Empty;

            // Act
            string? result = rule.Validate(emptyValue, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Field must be exactly 5 characters long");
        }
    }

    #endregion Invalid Length Tests

    #region Null Value Tests

    /// <summary>
    /// Tests for null value handling
    /// </summary>
    public class NullValueTests
    {
        [Fact]
        public void Validate_WithNullValue_ShouldReturnNull()
        {
            // Arrange
            ExactLengthRule rule = new(10);

            // Act
            string? result = rule.Validate(null!, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        public void Validate_WithNullValueForAnyLength_ShouldReturnNull(int length)
        {
            // Arrange
            ExactLengthRule rule = new(length);

            // Act
            string? result = rule.Validate(null!, "Field");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion Null Value Tests

    #region Boundary Tests

    /// <summary>
    /// Tests for boundary conditions and edge cases
    /// </summary>
    public class BoundaryTests
    {
        [Fact]
        public void Validate_WithOneCharacterTooShort_ShouldReturnErrorMessage()
        {
            // Arrange
            ExactLengthRule rule = new(10);
            string value = "123456789"; // 9 characters

            // Act
            string? result = rule.Validate(value, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Field must be exactly 10 characters long");
        }

        [Fact]
        public void Validate_WithOneCharacterTooLong_ShouldReturnErrorMessage()
        {
            // Arrange
            ExactLengthRule rule = new(10);
            string value = "12345678901"; // 11 characters

            // Act
            string? result = rule.Validate(value, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Field must be exactly 10 characters long");
        }

        [Fact]
        public void Validate_WithMaxIntLength_ShouldWorkCorrectly()
        {
            // Arrange
            ExactLengthRule rule = new(1000);
            string exactLength = new('A', 1000);
            string tooLong = new('A', 1001);

            // Act
            string? validResult = rule.Validate(exactLength, "Field");
            string? invalidResult = rule.Validate(tooLong, "Field");

            // Assert
            validResult.ShouldBeNull();
            invalidResult.ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithVeryLargeLength_ShouldHandleGracefully()
        {
            // Arrange
            ExactLengthRule rule = new(int.MaxValue);
            string shortValue = "test";

            // Act
            string? result = rule.Validate(shortValue, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe($"Field must be exactly {int.MaxValue} characters long");
        }
    }

    #endregion Boundary Tests

    #region Error Message Tests

    /// <summary>
    /// Tests for error message formatting and content
    /// </summary>
    public class ErrorMessageTests
    {
        [Fact]
        public void Validate_WithInvalidLength_ShouldIncludeDisplayName()
        {
            // Arrange
            ExactLengthRule rule = new(8);
            string value = "short";
            string displayName = "Product Code";

            // Act
            string? result = rule.Validate(value, displayName);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldStartWith("Product Code");
        }

        [Fact]
        public void Validate_WithInvalidLength_ShouldIncludeExpectedLength()
        {
            // Arrange
            int expectedLength = 15;
            ExactLengthRule rule = new(expectedLength);
            string value = "too short";

            // Act
            string? result = rule.Validate(value, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain(expectedLength.ToString());
        }

        [Fact]
        public void Validate_WithInvalidLength_ShouldContainExactWordExactly()
        {
            // Arrange
            ExactLengthRule rule = new(10);
            string value = "short";

            // Act
            string? result = rule.Validate(value, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("exactly");
        }

        [Theory]
        [InlineData("Username", 8)]
        [InlineData("Password", 12)]
        [InlineData("Email Address", 50)]
        [InlineData("Product ID", 6)]
        public void Validate_WithVariousDisplayNames_ShouldFormatCorrectly(string displayName, int length)
        {
            // Arrange
            ExactLengthRule rule = new(length);
            string invalidValue = "x";

            // Act
            string? result = rule.Validate(invalidValue, displayName);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe($"{displayName} must be exactly {length} characters long");
        }
    }

    #endregion Error Message Tests

    #region Multiple Validation Tests

    /// <summary>
    /// Tests for using the same rule instance multiple times
    /// </summary>
    public class MultipleValidationTests
    {
        [Fact]
        public void Validate_SameRuleMultipleTimes_ShouldProduceConsistentResults()
        {
            // Arrange
            ExactLengthRule rule = new(7);
            string validValue = "exactly";
            string invalidValue = "wrong";

            // Act
            string? result1 = rule.Validate(validValue, "Field1");
            string? result2 = rule.Validate(invalidValue, "Field2");
            string? result3 = rule.Validate(validValue, "Field3");

            // Assert
            result1.ShouldBeNull();
            result2.ShouldNotBeNull();
            result3.ShouldBeNull();
        }

        [Fact]
        public void Validate_DifferentRuleInstances_ShouldWorkIndependently()
        {
            // Arrange
            ExactLengthRule rule1 = new(5);
            ExactLengthRule rule2 = new(10);
            string value = "Hello";

            // Act
            string? result1 = rule1.Validate(value, "Field");
            string? result2 = rule2.Validate(value, "Field");

            // Assert
            result1.ShouldBeNull();
            result2.ShouldNotBeNull();
        }
    }

    #endregion Multiple Validation Tests
}