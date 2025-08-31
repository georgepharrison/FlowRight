using System.Security;
using FlowRight.Core.Extensions;
using FlowRight.Core.Results;
using Shouldly;

namespace FlowRight.Core.Tests.Results;

/// <summary>
/// Tests for async-friendly extension methods for the Result class.
/// These tests verify proper async behavior, exception handling, and result composition.
/// </summary>
public class ResultAsyncExtensionTests
{
    #region MatchAsync Tests

    [Fact]
    public async Task MatchAsync_WithSuccessResult_ShouldCallOnSuccessAsyncHandler()
    {
        // Arrange
        Result result = Result.Success();
        string expectedValue = "Success!";

        // Act
        string actualValue = await result.MatchAsync(
            onSuccess: async () =>
            {
                await Task.Delay(10);
                return expectedValue;
            },
            onFailure: async error =>
            {
                await Task.Delay(10);
                return $"Failed: {error}";
            });

        // Assert
        actualValue.ShouldBe(expectedValue);
    }

    [Fact]
    public async Task MatchAsync_WithFailureResult_ShouldCallOnFailureAsyncHandler()
    {
        // Arrange
        string errorMessage = "Something went wrong";
        Result result = Result.Failure(errorMessage);
        string expectedValue = $"Failed: {errorMessage}";

        // Act
        string actualValue = await result.MatchAsync(
            onSuccess: async () =>
            {
                await Task.Delay(10);
                return "Success!";
            },
            onFailure: async error =>
            {
                await Task.Delay(10);
                return $"Failed: {error}";
            });

        // Assert
        actualValue.ShouldBe(expectedValue);
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
                onFailure: async _ => await Task.FromResult("error")));
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
        Result securityResult = Result.Failure(new SecurityException("Access denied"));
        Result validationResult = Result.ValidationFailure(new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error1", "Error2" } }
        });
        Result cancelledResult = Result.Failure(new OperationCanceledException("Cancelled"));

        // Act & Assert - Security
        string securityResponse = await securityResult.MatchAsync(
            onSuccess: async () => await Task.FromResult("Success"),
            onError: async error => await Task.FromResult($"Error: {error}"),
            onSecurityException: async error => await Task.FromResult($"Security: {error}"),
            onValidationException: async errors => await Task.FromResult($"Validation: {errors.Count} errors"),
            onOperationCanceledException: async error => await Task.FromResult($"Cancelled: {error}"));

        securityResponse.ShouldBe("Security: Access denied");

        // Act & Assert - Validation
        string validationResponse = await validationResult.MatchAsync(
            onSuccess: async () => await Task.FromResult("Success"),
            onError: async error => await Task.FromResult($"Error: {error}"),
            onSecurityException: async error => await Task.FromResult($"Security: {error}"),
            onValidationException: async errors => await Task.FromResult($"Validation: {errors.Count} errors"),
            onOperationCanceledException: async error => await Task.FromResult($"Cancelled: {error}"));

        validationResponse.ShouldBe("Validation: 1 errors");

        // Act & Assert - Cancelled
        string cancelledResponse = await cancelledResult.MatchAsync(
            onSuccess: async () => await Task.FromResult("Success"),
            onError: async error => await Task.FromResult($"Error: {error}"),
            onSecurityException: async error => await Task.FromResult($"Security: {error}"),
            onValidationException: async errors => await Task.FromResult($"Validation: {errors.Count} errors"),
            onOperationCanceledException: async error => await Task.FromResult($"Cancelled: {error}"));

        cancelledResponse.ShouldBe("Cancelled: Cancelled");
    }

    #endregion MatchAsync Tests

    #region MatchAsync for Result<T> Tests

    [Fact]
    public async Task MatchAsync_ResultT_WithSuccessResult_ShouldCallOnSuccessAsyncHandler()
    {
        // Arrange
        int value = 42;
        Result<int> result = Result.Success(value);
        string expectedValue = $"Success: {value}";

        // Act
        string actualValue = await result.MatchAsync(
            onSuccess: async val =>
            {
                await Task.Delay(10);
                return $"Success: {val}";
            },
            onFailure: async error =>
            {
                await Task.Delay(10);
                return $"Failed: {error}";
            });

        // Assert
        actualValue.ShouldBe(expectedValue);
    }

    [Fact]
    public async Task MatchAsync_ResultT_WithFailureResult_ShouldCallOnFailureAsyncHandler()
    {
        // Arrange
        string errorMessage = "Something went wrong";
        Result<int> result = Result.Failure<int>(errorMessage);
        string expectedValue = $"Failed: {errorMessage}";

        // Act
        string actualValue = await result.MatchAsync(
            onSuccess: async val =>
            {
                await Task.Delay(10);
                return $"Success: {val}";
            },
            onFailure: async error =>
            {
                await Task.Delay(10);
                return $"Failed: {error}";
            });

        // Assert
        actualValue.ShouldBe(expectedValue);
    }

    #endregion MatchAsync for Result<T> Tests

    #region SwitchAsync Tests

    [Fact]
    public async Task SwitchAsync_WithSuccessResult_ShouldCallOnSuccessAsyncHandler()
    {
        // Arrange
        Result result = Result.Success();
        bool onSuccessCalled = false;
        bool onFailureCalled = false;

        // Act
        await result.SwitchAsync(
            onSuccess: async () =>
            {
                await Task.Delay(10);
                onSuccessCalled = true;
            },
            onFailure: async error =>
            {
                await Task.Delay(10);
                onFailureCalled = true;
            });

        // Assert
        onSuccessCalled.ShouldBeTrue();
        onFailureCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task SwitchAsync_WithFailureResult_ShouldCallOnFailureAsyncHandler()
    {
        // Arrange
        Result result = Result.Failure("Error");
        bool onSuccessCalled = false;
        bool onFailureCalled = false;

        // Act
        await result.SwitchAsync(
            onSuccess: async () =>
            {
                await Task.Delay(10);
                onSuccessCalled = true;
            },
            onFailure: async error =>
            {
                await Task.Delay(10);
                onFailureCalled = true;
            });

        // Assert
        onSuccessCalled.ShouldBeFalse();
        onFailureCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task SwitchAsync_WithOperationCancelledAndExcludeFlag_ShouldNotCallHandler()
    {
        // Arrange
        Result result = Result.Failure(new OperationCanceledException("Cancelled"));
        bool onSuccessCalled = false;
        bool onFailureCalled = false;

        // Act
        await result.SwitchAsync(
            onSuccess: async () =>
            {
                await Task.Delay(10);
                onSuccessCalled = true;
            },
            onFailure: async error =>
            {
                await Task.Delay(10);
                onFailureCalled = true;
            },
            includeOperationCancelledFailures: false);

        // Assert
        onSuccessCalled.ShouldBeFalse();
        onFailureCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task SwitchAsync_WithOperationCancelledAndIncludeFlag_ShouldCallHandler()
    {
        // Arrange
        Result result = Result.Failure(new OperationCanceledException("Cancelled"));
        bool onSuccessCalled = false;
        bool onFailureCalled = false;

        // Act
        await result.SwitchAsync(
            onSuccess: async () =>
            {
                await Task.Delay(10);
                onSuccessCalled = true;
            },
            onFailure: async error =>
            {
                await Task.Delay(10);
                onFailureCalled = true;
            },
            includeOperationCancelledFailures: true);

        // Assert
        onSuccessCalled.ShouldBeFalse();
        onFailureCalled.ShouldBeTrue();
    }

    #endregion SwitchAsync Tests

    #region SwitchAsync for Result<T> Tests

    [Fact]
    public async Task SwitchAsync_ResultT_WithSuccessResult_ShouldCallOnSuccessAsyncHandler()
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
                await Task.Delay(10);
                onSuccessCalled = true;
                receivedValue = val;
            },
            onFailure: async error =>
            {
                await Task.Delay(10);
                onFailureCalled = true;
            });

        // Assert
        onSuccessCalled.ShouldBeTrue();
        onFailureCalled.ShouldBeFalse();
        receivedValue.ShouldBe(value);
    }

    #endregion SwitchAsync for Result<T> Tests

    #region ThenAsync Tests

    [Fact]
    public async Task ThenAsync_WithSuccessResult_ShouldExecuteNextOperation()
    {
        // Arrange
        Result result = Result.Success();
        string expectedValue = "Next operation succeeded";

        // Act
        Result<string> nextResult = await result.ThenAsync(async () =>
        {
            await Task.Delay(10);
            return Result.Success(expectedValue);
        });

        // Assert
        nextResult.IsSuccess.ShouldBeTrue();
        nextResult.TryGetValue(out string? value).ShouldBeTrue();
        value.ShouldBe(expectedValue);
    }

    [Fact]
    public async Task ThenAsync_WithFailureResult_ShouldReturnFailureWithoutExecutingNextOperation()
    {
        // Arrange
        string errorMessage = "Initial failure";
        Result result = Result.Failure(errorMessage);
        bool nextOperationCalled = false;

        // Act
        Result<string> nextResult = await result.ThenAsync(async () =>
        {
            await Task.Delay(10);
            nextOperationCalled = true;
            return Result.Success("Should not be called");
        });

        // Assert
        nextResult.IsFailure.ShouldBeTrue();
        nextResult.Error.ShouldBe(errorMessage);
        nextOperationCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ThenAsync_ResultT_WithSuccessResult_ShouldTransformValue()
    {
        // Arrange
        int initialValue = 42;
        Result<int> result = Result.Success(initialValue);
        string expectedValue = $"Processed: {initialValue}";

        // Act
        Result<string> nextResult = await result.ThenAsync(async value =>
        {
            await Task.Delay(10);
            return Result.Success($"Processed: {value}");
        });

        // Assert
        nextResult.IsSuccess.ShouldBeTrue();
        nextResult.TryGetValue(out string? processedValue).ShouldBeTrue();
        processedValue.ShouldBe(expectedValue);
    }

    #endregion ThenAsync Tests

    #region MapAsync Tests

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
            await Task.Delay(10);
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
            await Task.Delay(10);
            mapperCalled = true;
            return $"Value: {value}";
        });

        // Assert
        mappedResult.IsFailure.ShouldBeTrue();
        mappedResult.Error.ShouldBe(errorMessage);
        mapperCalled.ShouldBeFalse();
    }

    #endregion MapAsync Tests

    #region CombineAsync Tests

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
        combinedResult.Failures.ShouldContainKey("Field1");
        combinedResult.Failures.ShouldContainKey("Error");
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
    public async Task CombineAsync_WithNullTasks_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await ResultAsyncExtensions.CombineAsync((Task<Result>[])null!));
    }

    #endregion CombineAsync Tests
}