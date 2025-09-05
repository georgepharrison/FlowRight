using FlowRight.Validation.Context;
using FlowRight.Validation.Rules;
using Shouldly;

namespace FlowRight.Validation.Tests.Rules;

/// <summary>
/// Comprehensive failing tests for MustContextAwareRule<T> that define expected behavior
/// for synchronous context-aware validation rules. These tests follow TDD principles and will initially 
/// fail until the MustContextAwareRule<T> implementation is complete.
/// 
/// Test Coverage:
/// - Valid conditions (returning true)
/// - Invalid conditions (returning false)
/// - Context access (root objects, services, custom data)
/// - Exception handling in conditions
/// - Null context handling
/// - Fallback IRule interface compatibility
/// - Constructor validation
/// - Multiple validation scenarios
/// - Cross-property validation scenarios
/// 
/// Current Status: FAILING as expected (TDD Red phase)
/// These failing tests serve as executable specifications for MustContextAwareRule<T>.
/// </summary>
public class MustContextAwareRuleTests
{
    #region Test Data Classes

    /// <summary>
    /// Test user model for validation scenarios
    /// </summary>
    public class TestUser
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int Age { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Mock service for testing service integration
    /// </summary>
    public interface ITestValidationService
    {
        bool IsValid(string value);
        bool ValidateWithCriteria(string value, string criteria);
        bool ThrowException(string value);
    }

    /// <summary>
    /// Mock implementation of ITestValidationService
    /// </summary>
    public class MockTestValidationService : ITestValidationService
    {
        public bool ShouldReturnValid { get; set; } = true;
        public bool ShouldThrowException { get; set; }
        public Exception? ExceptionToThrow { get; set; }

        public bool IsValid(string value)
        {
            if (ShouldThrowException && ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }
            
            return ShouldReturnValid;
        }

        public bool ValidateWithCriteria(string value, string criteria)
        {
            if (ShouldThrowException && ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }
            
            return ShouldReturnValid && value.Contains(criteria);
        }

        public bool ThrowException(string value)
        {
            throw new InvalidOperationException("Service validation failed");
        }
    }

    #endregion Test Data Classes

    #region Constructor Tests

    /// <summary>
    /// Tests for MustContextAwareRule constructor behavior
    /// </summary>
    public class Constructor
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateRule()
        {
            // Arrange
            Func<string, IValidationContext, bool> condition = (value, context) => !string.IsNullOrEmpty(value);
            string errorMessage = "Value is required";

            // Act
            MustContextAwareRule<string> rule = new(condition, errorMessage);

            // Assert
            rule.ShouldNotBeNull();
        }

        [Fact]
        public void Constructor_WithNullCondition_ShouldThrowArgumentNullException()
        {
            // Arrange
            Func<string, IValidationContext, bool>? nullCondition = null;
            string errorMessage = "Error message";

            // Act & Assert
            ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => 
                new MustContextAwareRule<string>(nullCondition!, errorMessage));
            exception.ParamName.ShouldBe("condition");
        }

        [Fact]
        public void Constructor_WithNullErrorMessage_ShouldThrowArgumentNullException()
        {
            // Arrange
            Func<string, IValidationContext, bool> condition = (value, context) => true;
            string? nullErrorMessage = null;

            // Act & Assert
            ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => 
                new MustContextAwareRule<string>(condition, nullErrorMessage!));
            exception.ParamName.ShouldBe("errorMessage");
        }

        [Fact]
        public void Constructor_WithEmptyErrorMessage_ShouldCreateRuleSuccessfully()
        {
            // Arrange
            Func<string, IValidationContext, bool> condition = (value, context) => true;
            string emptyErrorMessage = string.Empty;

            // Act
            MustContextAwareRule<string> rule = new(condition, emptyErrorMessage);

            // Assert
            rule.ShouldNotBeNull();
        }
    }

    #endregion Constructor Tests

    #region Valid Condition Tests

    /// <summary>
    /// Tests for conditions that return true (valid scenarios)
    /// </summary>
    public class ValidConditionTests
    {
        [Fact]
        public void Validate_WithConditionReturningTrue_ShouldReturnNull()
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (value, context) => !string.IsNullOrEmpty(value),
                "Value is required");
            
            IValidationContext context = ValidationContext.Create();
            string validValue = "Hello";

            // Act
            string? result = rule.Validate(validValue, "Field", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithSimpleCondition_ShouldReturnNull()
        {
            // Arrange
            MustContextAwareRule<int> rule = new(
                (value, context) => value > 0,
                "Value must be positive");
            
            IValidationContext context = ValidationContext.Create();
            int validValue = 42;

            // Act
            string? result = rule.Validate(validValue, "Number", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithComplexLogic_ShouldReturnNull()
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (email, context) =>
                {
                    // Simulate complex validation
                    bool isValidFormat = email.Contains("@");
                    bool isValidDomain = email.EndsWith(".com");
                    bool hasValidLength = email.Length >= 5;
                    return isValidFormat && isValidDomain && hasValidLength;
                },
                "Email format is invalid");
            
            IValidationContext context = ValidationContext.Create();
            string validEmail = "test@example.com";

            // Act
            string? result = rule.Validate(validEmail, "Email", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithAlwaysTrueCondition_ShouldReturnNull()
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (value, context) => true,
                "This should never fail");
            
            IValidationContext context = ValidationContext.Create();
            string anyValue = "any value";

            // Act
            string? result = rule.Validate(anyValue, "Field", context);

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion Valid Condition Tests

    #region Invalid Condition Tests

    /// <summary>
    /// Tests for conditions that return false (invalid scenarios)
    /// </summary>
    public class InvalidConditionTests
    {
        [Fact]
        public void Validate_WithConditionReturningFalse_ShouldReturnErrorMessage()
        {
            // Arrange
            string expectedError = "Value is invalid";
            MustContextAwareRule<string> rule = new(
                (value, context) => false, // Always invalid
                expectedError);
            
            IValidationContext context = ValidationContext.Create();
            string testValue = "test";

            // Act
            string? result = rule.Validate(testValue, "Field", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedError);
        }

        [Fact]
        public void Validate_WithFailingCondition_ShouldReturnErrorMessage()
        {
            // Arrange
            string expectedError = "Email is already taken";
            MustContextAwareRule<string> rule = new(
                (email, context) => !email.Equals("existing@example.com", StringComparison.OrdinalIgnoreCase),
                expectedError);
            
            IValidationContext context = ValidationContext.Create();
            string existingEmail = "existing@example.com";

            // Act
            string? result = rule.Validate(existingEmail, "Email", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedError);
        }

        [Fact]
        public void Validate_WithComplexFailingCondition_ShouldReturnErrorMessage()
        {
            // Arrange
            string expectedError = "Age validation failed";
            MustContextAwareRule<int> rule = new(
                (age, context) =>
                {
                    bool isAdult = age >= 18;
                    bool isSenior = age >= 65;
                    bool isValidRange = age <= 120;
                    
                    // Complex business logic - must be adult but not senior
                    return isAdult && !isSenior && isValidRange;
                },
                expectedError);
            
            IValidationContext context = ValidationContext.Create();
            int invalidAge = 70; // Senior age that should fail

            // Act
            string? result = rule.Validate(invalidAge, "Age", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedError);
        }

        [Theory]
        [InlineData("Custom error 1")]
        [InlineData("Validation failed with specific reason")]
        [InlineData("Business rule violation detected")]
        public void Validate_WithDifferentErrorMessages_ShouldReturnCorrectMessage(string errorMessage)
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (value, context) => false,
                errorMessage);
            
            IValidationContext context = ValidationContext.Create();

            // Act
            string? result = rule.Validate("test", "Field", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(errorMessage);
        }
    }

    #endregion Invalid Condition Tests

    #region Context Access Tests

    /// <summary>
    /// Tests for accessing validation context within conditions
    /// </summary>
    public class ContextAccessTests
    {
        [Fact]
        public void Validate_WithRootObjectAccess_ShouldUseRootObjectInValidation()
        {
            // Arrange
            TestUser user = new() { Email = "test@example.com", Username = "testuser" };
            MustContextAwareRule<string> rule = new(
                (email, context) =>
                {
                    TestUser? rootUser = context.GetRootObject<TestUser>();
                    return rootUser != null && !email.Equals(rootUser.Username, StringComparison.OrdinalIgnoreCase);
                },
                "Email cannot be the same as username");
            
            IValidationContext context = ValidationContext.Create(user);
            string differentEmail = "different@example.com";

            // Act
            string? result = rule.Validate(differentEmail, "Email", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithRootObjectAccessFailing_ShouldReturnErrorMessage()
        {
            // Arrange
            TestUser user = new() { Email = "test@example.com", Username = "test@example.com" };
            MustContextAwareRule<string> rule = new(
                (email, context) =>
                {
                    TestUser? rootUser = context.GetRootObject<TestUser>();
                    return rootUser == null || !email.Equals(rootUser.Username, StringComparison.OrdinalIgnoreCase);
                },
                "Email cannot be the same as username");
            
            IValidationContext context = ValidationContext.Create(user);
            string sameAsUsername = "test@example.com";

            // Act
            string? result = rule.Validate(sameAsUsername, "Email", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Email cannot be the same as username");
        }

        [Fact]
        public void Validate_WithCrossPropertyValidation_ShouldUseMultipleProperties()
        {
            // Arrange
            TestUser user = new() { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };
            MustContextAwareRule<string> rule = new(
                (email, context) =>
                {
                    TestUser? rootUser = context.GetRootObject<TestUser>();
                    if (rootUser != null)
                    {
                        string expectedPrefix = $"{rootUser.FirstName.ToLower()}.{rootUser.LastName.ToLower()}";
                        return email.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase);
                    }
                    return false;
                },
                "Email must start with first.last name format");
            
            IValidationContext context = ValidationContext.Create(user);
            string validEmail = "john.doe@example.com";

            // Act
            string? result = rule.Validate(validEmail, "Email", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithCrossPropertyValidationFailing_ShouldReturnErrorMessage()
        {
            // Arrange
            TestUser user = new() { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };
            MustContextAwareRule<string> rule = new(
                (email, context) =>
                {
                    TestUser? rootUser = context.GetRootObject<TestUser>();
                    if (rootUser != null)
                    {
                        string expectedPrefix = $"{rootUser.FirstName.ToLower()}.{rootUser.LastName.ToLower()}";
                        return email.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase);
                    }
                    return false;
                },
                "Email must start with first.last name format");
            
            IValidationContext context = ValidationContext.Create(user);
            string invalidEmail = "different@example.com";

            // Act
            string? result = rule.Validate(invalidEmail, "Email", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Email must start with first.last name format");
        }

        [Fact]
        public void Validate_WithServiceAccess_ShouldUseServiceInValidation()
        {
            // Arrange
            MockTestValidationService mockService = new() { ShouldReturnValid = true };
            IServiceProvider serviceProvider = CreateServiceProvider(mockService);
            
            MustContextAwareRule<string> rule = new(
                (value, context) =>
                {
                    ITestValidationService? service = context.GetService<ITestValidationService>();
                    if (service != null)
                    {
                        return service.IsValid(value);
                    }
                    return false;
                },
                "Service validation failed");
            
            IValidationContext context = ValidationContext.Create(serviceProvider: serviceProvider);
            string testValue = "test";

            // Act
            string? result = rule.Validate(testValue, "Field", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithServiceAccessFailing_ShouldReturnErrorMessage()
        {
            // Arrange
            MockTestValidationService mockService = new() { ShouldReturnValid = false };
            IServiceProvider serviceProvider = CreateServiceProvider(mockService);
            
            MustContextAwareRule<string> rule = new(
                (value, context) =>
                {
                    ITestValidationService? service = context.GetService<ITestValidationService>();
                    if (service != null)
                    {
                        return service.IsValid(value);
                    }
                    return false;
                },
                "Service validation failed");
            
            IValidationContext context = ValidationContext.Create(serviceProvider: serviceProvider);
            string testValue = "test";

            // Act
            string? result = rule.Validate(testValue, "Field", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Service validation failed");
        }

        [Fact]
        public void Validate_WithCustomDataAccess_ShouldUseCustomDataInValidation()
        {
            // Arrange
            MustContextAwareRule<int> rule = new(
                (age, context) =>
                {
                    int maxAge = context.GetCustomData<int>("MaxAllowedAge");
                    return maxAge == 0 || age <= maxAge;
                },
                "Age exceeds maximum allowed limit");
            
            IValidationContext context = ValidationContext.Create();
            context.SetCustomData("MaxAllowedAge", 65);
            int validAge = 50;

            // Act
            string? result = rule.Validate(validAge, "Age", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithCustomDataAccessFailing_ShouldReturnErrorMessage()
        {
            // Arrange
            MustContextAwareRule<int> rule = new(
                (age, context) =>
                {
                    int maxAge = context.GetCustomData<int>("MaxAllowedAge");
                    return maxAge == 0 || age <= maxAge;
                },
                "Age exceeds maximum allowed limit");
            
            IValidationContext context = ValidationContext.Create();
            context.SetCustomData("MaxAllowedAge", 65);
            int invalidAge = 70;

            // Act
            string? result = rule.Validate(invalidAge, "Age", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Age exceeds maximum allowed limit");
        }

        [Fact]
        public void Validate_WithCombinedContextAccess_ShouldUseAllContextFeatures()
        {
            // Arrange
            TestUser user = new() { Age = 25, Username = "testuser" };
            MockTestValidationService mockService = new() { ShouldReturnValid = true };
            IServiceProvider serviceProvider = CreateServiceProvider(mockService);
            
            MustContextAwareRule<string> rule = new(
                (email, context) =>
                {
                    TestUser? rootUser = context.GetRootObject<TestUser>();
                    ITestValidationService? service = context.GetService<ITestValidationService>();
                    bool strictMode = context.GetCustomData<bool>("StrictMode");
                    
                    if (rootUser != null && service != null)
                    {
                        bool basicValidation = service.IsValid(email);
                        bool ageCheck = rootUser.Age >= 18;
                        bool usernameCheck = !email.Equals(rootUser.Username, StringComparison.OrdinalIgnoreCase);
                        
                        if (strictMode)
                        {
                            return basicValidation && ageCheck && usernameCheck && email.Contains("@");
                        }
                        return basicValidation && ageCheck;
                    }
                    return false;
                },
                "Comprehensive validation failed");
            
            IValidationContext context = ValidationContext.Create(user, serviceProvider);
            context.SetCustomData("StrictMode", true);
            string validEmail = "different@example.com";

            // Act
            string? result = rule.Validate(validEmail, "Email", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullRootObject_ShouldHandleGracefully()
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (value, context) =>
                {
                    TestUser? rootUser = context.GetRootObject<TestUser>();
                    return rootUser == null || !string.IsNullOrEmpty(value);
                },
                "Value is required when user exists");
            
            IValidationContext context = ValidationContext.Create(); // No root object
            string testValue = "test";

            // Act
            string? result = rule.Validate(testValue, "Field", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithNullService_ShouldHandleGracefully()
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (value, context) =>
                {
                    ITestValidationService? service = context.GetService<ITestValidationService>();
                    return service == null || service.IsValid(value);
                },
                "Service validation failed");
            
            IValidationContext context = ValidationContext.Create(); // No service provider
            string testValue = "test";

            // Act
            string? result = rule.Validate(testValue, "Field", context);

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion Context Access Tests

    #region Exception Handling Tests

    /// <summary>
    /// Tests for exception handling within conditions
    /// </summary>
    public class ExceptionHandlingTests
    {
        [Fact]
        public void Validate_WithConditionThrowingException_ShouldReturnErrorWithExceptionMessage()
        {
            // Arrange
            string originalErrorMessage = "Original validation error";
            MustContextAwareRule<string> rule = new(
                (value, context) => throw new InvalidOperationException("Something went wrong"),
                originalErrorMessage);
            
            IValidationContext context = ValidationContext.Create();

            // Act
            string? result = rule.Validate("test", "Field", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldStartWith("Validation error:");
            result.ShouldContain("Something went wrong");
        }

        [Fact]
        public void Validate_WithServiceThrowingException_ShouldReturnErrorMessage()
        {
            // Arrange
            MockTestValidationService mockService = new() 
            { 
                ShouldThrowException = true, 
                ExceptionToThrow = new TimeoutException("Service timeout")
            };
            IServiceProvider serviceProvider = CreateServiceProvider(mockService);
            
            MustContextAwareRule<string> rule = new(
                (value, context) =>
                {
                    ITestValidationService? service = context.GetService<ITestValidationService>();
                    if (service != null)
                    {
                        return service.IsValid(value);
                    }
                    return false;
                },
                "Service validation failed");
            
            IValidationContext context = ValidationContext.Create(serviceProvider: serviceProvider);

            // Act
            string? result = rule.Validate("test", "Field", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldStartWith("Validation error:");
            result.ShouldContain("Service timeout");
        }

        [Fact]
        public void Validate_WithArgumentNullExceptionInCondition_ShouldRethrowException()
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (value, context) => throw new ArgumentNullException("testParam", "Parameter cannot be null"),
                "Validation error");
            
            IValidationContext context = ValidationContext.Create();

            // Act & Assert
            ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => 
                rule.Validate("test", "Field", context));
            exception.ParamName.ShouldBe("testParam");
        }

        [Theory]
        [InlineData(typeof(InvalidDataException), "Invalid data provided")]
        [InlineData(typeof(NotSupportedException), "Operation not supported")]
        [InlineData(typeof(TimeoutException), "Operation timed out")]
        public void Validate_WithVariousExceptionTypes_ShouldReturnErrorWithExceptionMessage(Type exceptionType, string exceptionMessage)
        {
            // Arrange
            Exception exceptionToThrow = (Exception)Activator.CreateInstance(exceptionType, exceptionMessage)!;
            MustContextAwareRule<string> rule = new(
                (value, context) => throw exceptionToThrow,
                "Original error");
            
            IValidationContext context = ValidationContext.Create();

            // Act
            string? result = rule.Validate("test", "Field", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldStartWith("Validation error:");
            result.ShouldContain(exceptionMessage);
        }

        [Fact]
        public void Validate_WithNullReferenceExceptionInCondition_ShouldReturnErrorMessage()
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (value, context) =>
                {
                    string? nullString = null;
                    return nullString.Length > 0; // Will throw NullReferenceException
                },
                "Original error");
            
            IValidationContext context = ValidationContext.Create();

            // Act
            string? result = rule.Validate("test", "Field", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldStartWith("Validation error:");
        }
    }

    #endregion Exception Handling Tests

    #region Null Context Tests

    /// <summary>
    /// Tests for null context handling
    /// </summary>
    public class NullContextTests
    {
        [Fact]
        public void Validate_WithNullContextInContextAwareMethod_ShouldThrowArgumentNullException()
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (value, context) => true,
                "Error message");
            
            IValidationContext? nullContext = null;

            // Act & Assert
            ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => 
                rule.Validate("test", "Field", nullContext!));
            exception.ParamName.ShouldBe("context");
        }
    }

    #endregion Null Context Tests

    #region Fallback IRule Interface Tests

    /// <summary>
    /// Tests for the fallback IRule interface implementation
    /// </summary>
    public class FallbackIRuleInterfaceTests
    {
        [Fact]
        public void Validate_WithoutContext_ShouldCreateMinimalContextAndValidate()
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (value, context) => !string.IsNullOrEmpty(value),
                "Value is required");
            
            string validValue = "test";

            // Act
            string? result = rule.Validate(validValue, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithoutContextFailing_ShouldReturnErrorMessage()
        {
            // Arrange
            string expectedError = "Value is invalid";
            MustContextAwareRule<string> rule = new(
                (value, context) => false,
                expectedError);
            
            string testValue = "test";

            // Act
            string? result = rule.Validate(testValue, "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedError);
        }

        [Fact]
        public void Validate_WithoutContextButUsingContextFeatures_ShouldWorkWithLimitedContext()
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (value, context) =>
                {
                    // Try to access context features that won't be available
                    TestUser? user = context.GetRootObject<TestUser>();
                    ITestValidationService? service = context.GetService<ITestValidationService>();
                    
                    // Should fall back to basic validation when context features aren't available
                    return user == null && service == null && !string.IsNullOrEmpty(value);
                },
                "Value is required");
            
            string validValue = "test";

            // Act
            string? result = rule.Validate(validValue, "Field");

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithoutContextAndException_ShouldReturnErrorMessage()
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (value, context) => throw new InvalidOperationException("Condition failed"),
                "Original error");

            // Act
            string? result = rule.Validate("test", "Field");

            // Assert
            result.ShouldNotBeNull();
            result.ShouldStartWith("Validation error:");
            result.ShouldContain("Condition failed");
        }
    }

    #endregion Fallback IRule Interface Tests

    #region Multiple Validation Tests

    /// <summary>
    /// Tests for multiple validation scenarios
    /// </summary>
    public class MultipleValidationTests
    {
        [Fact]
        public void Validate_SameRuleMultipleTimes_ShouldProduceConsistentResults()
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (value, context) => value.Length >= 5,
                "Value too short");
            
            IValidationContext context = ValidationContext.Create();

            // Act
            string? result1 = rule.Validate("Valid", "Field1", context);
            string? result2 = rule.Validate("Hi", "Field2", context);
            string? result3 = rule.Validate("AlsoValid", "Field3", context);

            // Assert
            result1.ShouldBeNull();
            result2.ShouldNotBeNull();
            result2.ShouldBe("Value too short");
            result3.ShouldBeNull();
        }

        [Fact]
        public void Validate_DifferentRuleInstances_ShouldWorkIndependently()
        {
            // Arrange
            MustContextAwareRule<int> rule1 = new(
                (value, context) => value > 10,
                "Must be greater than 10");
                
            MustContextAwareRule<int> rule2 = new(
                (value, context) => value < 100,
                "Must be less than 100");
            
            IValidationContext context = ValidationContext.Create();
            int testValue = 50;

            // Act
            string? result1 = rule1.Validate(testValue, "Field", context);
            string? result2 = rule2.Validate(testValue, "Field", context);

            // Assert
            result1.ShouldBeNull();
            result2.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithDifferentValueTypes_ShouldWorkCorrectly()
        {
            // Arrange
            MustContextAwareRule<TestUser> userRule = new(
                (user, context) => user != null && !string.IsNullOrEmpty(user.Email),
                "User must have email");
                
            MustContextAwareRule<decimal> decimalRule = new(
                (value, context) => value >= 0,
                "Value must be non-negative");
            
            IValidationContext context = ValidationContext.Create();
            TestUser validUser = new() { Email = "test@example.com" };
            decimal validDecimal = 42.50m;

            // Act
            string? userResult = userRule.Validate(validUser, "User", context);
            string? decimalResult = decimalRule.Validate(validDecimal, "Amount", context);

            // Assert
            userResult.ShouldBeNull();
            decimalResult.ShouldBeNull();
        }

        [Fact]
        public void Validate_WithSharedContext_ShouldMaintainContextState()
        {
            // Arrange
            IValidationContext sharedContext = ValidationContext.Create();
            sharedContext.SetCustomData("SharedCounter", 0);
            
            MustContextAwareRule<string> rule = new(
                (value, context) =>
                {
                    int counter = context.GetCustomData<int>("SharedCounter");
                    context.SetCustomData("SharedCounter", counter + 1);
                    return !string.IsNullOrEmpty(value);
                },
                "Value is required");

            // Act
            string? result1 = rule.Validate("test1", "Field1", sharedContext);
            string? result2 = rule.Validate("test2", "Field2", sharedContext);
            int finalCounter = sharedContext.GetCustomData<int>("SharedCounter");

            // Assert
            result1.ShouldBeNull();
            result2.ShouldBeNull();
            finalCounter.ShouldBe(2);
        }

        [Fact]
        public void Validate_WithThreadSafety_ShouldWorkConcurrently()
        {
            // Arrange
            MustContextAwareRule<string> rule = new(
                (value, context) => value.Length >= 3,
                "Value too short");
            
            IValidationContext context = ValidationContext.Create();
            string[] testValues = ["Valid", "Hi", "Also", "X", "Valid123"];
            List<string?> results = [];

            // Act
            Parallel.ForEach(testValues, value =>
            {
                string? result = rule.Validate(value, "Field", context);
                lock (results)
                {
                    results.Add(result);
                }
            });

            // Assert
            results.Count.ShouldBe(5);
            results.Count(r => r == null).ShouldBe(3); // "Valid", "Also", "Valid123" should pass
            results.Count(r => r != null).ShouldBe(2); // "Hi", "X" should fail
        }
    }

    #endregion Multiple Validation Tests

    #region Performance Tests

    /// <summary>
    /// Tests for performance characteristics
    /// </summary>
    public class PerformanceTests
    {
        [Fact]
        public void Validate_WithSimpleCondition_ShouldExecuteQuickly()
        {
            // Arrange
            MustContextAwareRule<int> rule = new(
                (value, context) => value > 0,
                "Value must be positive");
            
            IValidationContext context = ValidationContext.Create();
            DateTime startTime = DateTime.UtcNow;

            // Act
            for (int i = 0; i < 1000; i++)
            {
                rule.Validate(42, "Field", context);
            }
            DateTime endTime = DateTime.UtcNow;

            // Assert
            (endTime - startTime).TotalMilliseconds.ShouldBeLessThan(100);
        }

        [Fact]
        public void Validate_WithComplexContextAccess_ShouldStillExecuteReasonably()
        {
            // Arrange
            TestUser user = new() { Age = 25, Email = "test@example.com", Username = "testuser" };
            MockTestValidationService mockService = new() { ShouldReturnValid = true };
            IServiceProvider serviceProvider = CreateServiceProvider(mockService);
            
            MustContextAwareRule<string> rule = new(
                (email, context) =>
                {
                    TestUser? rootUser = context.GetRootObject<TestUser>();
                    ITestValidationService? service = context.GetService<ITestValidationService>();
                    bool hasCustomData = context.HasCustomData("TestKey");
                    
                    return rootUser != null && service != null && service.IsValid(email);
                },
                "Complex validation failed");
            
            IValidationContext context = ValidationContext.Create(user, serviceProvider);
            context.SetCustomData("TestKey", "TestValue");
            DateTime startTime = DateTime.UtcNow;

            // Act
            for (int i = 0; i < 100; i++)
            {
                rule.Validate("test@example.com", "Email", context);
            }
            DateTime endTime = DateTime.UtcNow;

            // Assert
            (endTime - startTime).TotalMilliseconds.ShouldBeLessThan(500);
        }
    }

    #endregion Performance Tests

    #region Helper Methods

    /// <summary>
    /// Creates a mock service provider with the specified validation service
    /// </summary>
    private static IServiceProvider CreateServiceProvider(ITestValidationService validationService)
    {
        Dictionary<Type, object> services = new()
        {
            [typeof(ITestValidationService)] = validationService
        };

        return new MockServiceProvider(services);
    }

    /// <summary>
    /// Mock service provider implementation for testing
    /// </summary>
    private class MockServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services;

        public MockServiceProvider(Dictionary<Type, object> services)
        {
            _services = services;
        }

        public object? GetService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out object? service);
            return service;
        }
    }

    #endregion Helper Methods
}