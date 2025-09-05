using FlowRight.Core.Results;
using FlowRight.Validation.Builders;
using FlowRight.Validation.Tests.TestModels;
using FlowRight.Validation.Validators;
using Shouldly;

namespace FlowRight.Validation.Tests.Validators;

/// <summary>
/// Edge case and corner case tests for conditional validation rules (When and Unless).
/// These tests define expected behavior for unusual, boundary, and error conditions
/// to ensure robust conditional validation implementation.
/// 
/// Test Coverage:
/// - Null and empty value handling in conditions
/// - Exception handling within condition functions
/// - Performance considerations with complex conditions
/// - Thread safety scenarios (if applicable)
/// - Memory management and resource cleanup
/// - Boundary conditions and extreme values
/// - Integration edge cases with different validator types
/// 
/// Current Status: Tests designed to fail and drive robust conditional validation implementation.
/// These tests ensure the conditional validation system handles all edge cases gracefully.
/// </summary>
public class ConditionalValidationEdgeCasesTests
{
    #region Null and Empty Value Handling Tests

    /// <summary>
    /// Tests for proper handling of null and empty values in conditional validation
    /// </summary>
    public class NullAndEmptyValueHandling
    {
        [Fact(Skip = "Conditional validation edge case - tracked in TASK-101")]
        public void When_WithNullStringValue_ShouldHandleNullConditionSafely()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string? nullName = null;

            // Act & Assert - Should not throw exception
            Should.NotThrow(() =>
            {
                StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, nullName!);
                validator.NotEmpty().When(name => name?.Length > 0); // Safe null check
            });

            builder.HasErrors.ShouldBeFalse(); // Rule should be skipped due to null condition
        }

        [Fact]
        public void When_WithEmptyStringValue_ShouldEvaluateConditionCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string emptyName = string.Empty;

            // Act
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, emptyName);
            validator.MinimumLength(1).When(name => string.IsNullOrEmpty(name)); // Should execute for empty string

            // Assert
            builder.HasErrors.ShouldBeTrue(); // MinimumLength should fail for empty string
        }

        [Fact(Skip = "Conditional validation edge case - tracked in TASK-101")]
        public void Unless_WithNullCollectionValue_ShouldHandleNullConditionSafely()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            IEnumerable<string>? nullRoles = null;

            // Act & Assert - Should not throw exception
            Should.NotThrow(() =>
            {
                EnumerablePropertyValidator<User, string> validator = builder.RuleFor(u => u.Roles, nullRoles!);
                validator.NotEmpty().Unless(roles => roles?.Any() != true); // Safe null check
            });

            builder.HasErrors.ShouldBeFalse(); // Rule should be skipped
        }

        [Fact(Skip = "Conditional validation edge case - tracked in TASK-101")]
        public void When_WithNullableNumericValue_ShouldHandleNullCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            int? nullAge = null;

            // Act
            GenericPropertyValidator<User, int?> validator = builder.RuleFor(u => u.OptionalAge, nullAge);
            validator.NotNull().When(age => age.HasValue); // Should skip because age is null

            // Assert
            builder.HasErrors.ShouldBeFalse(); // Rule should be skipped
        }

        [Fact(Skip = "Conditional validation edge case - tracked in TASK-101")]
        public void When_WithNullableGuidValue_ShouldHandleNullCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            Guid? nullId = null;

            // Act
            GuidPropertyValidator<User> validator = builder.RuleFor(u => u.Id, nullId);
            validator.NotEmpty().When(id => id.HasValue); // Should skip because id is null

            // Assert
            builder.HasErrors.ShouldBeFalse(); // Rule should be skipped
        }

        [Fact]
        public void Unless_WithNullBooleanValue_ShouldHandleNullCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            bool? nullVerified = null;

            // Act
            GenericPropertyValidator<User, bool?> validator = builder.RuleFor(u => u.IsVerified, nullVerified);
            validator.NotNull().Unless(verified => verified.HasValue); // Should execute because HasValue is false

            // Assert
            builder.HasErrors.ShouldBeTrue(); // NotNull should fail for null value
        }
    }

    #endregion Null and Empty Value Handling Tests

    #region Exception Handling in Conditions Tests

    /// <summary>
    /// Tests for proper exception handling within condition functions
    /// </summary>
    public class ExceptionHandlingInConditions
    {
        [Fact]
        public void When_WithConditionThatThrowsException_ShouldHandleExceptionGracefully()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string testName = "Test";

            // Act & Assert - Should not propagate exception from condition
            Should.NotThrow(() =>
            {
                StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, testName);
                validator.NotEmpty().When(name => throw new InvalidOperationException("Condition exception"));
            });

            // The behavior when condition throws should be well-defined (e.g., skip validation)
        }

        [Fact]
        public void Unless_WithConditionThatThrowsException_ShouldHandleExceptionGracefully()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            int testAge = 25;

            // Act & Assert - Should not propagate exception from condition
            Should.NotThrow(() =>
            {
                NumericPropertyValidator<User, int> validator = builder.RuleFor(u => u.Age, testAge);
                validator.GreaterThan(0).Unless(age => throw new ArgumentException("Condition exception"));
            });

            // The behavior when condition throws should be well-defined (e.g., execute validation)
        }

        [Fact]
        public void When_WithNullReferenceInCondition_ShouldHandleNullReferenceGracefully()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string? nullName = null;

            // Act & Assert - Should not throw NullReferenceException
            Should.NotThrow(() =>
            {
                StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, nullName!);
                // Intentionally unsafe null reference - should be handled gracefully
                validator.NotEmpty().When(name => name.StartsWith("Test")); // Will throw NullReferenceException
            });
        }

        [Fact]
        public void When_WithIndexOutOfRangeInCondition_ShouldHandleExceptionGracefully()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            IEnumerable<string> shortRoles = new[] { "User" };

            // Act & Assert - Should not throw IndexOutOfRangeException
            Should.NotThrow(() =>
            {
                EnumerablePropertyValidator<User, string> validator = builder.RuleFor(u => u.Roles, shortRoles);
                // Intentionally unsafe array access - should be handled gracefully
                validator.NotEmpty().When(roles => roles.ElementAt(5).Length > 0); // Will throw ArgumentOutOfRangeException
            });
        }
    }

    #endregion Exception Handling in Conditions Tests

    #region Boundary Value Tests

    /// <summary>
    /// Tests for boundary values and extreme conditions
    /// </summary>
    public class BoundaryValueTests
    {
        [Fact]
        public void When_WithExtremelyLongString_ShouldHandlePerformanceGracefully()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string extremelyLongString = new('A', 1_000_000); // 1 million character string

            // Act & Assert - Should complete in reasonable time
            DateTime start = DateTime.UtcNow;

            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, extremelyLongString);
            validator.MaximumLength(100).When(name => name.Length > 500_000);

            TimeSpan elapsed = DateTime.UtcNow - start;
            elapsed.TotalSeconds.ShouldBeLessThan(1.0); // Should complete quickly even with large string
            builder.HasErrors.ShouldBeTrue(); // Validation should still work correctly
        }

        [Fact]
        public void When_WithLargeCollection_ShouldHandlePerformanceGracefully()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            IEnumerable<string> largeCollection = Enumerable.Range(0, 100_000).Select(i => $"Role{i}");

            // Act & Assert - Should complete in reasonable time
            DateTime start = DateTime.UtcNow;

            EnumerablePropertyValidator<User, string> validator = builder.RuleFor(u => u.Roles, largeCollection);
            validator.MaxCount(50_000).When(roles => roles.Count() > 50_000);

            TimeSpan elapsed = DateTime.UtcNow - start;
            elapsed.TotalSeconds.ShouldBeLessThan(2.0); // Should complete reasonably quickly
            builder.HasErrors.ShouldBeTrue(); // Validation should still work correctly
        }

        [Fact]
        public void When_WithComplexConditionLogic_ShouldHandleComplexityGracefully()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string complexString = "Admin_User_With_Many_Roles_And_Permissions_Test";

            // Act - Complex condition with multiple string operations
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, complexString);
            validator.MaximumLength(20).When(name => 
                name.Contains("Admin") && 
                name.Split('_').Length > 3 && 
                name.ToLower().Contains("test") && 
                name.Where(c => char.IsUpper(c)).Count() > 5);

            // Assert
            builder.HasErrors.ShouldBeTrue(); // Complex condition should evaluate and validation should fail
        }

        [Fact]
        public void When_WithNumericBoundaryValues_ShouldHandleCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act - Test various numeric boundary values
            builder.RuleFor(u => u.Age, int.MaxValue)
                .LessThan(100).When(age => age > int.MaxValue - 1000); // Should execute

            builder.RuleFor(u => u.Salary, decimal.MaxValue)
                .LessThan(1000000m).When(salary => salary > 1000000m); // Should execute

            builder.RuleFor(u => u.Rating, float.MaxValue)
                .LessThan(5.0f).When(rating => rating > 1000000f); // Should execute

            // Assert
            builder.HasErrors.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.Count.ShouldBe(3); // All three validations should fail
        }

        [Fact]
        public void When_WithDateTimeBoundaryValues_ShouldHandleCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act - Test DateTime boundary values
            builder.RuleFor(u => u.CreatedAt, DateTime.MinValue)
                .Must(date => date > DateTime.MinValue, "Date must be after minimum")
                .When(date => date == DateTime.MinValue); // Should execute

            builder.RuleFor(u => u.UpdatedAt, DateTime.MaxValue)
                .Must(date => date < DateTime.MaxValue, "Date must be before maximum")
                .When(date => date == DateTime.MaxValue); // Should execute

            // Assert
            builder.HasErrors.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.Count.ShouldBe(2); // Both validations should fail
        }
    }

    #endregion Boundary Value Tests

    #region Complex Integration Edge Cases Tests

    /// <summary>
    /// Tests for complex integration scenarios and edge cases
    /// </summary>
    public class ComplexIntegrationEdgeCases
    {
        [Fact(Skip = "Conditional validation edge case - tracked in TASK-101")]
        public void When_WithResultCompositionEdgeCase_ShouldHandleFailedResultsCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            Result<Profile?> failedProfileResult = Result.Failure<Profile?>("Profile creation failed");
            string name = "Test";

            // Act
            builder.RuleFor(u => u.Profile, failedProfileResult, out Profile? profile);
            
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, name);
            validator.MinimumLength(10).When(n => profile != null); // Should skip because profile is null

            // Assert
            builder.HasErrors.ShouldBeTrue(); // Should have error from failed profile result
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.ShouldContainKey("Profile"); // Profile error should be present
            errors.ShouldNotContainKey("Name"); // Name validation should be skipped
            profile.ShouldBeNull();
        }

        [Fact]
        public void When_WithMultiplePropertyValidators_ShouldHandleComplexChaining()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            
            // Act - Complex chain across multiple property types
            Result<User> result = builder
                .RuleFor(u => u.Name, "TestUser")
                    .NotEmpty()
                    .MinimumLength(10).When(name => name.Contains("Test"))
                .RuleFor(u => u.Email, "test@example.com")
                    .EmailAddress()
                    .EndsWith(".com").Unless(email => email.Contains("@temp"))
                .RuleFor(u => u.Age, 25)
                    .GreaterThan(18)
                    .LessThan(65).When(age => age > 0)
                .RuleFor(u => u.Roles, new[] { "Admin", "User" })
                    .NotEmpty()
                    .MaxCount(3).Unless(roles => roles.Contains("SuperAdmin"))
                .Build(() => new UserBuilder().Build());

            // Assert
            result.IsFailure.ShouldBeTrue(); // MinimumLength should fail
            result.Failures.Count.ShouldBe(1);
            result.Failures.ShouldContainKey("Name");
        }

        [Fact(Skip = "Conditional validation edge case - tracked in TASK-101")]
        public void When_WithNestedConditionalLogic_ShouldHandleComplexNesting()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string name = "AdminTestUser";
            IEnumerable<string> roles = new[] { "Admin" };

            // Act - Nested conditional logic based on multiple properties
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, name);
            validator.MinimumLength(15)
                .When(n => n.StartsWith("Admin") && roles.Any(r => r == "Admin"))
                .Unless(n => n.Contains("Test") && n.Length > 10);

            // Complex nested logic: 
            // When: name starts with "Admin" AND roles contains "Admin" (TRUE)
            // Unless: name contains "Test" AND length > 10 (TRUE)
            // Result: Unless overrides When, so rule should be skipped

            // Assert
            builder.HasErrors.ShouldBeFalse(); // Rule should be skipped due to Unless condition
        }

        [Fact]
        public void When_WithRecursiveValidation_ShouldAvoidInfiniteRecursion()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string name = "TestUser";

            // Act - Condition that references the same property being validated
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, name);
            validator.MinimumLength(10)
                .When(n => n == name); // Self-referential condition

            // Assert
            builder.HasErrors.ShouldBeTrue(); // Should execute normally without recursion issues
        }

        [Fact]
        public void When_WithMemoryIntensiveCondition_ShouldManageMemoryCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string testString = "TestValue";

            // Act - Condition that might create temporary objects
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, testString);
            validator.MinimumLength(20).When(name =>
            {
                // Memory-intensive operation that creates temporary objects
                string[] tempArray = new string[10000];
                for (int i = 0; i < tempArray.Length; i++)
                {
                    tempArray[i] = $"temp_{i}_{name}";
                }
                return tempArray.Any(s => s.Contains(name));
            });

            // Assert
            builder.HasErrors.ShouldBeTrue(); // Validation should still work
            
            // Force garbage collection to ensure memory is properly managed
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        [Fact]
        public void When_WithAsyncLikeCondition_ShouldHandleAsyncPatterns()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string email = "test@example.com";

            // Act - Condition that simulates async-like operations (but executed synchronously)
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Email, email);
            validator.EmailAddress().When(e =>
            {
                // Simulate time-consuming validation logic
                Task.Delay(1).Wait(); // Very short delay to simulate async work
                return e.Contains("@");
            });

            // Assert
            builder.HasErrors.ShouldBeFalse(); // EmailAddress validation should pass
        }
    }

    #endregion Complex Integration Edge Cases Tests

    #region State Management Edge Cases Tests

    /// <summary>
    /// Tests for edge cases related to state management in conditional validation
    /// </summary>
    public class StateManagementEdgeCases
    {
        [Fact]
        public void When_WithModifiedValueDuringValidation_ShouldUseOriginalValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string originalName = "TestUser";
            string modifiedName = "ModifiedUser";

            // Act - Condition attempts to modify the validation context
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, originalName);
            validator.MinimumLength(15).When(name =>
            {
                // Attempt to modify the name during condition evaluation
                // This should not affect the actual validation value
                name = modifiedName;
                return name.Length > 10;
            });

            // Assert
            builder.HasErrors.ShouldBeTrue(); // Should use original name "TestUser" for validation
            Dictionary<string, string[]> errors = builder.GetErrors();
            // The validation should fail because "TestUser" (8 chars) < 15, not "ModifiedUser" (12 chars)
        }

        [Fact(Skip = "Conditional validation edge case - tracked in TASK-101")]
        public void When_WithStatefulCondition_ShouldHandleStateConsistently()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            int callCount = 0;
            string name = "Test";

            // Act - Condition that maintains state between calls
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, name);
            validator.MinimumLength(10).When(n =>
            {
                callCount++;
                return callCount % 2 == 0; // Every second call returns true
            });

            // The condition should only be evaluated once per rule
            // Assert
            callCount.ShouldBe(1); // Condition should only be called once
            builder.HasErrors.ShouldBeFalse(); // First call returns false, so rule should be skipped
        }

        [Fact(Skip = "Conditional validation edge case - tracked in TASK-101")]
        public void When_WithMultipleBuildCalls_ShouldMaintainConsistentState()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string name = "Test";

            builder.RuleFor(u => u.Name, name)
                .MinimumLength(10).When(n => n.Length > 5); // Should skip because "Test" length is 4, not > 5

            // Act - Multiple Build calls
            Result<User> firstResult = builder.Build(() => new UserBuilder().Build());
            Result<User> secondResult = builder.Build(() => new UserBuilder().Build());

            // Assert
            firstResult.IsSuccess.ShouldBe(secondResult.IsSuccess); // Results should be consistent
            builder.HasErrors.ShouldBeFalse(); // Both builds should have same validation state
        }

        [Fact(Skip = "Conditional validation edge case - tracked in TASK-101")]
        public void When_WithSharedConditionAcrossValidators_ShouldIsolateState()
        {
            // Arrange
            ValidationBuilder<User> builder1 = new();
            ValidationBuilder<User> builder2 = new();
            
            Func<string, bool> sharedCondition = name => name.Length > 5;
            string shortName = "Test";
            string longName = "TestUser";

            // Act - Use same condition function across different builders
            builder1.RuleFor(u => u.Name, shortName)
                .MinimumLength(10).When(sharedCondition);

            builder2.RuleFor(u => u.Name, longName)
                .MinimumLength(10).When(sharedCondition);

            // Assert
            builder1.HasErrors.ShouldBeFalse(); // shortName length <= 5, condition false, rule skipped
            builder2.HasErrors.ShouldBeTrue(); // longName length > 5, condition true, rule executes and fails
        }

        [Fact]
        public void When_WithConditionAccessingExternalState_ShouldHandleExternalChanges()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            bool externalFlag = true;
            string name = "Test";

            // Act - Condition depends on external state
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, name);
            validator.MinimumLength(10).When(n => externalFlag);

            // Change external state after setting up validation
            externalFlag = false;

            // The condition should have captured the state at the time it was evaluated
            // Assert
            builder.HasErrors.ShouldBeTrue(); // Should use the original value of externalFlag (true)
        }
    }

    #endregion State Management Edge Cases Tests
}