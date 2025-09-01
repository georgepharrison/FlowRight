using FlowRight.Validation.Context;
using FlowRight.Validation.Rules;
using Shouldly;

namespace FlowRight.Validation.Tests.Rules;

/// <summary>
/// Comprehensive failing tests for MustAsyncContextAwareRule<T> that define expected behavior
/// for async context-aware validation rules. These tests follow TDD principles and will initially 
/// fail until the MustAsyncContextAwareRule<T> implementation is complete.
/// 
/// Test Coverage:
/// - Valid async conditions (returning true)
/// - Invalid async conditions (returning false)
/// - Context access (root objects, services, custom data)
/// - Exception handling in async conditions
/// - Null context handling
/// - Constructor validation
/// - Async operation scenarios
/// - Task cancellation scenarios
/// - Multiple validation scenarios
/// 
/// Current Status: FAILING as expected (TDD Red phase)
/// These failing tests serve as executable specifications for MustAsyncContextAwareRule<T>.
/// </summary>
public class MustAsyncContextAwareRuleTests
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
    }

    /// <summary>
    /// Mock service for testing service integration
    /// </summary>
    public interface ITestValidationService
    {
        Task<bool> ValidateAsync(string value);
        Task<bool> ValidateWithDelayAsync(string value, int delayMs);
        Task<bool> ThrowExceptionAsync(string value);
    }

    /// <summary>
    /// Mock implementation of ITestValidationService
    /// </summary>
    public class MockTestValidationService : ITestValidationService
    {
        public bool ShouldReturnValid { get; set; } = true;
        public bool ShouldThrowException { get; set; }
        public Exception? ExceptionToThrow { get; set; }

        public async Task<bool> ValidateAsync(string value)
        {
            if (ShouldThrowException && ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }
            
            await Task.Delay(1); // Simulate async work
            return ShouldReturnValid;
        }

        public async Task<bool> ValidateWithDelayAsync(string value, int delayMs)
        {
            await Task.Delay(delayMs);
            return ShouldReturnValid;
        }

        public async Task<bool> ThrowExceptionAsync(string value)
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Service validation failed");
        }
    }

    #endregion Test Data Classes

    #region Constructor Tests

    /// <summary>
    /// Tests for MustAsyncContextAwareRule constructor behavior
    /// </summary>
    public class Constructor
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateRule()
        {
            // Arrange
            Func<string, IValidationContext, Task<bool>> condition = async (value, context) =>
            {
                await Task.Delay(1);
                return !string.IsNullOrEmpty(value);
            };
            string errorMessage = "Value is required";

            // Act
            MustAsyncContextAwareRule<string> rule = new(condition, errorMessage);

            // Assert
            rule.ShouldNotBeNull();
        }

        [Fact]
        public void Constructor_WithNullCondition_ShouldThrowArgumentNullException()
        {
            // Arrange
            Func<string, IValidationContext, Task<bool>>? nullCondition = null;
            string errorMessage = "Error message";

            // Act & Assert
            ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => 
                new MustAsyncContextAwareRule<string>(nullCondition!, errorMessage));
            exception.ParamName.ShouldBe("condition");
        }

        [Fact]
        public void Constructor_WithNullErrorMessage_ShouldThrowArgumentNullException()
        {
            // Arrange
            Func<string, IValidationContext, Task<bool>> condition = async (value, context) =>
            {
                await Task.Delay(1);
                return true;
            };
            string? nullErrorMessage = null;

            // Act & Assert
            ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => 
                new MustAsyncContextAwareRule<string>(condition, nullErrorMessage!));
            exception.ParamName.ShouldBe("errorMessage");
        }

        [Fact]
        public void Constructor_WithEmptyErrorMessage_ShouldCreateRuleSuccessfully()
        {
            // Arrange
            Func<string, IValidationContext, Task<bool>> condition = async (value, context) =>
            {
                await Task.Delay(1);
                return true;
            };
            string emptyErrorMessage = string.Empty;

            // Act
            MustAsyncContextAwareRule<string> rule = new(condition, emptyErrorMessage);

            // Assert
            rule.ShouldNotBeNull();
        }
    }

    #endregion Constructor Tests

    #region Valid Condition Tests

    /// <summary>
    /// Tests for async conditions that return true (valid scenarios)
    /// </summary>
    public class ValidConditionTests
    {
        [Fact]
        public async Task ValidateAsync_WithConditionReturningTrue_ShouldReturnNull()
        {
            // Arrange
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    await Task.Delay(1);
                    return !string.IsNullOrEmpty(value);
                },
                "Value is required");
            
            IValidationContext context = ValidationContext.Create();
            string validValue = "Hello";

            // Act
            string? result = await rule.ValidateAsync(validValue, "Field", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ValidateAsync_WithSimpleAsyncCondition_ShouldReturnNull()
        {
            // Arrange
            MustAsyncContextAwareRule<int> rule = new(
                async (value, context) =>
                {
                    await Task.Delay(10);
                    return value > 0;
                },
                "Value must be positive");
            
            IValidationContext context = ValidationContext.Create();
            int validValue = 42;

            // Act
            string? result = await rule.ValidateAsync(validValue, "Number", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ValidateAsync_WithComplexAsyncLogic_ShouldReturnNull()
        {
            // Arrange
            MustAsyncContextAwareRule<string> rule = new(
                async (email, context) =>
                {
                    await Task.Delay(5);
                    // Simulate complex async validation
                    bool isValidFormat = email.Contains("@");
                    bool isValidDomain = await Task.FromResult(email.EndsWith(".com"));
                    return isValidFormat && isValidDomain;
                },
                "Email format is invalid");
            
            IValidationContext context = ValidationContext.Create();
            string validEmail = "test@example.com";

            // Act
            string? result = await rule.ValidateAsync(validEmail, "Email", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ValidateAsync_WithTaskFromResult_ShouldReturnNull()
        {
            // Arrange
            MustAsyncContextAwareRule<string> rule = new(
                (value, context) => Task.FromResult(value.Length >= 5),
                "Value too short");
            
            IValidationContext context = ValidationContext.Create();
            string validValue = "ValidLength";

            // Act
            string? result = await rule.ValidateAsync(validValue, "Field", context);

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion Valid Condition Tests

    #region Invalid Condition Tests

    /// <summary>
    /// Tests for async conditions that return false (invalid scenarios)
    /// </summary>
    public class InvalidConditionTests
    {
        [Fact]
        public async Task ValidateAsync_WithConditionReturningFalse_ShouldReturnErrorMessage()
        {
            // Arrange
            string expectedError = "Value is invalid";
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    await Task.Delay(1);
                    return false; // Always invalid
                },
                expectedError);
            
            IValidationContext context = ValidationContext.Create();
            string testValue = "test";

            // Act
            string? result = await rule.ValidateAsync(testValue, "Field", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedError);
        }

        [Fact]
        public async Task ValidateAsync_WithFailingAsyncCondition_ShouldReturnErrorMessage()
        {
            // Arrange
            string expectedError = "Email is already taken";
            MustAsyncContextAwareRule<string> rule = new(
                async (email, context) =>
                {
                    await Task.Delay(10);
                    // Simulate email already exists check
                    return !email.Equals("existing@example.com", StringComparison.OrdinalIgnoreCase);
                },
                expectedError);
            
            IValidationContext context = ValidationContext.Create();
            string existingEmail = "existing@example.com";

            // Act
            string? result = await rule.ValidateAsync(existingEmail, "Email", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedError);
        }

        [Fact]
        public async Task ValidateAsync_WithComplexFailingCondition_ShouldReturnErrorMessage()
        {
            // Arrange
            string expectedError = "Age validation failed";
            MustAsyncContextAwareRule<int> rule = new(
                async (age, context) =>
                {
                    await Task.Delay(5);
                    bool isAdult = age >= 18;
                    bool isSenior = age >= 65;
                    bool isValidRange = age <= 120;
                    
                    // Complex business logic
                    return isAdult && !isSenior && isValidRange;
                },
                expectedError);
            
            IValidationContext context = ValidationContext.Create();
            int invalidAge = 70; // Senior age that should fail

            // Act
            string? result = await rule.ValidateAsync(invalidAge, "Age", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedError);
        }

        [Theory]
        [InlineData("Custom error 1")]
        [InlineData("Validation failed with specific reason")]
        [InlineData("Business rule violation detected")]
        public async Task ValidateAsync_WithDifferentErrorMessages_ShouldReturnCorrectMessage(string errorMessage)
        {
            // Arrange
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    await Task.Delay(1);
                    return false;
                },
                errorMessage);
            
            IValidationContext context = ValidationContext.Create();

            // Act
            string? result = await rule.ValidateAsync("test", "Field", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(errorMessage);
        }
    }

    #endregion Invalid Condition Tests

    #region Context Access Tests

    /// <summary>
    /// Tests for accessing validation context within async conditions
    /// </summary>
    public class ContextAccessTests
    {
        [Fact]
        public async Task ValidateAsync_WithRootObjectAccess_ShouldUseRootObjectInValidation()
        {
            // Arrange
            TestUser user = new() { Email = "test@example.com", Username = "testuser" };
            MustAsyncContextAwareRule<string> rule = new(
                async (email, context) =>
                {
                    await Task.Delay(1);
                    TestUser? rootUser = context.GetRootObject<TestUser>();
                    return rootUser != null && !email.Equals(rootUser.Username, StringComparison.OrdinalIgnoreCase);
                },
                "Email cannot be the same as username");
            
            IValidationContext context = ValidationContext.Create(user);
            string differentEmail = "different@example.com";

            // Act
            string? result = await rule.ValidateAsync(differentEmail, "Email", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ValidateAsync_WithRootObjectAccessFailing_ShouldReturnErrorMessage()
        {
            // Arrange
            TestUser user = new() { Email = "test@example.com", Username = "test@example.com" };
            MustAsyncContextAwareRule<string> rule = new(
                async (email, context) =>
                {
                    await Task.Delay(1);
                    TestUser? rootUser = context.GetRootObject<TestUser>();
                    return rootUser == null || !email.Equals(rootUser.Username, StringComparison.OrdinalIgnoreCase);
                },
                "Email cannot be the same as username");
            
            IValidationContext context = ValidationContext.Create(user);
            string sameAsUsername = "test@example.com";

            // Act
            string? result = await rule.ValidateAsync(sameAsUsername, "Email", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Email cannot be the same as username");
        }

        [Fact]
        public async Task ValidateAsync_WithServiceAccess_ShouldUseServiceInValidation()
        {
            // Arrange
            MockTestValidationService mockService = new() { ShouldReturnValid = true };
            IServiceProvider serviceProvider = CreateServiceProvider(mockService);
            
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    ITestValidationService? service = context.GetService<ITestValidationService>();
                    if (service != null)
                    {
                        return await service.ValidateAsync(value);
                    }
                    return false;
                },
                "Service validation failed");
            
            IValidationContext context = ValidationContext.Create(serviceProvider: serviceProvider);
            string testValue = "test";

            // Act
            string? result = await rule.ValidateAsync(testValue, "Field", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ValidateAsync_WithServiceAccessFailing_ShouldReturnErrorMessage()
        {
            // Arrange
            MockTestValidationService mockService = new() { ShouldReturnValid = false };
            IServiceProvider serviceProvider = CreateServiceProvider(mockService);
            
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    ITestValidationService? service = context.GetService<ITestValidationService>();
                    if (service != null)
                    {
                        return await service.ValidateAsync(value);
                    }
                    return false;
                },
                "Service validation failed");
            
            IValidationContext context = ValidationContext.Create(serviceProvider: serviceProvider);
            string testValue = "test";

            // Act
            string? result = await rule.ValidateAsync(testValue, "Field", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Service validation failed");
        }

        [Fact]
        public async Task ValidateAsync_WithCustomDataAccess_ShouldUseCustomDataInValidation()
        {
            // Arrange
            MustAsyncContextAwareRule<int> rule = new(
                async (age, context) =>
                {
                    await Task.Delay(1);
                    int maxAge = context.GetCustomData<int>("MaxAllowedAge");
                    return maxAge == 0 || age <= maxAge;
                },
                "Age exceeds maximum allowed limit");
            
            IValidationContext context = ValidationContext.Create();
            context.SetCustomData("MaxAllowedAge", 65);
            int validAge = 50;

            // Act
            string? result = await rule.ValidateAsync(validAge, "Age", context);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ValidateAsync_WithCustomDataAccessFailing_ShouldReturnErrorMessage()
        {
            // Arrange
            MustAsyncContextAwareRule<int> rule = new(
                async (age, context) =>
                {
                    await Task.Delay(1);
                    int maxAge = context.GetCustomData<int>("MaxAllowedAge");
                    return maxAge == 0 || age <= maxAge;
                },
                "Age exceeds maximum allowed limit");
            
            IValidationContext context = ValidationContext.Create();
            context.SetCustomData("MaxAllowedAge", 65);
            int invalidAge = 70;

            // Act
            string? result = await rule.ValidateAsync(invalidAge, "Age", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe("Age exceeds maximum allowed limit");
        }

        [Fact]
        public async Task ValidateAsync_WithCombinedContextAccess_ShouldUseAllContextFeatures()
        {
            // Arrange
            TestUser user = new() { Age = 25, Username = "testuser" };
            MockTestValidationService mockService = new() { ShouldReturnValid = true };
            IServiceProvider serviceProvider = CreateServiceProvider(mockService);
            
            MustAsyncContextAwareRule<string> rule = new(
                async (email, context) =>
                {
                    await Task.Delay(1);
                    TestUser? rootUser = context.GetRootObject<TestUser>();
                    ITestValidationService? service = context.GetService<ITestValidationService>();
                    bool strictMode = context.GetCustomData<bool>("StrictMode");
                    
                    if (rootUser != null && service != null)
                    {
                        bool basicValidation = await service.ValidateAsync(email);
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
            string? result = await rule.ValidateAsync(validEmail, "Email", context);

            // Assert
            result.ShouldBeNull();
        }
    }

    #endregion Context Access Tests

    #region Exception Handling Tests

    /// <summary>
    /// Tests for exception handling within async conditions
    /// </summary>
    public class ExceptionHandlingTests
    {
        [Fact]
        public async Task ValidateAsync_WithConditionThrowingException_ShouldReturnErrorWithExceptionMessage()
        {
            // Arrange
            string originalErrorMessage = "Original validation error";
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    await Task.Delay(1);
                    throw new InvalidOperationException("Something went wrong");
                },
                originalErrorMessage);
            
            IValidationContext context = ValidationContext.Create();

            // Act
            string? result = await rule.ValidateAsync("test", "Field", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldStartWith("Async validation error:");
            result.ShouldContain("Something went wrong");
        }

        [Fact]
        public async Task ValidateAsync_WithServiceThrowingException_ShouldReturnErrorMessage()
        {
            // Arrange
            MockTestValidationService mockService = new() 
            { 
                ShouldThrowException = true, 
                ExceptionToThrow = new TimeoutException("Service timeout")
            };
            IServiceProvider serviceProvider = CreateServiceProvider(mockService);
            
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    ITestValidationService? service = context.GetService<ITestValidationService>();
                    if (service != null)
                    {
                        return await service.ValidateAsync(value);
                    }
                    return false;
                },
                "Service validation failed");
            
            IValidationContext context = ValidationContext.Create(serviceProvider: serviceProvider);

            // Act
            string? result = await rule.ValidateAsync("test", "Field", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldStartWith("Async validation error:");
            result.ShouldContain("Service timeout");
        }

        [Fact]
        public async Task ValidateAsync_WithArgumentNullExceptionInCondition_ShouldRethrowException()
        {
            // Arrange
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    await Task.Delay(1);
                    throw new ArgumentNullException("testParam", "Parameter cannot be null");
                },
                "Validation error");
            
            IValidationContext context = ValidationContext.Create();

            // Act & Assert
            ArgumentNullException exception = await Should.ThrowAsync<ArgumentNullException>(
                async () => await rule.ValidateAsync("test", "Field", context));
            exception.ParamName.ShouldBe("testParam");
        }

        [Fact]
        public async Task ValidateAsync_WithTaskCanceledExceptionInCondition_ShouldRethrowException()
        {
            // Arrange
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    await Task.Delay(1);
                    throw new TaskCanceledException("Task was canceled");
                },
                "Validation error");
            
            IValidationContext context = ValidationContext.Create();

            // Act & Assert
            TaskCanceledException exception = await Should.ThrowAsync<TaskCanceledException>(
                async () => await rule.ValidateAsync("test", "Field", context));
            exception.Message.ShouldBe("A task was canceled.");
        }

        [Fact]
        public async Task ValidateAsync_WithOperationCanceledExceptionInCondition_ShouldRethrowException()
        {
            // Arrange
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    await Task.Delay(1);
                    throw new OperationCanceledException("Operation was canceled");
                },
                "Validation error");
            
            IValidationContext context = ValidationContext.Create();

            // Act & Assert
            OperationCanceledException exception = await Should.ThrowAsync<OperationCanceledException>(
                async () => await rule.ValidateAsync("test", "Field", context));
            exception.Message.ShouldBe("A task was canceled.");
        }

        [Theory]
        [InlineData(typeof(InvalidDataException), "Invalid data provided")]
        [InlineData(typeof(NotSupportedException), "Operation not supported")]
        [InlineData(typeof(TimeoutException), "Operation timed out")]
        public async Task ValidateAsync_WithVariousExceptionTypes_ShouldReturnErrorWithExceptionMessage(Type exceptionType, string exceptionMessage)
        {
            // Arrange
            Exception exceptionToThrow = (Exception)Activator.CreateInstance(exceptionType, exceptionMessage)!;
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    await Task.Delay(1);
                    throw exceptionToThrow;
                },
                "Original error");
            
            IValidationContext context = ValidationContext.Create();

            // Act
            string? result = await rule.ValidateAsync("test", "Field", context);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldStartWith("Async validation error:");
            result.ShouldContain(exceptionMessage);
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
        public async Task ValidateAsync_WithNullContext_ShouldThrowArgumentNullException()
        {
            // Arrange
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    await Task.Delay(1);
                    return true;
                },
                "Error message");
            
            IValidationContext? nullContext = null;

            // Act & Assert
            ArgumentNullException exception = await Should.ThrowAsync<ArgumentNullException>(
                async () => await rule.ValidateAsync("test", "Field", nullContext!));
            exception.ParamName.ShouldBe("context");
        }
    }

    #endregion Null Context Tests

    #region Async Operation Tests

    /// <summary>
    /// Tests for various async operation scenarios
    /// </summary>
    public class AsyncOperationTests
    {
        [Fact]
        public async Task ValidateAsync_WithDelayInCondition_ShouldWaitForCompletion()
        {
            // Arrange
            bool conditionExecuted = false;
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    await Task.Delay(50);
                    conditionExecuted = true;
                    return !string.IsNullOrEmpty(value);
                },
                "Value is required");
            
            IValidationContext context = ValidationContext.Create();
            DateTime startTime = DateTime.UtcNow;

            // Act
            string? result = await rule.ValidateAsync("test", "Field", context);
            DateTime endTime = DateTime.UtcNow;

            // Assert
            result.ShouldBeNull();
            conditionExecuted.ShouldBeTrue();
            (endTime - startTime).TotalMilliseconds.ShouldBeGreaterThanOrEqualTo(45);
        }

        [Fact]
        public async Task ValidateAsync_WithConcurrentValidations_ShouldHandleCorrectly()
        {
            // Arrange
            int executionCount = 0;
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    await Task.Delay(10);
                    Interlocked.Increment(ref executionCount);
                    return !string.IsNullOrEmpty(value);
                },
                "Value is required");
            
            IValidationContext context = ValidationContext.Create();
            Task<string?>[] validationTasks = new Task<string?>[5];
            
            // Act
            for (int i = 0; i < validationTasks.Length; i++)
            {
                string testValue = $"test{i}";
                validationTasks[i] = rule.ValidateAsync(testValue, "Field", context);
            }
            
            string?[] results = await Task.WhenAll(validationTasks);

            // Assert
            results.ShouldAllBe(r => r == null);
            executionCount.ShouldBe(5);
        }

        [Fact]
        public async Task ValidateAsync_WithServiceWithDelay_ShouldWorkCorrectly()
        {
            // Arrange
            MockTestValidationService mockService = new() { ShouldReturnValid = true };
            IServiceProvider serviceProvider = CreateServiceProvider(mockService);
            
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    ITestValidationService? service = context.GetService<ITestValidationService>();
                    if (service != null)
                    {
                        return await service.ValidateWithDelayAsync(value, 25);
                    }
                    return false;
                },
                "Service validation failed");
            
            IValidationContext context = ValidationContext.Create(serviceProvider: serviceProvider);
            DateTime startTime = DateTime.UtcNow;

            // Act
            string? result = await rule.ValidateAsync("test", "Field", context);
            DateTime endTime = DateTime.UtcNow;

            // Assert
            result.ShouldBeNull();
            (endTime - startTime).TotalMilliseconds.ShouldBeGreaterThanOrEqualTo(20);
        }
    }

    #endregion Async Operation Tests

    #region Multiple Validation Tests

    /// <summary>
    /// Tests for multiple validation scenarios
    /// </summary>
    public class MultipleValidationTests
    {
        [Fact]
        public async Task ValidateAsync_SameRuleMultipleTimes_ShouldProduceConsistentResults()
        {
            // Arrange
            MustAsyncContextAwareRule<string> rule = new(
                async (value, context) =>
                {
                    await Task.Delay(1);
                    return value.Length >= 5;
                },
                "Value too short");
            
            IValidationContext context = ValidationContext.Create();

            // Act
            string? result1 = await rule.ValidateAsync("Valid", "Field1", context);
            string? result2 = await rule.ValidateAsync("Hi", "Field2", context);
            string? result3 = await rule.ValidateAsync("AlsoValid", "Field3", context);

            // Assert
            result1.ShouldBeNull();
            result2.ShouldNotBeNull();
            result2.ShouldBe("Value too short");
            result3.ShouldBeNull();
        }

        [Fact]
        public async Task ValidateAsync_DifferentRuleInstances_ShouldWorkIndependently()
        {
            // Arrange
            MustAsyncContextAwareRule<int> rule1 = new(
                async (value, context) =>
                {
                    await Task.Delay(1);
                    return value > 10;
                },
                "Must be greater than 10");
                
            MustAsyncContextAwareRule<int> rule2 = new(
                async (value, context) =>
                {
                    await Task.Delay(1);
                    return value < 100;
                },
                "Must be less than 100");
            
            IValidationContext context = ValidationContext.Create();
            int testValue = 50;

            // Act
            string? result1 = await rule1.ValidateAsync(testValue, "Field", context);
            string? result2 = await rule2.ValidateAsync(testValue, "Field", context);

            // Assert
            result1.ShouldBeNull();
            result2.ShouldBeNull();
        }

        [Fact]
        public async Task ValidateAsync_WithDifferentValueTypes_ShouldWorkCorrectly()
        {
            // Arrange
            MustAsyncContextAwareRule<TestUser> userRule = new(
                async (user, context) =>
                {
                    await Task.Delay(1);
                    return user != null && !string.IsNullOrEmpty(user.Email);
                },
                "User must have email");
                
            MustAsyncContextAwareRule<decimal> decimalRule = new(
                async (value, context) =>
                {
                    await Task.Delay(1);
                    return value >= 0;
                },
                "Value must be non-negative");
            
            IValidationContext context = ValidationContext.Create();
            TestUser validUser = new() { Email = "test@example.com" };
            decimal validDecimal = 42.50m;

            // Act
            string? userResult = await userRule.ValidateAsync(validUser, "User", context);
            string? decimalResult = await decimalRule.ValidateAsync(validDecimal, "Amount", context);

            // Assert
            userResult.ShouldBeNull();
            decimalResult.ShouldBeNull();
        }
    }

    #endregion Multiple Validation Tests

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