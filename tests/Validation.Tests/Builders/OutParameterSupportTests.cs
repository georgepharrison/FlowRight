using FlowRight.Core.Results;
using FlowRight.Validation.Builders;
using FlowRight.Validation.Tests.TestModels;
using Shouldly;

namespace FlowRight.Validation.Tests.Builders;

/// <summary>
/// Test suite for TASK-038: Out parameter support for value extraction in ValidationBuilder&lt;T&gt;.
/// These tests define the expected behavior for extracting validated values through out parameters
/// on all RuleFor method overloads, enabling cleaner object construction patterns.
/// 
/// Following TDD principles - these tests will initially fail until implementation is complete.
/// </summary>
public class OutParameterSupportTests
{
    #region String Property Out Parameter Tests
    
    /// <summary>
    /// Tests for string property validation with out parameter support
    /// </summary>
    public class StringPropertyOutParameters
    {
        [Fact]
        public void RuleFor_StringProperty_WithOutParameter_ShouldProvideValidatedValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string inputName = "John Doe";

            // Act
            builder.RuleFor(u => u.Name, inputName, out string? validatedName);

            Result<User> result = builder.Build(() => new User(validatedName!, "john@example.com", 25, Guid.NewGuid(), []));

            // Assert
            result.IsSuccess.ShouldBeTrue();
            validatedName.ShouldBe(inputName);
            result.TryGetValue(out User? user).ShouldBeTrue();
            user.Name.ShouldBe(inputName);
        }

        [Fact]
        public void RuleFor_StringProperty_WithValidationFailure_ShouldProvideNullValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string inputName = "";

            // Act
            builder.RuleFor(u => u.Name, inputName, out string? validatedName);

            Result<User> result = builder.Build(() => new User(validatedName ?? "Default", "john@example.com", 25, Guid.NewGuid(), []));

            // Assert
            result.IsFailure.ShouldBeTrue();
            validatedName.ShouldBeNull();
        }
    }

    #endregion

    #region Numeric Property Out Parameter Tests
    
    /// <summary>
    /// Tests for numeric property validation with out parameter support
    /// </summary>
    public class NumericPropertyOutParameters
    {
        [Fact]
        public void RuleFor_IntProperty_WithOutParameter_ShouldProvideValidatedValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            int inputAge = 30;

            // Act
            builder.RuleFor(u => u.Age, inputAge, out int? validatedAge);

            Result<User> result = builder.Build(() => new User("John", "john@example.com", validatedAge!.Value, Guid.NewGuid(), []));

            // Assert
            result.IsSuccess.ShouldBeTrue();
            validatedAge.ShouldBe(inputAge);
            result.TryGetValue(out User? user1).ShouldBeTrue();
            user1.Age.ShouldBe(inputAge);
        }

        [Fact]
        public void RuleFor_IntProperty_WithValidationFailure_ShouldProvideNullValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            int inputAge = -5; // Invalid: less than 0

            // Act
            builder.RuleFor(u => u.Age, inputAge, out int? validatedAge);

            Result<User> result = builder.Build(() => new User("John", "john@example.com", validatedAge ?? 0, Guid.NewGuid(), []));

            // Assert
            result.IsFailure.ShouldBeTrue();
            validatedAge.ShouldBeNull();
        }

        [Fact]
        public void RuleFor_DecimalProperty_WithOutParameter_ShouldProvideValidatedValue()
        {
            // Arrange
            ValidationBuilder<TestEntity> builder = new();
            decimal inputValue = 123.45m;

            // Act
            builder.RuleFor(e => e.DecimalValue, inputValue, out decimal? validatedValue);

            Result<TestEntity> result = builder.Build(() => new TestEntity { DecimalValue = validatedValue!.Value });

            // Assert
            result.IsSuccess.ShouldBeTrue();
            validatedValue.ShouldBe(inputValue);
            result.TryGetValue(out TestEntity? entity1).ShouldBeTrue();
            entity1.DecimalValue.ShouldBe(inputValue);
        }
    }

    #endregion

    #region Generic Property Out Parameter Tests
    
    /// <summary>
    /// Tests for generic property validation with out parameter support
    /// </summary>
    public class GenericPropertyOutParameters
    {
        [Fact]
        public void RuleFor_BoolProperty_WithOutParameter_ShouldProvideValidatedValue()
        {
            // Arrange
            ValidationBuilder<TestEntity> builder = new();
            bool inputValue = true;

            // Act
            builder.RuleFor(e => e.IsActive, inputValue, out bool? validatedValue);

            Result<TestEntity> result = builder.Build(() => new TestEntity { IsActive = validatedValue!.Value });

            // Assert
            result.IsSuccess.ShouldBeTrue();
            validatedValue.ShouldBe(inputValue);
            result.TryGetValue(out TestEntity? entity2).ShouldBeTrue();
            entity2.IsActive.ShouldBe(inputValue);
        }

        [Fact]
        public void RuleFor_DateTimeProperty_WithOutParameter_ShouldProvideValidatedValue()
        {
            // Arrange
            ValidationBuilder<TestEntity> builder = new();
            DateTime inputValue = new(2024, 1, 1);

            // Act
            builder.RuleFor(e => e.CreatedAt, inputValue, out DateTime? validatedValue);

            Result<TestEntity> result = builder.Build(() => new TestEntity { CreatedAt = validatedValue!.Value });

            // Assert
            result.IsSuccess.ShouldBeTrue();
            validatedValue.ShouldBe(inputValue);
            result.TryGetValue(out TestEntity? entity3).ShouldBeTrue();
            entity3.CreatedAt.ShouldBe(inputValue);
        }
    }

    #endregion

    #region Enumerable Property Out Parameter Tests
    
    /// <summary>
    /// Tests for enumerable property validation with out parameter support
    /// </summary>
    public class EnumerablePropertyOutParameters
    {
        [Fact]
        public void RuleFor_EnumerableProperty_WithOutParameter_ShouldProvideValidatedValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            IEnumerable<string> inputRoles = ["Admin", "User"];

            // Act
            builder.RuleFor(u => u.Roles, inputRoles, out IEnumerable<string>? validatedRoles);

            Result<User> result = builder.Build(() => new User("John", "john@example.com", 25, Guid.NewGuid(), validatedRoles!));

            // Assert
            result.IsSuccess.ShouldBeTrue();
            validatedRoles.ShouldNotBeNull();
            validatedRoles.ShouldBe(inputRoles);
            result.TryGetValue(out User? user2).ShouldBeTrue();
            user2.Roles.ShouldBe(inputRoles);
        }
    }

    #endregion

    #region Guid Property Out Parameter Tests
    
    /// <summary>
    /// Tests for Guid property validation with out parameter support
    /// </summary>
    public class GuidPropertyOutParameters
    {
        [Fact]
        public void RuleFor_GuidProperty_WithOutParameter_ShouldProvideValidatedValue()
        {
            // Arrange
            ValidationBuilder<TestEntity> builder = new();
            Guid inputId = Guid.NewGuid();

            // Act
            builder.RuleFor(e => e.Id, inputId, out Guid? validatedId);

            Result<TestEntity> result = builder.Build(() => new TestEntity { Id = validatedId!.Value });

            // Assert
            result.IsSuccess.ShouldBeTrue();
            validatedId.ShouldBe(inputId);
            result.TryGetValue(out TestEntity? entity4).ShouldBeTrue();
            entity4.Id.ShouldBe(inputId);
        }
    }

    #endregion

    #region Complex Composition Tests
    
    /// <summary>
    /// Tests for complex validation composition with multiple out parameters
    /// </summary>
    public class ComplexCompositionTests
    {
        [Fact]
        public void RuleFor_MultipleOutParameters_ShouldProvideAllValidatedValues()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string inputName = "John Doe";
            int inputAge = 30;
            string inputEmail = "john@example.com";

            // Act
            builder.RuleFor(u => u.Name, inputName, out string? validatedName);
            builder.RuleFor(u => u.Age, inputAge, out int? validatedAge);
            builder.RuleFor(u => u.Email, inputEmail, out string? validatedEmail);

            Result<User> result = builder.Build(() => new User(validatedName!, validatedEmail!, validatedAge!.Value, Guid.NewGuid(), []));

            // Assert
            result.IsSuccess.ShouldBeTrue();
            validatedName.ShouldBe(inputName);
            validatedAge.ShouldBe(inputAge);
            validatedEmail.ShouldBe(inputEmail);
            
            result.TryGetValue(out User? user3).ShouldBeTrue();
            user3.Name.ShouldBe(inputName);
            user3.Age.ShouldBe(inputAge);
            user3.Email.ShouldBe(inputEmail);
        }

        [Fact]
        public void RuleFor_MixedSuccessAndFailure_ShouldProvideCorrectValues()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string inputName = "John"; // Valid
            int inputAge = -5;         // Invalid
            string inputEmail = "john@example.com"; // Valid

            // Act
            builder.RuleFor(u => u.Name, inputName, out string? validatedName);
            builder.RuleFor(u => u.Age, inputAge, out int? validatedAge);
            builder.RuleFor(u => u.Email, inputEmail, out string? validatedEmail);

            Result<User> result = builder.Build(() => new User(
                validatedName ?? "Default", 
                validatedEmail ?? "default@example.com",
                validatedAge ?? 0,
                Guid.NewGuid(),
                []));

            // Assert
            result.IsFailure.ShouldBeTrue();
            
            // Valid values should be provided
            validatedName.ShouldBe(inputName);
            validatedEmail.ShouldBe(inputEmail);
            
            // Invalid values should be null
            validatedAge.ShouldBeNull();
        }
    }

    #endregion
}

/// <summary>
/// Test entity for validation testing
/// </summary>
public class TestEntity
{
    public Guid? Id { get; set; }
    public decimal DecimalValue { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}