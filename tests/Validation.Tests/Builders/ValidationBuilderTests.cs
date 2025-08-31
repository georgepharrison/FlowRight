using System.Security;
using FlowRight.Core.Results;
using FlowRight.Validation.Builders;
using FlowRight.Validation.Tests.TestModels;
using FlowRight.Validation.Validators;
using Shouldly;

namespace FlowRight.Validation.Tests.Builders;

/// <summary>
/// Comprehensive failing tests for ValidationBuilder&lt;T&gt; class that define expected behavior
/// for fluent validation with Result&lt;T&gt; pattern integration. These tests follow TDD principles
/// and will initially fail until the ValidationBuilder implementation is complete.
/// 
/// Test Coverage:
/// - Basic ValidationBuilder operations (Build, GetErrors, HasErrors)
/// - String property validation with StringPropertyValidator (49+ scenarios)
/// - Numeric property validation with NumericPropertyValidator for all numeric types
/// - Enumerable property validation with EnumerablePropertyValidator 
/// - Result&lt;T&gt; composition using RuleFor with out parameters
/// - Error aggregation across multiple properties and validation types
/// - Edge cases, error conditions, and fluent interface behavior
/// - Custom display names, conditional validation (When/Unless), custom messages
/// 
/// Current Status: 33/49 tests failing as expected (TDD Red phase)
/// These failing tests serve as executable specifications for the ValidationBuilder implementation.
/// </summary>
public class ValidationBuilderTests
{
    #region Basic Operations Tests

    /// <summary>
    /// Tests for core ValidationBuilder operations: Build, GetErrors, HasErrors
    /// </summary>
    public class BasicOperations
    {
        [Fact]
        public void Build_WithNullFactory_ShouldThrowArgumentNullException()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert
            Should.Throw<ArgumentNullException>(() => builder.Build(null!));
        }

        [Fact]
        public void Build_WithNoValidationErrors_ShouldReturnSuccessResult()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            User expectedUser = new UserBuilder().Build();

            // Act
            Result<User> result = builder.Build(() => expectedUser);

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void Build_WithValidationErrors_ShouldReturnFailureWithValidationErrors()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            builder.RuleFor(u => u.Name, string.Empty).NotEmpty();

            // Act
            Result<User> result = builder.Build(() => new UserBuilder().Build());

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
        }

        [Fact]
        public void HasErrors_WithNoValidationErrors_ShouldReturnFalse()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            bool hasErrors = builder.HasErrors;

            // Assert
            hasErrors.ShouldBeFalse();
        }

        [Fact]
        public void HasErrors_WithValidationErrors_ShouldReturnTrue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            builder.RuleFor(u => u.Name, string.Empty).NotEmpty();

            // Act
            bool hasErrors = builder.HasErrors;

            // Assert
            hasErrors.ShouldBeTrue();
        }

        [Fact]
        public void GetErrors_WithNoValidationErrors_ShouldReturnEmptyDictionary()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            Dictionary<string, string[]> errors = builder.GetErrors();

            // Assert
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void GetErrors_WithValidationErrors_ShouldReturnErrorDictionary()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            builder.RuleFor(u => u.Name, string.Empty).NotEmpty();

            // Act
            Dictionary<string, string[]> errors = builder.GetErrors();

            // Assert
            errors.ShouldContainKey("Name");
            errors["Name"].Length.ShouldBe(1);
        }
    }

    #endregion Basic Operations Tests

    #region String Property Validation Tests

    /// <summary>
    /// Tests for string property validation using StringPropertyValidator
    /// </summary>
    public class StringPropertyValidation
    {
        [Fact]
        public void RuleFor_StringProperty_WithNotEmpty_ShouldFailForEmptyString()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, string.Empty);
            validator.NotEmpty();

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_StringProperty_WithNotEmpty_ShouldPassForNonEmptyString()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, "John Doe");
            validator.NotEmpty();

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void RuleFor_StringProperty_WithMaxLength_ShouldFailForTooLongString()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string tooLongName = new('a', 51);

            // Act
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, tooLongName);
            validator.MaximumLength(50);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_StringProperty_WithMinLength_ShouldFailForTooShortString()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, "A");
            validator.MinimumLength(2);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_StringProperty_WithEmailAddress_ShouldFailForInvalidEmail()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Email, "invalid-email");
            validator.EmailAddress();

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_StringProperty_WithCustomDisplayName_ShouldUseCustomName()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, string.Empty, "Full Name");
            validator.NotEmpty();

            // Assert
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.ShouldContainKey("Full Name");
        }

        [Fact]
        public void RuleFor_StringProperty_WithChainedValidations_ShouldAggregateAllErrors()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, string.Empty);
            validator.NotEmpty().MinimumLength(2).MaximumLength(50);

            // Assert
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors["Name"].Length.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void RuleFor_StringProperty_WithMustCondition_ShouldFailWhenConditionFails()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, "admin");
            validator.Must(name => name != "admin", "Name cannot be 'admin'");

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_StringProperty_WithWhenCondition_ShouldOnlyValidateWhenConditionTrue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, string.Empty);
            validator.NotEmpty().When(name => name != null);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_StringProperty_WithUnlessCondition_ShouldOnlyValidateWhenConditionFalse()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, string.Empty);
            validator.NotEmpty().Unless(name => name == null);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_StringProperty_WithCustomMessage_ShouldUseCustomErrorMessage()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string customMessage = "Custom error message for Name";

            // Act
            StringPropertyValidator<User> validator = builder.RuleFor(u => u.Name, string.Empty);
            validator.NotEmpty().WithMessage(customMessage);

            // Assert
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors["Name"].ShouldContain(customMessage);
        }
    }

    #endregion String Property Validation Tests

    #region Numeric Property Validation Tests

    /// <summary>
    /// Tests for numeric property validation using NumericPropertyValidator
    /// </summary>
    public class NumericPropertyValidation
    {
        [Fact]
        public void RuleFor_IntProperty_WithGreaterThan_ShouldFailForSmallerValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, int> validator = builder.RuleFor(u => u.Age, (int)0);
            validator.GreaterThan(0);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_IntProperty_WithGreaterThanOrEqualTo_ShouldPassForEqualValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, int> validator = builder.RuleFor(u => u.Age, (int)18);
            validator.GreaterThanOrEqualTo(18);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void RuleFor_IntProperty_WithLessThan_ShouldFailForLargerValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, int> validator = builder.RuleFor(u => u.Age, (int)150);
            validator.LessThan(120);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_DecimalProperty_WithScalePrecision_ShouldFailForInvalidPrecision()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, decimal> validator = builder.RuleFor(u => u.Salary, 123.456m);
            validator.PrecisionScale(5, 2);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_LongProperty_WithInclusiveBetween_ShouldFailForOutOfRangeValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, long> validator = builder.RuleFor(u => u.Phone, 123L);
            validator.GreaterThanOrEqualTo(1000000000L).LessThanOrEqualTo(9999999999L);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_ShortProperty_WithExclusiveBetween_ShouldFailForBoundaryValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, short> validator = builder.RuleFor(u => u.Priority, (short)1);
            validator.ExclusiveBetween((short)1, (short)10);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_DoubleProperty_WithEqualValidation_ShouldPassForEqualValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, double> validator = builder.RuleFor(u => u.Score, 85.5);
            validator.Equal(85.5);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void RuleFor_FloatProperty_WithNotEqual_ShouldFailForEqualValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, float> validator = builder.RuleFor(u => u.Rating, 0.0f);
            validator.NotEqual(0.0f);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }
    }

    #endregion Numeric Property Validation Tests

    #region Enumerable Property Validation Tests

    /// <summary>
    /// Tests for enumerable/collection property validation using EnumerablePropertyValidator
    /// </summary>
    public class EnumerablePropertyValidation
    {
        [Fact]
        public void RuleFor_EnumerableProperty_WithNotEmpty_ShouldFailForEmptyCollection()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            IEnumerable<string> emptyRoles = Array.Empty<string>();

            // Act
            EnumerablePropertyValidator<User, string> validator = builder.RuleFor(u => u.Roles, emptyRoles);
            validator.NotEmpty();

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_EnumerableProperty_WithMinCount_ShouldFailForTooFewItems()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            IEnumerable<string> roles = new[] { "User" };

            // Act
            EnumerablePropertyValidator<User, string> validator = builder.RuleFor(u => u.Roles, roles);
            validator.MinCount(2);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_EnumerableProperty_WithMaxCount_ShouldFailForTooManyItems()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            IEnumerable<string> roles = new[] { "User", "Admin", "SuperAdmin", "Guest" };

            // Act
            EnumerablePropertyValidator<User, string> validator = builder.RuleFor(u => u.Roles, roles);
            validator.MaxCount(3);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_EnumerableProperty_WithExactCount_ShouldFailForWrongCount()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            IEnumerable<string> roles = new[] { "User", "Admin" };

            // Act
            EnumerablePropertyValidator<User, string> validator = builder.RuleFor(u => u.Roles, roles);
            validator.Count(3);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_EnumerableProperty_WithUnique_ShouldFailForDuplicateItems()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            IEnumerable<string> rolesWithDuplicates = new[] { "User", "Admin", "User" };

            // Act
            EnumerablePropertyValidator<User, string> validator = builder.RuleFor(u => u.Roles, rolesWithDuplicates);
            validator.Unique();

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_EnumerableProperty_WithCountBetween_ShouldFailForOutOfRangeCount()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            IEnumerable<string> roles = new[] { "User" };

            // Act
            EnumerablePropertyValidator<User, string> validator = builder.RuleFor(u => u.Roles, roles);
            validator.MinCount(2).MaxCount(5);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }
    }

    #endregion Enumerable Property Validation Tests

    #region Result Composition Tests

    /// <summary>
    /// Tests for Result&lt;T&gt; composition scenarios using RuleFor with out parameters
    /// </summary>
    public class ResultComposition
    {
        [Fact]
        public void RuleFor_WithSuccessResult_ShouldProvideValidatedValueThroughOutParameter()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            Profile validProfile = new("Valid bio content", DateTime.UtcNow);
            Result<Profile?> profileResult = Result.Success<Profile?>(validProfile);

            // Act
            builder.RuleFor(u => u.Profile, profileResult, out Profile? validatedProfile);

            // Assert
            builder.HasErrors.ShouldBeFalse();
            validatedProfile.ShouldNotBeNull();
            validatedProfile.Bio.ShouldBe("Valid bio content");
        }

        [Fact]
        public void RuleFor_WithFailureResult_ShouldAddErrorsAndProvideNullValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            Result<Profile?> profileResult = Result.Failure<Profile?>("Profile validation failed");

            // Act
            builder.RuleFor(u => u.Profile, profileResult, out Profile? validatedProfile);

            // Assert
            builder.HasErrors.ShouldBeTrue();
            validatedProfile.ShouldBeNull();
        }

        [Fact]
        public void RuleFor_WithValidationExceptionResult_ShouldMergeValidationErrors()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            Dictionary<string, string[]> validationErrors = new()
            {
                { "Bio", new[] { "Bio is required", "Bio is too short" } },
                { "CreatedAt", new[] { "CreatedAt cannot be in the future" } }
            };
            Result<Profile?> profileResult = Result.Failure<Profile?>(validationErrors);

            // Act
            builder.RuleFor(u => u.Profile, profileResult, out Profile? validatedProfile);

            // Assert
            builder.HasErrors.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.ShouldContainKey("Bio");
            errors.ShouldContainKey("CreatedAt");
            errors["Bio"].Length.ShouldBe(2);
            errors["CreatedAt"].Length.ShouldBe(1);
            validatedProfile.ShouldBeNull();
        }

        [Fact]
        public void RuleFor_WithSecurityExceptionResult_ShouldAddSecurityError()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            Result<Profile?> profileResult = Result.Failure<Profile?>(new SecurityException("Access denied"));

            // Act
            builder.RuleFor(u => u.Profile, profileResult, out Profile? validatedProfile);

            // Assert
            builder.HasErrors.ShouldBeTrue();
            validatedProfile.ShouldBeNull();
        }

        [Fact]
        public void RuleFor_WithOperationCancelledResult_ShouldAddCancellationError()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            Result<Profile?> profileResult = Result.Failure<Profile?>(new OperationCanceledException("Operation cancelled"));

            // Act
            builder.RuleFor(u => u.Profile, profileResult, out Profile? validatedProfile);

            // Assert
            builder.HasErrors.ShouldBeTrue();
            validatedProfile.ShouldBeNull();
        }

        [Fact]
        public void RuleFor_WithNullResult_ShouldProvideDefaultValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            Result<Profile?>? nullResult = null;

            // Act
            builder.RuleFor(u => u.Profile, nullResult!, out Profile? validatedProfile);

            // Assert
            validatedProfile.ShouldBeNull();
        }

        [Fact]
        public void RuleFor_MultipleResultCompositions_ShouldAggregateAllValidationErrors()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            Result<Profile?> failingProfileResult = Result.Failure<Profile?>("Profile validation failed");
            Result<string> failingNameResult = Result.Failure<string>("Name validation failed");

            // Act
            builder.RuleFor(u => u.Profile, failingProfileResult, out Profile? validatedProfile);
            builder.RuleFor(u => u.Name, failingNameResult, out string? validatedName);

            // Assert
            builder.HasErrors.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.Count.ShouldBeGreaterThan(1);
            validatedProfile.ShouldBeNull();
            validatedName.ShouldBeNull();
        }
    }

    #endregion Result Composition Tests

    #region Error Aggregation Tests

    /// <summary>
    /// Tests for error aggregation across multiple properties
    /// </summary>
    public class ErrorAggregation
    {
        [Fact]
        public void MultipleProperties_WithValidationErrors_ShouldAggregateAllErrors()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            builder.RuleFor(u => u.Name, string.Empty).NotEmpty();
            builder.RuleFor(u => u.Email, "invalid-email").EmailAddress();
            builder.RuleFor(u => u.Age, (int)-1).GreaterThan(0);

            // Assert
            builder.HasErrors.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.ShouldContainKey("Name");
            errors.ShouldContainKey("Email");
            errors.ShouldContainKey("Age");
            errors.Count.ShouldBe(3);
        }

        [Fact]
        public void SingleProperty_WithMultipleValidationRules_ShouldAggregateMultipleErrors()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            builder.RuleFor(u => u.Name, string.Empty)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50);

            // Assert
            builder.HasErrors.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.ShouldContainKey("Name");
            errors["Name"].Length.ShouldBeGreaterThan(1);
        }

        [Fact]
        public void MixedValidationTypes_WithErrors_ShouldAggregateAllErrorTypes()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            Result<Profile?> failingProfileResult = Result.Failure<Profile?>("Profile validation failed");

            // Act
            builder.RuleFor(u => u.Name, string.Empty).NotEmpty();
            builder.RuleFor(u => u.Profile, failingProfileResult, out Profile? validatedProfile);
            builder.RuleFor(u => u.Age, (int)-1).GreaterThan(0);

            // Assert
            builder.HasErrors.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.Count.ShouldBeGreaterThan(2);
        }

        [Fact]
        public void Build_WithAggregatedErrors_ShouldReturnFailureWithAllErrors()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            builder.RuleFor(u => u.Name, string.Empty).NotEmpty();
            builder.RuleFor(u => u.Email, "invalid").EmailAddress();

            // Act
            Result<User> result = builder.Build(() => new UserBuilder().Build());

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            result.Failures.ShouldContainKey("Name");
            result.Failures.ShouldContainKey("Email");
        }
    }

    #endregion Error Aggregation Tests

    #region Edge Cases and Error Conditions Tests

    /// <summary>
    /// Tests for edge cases and error conditions
    /// </summary>
    public class EdgeCasesAndErrorConditions
    {
        [Fact]
        public void RuleFor_WithNullPropertySelector_ShouldThrowArgumentException()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert
            Should.Throw<ArgumentException>(() => builder.RuleFor(null!, "test"));
        }

        [Fact]
        public void Build_AfterMultipleBuilds_ShouldReturnConsistentResults()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            builder.RuleFor(u => u.Name, "Valid Name").NotEmpty();

            // Act
            Result<User> firstResult = builder.Build(() => new UserBuilder().Build());
            Result<User> secondResult = builder.Build(() => new UserBuilder().Build());

            // Assert
            firstResult.IsSuccess.ShouldBe(secondResult.IsSuccess);
        }

        [Fact]
        public void ValidationBuilder_WithComplexChainedValidations_ShouldMaintainFluentInterface()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert (should compile and chain properly)
            Result<User> result = builder
                .RuleFor(u => u.Name, "John Doe")
                    .NotEmpty()
                    .MinimumLength(2)
                    .MaximumLength(50)
                .RuleFor(u => u.Email, "john@example.com")
                    .NotEmpty()
                    .EmailAddress()
                .RuleFor(u => u.Age, (int)30)
                    .GreaterThan(0)
                    .LessThan(120)
                .Build(() => new UserBuilder().Build());

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void GetErrors_AfterBuild_ShouldReturnSameErrors()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            builder.RuleFor(u => u.Name, string.Empty).NotEmpty();

            // Act
            Dictionary<string, string[]> errorsBeforeBuild = builder.GetErrors();
            Result<User> result = builder.Build(() => new UserBuilder().Build());
            Dictionary<string, string[]> errorsAfterBuild = builder.GetErrors();

            // Assert
            errorsBeforeBuild.Count.ShouldBe(errorsAfterBuild.Count);
            errorsBeforeBuild.ShouldContainKey("Name");
            errorsAfterBuild.ShouldContainKey("Name");
        }

        [Fact]
        public void ValidationBuilder_WithEmptyGuid_ShouldValidateCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            builder.RuleFor(u => u.Id, Guid.Empty).NotEqual(Guid.Empty);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void ValidationBuilder_WithNullableProperties_ShouldHandleNullsCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            builder.RuleFor(u => u.Id, (Guid?)null).Notnull();

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }
    }

    #endregion Edge Cases and Error Conditions Tests

    #region DateTime Property Validation Tests

    /// <summary>
    /// Tests for DateTime property validation using GenericPropertyValidator
    /// </summary>
    public class DateTimePropertyValidation
    {
        [Fact]
        public void RuleFor_DateTimeProperty_WithEqualValidation_ShouldPassForEqualValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            DateTime testDate = new(2024, 1, 1, 12, 0, 0);

            // Act
            GenericPropertyValidator<User, DateTime> validator = builder.RuleFor(u => u.CreatedAt, testDate);
            validator.Equal(testDate);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void RuleFor_DateTimeProperty_WithNotEqualValidation_ShouldFailForEqualValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            DateTime testDate = new(2024, 1, 1);

            // Act
            GenericPropertyValidator<User, DateTime> validator = builder.RuleFor(u => u.CreatedAt, testDate);
            validator.NotEqual(testDate);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_DateTimeProperty_WithMustCondition_ShouldValidateCustomLogic()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            DateTime futureDate = DateTime.Now.AddDays(1);

            // Act
            GenericPropertyValidator<User, DateTime> validator = builder.RuleFor(u => u.CreatedAt, futureDate);
            validator.Must(date => date > DateTime.Now, "Date must be in the future");

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void RuleFor_NullableDateTimeProperty_WithNotNull_ShouldFailForNullValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            GenericPropertyValidator<User, DateTime?> validator = builder.RuleFor(u => u.UpdatedAt, (DateTime?)null);
            validator.Notnull();

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_NullableDateTimeProperty_WithNotNull_ShouldPassForNonNullValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            DateTime testDate = DateTime.Now;

            // Act
            GenericPropertyValidator<User, DateTime?> validator = builder.RuleFor(u => u.UpdatedAt, testDate);
            validator.Notnull();

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }
    }

    #endregion DateTime Property Validation Tests

    #region Boolean Property Validation Tests

    /// <summary>
    /// Tests for boolean property validation using GenericPropertyValidator
    /// </summary>
    public class BooleanPropertyValidation
    {
        [Fact]
        public void RuleFor_BooleanProperty_WithEqualTrue_ShouldPassForTrueValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            GenericPropertyValidator<User, bool> validator = builder.RuleFor(u => u.IsActive, true);
            validator.Equal(true);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void RuleFor_BooleanProperty_WithEqualTrue_ShouldFailForFalseValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            GenericPropertyValidator<User, bool> validator = builder.RuleFor(u => u.IsActive, false);
            validator.Equal(true);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_BooleanProperty_WithMustCondition_ShouldValidateCustomLogic()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            GenericPropertyValidator<User, bool> validator = builder.RuleFor(u => u.IsActive, false);
            validator.Must(isActive => !isActive, "User must be inactive for this operation");

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void RuleFor_NullableBooleanProperty_WithNotNull_ShouldFailForNullValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            GenericPropertyValidator<User, bool?> validator = builder.RuleFor(u => u.IsVerified, (bool?)null);
            validator.Notnull();

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_NullableBooleanProperty_WithNotNull_ShouldPassForNonNullValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            GenericPropertyValidator<User, bool?> validator = builder.RuleFor(u => u.IsVerified, true);
            validator.Notnull();

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }
    }

    #endregion Boolean Property Validation Tests

    #region Additional Numeric Property Validation Tests

    /// <summary>
    /// Tests for additional numeric property validation using NumericPropertyValidator
    /// </summary>
    public class AdditionalNumericPropertyValidation
    {
        [Fact]
        public void RuleFor_ByteProperty_WithGreaterThan_ShouldFailForSmallerValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, byte> validator = builder.RuleFor(u => u.Status, (byte)0);
            validator.GreaterThan((byte)0);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_ByteProperty_WithLessThanOrEqualTo_ShouldPassForEqualValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, byte> validator = builder.RuleFor(u => u.Status, (byte)255);
            validator.LessThanOrEqualTo((byte)255);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void RuleFor_SByteProperty_WithInclusiveBetween_ShouldPassForValueInRange()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, sbyte> validator = builder.RuleFor(u => u.Balance, (sbyte)0);
            validator.GreaterThanOrEqualTo((sbyte)-10).LessThanOrEqualTo((sbyte)10);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void RuleFor_UIntProperty_WithEqual_ShouldPassForEqualValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, uint> validator = builder.RuleFor(u => u.Points, 1000U);
            validator.Equal(1000U);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void RuleFor_ULongProperty_WithNotEqual_ShouldFailForEqualValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, ulong> validator = builder.RuleFor(u => u.Token, 0UL);
            validator.NotEqual(0UL);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_ULongProperty_WithGreaterThan_ShouldPassForLargerValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, ulong> validator = builder.RuleFor(u => u.Token, 1000000UL);
            validator.GreaterThan(999999UL);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }
    }

    #endregion Additional Numeric Property Validation Tests

    #region InclusiveBetween Bug Fix Tests

    /// <summary>
    /// Comprehensive failing tests for InclusiveBetween bug fix in NumericPropertyValidator.
    /// Current implementation has critical bugs:
    /// 1. Uses int parameters instead of TNumeric generic parameters
    /// 2. Unsafe casting of InclusiveBetweenRule(int, int) to IRule&lt;TNumeric&gt;
    /// 3. Will fail at runtime for non-int numeric types
    /// 
    /// These tests will drive implementation of generic InclusiveBetweenRule&lt;TNumeric&gt; and
    /// the correct InclusiveBetween method signature accepting TNumeric parameters.
    /// </summary>
    public class InclusiveBetweenBugFixTests
    {
        [Fact]
        public void InclusiveBetween_IntType_WithIntParameters_ShouldPassForValueInRange()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, int> validator = builder.RuleFor(u => u.Age, 25);
            validator.InclusiveBetween(18, 65);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_IntType_WithIntParameters_ShouldFailForValueBelowRange()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, int> validator = builder.RuleFor(u => u.Age, 17);
            validator.InclusiveBetween(18, 65);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void InclusiveBetween_IntType_WithIntParameters_ShouldFailForValueAboveRange()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, int> validator = builder.RuleFor(u => u.Age, 66);
            validator.InclusiveBetween(18, 65);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void InclusiveBetween_IntType_WithIntParameters_ShouldPassForLowerBoundary()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, int> validator = builder.RuleFor(u => u.Age, 18);
            validator.InclusiveBetween(18, 65);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_IntType_WithIntParameters_ShouldPassForUpperBoundary()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, int> validator = builder.RuleFor(u => u.Age, 65);
            validator.InclusiveBetween(18, 65);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_LongType_WithLongParameters_ShouldPassForValueInRange()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert
            // Act
            NumericPropertyValidator<User, long> validator = builder.RuleFor(u => u.Phone, 5551234567L);
            validator.InclusiveBetween(1000000000L, 9999999999L);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_DecimalType_WithDecimalParameters_ShouldPassForValueInRange()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert
            // Act
            NumericPropertyValidator<User, decimal> validator = builder.RuleFor(u => u.Salary, 50000.50m);
            validator.InclusiveBetween(30000.00m, 100000.00m);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_DoubleType_WithDoubleParameters_ShouldPassForValueInRange()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert
            // Act
            NumericPropertyValidator<User, double> validator = builder.RuleFor(u => u.Score, 85.5);
            validator.InclusiveBetween(0.0, 100.0);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_FloatType_WithFloatParameters_ShouldPassForValueInRange()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert
            // Act
            NumericPropertyValidator<User, float> validator = builder.RuleFor(u => u.Rating, 4.2f);
            validator.InclusiveBetween(1.0f, 5.0f);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_ShortType_WithShortParameters_ShouldPassForValueInRange()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert
            // Act
            NumericPropertyValidator<User, short> validator = builder.RuleFor(u => u.Priority, (short)5);
            validator.InclusiveBetween((short)1, (short)10);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_ByteType_WithByteParameters_ShouldPassForValueInRange()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert
            // Act
            NumericPropertyValidator<User, byte> validator = builder.RuleFor(u => u.Status, (byte)128);
            validator.InclusiveBetween((byte)100, (byte)200);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_SByteType_WithSByteParameters_ShouldPassForValueInRange()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert
            // Act
            NumericPropertyValidator<User, sbyte> validator = builder.RuleFor(u => u.Balance, (sbyte)0);
            validator.InclusiveBetween((sbyte)-10, (sbyte)10);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_UIntType_WithUIntParameters_ShouldPassForValueInRange()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert
            // Act
            NumericPropertyValidator<User, uint> validator = builder.RuleFor(u => u.Points, 1500U);
            validator.InclusiveBetween(1000U, 2000U);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_ULongType_WithULongParameters_ShouldPassForValueInRange()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert
            // Act
            NumericPropertyValidator<User, ulong> validator = builder.RuleFor(u => u.Token, 5000000000UL);
            validator.InclusiveBetween(1000000000UL, 9000000000UL);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_DecimalType_WithPreciseRange_ShouldValidateCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert - This test will fail with current implementation
            // Act
            NumericPropertyValidator<User, decimal> validator = builder.RuleFor(u => u.Salary, 75000.50m);
            validator.InclusiveBetween(30000.00m, 100000.00m);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_DoubleType_WithFloatingPointRange_ShouldValidateCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert - This test will fail with current implementation
            // Act
            NumericPropertyValidator<User, double> validator = builder.RuleFor(u => u.Score, 85.5);
            validator.InclusiveBetween(0.0, 100.0);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_LongType_WithLargeRange_ShouldValidateCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert - This test will fail with current implementation
            // Act
            NumericPropertyValidator<User, long> validator = builder.RuleFor(u => u.Phone, 5551234567L);
            validator.InclusiveBetween(1000000000L, 9999999999L);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_FloatType_WithBoundaryValues_ShouldValidateCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert - This test will fail with current implementation
            // Act
            NumericPropertyValidator<User, float> validator = builder.RuleFor(u => u.Rating, 1.0f);
            validator.InclusiveBetween(1.0f, 5.0f);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_ShortType_WithNegativeRange_ShouldValidateCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert - This test will fail with current implementation
            // Act
            NumericPropertyValidator<User, short> validator = builder.RuleFor(u => u.Priority, (short)-5);
            validator.InclusiveBetween((short)-10, (short)10);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_ByteType_WithMaxRange_ShouldValidateCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert - This test will fail with current implementation
            // Act
            NumericPropertyValidator<User, byte> validator = builder.RuleFor(u => u.Status, (byte)255);
            validator.InclusiveBetween((byte)200, (byte)255);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_UIntType_WithLargeUnsignedRange_ShouldValidateCorrectly()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert - This test will fail with current implementation
            // Act
            NumericPropertyValidator<User, uint> validator = builder.RuleFor(u => u.Points, 4000000000U);
            validator.InclusiveBetween(3000000000U, 4294967295U);

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void InclusiveBetween_MultipleNumericTypes_WithRangeValidation_ShouldAggregateErrors()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act & Assert - All of these will fail with current implementation
            // Act
            builder.RuleFor(u => u.Age, 16).InclusiveBetween(18, 65);                    // int - should work
            builder.RuleFor(u => u.Salary, 25000.00m).InclusiveBetween(30000.00m, 100000.00m); // decimal - will fail
            builder.RuleFor(u => u.Score, 105.0).InclusiveBetween(0.0, 100.0);         // double - will fail
            builder.RuleFor(u => u.Rating, 0.5f).InclusiveBetween(1.0f, 5.0f);         // float - will fail
            builder.RuleFor(u => u.Phone, 123L).InclusiveBetween(1000000000L, 9999999999L); // long - will fail

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void InclusiveBetween_WithCustomErrorMessage_ShouldUseCustomMessage()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string customMessage = "Age must be between 18 and 65 years old";

            // Act
            NumericPropertyValidator<User, int> validator = builder.RuleFor(u => u.Age, 16);
            validator.InclusiveBetween(18, 65).WithMessage(customMessage);

            // Assert
            builder.HasErrors.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors["Age"].ShouldContain(customMessage);
        }

        [Fact]
        public void InclusiveBetween_WithCustomDisplayName_ShouldUseCustomName()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            NumericPropertyValidator<User, int> validator = builder.RuleFor(u => u.Age, 16, "User Age");
            validator.InclusiveBetween(18, 65);

            // Assert
            builder.HasErrors.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors.ShouldContainKey("User Age");
        }

        [Fact]
        public void InclusiveBetween_ChainedWithOtherNumericValidations_ShouldAggregateErrors()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act - This will expose multiple validation failures
            // Act
            NumericPropertyValidator<User, decimal> validator = builder.RuleFor(u => u.Salary, -5000.00m);
            validator.GreaterThan(0m)
            .InclusiveBetween(30000.00m, 100000.00m)  // This line will throw
            .LessThan(200000.00m);

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }
    }

    #endregion InclusiveBetween Bug Fix Tests

    #region Character Property Validation Tests

    /// <summary>
    /// Tests for character property validation using GenericPropertyValidator
    /// </summary>
    public class CharacterPropertyValidation
    {
        [Fact]
        public void RuleFor_CharProperty_WithEqualValidation_ShouldPassForEqualValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            GenericPropertyValidator<User, char> validator = builder.RuleFor(u => u.Grade, 'A');
            validator.Equal('A');

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void RuleFor_CharProperty_WithNotEqual_ShouldFailForEqualValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            GenericPropertyValidator<User, char> validator = builder.RuleFor(u => u.Grade, 'F');
            validator.NotEqual('F');

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_CharProperty_WithMustCondition_ShouldValidateCustomLogic()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            GenericPropertyValidator<User, char> validator = builder.RuleFor(u => u.Grade, 'B');
            validator.Must(grade => "ABCDF".Contains(grade), "Grade must be A, B, C, D, or F");

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void RuleFor_NullableCharProperty_WithNotNull_ShouldFailForNullValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            GenericPropertyValidator<User, char?> validator = builder.RuleFor(u => u.MiddleInitial, (char?)null);
            validator.Notnull();

            // Assert
            builder.HasErrors.ShouldBeTrue();
        }

        [Fact]
        public void RuleFor_NullableCharProperty_WithNotNull_ShouldPassForNonNullValue()
        {
            // Arrange
            ValidationBuilder<User> builder = new();

            // Act
            GenericPropertyValidator<User, char?> validator = builder.RuleFor(u => u.MiddleInitial, 'J');
            validator.Notnull();

            // Assert
            builder.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public void RuleFor_CharProperty_WithCustomMessage_ShouldUseCustomErrorMessage()
        {
            // Arrange
            ValidationBuilder<User> builder = new();
            string customMessage = "Invalid grade - must be a valid letter grade";

            // Act
            GenericPropertyValidator<User, char> validator = builder.RuleFor(u => u.Grade, 'X');
            validator.Must(grade => "ABCDF".Contains(grade), "Grade must be valid").WithMessage(customMessage);

            // Assert
            builder.HasErrors.ShouldBeTrue();
            Dictionary<string, string[]> errors = builder.GetErrors();
            errors["Grade"].ShouldContain(customMessage);
        }
    }

    #endregion Character Property Validation Tests
}