using FlowRight.Validation.Rules;
using Shouldly;

namespace FlowRight.Validation.Tests.Rules;

/// <summary>
/// Comprehensive failing tests for LengthRule that define expected behavior
/// for string length range validation. These tests follow TDD principles and will initially 
/// fail until the LengthRule implementation is complete.
/// 
/// Test Coverage:
/// - Valid strings within min/max length bounds
/// - Invalid strings too short (below minimum)
/// - Invalid strings too long (above maximum)
/// - Edge cases (null values, empty strings, equal min/max)
/// - Boundary conditions (exactly at min/max)
/// - Error message validation for different scenarios
/// - Constructor validation for invalid ranges
/// 
/// Current Status: FAILING as expected (TDD Red phase)
/// These failing tests serve as executable specifications for LengthRule.
/// </summary>
public class LengthRuleTests
{
    #region Constructor Tests

    /// <summary>
    /// Tests for LengthRule constructor behavior
    /// </summary>
    public class Constructor
    {
        [Fact]
        public void Constructor_WithValidRange_ShouldCreateRule()
        {
            // Arrange & Act
            LengthRule rule = new(1, 10);

            // Assert
            rule.ShouldNotBeNull();
        }

        [Fact]
        public void Constructor_WithEqualMinMax_ShouldCreateRule()
        {
            // Arrange & Act
            LengthRule rule = new(5, 5);

            // Assert
            rule.ShouldNotBeNull();
        }

        [Fact]
        public void Constructor_WithZeroMinimum_ShouldCreateRule()
        {
            // Arrange & Act
            LengthRule rule = new(0, 10);

            // Assert
            rule.ShouldNotBeNull();
        }

        [Fact]
        public void Constructor_WithReversedBounds_ShouldCreateRule()
        {
            // Arrange & Act
            LengthRule rule = new(10, 5);

            // Assert
            rule.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(-1, 10)]
        [InlineData(0, -1)]
        [InlineData(-5, -2)]
        public void Constructor_WithNegativeValues_ShouldCreateRule(int min, int max)
        {
            // Arrange & Act
            LengthRule rule = new(min, max);

            // Assert
            rule.ShouldNotBeNull();
        }
    }

    #endregion Constructor Tests

    #region Valid Length Tests

    /// <summary>
    /// Tests for strings with valid lengths within the specified range
    /// </summary>
    public class ValidLengthTests
    {
        [Fact]
        public void Validate_WithLengthAtMinimum_ShouldReturnNull()
        {
            // Arrange
            LengthRule rule = new(3, 10);
            string validValue = "ABC";

            // Act
            string? result = rule.Validate(validValue, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithLengthAtMaximum_ShouldReturnNull()
        {
            // Arrange
            LengthRule rule = new(3, 10);
            string validValue = "1234567890";

            // Act
            string? result = rule.Validate(validValue, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithLengthInMiddleOfRange_ShouldReturnNull()
        {
            // Arrange
            LengthRule rule = new(2, 8);
            string validValue = "Hello";

            // Act
            string? result = rule.Validate(validValue, "Greeting");

            // Assert
            result.ShouldBeNull();
        }

        [Theory]
        [InlineData(1, 10, "A")]
        [InlineData(1, 10, "Hello")]
        [InlineData(1, 10, "1234567890")]
        [InlineData(5, 5, "Exact")]
        [InlineData(0, 5, "")]
        [InlineData(0, 5, "Hi")]
        public void Validate_WithVariousValidLengths_ShouldReturnNull(int min, int max, string value)
        {
            // Arrange
            LengthRule rule = new(min, max);

            // Act
            string? result = rule.Validate(value, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithSpecialCharacters_ShouldReturnNull()
        {
            // Arrange
            LengthRule rule = new(5, 15);
            string specialChars = "Hello@#$%^";

            // Act
            string? result = rule.Validate(specialChars, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithUnicodeCharacters_ShouldReturnNull()
        {
            // Arrange
            LengthRule rule = new(5, 10);
            string unicodeValue = "Héllo🙂";

            // Act
            string? result = rule.Validate(unicodeValue, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithWhitespaceOnly_ShouldReturnNull()
        {
            // Arrange
            LengthRule rule = new(3, 8);
            string whitespaceValue = "   ";

            // Act
            string? result = rule.Validate(whitespaceValue, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithEmptyStringAndZeroMinimum_ShouldReturnNull()
        {
            // Arrange
            LengthRule rule = new(0, 10);
            string emptyValue = string.Empty;

            // Act
            string? result = rule.Validate(emptyValue, "Field");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion Valid Length Tests

    #region Invalid Length - Too Short Tests

    /// <summary>
    /// Tests for strings that are too short (below minimum length)
    /// </summary>
    public class TooShortTests
    {
        [Fact]
        public void Validate_WithLengthBelowMinimum_ShouldReturnErrorMessage()
        {
            // Arrange
            LengthRule rule = new(5, 10);
            string shortValue = "Hi";

            // Act
            string? result = rule.Validate(shortValue, "Password");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Password must be at least 5 characters");
        }

        [Fact]
        public void Validate_WithEmptyStringWhenMinimumRequired_ShouldReturnErrorMessage()
        {
            // Arrange
            LengthRule rule = new(3, 10);
            string emptyValue = string.Empty;

            // Act
            string? result = rule.Validate(emptyValue, "Name");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Name must be at least 3 characters");
        }

        [Theory]
        [InlineData(5, 10, "", "Field")]
        [InlineData(5, 10, "Hi", "Code")]
        [InlineData(10, 20, "Short", "Description")]
        [InlineData(1, 5, "", "Initial")]
        public void Validate_WithVariousTooShortValues_ShouldReturnCorrectErrorMessage(int min, int max, string value, string displayName)
        {
            // Arrange
            LengthRule rule = new(min, max);

            // Act
            string? result = rule.Validate(value, displayName);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe($"{displayName} must be at least {min} characters");
        }

        [Fact]
        public void Validate_WithOneCharacterBelowMinimum_ShouldReturnErrorMessage()
        {
            // Arrange
            LengthRule rule = new(10, 20);
            string justUnderMin = "123456789"; // 9 characters

            // Act
            string? result = rule.Validate(justUnderMin, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Field must be at least 10 characters");
        }
    }

    #endregion Invalid Length - Too Short Tests

    #region Invalid Length - Too Long Tests

    /// <summary>
    /// Tests for strings that are too long (above maximum length)
    /// </summary>
    public class TooLongTests
    {
        [Fact]
        public void Validate_WithLengthAboveMaximum_ShouldReturnErrorMessage()
        {
            // Arrange
            LengthRule rule = new(3, 8);
            string longValue = "This is way too long";

            // Act
            string? result = rule.Validate(longValue, "Title");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Title must not exceed 8 characters");
        }

        [Theory]
        [InlineData(1, 5, "This is too long", "Field")]
        [InlineData(5, 10, "This is definitely too long", "Description")]
        public void Validate_WithVariousTooLongValues_ShouldReturnCorrectErrorMessage(int min, int max, string value, string displayName)
        {
            // Arrange
            LengthRule rule = new(min, max);

            // Act
            string? result = rule.Validate(value, displayName);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe($"{displayName} must not exceed {max} characters");
        }

        [Fact]
        public void Validate_WithExactMaxLength_ShouldReturnNull()
        {
            // Arrange
            LengthRule rule = new(3, 7);
            string exactMaxValue = "TooLong"; // exactly 7 characters

            // Act
            string? result = rule.Validate(exactMaxValue, "Code");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithOneCharacterAboveMaximum_ShouldReturnErrorMessage()
        {
            // Arrange
            LengthRule rule = new(5, 10);
            string justOverMax = "12345678901"; // 11 characters

            // Act
            string? result = rule.Validate(justOverMax, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Field must not exceed 10 characters");
        }

        [Fact]
        public void Validate_WithVeryLongString_ShouldReturnErrorMessage()
        {
            // Arrange
            LengthRule rule = new(5, 20);
            string veryLongValue = new('A', 100);

            // Act
            string? result = rule.Validate(veryLongValue, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Field must not exceed 20 characters");
        }
    }

    #endregion Invalid Length - Too Long Tests

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
            LengthRule rule = new(5, 10);

            // Act
            string? result = rule.Validate(null!, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(1, 10)]
        [InlineData(5, 5)]
        [InlineData(10, 100)]
        public void Validate_WithNullValueForAnyRange_ShouldReturnNull(int min, int max)
        {
            // Arrange
            LengthRule rule = new(min, max);

            // Act
            string? result = rule.Validate(null!, "Field");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion Null Value Tests

    #region Equal Min Max Tests

    /// <summary>
    /// Tests for cases where minimum and maximum lengths are equal
    /// </summary>
    public class EqualMinMaxTests
    {
        [Fact]
        public void Validate_WithEqualMinMaxAndCorrectLength_ShouldReturnNull()
        {
            // Arrange
            LengthRule rule = new(5, 5);
            string exactLength = "Hello";

            // Act
            string? result = rule.Validate(exactLength, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithEqualMinMaxAndTooShort_ShouldReturnMinimumErrorMessage()
        {
            // Arrange
            LengthRule rule = new(5, 5);
            string tooShort = "Hi";

            // Act
            string? result = rule.Validate(tooShort, "Code");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Code must be at least 5 characters");
        }

        [Fact]
        public void Validate_WithEqualMinMaxAndTooLong_ShouldReturnMaximumErrorMessage()
        {
            // Arrange
            LengthRule rule = new(5, 5);
            string tooLong = "HelloWorld";

            // Act
            string? result = rule.Validate(tooLong, "Code");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Code must not exceed 5 characters");
        }

        [Fact]
        public void Validate_WithZeroLengthRequiredAndEmptyString_ShouldReturnNull()
        {
            // Arrange
            LengthRule rule = new(0, 0);
            string emptyValue = string.Empty;

            // Act
            string? result = rule.Validate(emptyValue, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithZeroLengthRequiredAndNonEmpty_ShouldReturnErrorMessage()
        {
            // Arrange
            LengthRule rule = new(0, 0);
            string nonEmpty = "A";

            // Act
            string? result = rule.Validate(nonEmpty, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Field must not exceed 0 characters");
        }
    }

    #endregion Equal Min Max Tests

    #region Error Message Tests

    /// <summary>
    /// Tests for error message formatting and content
    /// </summary>
    public class ErrorMessageTests
    {
        [Fact]
        public void Validate_WithTooShortString_ShouldIncludeDisplayName()
        {
            // Arrange
            LengthRule rule = new(10, 20);
            string shortValue = "short";
            string displayName = "User Password";

            // Act
            string? result = rule.Validate(shortValue, displayName);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldStartWith("User Password");
        }

        [Fact]
        public void Validate_WithTooLongString_ShouldIncludeDisplayName()
        {
            // Arrange
            LengthRule rule = new(5, 10);
            string longValue = "this is way too long";
            string displayName = "Product Name";

            // Act
            string? result = rule.Validate(longValue, displayName);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldStartWith("Product Name");
        }

        [Fact]
        public void Validate_WithTooShortString_ShouldIncludeMinimumLength()
        {
            // Arrange
            int minimumLength = 8;
            LengthRule rule = new(minimumLength, 15);
            string shortValue = "short";

            // Act
            string? result = rule.Validate(shortValue, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain(minimumLength.ToString());
        }

        [Fact]
        public void Validate_WithTooLongString_ShouldIncludeMaximumLength()
        {
            // Arrange
            int maximumLength = 10;
            LengthRule rule = new(5, maximumLength);
            string longValue = "this is too long";

            // Act
            string? result = rule.Validate(longValue, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain(maximumLength.ToString());
        }

        [Theory]
        [InlineData("Username", 3, 20)]
        [InlineData("Email Address", 5, 100)]
        [InlineData("Description", 10, 500)]
        public void Validate_WithTooShortValues_ShouldFormatCorrectly(string displayName, int min, int max)
        {
            // Arrange
            LengthRule rule = new(min, max);
            string shortValue = "x";

            // Act
            string? result = rule.Validate(shortValue, displayName);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe($"{displayName} must be at least {min} characters");
        }

        [Theory]
        [InlineData("Title", 1, 15)]
        [InlineData("Summary", 5, 50)]
        [InlineData("Comment", 10, 200)]
        public void Validate_WithTooLongValues_ShouldFormatCorrectly(string displayName, int min, int max)
        {
            // Arrange
            LengthRule rule = new(min, max);
            string longValue = new('A', max + 10);

            // Act
            string? result = rule.Validate(longValue, displayName);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe($"{displayName} must not exceed {max} characters");
        }
    }

    #endregion Error Message Tests

    #region Boundary Tests

    /// <summary>
    /// Tests for boundary conditions and edge cases
    /// </summary>
    public class BoundaryTests
    {
        [Fact]
        public void Validate_WithExactMinimumLength_ShouldReturnNull()
        {
            // Arrange
            LengthRule rule = new(5, 15);
            string exactMin = "12345";

            // Act
            string? result = rule.Validate(exactMin, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithExactMaximumLength_ShouldReturnNull()
        {
            // Arrange
            LengthRule rule = new(5, 15);
            string exactMax = "123456789012345";

            // Act
            string? result = rule.Validate(exactMax, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithVeryLargeMaximum_ShouldHandleGracefully()
        {
            // Arrange
            LengthRule rule = new(0, int.MaxValue);
            string normalValue = "This is a normal string";

            // Act
            string? result = rule.Validate(normalValue, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithLargeStringWithinBounds_ShouldReturnNull()
        {
            // Arrange
            LengthRule rule = new(500, 1500);
            string largeString = new('A', 1000);

            // Act
            string? result = rule.Validate(largeString, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithLargeStringExceedingBounds_ShouldReturnErrorMessage()
        {
            // Arrange
            int maxLength = 100;
            LengthRule rule = new(10, maxLength);
            string tooLargeString = new('A', maxLength + 50);

            // Act
            string? result = rule.Validate(tooLargeString, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe($"Field must not exceed {maxLength} characters");
        }
    }

    #endregion Boundary Tests

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
            LengthRule rule = new(5, 10);
            string validValue = "Hello";
            string tooShort = "Hi";
            string tooLong = "This is too long";

            // Act
            string? result1 = rule.Validate(validValue, "Field1");
            string? result2 = rule.Validate(tooShort, "Field2");
            string? result3 = rule.Validate(tooLong, "Field3");
            string? result4 = rule.Validate(validValue, "Field4");

            // Assert
            result1.ShouldBeNull();
            result2.ShouldNotBeNull();
            result2.ShouldContain("at least");
            result3.ShouldNotBeNull();
            result3.ShouldContain("not exceed");
            result4.ShouldBeNull();
        }

        [Fact]
        public void Validate_DifferentRuleInstances_ShouldWorkIndependently()
        {
            // Arrange
            LengthRule rule1 = new(3, 8);
            LengthRule rule2 = new(10, 20);
            string testValue = "Hello";

            // Act
            string? result1 = rule1.Validate(testValue, "Field");
            string? result2 = rule2.Validate(testValue, "Field");

            // Assert
            result1.ShouldBeNull();
            result2.ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithThreadSafety_ShouldWorkConcurrently()
        {
            // Arrange
            LengthRule rule = new(5, 15);
            string[] testValues = ["Valid", "X", "This is way too long for the rule"];
            List<string?> results = [];

            // Act
            Parallel.ForEach(testValues, value =>
            {
                string? result = rule.Validate(value, "Field");
                lock (results)
                {
                    results.Add(result);
                }
            });

            // Assert
            results.Count.ShouldBe(3);
            results.Count(r => r == null).ShouldBe(1); // "Valid" should pass
            results.Count(r => r != null).ShouldBe(2); // "X" and long string should fail
        }
    }

    #endregion Multiple Validation Tests
}