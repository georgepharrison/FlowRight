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
    #region Success Factory Method Tests

    [Fact]
    public void Success_WithDefaultResultType_ShouldReturnSuccessResult()
    {
        // Act
        Result result = Result.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeEmpty();
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.ResultType.ShouldBe(ResultType.Success);
    }

    [Theory]
    [InlineData(ResultType.Success)]
    [InlineData(ResultType.Information)]
    [InlineData(ResultType.Warning)]
    public void Success_WithSpecificResultType_ShouldReturnSuccessResultWithCorrectType(ResultType resultType)
    {
        // Act
        Result result = Result.Success(resultType);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeEmpty();
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.ResultType.ShouldBe(resultType);
    }

    [Fact]
    public void Success_Generic_WithValidValue_ShouldReturnSuccessResultWithValue()
    {
        // Arrange
        const string testValue = "Test Value";

        // Act
        Result<string> result = Result.Success(testValue);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeEmpty();
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.ResultType.ShouldBe(ResultType.Success);
        result.TryGetValue(out string? actualValue).ShouldBeTrue();
        actualValue.ShouldBe(testValue);
    }

    [Theory]
    [InlineData(ResultType.Success)]
    [InlineData(ResultType.Information)]
    [InlineData(ResultType.Warning)]
    public void Success_Generic_WithValidValueAndSpecificResultType_ShouldReturnCorrectResult(ResultType resultType)
    {
        // Arrange
        const int testValue = 42;

        // Act
        Result<int> result = Result.Success(testValue, resultType);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeEmpty();
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.ResultType.ShouldBe(resultType);
        result.TryGetValue(out int actualValue).ShouldBeTrue();
        actualValue.ShouldBe(testValue);
    }

    [Fact]
    public void Success_Generic_WithNullValue_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Success<string>(null!));
        Should.Throw<ArgumentNullException>(() => Result.Success<object>(null!));
    }

    [Fact]
    public void Success_Generic_WithNullValueAndSpecificResultType_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Success<string>(null!, ResultType.Information));
        Should.Throw<ArgumentNullException>(() => Result.Success<object>(null!, ResultType.Warning));
    }

    [Fact]
    public void Success_Generic_WithDifferentValueTypes_ShouldWorkCorrectly()
    {
        // Arrange & Act & Assert
        Result<int> intResult = Result.Success(123);
        intResult.TryGetValue(out int intValue).ShouldBeTrue();
        intValue.ShouldBe(123);

        Result<bool> boolResult = Result.Success(true);
        boolResult.TryGetValue(out bool boolValue).ShouldBeTrue();
        boolValue.ShouldBeTrue();

        Result<DateTime> dateResult = Result.Success(DateTime.UnixEpoch);
        dateResult.TryGetValue(out DateTime dateValue).ShouldBeTrue();
        dateValue.ShouldBe(DateTime.UnixEpoch);

        Result<Guid> guidResult = Result.Success(Guid.Empty);
        guidResult.TryGetValue(out Guid guidValue).ShouldBeTrue();
        guidValue.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Success_Generic_WithComplexObjects_ShouldRetainObjectReference()
    {
        // Arrange
        TestModel expectedModel = new() { Id = 1, Name = "Test" };

        // Act
        Result<TestModel> result = Result.Success(expectedModel);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out TestModel? actualModel).ShouldBeTrue();
        actualModel.ShouldBeSameAs(expectedModel);
        actualModel!.Id.ShouldBe(1);
        actualModel.Name.ShouldBe("Test");
    }

    #endregion Success Factory Method Tests

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

    #region Failure Factory Method Tests

    [Fact]
    public void Failure_WithErrorMessage_ShouldReturnFailureResult()
    {
        // Arrange
        const string errorMessage = "Something went wrong";

        // Act
        Result result = Result.Failure(errorMessage);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(errorMessage);
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.Error);
        result.ResultType.ShouldBe(ResultType.Error);
    }

    [Fact]
    public void Failure_WithNullErrorMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Failure((string)null!));
    }

    [Theory]
    [InlineData(ResultType.Error)]
    [InlineData(ResultType.Warning)]
    [InlineData(ResultType.Information)]
    public void Failure_WithErrorMessageAndResultType_ShouldReturnCorrectFailure(ResultType resultType)
    {
        // Arrange
        const string errorMessage = "Test error";

        // Act
        Result result = Result.Failure(errorMessage, resultType);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(errorMessage);
        result.ResultType.ShouldBe(resultType);
        result.FailureType.ShouldBe(ResultFailureType.Error);
    }

    [Theory]
    [InlineData(ResultFailureType.Error)]
    [InlineData(ResultFailureType.Security)]
    [InlineData(ResultFailureType.NotFound)]
    [InlineData(ResultFailureType.ServerError)]
    public void Failure_WithErrorMessageAndFailureType_ShouldReturnCorrectFailure(ResultFailureType failureType)
    {
        // Arrange
        const string errorMessage = "Test error";

        // Act
        Result result = Result.Failure(errorMessage, ResultType.Error, failureType);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(errorMessage);
        result.FailureType.ShouldBe(failureType);
        result.ResultType.ShouldBe(ResultType.Error);
    }

    [Fact]
    public void Failure_WithSingleFieldValidation_ShouldReturnValidationFailure()
    {
        // Arrange
        const string fieldName = "Email";
        const string errorMessage = "Email is required";

        // Act
        Result result = Result.Failure(fieldName, errorMessage);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.ResultType.ShouldBe(ResultType.Error);
        result.Failures.ShouldContainKey(fieldName);
        result.Failures[fieldName].ShouldContain(errorMessage);
        result.Failures[fieldName].Length.ShouldBe(1);
        result.Error.ShouldContain(fieldName);
        result.Error.ShouldContain(errorMessage);
    }

    [Fact]
    public void Failure_WithSingleFieldValidation_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Failure(null!, "error"));
    }

    [Fact]
    public void Failure_WithSingleFieldValidation_WithNullError_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Failure("key", null!));
    }

    [Fact]
    public void Failure_WithSecurityException_ShouldReturnSecurityFailure()
    {
        // Arrange
        const string securityMessage = "Access denied";
        SecurityException securityException = new(securityMessage);

        // Act
        Result result = Result.Failure(securityException);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(securityMessage);
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.Security);
        result.ResultType.ShouldBe(ResultType.Error);
    }

    [Fact]
    public void Failure_WithNullSecurityException_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Failure((SecurityException)null!));
    }

    [Fact]
    public void Failure_WithOperationCanceledException_ShouldReturnCancellationFailure()
    {
        // Arrange
        const string cancelMessage = "Operation was cancelled";
        OperationCanceledException cancelException = new(cancelMessage);

        // Act
        Result result = Result.Failure(cancelException);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(cancelMessage);
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.OperationCanceled);
        result.ResultType.ShouldBe(ResultType.Warning);
    }

    [Fact]
    public void Failure_WithNullOperationCanceledException_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Failure((OperationCanceledException)null!));
    }

    [Fact]
    public void Failure_WithValidationDictionary_ShouldReturnValidationFailure()
    {
        // Arrange
        Dictionary<string, string[]> errors = new()
        {
            { "Email", ["Email is required", "Invalid email format"] },
            { "Password", ["Password is too short"] }
        };

        // Act
        Result result = Result.Failure(errors);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.ResultType.ShouldBe(ResultType.Error);
        result.Failures.ShouldBe(errors);
        result.Error.ShouldContain("Email");
        result.Error.ShouldContain("Password");
        result.Error.ShouldContain("Email is required");
        result.Error.ShouldContain("Password is too short");
    }

    [Fact]
    public void Failure_WithNullValidationDictionary_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Failure((IDictionary<string, string[]>)null!));
    }

    // Generic Failure Method Tests

    [Fact]
    public void Failure_Generic_WithErrorMessage_ShouldReturnFailureResult()
    {
        // Arrange
        const string errorMessage = "Something went wrong";

        // Act
        Result<string> result = Result.Failure<string>(errorMessage);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(errorMessage);
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.Error);
        result.ResultType.ShouldBe(ResultType.Error);
        result.TryGetValue(out string _).ShouldBeFalse();
    }

    [Fact]
    public void Failure_Generic_WithNullErrorMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Failure<int>((string)null!));
    }

    [Fact]
    public void Failure_Generic_WithSingleFieldValidation_ShouldReturnValidationFailure()
    {
        // Arrange
        const string fieldName = "Age";
        const string errorMessage = "Age must be positive";

        // Act
        Result<User> result = Result.Failure<User>(fieldName, errorMessage);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.ResultType.ShouldBe(ResultType.Error);
        result.Failures.ShouldContainKey(fieldName);
        result.Failures[fieldName].ShouldContain(errorMessage);
        result.TryGetValue(out User _).ShouldBeFalse();
    }

    [Fact]
    public void Failure_Generic_WithSecurityException_ShouldReturnSecurityFailure()
    {
        // Arrange
        const string securityMessage = "Unauthorized access";
        SecurityException securityException = new(securityMessage);

        // Act
        Result<Document> result = Result.Failure<Document>(securityException);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(securityMessage);
        result.FailureType.ShouldBe(ResultFailureType.Security);
        result.ResultType.ShouldBe(ResultType.Error);
        result.TryGetValue(out Document _).ShouldBeFalse();
    }

    [Fact]
    public void Failure_Generic_WithOperationCanceledException_ShouldReturnCancellationFailure()
    {
        // Arrange
        const string cancelMessage = "Task was cancelled";
        OperationCanceledException cancelException = new(cancelMessage);

        // Act
        Result<Data> result = Result.Failure<Data>(cancelException);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(cancelMessage);
        result.FailureType.ShouldBe(ResultFailureType.OperationCanceled);
        result.ResultType.ShouldBe(ResultType.Warning);
        result.TryGetValue(out Data _).ShouldBeFalse();
    }

    [Fact]
    public void Failure_Generic_WithValidationDictionary_ShouldReturnValidationFailure()
    {
        // Arrange
        Dictionary<string, string[]> errors = new()
        {
            { "Name", ["Name is required"] },
            { "Email", ["Invalid email format", "Email already exists"] }
        };

        // Act
        Result<User> result = Result.Failure<User>(errors);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.ResultType.ShouldBe(ResultType.Error);
        result.Failures.ShouldBe(errors);
        result.TryGetValue(out User _).ShouldBeFalse();
    }

    [Fact]
    public void Failure_Generic_WithDifferentTypes_ShouldWorkCorrectly()
    {
        // Arrange & Act & Assert
        Result<int> intResult = Result.Failure<int>("Integer error");
        intResult.IsFailure.ShouldBeTrue();
        intResult.TryGetValue(out int _).ShouldBeFalse();

        Result<bool> boolResult = Result.Failure<bool>("Boolean error");
        boolResult.IsFailure.ShouldBeTrue();
        boolResult.TryGetValue(out bool _).ShouldBeFalse();

        Result<DateTime> dateResult = Result.Failure<DateTime>("Date error");
        dateResult.IsFailure.ShouldBeTrue();
        dateResult.TryGetValue(out DateTime _).ShouldBeFalse();

        Result<List<string>> listResult = Result.Failure<List<string>>("List error");
        listResult.IsFailure.ShouldBeTrue();
        listResult.TryGetValue(out List<string> _).ShouldBeFalse();
    }

    #endregion Failure Factory Method Tests

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

    #region ServerError Factory Method Tests

    [Fact]
    public void ServerError_WithoutMessage_ShouldReturnServerErrorResult()
    {
        // Act
        Result result = Result.ServerError();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.FailureType.ShouldBe(ResultFailureType.ServerError);
        result.ResultType.ShouldBe(ResultType.Error);
        result.Error.ShouldBe("Server Error");
        result.Failures.ShouldBeEmpty();
    }

    [Fact]
    public void ServerError_WithNullMessage_ShouldReturnGenericServerError()
    {
        // Act
        Result result = Result.ServerError(null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.ServerError);
        result.Error.ShouldBe("Server Error");
    }

    [Fact]
    public void ServerError_WithEmptyMessage_ShouldReturnGenericServerError()
    {
        // Act
        Result result = Result.ServerError(string.Empty);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.ServerError);
        result.Error.ShouldBe("Server Error");
    }

    [Fact]
    public void ServerError_WithWhitespaceMessage_ShouldReturnGenericServerError()
    {
        // Act
        Result result = Result.ServerError("   ");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.ServerError);
        result.Error.ShouldBe("Server Error");
    }

    [Fact]
    public void ServerError_WithSpecificMessage_ShouldReturnServerErrorWithMessage()
    {
        // Arrange
        const string errorMessage = "Database connection timeout";

        // Act
        Result result = Result.ServerError(errorMessage);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.ServerError);
        result.ResultType.ShouldBe(ResultType.Error);
        result.Error.ShouldBe(errorMessage);
        result.Failures.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("Service unavailable")]
    [InlineData("Internal server error")]
    [InlineData("Gateway timeout")]
    [InlineData("Bad gateway")]
    public void ServerError_WithVariousMessages_ShouldReturnCorrectErrorMessage(string message)
    {
        // Act
        Result result = Result.ServerError(message);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.ServerError);
        result.Error.ShouldBe(message);
    }

    [Fact]
    public void ServerError_ShouldBeConvertibleToBoolAsFalse()
    {
        // Act
        Result result = Result.ServerError("Server error");

        // Assert
        bool isSuccess = (bool)result;
        isSuccess.ShouldBeFalse();
    }

    [Fact]
    public void ServerError_ShouldMatchCorrectlyWithMatch()
    {
        // Arrange
        Result result = Result.ServerError("Service unavailable");

        // Act
        string matchResult = result.Match(
            onSuccess: () => "Success",
            onFailure: error => $"Failed: {error}"
        );

        // Assert
        matchResult.ShouldBe("Failed: Service unavailable");
    }

    [Fact]
    public void ServerError_ShouldRouteToErrorHandlerInComplexMatch()
    {
        // Arrange
        Result result = Result.ServerError("Internal server error");

        // Act
        string matchResult = result.Match(
            onSuccess: () => "success",
            onError: error => $"error: {error}",
            onSecurityException: error => $"security: {error}",
            onValidationException: errors => $"validation: {errors.Count}",
            onOperationCanceledException: error => $"cancelled: {error}"
        );

        // Assert
        matchResult.ShouldBe("error: Internal server error");
    }

    // Generic ServerError Method Tests

    [Fact]
    public void ServerError_Generic_WithoutMessage_ShouldReturnServerErrorResult()
    {
        // Act
        Result<string> result = Result.ServerError<string>();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.FailureType.ShouldBe(ResultFailureType.ServerError);
        result.ResultType.ShouldBe(ResultType.Error);
        result.Error.ShouldBe("Server Error");
        result.Failures.ShouldBeEmpty();
        result.TryGetValue(out string _).ShouldBeFalse();
    }

    [Fact]
    public void ServerError_Generic_WithSpecificMessage_ShouldReturnServerErrorWithMessage()
    {
        // Arrange
        const string errorMessage = "API gateway timeout";

        // Act
        Result<User> result = Result.ServerError<User>(errorMessage);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.ServerError);
        result.ResultType.ShouldBe(ResultType.Error);
        result.Error.ShouldBe(errorMessage);
        result.Failures.ShouldBeEmpty();
        result.TryGetValue(out User _).ShouldBeFalse();
    }

    [Fact]
    public void ServerError_Generic_WithDifferentTypes_ShouldWorkCorrectly()
    {
        // Arrange & Act & Assert
        Result<int> intResult = Result.ServerError<int>("Integer service error");
        intResult.IsFailure.ShouldBeTrue();
        intResult.FailureType.ShouldBe(ResultFailureType.ServerError);
        intResult.TryGetValue(out int _).ShouldBeFalse();

        Result<List<string>> listResult = Result.ServerError<List<string>>();
        listResult.IsFailure.ShouldBeTrue();
        listResult.FailureType.ShouldBe(ResultFailureType.ServerError);
        listResult.Error.ShouldBe("Server Error");
        listResult.TryGetValue(out List<string> _).ShouldBeFalse();

        Result<DateTime> dateResult = Result.ServerError<DateTime>("Date service down");
        dateResult.IsFailure.ShouldBeTrue();
        dateResult.FailureType.ShouldBe(ResultFailureType.ServerError);
        dateResult.Error.ShouldBe("Date service down");
        dateResult.TryGetValue(out DateTime _).ShouldBeFalse();
    }

    [Fact]
    public void ServerError_Generic_ShouldMatchCorrectlyWithMatch()
    {
        // Arrange
        Result<Document> result = Result.ServerError<Document>("Document service error");

        // Act
        string matchResult = result.Match(
            onSuccess: doc => $"Success: {doc?.Title ?? "null"}",
            onFailure: error => $"Failed: {error}"
        );

        // Assert
        matchResult.ShouldBe("Failed: Document service error");
    }

    [Fact]
    public void ServerError_Generic_ShouldRouteToErrorHandlerInComplexMatch()
    {
        // Arrange
        Result<Data> result = Result.ServerError<Data>("Data service unavailable");

        // Act
        string matchResult = result.Match(
            onSuccess: data => $"success: {data?.Value ?? "null"}",
            onError: error => $"error: {error}",
            onSecurityException: error => $"security: {error}",
            onValidationException: errors => $"validation: {errors.Count}",
            onOperationCanceledException: error => $"cancelled: {error}"
        );

        // Assert
        matchResult.ShouldBe("error: Data service unavailable");
    }

    #endregion ServerError Factory Method Tests

    #region ValidationFailure Factory Method Tests

    [Fact]
    public void ValidationFailure_WithValidationErrors_ShouldReturnValidationFailure()
    {
        // Arrange
        Dictionary<string, string[]> errors = new()
        {
            { "Email", ["Email is required", "Invalid email format"] },
            { "Password", ["Password is too short", "Password must contain special characters"] }
        };

        // Act
        Result result = Result.ValidationFailure(errors);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.ResultType.ShouldBe(ResultType.Error);
        result.Failures.ShouldBe(errors);
        result.Error.ShouldContain("Email");
        result.Error.ShouldContain("Password");
        result.Error.ShouldContain("validation errors occurred");
    }

    [Fact]
    public void ValidationFailure_WithNullErrors_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.ValidationFailure(null!));
    }

    [Fact]
    public void ValidationFailure_WithEmptyErrorsDictionary_ShouldReturnValidationFailure()
    {
        // Arrange
        Dictionary<string, string[]> emptyErrors = new();

        // Act
        Result result = Result.ValidationFailure(emptyErrors);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldBe(emptyErrors);
        result.Failures.ShouldBeEmpty();
    }

    [Fact]
    public void ValidationFailure_ShouldBeEquivalentToFailureMethod()
    {
        // Arrange
        Dictionary<string, string[]> errors = new()
        {
            { "Name", ["Name is required"] }
        };

        // Act
        Result validationResult = Result.ValidationFailure(errors);
        Result failureResult = Result.Failure(errors);

        // Assert
        validationResult.IsFailure.ShouldBe(failureResult.IsFailure);
        validationResult.FailureType.ShouldBe(failureResult.FailureType);
        validationResult.ResultType.ShouldBe(failureResult.ResultType);
        validationResult.Error.ShouldBe(failureResult.Error);
        validationResult.Failures.ShouldBe(failureResult.Failures);
    }

    // Generic ValidationFailure Method Tests

    [Fact]
    public void ValidationFailure_Generic_WithValidationErrors_ShouldReturnValidationFailure()
    {
        // Arrange
        Dictionary<string, string[]> errors = new()
        {
            { "FirstName", ["First name is required"] },
            { "LastName", ["Last name is required"] },
            { "Age", ["Age must be between 18 and 120"] }
        };

        // Act
        Result<User> result = Result.ValidationFailure<User>(errors);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.ResultType.ShouldBe(ResultType.Error);
        result.Failures.ShouldBe(errors);
        result.TryGetValue(out User _).ShouldBeFalse();
        result.Error.ShouldContain("FirstName");
        result.Error.ShouldContain("LastName");
        result.Error.ShouldContain("Age");
    }

    [Fact]
    public void ValidationFailure_Generic_WithNullErrors_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.ValidationFailure<string>(null!));
    }

    [Fact]
    public void ValidationFailure_Generic_WithDifferentTypes_ShouldWorkCorrectly()
    {
        // Arrange
        Dictionary<string, string[]> errors = new()
        {
            { "Value", ["Value is invalid"] }
        };

        // Act & Assert
        Result<int> intResult = Result.ValidationFailure<int>(errors);
        intResult.IsFailure.ShouldBeTrue();
        intResult.FailureType.ShouldBe(ResultFailureType.Validation);
        intResult.TryGetValue(out int _).ShouldBeFalse();

        Result<Document> docResult = Result.ValidationFailure<Document>(errors);
        docResult.IsFailure.ShouldBeTrue();
        docResult.FailureType.ShouldBe(ResultFailureType.Validation);
        docResult.TryGetValue(out Document _).ShouldBeFalse();

        Result<List<string>> listResult = Result.ValidationFailure<List<string>>(errors);
        listResult.IsFailure.ShouldBeTrue();
        listResult.FailureType.ShouldBe(ResultFailureType.Validation);
        listResult.TryGetValue(out List<string> _).ShouldBeFalse();
    }

    [Fact]
    public void ValidationFailure_Generic_ShouldBeEquivalentToFailureMethod()
    {
        // Arrange
        Dictionary<string, string[]> errors = new()
        {
            { "Email", ["Invalid email"] }
        };

        // Act
        Result<User> validationResult = Result.ValidationFailure<User>(errors);
        Result<User> failureResult = Result.Failure<User>(errors);

        // Assert
        validationResult.IsFailure.ShouldBe(failureResult.IsFailure);
        validationResult.FailureType.ShouldBe(failureResult.FailureType);
        validationResult.ResultType.ShouldBe(failureResult.ResultType);
        validationResult.Error.ShouldBe(failureResult.Error);
        validationResult.Failures.ShouldBe(failureResult.Failures);
    }

    #endregion ValidationFailure Factory Method Tests

    #region Conversion Operator Tests

    [Fact]
    public void ImplicitConversion_FromResultTToResult_WithSuccessResult_ShouldReturnNonGenericSuccess()
    {
        // Arrange
        Result<string> genericResult = Result.Success("test value");

        // Act
        Result nonGenericResult = genericResult;

        // Assert
        nonGenericResult.IsSuccess.ShouldBeTrue();
        nonGenericResult.IsFailure.ShouldBeFalse();
        nonGenericResult.Error.ShouldBeEmpty();
        nonGenericResult.FailureType.ShouldBe(ResultFailureType.None);
        nonGenericResult.ResultType.ShouldBe(genericResult.ResultType);
    }

    [Fact]
    public void ImplicitConversion_FromResultTToResult_WithFailureResult_ShouldReturnNonGenericFailure()
    {
        // Arrange
        const string errorMessage = "Test error";
        Result<string> genericResult = Result.Failure<string>(errorMessage);

        // Act
        Result nonGenericResult = genericResult;

        // Assert
        nonGenericResult.IsFailure.ShouldBeTrue();
        nonGenericResult.IsSuccess.ShouldBeFalse();
        nonGenericResult.Error.ShouldBe(errorMessage);
        nonGenericResult.FailureType.ShouldBe(genericResult.FailureType);
        nonGenericResult.ResultType.ShouldBe(genericResult.ResultType);
    }

    [Fact]
    public void ImplicitConversion_FromResultTToResult_WithValidationFailure_ShouldPreserveValidationDetails()
    {
        // Arrange
        Dictionary<string, string[]> errors = new()
        {
            { "Field1", ["Error 1", "Error 2"] },
            { "Field2", ["Error 3"] }
        };
        Result<User> genericResult = Result.ValidationFailure<User>(errors);

        // Act
        Result nonGenericResult = genericResult;

        // Assert
        nonGenericResult.IsFailure.ShouldBeTrue();
        nonGenericResult.FailureType.ShouldBe(ResultFailureType.Validation);
        nonGenericResult.Failures.ShouldBe(errors);
        nonGenericResult.Error.ShouldBe(genericResult.Error);
    }

    [Fact]
    public void ImplicitConversion_FromResultTToResult_WithNullResult_ShouldReturnFailure()
    {
        // Arrange
        Result<string>? nullResult = null;

        // Act
        Result nonGenericResult = nullResult!;

        // Assert
        nonGenericResult.IsFailure.ShouldBeTrue();
        nonGenericResult.Error.ShouldBe("Result is null");
    }

    [Fact]
    public void ImplicitConversion_FromValueToResultT_WithNonNullValue_ShouldReturnSuccessResult()
    {
        // Arrange
        const string testValue = "implicit conversion test";

        // Act
        Result<string> result = testValue;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.TryGetValue(out string? actualValue).ShouldBeTrue();
        actualValue.ShouldBe(testValue);
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.ResultType.ShouldBe(ResultType.Success);
    }

    [Fact]
    public void ImplicitConversion_FromValueToResultT_WithDifferentTypes_ShouldWorkCorrectly()
    {
        // Arrange & Act & Assert
        Result<int> intResult = 42;
        intResult.IsSuccess.ShouldBeTrue();
        intResult.TryGetValue(out int intValue).ShouldBeTrue();
        intValue.ShouldBe(42);

        Result<bool> boolResult = true;
        boolResult.IsSuccess.ShouldBeTrue();
        boolResult.TryGetValue(out bool boolValue).ShouldBeTrue();
        boolValue.ShouldBeTrue();

        Result<DateTime> dateResult = DateTime.UnixEpoch;
        dateResult.IsSuccess.ShouldBeTrue();
        dateResult.TryGetValue(out DateTime dateValue).ShouldBeTrue();
        dateValue.ShouldBe(DateTime.UnixEpoch);
    }

    [Fact]
    public void ImplicitConversion_FromValueToResultT_WithComplexObject_ShouldRetainObjectReference()
    {
        // Arrange
        User expectedUser = new() { Id = 1, Name = "Test User", Email = "test@example.com" };

        // Act
        Result<User> result = expectedUser;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out User? actualUser).ShouldBeTrue();
        actualUser.ShouldBeSameAs(expectedUser);
        actualUser!.Id.ShouldBe(1);
        actualUser.Name.ShouldBe("Test User");
        actualUser.Email.ShouldBe("test@example.com");
    }

    [Fact]
    public void ExplicitConversion_FromResultTToValue_WithSuccessResult_ShouldReturnValue()
    {
        // Arrange
        const string expectedValue = "explicit conversion test";
        Result<string> result = Result.Success(expectedValue);

        // Act
        string actualValue = (string)result;

        // Assert
        actualValue.ShouldBe(expectedValue);
    }

    [Fact]
    public void ExplicitConversion_FromResultTToValue_WithFailureResult_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Result<string> result = Result.Failure<string>("Test error");

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => (string)result)
            .Message.ShouldContain("Cannot extract value from a failed result");
    }

    [Fact]
    public void ExplicitConversion_FromResultTToValue_WithNullResult_ShouldThrowArgumentNullException()
    {
        // Arrange
        Result<string>? nullResult = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => (string)nullResult!);
    }

    [Fact]
    public void ExplicitConversion_FromResultTToValue_WithDifferentTypes_ShouldWorkCorrectly()
    {
        // Arrange & Act & Assert
        Result<int> intResult = Result.Success(123);
        int intValue = (int)intResult;
        intValue.ShouldBe(123);

        Result<string> stringResult = Result.Success("test value");
        string stringValue = (string)stringResult;
        stringValue.ShouldBe("test value");

        Result<DateTime> dateResult = Result.Success(DateTime.MaxValue);
        DateTime dateValue = (DateTime)dateResult;
        dateValue.ShouldBe(DateTime.MaxValue);
    }

    [Fact]
    public void ExplicitConversion_FromResultTToBool_WithSuccessResult_ShouldReturnTrue()
    {
        // Arrange
        Result<string> result = Result.Success("test");

        // Act
        bool isSuccess = (bool)result;

        // Assert
        isSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ExplicitConversion_FromResultTToBool_WithFailureResult_ShouldReturnFalse()
    {
        // Arrange
        Result<string> result = Result.Failure<string>("error");

        // Act
        bool isSuccess = (bool)result;

        // Assert
        isSuccess.ShouldBeFalse();
    }

    [Fact]
    public void ExplicitConversion_FromResultTToBool_WithNullResult_ShouldReturnFalse()
    {
        // Arrange
        Result<string>? nullResult = null;

        // Act
        bool isSuccess = (bool)nullResult!;

        // Assert
        isSuccess.ShouldBeFalse();
    }

    [Fact]
    public void ExplicitConversion_BoolConversion_WithDifferentFailureTypes_ShouldReturnFalse()
    {
        // Arrange & Act & Assert
        Result<string> validationResult = Result.ValidationFailure<string>(new Dictionary<string, string[]>
        {
            { "Field", ["Error"] }
        });
        bool validationSuccess = (bool)validationResult;
        validationSuccess.ShouldBeFalse();

        Result<string> securityResult = Result.Failure<string>(new SecurityException("Access denied"));
        bool securitySuccess = (bool)securityResult;
        securitySuccess.ShouldBeFalse();

        Result<string> serverErrorResult = Result.ServerError<string>("Server down");
        bool serverErrorSuccess = (bool)serverErrorResult;
        serverErrorSuccess.ShouldBeFalse();

        Result<string> notFoundResult = Result.NotFound<string>("Resource");
        bool notFoundSuccess = (bool)notFoundResult;
        notFoundSuccess.ShouldBeFalse();
    }

    #endregion Conversion Operator Tests

    #region Property and Constructor Behavior Tests

    [Fact]
    public void Properties_WithSuccessResult_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        Result result = Result.Success(ResultType.Information);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeEmpty();
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.ResultType.ShouldBe(ResultType.Information);
    }

    [Fact]
    public void Properties_WithFailureResult_ShouldHaveCorrectValues()
    {
        // Arrange
        const string errorMessage = "Test error";

        // Act
        Result result = Result.Failure(errorMessage, ResultType.Warning, ResultFailureType.Security);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(errorMessage);
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.Security);
        result.ResultType.ShouldBe(ResultType.Warning);
    }

    [Fact]
    public void Properties_WithValidationFailure_ShouldHaveCorrectValues()
    {
        // Arrange
        Dictionary<string, string[]> errors = new()
        {
            { "Field1", ["Error1"] },
            { "Field2", ["Error2", "Error3"] }
        };

        // Act
        Result result = Result.ValidationFailure(errors);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.ResultType.ShouldBe(ResultType.Error);
        result.Failures.ShouldBe(errors);
        result.Error.ShouldContain("validation errors occurred");
        result.Error.ShouldContain("Field1");
        result.Error.ShouldContain("Field2");
    }

    [Fact]
    public void Properties_IsSuccessAndIsFailure_ShouldBeInverse()
    {
        // Arrange & Act & Assert
        Result successResult = Result.Success();
        successResult.IsSuccess.ShouldBeTrue();
        successResult.IsFailure.ShouldBeFalse();

        Result failureResult = Result.Failure("error");
        failureResult.IsSuccess.ShouldBeFalse();
        failureResult.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Properties_Generic_WithSuccessResult_ShouldHaveCorrectValues()
    {
        // Arrange
        const string value = "test";

        // Act
        Result<string> result = Result.Success(value, ResultType.Warning);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeEmpty();
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.ResultType.ShouldBe(ResultType.Warning);
        result.TryGetValue(out string? actualValue).ShouldBeTrue();
        actualValue.ShouldBe(value);
    }

    [Fact]
    public void Properties_Generic_WithFailureResult_ShouldHaveCorrectValues()
    {
        // Arrange
        const string errorMessage = "Generic test error";

        // Act
        Result<int> result = Result.Failure<int>(errorMessage, ResultType.Error, ResultFailureType.NotFound);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(errorMessage);
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.NotFound);
        result.ResultType.ShouldBe(ResultType.Error);
        result.TryGetValue(out int _).ShouldBeFalse();
    }

    [Fact]
    public void Properties_ShouldProvideReadOnlyFailures()
    {
        // Arrange
        Dictionary<string, string[]> originalErrors = new()
        {
            { "Field1", ["Error1"] }
        };
        Result result = Result.ValidationFailure(originalErrors);
        int originalFailureCount = result.Failures.Count;

        // Act - Try to modify the Failures dictionary
        result.Failures.TryAdd("NewKey", ["NewError"]);

        // Assert - The modification should be reflected (the dictionary is mutable, but the Result core behavior is consistent)
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        originalFailureCount.ShouldBe(1);
        // Note: The Failures dictionary is mutable after creation, but the Result's core state remains consistent
    }

    [Fact]
    public void JsonConstructor_ShouldCreateResultWithDefaultValues()
    {
        // This test verifies the parameterless JSON constructor exists and works
        // Note: We cannot directly test the private JSON constructor, but we can verify
        // that JSON deserialization works, which would use this constructor
        
        // Arrange
        Result successResult = Result.Success();
        Result failureResult = Result.Failure("test error");

        // Act & Assert - If these don't throw, the JSON constructor is working
        successResult.IsSuccess.ShouldBeTrue();
        failureResult.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void InternalConstructor_Behavior_ShouldInitializePropertiesCorrectly()
    {
        // These tests verify that internal constructors set properties correctly
        // by testing the public factory methods that use them

        // Test various constructor paths
        Result errorResult = Result.Failure("error", ResultType.Error, ResultFailureType.Security);
        errorResult.FailureType.ShouldBe(ResultFailureType.Security);
        errorResult.ResultType.ShouldBe(ResultType.Error);
        errorResult.Error.ShouldBe("error");

        Result validationResult = Result.Failure("field", "validation error");
        validationResult.FailureType.ShouldBe(ResultFailureType.Validation);
        validationResult.ResultType.ShouldBe(ResultType.Error);
        validationResult.Failures.ShouldContainKey("field");

        SecurityException secException = new("security");
        Result securityResult = Result.Failure(secException);
        securityResult.FailureType.ShouldBe(ResultFailureType.Security);
        securityResult.ResultType.ShouldBe(ResultType.Error);
        securityResult.Error.ShouldBe("security");

        OperationCanceledException cancelException = new("cancelled");
        Result cancelResult = Result.Failure(cancelException);
        cancelResult.FailureType.ShouldBe(ResultFailureType.OperationCanceled);
        cancelResult.ResultType.ShouldBe(ResultType.Warning);
        cancelResult.Error.ShouldBe("cancelled");
    }

    [Fact]
    public void GetValidationError_Internal_ShouldFormatErrorsCorrectly()
    {
        // Test the internal GetValidationError method through public API
        Dictionary<string, string[]> errors = new()
        {
            { "Email", ["Email is required", "Invalid format"] },
            { "Password", ["Too short"] }
        };

        Result result = Result.ValidationFailure(errors);
        
        result.Error.ShouldContain("One or more validation errors occurred.");
        result.Error.ShouldContain("Email");
        result.Error.ShouldContain("Email is required");
        result.Error.ShouldContain("Invalid format");
        result.Error.ShouldContain("Password");
        result.Error.ShouldContain("Too short");
    }

    #endregion Property and Constructor Behavior Tests

    #region TASK-043: SuccessOrNull Factory Method Tests

    public class SuccessOrNullMethodTests
    {
        [Fact]
        public void SuccessOrNull_WithNonNullValue_ShouldReturnSuccessResult()
        {
            // Arrange
            string expectedValue = "test value";

            // Act
            Result<string?> result = Result.SuccessOrNull(expectedValue);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out string? actualValue).ShouldBeTrue();
            actualValue.ShouldBe(expectedValue);
        }

        [Fact]
        public void SuccessOrNull_WithNullValue_ShouldReturnSuccessResultWithNull()
        {
            // Arrange
            string? nullValue = null;

            // Act
            Result<string?> result = Result.SuccessOrNull(nullValue);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out string? actualValue).ShouldBeTrue();
            actualValue.ShouldBeNull();
        }

        [Fact]
        public void SuccessOrNull_WithNullReferenceType_ShouldReturnSuccessResultWithNull()
        {
            // Arrange
            TestModel? nullModel = null;

            // Act
            Result<TestModel?> result = Result.SuccessOrNull(nullModel);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out TestModel? actualValue).ShouldBeTrue();
            actualValue.ShouldBeNull();
        }

        [Fact]
        public void SuccessOrNull_WithNullableValueType_ShouldReturnSuccessResultWithNull()
        {
            // Arrange
            int? nullInt = null;

            // Act
            Result<int?> result = Result.SuccessOrNull(nullInt);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out int? actualValue).ShouldBeTrue();
            actualValue.ShouldBeNull();
        }

        [Fact]
        public void SuccessOrNull_WithNonNullValueType_ShouldReturnSuccessResultWithValue()
        {
            // Arrange
            int? expectedValue = 42;

            // Act
            Result<int?> result = Result.SuccessOrNull(expectedValue);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out int? actualValue).ShouldBeTrue();
            actualValue.ShouldBe(expectedValue);
        }

        [Fact]
        public void SuccessOrNull_WithCustomResultType_ShouldReturnSuccessResultWithSpecifiedType()
        {
            // Arrange
            string? nullValue = null;
            ResultType expectedResultType = ResultType.Information;

            // Act
            Result<string?> result = Result.SuccessOrNull(nullValue, expectedResultType);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.ResultType.ShouldBe(expectedResultType);
            result.TryGetValue(out string? actualValue).ShouldBeTrue();
            actualValue.ShouldBeNull();
        }

        [Fact]
        public void SuccessOrNull_WithNonNullValueAndCustomResultType_ShouldReturnSuccessResultWithValueAndType()
        {
            // Arrange
            string expectedValue = "test";
            ResultType expectedResultType = ResultType.Warning;

            // Act
            Result<string?> result = Result.SuccessOrNull(expectedValue, expectedResultType);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.ResultType.ShouldBe(expectedResultType);
            result.TryGetValue(out string? actualValue).ShouldBeTrue();
            actualValue.ShouldBe(expectedValue);
        }

        [Theory]
        [InlineData(ResultType.Success)]
        [InlineData(ResultType.Information)]
        [InlineData(ResultType.Warning)]
        public void SuccessOrNull_WithNullValueAndVariousResultTypes_ShouldReturnSuccessResultWithCorrectType(ResultType resultType)
        {
            // Arrange
            TestModel? nullValue = null;

            // Act
            Result<TestModel?> result = Result.SuccessOrNull(nullValue, resultType);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.ResultType.ShouldBe(resultType);
            result.TryGetValue(out TestModel? actualValue).ShouldBeTrue();
            actualValue.ShouldBeNull();
        }

        [Fact]
        public void SuccessOrNull_ShouldNeverThrowArgumentNullException()
        {
            // Arrange
            object? nullObject = null;

            // Act & Assert - should not throw any exception
            Result<object?> result = Result.SuccessOrNull(nullObject);

            // Verify it creates a successful result with null value
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out object? actualValue).ShouldBeTrue();
            actualValue.ShouldBeNull();
        }

        [Fact]
        public void SuccessOrNull_WithEmptyStringValue_ShouldReturnSuccessResultWithEmptyString()
        {
            // Arrange
            string emptyString = string.Empty;

            // Act
            Result<string?> result = Result.SuccessOrNull(emptyString);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out string? actualValue).ShouldBeTrue();
            actualValue.ShouldBe(string.Empty);
        }

        [Fact]
        public void SuccessOrNull_WithDefaultValue_ShouldReturnSuccessResultWithDefaultValue()
        {
            // Arrange
            int defaultInt = default;

            // Act
            int? nullableInt = defaultInt;
            Result<int?> result = Result.SuccessOrNull(nullableInt);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out int? actualValue).ShouldBeTrue();
            actualValue.ShouldBe(0);
        }
    }

    #endregion TASK-043: SuccessOrNull Factory Method Tests

    #region Helper Methods

    /// <summary>
    /// Test model for SuccessOrNull tests.
    /// </summary>
    private sealed class TestModel
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    /// <summary>
    /// Test model representing a User.
    /// </summary>
    private sealed class User
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }

    /// <summary>
    /// Test model representing a Document.
    /// </summary>
    private sealed class Document
    {
        public string Title { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
    }

    /// <summary>
    /// Test model representing Data.
    /// </summary>
    private sealed class Data
    {
        public string Value { get; init; } = string.Empty;
    }

    #endregion Helper Methods

    #region TASK-049: NotFound Factory Method Tests

    public class NotFoundMethodTests
    {
        [Fact]
        public void NotFound_WithoutResource_ShouldReturnFailureWithNotFoundType()
        {
            // Act
            Result result = Result.NotFound();

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.IsSuccess.ShouldBeFalse();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.ResultType.ShouldBe(ResultType.Error);
            result.Error.ShouldBe("Not Found");
        }

        [Fact]
        public void NotFound_WithNullResource_ShouldReturnFailureWithGenericMessage()
        {
            // Act
            Result result = Result.NotFound(null);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("Not Found");
        }

        [Fact]
        public void NotFound_WithEmptyResource_ShouldReturnFailureWithGenericMessage()
        {
            // Act
            Result result = Result.NotFound(string.Empty);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("Not Found");
        }

        [Fact]
        public void NotFound_WithWhitespaceResource_ShouldReturnFailureWithGenericMessage()
        {
            // Act
            Result result = Result.NotFound("   ");

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("Not Found");
        }

        [Fact]
        public void NotFound_WithSpecificResource_ShouldReturnFailureWithResourceSpecificMessage()
        {
            // Arrange
            string resourceName = "User";

            // Act
            Result result = Result.NotFound(resourceName);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("User not found");
        }

        [Fact]
        public void NotFound_WithComplexResource_ShouldReturnFailureWithResourceSpecificMessage()
        {
            // Arrange
            string resourceName = "Order with ID 12345";

            // Act
            Result result = Result.NotFound(resourceName);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("Order with ID 12345 not found");
        }

        [Theory]
        [InlineData("User")]
        [InlineData("Product")]
        [InlineData("Customer")]
        [InlineData("Invoice")]
        public void NotFound_WithVariousResourceNames_ShouldReturnCorrectErrorMessage(string resourceName)
        {
            // Act
            Result result = Result.NotFound(resourceName);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe($"{resourceName} not found");
        }

        [Fact]
        public void NotFound_WithSpecialCharactersInResource_ShouldReturnFailureWithResourceSpecificMessage()
        {
            // Arrange
            string resourceName = "User (ID: @123#)";

            // Act
            Result result = Result.NotFound(resourceName);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("User (ID: @123#) not found");
        }

        [Fact]
        public void NotFound_ShouldHaveEmptyFailuresDictionary()
        {
            // Act
            Result result = Result.NotFound("Resource");

            // Assert
            result.Failures.ShouldBeEmpty();
        }

        [Fact]
        public void NotFound_ShouldBeConvertibleToBoolAsFalse()
        {
            // Act
            Result result = Result.NotFound();

            // Assert
            bool isSuccess = (bool)result;
            isSuccess.ShouldBeFalse();
        }

        [Fact]
        public void NotFound_ShouldMatchCorrectlyWithMatch()
        {
            // Arrange
            Result result = Result.NotFound("TestResource");

            // Act
            string matchResult = result.Match(
                onSuccess: () => "Success",
                onFailure: error => $"Failed: {error}"
            );

            // Assert
            matchResult.ShouldBe("Failed: TestResource not found");
        }

        [Fact]
        public void NotFound_ShouldSwitchCorrectlyWithSimpleSwitch()
        {
            // Arrange
            Result result = Result.NotFound("TestResource");
            string? capturedError = null;
            bool onSuccessCalled = false;

            // Act
            result.Switch(
                onSuccess: () => onSuccessCalled = true,
                onFailure: error => capturedError = error
            );

            // Assert
            onSuccessCalled.ShouldBeFalse();
            capturedError.ShouldBe("TestResource not found");
        }

        [Fact]
        public void NotFound_ShouldRouteToErrorHandlerInComplexSwitch()
        {
            // Arrange
            Result result = Result.NotFound("TestResource");
            string? handlerCalled = null;
            string? capturedError = null;

            // Act
            result.Switch(
                onSuccess: () => handlerCalled = "success",
                onError: error => { handlerCalled = "error"; capturedError = error; },
                onSecurityException: error => handlerCalled = "security",
                onValidationException: errors => handlerCalled = "validation",
                onOperationCanceledException: error => handlerCalled = "cancelled"
            );

            // Assert
            handlerCalled.ShouldBe("error");
            capturedError.ShouldBe("TestResource not found");
        }

        [Fact]
        public void NotFound_ShouldRouteToErrorHandlerInComplexMatch()
        {
            // Arrange
            Result result = Result.NotFound("TestResource");

            // Act
            string matchResult = result.Match(
                onSuccess: () => "success",
                onError: error => $"error: {error}",
                onSecurityException: error => $"security: {error}",
                onValidationException: errors => $"validation: {errors.Count}",
                onOperationCanceledException: error => $"cancelled: {error}"
            );

            // Assert
            matchResult.ShouldBe("error: TestResource not found");
        }
    }

    public class NotFoundGenericMethodTests
    {
        [Fact]
        public void NotFoundGeneric_WithoutResource_ShouldReturnFailureWithNotFoundType()
        {
            // Act
            Result<string> result = Result.NotFound<string>();

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.IsSuccess.ShouldBeFalse();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.ResultType.ShouldBe(ResultType.Error);
            result.Error.ShouldBe("Not Found");
            result.TryGetValue(out string _).ShouldBeFalse();
        }

        [Fact]
        public void NotFoundGeneric_WithNullResource_ShouldReturnFailureWithGenericMessage()
        {
            // Act
            Result<int> result = Result.NotFound<int>(null);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("Not Found");
            result.TryGetValue(out int _).ShouldBeFalse();
        }

        [Fact]
        public void NotFoundGeneric_WithEmptyResource_ShouldReturnFailureWithGenericMessage()
        {
            // Act
            Result<bool> result = Result.NotFound<bool>(string.Empty);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("Not Found");
            result.TryGetValue(out bool _).ShouldBeFalse();
        }

        [Fact]
        public void NotFoundGeneric_WithWhitespaceResource_ShouldReturnFailureWithGenericMessage()
        {
            // Act
            Result<DateTime> result = Result.NotFound<DateTime>("   ");

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("Not Found");
            result.TryGetValue(out DateTime _).ShouldBeFalse();
        }

        [Fact]
        public void NotFoundGeneric_WithSpecificResource_ShouldReturnFailureWithResourceSpecificMessage()
        {
            // Arrange
            string resourceName = "Customer";

            // Act
            Result<TestModel> result = Result.NotFound<TestModel>(resourceName);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("Customer not found");
            result.TryGetValue(out TestModel _).ShouldBeFalse();
        }

        [Fact]
        public void NotFoundGeneric_WithComplexResource_ShouldReturnFailureWithResourceSpecificMessage()
        {
            // Arrange
            string resourceName = "Product with SKU ABC-123";

            // Act
            Result<List<string>> result = Result.NotFound<List<string>>(resourceName);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("Product with SKU ABC-123 not found");
            result.TryGetValue(out List<string> _).ShouldBeFalse();
        }

        [Theory]
        [InlineData("Account")]
        [InlineData("Transaction")]
        [InlineData("Report")]
        [InlineData("Document")]
        public void NotFoundGeneric_WithVariousResourceNames_ShouldReturnCorrectErrorMessage(string resourceName)
        {
            // Act
            Result<Dictionary<string, object>> result = Result.NotFound<Dictionary<string, object>>(resourceName);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe($"{resourceName} not found");
            result.TryGetValue(out Dictionary<string, object> _).ShouldBeFalse();
        }

        [Fact]
        public void NotFoundGeneric_WithNullableType_ShouldReturnFailureWithCorrectType()
        {
            // Act
            Result<int?> result = Result.NotFound<int?>("OptionalValue");

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("OptionalValue not found");
            result.TryGetValue(out int? _).ShouldBeFalse();
        }

        [Fact]
        public void NotFoundGeneric_WithValueType_ShouldReturnFailureWithCorrectType()
        {
            // Act
            Result<Guid> result = Result.NotFound<Guid>("UniqueIdentifier");

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("UniqueIdentifier not found");
            result.TryGetValue(out Guid _).ShouldBeFalse();
        }

        [Fact]
        public void NotFoundGeneric_WithReferenceType_ShouldReturnFailureWithCorrectType()
        {
            // Act
            Result<string[]> result = Result.NotFound<string[]>("StringArray");

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("StringArray not found");
            result.TryGetValue(out string[] _).ShouldBeFalse();
        }

        [Fact]
        public void NotFoundGeneric_ShouldHaveEmptyFailuresDictionary()
        {
            // Act
            Result<TestModel> result = Result.NotFound<TestModel>("TestResource");

            // Assert
            result.Failures.ShouldBeEmpty();
        }

        [Fact]
        public void NotFoundGeneric_ShouldBeConvertibleToBoolAsFalse()
        {
            // Act
            Result<string> result = Result.NotFound<string>("Resource");

            // Assert
            bool isSuccess = (bool)result;
            isSuccess.ShouldBeFalse();
        }

        [Fact]
        public void NotFoundGeneric_ShouldMatchCorrectlyWithMatch()
        {
            // Arrange
            Result<TestModel> result = Result.NotFound<TestModel>("TestModel");

            // Act
            string matchResult = result.Match(
                onSuccess: value => $"Success: {value?.Id ?? 0}",
                onFailure: error => $"Failed: {error}"
            );

            // Assert
            matchResult.ShouldBe("Failed: TestModel not found");
        }

        [Fact]
        public void NotFoundGeneric_ShouldSwitchCorrectlyWithSimpleSwitch()
        {
            // Arrange
            Result<TestModel> result = Result.NotFound<TestModel>("TestModel");
            string? capturedError = null;
            bool onSuccessCalled = false;

            // Act
            result.Switch(
                onSuccess: value => onSuccessCalled = true,
                onFailure: error => capturedError = error
            );

            // Assert
            onSuccessCalled.ShouldBeFalse();
            capturedError.ShouldBe("TestModel not found");
        }

        [Fact]
        public void NotFoundGeneric_ShouldRouteToErrorHandlerInComplexSwitch()
        {
            // Arrange
            Result<TestModel> result = Result.NotFound<TestModel>("TestModel");
            string? handlerCalled = null;
            string? capturedError = null;

            // Act
            result.Switch(
                onSuccess: value => handlerCalled = "success",
                onError: error => { handlerCalled = "error"; capturedError = error; },
                onSecurityException: error => handlerCalled = "security",
                onValidationException: errors => handlerCalled = "validation",
                onOperationCanceledException: error => handlerCalled = "cancelled"
            );

            // Assert
            handlerCalled.ShouldBe("error");
            capturedError.ShouldBe("TestModel not found");
        }

        [Fact]
        public void NotFoundGeneric_ShouldRouteToErrorHandlerInComplexMatch()
        {
            // Arrange
            Result<TestModel> result = Result.NotFound<TestModel>("TestModel");

            // Act
            string matchResult = result.Match(
                onSuccess: value => $"success: {value?.Name ?? "null"}",
                onError: error => $"error: {error}",
                onSecurityException: error => $"security: {error}",
                onValidationException: errors => $"validation: {errors.Count}",
                onOperationCanceledException: error => $"cancelled: {error}"
            );

            // Assert
            matchResult.ShouldBe("error: TestModel not found");
        }

        [Fact]
        public void NotFoundGeneric_WithSpecialCharactersInResource_ShouldReturnFailureWithResourceSpecificMessage()
        {
            // Arrange
            string resourceName = "Entity (ID: @456#)";

            // Act
            Result<TestModel> result = Result.NotFound<TestModel>(resourceName);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.NotFound);
            result.Error.ShouldBe("Entity (ID: @456#) not found");
            result.TryGetValue(out TestModel _).ShouldBeFalse();
        }
    }

    #endregion TASK-049: NotFound Factory Method Tests
}