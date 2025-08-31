using System.Security;
using FlowRight.Core.Results;
using Shouldly;

namespace FlowRight.Core.Tests.Results;

/// <summary>
/// Tests for the non-generic Result class to ensure proper behavior
/// of success, failure, and aggregation operations.
/// </summary>
public class ResultTests
{
    #region Combine Method Tests

    [Fact]
    public void Combine_WithNullResults_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Combine(null!));
    }

    [Fact]
    public void Combine_WithEmptyResults_ShouldReturnSuccess()
    {
        // Arrange & Act
        Result result = Result.Combine();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeEmpty();
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.ResultType.ShouldBe(ResultType.Success);
    }

    [Fact]
    public void Combine_WithAllSuccessResults_ShouldReturnSuccess()
    {
        // Arrange
        Result[] successResults = [
            Result.Success(),
            Result.Success(ResultType.Information),
            Result.Success(ResultType.Warning)
        ];

        // Act
        Result result = Result.Combine(successResults);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeEmpty();
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.ResultType.ShouldBe(ResultType.Success);
    }

    [Fact]
    public void Combine_WithSingleErrorResult_ShouldReturnFailureWithError()
    {
        // Arrange
        const string errorMessage = "Operation failed";
        Result[] results = [
            Result.Success(),
            Result.Failure(errorMessage),
            Result.Success()
        ];

        // Act
        Result result = Result.Combine(results);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.ResultType.ShouldBe(ResultType.Error);
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldContainKey(ResultFailureType.Error.ToString());
        result.Failures[ResultFailureType.Error.ToString()].ShouldContain(errorMessage);
    }

    [Fact]
    public void Combine_WithMultipleErrorResults_ShouldAggregateAllErrors()
    {
        // Arrange
        const string error1 = "First error";
        const string error2 = "Second error";
        const string error3 = "Third error";
        Result[] results = [
            Result.Failure(error1),
            Result.Success(),
            Result.Failure(error2),
            Result.Failure(error3)
        ];

        // Act
        Result result = Result.Combine(results);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldContainKey(ResultFailureType.Error.ToString());
        string[] errorMessages = result.Failures[ResultFailureType.Error.ToString()];
        errorMessages.ShouldContain(error1);
        errorMessages.ShouldContain(error2);
        errorMessages.ShouldContain(error3);
        errorMessages.Length.ShouldBe(3);
    }

    [Fact]
    public void Combine_WithValidationResults_ShouldAggregateValidationErrors()
    {
        // Arrange
        Dictionary<string, string[]> validationErrors1 = new()
        {
            { "Email", ["Email is required", "Invalid email format"] },
            { "Password", ["Password is too short"] }
        };
        Dictionary<string, string[]> validationErrors2 = new()
        {
            { "Email", ["Email already exists"] },
            { "Username", ["Username is required"] }
        };

        Result[] results = [
            Result.Failure(validationErrors1),
            Result.Success(),
            Result.Failure(validationErrors2)
        ];

        // Act
        Result result = Result.Combine(results);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        
        // Check Email field has all errors from both validation results
        result.Failures.ShouldContainKey("Email");
        string[] emailErrors = result.Failures["Email"];
        emailErrors.ShouldContain("Email is required");
        emailErrors.ShouldContain("Invalid email format");
        emailErrors.ShouldContain("Email already exists");
        emailErrors.Length.ShouldBe(3);

        // Check Password field
        result.Failures.ShouldContainKey("Password");
        result.Failures["Password"].ShouldContain("Password is too short");
        result.Failures["Password"].Length.ShouldBe(1);

        // Check Username field
        result.Failures.ShouldContainKey("Username");
        result.Failures["Username"].ShouldContain("Username is required");
        result.Failures["Username"].Length.ShouldBe(1);
    }

    [Fact]
    public void Combine_WithSecurityResults_ShouldAggregateSecurityErrors()
    {
        // Arrange
        const string securityError1 = "Access denied";
        const string securityError2 = "Unauthorized access";
        Result[] results = [
            Result.Failure(new SecurityException(securityError1)),
            Result.Success(),
            Result.Failure(new SecurityException(securityError2))
        ];

        // Act
        Result result = Result.Combine(results);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldContainKey(ResultFailureType.Security.ToString());
        string[] securityErrors = result.Failures[ResultFailureType.Security.ToString()];
        securityErrors.ShouldContain(securityError1);
        securityErrors.ShouldContain(securityError2);
        securityErrors.Length.ShouldBe(2);
    }

    [Fact]
    public void Combine_WithOperationCanceledResults_ShouldAggregateCancellationErrors()
    {
        // Arrange
        const string cancelMessage1 = "Operation was canceled due to timeout";
        const string cancelMessage2 = "User canceled the operation";
        Result[] results = [
            Result.Failure(new OperationCanceledException(cancelMessage1)),
            Result.Success(),
            Result.Failure(new OperationCanceledException(cancelMessage2))
        ];

        // Act
        Result result = Result.Combine(results);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldContainKey(ResultFailureType.OperationCanceled.ToString());
        string[] cancelErrors = result.Failures[ResultFailureType.OperationCanceled.ToString()];
        cancelErrors.ShouldContain(cancelMessage1);
        cancelErrors.ShouldContain(cancelMessage2);
        cancelErrors.Length.ShouldBe(2);
    }

    [Fact]
    public void Combine_WithMixedFailureTypes_ShouldAggregateAllErrorTypes()
    {
        // Arrange
        const string generalError = "General error";
        const string securityError = "Security violation";
        Dictionary<string, string[]> validationErrors = new()
        {
            { "Field1", ["Validation error"] }
        };
        const string cancelError = "Operation canceled";

        Result[] results = [
            Result.Failure(generalError),
            Result.Failure(new SecurityException(securityError)),
            Result.Failure(validationErrors),
            Result.Failure(new OperationCanceledException(cancelError)),
            Result.Success()
        ];

        // Act
        Result result = Result.Combine(results);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        
        // Verify all error types are aggregated
        result.Failures.ShouldContainKey(ResultFailureType.Error.ToString());
        result.Failures[ResultFailureType.Error.ToString()].ShouldContain(generalError);
        
        result.Failures.ShouldContainKey(ResultFailureType.Security.ToString());
        result.Failures[ResultFailureType.Security.ToString()].ShouldContain(securityError);
        
        result.Failures.ShouldContainKey("Field1");
        result.Failures["Field1"].ShouldContain("Validation error");
        
        result.Failures.ShouldContainKey(ResultFailureType.OperationCanceled.ToString());
        result.Failures[ResultFailureType.OperationCanceled.ToString()].ShouldContain(cancelError);
        
        result.Failures.Count.ShouldBe(4);
    }

    #endregion Combine Method Tests

    #region Match Method Tests

    [Fact]
    public void Match_WithNullOnSuccess_ShouldThrowArgumentNullException()
    {
        // Arrange
        Result result = Result.Success();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Match<string>(null!, error => "failure"));
    }

    [Fact]
    public void Match_WithNullOnFailure_ShouldThrowArgumentNullException()
    {
        // Arrange
        Result result = Result.Success();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Match(() => "success", null!));
    }

    [Fact]
    public void Match_WithSuccessResult_ShouldCallOnSuccess()
    {
        // Arrange
        Result result = Result.Success();
        const string expectedValue = "success result";

        // Act
        string actualValue = result.Match(
            onSuccess: () => expectedValue,
            onFailure: error => $"failure: {error}"
        );

        // Assert
        actualValue.ShouldBe(expectedValue);
    }

    [Fact]
    public void Match_WithFailureResult_ShouldCallOnFailure()
    {
        // Arrange
        const string errorMessage = "Something went wrong";
        Result result = Result.Failure(errorMessage);
        const string successValue = "success result";

        // Act
        string actualValue = result.Match(
            onSuccess: () => successValue,
            onFailure: error => $"failure: {error}"
        );

        // Assert
        actualValue.ShouldBe($"failure: {errorMessage}");
    }

    [Fact]
    public void Match_WithResultTypePreservation_ShouldReturnCorrectValue()
    {
        // Arrange
        Result informationResult = Result.Success(ResultType.Information);
        Result warningResult = Result.Success(ResultType.Warning);

        // Act
        string infoMessage = informationResult.Match(
            onSuccess: () => "info success",
            onFailure: error => $"info failure: {error}"
        );
        
        string warningMessage = warningResult.Match(
            onSuccess: () => "warning success", 
            onFailure: error => $"warning failure: {error}"
        );

        // Assert
        infoMessage.ShouldBe("info success");
        warningMessage.ShouldBe("warning success");
    }

    // Complex Match method tests

    [Fact]
    public void Match_ComplexWithNullHandlers_ShouldThrowArgumentNullException()
    {
        // Arrange
        Result result = Result.Success();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Match<string>(
            null!, 
            error => "error", 
            sec => "security", 
            val => "validation", 
            cancel => "canceled"
        ));

        Should.Throw<ArgumentNullException>(() => result.Match<string>(
            () => "success", 
            null!, 
            sec => "security", 
            val => "validation", 
            cancel => "canceled"
        ));

        Should.Throw<ArgumentNullException>(() => result.Match<string>(
            () => "success", 
            error => "error", 
            null!, 
            val => "validation", 
            cancel => "canceled"
        ));

        Should.Throw<ArgumentNullException>(() => result.Match<string>(
            () => "success", 
            error => "error", 
            sec => "security", 
            null!, 
            cancel => "canceled"
        ));

        Should.Throw<ArgumentNullException>(() => result.Match<string>(
            () => "success", 
            error => "error", 
            sec => "security", 
            val => "validation", 
            null!
        ));
    }

    [Fact]
    public void Match_ComplexWithSuccess_ShouldCallOnSuccess()
    {
        // Arrange
        Result result = Result.Success();
        const string expectedValue = "success result";

        // Act
        string actualValue = result.Match(
            onSuccess: () => expectedValue,
            onError: error => "error",
            onSecurityException: error => "security",
            onValidationException: errors => "validation",
            onOperationCanceledException: error => "canceled"
        );

        // Assert
        actualValue.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(ResultFailureType.Error)]
    [InlineData(ResultFailureType.Security)]
    [InlineData(ResultFailureType.OperationCanceled)]
    public void Match_ComplexWithSpecificFailureTypes_ShouldRouteToCorrectHandler(ResultFailureType failureType)
    {
        // Arrange
        Result result = CreateFailureByType(failureType);

        // Act
        string output = result.Match(
            onSuccess: () => "success",
            onError: error => "general error",
            onSecurityException: error => "security error",
            onValidationException: errors => "validation error",
            onOperationCanceledException: error => "cancellation error"
        );

        // Assert
        string expected = failureType switch
        {
            ResultFailureType.Error => "general error",
            ResultFailureType.Security => "security error",
            ResultFailureType.OperationCanceled => "cancellation error",
            _ => throw new ArgumentException($"Unexpected failure type: {failureType}")
        };
        output.ShouldBe(expected);
    }

    [Fact]
    public void Match_ComplexWithValidationFailure_ShouldCallOnValidationException()
    {
        // Arrange
        Dictionary<string, string[]> validationErrors = new()
        {
            { "Email", ["Email is required"] },
            { "Password", ["Password too short"] }
        };
        Result result = Result.Failure(validationErrors);

        // Act
        string output = result.Match(
            onSuccess: () => "success",
            onError: error => "error",
            onSecurityException: error => "security",
            onValidationException: errors => $"validation: {errors.Count} fields",
            onOperationCanceledException: error => "canceled"
        );

        // Assert
        output.ShouldBe("validation: 2 fields");
    }

    #endregion Match Method Tests

    #region Helper Methods

    private static Result CreateFailureByType(ResultFailureType failureType)
    {
        return failureType switch
        {
            ResultFailureType.Error => Result.Failure("General error"),
            ResultFailureType.Security => Result.Failure(new SecurityException("Security error")),
            ResultFailureType.OperationCanceled => Result.Failure(new OperationCanceledException("Operation canceled")),
            _ => throw new ArgumentException($"Unsupported failure type: {failureType}")
        };
    }

    #endregion Helper Methods

    #region Switch Method Tests

    [Fact]
    public void Switch_SimpleWithNullOnSuccess_ShouldThrowArgumentNullException()
    {
        // Arrange
        Result result = Result.Success();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Switch(null!, error => { }));
    }

    [Fact]
    public void Switch_SimpleWithNullOnFailure_ShouldThrowArgumentNullException()
    {
        // Arrange
        Result result = Result.Success();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Switch(() => { }, null!));
    }

    [Fact]
    public void Switch_SimpleWithSuccessResult_ShouldCallOnSuccess()
    {
        // Arrange
        Result result = Result.Success();
        bool onSuccessCalled = false;
        bool onFailureCalled = false;

        // Act
        result.Switch(
            onSuccess: () => onSuccessCalled = true,
            onFailure: error => onFailureCalled = true
        );

        // Assert
        onSuccessCalled.ShouldBeTrue();
        onFailureCalled.ShouldBeFalse();
    }

    [Fact]
    public void Switch_SimpleWithFailureResult_ShouldCallOnFailure()
    {
        // Arrange
        const string errorMessage = "Something went wrong";
        Result result = Result.Failure(errorMessage);
        bool onSuccessCalled = false;
        string? capturedError = null;

        // Act
        result.Switch(
            onSuccess: () => onSuccessCalled = true,
            onFailure: error => capturedError = error
        );

        // Assert
        onSuccessCalled.ShouldBeFalse();
        capturedError.ShouldBe(errorMessage);
    }

    [Fact]
    public void Switch_SimpleWithOperationCancelledAndIncludeFlag_ShouldCallOnFailure()
    {
        // Arrange
        const string cancelMessage = "Operation was cancelled";
        Result result = Result.Failure(new OperationCanceledException(cancelMessage));
        bool onSuccessCalled = false;
        string? capturedError = null;

        // Act
        result.Switch(
            onSuccess: () => onSuccessCalled = true,
            onFailure: error => capturedError = error,
            includeOperationCancelledFailures: true
        );

        // Assert
        onSuccessCalled.ShouldBeFalse();
        capturedError.ShouldBe(cancelMessage);
    }

    [Fact]
    public void Switch_SimpleWithOperationCancelledAndNoIncludeFlag_ShouldNotCallHandlers()
    {
        // Arrange
        const string cancelMessage = "Operation was cancelled";
        Result result = Result.Failure(new OperationCanceledException(cancelMessage));
        bool onSuccessCalled = false;
        bool onFailureCalled = false;

        // Act
        result.Switch(
            onSuccess: () => onSuccessCalled = true,
            onFailure: error => onFailureCalled = true,
            includeOperationCancelledFailures: false
        );

        // Assert
        onSuccessCalled.ShouldBeFalse();
        onFailureCalled.ShouldBeFalse();
    }

    // Complex Switch method tests

    [Fact]
    public void Switch_ComplexWithNullHandlers_ShouldThrowArgumentNullException()
    {
        // Arrange
        Result result = Result.Success();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Switch(
            null!, 
            error => { }, 
            sec => { }, 
            val => { }
        ));

        Should.Throw<ArgumentNullException>(() => result.Switch(
            () => { }, 
            null!, 
            sec => { }, 
            val => { }
        ));

        Should.Throw<ArgumentNullException>(() => result.Switch(
            () => { }, 
            error => { }, 
            null!, 
            val => { }
        ));

        Should.Throw<ArgumentNullException>(() => result.Switch(
            () => { }, 
            error => { }, 
            sec => { }, 
            null!
        ));
    }

    [Fact]
    public void Switch_ComplexWithSuccess_ShouldCallOnSuccess()
    {
        // Arrange
        Result result = Result.Success();
        bool onSuccessCalled = false;
        bool onErrorCalled = false;
        bool onSecurityCalled = false;
        bool onValidationCalled = false;
        bool onCancelledCalled = false;

        // Act
        result.Switch(
            onSuccess: () => onSuccessCalled = true,
            onError: error => onErrorCalled = true,
            onSecurityException: error => onSecurityCalled = true,
            onValidationException: errors => onValidationCalled = true,
            onOperationCanceledException: error => onCancelledCalled = true
        );

        // Assert
        onSuccessCalled.ShouldBeTrue();
        onErrorCalled.ShouldBeFalse();
        onSecurityCalled.ShouldBeFalse();
        onValidationCalled.ShouldBeFalse();
        onCancelledCalled.ShouldBeFalse();
    }

    [Theory]
    [InlineData(ResultFailureType.Error)]
    [InlineData(ResultFailureType.Security)]
    [InlineData(ResultFailureType.OperationCanceled)]
    public void Switch_ComplexWithSpecificFailureTypes_ShouldRouteToCorrectHandler(ResultFailureType failureType)
    {
        // Arrange
        Result result = CreateFailureByType(failureType);
        string? handlerCalled = null;

        // Act
        result.Switch(
            onSuccess: () => handlerCalled = "success",
            onError: error => handlerCalled = "error",
            onSecurityException: error => handlerCalled = "security",
            onValidationException: errors => handlerCalled = "validation",
            onOperationCanceledException: error => handlerCalled = "cancelled"
        );

        // Assert
        string expected = failureType switch
        {
            ResultFailureType.Error => "error",
            ResultFailureType.Security => "security",
            ResultFailureType.OperationCanceled => "cancelled",
            _ => throw new ArgumentException($"Unexpected failure type: {failureType}")
        };
        handlerCalled.ShouldBe(expected);
    }

    [Fact]
    public void Switch_ComplexWithValidationFailure_ShouldCallOnValidationException()
    {
        // Arrange
        Dictionary<string, string[]> validationErrors = new()
        {
            { "Email", ["Email is required"] },
            { "Password", ["Password too short"] }
        };
        Result result = Result.Failure(validationErrors);
        string? handlerCalled = null;
        IDictionary<string, string[]>? capturedErrors = null;

        // Act
        result.Switch(
            onSuccess: () => handlerCalled = "success",
            onError: error => handlerCalled = "error",
            onSecurityException: error => handlerCalled = "security",
            onValidationException: errors => { handlerCalled = "validation"; capturedErrors = errors; },
            onOperationCanceledException: error => handlerCalled = "cancelled"
        );

        // Assert
        handlerCalled.ShouldBe("validation");
        capturedErrors.ShouldNotBeNull();
        capturedErrors.ShouldContainKey("Email");
        capturedErrors.ShouldContainKey("Password");
    }

    [Fact]
    public void Switch_ComplexWithOperationCancelledAndNullHandler_ShouldNotThrow()
    {
        // Arrange
        Result result = Result.Failure(new OperationCanceledException("Cancelled"));
        bool anyHandlerCalled = false;

        // Act & Assert (should not throw)
        result.Switch(
            onSuccess: () => anyHandlerCalled = true,
            onError: error => anyHandlerCalled = true,
            onSecurityException: error => anyHandlerCalled = true,
            onValidationException: errors => anyHandlerCalled = true,
            onOperationCanceledException: null
        );

        // Assert
        anyHandlerCalled.ShouldBeFalse();
    }

    [Fact]
    public void Switch_ComplexWithOperationCancelledAndHandler_ShouldCallHandler()
    {
        // Arrange
        const string cancelMessage = "Operation timeout";
        Result result = Result.Failure(new OperationCanceledException(cancelMessage));
        string? handlerCalled = null;
        string? capturedMessage = null;

        // Act
        result.Switch(
            onSuccess: () => handlerCalled = "success",
            onError: error => handlerCalled = "error",
            onSecurityException: error => handlerCalled = "security",
            onValidationException: errors => handlerCalled = "validation",
            onOperationCanceledException: error => { handlerCalled = "cancelled"; capturedMessage = error; }
        );

        // Assert
        handlerCalled.ShouldBe("cancelled");
        capturedMessage.ShouldBe(cancelMessage);
    }

    #endregion Switch Method Tests

    #region Explicit Operator Tests

    [Fact]
    public void ExplicitOperator_FromResultToBool_WithSuccessResult_ShouldReturnTrue()
    {
        // Arrange
        Result result = Result.Success();

        // Act
        bool isSuccess = (bool)result; // Explicit conversion

        // Assert
        isSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ExplicitOperator_FromResultToBool_WithFailureResult_ShouldReturnFalse()
    {
        // Arrange
        Result result = Result.Failure("Error occurred");

        // Act
        bool isSuccess = (bool)result; // Explicit conversion

        // Assert
        isSuccess.ShouldBeFalse();
    }

    [Fact]
    public void ExplicitOperator_FromResultToBool_WithNullResult_ShouldReturnFalse()
    {
        // Arrange
        Result? nullResult = null;

        // Act
        bool isSuccess = (bool)nullResult!; // Explicit conversion

        // Assert
        isSuccess.ShouldBeFalse();
    }

    #endregion Explicit Operator Tests
}