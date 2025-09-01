using FlowRight.Validation.Rules;
using Shouldly;

namespace FlowRight.Validation.Tests.Rules;

/// <summary>
/// Comprehensive failing tests for general validation rules that define expected behavior
/// for universal validation patterns. These tests follow TDD principles 
/// and will initially fail until the validation rule implementations are complete.
/// 
/// Test Coverage:
/// - NullRule&lt;T&gt; - validates value is null
/// - NotNullRule&lt;T&gt; - validates value is not null
/// - EmptyRule&lt;T&gt; - validates value is empty (strings, collections, etc.)
/// - NotEmptyRule&lt;T&gt; - validates value is not empty
/// - EqualRule&lt;T&gt; - validates value equals expected value
/// - NotEqualRule&lt;T&gt; - validates value does not equal comparison value
/// - MustRule&lt;T&gt; - validates value passes custom predicate function
/// 
/// Current Status: FAILING as expected (TDD Red phase)
/// These failing tests serve as executable specifications for general validation rules.
/// </summary>
public class GeneralValidationRulesTests
{
    #region NullRule Tests

    /// <summary>
    /// Tests for NullRule&lt;T&gt; - validates value is null
    /// </summary>
    public class NullRuleTests
    {
        [Fact]
        public void Validate_WithNullValue_ShouldReturnNull()
        {
            // Arrange
            NullRule<string> rule = new();
            string? nullValue = null;

            // Act
            string? result = rule.Validate(nullValue, "Value");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNonNullValue_ShouldReturnErrorMessage()
        {
            // Arrange
            NullRule<string> rule = new();
            string nonNullValue = "test";

            // Act
            string? result = rule.Validate(nonNullValue, "Value");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be null");
        }

        [Fact]
        public void Validate_WithNullReferenceType_ShouldReturnNull()
        {
            // Arrange
            NullRule<object> rule = new();
            object? nullObject = null;

            // Act
            string? result = rule.Validate(nullObject, "Object");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNonNullReferenceType_ShouldReturnErrorMessage()
        {
            // Arrange
            NullRule<object> rule = new();
            object nonNullObject = new();

            // Act
            string? result = rule.Validate(nonNullObject, "Object");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be null");
        }

        [Fact]
        public void Validate_WithNullableValueType_ShouldWork()
        {
            // Arrange
            NullRule<int?> rule = new();
            int? nullInt = null;
            int? nonNullInt = 42;

            // Act & Assert
            rule.Validate(nullInt, "NullInt").ShouldBeNull();
            rule.Validate(nonNullInt, "NonNullInt").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithNullableGuid_ShouldWork()
        {
            // Arrange
            NullRule<Guid?> rule = new();
            Guid? nullGuid = null;
            Guid? nonNullGuid = Guid.NewGuid();

            // Act & Assert
            rule.Validate(nullGuid, "NullGuid").ShouldBeNull();
            rule.Validate(nonNullGuid, "NonNullGuid").ShouldNotBeNull();
        }
    }

    #endregion NullRule Tests

    #region NotNullRule Tests

    /// <summary>
    /// Tests for NotNullRule&lt;T&gt; - validates value is not null
    /// </summary>
    public class NotNullRuleTests
    {
        [Fact]
        public void Validate_WithNonNullValue_ShouldReturnNull()
        {
            // Arrange
            NotNullRule<string> rule = new();
            string nonNullValue = "test";

            // Act
            string? result = rule.Validate(nonNullValue, "Value");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullValue_ShouldReturnErrorMessage()
        {
            // Arrange
            NotNullRule<string> rule = new();
            string? nullValue = null;

            // Act
            string? result = rule.Validate(nullValue, "Value");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be null");
        }

        [Fact]
        public void Validate_WithEmptyStringValue_ShouldReturnNull()
        {
            // Arrange
            NotNullRule<string> rule = new();
            string emptyValue = string.Empty;

            // Act
            string? result = rule.Validate(emptyValue, "Value");

            // Assert
            result.ShouldBeNull(); // Empty string is not null
        }

        [Fact]
        public void Validate_WithNonNullReferenceType_ShouldReturnNull()
        {
            // Arrange
            NotNullRule<object> rule = new();
            object nonNullObject = new();

            // Act
            string? result = rule.Validate(nonNullObject, "Object");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullReferenceType_ShouldReturnErrorMessage()
        {
            // Arrange
            NotNullRule<object> rule = new();
            object? nullObject = null;

            // Act
            string? result = rule.Validate(nullObject, "Object");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be null");
        }

        [Fact]
        public void Validate_WithNullableValueTypes_ShouldWork()
        {
            // Arrange
            NotNullRule<int?> intRule = new();
            NotNullRule<DateTime?> dateRule = new();
            
            int? nullInt = null;
            int? validInt = 42;
            DateTime? nullDate = null;
            DateTime? validDate = DateTime.Now;

            // Act & Assert
            intRule.Validate(validInt, "ValidInt").ShouldBeNull();
            intRule.Validate(nullInt, "NullInt").ShouldNotBeNull();
            dateRule.Validate(validDate, "ValidDate").ShouldBeNull();
            dateRule.Validate(nullDate, "NullDate").ShouldNotBeNull();
        }
    }

    #endregion NotNullRule Tests

    #region EmptyRule Tests

    /// <summary>
    /// Tests for EmptyRule&lt;T&gt; - validates value is empty
    /// </summary>
    public class EmptyRuleTests
    {
        [Fact]
        public void Validate_WithEmptyString_ShouldReturnNull()
        {
            // Arrange
            EmptyRule<string> rule = new();
            string emptyString = string.Empty;

            // Act
            string? result = rule.Validate(emptyString, "Value");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNonEmptyString_ShouldReturnErrorMessage()
        {
            // Arrange
            EmptyRule<string> rule = new();
            string nonEmptyString = "test";

            // Act
            string? result = rule.Validate(nonEmptyString, "Value");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be empty");
        }

        [Fact]
        public void Validate_WithNullString_ShouldReturnNull()
        {
            // Arrange
            EmptyRule<string> rule = new();
            string? nullString = null;

            // Act
            string? result = rule.Validate(nullString, "Value");

            // Assert
            result.ShouldBeNull(); // Null is considered "empty"
        }

        [Fact]
        public void Validate_WithWhitespaceString_ShouldReturnErrorMessage()
        {
            // Arrange
            EmptyRule<string> rule = new();
            string whitespaceString = "   ";

            // Act
            string? result = rule.Validate(whitespaceString, "Value");

            // Assert
            result.ShouldNotBeNull(); // Whitespace is not empty
            result.ShouldContain("must be empty");
        }

        [Fact]
        public void Validate_WithEmptyCollection_ShouldReturnNull()
        {
            // Arrange
            EmptyRule<IEnumerable<string>> rule = new();
            IEnumerable<string> emptyCollection = Array.Empty<string>();

            // Act
            string? result = rule.Validate(emptyCollection, "Collection");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNonEmptyCollection_ShouldReturnErrorMessage()
        {
            // Arrange
            EmptyRule<IEnumerable<string>> rule = new();
            IEnumerable<string> nonEmptyCollection = new[] { "item" };

            // Act
            string? result = rule.Validate(nonEmptyCollection, "Collection");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be empty");
        }

        [Fact]
        public void Validate_WithEmptyGuid_ShouldReturnNull()
        {
            // Arrange
            EmptyRule<Guid> rule = new();
            Guid emptyGuid = Guid.Empty;

            // Act
            string? result = rule.Validate(emptyGuid, "GUID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNonEmptyGuid_ShouldReturnErrorMessage()
        {
            // Arrange
            EmptyRule<Guid> rule = new();
            Guid nonEmptyGuid = Guid.NewGuid();

            // Act
            string? result = rule.Validate(nonEmptyGuid, "GUID");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be empty");
        }
    }

    #endregion EmptyRule Tests

    #region NotEmptyRule Tests

    /// <summary>
    /// Tests for NotEmptyRule&lt;T&gt; - validates value is not empty
    /// </summary>
    public class NotEmptyRuleTests
    {
        [Fact]
        public void Validate_WithNonEmptyString_ShouldReturnNull()
        {
            // Arrange
            NotEmptyRule<string> rule = new();
            string nonEmptyString = "test";

            // Act
            string? result = rule.Validate(nonEmptyString, "Value");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithEmptyString_ShouldReturnErrorMessage()
        {
            // Arrange
            NotEmptyRule<string> rule = new();
            string emptyString = string.Empty;

            // Act
            string? result = rule.Validate(emptyString, "Value");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be empty");
        }

        [Fact]
        public void Validate_WithNullString_ShouldReturnErrorMessage()
        {
            // Arrange
            NotEmptyRule<string> rule = new();
            string? nullString = null;

            // Act
            string? result = rule.Validate(nullString, "Value");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be empty");
        }

        [Fact]
        public void Validate_WithWhitespaceString_ShouldReturnNull()
        {
            // Arrange
            NotEmptyRule<string> rule = new();
            string whitespaceString = "   ";

            // Act
            string? result = rule.Validate(whitespaceString, "Value");

            // Assert
            result.ShouldBeNull(); // Whitespace is not empty for basic NotEmpty rule
        }

        [Fact]
        public void Validate_WithNonEmptyCollection_ShouldReturnNull()
        {
            // Arrange
            NotEmptyRule<IEnumerable<string>> rule = new();
            IEnumerable<string> nonEmptyCollection = new[] { "item" };

            // Act
            string? result = rule.Validate(nonEmptyCollection, "Collection");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithEmptyCollection_ShouldReturnErrorMessage()
        {
            // Arrange
            NotEmptyRule<IEnumerable<string>> rule = new();
            IEnumerable<string> emptyCollection = Array.Empty<string>();

            // Act
            string? result = rule.Validate(emptyCollection, "Collection");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be empty");
        }

        [Fact]
        public void Validate_WithNonEmptyGuid_ShouldReturnNull()
        {
            // Arrange
            NotEmptyRule<Guid> rule = new();
            Guid nonEmptyGuid = Guid.NewGuid();

            // Act
            string? result = rule.Validate(nonEmptyGuid, "GUID");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithEmptyGuid_ShouldReturnErrorMessage()
        {
            // Arrange
            NotEmptyRule<Guid> rule = new();
            Guid emptyGuid = Guid.Empty;

            // Act
            string? result = rule.Validate(emptyGuid, "GUID");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be empty");
        }
    }

    #endregion NotEmptyRule Tests

    #region EqualRule Tests

    /// <summary>
    /// Tests for EqualRule&lt;T&gt; - validates value equals expected value
    /// </summary>
    public class EqualRuleTests
    {
        [Fact]
        public void Validate_WithEqualStringValues_ShouldReturnNull()
        {
            // Arrange
            EqualRule<string> rule = new("expected");
            string equalValue = "expected";

            // Act
            string? result = rule.Validate(equalValue, "Value");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithDifferentStringValues_ShouldReturnErrorMessage()
        {
            // Arrange
            EqualRule<string> rule = new("expected");
            string differentValue = "different";

            // Act
            string? result = rule.Validate(differentValue, "Value");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be equal to 'expected'");
        }

        [Fact]
        public void Validate_WithEqualIntegerValues_ShouldReturnNull()
        {
            // Arrange
            EqualRule<int> rule = new(42);
            int equalValue = 42;

            // Act
            string? result = rule.Validate(equalValue, "Number");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithDifferentIntegerValues_ShouldReturnErrorMessage()
        {
            // Arrange
            EqualRule<int> rule = new(42);
            int differentValue = 24;

            // Act
            string? result = rule.Validate(differentValue, "Number");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be equal to 42");
        }

        [Fact]
        public void Validate_WithNullValues_ShouldWork()
        {
            // Arrange
            EqualRule<string?> rule = new(null);
            string? nullValue = null;
            string nonNullValue = "test";

            // Act & Assert
            rule.Validate(nullValue, "NullValue").ShouldBeNull();
            rule.Validate(nonNullValue, "NonNullValue").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithEqualComplexObjects_ShouldReturnNull()
        {
            // Arrange
            TestObject expectedObject = new("Test", 42);
            EqualRule<TestObject> rule = new(expectedObject);
            TestObject equalObject = new("Test", 42);

            // Act
            string? result = rule.Validate(equalObject, "Object");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithDifferentComplexObjects_ShouldReturnErrorMessage()
        {
            // Arrange
            TestObject expectedObject = new("Test", 42);
            EqualRule<TestObject> rule = new(expectedObject);
            TestObject differentObject = new("Different", 24);

            // Act
            string? result = rule.Validate(differentObject, "Object");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be equal to");
        }

        [Fact]
        public void Validate_WithDateTimeValues_ShouldWork()
        {
            // Arrange
            DateTime expectedDate = new(2024, 1, 1);
            EqualRule<DateTime> rule = new(expectedDate);
            DateTime equalDate = new(2024, 1, 1);
            DateTime differentDate = new(2024, 1, 2);

            // Act & Assert
            rule.Validate(equalDate, "EqualDate").ShouldBeNull();
            rule.Validate(differentDate, "DifferentDate").ShouldNotBeNull();
        }

        private record TestObject(string Name, int Value);
    }

    #endregion EqualRule Tests

    #region NotEqualRule Tests

    /// <summary>
    /// Tests for NotEqualRule&lt;T&gt; - validates value does not equal comparison value
    /// </summary>
    public class NotEqualRuleTests
    {
        [Fact]
        public void Validate_WithDifferentStringValues_ShouldReturnNull()
        {
            // Arrange
            NotEqualRule<string> rule = new("forbidden");
            string differentValue = "allowed";

            // Act
            string? result = rule.Validate(differentValue, "Value");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithEqualStringValues_ShouldReturnErrorMessage()
        {
            // Arrange
            NotEqualRule<string> rule = new("forbidden");
            string equalValue = "forbidden";

            // Act
            string? result = rule.Validate(equalValue, "Value");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be equal to 'forbidden'");
        }

        [Fact]
        public void Validate_WithDifferentIntegerValues_ShouldReturnNull()
        {
            // Arrange
            NotEqualRule<int> rule = new(0);
            int differentValue = 42;

            // Act
            string? result = rule.Validate(differentValue, "Number");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithEqualIntegerValues_ShouldReturnErrorMessage()
        {
            // Arrange
            NotEqualRule<int> rule = new(0);
            int equalValue = 0;

            // Act
            string? result = rule.Validate(equalValue, "Number");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be equal to 0");
        }

        [Fact]
        public void Validate_WithNullValues_ShouldWork()
        {
            // Arrange
            NotEqualRule<string?> rule = new(null);
            string? nullValue = null;
            string nonNullValue = "test";

            // Act & Assert
            rule.Validate(nonNullValue, "NonNullValue").ShouldBeNull();
            rule.Validate(nullValue, "NullValue").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithGuidValues_ShouldWork()
        {
            // Arrange
            Guid forbiddenGuid = Guid.Empty;
            NotEqualRule<Guid> rule = new(forbiddenGuid);
            Guid differentGuid = Guid.NewGuid();

            // Act & Assert
            rule.Validate(differentGuid, "DifferentGuid").ShouldBeNull();
            rule.Validate(forbiddenGuid, "ForbiddenGuid").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithComplexObjects_ShouldWork()
        {
            // Arrange
            TestItem forbiddenItem = new("Forbidden", 0);
            NotEqualRule<TestItem> rule = new(forbiddenItem);
            TestItem allowedItem = new("Allowed", 1);
            TestItem equalItem = new("Forbidden", 0);

            // Act & Assert
            rule.Validate(allowedItem, "AllowedItem").ShouldBeNull();
            rule.Validate(equalItem, "EqualItem").ShouldNotBeNull();
        }

        private record TestItem(string Name, int Value);
    }

    #endregion NotEqualRule Tests

    #region MustRule Tests

    /// <summary>
    /// Tests for MustRule&lt;T&gt; - validates value passes custom predicate function
    /// </summary>
    public class MustRuleTests
    {
        [Fact]
        public void Validate_WithPassingPredicate_ShouldReturnNull()
        {
            // Arrange
            MustRule<int> rule = new(x => x > 0, "must be positive");
            int validValue = 5;

            // Act
            string? result = rule.Validate(validValue, "Number");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithFailingPredicate_ShouldReturnErrorMessage()
        {
            // Arrange
            MustRule<int> rule = new(x => x > 0, "must be positive");
            int invalidValue = -5;

            // Act
            string? result = rule.Validate(invalidValue, "Number");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be positive");
        }

        [Fact]
        public void Validate_WithStringLengthPredicate_ShouldWork()
        {
            // Arrange
            MustRule<string> rule = new(s => !string.IsNullOrEmpty(s) && s.Length >= 3, "must be at least 3 characters");
            string validString = "Hello";
            string invalidString = "Hi";
            string? nullString = null;

            // Act & Assert
            rule.Validate(validString, "ValidString").ShouldBeNull();
            rule.Validate(invalidString, "InvalidString").ShouldNotBeNull();
            rule.Validate(nullString!, "NullString").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithComplexObjectPredicate_ShouldWork()
        {
            // Arrange
            MustRule<Person> rule = new(p => p.Age >= 18 && !string.IsNullOrEmpty(p.Name), "must be adult with valid name");
            Person validPerson = new("John", 25);
            Person invalidPerson1 = new("Jane", 16);
            Person invalidPerson2 = new("", 30);

            // Act & Assert
            rule.Validate(validPerson, "ValidPerson").ShouldBeNull();
            rule.Validate(invalidPerson1, "InvalidPerson1").ShouldNotBeNull();
            rule.Validate(invalidPerson2, "InvalidPerson2").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithDateTimePredicate_ShouldWork()
        {
            // Arrange
            DateTime cutoffDate = new(2024, 1, 1);
            MustRule<DateTime> rule = new(d => d >= cutoffDate, "must be after 2024-01-01");
            DateTime validDate = new(2024, 6, 15);
            DateTime invalidDate = new(2023, 12, 31);

            // Act & Assert
            rule.Validate(validDate, "ValidDate").ShouldBeNull();
            rule.Validate(invalidDate, "InvalidDate").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithCollectionPredicate_ShouldWork()
        {
            // Arrange
            MustRule<IEnumerable<string>> rule = new(items => items.Count() > 0 && items.All(s => !string.IsNullOrEmpty(s)), "must contain non-empty items");
            IEnumerable<string> validCollection = new[] { "Item1", "Item2" };
            IEnumerable<string> invalidCollection1 = Array.Empty<string>();
            IEnumerable<string> invalidCollection2 = new[] { "Item1", "" };

            // Act & Assert
            rule.Validate(validCollection, "ValidCollection").ShouldBeNull();
            rule.Validate(invalidCollection1, "InvalidCollection1").ShouldNotBeNull();
            rule.Validate(invalidCollection2, "InvalidCollection2").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithNullablePredicate_ShouldWork()
        {
            // Arrange
            MustRule<int?> rule = new(x => x.HasValue && x.Value > 0, "must have positive value");
            int? validValue = 5;
            int? invalidValue1 = null;
            int? invalidValue2 = -5;

            // Act & Assert
            rule.Validate(validValue, "ValidValue").ShouldBeNull();
            rule.Validate(invalidValue1, "InvalidValue1").ShouldNotBeNull();
            rule.Validate(invalidValue2, "InvalidValue2").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithMultipleConditionsPredicate_ShouldWork()
        {
            // Arrange
            MustRule<string> rule = new(
                s => !string.IsNullOrEmpty(s) && 
                     s.Length >= 8 && 
                     s.Any(char.IsDigit) && 
                     s.Any(char.IsUpper) && 
                     s.Any(char.IsLower),
                "must be strong password (8+ chars, digit, upper, lower)"
            );
            
            string validPassword = "MyPass123";
            string invalidPassword1 = "weak";
            string invalidPassword2 = "NoDigits";
            string invalidPassword3 = "nouppcase123";

            // Act & Assert
            rule.Validate(validPassword, "ValidPassword").ShouldBeNull();
            rule.Validate(invalidPassword1, "InvalidPassword1").ShouldNotBeNull();
            rule.Validate(invalidPassword2, "InvalidPassword2").ShouldNotBeNull();
            rule.Validate(invalidPassword3, "InvalidPassword3").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithCustomMessageFormatting_ShouldWork()
        {
            // Arrange
            string customMessage = "The value '{0}' does not meet the custom criteria for {1}";
            MustRule<int> rule = new(x => x % 2 == 0, customMessage);
            int oddValue = 3;

            // Act
            string? result = rule.Validate(oddValue, "EvenNumber");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain(customMessage.Replace("{0}", oddValue.ToString()).Replace("{1}", "EvenNumber"));
        }

        private record Person(string Name, int Age);
    }

    #endregion MustRule Tests

    #region Edge Cases and Integration Tests

    /// <summary>
    /// Tests for edge cases and integration scenarios with general rules
    /// </summary>
    public class GeneralRulesEdgeCasesTests
    {
        [Fact]
        public void Validate_ChainedGeneralRules_ShouldWorkTogether()
        {
            // Arrange
            NotNullRule<string> notNullRule = new();
            NotEmptyRule<string> notEmptyRule = new();
            MustRule<string> lengthRule = new(s => s.Length >= 3, "must be at least 3 characters");

            string validValue = "Hello";
            string nullValue = null!;
            string emptyValue = "";
            string shortValue = "Hi";

            // Act & Assert - Valid value passes all rules
            notNullRule.Validate(validValue, "ValidValue").ShouldBeNull();
            notEmptyRule.Validate(validValue, "ValidValue").ShouldBeNull();
            lengthRule.Validate(validValue, "ValidValue").ShouldBeNull();

            // Act & Assert - Invalid values fail appropriate rules
            notNullRule.Validate(nullValue, "NullValue").ShouldNotBeNull();
            notEmptyRule.Validate(emptyValue, "EmptyValue").ShouldNotBeNull();
            lengthRule.Validate(shortValue, "ShortValue").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithDefaultValues_ShouldHandleCorrectly()
        {
            // Arrange
            EqualRule<int> intRule = new(default(int));
            EqualRule<Guid> guidRule = new(default(Guid));
            EqualRule<DateTime> dateRule = new(default(DateTime));

            // Act & Assert
            intRule.Validate(0, "Zero").ShouldBeNull();
            guidRule.Validate(Guid.Empty, "EmptyGuid").ShouldBeNull();
            dateRule.Validate(DateTime.MinValue, "MinDate").ShouldBeNull();
        }

        [Fact]
        public void Validate_WithValueTypesAndReferenceTypes_ShouldBehaveConsistently()
        {
            // Arrange
            NotEqualRule<int> intRule = new(0);
            NotEqualRule<string> stringRule = new("forbidden");
            NotEqualRule<object> objectRule = new(new object());

            // Act & Assert - Different values should pass
            intRule.Validate(1, "NonZero").ShouldBeNull();
            stringRule.Validate("allowed", "Allowed").ShouldBeNull();
            objectRule.Validate(new object(), "DifferentObject").ShouldBeNull();

            // Act & Assert - Equal values should fail
            intRule.Validate(0, "Zero").ShouldNotBeNull();
            stringRule.Validate("forbidden", "Forbidden").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithGenericConstraints_ShouldWork()
        {
            // Arrange - Test with different generic types
            EqualRule<IComparable<int>> comparableIntRule = new(42);
            string[] expectedArray = ["test"];
            EqualRule<IEnumerable<string>> enumerableRule = new(expectedArray);

            // Act & Assert
            comparableIntRule.Validate(42, "ComparableInt").ShouldBeNull();
            enumerableRule.Validate(expectedArray, "Enumerable").ShouldBeNull(); // Same instance should be equal
        }

        [Fact]
        public void Validate_WithComplexPredicateLogic_ShouldHandleEdgeCases()
        {
            // Arrange - Complex validation logic combining multiple conditions
            MustRule<string> complexRule = new(
                value => {
                    if (string.IsNullOrEmpty(value)) return false;
                    
                    // Must be alphanumeric with specific pattern
                    bool hasValidFormat = value.All(c => char.IsLetterOrDigit(c) || c == '-');
                    bool hasValidLength = value.Length >= 5 && value.Length <= 20;
                    bool startsWithLetter = char.IsLetter(value[0]);
                    bool endsWithDigit = char.IsDigit(value[^1]);
                    
                    return hasValidFormat && hasValidLength && startsWithLetter && endsWithDigit;
                },
                "must be 5-20 chars, alphanumeric with dashes, start with letter, end with digit"
            );

            // Test various combinations
            string validValue = "User-123-1";
            string invalidLength = "U1";
            string invalidStart = "1User-1";
            string invalidEnd = "User-1A";
            string invalidChars = "User@123";

            // Act & Assert
            complexRule.Validate(validValue, "Valid").ShouldBeNull();
            complexRule.Validate(invalidLength, "InvalidLength").ShouldNotBeNull();
            complexRule.Validate(invalidStart, "InvalidStart").ShouldNotBeNull();
            complexRule.Validate(invalidEnd, "InvalidEnd").ShouldNotBeNull();
            complexRule.Validate(invalidChars, "InvalidChars").ShouldNotBeNull();
        }

        [Fact]
        public void Validate_WithCultureSensitiveComparisons_ShouldWork()
        {
            // Arrange
            EqualRule<string> caseInsensitiveRule = new("TEST");
            
            string upperCase = "TEST";
            string lowerCase = "test";
            string mixedCase = "Test";

            // Act & Assert - Default comparison is case-sensitive
            caseInsensitiveRule.Validate(upperCase, "UpperCase").ShouldBeNull();
            caseInsensitiveRule.Validate(lowerCase, "LowerCase").ShouldNotBeNull();
            caseInsensitiveRule.Validate(mixedCase, "MixedCase").ShouldNotBeNull();
        }
    }

    #endregion Edge Cases and Integration Tests
}