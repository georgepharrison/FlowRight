using FlowRight.Validation.Rules;
using Shouldly;

namespace FlowRight.Validation.Tests.Rules;

/// <summary>
/// Comprehensive failing tests for numeric validation rules that define expected behavior
/// for numeric validation patterns. These tests follow TDD principles and will initially 
/// fail until the validation rule implementations are complete.
/// 
/// Test Coverage:
/// - GreaterThanRule&lt;T&gt; - validates values greater than comparison
/// - LessThanRule&lt;T&gt; - validates values less than comparison  
/// - BetweenRule&lt;T&gt; - validates values within range (inclusive/exclusive)
/// - PositiveRule&lt;T&gt; - validates positive numbers
/// - NegativeRule&lt;T&gt; - validates negative numbers
/// - InclusiveBetweenRule&lt;T&gt; - validates values within inclusive range
/// - ExclusiveBetweenRule&lt;T&gt; - validates values within exclusive range
/// 
/// Current Status: FAILING as expected (TDD Red phase)
/// These failing tests serve as executable specifications for numeric validation rules.
/// </summary>
public class NumericValidationRulesTests
{
    #region GreaterThanRule Tests

    /// <summary>
    /// Tests for GreaterThanRule&lt;T&gt; - validates values are greater than comparison value
    /// </summary>
    public class GreaterThanRuleTests
    {
        [Fact]
        public void Validate_IntValue_WithGreaterValue_ShouldReturnNull()
        {
            // Arrange
            GreaterThanRule<int> rule = new(5);
            int validValue = 10;

            // Act
            string? result = rule.Validate(validValue, "Age");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_IntValue_WithEqualValue_ShouldReturnErrorMessage()
        {
            // Arrange
            GreaterThanRule<int> rule = new(5);
            int invalidValue = 5;

            // Act
            string? result = rule.Validate(invalidValue, "Age");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be greater than 5");
        }

        [Fact]
        public void Validate_IntValue_WithLesserValue_ShouldReturnErrorMessage()
        {
            // Arrange
            GreaterThanRule<int> rule = new(5);
            int invalidValue = 3;

            // Act
            string? result = rule.Validate(invalidValue, "Age");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be greater than 5");
        }

        [Fact]
        public void Validate_DecimalValue_WithGreaterValue_ShouldReturnNull()
        {
            // Arrange
            GreaterThanRule<decimal> rule = new(100.50m);
            decimal validValue = 150.75m;

            // Act
            string? result = rule.Validate(validValue, "Salary");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_DoubleValue_WithGreaterValue_ShouldReturnNull()
        {
            // Arrange
            GreaterThanRule<double> rule = new(85.5);
            double validValue = 90.0;

            // Act
            string? result = rule.Validate(validValue, "Score");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_LongValue_WithGreaterValue_ShouldReturnNull()
        {
            // Arrange
            GreaterThanRule<long> rule = new(1000000000L);
            long validValue = 5555555555L;

            // Act
            string? result = rule.Validate(validValue, "Phone");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_NullValue_ShouldReturnNull()
        {
            // Arrange
            GreaterThanRule<int?> rule = new(5);

            // Act
            string? result = rule.Validate(null, "Age");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion GreaterThanRule Tests

    #region LessThanRule Tests

    /// <summary>
    /// Tests for LessThanRule&lt;T&gt; - validates values are less than comparison value
    /// </summary>
    public class LessThanRuleTests
    {
        [Fact]
        public void Validate_IntValue_WithLesserValue_ShouldReturnNull()
        {
            // Arrange
            LessThanRule<int> rule = new(100);
            int validValue = 50;

            // Act
            string? result = rule.Validate(validValue, "Age");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_IntValue_WithEqualValue_ShouldReturnErrorMessage()
        {
            // Arrange
            LessThanRule<int> rule = new(100);
            int invalidValue = 100;

            // Act
            string? result = rule.Validate(invalidValue, "Age");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be less than 100");
        }

        [Fact]
        public void Validate_IntValue_WithGreaterValue_ShouldReturnErrorMessage()
        {
            // Arrange
            LessThanRule<int> rule = new(100);
            int invalidValue = 150;

            // Act
            string? result = rule.Validate(invalidValue, "Age");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be less than 100");
        }

        [Fact]
        public void Validate_DecimalValue_WithLesserValue_ShouldReturnNull()
        {
            // Arrange
            LessThanRule<decimal> rule = new(1000.00m);
            decimal validValue = 999.99m;

            // Act
            string? result = rule.Validate(validValue, "Amount");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_FloatValue_WithLesserValue_ShouldReturnNull()
        {
            // Arrange
            LessThanRule<float> rule = new(5.0f);
            float validValue = 4.9f;

            // Act
            string? result = rule.Validate(validValue, "Rating");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_ShortValue_WithLesserValue_ShouldReturnNull()
        {
            // Arrange
            LessThanRule<short> rule = new(1000);
            short validValue = 500;

            // Act
            string? result = rule.Validate(validValue, "Priority");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion LessThanRule Tests

    #region BetweenRule Tests

    /// <summary>
    /// Tests for BetweenRule&lt;T&gt; - validates values are within specified range
    /// </summary>
    public class BetweenRuleTests
    {
        [Fact]
        public void Validate_IntValue_WithinRange_ShouldReturnNull()
        {
            // Arrange
            BetweenRule<int> rule = new(18, 65);
            int validValue = 30;

            // Act
            string? result = rule.Validate(validValue, "Age");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_IntValue_AtLowerBoundary_ShouldReturnNull()
        {
            // Arrange
            BetweenRule<int> rule = new(18, 65);
            int validValue = 18;

            // Act
            string? result = rule.Validate(validValue, "Age");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_IntValue_AtUpperBoundary_ShouldReturnNull()
        {
            // Arrange
            BetweenRule<int> rule = new(18, 65);
            int validValue = 65;

            // Act
            string? result = rule.Validate(validValue, "Age");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_IntValue_BelowRange_ShouldReturnErrorMessage()
        {
            // Arrange
            BetweenRule<int> rule = new(18, 65);
            int invalidValue = 17;

            // Act
            string? result = rule.Validate(invalidValue, "Age");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be between 18 and 65");
        }

        [Fact]
        public void Validate_IntValue_AboveRange_ShouldReturnErrorMessage()
        {
            // Arrange
            BetweenRule<int> rule = new(18, 65);
            int invalidValue = 66;

            // Act
            string? result = rule.Validate(invalidValue, "Age");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be between 18 and 65");
        }

        [Fact]
        public void Validate_DecimalValue_WithinRange_ShouldReturnNull()
        {
            // Arrange
            BetweenRule<decimal> rule = new(1000.00m, 5000.00m);
            decimal validValue = 2500.50m;

            // Act
            string? result = rule.Validate(validValue, "Amount");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion BetweenRule Tests

    #region PositiveRule Tests

    /// <summary>
    /// Tests for PositiveRule&lt;T&gt; - validates values are positive (greater than zero)
    /// </summary>
    public class PositiveRuleTests
    {
        [Fact]
        public void Validate_IntValue_WithPositiveValue_ShouldReturnNull()
        {
            // Arrange
            PositiveRule<int> rule = new();
            int validValue = 5;

            // Act
            string? result = rule.Validate(validValue, "Count");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_IntValue_WithZero_ShouldReturnErrorMessage()
        {
            // Arrange
            PositiveRule<int> rule = new();
            int invalidValue = 0;

            // Act
            string? result = rule.Validate(invalidValue, "Count");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be positive");
        }

        [Fact]
        public void Validate_IntValue_WithNegativeValue_ShouldReturnErrorMessage()
        {
            // Arrange
            PositiveRule<int> rule = new();
            int invalidValue = -5;

            // Act
            string? result = rule.Validate(invalidValue, "Count");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be positive");
        }

        [Fact]
        public void Validate_DecimalValue_WithPositiveValue_ShouldReturnNull()
        {
            // Arrange
            PositiveRule<decimal> rule = new();
            decimal validValue = 123.45m;

            // Act
            string? result = rule.Validate(validValue, "Amount");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_DoubleValue_WithSmallPositiveValue_ShouldReturnNull()
        {
            // Arrange
            PositiveRule<double> rule = new();
            double validValue = 0.001;

            // Act
            string? result = rule.Validate(validValue, "Rate");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_FloatValue_WithPositiveValue_ShouldReturnNull()
        {
            // Arrange
            PositiveRule<float> rule = new();
            float validValue = 1.5f;

            // Act
            string? result = rule.Validate(validValue, "Rating");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_LongValue_WithPositiveValue_ShouldReturnNull()
        {
            // Arrange
            PositiveRule<long> rule = new();
            long validValue = 1234567890L;

            // Act
            string? result = rule.Validate(validValue, "ID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_ShortValue_WithPositiveValue_ShouldReturnNull()
        {
            // Arrange
            PositiveRule<short> rule = new();
            short validValue = 100;

            // Act
            string? result = rule.Validate(validValue, "Priority");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion PositiveRule Tests

    #region NegativeRule Tests

    /// <summary>
    /// Tests for NegativeRule&lt;T&gt; - validates values are negative (less than zero)
    /// </summary>
    public class NegativeRuleTests
    {
        [Fact]
        public void Validate_IntValue_WithNegativeValue_ShouldReturnNull()
        {
            // Arrange
            NegativeRule<int> rule = new();
            int validValue = -5;

            // Act
            string? result = rule.Validate(validValue, "Adjustment");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_IntValue_WithZero_ShouldReturnErrorMessage()
        {
            // Arrange
            NegativeRule<int> rule = new();
            int invalidValue = 0;

            // Act
            string? result = rule.Validate(invalidValue, "Adjustment");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be negative");
        }

        [Fact]
        public void Validate_IntValue_WithPositiveValue_ShouldReturnErrorMessage()
        {
            // Arrange
            NegativeRule<int> rule = new();
            int invalidValue = 5;

            // Act
            string? result = rule.Validate(invalidValue, "Adjustment");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be negative");
        }

        [Fact]
        public void Validate_DecimalValue_WithNegativeValue_ShouldReturnNull()
        {
            // Arrange
            NegativeRule<decimal> rule = new();
            decimal validValue = -123.45m;

            // Act
            string? result = rule.Validate(validValue, "Balance");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_DoubleValue_WithSmallNegativeValue_ShouldReturnNull()
        {
            // Arrange
            NegativeRule<double> rule = new();
            double validValue = -0.001;

            // Act
            string? result = rule.Validate(validValue, "Change");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_FloatValue_WithNegativeValue_ShouldReturnNull()
        {
            // Arrange
            NegativeRule<float> rule = new();
            float validValue = -2.5f;

            // Act
            string? result = rule.Validate(validValue, "Offset");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_SByteValue_WithNegativeValue_ShouldReturnNull()
        {
            // Arrange
            NegativeRule<sbyte> rule = new();
            sbyte validValue = -10;

            // Act
            string? result = rule.Validate(validValue, "Delta");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion NegativeRule Tests

    #region InclusiveBetweenRule Tests

    /// <summary>
    /// Tests for InclusiveBetweenRule&lt;T&gt; - validates values are within inclusive range
    /// </summary>
    public class InclusiveBetweenRuleTests
    {
        [Fact]
        public void Validate_IntValue_WithinInclusiveRange_ShouldReturnNull()
        {
            // Arrange
            InclusiveBetweenRule<int> rule = new(18, 65);
            int validValue = 30;

            // Act
            string? result = rule.Validate(validValue, "Age");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_IntValue_AtLowerBoundaryInclusive_ShouldReturnNull()
        {
            // Arrange
            InclusiveBetweenRule<int> rule = new(18, 65);
            int validValue = 18;

            // Act
            string? result = rule.Validate(validValue, "Age");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_IntValue_AtUpperBoundaryInclusive_ShouldReturnNull()
        {
            // Arrange
            InclusiveBetweenRule<int> rule = new(18, 65);
            int validValue = 65;

            // Act
            string? result = rule.Validate(validValue, "Age");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_IntValue_BelowInclusiveRange_ShouldReturnErrorMessage()
        {
            // Arrange
            InclusiveBetweenRule<int> rule = new(18, 65);
            int invalidValue = 17;

            // Act
            string? result = rule.Validate(invalidValue, "Age");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be between 18 and 65 (inclusive)");
        }

        [Fact]
        public void Validate_IntValue_AboveInclusiveRange_ShouldReturnErrorMessage()
        {
            // Arrange
            InclusiveBetweenRule<int> rule = new(18, 65);
            int invalidValue = 66;

            // Act
            string? result = rule.Validate(invalidValue, "Age");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be between 18 and 65 (inclusive)");
        }

        [Fact]
        public void Validate_DecimalValue_WithinInclusiveRange_ShouldReturnNull()
        {
            // Arrange
            InclusiveBetweenRule<decimal> rule = new(1000.00m, 5000.00m);
            decimal validValue = 1000.00m; // At lower boundary

            // Act
            string? result = rule.Validate(validValue, "Amount");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_DoubleValue_WithinInclusiveRange_ShouldReturnNull()
        {
            // Arrange
            InclusiveBetweenRule<double> rule = new(0.0, 100.0);
            double validValue = 100.0; // At upper boundary

            // Act
            string? result = rule.Validate(validValue, "Percentage");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_LongValue_WithinInclusiveRange_ShouldReturnNull()
        {
            // Arrange
            InclusiveBetweenRule<long> rule = new(1000000000L, 9999999999L);
            long validValue = 5555555555L;

            // Act
            string? result = rule.Validate(validValue, "Phone");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_FloatValue_WithinInclusiveRange_ShouldReturnNull()
        {
            // Arrange
            InclusiveBetweenRule<float> rule = new(1.0f, 5.0f);
            float validValue = 3.5f;

            // Act
            string? result = rule.Validate(validValue, "Rating");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_ShortValue_WithinInclusiveRange_ShouldReturnNull()
        {
            // Arrange
            InclusiveBetweenRule<short> rule = new(1, 10);
            short validValue = 5;

            // Act
            string? result = rule.Validate(validValue, "Priority");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_ByteValue_WithinInclusiveRange_ShouldReturnNull()
        {
            // Arrange
            InclusiveBetweenRule<byte> rule = new(100, 200);
            byte validValue = 150;

            // Act
            string? result = rule.Validate(validValue, "Status");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_UIntValue_WithinInclusiveRange_ShouldReturnNull()
        {
            // Arrange
            InclusiveBetweenRule<uint> rule = new(1000U, 9999U);
            uint validValue = 5000U;

            // Act
            string? result = rule.Validate(validValue, "Points");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_ULongValue_WithinInclusiveRange_ShouldReturnNull()
        {
            // Arrange
            InclusiveBetweenRule<ulong> rule = new(1000000000UL, 9000000000UL);
            ulong validValue = 5000000000UL;

            // Act
            string? result = rule.Validate(validValue, "Token");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion InclusiveBetweenRule Tests

    #region ExclusiveBetweenRule Tests

    /// <summary>
    /// Tests for ExclusiveBetweenRule&lt;T&gt; - validates values are within exclusive range
    /// </summary>
    public class ExclusiveBetweenRuleTests
    {
        [Fact]
        public void Validate_IntValue_WithinExclusiveRange_ShouldReturnNull()
        {
            // Arrange
            ExclusiveBetweenRule<int> rule = new(18, 65);
            int validValue = 30;

            // Act
            string? result = rule.Validate(validValue, "Age");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_IntValue_AtLowerBoundaryExclusive_ShouldReturnErrorMessage()
        {
            // Arrange
            ExclusiveBetweenRule<int> rule = new(18, 65);
            int invalidValue = 18;

            // Act
            string? result = rule.Validate(invalidValue, "Age");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be between 18 and 65 (exclusive)");
        }

        [Fact]
        public void Validate_IntValue_AtUpperBoundaryExclusive_ShouldReturnErrorMessage()
        {
            // Arrange
            ExclusiveBetweenRule<int> rule = new(18, 65);
            int invalidValue = 65;

            // Act
            string? result = rule.Validate(invalidValue, "Age");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be between 18 and 65 (exclusive)");
        }

        [Fact]
        public void Validate_IntValue_JustInsideExclusiveRange_ShouldReturnNull()
        {
            // Arrange
            ExclusiveBetweenRule<int> rule = new(18, 65);
            int validValue = 19;

            // Act
            string? result = rule.Validate(validValue, "Age");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_DecimalValue_WithinExclusiveRange_ShouldReturnNull()
        {
            // Arrange
            ExclusiveBetweenRule<decimal> rule = new(0.0m, 100.0m);
            decimal validValue = 50.0m;

            // Act
            string? result = rule.Validate(validValue, "Percentage");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_DoubleValue_AtBoundaryExclusive_ShouldReturnErrorMessage()
        {
            // Arrange
            ExclusiveBetweenRule<double> rule = new(0.0, 1.0);
            double invalidValue = 1.0;

            // Act
            string? result = rule.Validate(invalidValue, "Probability");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be between 0 and 1 (exclusive)");
        }

        [Fact]
        public void Validate_FloatValue_WithinExclusiveRange_ShouldReturnNull()
        {
            // Arrange
            ExclusiveBetweenRule<float> rule = new(0.0f, 10.0f);
            float validValue = 5.5f;

            // Act
            string? result = rule.Validate(validValue, "Score");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion ExclusiveBetweenRule Tests

    #region Edge Cases and Cross-Type Tests

    /// <summary>
    /// Tests for edge cases and cross-type numeric validations
    /// </summary>
    public class NumericEdgeCasesTests
    {
        [Fact]
        public void Validate_NullableTypes_WithNullValues_ShouldHandleCorrectly()
        {
            // Arrange
            PositiveRule<int?> positiveRule = new();
            GreaterThanRule<decimal?> greaterRule = new(100.0m);
            LessThanRule<double?> lessRule = new(1000.0);

            // Act & Assert
            positiveRule.Validate(null, "Value").ShouldBeNull();
            greaterRule.Validate(null, "Value").ShouldBeNull();
            lessRule.Validate(null, "Value").ShouldBeNull();
        }

        [Fact]
        public void Validate_MinMaxValues_ShouldHandleExtremeValues()
        {
            // Arrange
            GreaterThanRule<int> intRule = new(int.MaxValue - 1);
            LessThanRule<long> longRule = new(long.MinValue + 1);
            PositiveRule<decimal> decimalRule = new();

            // Act & Assert
            intRule.Validate(int.MaxValue, "MaxInt").ShouldBeNull();
            longRule.Validate(long.MinValue, "MinLong").ShouldBeNull();
            decimalRule.Validate(decimal.MaxValue, "MaxDecimal").ShouldBeNull();
        }

        [Fact]
        public void Validate_FloatingPointPrecision_ShouldHandleCorrectly()
        {
            // Arrange
            GreaterThanRule<double> doubleRule = new(0.1 + 0.2); // Known floating point issue: 0.1 + 0.2 = 0.30000000000000004
            BetweenRule<float> floatRule = new(0.999999f, 1.000001f);

            double testDouble = 0.3; // This is actually less than 0.30000000000000004
            float testFloat = 1.0f;

            // Act & Assert - Floating point precision must be handled correctly
            doubleRule.Validate(testDouble, "Double").ShouldNotBeNull(); // 0.3 is NOT greater than 0.30000000000000004
            floatRule.Validate(testFloat, "Float").ShouldBeNull(); // 1.0f IS between 0.999999f and 1.000001f
        }

        [Fact]
        public void Validate_UnsignedTypes_WithValidValues_ShouldPass()
        {
            // Arrange
            GreaterThanRule<uint> uintRule = new(1000U);
            LessThanRule<ulong> ulongRule = new(ulong.MaxValue);
            PositiveRule<ushort> ushortRule = new();

            // Act & Assert
            uintRule.Validate(2000U, "UInt").ShouldBeNull();
            ulongRule.Validate(1000UL, "ULong").ShouldBeNull();
            ushortRule.Validate((ushort)100, "UShort").ShouldBeNull();
        }

        [Fact]
        public void Validate_SignedTypes_WithValidValues_ShouldPass()
        {
            // Arrange
            NegativeRule<sbyte> sbyteRule = new();
            BetweenRule<short> shortRule = new(-1000, 1000);
            PositiveRule<long> longRule = new();

            // Act & Assert
            sbyteRule.Validate((sbyte)-10, "SByte").ShouldBeNull();
            shortRule.Validate((short)500, "Short").ShouldBeNull();
            longRule.Validate(1000000L, "Long").ShouldBeNull();
        }
    }

    #endregion Edge Cases and Cross-Type Tests
}