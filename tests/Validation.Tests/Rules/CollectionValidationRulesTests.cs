using FlowRight.Validation.Rules;
using Shouldly;

namespace FlowRight.Validation.Tests.Rules;

/// <summary>
/// Comprehensive failing tests for collection validation rules that define expected behavior
/// for collection and enumerable validation patterns. These tests follow TDD principles 
/// and will initially fail until the validation rule implementations are complete.
/// 
/// Test Coverage:
/// - CountRule&lt;TItem&gt; - validates exact count of items
/// - UniqueRule&lt;TItem&gt; - validates all items are unique
/// - EachRule&lt;TItem&gt; - validates each item with nested rule
/// - ContainsItemRule&lt;TItem&gt; - validates collection contains specific item
/// - MinCountRule&lt;TItem&gt; - validates minimum number of items
/// - MaxCountRule&lt;TItem&gt; - validates maximum number of items
/// - NotEmptyRule&lt;TItem&gt; - validates collection is not empty
/// - EmptyRule&lt;TItem&gt; - validates collection is empty
/// 
/// Current Status: FAILING as expected (TDD Red phase)
/// These failing tests serve as executable specifications for collection validation rules.
/// </summary>
public class CollectionValidationRulesTests
{
    #region CountRule Tests

    /// <summary>
    /// Tests for CountRule&lt;TItem&gt; - validates collection has exact number of items
    /// </summary>
    public class CountRuleTests
    {
        [Fact]
        public void Validate_WithExactCount_ShouldReturnNull()
        {
            // Arrange
            CountRule<string> rule = new(3);
            IEnumerable<string> validCollection = new[] { "Item1", "Item2", "Item3" };

            // Act
            string? result = rule.Validate(validCollection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithTooManyItems_ShouldReturnErrorMessage()
        {
            // Arrange
            CountRule<string> rule = new(2);
            IEnumerable<string> invalidCollection = new[] { "Item1", "Item2", "Item3" };

            // Act
            string? result = rule.Validate(invalidCollection, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain exactly 2 items");
            result.ShouldContain("but found 3");
        }

        [Fact]
        public void Validate_WithTooFewItems_ShouldReturnErrorMessage()
        {
            // Arrange
            CountRule<string> rule = new(5);
            IEnumerable<string> invalidCollection = new[] { "Item1", "Item2" };

            // Act
            string? result = rule.Validate(invalidCollection, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain exactly 5 items");
            result.ShouldContain("but found 2");
        }

        [Fact]
        public void Validate_WithEmptyCollection_ShouldReturnErrorMessageForNonZeroCount()
        {
            // Arrange
            CountRule<string> rule = new(1);
            IEnumerable<string> emptyCollection = Array.Empty<string>();

            // Act
            string? result = rule.Validate(emptyCollection, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain exactly 1 items");
            result.ShouldContain("but found 0");
        }

        [Fact]
        public void Validate_WithEmptyCollectionExpectingZero_ShouldReturnNull()
        {
            // Arrange
            CountRule<string> rule = new(0);
            IEnumerable<string> emptyCollection = Array.Empty<string>();

            // Act
            string? result = rule.Validate(emptyCollection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullCollection_ShouldReturnErrorMessage()
        {
            // Arrange
            CountRule<string> rule = new(3);

            // Act
            string? result = rule.Validate(null!, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be null");
        }

        [Fact]
        public void Validate_WithDifferentItemTypes_ShouldWork()
        {
            // Arrange
            CountRule<int> intRule = new(4);
            IEnumerable<int> intCollection = new[] { 1, 2, 3, 4 };

            // Act
            string? result = intRule.Validate(intCollection, "Numbers");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion CountRule Tests

    #region UniqueRule Tests

    /// <summary>
    /// Tests for UniqueRule&lt;TItem&gt; - validates all items in collection are unique
    /// </summary>
    public class UniqueRuleTests
    {
        [Fact]
        public void Validate_WithUniqueItems_ShouldReturnNull()
        {
            // Arrange
            UniqueRule<string> rule = new();
            IEnumerable<string> uniqueCollection = new[] { "Item1", "Item2", "Item3" };

            // Act
            string? result = rule.Validate(uniqueCollection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithDuplicateItems_ShouldReturnErrorMessage()
        {
            // Arrange
            UniqueRule<string> rule = new();
            IEnumerable<string> duplicateCollection = new[] { "Item1", "Item2", "Item1" };

            // Act
            string? result = rule.Validate(duplicateCollection, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain unique items");
        }

        [Fact]
        public void Validate_WithEmptyCollection_ShouldReturnNull()
        {
            // Arrange
            UniqueRule<string> rule = new();
            IEnumerable<string> emptyCollection = Array.Empty<string>();

            // Act
            string? result = rule.Validate(emptyCollection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithSingleItem_ShouldReturnNull()
        {
            // Arrange
            UniqueRule<string> rule = new();
            IEnumerable<string> singleItemCollection = new[] { "Item1" };

            // Act
            string? result = rule.Validate(singleItemCollection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullCollection_ShouldReturnErrorMessage()
        {
            // Arrange
            UniqueRule<string> rule = new();

            // Act
            string? result = rule.Validate(null!, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be null");
        }

        [Fact]
        public void Validate_WithIntegerDuplicates_ShouldReturnErrorMessage()
        {
            // Arrange
            UniqueRule<int> rule = new();
            IEnumerable<int> duplicateNumbers = new[] { 1, 2, 3, 2, 4 };

            // Act
            string? result = rule.Validate(duplicateNumbers, "Numbers");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain unique items");
        }

        [Fact]
        public void Validate_WithComplexObjectsDuplicates_ShouldReturnErrorMessage()
        {
            // Arrange
            UniqueRule<TestItem> rule = new();
            TestItem item1 = new("A", 1);
            TestItem item2 = new("B", 2);
            TestItem item3 = new("A", 1); // Duplicate of item1
            IEnumerable<TestItem> duplicateObjects = new[] { item1, item2, item3 };

            // Act
            string? result = rule.Validate(duplicateObjects, "Objects");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain unique items");
        }

        private record TestItem(string Name, int Value);
    }

    #endregion UniqueRule Tests

    #region EachRule Tests

    /// <summary>
    /// Tests for EachRule&lt;TItem&gt; - validates each item in collection with nested rule
    /// </summary>
    public class EachRuleTests
    {
        [Fact]
        public void Validate_WithAllItemsPassingNestedRule_ShouldReturnNull()
        {
            // Arrange
            NotEmptyRule<string> nestedRule = new();
            EachRule<string> rule = new(nestedRule);
            IEnumerable<string> validCollection = new[] { "Item1", "Item2", "Item3" };

            // Act
            string? result = rule.Validate(validCollection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithSomeItemsFailingNestedRule_ShouldReturnErrorMessage()
        {
            // Arrange
            NotEmptyRule<string> nestedRule = new();
            EachRule<string> rule = new(nestedRule);
            IEnumerable<string> invalidCollection = new[] { "Item1", "", "Item3" };

            // Act
            string? result = rule.Validate(invalidCollection, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("contains invalid items");
        }

        [Fact]
        public void Validate_WithEmptyCollection_ShouldReturnNull()
        {
            // Arrange
            NotEmptyRule<string> nestedRule = new();
            EachRule<string> rule = new(nestedRule);
            IEnumerable<string> emptyCollection = Array.Empty<string>();

            // Act
            string? result = rule.Validate(emptyCollection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullCollection_ShouldReturnErrorMessage()
        {
            // Arrange
            NotEmptyRule<string> nestedRule = new();
            EachRule<string> rule = new(nestedRule);

            // Act
            string? result = rule.Validate(null!, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be null");
        }

        [Fact]
        public void Validate_WithNumericRuleOnEachItem_ShouldWork()
        {
            // Arrange
            GreaterThanRule<int> nestedRule = new(0);
            EachRule<int> rule = new(nestedRule);
            IEnumerable<int> validNumbers = new[] { 1, 5, 10, 100 };

            // Act
            string? result = rule.Validate(validNumbers, "Numbers");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithSomeNegativeNumbers_ShouldReturnErrorMessage()
        {
            // Arrange
            GreaterThanRule<int> nestedRule = new(0);
            EachRule<int> rule = new(nestedRule);
            IEnumerable<int> mixedNumbers = new[] { 1, -5, 10, -2 };

            // Act
            string? result = rule.Validate(mixedNumbers, "Numbers");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("contains invalid items");
        }
    }

    #endregion EachRule Tests

    #region ContainsItemRule Tests

    /// <summary>
    /// Tests for ContainsItemRule&lt;TItem&gt; - validates collection contains specific item
    /// </summary>
    public class ContainsItemRuleTests
    {
        [Fact]
        public void Validate_WithContainedItem_ShouldReturnNull()
        {
            // Arrange
            ContainsItemRule<string> rule = new("Item2");
            IEnumerable<string> collection = new[] { "Item1", "Item2", "Item3" };

            // Act
            string? result = rule.Validate(collection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithoutContainedItem_ShouldReturnErrorMessage()
        {
            // Arrange
            ContainsItemRule<string> rule = new("Item4");
            IEnumerable<string> collection = new[] { "Item1", "Item2", "Item3" };

            // Act
            string? result = rule.Validate(collection, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain item 'Item4'");
        }

        [Fact]
        public void Validate_WithEmptyCollection_ShouldReturnErrorMessage()
        {
            // Arrange
            ContainsItemRule<string> rule = new("Item1");
            IEnumerable<string> emptyCollection = Array.Empty<string>();

            // Act
            string? result = rule.Validate(emptyCollection, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain item 'Item1'");
        }

        [Fact]
        public void Validate_WithNullCollection_ShouldReturnErrorMessage()
        {
            // Arrange
            ContainsItemRule<string> rule = new("Item1");

            // Act
            string? result = rule.Validate(null!, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be null");
        }

        [Fact]
        public void Validate_WithNullItem_ShouldWork()
        {
            // Arrange
            ContainsItemRule<string?> rule = new(null);
            IEnumerable<string?> collection = new[] { "Item1", null, "Item3" };

            // Act
            string? result = rule.Validate(collection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNumericItem_ShouldWork()
        {
            // Arrange
            ContainsItemRule<int> rule = new(42);
            IEnumerable<int> numbers = new[] { 1, 42, 100 };

            // Act
            string? result = rule.Validate(numbers, "Numbers");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithComplexObjectItem_ShouldWork()
        {
            // Arrange
            TestUser targetUser = new("John", 30);
            ContainsItemRule<TestUser> rule = new(targetUser);
            IEnumerable<TestUser> users = new[] 
            { 
                new TestUser("Jane", 25), 
                targetUser, 
                new TestUser("Bob", 35) 
            };

            // Act
            string? result = rule.Validate(users, "Users");

            // Assert
            result.ShouldBeNull();
        }

        private record TestUser(string Name, int Age);
    }

    #endregion ContainsItemRule Tests

    #region MinCountRule Tests

    /// <summary>
    /// Tests for MinCountRule&lt;TItem&gt; - validates collection has minimum number of items
    /// </summary>
    public class MinCountRuleTests
    {
        [Fact]
        public void Validate_WithExactMinimumCount_ShouldReturnNull()
        {
            // Arrange
            MinCountRule<string> rule = new(3);
            IEnumerable<string> collection = new[] { "Item1", "Item2", "Item3" };

            // Act
            string? result = rule.Validate(collection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithMoreThanMinimumCount_ShouldReturnNull()
        {
            // Arrange
            MinCountRule<string> rule = new(2);
            IEnumerable<string> collection = new[] { "Item1", "Item2", "Item3", "Item4" };

            // Act
            string? result = rule.Validate(collection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithLessThanMinimumCount_ShouldReturnErrorMessage()
        {
            // Arrange
            MinCountRule<string> rule = new(5);
            IEnumerable<string> collection = new[] { "Item1", "Item2" };

            // Act
            string? result = rule.Validate(collection, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain at least 5 items");
            result.ShouldContain("but found 2");
        }

        [Fact]
        public void Validate_WithEmptyCollectionAndNonZeroMinimum_ShouldReturnErrorMessage()
        {
            // Arrange
            MinCountRule<string> rule = new(1);
            IEnumerable<string> emptyCollection = Array.Empty<string>();

            // Act
            string? result = rule.Validate(emptyCollection, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain at least 1 items");
        }

        [Fact]
        public void Validate_WithEmptyCollectionAndZeroMinimum_ShouldReturnNull()
        {
            // Arrange
            MinCountRule<string> rule = new(0);
            IEnumerable<string> emptyCollection = Array.Empty<string>();

            // Act
            string? result = rule.Validate(emptyCollection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullCollection_ShouldReturnErrorMessage()
        {
            // Arrange
            MinCountRule<string> rule = new(1);

            // Act
            string? result = rule.Validate(null!, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be null");
        }
    }

    #endregion MinCountRule Tests

    #region MaxCountRule Tests

    /// <summary>
    /// Tests for MaxCountRule&lt;TItem&gt; - validates collection has maximum number of items
    /// </summary>
    public class MaxCountRuleTests
    {
        [Fact]
        public void Validate_WithExactMaximumCount_ShouldReturnNull()
        {
            // Arrange
            MaxCountRule<string> rule = new(3);
            IEnumerable<string> collection = new[] { "Item1", "Item2", "Item3" };

            // Act
            string? result = rule.Validate(collection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithLessThanMaximumCount_ShouldReturnNull()
        {
            // Arrange
            MaxCountRule<string> rule = new(5);
            IEnumerable<string> collection = new[] { "Item1", "Item2" };

            // Act
            string? result = rule.Validate(collection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithMoreThanMaximumCount_ShouldReturnErrorMessage()
        {
            // Arrange
            MaxCountRule<string> rule = new(2);
            IEnumerable<string> collection = new[] { "Item1", "Item2", "Item3", "Item4" };

            // Act
            string? result = rule.Validate(collection, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain at most 2 items");
            result.ShouldContain("but found 4");
        }

        [Fact]
        public void Validate_WithEmptyCollection_ShouldReturnNull()
        {
            // Arrange
            MaxCountRule<string> rule = new(3);
            IEnumerable<string> emptyCollection = Array.Empty<string>();

            // Act
            string? result = rule.Validate(emptyCollection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithZeroMaximumAndNonEmptyCollection_ShouldReturnErrorMessage()
        {
            // Arrange
            MaxCountRule<string> rule = new(0);
            IEnumerable<string> collection = new[] { "Item1" };

            // Act
            string? result = rule.Validate(collection, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must contain at most 0 items");
        }

        [Fact]
        public void Validate_WithNullCollection_ShouldReturnErrorMessage()
        {
            // Arrange
            MaxCountRule<string> rule = new(5);

            // Act
            string? result = rule.Validate(null!, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be null");
        }
    }

    #endregion MaxCountRule Tests

    #region NotEmptyRule Collection Tests

    /// <summary>
    /// Tests for NotEmptyRule&lt;TItem&gt; when applied to collections
    /// </summary>
    public class NotEmptyRuleCollectionTests
    {
        [Fact]
        public void Validate_WithNonEmptyCollection_ShouldReturnNull()
        {
            // Arrange
            NotEmptyRule<IEnumerable<string>> rule = new();
            IEnumerable<string> nonEmptyCollection = new[] { "Item1" };

            // Act
            string? result = rule.Validate(nonEmptyCollection, "Items");

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
            string? result = rule.Validate(emptyCollection, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be empty");
        }

        [Fact]
        public void Validate_WithNullCollection_ShouldReturnErrorMessage()
        {
            // Arrange
            NotEmptyRule<IEnumerable<string>> rule = new();

            // Act
            string? result = rule.Validate(null!, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must not be empty");
        }
    }

    #endregion NotEmptyRule Collection Tests

    #region EmptyRule Collection Tests

    /// <summary>
    /// Tests for EmptyRule&lt;TItem&gt; when applied to collections
    /// </summary>
    public class EmptyRuleCollectionTests
    {
        [Fact]
        public void Validate_WithEmptyCollection_ShouldReturnNull()
        {
            // Arrange
            EmptyRule<IEnumerable<string>> rule = new();
            IEnumerable<string> emptyCollection = Array.Empty<string>();

            // Act
            string? result = rule.Validate(emptyCollection, "Items");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNonEmptyCollection_ShouldReturnErrorMessage()
        {
            // Arrange
            EmptyRule<IEnumerable<string>> rule = new();
            IEnumerable<string> nonEmptyCollection = new[] { "Item1" };

            // Act
            string? result = rule.Validate(nonEmptyCollection, "Items");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldContain("must be empty");
        }

        [Fact]
        public void Validate_WithNullCollection_ShouldReturnNull()
        {
            // Arrange
            EmptyRule<IEnumerable<string>> rule = new();

            // Act
            string? result = rule.Validate(null!, "Items");

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion EmptyRule Collection Tests

    #region Edge Cases and Integration Tests

    /// <summary>
    /// Tests for edge cases and integration scenarios with collection rules
    /// </summary>
    public class CollectionEdgeCasesTests
    {
        [Fact]
        public void Validate_WithLargeCollections_ShouldHandleCorrectly()
        {
            // Arrange
            CountRule<int> rule = new(10000);
            IEnumerable<int> largeCollection = Enumerable.Range(1, 10000);

            // Act
            string? result = rule.Validate(largeCollection, "Numbers");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithLazyEvaluatedCollection_ShouldWork()
        {
            // Arrange
            MinCountRule<int> rule = new(3);
            IEnumerable<int> lazyCollection = GenerateNumbers().Take(5);

            // Act
            string? result = rule.Validate(lazyCollection, "Numbers");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithComplexObjectCollection_ShouldWork()
        {
            // Arrange
            CountRule<ComplexObject> rule = new(2);
            IEnumerable<ComplexObject> objects = new[]
            {
                new ComplexObject("A", new[] { 1, 2, 3 }),
                new ComplexObject("B", new[] { 4, 5, 6 })
            };

            // Act
            string? result = rule.Validate(objects, "Objects");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNestedCollections_ShouldWork()
        {
            // Arrange
            CountRule<IEnumerable<int>> rule = new(3);
            IEnumerable<IEnumerable<int>> nestedCollections = new[]
            {
                new[] { 1, 2 },
                new[] { 3, 4, 5 },
                new[] { 6 }
            };

            // Act
            string? result = rule.Validate(nestedCollections, "NestedLists");

            // Assert
            result.ShouldBeNull();
        }

        private static IEnumerable<int> GenerateNumbers()
        {
            for (int i = 1; i <= 10; i++)
            {
                yield return i;
            }
        }

        private record ComplexObject(string Name, int[] Values);
    }

    #endregion Edge Cases and Integration Tests
}