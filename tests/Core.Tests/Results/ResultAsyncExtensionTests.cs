using System.Security;
using FlowRight.Core.Extensions;
using FlowRight.Core.Results;
using Shouldly;

namespace FlowRight.Core.Tests.Results;

/// <summary>
/// Comprehensive tests for async-friendly extension methods for the Result class.
/// These tests verify proper async behavior, exception handling, and result composition
/// across all method overloads and failure scenarios.
/// </summary>
public class ResultAsyncExtensionTests
{
    #region MatchAsync Extensions for Result Tests

    public class MatchAsyncForResult
    {
        [Fact]
        public async Task MatchAsync_WithSuccessResult_ShouldCallOnSuccessHandler()
        {
            // Arrange
            Result result = Result.Success();
            string expectedValue = "Success executed";

            // Act
            string actualValue = await result.MatchAsync(
                onSuccess: async () =>
                {
                    await Task.Delay(1);
                    return expectedValue;
                },
                onFailure: async error => await Task.FromResult($"Failed: {error}"));

            // Assert
            actualValue.ShouldBe(expectedValue);
        }

        [Fact]
        public async Task MatchAsync_WithFailureResult_ShouldCallOnFailureHandler()
        {
            // Arrange
            string errorMessage = "Operation failed";
            Result result = Result.Failure(errorMessage);

            // Act
            string actualValue = await result.MatchAsync(
                onSuccess: async () => await Task.FromResult("Success"),
                onFailure: async error => await Task.FromResult($"Failed: {error}"));

            // Assert
            actualValue.ShouldBe($"Failed: {errorMessage}");
        }

        [Fact]
        public async Task MatchAsync_WithNullResult_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = null!;

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MatchAsync<string>(
                    onSuccess: async () => await Task.FromResult("success"),
                    onFailure: async _ => await Task.FromResult("failure")));
        }

        [Fact]
        public async Task MatchAsync_WithNullOnSuccessHandler_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = Result.Success();

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MatchAsync<string>(
                    onSuccess: null!,
                    onFailure: async _ => await Task.FromResult("failure")));
        }

        [Fact]
        public async Task MatchAsync_WithNullOnFailureHandler_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = Result.Success();

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MatchAsync(
                    onSuccess: async () => await Task.FromResult("success"),
                    onFailure: null!));
        }

        [Fact]
        public async Task MatchAsync_WithSpecificFailureTypes_ShouldRouteToCorrectHandler()
        {
            // Arrange
            Result errorResult = Result.Failure("General error");
            Result securityResult = Result.Failure(new SecurityException("Security violation"));
            IDictionary<string, string[]> validationErrors = new Dictionary<string, string[]>
            {
                { "Field1", new[] { "Error1", "Error2" } },
                { "Field2", new[] { "Error3" } }
            };
            Result validationResult = Result.ValidationFailure(validationErrors);
            Result cancelledResult = Result.Failure(new OperationCanceledException("Operation cancelled"));

            // Act & Assert - Error
            string errorResponse = await errorResult.MatchAsync(
                onSuccess: async () => await Task.FromResult("Success"),
                onError: async error => await Task.FromResult($"Error: {error}"),
                onSecurityException: async error => await Task.FromResult($"Security: {error}"),
                onValidationException: async errors => await Task.FromResult($"Validation: {errors.Count}"),
                onOperationCanceledException: async error => await Task.FromResult($"Cancelled: {error}"));

            errorResponse.ShouldBe("Error: General error");

            // Act & Assert - Security
            string securityResponse = await securityResult.MatchAsync(
                onSuccess: async () => await Task.FromResult("Success"),
                onError: async error => await Task.FromResult($"Error: {error}"),
                onSecurityException: async error => await Task.FromResult($"Security: {error}"),
                onValidationException: async errors => await Task.FromResult($"Validation: {errors.Count}"),
                onOperationCanceledException: async error => await Task.FromResult($"Cancelled: {error}"));

            securityResponse.ShouldBe("Security: Security violation");

            // Act & Assert - Validation
            string validationResponse = await validationResult.MatchAsync(
                onSuccess: async () => await Task.FromResult("Success"),
                onError: async error => await Task.FromResult($"Error: {error}"),
                onSecurityException: async error => await Task.FromResult($"Security: {error}"),
                onValidationException: async errors => await Task.FromResult($"Validation: {errors.Count}"),
                onOperationCanceledException: async error => await Task.FromResult($"Cancelled: {error}"));

            validationResponse.ShouldBe("Validation: 2");

            // Act & Assert - Operation Cancelled
            string cancelledResponse = await cancelledResult.MatchAsync(
                onSuccess: async () => await Task.FromResult("Success"),
                onError: async error => await Task.FromResult($"Error: {error}"),
                onSecurityException: async error => await Task.FromResult($"Security: {error}"),
                onValidationException: async errors => await Task.FromResult($"Validation: {errors.Count}"),
                onOperationCanceledException: async error => await Task.FromResult($"Cancelled: {error}"));

            cancelledResponse.ShouldBe("Cancelled: Operation cancelled");
        }

        [Fact]
        public async Task MatchAsync_WithSpecificHandlers_AndNullResult_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = null!;

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MatchAsync<string>(
                    onSuccess: async () => await Task.FromResult("success"),
                    onError: async _ => await Task.FromResult("error"),
                    onSecurityException: async _ => await Task.FromResult("security"),
                    onValidationException: async _ => await Task.FromResult("validation"),
                    onOperationCanceledException: async _ => await Task.FromResult("cancelled")));
        }

        [Fact]
        public async Task MatchAsync_WithSpecificHandlers_AndNullOnSuccessHandler_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = Result.Success();

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MatchAsync<string>(
                    onSuccess: null!,
                    onError: async _ => await Task.FromResult("error"),
                    onSecurityException: async _ => await Task.FromResult("security"),
                    onValidationException: async _ => await Task.FromResult("validation"),
                    onOperationCanceledException: async _ => await Task.FromResult("cancelled")));
        }

        [Fact]
        public async Task MatchAsync_WithSpecificHandlers_AndNullOnErrorHandler_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = Result.Success();

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MatchAsync<string>(
                    onSuccess: async () => await Task.FromResult("success"),
                    onError: null!,
                    onSecurityException: async _ => await Task.FromResult("security"),
                    onValidationException: async _ => await Task.FromResult("validation"),
                    onOperationCanceledException: async _ => await Task.FromResult("cancelled")));
        }

        [Fact]
        public async Task MatchAsync_WithSpecificHandlers_AndNullOnSecurityHandler_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = Result.Success();

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MatchAsync<string>(
                    onSuccess: async () => await Task.FromResult("success"),
                    onError: async _ => await Task.FromResult("error"),
                    onSecurityException: null!,
                    onValidationException: async _ => await Task.FromResult("validation"),
                    onOperationCanceledException: async _ => await Task.FromResult("cancelled")));
        }

        [Fact]
        public async Task MatchAsync_WithSpecificHandlers_AndNullOnValidationHandler_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = Result.Success();

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MatchAsync<string>(
                    onSuccess: async () => await Task.FromResult("success"),
                    onError: async _ => await Task.FromResult("error"),
                    onSecurityException: async _ => await Task.FromResult("security"),
                    onValidationException: null!,
                    onOperationCanceledException: async _ => await Task.FromResult("cancelled")));
        }

        [Fact]
        public async Task MatchAsync_WithSpecificHandlers_AndNullOnOperationCancelledHandler_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = Result.Success();

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MatchAsync<string>(
                    onSuccess: async () => await Task.FromResult("success"),
                    onError: async _ => await Task.FromResult("error"),
                    onSecurityException: async _ => await Task.FromResult("security"),
                    onValidationException: async _ => await Task.FromResult("validation"),
                    onOperationCanceledException: null!));
        }

        [Fact]
        public async Task MatchAsync_WithUnknownFailureType_ShouldThrowNotImplementedException()
        {
            // Arrange
            // This will require creating a Result with an unknown failure type - which should be impossible
            // in the current implementation, but we'll test the defensive coding path
            // This test would need to be implemented if there's a way to inject an unknown failure type
            // For now, this is marked as a failing test until the implementation is examined

            // Act & Assert
            // This test is designed to fail until we can create a scenario with unknown FailureType
            true.ShouldBeFalse("Test not yet implemented - need to create Result with unknown FailureType");
        }
    }

    #endregion MatchAsync Extensions for Result Tests

    #region MatchAsync Extensions for Result<T> Tests

    public class MatchAsyncForResultT
    {
        [Fact]
        public async Task MatchAsync_WithSuccessResult_ShouldCallOnSuccessHandler()
        {
            // Arrange
            int value = 42;
            Result<int> result = Result.Success(value);

            // Act
            string actualValue = await result.MatchAsync(
                onSuccess: async val => await Task.FromResult($"Success: {val}"),
                onFailure: async error => await Task.FromResult($"Failed: {error}"));

            // Assert
            actualValue.ShouldBe($"Success: {value}");
        }

        [Fact]
        public async Task MatchAsync_WithFailureResult_ShouldCallOnFailureHandler()
        {
            // Arrange
            string errorMessage = "Operation failed";
            Result<int> result = Result.Failure<int>(errorMessage);

            // Act
            string actualValue = await result.MatchAsync(
                onSuccess: async val => await Task.FromResult($"Success: {val}"),
                onFailure: async error => await Task.FromResult($"Failed: {error}"));

            // Assert
            actualValue.ShouldBe($"Failed: {errorMessage}");
        }

        [Fact]
        public async Task MatchAsync_WithSuccessResultButTryGetValueFails_ShouldCallOnFailureHandler()
        {
            // Arrange
            // This test is designed to fail initially as it requires a scenario where
            // IsSuccess is true but TryGetValue fails - need to investigate if this is possible
            // Act & Assert
            true.ShouldBeFalse("Test not yet implemented - need scenario where IsSuccess=true but TryGetValue fails");
        }

        [Fact]
        public async Task MatchAsync_WithNullResult_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result<int> result = null!;

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MatchAsync(
                    onSuccess: async _ => await Task.FromResult("success"),
                    onFailure: async _ => await Task.FromResult("failure")));
        }

        [Fact]
        public async Task MatchAsync_WithNullOnSuccessHandler_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result<int> result = Result.Success(42);

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MatchAsync(
                    onSuccess: null!,
                    onFailure: async _ => await Task.FromResult("failure")));
        }

        [Fact]
        public async Task MatchAsync_WithNullOnFailureHandler_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result<int> result = Result.Success(42);

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MatchAsync(
                    onSuccess: async val => await Task.FromResult($"Success: {val}"),
                    onFailure: null!));
        }

        [Fact]
        public async Task MatchAsync_WithSpecificFailureTypes_ShouldRouteToCorrectHandler()
        {
            // Arrange
            Result<string> errorResult = Result.Failure<string>("General error");
            Result<string> securityResult = Result.Failure<string>(new SecurityException("Security violation"));
            IDictionary<string, string[]> validationErrors = new Dictionary<string, string[]>
            {
                { "Field1", new[] { "Error1" } }
            };
            Result<string> validationResult = Result.Failure<string>(validationErrors);
            Result<string> cancelledResult = Result.Failure<string>(new OperationCanceledException("Operation cancelled"));

            // Act & Assert - Error
            string errorResponse = await errorResult.MatchAsync(
                onSuccess: async val => await Task.FromResult($"Success: {val}"),
                onError: async error => await Task.FromResult($"Error: {error}"),
                onSecurityException: async error => await Task.FromResult($"Security: {error}"),
                onValidationException: async errors => await Task.FromResult($"Validation: {errors.Count}"),
                onOperationCanceledException: async error => await Task.FromResult($"Cancelled: {error}"));

            errorResponse.ShouldBe("Error: General error");

            // Act & Assert - Security
            string securityResponse = await securityResult.MatchAsync(
                onSuccess: async val => await Task.FromResult($"Success: {val}"),
                onError: async error => await Task.FromResult($"Error: {error}"),
                onSecurityException: async error => await Task.FromResult($"Security: {error}"),
                onValidationException: async errors => await Task.FromResult($"Validation: {errors.Count}"),
                onOperationCanceledException: async error => await Task.FromResult($"Cancelled: {error}"));

            securityResponse.ShouldBe("Security: Security violation");

            // Act & Assert - Validation
            string validationResponse = await validationResult.MatchAsync(
                onSuccess: async val => await Task.FromResult($"Success: {val}"),
                onError: async error => await Task.FromResult($"Error: {error}"),
                onSecurityException: async error => await Task.FromResult($"Security: {error}"),
                onValidationException: async errors => await Task.FromResult($"Validation: {errors.Count}"),
                onOperationCanceledException: async error => await Task.FromResult($"Cancelled: {error}"));

            validationResponse.ShouldBe("Validation: 1");

            // Act & Assert - Operation Cancelled
            string cancelledResponse = await cancelledResult.MatchAsync(
                onSuccess: async val => await Task.FromResult($"Success: {val}"),
                onError: async error => await Task.FromResult($"Error: {error}"),
                onSecurityException: async error => await Task.FromResult($"Security: {error}"),
                onValidationException: async errors => await Task.FromResult($"Validation: {errors.Count}"),
                onOperationCanceledException: async error => await Task.FromResult($"Cancelled: {error}"));

            cancelledResponse.ShouldBe("Cancelled: Operation cancelled");
        }

        [Fact]
        public async Task MatchAsync_WithSpecificHandlers_AndTryGetValueFailsOnSuccessResult_ShouldCallOnFailureHandler()
        {
            // Arrange
            // This test is designed to fail initially - need to create scenario where TryGetValue fails on success
            // Act & Assert
            true.ShouldBeFalse("Test not yet implemented - need scenario where success result has TryGetValue failure");
        }

        [Fact]
        public async Task MatchAsync_WithUnknownFailureType_ShouldThrowNotImplementedException()
        {
            // Arrange
            // This test is designed to fail initially - need way to create unknown failure type
            // Act & Assert
            true.ShouldBeFalse("Test not yet implemented - need to create Result<T> with unknown FailureType");
        }
    }

    #endregion MatchAsync Extensions for Result<T> Tests

    #region SwitchAsync Extensions for Result Tests

    public class SwitchAsyncForResult
    {
        [Fact]
        public async Task SwitchAsync_WithSuccessResult_ShouldCallOnSuccessHandler()
        {
            // Arrange
            Result result = Result.Success();
            bool onSuccessCalled = false;
            bool onFailureCalled = false;

            // Act
            await result.SwitchAsync(
                onSuccess: async () =>
                {
                    await Task.Delay(1);
                    onSuccessCalled = true;
                },
                onFailure: async error =>
                {
                    await Task.Delay(1);
                    onFailureCalled = true;
                });

            // Assert
            onSuccessCalled.ShouldBeTrue();
            onFailureCalled.ShouldBeFalse();
        }

        [Fact]
        public async Task SwitchAsync_WithFailureResult_ShouldCallOnFailureHandler()
        {
            // Arrange
            Result result = Result.Failure("Error message");
            bool onSuccessCalled = false;
            bool onFailureCalled = false;

            // Act
            await result.SwitchAsync(
                onSuccess: async () =>
                {
                    await Task.Delay(1);
                    onSuccessCalled = true;
                },
                onFailure: async error =>
                {
                    await Task.Delay(1);
                    onFailureCalled = true;
                });

            // Assert
            onSuccessCalled.ShouldBeFalse();
            onFailureCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task SwitchAsync_WithOperationCancelledAndIncludeOperationCancelledFalse_ShouldNotCallAnyHandler()
        {
            // Arrange
            Result result = Result.Failure(new OperationCanceledException("Cancelled"));
            bool onSuccessCalled = false;
            bool onFailureCalled = false;

            // Act
            await result.SwitchAsync(
                onSuccess: async () =>
                {
                    await Task.Delay(1);
                    onSuccessCalled = true;
                },
                onFailure: async error =>
                {
                    await Task.Delay(1);
                    onFailureCalled = true;
                },
                includeOperationCancelledFailures: false);

            // Assert
            onSuccessCalled.ShouldBeFalse();
            onFailureCalled.ShouldBeFalse();
        }

        [Fact]
        public async Task SwitchAsync_WithOperationCancelledAndIncludeOperationCancelledTrue_ShouldCallOnFailureHandler()
        {
            // Arrange
            Result result = Result.Failure(new OperationCanceledException("Cancelled"));
            bool onSuccessCalled = false;
            bool onFailureCalled = false;

            // Act
            await result.SwitchAsync(
                onSuccess: async () =>
                {
                    await Task.Delay(1);
                    onSuccessCalled = true;
                },
                onFailure: async error =>
                {
                    await Task.Delay(1);
                    onFailureCalled = true;
                },
                includeOperationCancelledFailures: true);

            // Assert
            onSuccessCalled.ShouldBeFalse();
            onFailureCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task SwitchAsync_WithNullResult_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = null!;

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.SwitchAsync(
                    onSuccess: async () => await Task.CompletedTask,
                    onFailure: async _ => await Task.CompletedTask));
        }

        [Fact]
        public async Task SwitchAsync_WithNullOnSuccessHandler_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = Result.Success();

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.SwitchAsync(
                    onSuccess: null!,
                    onFailure: async _ => await Task.CompletedTask));
        }

        [Fact]
        public async Task SwitchAsync_WithNullOnFailureHandler_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = Result.Success();

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.SwitchAsync(
                    onSuccess: async () => await Task.CompletedTask,
                    onFailure: null!));
        }

        [Fact]
        public async Task SwitchAsync_WithSpecificFailureHandlers_ShouldRouteToCorrectHandler()
        {
            // Arrange
            Result errorResult = Result.Failure("General error");
            Result securityResult = Result.Failure(new SecurityException("Security violation"));
            IDictionary<string, string[]> validationErrors = new Dictionary<string, string[]>
            {
                { "Field1", new[] { "Error1" } }
            };
            Result validationResult = Result.ValidationFailure(validationErrors);
            Result cancelledResult = Result.Failure(new OperationCanceledException("Operation cancelled"));

            bool errorHandlerCalled = false;
            bool securityHandlerCalled = false;
            bool validationHandlerCalled = false;
            bool cancelledHandlerCalled = false;

            // Act & Assert - Error
            await errorResult.SwitchAsync(
                onSuccess: async () => await Task.CompletedTask,
                onError: async error =>
                {
                    await Task.Delay(1);
                    errorHandlerCalled = true;
                },
                onSecurityException: async error => await Task.CompletedTask,
                onValidationException: async errors => await Task.CompletedTask,
                onOperationCanceledException: async error => await Task.CompletedTask);

            errorHandlerCalled.ShouldBeTrue();

            // Act & Assert - Security
            await securityResult.SwitchAsync(
                onSuccess: async () => await Task.CompletedTask,
                onError: async error => await Task.CompletedTask,
                onSecurityException: async error =>
                {
                    await Task.Delay(1);
                    securityHandlerCalled = true;
                },
                onValidationException: async errors => await Task.CompletedTask,
                onOperationCanceledException: async error => await Task.CompletedTask);

            securityHandlerCalled.ShouldBeTrue();

            // Act & Assert - Validation
            await validationResult.SwitchAsync(
                onSuccess: async () => await Task.CompletedTask,
                onError: async error => await Task.CompletedTask,
                onSecurityException: async error => await Task.CompletedTask,
                onValidationException: async errors =>
                {
                    await Task.Delay(1);
                    validationHandlerCalled = true;
                },
                onOperationCanceledException: async error => await Task.CompletedTask);

            validationHandlerCalled.ShouldBeTrue();

            // Act & Assert - Operation Cancelled with handler
            await cancelledResult.SwitchAsync(
                onSuccess: async () => await Task.CompletedTask,
                onError: async error => await Task.CompletedTask,
                onSecurityException: async error => await Task.CompletedTask,
                onValidationException: async errors => await Task.CompletedTask,
                onOperationCanceledException: async error =>
                {
                    await Task.Delay(1);
                    cancelledHandlerCalled = true;
                });

            cancelledHandlerCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task SwitchAsync_WithSpecificFailureHandlers_AndOperationCancelledWithNullHandler_ShouldNotCallAnyHandler()
        {
            // Arrange
            Result cancelledResult = Result.Failure(new OperationCanceledException("Operation cancelled"));
            bool anyHandlerCalled = false;

            // Act
            await cancelledResult.SwitchAsync(
                onSuccess: async () =>
                {
                    await Task.Delay(1);
                    anyHandlerCalled = true;
                },
                onError: async error =>
                {
                    await Task.Delay(1);
                    anyHandlerCalled = true;
                },
                onSecurityException: async error =>
                {
                    await Task.Delay(1);
                    anyHandlerCalled = true;
                },
                onValidationException: async errors =>
                {
                    await Task.Delay(1);
                    anyHandlerCalled = true;
                },
                onOperationCanceledException: null);

            // Assert
            anyHandlerCalled.ShouldBeFalse();
        }

        [Fact]
        public async Task SwitchAsync_WithUnknownFailureType_ShouldThrowNotImplementedException()
        {
            // Arrange
            // This test is designed to fail initially - need way to create unknown failure type
            // Act & Assert
            true.ShouldBeFalse("Test not yet implemented - need to create Result with unknown FailureType");
        }
    }

    #endregion SwitchAsync Extensions for Result Tests

    #region SwitchAsync Extensions for Result<T> Tests

    public class SwitchAsyncForResultT
    {
        [Fact]
        public async Task SwitchAsync_WithSuccessResult_ShouldCallOnSuccessHandler()
        {
            // Arrange
            int value = 42;
            Result<int> result = Result.Success(value);
            bool onSuccessCalled = false;
            bool onFailureCalled = false;
            int receivedValue = 0;

            // Act
            await result.SwitchAsync(
                onSuccess: async val =>
                {
                    await Task.Delay(1);
                    onSuccessCalled = true;
                    receivedValue = val;
                },
                onFailure: async error =>
                {
                    await Task.Delay(1);
                    onFailureCalled = true;
                });

            // Assert
            onSuccessCalled.ShouldBeTrue();
            onFailureCalled.ShouldBeFalse();
            receivedValue.ShouldBe(value);
        }

        [Fact]
        public async Task SwitchAsync_WithFailureResult_ShouldCallOnFailureHandler()
        {
            // Arrange
            Result<int> result = Result.Failure<int>("Error message");
            bool onSuccessCalled = false;
            bool onFailureCalled = false;

            // Act
            await result.SwitchAsync(
                onSuccess: async val =>
                {
                    await Task.Delay(1);
                    onSuccessCalled = true;
                },
                onFailure: async error =>
                {
                    await Task.Delay(1);
                    onFailureCalled = true;
                });

            // Assert
            onSuccessCalled.ShouldBeFalse();
            onFailureCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task SwitchAsync_WithSuccessResultButTryGetValueFails_ShouldCallOnFailureHandler()
        {
            // Arrange
            // This test is designed to fail initially - need scenario where TryGetValue fails on success
            // Act & Assert
            true.ShouldBeFalse("Test not yet implemented - need scenario where IsSuccess=true but TryGetValue fails");
        }

        [Fact]
        public async Task SwitchAsync_WithErrorFailureType_ShouldCallOnFailureHandler()
        {
            // Arrange
            Result<int> result = Result.Failure<int>("General error");
            bool onSuccessCalled = false;
            bool onFailureCalled = false;

            // Act
            await result.SwitchAsync(
                onSuccess: async val => { onSuccessCalled = true; await Task.CompletedTask; },
                onFailure: async error => { onFailureCalled = true; await Task.CompletedTask; });

            // Assert
            onSuccessCalled.ShouldBeFalse();
            onFailureCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task SwitchAsync_WithSecurityFailureType_ShouldCallOnFailureHandler()
        {
            // Arrange
            Result<int> result = Result.Failure<int>(new SecurityException("Security error"));
            bool onSuccessCalled = false;
            bool onFailureCalled = false;

            // Act
            await result.SwitchAsync(
                onSuccess: async val => { onSuccessCalled = true; await Task.CompletedTask; },
                onFailure: async error => { onFailureCalled = true; await Task.CompletedTask; });

            // Assert
            onSuccessCalled.ShouldBeFalse();
            onFailureCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task SwitchAsync_WithValidationFailureType_ShouldCallOnFailureHandler()
        {
            // Arrange
            IDictionary<string, string[]> validationErrors = new Dictionary<string, string[]>
            {
                { "Field1", new[] { "Error1" } }
            };
            Result<int> result = Result.Failure<int>(validationErrors);
            bool onSuccessCalled = false;
            bool onFailureCalled = false;

            // Act
            await result.SwitchAsync(
                onSuccess: async val => { onSuccessCalled = true; await Task.CompletedTask; },
                onFailure: async error => { onFailureCalled = true; await Task.CompletedTask; });

            // Assert
            onSuccessCalled.ShouldBeFalse();
            onFailureCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task SwitchAsync_WithOperationCancelledAndIncludeOperationCancelledFalse_ShouldNotCallAnyHandler()
        {
            // Arrange
            Result<int> result = Result.Failure<int>(new OperationCanceledException("Cancelled"));
            bool onSuccessCalled = false;
            bool onFailureCalled = false;

            // Act
            await result.SwitchAsync(
                onSuccess: async val => { onSuccessCalled = true; await Task.CompletedTask; },
                onFailure: async error => { onFailureCalled = true; await Task.CompletedTask; },
                includeOperationCancelledFailures: false);

            // Assert
            onSuccessCalled.ShouldBeFalse();
            onFailureCalled.ShouldBeFalse();
        }

        [Fact]
        public async Task SwitchAsync_WithOperationCancelledAndIncludeOperationCancelledTrue_ShouldCallOnFailureHandler()
        {
            // Arrange
            Result<int> result = Result.Failure<int>(new OperationCanceledException("Cancelled"));
            bool onSuccessCalled = false;
            bool onFailureCalled = false;

            // Act
            await result.SwitchAsync(
                onSuccess: async val => { onSuccessCalled = true; await Task.CompletedTask; },
                onFailure: async error => { onFailureCalled = true; await Task.CompletedTask; },
                includeOperationCancelledFailures: true);

            // Assert
            onSuccessCalled.ShouldBeFalse();
            onFailureCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task SwitchAsync_WithSpecificFailureHandlers_ShouldRouteToCorrectHandler()
        {
            // Arrange
            Result<string> errorResult = Result.Failure<string>("General error");
            Result<string> securityResult = Result.Failure<string>(new SecurityException("Security violation"));
            IDictionary<string, string[]> validationErrors = new Dictionary<string, string[]>
            {
                { "Field1", new[] { "Error1" } }
            };
            Result<string> validationResult = Result.Failure<string>(validationErrors);
            Result<string> cancelledResult = Result.Failure<string>(new OperationCanceledException("Operation cancelled"));

            bool errorHandlerCalled = false;
            bool securityHandlerCalled = false;
            bool validationHandlerCalled = false;
            bool cancelledHandlerCalled = false;

            // Act & Assert - Error
            await errorResult.SwitchAsync(
                onSuccess: async val => await Task.CompletedTask,
                onError: async error => { errorHandlerCalled = true; await Task.CompletedTask; },
                onSecurityException: async error => await Task.CompletedTask,
                onValidationException: async errors => await Task.CompletedTask,
                onOperationCanceledException: async error => await Task.CompletedTask);

            errorHandlerCalled.ShouldBeTrue();

            // Act & Assert - Security
            await securityResult.SwitchAsync(
                onSuccess: async val => await Task.CompletedTask,
                onError: async error => await Task.CompletedTask,
                onSecurityException: async error => { securityHandlerCalled = true; await Task.CompletedTask; },
                onValidationException: async errors => await Task.CompletedTask,
                onOperationCanceledException: async error => await Task.CompletedTask);

            securityHandlerCalled.ShouldBeTrue();

            // Act & Assert - Validation
            await validationResult.SwitchAsync(
                onSuccess: async val => await Task.CompletedTask,
                onError: async error => await Task.CompletedTask,
                onSecurityException: async error => await Task.CompletedTask,
                onValidationException: async errors => { validationHandlerCalled = true; await Task.CompletedTask; },
                onOperationCanceledException: async error => await Task.CompletedTask);

            validationHandlerCalled.ShouldBeTrue();

            // Act & Assert - Operation Cancelled with handler
            await cancelledResult.SwitchAsync(
                onSuccess: async val => await Task.CompletedTask,
                onError: async error => await Task.CompletedTask,
                onSecurityException: async error => await Task.CompletedTask,
                onValidationException: async errors => await Task.CompletedTask,
                onOperationCanceledException: async error => { cancelledHandlerCalled = true; await Task.CompletedTask; });

            cancelledHandlerCalled.ShouldBeTrue();
        }

        [Fact]
        public async Task SwitchAsync_WithUnknownFailureType_ShouldThrowNotImplementedException()
        {
            // Arrange
            // This test is designed to fail initially - need way to create unknown failure type
            // Act & Assert
            true.ShouldBeFalse("Test not yet implemented - need to create Result<T> with unknown FailureType");
        }
    }

    #endregion SwitchAsync Extensions for Result<T> Tests

    #region ThenAsync Extensions Tests

    public class ThenAsyncExtensions
    {
        [Fact]
        public async Task ThenAsync_ResultToResultT_WithSuccessResult_ShouldExecuteNextOperation()
        {
            // Arrange
            Result result = Result.Success();
            string expectedValue = "Success from next operation";

            // Act
            Result<string> nextResult = await result.ThenAsync(async () =>
            {
                await Task.Delay(1);
                return Result.Success(expectedValue);
            });

            // Assert
            nextResult.IsSuccess.ShouldBeTrue();
            nextResult.TryGetValue(out string? value).ShouldBeTrue();
            value.ShouldBe(expectedValue);
        }

        [Fact]
        public async Task ThenAsync_ResultToResultT_WithFailureResult_ShouldReturnFailureWithoutExecutingNext()
        {
            // Arrange
            string errorMessage = "Initial failure";
            Result result = Result.Failure(errorMessage);
            bool nextOperationCalled = false;

            // Act
            Result<string> nextResult = await result.ThenAsync(async () =>
            {
                await Task.Delay(1);
                nextOperationCalled = true;
                return Result.Success("Should not be called");
            });

            // Assert
            nextResult.IsFailure.ShouldBeTrue();
            nextResult.Error.ShouldBe(errorMessage);
            nextOperationCalled.ShouldBeFalse();
        }

        [Fact]
        public async Task ThenAsync_ResultToResult_WithSuccessResult_ShouldExecuteNextOperation()
        {
            // Arrange
            Result result = Result.Success();

            // Act
            Result nextResult = await result.ThenAsync(async () =>
            {
                await Task.Delay(1);
                return Result.Success();
            });

            // Assert
            nextResult.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task ThenAsync_ResultToResult_WithFailureResult_ShouldReturnOriginalFailure()
        {
            // Arrange
            string errorMessage = "Initial failure";
            Result result = Result.Failure(errorMessage);
            bool nextOperationCalled = false;

            // Act
            Result nextResult = await result.ThenAsync(async () =>
            {
                await Task.Delay(1);
                nextOperationCalled = true;
                return Result.Success();
            });

            // Assert
            nextResult.IsFailure.ShouldBeTrue();
            nextResult.Error.ShouldBe(errorMessage);
            nextOperationCalled.ShouldBeFalse();
        }

        [Fact]
        public async Task ThenAsync_ResultTToResultTNext_WithSuccessResult_ShouldTransformValue()
        {
            // Arrange
            int initialValue = 42;
            Result<int> result = Result.Success(initialValue);
            string expectedValue = $"Processed: {initialValue}";

            // Act
            Result<string> nextResult = await result.ThenAsync(async value =>
            {
                await Task.Delay(1);
                return Result.Success($"Processed: {value}");
            });

            // Assert
            nextResult.IsSuccess.ShouldBeTrue();
            nextResult.TryGetValue(out string? processedValue).ShouldBeTrue();
            processedValue.ShouldBe(expectedValue);
        }

        [Fact]
        public async Task ThenAsync_ResultTToResultTNext_WithFailureResult_ShouldReturnFailureWithoutExecutingNext()
        {
            // Arrange
            string errorMessage = "Initial failure";
            Result<int> result = Result.Failure<int>(errorMessage);
            bool nextOperationCalled = false;

            // Act
            Result<string> nextResult = await result.ThenAsync(async value =>
            {
                await Task.Delay(1);
                nextOperationCalled = true;
                return Result.Success($"Processed: {value}");
            });

            // Assert
            nextResult.IsFailure.ShouldBeTrue();
            nextResult.Error.ShouldBe(errorMessage);
            nextOperationCalled.ShouldBeFalse();
        }

        [Fact]
        public async Task ThenAsync_ResultTToResult_WithSuccessResult_ShouldExecuteNextOperation()
        {
            // Arrange
            int initialValue = 42;
            Result<int> result = Result.Success(initialValue);
            bool nextOperationCalled = false;
            int receivedValue = 0;

            // Act
            Result nextResult = await result.ThenAsync(async value =>
            {
                await Task.Delay(1);
                nextOperationCalled = true;
                receivedValue = value;
                return Result.Success();
            });

            // Assert
            nextResult.IsSuccess.ShouldBeTrue();
            nextOperationCalled.ShouldBeTrue();
            receivedValue.ShouldBe(initialValue);
        }

        [Fact]
        public async Task ThenAsync_ResultTToResult_WithFailureResult_ShouldReturnOriginalFailure()
        {
            // Arrange
            string errorMessage = "Initial failure";
            Result<int> result = Result.Failure<int>(errorMessage);
            bool nextOperationCalled = false;

            // Act
            Result nextResult = await result.ThenAsync(async value =>
            {
                await Task.Delay(1);
                nextOperationCalled = true;
                return Result.Success();
            });

            // Assert
            nextResult.IsFailure.ShouldBeTrue();
            nextResult.Error.ShouldBe(errorMessage);
            nextOperationCalled.ShouldBeFalse();
        }

        [Fact]
        public async Task ThenAsync_ResultTToResultTNext_WithSuccessResultButTryGetValueFails_ShouldReturnFailure()
        {
            // Arrange
            // This test is designed to fail initially - need scenario where TryGetValue fails on success
            // Act & Assert
            true.ShouldBeFalse("Test not yet implemented - need scenario where IsSuccess=true but TryGetValue fails");
        }

        [Fact]
        public async Task ThenAsync_ResultTToResult_WithSuccessResultButTryGetValueFails_ShouldReturnFailure()
        {
            // Arrange
            // This test is designed to fail initially - need scenario where TryGetValue fails on success
            // Act & Assert
            true.ShouldBeFalse("Test not yet implemented - need scenario where IsSuccess=true but TryGetValue fails");
        }

        [Fact]
        public async Task ThenAsync_WithNullResult_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = null!;

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.ThenAsync(async () => await Task.FromResult(Result.Success<string>("test"))));
        }

        [Fact]
        public async Task ThenAsync_WithNullNextAsync_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result result = Result.Success();

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.ThenAsync((Func<Task<Result<string>>>)null!));
        }
    }

    #endregion ThenAsync Extensions Tests

    #region MapAsync Extensions Tests

    public class MapAsyncExtensions
    {
        [Fact]
        public async Task MapAsync_WithSuccessResult_ShouldTransformValue()
        {
            // Arrange
            int initialValue = 42;
            Result<int> result = Result.Success(initialValue);
            string expectedValue = $"Value: {initialValue}";

            // Act
            Result<string> mappedResult = await result.MapAsync(async value =>
            {
                await Task.Delay(1);
                return $"Value: {value}";
            });

            // Assert
            mappedResult.IsSuccess.ShouldBeTrue();
            mappedResult.TryGetValue(out string? mappedValue).ShouldBeTrue();
            mappedValue.ShouldBe(expectedValue);
        }

        [Fact]
        public async Task MapAsync_WithFailureResult_ShouldReturnFailureWithoutMapping()
        {
            // Arrange
            string errorMessage = "Initial failure";
            Result<int> result = Result.Failure<int>(errorMessage);
            bool mapperCalled = false;

            // Act
            Result<string> mappedResult = await result.MapAsync(async value =>
            {
                await Task.Delay(1);
                mapperCalled = true;
                return $"Value: {value}";
            });

            // Assert
            mappedResult.IsFailure.ShouldBeTrue();
            mappedResult.Error.ShouldBe(errorMessage);
            mapperCalled.ShouldBeFalse();
        }

        [Fact]
        public async Task MapAsync_WithSuccessResultButTryGetValueFails_ShouldReturnFailure()
        {
            // Arrange
            // This test is designed to fail initially - need scenario where TryGetValue fails on success
            // Act & Assert
            true.ShouldBeFalse("Test not yet implemented - need scenario where IsSuccess=true but TryGetValue fails");
        }

        [Fact]
        public async Task MapAsync_WithNullResult_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result<int> result = null!;

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MapAsync(async val => await Task.FromResult($"Value: {val}")));
        }

        [Fact]
        public async Task MapAsync_WithNullMapAsync_ShouldThrowArgumentNullException()
        {
            // Arrange
            Result<int> result = Result.Success(42);

            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await result.MapAsync((Func<int, Task<string>>)null!));
        }

        [Fact]
        public async Task MapAsync_WithSuccessResult_ShouldPreserveResultType()
        {
            // Arrange
            int initialValue = 42;
            Result<int> result = Result.Success(initialValue, ResultType.Information);

            // Act
            Result<string> mappedResult = await result.MapAsync(async value => await Task.FromResult($"Value: {value}"));

            // Assert
            mappedResult.IsSuccess.ShouldBeTrue();
            mappedResult.ResultType.ShouldBe(ResultType.Information);
        }
    }

    #endregion MapAsync Extensions Tests

    #region CombineAsync Extensions Tests

    public class CombineAsyncExtensions
    {
        [Fact]
        public async Task CombineAsync_WithAllSuccessfulTasks_ShouldReturnSuccess()
        {
            // Arrange
            Task<Result>[] tasks =
            [
                Task.FromResult(Result.Success()),
                Task.FromResult(Result.Success(ResultType.Information)),
                Task.FromResult(Result.Success(ResultType.Warning))
            ];

            // Act
            Result combinedResult = await ResultAsyncExtensions.CombineAsync(tasks);

            // Assert
            combinedResult.IsSuccess.ShouldBeTrue();
            combinedResult.Error.ShouldBeEmpty();
            combinedResult.Failures.ShouldBeEmpty();
        }

        [Fact]
        public async Task CombineAsync_WithSomeFailures_ShouldReturnFailureWithAggregatedErrors()
        {
            // Arrange
            Task<Result>[] tasks =
            [
                Task.FromResult(Result.Success()),
                Task.FromResult(Result.Failure("Error 1")),
                Task.FromResult(Result.Failure("Error 2")),
                Task.FromResult(Result.ValidationFailure(new Dictionary<string, string[]>
                {
                    { "Field1", new[] { "Validation error" } }
                }))
            ];

            // Act
            Result combinedResult = await ResultAsyncExtensions.CombineAsync(tasks);

            // Assert
            combinedResult.IsFailure.ShouldBeTrue();
            combinedResult.Failures.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task CombineAsync_WithConcurrentOperations_ShouldExecuteConcurrently()
        {
            // Arrange
            DateTime startTime = DateTime.Now;
            Task<Result>[] tasks =
            [
                CreateDelayedSuccessTask(100),
                CreateDelayedSuccessTask(100),
                CreateDelayedSuccessTask(100)
            ];

            // Act
            Result combinedResult = await ResultAsyncExtensions.CombineAsync(tasks);
            DateTime endTime = DateTime.Now;

            // Assert
            combinedResult.IsSuccess.ShouldBeTrue();
            // Should complete in roughly 100ms, not 300ms if run sequentially
            (endTime - startTime).TotalMilliseconds.ShouldBeLessThan(200);
        }

        [Fact]
        public async Task CombineAsync_ResultT_WithAllSuccessfulTasks_ShouldReturnFirstSuccessValue()
        {
            // Arrange
            Task<Result<string>>[] tasks =
            [
                Task.FromResult(Result.Success("First")),
                Task.FromResult(Result.Success("Second")),
                Task.FromResult(Result.Success("Third"))
            ];

            // Act
            Result<string> combinedResult = await ResultAsyncExtensions.CombineAsync(tasks);

            // Assert
            combinedResult.IsSuccess.ShouldBeTrue();
            combinedResult.TryGetValue(out string? value).ShouldBeTrue();
            value.ShouldBe("First");
        }

        [Fact]
        public async Task CombineAsync_ResultT_WithSomeFailures_ShouldReturnFailureWithAggregatedErrors()
        {
            // Arrange
            Task<Result<string>>[] tasks =
            [
                Task.FromResult(Result.Success("Success")),
                Task.FromResult(Result.Failure<string>("Error 1")),
                Task.FromResult(Result.Failure<string>("Error 2"))
            ];

            // Act
            Result<string> combinedResult = await ResultAsyncExtensions.CombineAsync(tasks);

            // Assert
            combinedResult.IsFailure.ShouldBeTrue();
            combinedResult.Failures.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task CombineAsync_WithNullTasks_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await ResultAsyncExtensions.CombineAsync((Task<Result>[])null!));
        }

        [Fact]
        public async Task CombineAsync_ResultT_WithNullTasks_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await ResultAsyncExtensions.CombineAsync((Task<Result<string>>[])null!));
        }

        [Fact]
        public async Task CombineAsync_WithEmptyTaskArray_ShouldReturnSuccess()
        {
            // Arrange
            Task<Result>[] tasks = [];

            // Act
            Result combinedResult = await ResultAsyncExtensions.CombineAsync(tasks);

            // Assert
            combinedResult.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task CombineAsync_WithFailedTasks_ShouldStillWaitForAll()
        {
            // Arrange
            DateTime startTime = DateTime.Now;
            Task<Result>[] tasks =
            [
                CreateDelayedFailureTask(50),
                CreateDelayedSuccessTask(100),
                CreateDelayedFailureTask(150)
            ];

            // Act
            Result combinedResult = await ResultAsyncExtensions.CombineAsync(tasks);
            DateTime endTime = DateTime.Now;

            // Assert
            combinedResult.IsFailure.ShouldBeTrue();
            // Should wait for all tasks, so roughly 150ms
            (endTime - startTime).TotalMilliseconds.ShouldBeGreaterThan(140);
        }

        private static async Task<Result> CreateDelayedSuccessTask(int delayMs)
        {
            await Task.Delay(delayMs);
            return Result.Success();
        }

        private static async Task<Result> CreateDelayedFailureTask(int delayMs)
        {
            await Task.Delay(delayMs);
            return Result.Failure($"Error after {delayMs}ms");
        }
    }

    #endregion CombineAsync Extensions Tests

    #region ConfigureAwait and Async Pattern Tests

    public class ConfigureAwaitTests
    {
        [Fact]
        public async Task AllAsyncMethods_ShouldUseConfigureAwaitFalse()
        {
            // Arrange
            // This test verifies that all async methods use ConfigureAwait(false)
            // by running in a context where SynchronizationContext would cause issues
            // if ConfigureAwait(false) is not used properly

            // Act & Assert
            // This test is designed to fail initially and needs to be implemented with
            // proper async context verification
            true.ShouldBeFalse("Test not yet implemented - need to verify ConfigureAwait(false) usage");
        }
    }

    #endregion ConfigureAwait and Async Pattern Tests

    #region Edge Cases and Error Scenarios Tests

    public class EdgeCaseTests
    {
        [Fact]
        public async Task AsyncExtensions_WithTaskThatThrowsException_ShouldPropagateException()
        {
            // Arrange
            Result result = Result.Success();

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(async () =>
                await result.MatchAsync<string>(
                    onSuccess: async () =>
                    {
                        await Task.Delay(1);
                        throw new InvalidOperationException("Test exception");
                    },
                    onFailure: async error => await Task.FromResult($"Failed: {error}")));
        }

        [Fact]
        public async Task AsyncExtensions_WithCancelledTask_ShouldPropagateTaskCancellation()
        {
            // Arrange
            Result result = Result.Success();
            CancellationTokenSource cts = new();
            cts.Cancel();

            // Act & Assert
            await Should.ThrowAsync<TaskCanceledException>(async () =>
                await result.MatchAsync<string>(
                    onSuccess: async () =>
                    {
                        await Task.Delay(100, cts.Token);
                        return "Should not reach here";
                    },
                    onFailure: async error => await Task.FromResult($"Failed: {error}")));
        }

        [Fact]
        public async Task AsyncExtensions_WithLongRunningTasks_ShouldNotBlock()
        {
            // Arrange
            Result result = Result.Success();
            DateTime startTime = DateTime.Now;

            // Act
            string response = await result.MatchAsync(
                onSuccess: async () =>
                {
                    // Simulate async work without blocking
                    await Task.Yield();
                    await Task.Delay(50);
                    return "Completed";
                },
                onFailure: async error => await Task.FromResult($"Failed: {error}"));

            DateTime endTime = DateTime.Now;

            // Assert
            response.ShouldBe("Completed");
            (endTime - startTime).TotalMilliseconds.ShouldBeGreaterThan(45);
        }
    }

    #endregion Edge Cases and Error Scenarios Tests
}