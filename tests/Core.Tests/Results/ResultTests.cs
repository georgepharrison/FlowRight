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
}