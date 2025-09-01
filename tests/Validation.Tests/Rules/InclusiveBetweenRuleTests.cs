using FlowRight.Validation.Rules;
using Shouldly;

namespace FlowRight.Validation.Tests.Rules;

/// <summary>
/// Comprehensive failing tests for InclusiveBetweenRule that define expected behavior
/// for inclusive range validation. These tests follow TDD principles and will initially 
/// fail until the InclusiveBetweenRule implementations are complete.
/// 
/// Test Coverage:
/// - Integer-specific InclusiveBetweenRule validation
/// - Generic InclusiveBetweenRule<T> validation for multiple numeric types
/// - Valid values within range (inclusive bounds)
/// - Invalid values below minimum
/// - Invalid values above maximum
/// - Edge cases (boundary values, negative ranges)
/// - Error message validation
/// - Multiple numeric types (int, long, decimal, double, float, short, byte)
/// 
/// Current Status: FAILING as expected (TDD Red phase)
/// These failing tests serve as executable specifications for InclusiveBetweenRule.
/// </summary>
public class InclusiveBetweenRuleTests
{
    #region Integer InclusiveBetweenRule Tests

    /// <summary>
    /// Tests for the integer-specific InclusiveBetweenRule
    /// </summary>
    public class IntegerInclusiveBetweenRuleTests
    {
        #region Constructor Tests

        public class Constructor
        {
            [Fact]
            public void Constructor_WithValidRange_ShouldCreateRule()
            {
                // Arrange & Act
                InclusiveBetweenRule rule = new(1, 10);

                // Assert
                rule.ShouldNotBeNull();
            }

            [Fact]
            public void Constructor_WithEqualBounds_ShouldCreateRule()
            {
                // Arrange & Act
                InclusiveBetweenRule rule = new(5, 5);

                // Assert
                rule.ShouldNotBeNull();
            }

            [Fact]
            public void Constructor_WithNegativeRange_ShouldCreateRule()
            {
                // Arrange & Act
                InclusiveBetweenRule rule = new(-10, -1);

                // Assert
                rule.ShouldNotBeNull();
            }

            [Fact]
            public void Constructor_WithReversedBounds_ShouldCreateRule()
            {
                // Arrange & Act
                InclusiveBetweenRule rule = new(10, 1);

                // Assert
                rule.ShouldNotBeNull();
            }
        }

        #endregion Constructor Tests

        #region Valid Range Tests

        public class ValidRangeTests
        {
            [Theory]
            [InlineData(1, 10, 1)]
            [InlineData(1, 10, 5)]
            [InlineData(1, 10, 10)]
            [InlineData(-5, 5, 0)]
            [InlineData(-10, -1, -5)]
            public void Validate_WithValueInRange_ShouldReturnNull(int from, int to, int value)
            {
                // Arrange
                InclusiveBetweenRule rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Field");

                // Assert
                result.ShouldBeNull();
            }

            [Fact]
            public void Validate_WithMinimumBoundaryValue_ShouldReturnNull()
            {
                // Arrange
                InclusiveBetweenRule rule = new(1, 100);
                int minimumValue = 1;

                // Act
                string? result = rule.Validate(minimumValue, "Age");

                // Assert
                result.ShouldBeNull();
            }

            [Fact]
            public void Validate_WithMaximumBoundaryValue_ShouldReturnNull()
            {
                // Arrange
                InclusiveBetweenRule rule = new(1, 100);
                int maximumValue = 100;

                // Act
                string? result = rule.Validate(maximumValue, "Age");

                // Assert
                result.ShouldBeNull();
            }

            [Fact]
            public void Validate_WithEqualBounds_ShouldReturnNull()
            {
                // Arrange
                InclusiveBetweenRule rule = new(42, 42);
                int exactValue = 42;

                // Act
                string? result = rule.Validate(exactValue, "Field");

                // Assert
                result.ShouldBeNull();
            }

            [Fact]
            public void Validate_WithZeroInRange_ShouldReturnNull()
            {
                // Arrange
                InclusiveBetweenRule rule = new(-1, 1);
                int zeroValue = 0;

                // Act
                string? result = rule.Validate(zeroValue, "Field");

                // Assert
                result.ShouldBeNull();
            }
        }

        #endregion Valid Range Tests

        #region Invalid Range Tests

        public class InvalidRangeTests
        {
            [Fact]
            public void Validate_WithValueBelowMinimum_ShouldReturnErrorMessage()
            {
                // Arrange
                InclusiveBetweenRule rule = new(10, 20);
                int belowMinimum = 5;

                // Act
                string? result = rule.Validate(belowMinimum, "Score");

                // Assert
                result.ShouldNotBeNull();
                result.ShouldBe("Score must be between 10 and 20 (inclusive)");
            }

            [Fact]
            public void Validate_WithValueAboveMaximum_ShouldReturnErrorMessage()
            {
                // Arrange
                InclusiveBetweenRule rule = new(10, 20);
                int aboveMaximum = 25;

                // Act
                string? result = rule.Validate(aboveMaximum, "Score");

                // Assert
                result.ShouldNotBeNull();
                result.ShouldBe("Score must be between 10 and 20 (inclusive)");
            }

            [Theory]
            [InlineData(1, 10, 0, "Age")]
            [InlineData(1, 10, 11, "Count")]
            [InlineData(-10, -1, -11, "Temperature")]
            [InlineData(-10, -1, 0, "Offset")]
            public void Validate_WithValuesOutsideRange_ShouldReturnCorrectErrorMessage(int from, int to, int value, string displayName)
            {
                // Arrange
                InclusiveBetweenRule rule = new(from, to);

                // Act
                string? result = rule.Validate(value, displayName);

                // Assert
                result.ShouldNotBeNull();
                result.ShouldBe($"{displayName} must be between {from} and {to} (inclusive)");
            }

            [Fact]
            public void Validate_WithMinValueInteger_ShouldReturnErrorMessage()
            {
                // Arrange
                InclusiveBetweenRule rule = new(0, 100);
                int minValue = int.MinValue;

                // Act
                string? result = rule.Validate(minValue, "Field");

                // Assert
                result.ShouldNotBeNull();
            }

            [Fact]
            public void Validate_WithMaxValueInteger_ShouldReturnErrorMessage()
            {
                // Arrange
                InclusiveBetweenRule rule = new(0, 100);
                int maxValue = int.MaxValue;

                // Act
                string? result = rule.Validate(maxValue, "Field");

                // Assert
                result.ShouldNotBeNull();
            }
        }

        #endregion Invalid Range Tests

        #region Error Message Tests

        public class ErrorMessageTests
        {
            [Fact]
            public void Validate_WithInvalidValue_ShouldIncludeDisplayName()
            {
                // Arrange
                InclusiveBetweenRule rule = new(1, 10);
                string displayName = "User Age";

                // Act
                string? result = rule.Validate(15, displayName);

                // Assert
                result.ShouldNotBeNull();
                result.ShouldStartWith("User Age");
            }

            [Fact]
            public void Validate_WithInvalidValue_ShouldIncludeBothBounds()
            {
                // Arrange
                int from = 5;
                int to = 25;
                InclusiveBetweenRule rule = new(from, to);

                // Act
                string? result = rule.Validate(30, "Field");

                // Assert
                result.ShouldNotBeNull();
                result.ShouldContain(from.ToString());
                result.ShouldContain(to.ToString());
            }

            [Fact]
            public void Validate_WithInvalidValue_ShouldIndicateInclusive()
            {
                // Arrange
                InclusiveBetweenRule rule = new(1, 10);

                // Act
                string? result = rule.Validate(15, "Field");

                // Assert
                result.ShouldNotBeNull();
                result.ShouldContain("(inclusive)");
            }
        }

        #endregion Error Message Tests
    }

    #endregion Integer InclusiveBetweenRule Tests

    #region Generic InclusiveBetweenRule Tests

    /// <summary>
    /// Tests for the generic InclusiveBetweenRule<T>
    /// </summary>
    public class GenericInclusiveBetweenRuleTests
    {
        #region Constructor Tests

        public class Constructor
        {
            [Fact]
            public void Constructor_WithValidDecimalRange_ShouldCreateRule()
            {
                // Arrange & Act
                InclusiveBetweenRule<decimal> rule = new(1.0m, 10.0m);

                // Assert
                rule.ShouldNotBeNull();
            }

            [Fact]
            public void Constructor_WithValidDoubleRange_ShouldCreateRule()
            {
                // Arrange & Act
                InclusiveBetweenRule<double> rule = new(1.0, 10.0);

                // Assert
                rule.ShouldNotBeNull();
            }

            [Fact]
            public void Constructor_WithValidLongRange_ShouldCreateRule()
            {
                // Arrange & Act
                InclusiveBetweenRule<long> rule = new(1L, 10L);

                // Assert
                rule.ShouldNotBeNull();
            }

            [Fact]
            public void Constructor_WithReversedDecimalBounds_ShouldCreateRule()
            {
                // Arrange & Act
                InclusiveBetweenRule<decimal> rule = new(10.0m, 1.0m);

                // Assert
                rule.ShouldNotBeNull();
            }
        }

        #endregion Constructor Tests

        #region Integer Generic Tests

        public class IntegerGenericTests
        {
            [Theory]
            [InlineData(1, 10, 1)]
            [InlineData(1, 10, 5)]
            [InlineData(1, 10, 10)]
            [InlineData(-5, 5, 0)]
            public void Validate_WithValidIntegerRange_ShouldReturnNull(int from, int to, int value)
            {
                // Arrange
                InclusiveBetweenRule<int> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Field");

                // Assert
                result.ShouldBeNull();
            }

            [Theory]
            [InlineData(5, 10, 4)]
            [InlineData(5, 10, 11)]
            [InlineData(-10, -5, -11)]
            [InlineData(-10, -5, -4)]
            public void Validate_WithInvalidIntegerRange_ShouldReturnErrorMessage(int from, int to, int value)
            {
                // Arrange
                InclusiveBetweenRule<int> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Field");

                // Assert
                result.ShouldNotBeNull();
                result.ShouldBe($"Field must be between {from} and {to} (inclusive)");
            }
        }

        #endregion Integer Generic Tests

        #region Decimal Tests

        public class DecimalTests
        {
            [Theory]
            [InlineData(1.0, 10.0, 1.0)]
            [InlineData(1.0, 10.0, 5.5)]
            [InlineData(1.0, 10.0, 10.0)]
            [InlineData(-5.5, 5.5, 0.0)]
            [InlineData(0.1, 0.9, 0.5)]
            public void Validate_WithValidDecimalRange_ShouldReturnNull(double fromDouble, double toDouble, double valueDouble)
            {
                // Arrange
                decimal from = (decimal)fromDouble;
                decimal to = (decimal)toDouble;
                decimal value = (decimal)valueDouble;
                InclusiveBetweenRule<decimal> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Price");

                // Assert
                result.ShouldBeNull();
            }

            [Theory]
            [InlineData(1.0, 10.0, 0.9)]
            [InlineData(1.0, 10.0, 10.1)]
            [InlineData(-5.5, -1.0, -5.6)]
            [InlineData(-5.5, -1.0, -0.9)]
            public void Validate_WithInvalidDecimalRange_ShouldReturnErrorMessage(double fromDouble, double toDouble, double valueDouble)
            {
                // Arrange
                decimal from = (decimal)fromDouble;
                decimal to = (decimal)toDouble;
                decimal value = (decimal)valueDouble;
                InclusiveBetweenRule<decimal> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Amount");

                // Assert
                result.ShouldNotBeNull();
                result.ShouldBe($"Amount must be between {from} and {to} (inclusive)");
            }

            [Fact]
            public void Validate_WithPreciseDecimalBoundaries_ShouldRespectPrecision()
            {
                // Arrange
                InclusiveBetweenRule<decimal> rule = new(1.000m, 2.000m);
                decimal exactMin = 1.000m;
                decimal exactMax = 2.000m;
                decimal justBelow = 0.999m;
                decimal justAbove = 2.001m;

                // Act
                string? resultMin = rule.Validate(exactMin, "Field");
                string? resultMax = rule.Validate(exactMax, "Field");
                string? resultBelow = rule.Validate(justBelow, "Field");
                string? resultAbove = rule.Validate(justAbove, "Field");

                // Assert
                resultMin.ShouldBeNull();
                resultMax.ShouldBeNull();
                resultBelow.ShouldNotBeNull();
                resultAbove.ShouldNotBeNull();
            }
        }

        #endregion Decimal Tests

        #region Double Tests

        public class DoubleTests
        {
            [Theory]
            [InlineData(1.0, 10.0, 1.0)]
            [InlineData(1.0, 10.0, 5.5)]
            [InlineData(1.0, 10.0, 10.0)]
            [InlineData(-5.5, 5.5, 0.0)]
            public void Validate_WithValidDoubleRange_ShouldReturnNull(double from, double to, double value)
            {
                // Arrange
                InclusiveBetweenRule<double> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Temperature");

                // Assert
                result.ShouldBeNull();
            }

            [Theory]
            [InlineData(1.0, 10.0, 0.9)]
            [InlineData(1.0, 10.0, 10.1)]
            [InlineData(-5.5, -1.0, -5.6)]
            [InlineData(-5.5, -1.0, -0.9)]
            public void Validate_WithInvalidDoubleRange_ShouldReturnErrorMessage(double from, double to, double value)
            {
                // Arrange
                InclusiveBetweenRule<double> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Ratio");

                // Assert
                result.ShouldNotBeNull();
                result.ShouldBe($"Ratio must be between {from} and {to} (inclusive)");
            }

            [Fact]
            public void Validate_WithDoubleMinMaxValues_ShouldHandleExtremeValues()
            {
                // Arrange
                InclusiveBetweenRule<double> rule = new(0.0, 1.0);
                double minValue = double.MinValue;
                double maxValue = double.MaxValue;

                // Act
                string? resultMin = rule.Validate(minValue, "Field");
                string? resultMax = rule.Validate(maxValue, "Field");

                // Assert
                resultMin.ShouldNotBeNull();
                resultMax.ShouldNotBeNull();
            }
        }

        #endregion Double Tests

        #region Float Tests

        public class FloatTests
        {
            [Theory]
            [InlineData(1.0f, 10.0f, 1.0f)]
            [InlineData(1.0f, 10.0f, 5.5f)]
            [InlineData(1.0f, 10.0f, 10.0f)]
            [InlineData(-5.5f, 5.5f, 0.0f)]
            public void Validate_WithValidFloatRange_ShouldReturnNull(float from, float to, float value)
            {
                // Arrange
                InclusiveBetweenRule<float> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Coordinate");

                // Assert
                result.ShouldBeNull();
            }

            [Theory]
            [InlineData(1.0f, 10.0f, 0.9f)]
            [InlineData(1.0f, 10.0f, 10.1f)]
            public void Validate_WithInvalidFloatRange_ShouldReturnErrorMessage(float from, float to, float value)
            {
                // Arrange
                InclusiveBetweenRule<float> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Factor");

                // Assert
                result.ShouldNotBeNull();
                result.ShouldBe($"Factor must be between {from} and {to} (inclusive)");
            }
        }

        #endregion Float Tests

        #region Long Tests

        public class LongTests
        {
            [Theory]
            [InlineData(1L, 100L, 1L)]
            [InlineData(1L, 100L, 50L)]
            [InlineData(1L, 100L, 100L)]
            [InlineData(-50L, 50L, 0L)]
            public void Validate_WithValidLongRange_ShouldReturnNull(long from, long to, long value)
            {
                // Arrange
                InclusiveBetweenRule<long> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Identifier");

                // Assert
                result.ShouldBeNull();
            }

            [Theory]
            [InlineData(1L, 100L, 0L)]
            [InlineData(1L, 100L, 101L)]
            [InlineData(-100L, -1L, -101L)]
            [InlineData(-100L, -1L, 0L)]
            public void Validate_WithInvalidLongRange_ShouldReturnErrorMessage(long from, long to, long value)
            {
                // Arrange
                InclusiveBetweenRule<long> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Count");

                // Assert
                result.ShouldNotBeNull();
                result.ShouldBe($"Count must be between {from} and {to} (inclusive)");
            }

            [Fact]
            public void Validate_WithLongMaxValues_ShouldHandleExtremeValues()
            {
                // Arrange
                InclusiveBetweenRule<long> rule = new(0L, 1000L);
                long minValue = long.MinValue;
                long maxValue = long.MaxValue;

                // Act
                string? resultMin = rule.Validate(minValue, "Field");
                string? resultMax = rule.Validate(maxValue, "Field");

                // Assert
                resultMin.ShouldNotBeNull();
                resultMax.ShouldNotBeNull();
            }
        }

        #endregion Long Tests

        #region Short Tests

        public class ShortTests
        {
            [Theory]
            [InlineData((short)1, (short)10, (short)1)]
            [InlineData((short)1, (short)10, (short)5)]
            [InlineData((short)1, (short)10, (short)10)]
            [InlineData((short)-5, (short)5, (short)0)]
            public void Validate_WithValidShortRange_ShouldReturnNull(short from, short to, short value)
            {
                // Arrange
                InclusiveBetweenRule<short> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Level");

                // Assert
                result.ShouldBeNull();
            }

            [Theory]
            [InlineData((short)5, (short)10, (short)4)]
            [InlineData((short)5, (short)10, (short)11)]
            public void Validate_WithInvalidShortRange_ShouldReturnErrorMessage(short from, short to, short value)
            {
                // Arrange
                InclusiveBetweenRule<short> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Priority");

                // Assert
                result.ShouldNotBeNull();
                result.ShouldBe($"Priority must be between {from} and {to} (inclusive)");
            }
        }

        #endregion Short Tests

        #region Byte Tests

        public class ByteTests
        {
            [Theory]
            [InlineData((byte)1, (byte)10, (byte)1)]
            [InlineData((byte)1, (byte)10, (byte)5)]
            [InlineData((byte)1, (byte)10, (byte)10)]
            [InlineData((byte)0, (byte)255, (byte)128)]
            public void Validate_WithValidByteRange_ShouldReturnNull(byte from, byte to, byte value)
            {
                // Arrange
                InclusiveBetweenRule<byte> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Intensity");

                // Assert
                result.ShouldBeNull();
            }

            [Theory]
            [InlineData((byte)10, (byte)20, (byte)9)]
            [InlineData((byte)10, (byte)20, (byte)21)]
            public void Validate_WithInvalidByteRange_ShouldReturnErrorMessage(byte from, byte to, byte value)
            {
                // Arrange
                InclusiveBetweenRule<byte> rule = new(from, to);

                // Act
                string? result = rule.Validate(value, "Opacity");

                // Assert
                result.ShouldNotBeNull();
                result.ShouldBe($"Opacity must be between {from} and {to} (inclusive)");
            }

            [Fact]
            public void Validate_WithFullByteRange_ShouldAcceptAllValues()
            {
                // Arrange
                InclusiveBetweenRule<byte> rule = new(byte.MinValue, byte.MaxValue);
                byte[] testValues = [0, 1, 127, 128, 254, 255];

                // Act & Assert
                foreach (byte value in testValues)
                {
                    string? result = rule.Validate(value, "Field");
                    result.ShouldBeNull($"Value {value} should be valid in full byte range");
                }
            }
        }

        #endregion Byte Tests

        #region Equal Bounds Tests

        public class EqualBoundsTests
        {
            [Fact]
            public void Validate_WithEqualDecimalBounds_ShouldAcceptExactValue()
            {
                // Arrange
                decimal exactValue = 3.14159m;
                InclusiveBetweenRule<decimal> rule = new(exactValue, exactValue);

                // Act
                string? result = rule.Validate(exactValue, "Pi");

                // Assert
                result.ShouldBeNull();
            }

            [Fact]
            public void Validate_WithEqualBoundsAndDifferentValue_ShouldReturnErrorMessage()
            {
                // Arrange
                int exactValue = 42;
                InclusiveBetweenRule<int> rule = new(exactValue, exactValue);
                int differentValue = 43;

                // Act
                string? result = rule.Validate(differentValue, "Answer");

                // Assert
                result.ShouldNotBeNull();
                result.ShouldBe($"Answer must be between {exactValue} and {exactValue} (inclusive)");
            }
        }

        #endregion Equal Bounds Tests

        #region Multiple Validation Tests

        public class MultipleValidationTests
        {
            [Fact]
            public void Validate_SameRuleMultipleTimes_ShouldProduceConsistentResults()
            {
                // Arrange
                InclusiveBetweenRule<int> rule = new(1, 10);

                // Act
                string? result1 = rule.Validate(5, "Field1");
                string? result2 = rule.Validate(15, "Field2");
                string? result3 = rule.Validate(1, "Field3");

                // Assert
                result1.ShouldBeNull();
                result2.ShouldNotBeNull();
                result3.ShouldBeNull();
            }

            [Fact]
            public void Validate_DifferentRuleInstances_ShouldWorkIndependently()
            {
                // Arrange
                InclusiveBetweenRule<decimal> rule1 = new(1.0m, 5.0m);
                InclusiveBetweenRule<decimal> rule2 = new(10.0m, 20.0m);
                decimal testValue = 3.0m;

                // Act
                string? result1 = rule1.Validate(testValue, "Field");
                string? result2 = rule2.Validate(testValue, "Field");

                // Assert
                result1.ShouldBeNull();
                result2.ShouldNotBeNull();
            }
        }

        #endregion Multiple Validation Tests
    }

    #endregion Generic InclusiveBetweenRule Tests
}