using System.Security;
using FlowRight.Core.Results;
using Shouldly;

namespace FlowRight.Core.Tests.Results;

/// <summary>
/// Comprehensive tests for Result&lt;T&gt; generic type support ensuring proper implementation
/// of the Result pattern with typed success values and all failure scenarios.
/// </summary>
public class ResultTTests
{
    #region Success Creation Tests

    [Fact]
    public void Success_WithValue_ShouldCreateSuccessfulResult()
    {
        // Arrange
        const string expectedValue = "test value";

        // Act
        Result<string> result = Result.Success(expectedValue);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.ResultType.ShouldBe(ResultType.Success);
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.Error.ShouldBeEmpty();
        result.Failures.ShouldBeEmpty();
        result.TryGetValue(out string? value).ShouldBeTrue();
        value.ShouldBe(expectedValue);
    }

    [Fact]
    public void Success_WithValueAndResultType_ShouldCreateSuccessfulResultWithSpecifiedType()
    {
        // Arrange
        const int expectedValue = 42;
        const ResultType resultType = ResultType.Information;

        // Act
        Result<int> result = Result.Success(expectedValue, resultType);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.ResultType.ShouldBe(resultType);
        result.TryGetValue(out int value).ShouldBeTrue();
        value.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Success_WithNullOrEmptyString_ShouldCreateSuccessfulResult(string? testValue)
    {
        // Arrange & Act
        Result<string?> result = Result.Success(testValue);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out string? value).ShouldBeTrue();
        value.ShouldBe(testValue);
    }

    [Fact]
    public void Success_WithNullValue_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Success<string?>(null));
    }

    [Fact]
    public void Success_WithComplexType_ShouldCreateSuccessfulResult()
    {
        // Arrange
        TestModel expectedValue = new() { Id = 1, Name = "Test" };

        // Act
        Result<TestModel> result = Result.Success(expectedValue);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out TestModel? value).ShouldBeTrue();
        value.ShouldBe(expectedValue);
        value.Id.ShouldBe(1);
        value.Name.ShouldBe("Test");
    }

    #endregion Success Creation Tests

    #region Failure Creation Tests

    [Fact]
    public void Failure_WithErrorMessage_ShouldCreateFailureResult()
    {
        // Arrange
        const string errorMessage = "Something went wrong";

        // Act
        Result<string> result = Result.Failure<string>(errorMessage);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.ResultType.ShouldBe(ResultType.Error);
        result.FailureType.ShouldBe(ResultFailureType.Error);
        result.Error.ShouldBe(errorMessage);
        result.Failures.ShouldBeEmpty();
        result.TryGetValue(out string? value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void Failure_WithErrorMessageAndResultType_ShouldCreateFailureResultWithSpecifiedType()
    {
        // Arrange
        const string errorMessage = "Warning occurred";
        const ResultType resultType = ResultType.Warning;

        // Act
        Result<int> result = Result.Failure<int>(errorMessage, resultType);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.ResultType.ShouldBe(resultType);
        result.FailureType.ShouldBe(ResultFailureType.Error);
        result.Error.ShouldBe(errorMessage);
    }

    [Fact]
    public void Failure_WithErrorMessageResultTypeAndFailureType_ShouldCreateFailureResultWithSpecifiedTypes()
    {
        // Arrange
        const string errorMessage = "Custom failure occurred";
        const ResultType resultType = ResultType.Warning;
        const ResultFailureType failureType = ResultFailureType.Error;

        // Act
        Result<string> result = Result.Failure<string>(errorMessage, resultType, failureType);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.ResultType.ShouldBe(resultType);
        result.FailureType.ShouldBe(failureType);
        result.Error.ShouldBe(errorMessage);
    }

    [Fact]
    public void Failure_WithValidationErrors_ShouldCreateValidationFailureResult()
    {
        // Arrange
        const string fieldName = "Email";
        const string errorMessage = "Email is required";

        // Act
        Result<string> result = Result.Failure<string>(fieldName, errorMessage);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.ResultType.ShouldBe(ResultType.Error);
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldContainKey(fieldName);
        result.Failures[fieldName].ShouldContain(errorMessage);
        result.Error.ShouldNotBeEmpty();
        result.Error.ShouldContain(fieldName);
        result.Error.ShouldContain(errorMessage);
    }

    [Fact]
    public void Failure_WithValidationErrorsDictionary_ShouldCreateValidationFailureResult()
    {
        // Arrange
        Dictionary<string, string[]> validationErrors = new()
        {
            ["Email"] = ["Email is required", "Email format is invalid"],
            ["Password"] = ["Password must be at least 8 characters"]
        };

        // Act
        Result<TestModel> result = Result.Failure<TestModel>(validationErrors);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldBe(validationErrors);
        result.Error.ShouldContain("Email");
        result.Error.ShouldContain("Password");
    }

    [Fact]
    public void Failure_WithSecurityException_ShouldCreateSecurityFailureResult()
    {
        // Arrange
        SecurityException securityException = new("Access denied");

        // Act
        Result<string> result = Result.Failure<string>(securityException);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.ResultType.ShouldBe(ResultType.Error);
        result.FailureType.ShouldBe(ResultFailureType.Security);
        result.Error.ShouldBe("Access denied");
    }

    [Fact]
    public void Failure_WithOperationCanceledException_ShouldCreateCancellationFailureResult()
    {
        // Arrange
        OperationCanceledException canceledException = new("Operation was canceled");

        // Act
        Result<int> result = Result.Failure<int>(canceledException);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.ResultType.ShouldBe(ResultType.Warning);
        result.FailureType.ShouldBe(ResultFailureType.OperationCanceled);
        result.Error.ShouldBe("Operation was canceled");
    }

    #endregion Failure Creation Tests

    #region Pattern Matching - Match Tests

    [Fact]
    public void Match_WithSuccessfulResult_ShouldExecuteSuccessFunction()
    {
        // Arrange
        const string value = "test";
        Result<string> result = Result.Success(value);

        // Act
        string output = result.Match(
            onSuccess: v => $"Success: {v}",
            onFailure: error => $"Failure: {error}");

        // Assert
        output.ShouldBe("Success: test");
    }

    [Fact]
    public void Match_WithFailureResult_ShouldExecuteFailureFunction()
    {
        // Arrange
        const string errorMessage = "Something went wrong";
        Result<string> result = Result.Failure<string>(errorMessage);

        // Act
        string output = result.Match(
            onSuccess: v => $"Success: {v}",
            onFailure: error => $"Failure: {error}");

        // Assert
        output.ShouldBe("Failure: Something went wrong");
    }

    [Fact]
    public void Match_WithNullFunctions_ShouldThrowArgumentNullException()
    {
        // Arrange
        Result<string> result = Result.Success("test");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Match<string>(null!, _ => "failure"));
        Should.Throw<ArgumentNullException>(() => result.Match<string>(_ => "success", null!));
    }

    [Theory]
    [InlineData(ResultFailureType.Error)]
    [InlineData(ResultFailureType.Security)]
    [InlineData(ResultFailureType.OperationCanceled)]
    public void Match_ComplexWithSpecificFailureTypes_ShouldRouteToCorrectHandler(ResultFailureType failureType)
    {
        // Arrange
        Result<string> result = CreateFailureByType<string>(failureType);

        // Act
        string output = result.Match(
            onSuccess: v => "success",
            onError: error => "general error",
            onSecurityException: error => "security error",
            onValidationException: errors => "validation error",
            onOperationCanceledException: error => "cancellation error");

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
    public void Match_ComplexWithValidationFailure_ShouldRouteToValidationHandler()
    {
        // Arrange
        Dictionary<string, string[]> validationErrors = new() { ["Field"] = ["Error"] };
        Result<string> result = Result.Failure<string>(validationErrors);

        // Act
        string output = result.Match(
            onSuccess: v => "success",
            onError: error => "general error",
            onSecurityException: error => "security error",
            onValidationException: errors => $"validation error: {errors.Count} fields",
            onOperationCanceledException: error => "cancellation error");

        // Assert
        output.ShouldBe("validation error: 1 fields");
    }

    [Fact]
    public void Match_ComplexWithSuccessfulResult_ShouldRouteToSuccessHandler()
    {
        // Arrange
        Result<int> result = Result.Success(42);

        // Act
        string output = result.Match(
            onSuccess: v => $"success: {v}",
            onError: error => "general error",
            onSecurityException: error => "security error",
            onValidationException: errors => "validation error",
            onOperationCanceledException: error => "cancellation error");

        // Assert
        output.ShouldBe("success: 42");
    }

    #endregion Pattern Matching - Match Tests

    #region Pattern Matching - Switch Tests

    [Fact]
    public void Switch_WithSuccessfulResult_ShouldExecuteSuccessAction()
    {
        // Arrange
        const string value = "test";
        Result<string> result = Result.Success(value);
        string? executedValue = null;
        bool failureActionCalled = false;

        // Act
        result.Switch(
            onSuccess: v => executedValue = v,
            onFailure: _ => failureActionCalled = true);

        // Assert
        executedValue.ShouldBe(value);
        failureActionCalled.ShouldBeFalse();
    }

    [Fact]
    public void Switch_WithFailureResult_ShouldExecuteFailureAction()
    {
        // Arrange
        const string errorMessage = "Something went wrong";
        Result<string> result = Result.Failure<string>(errorMessage);
        bool successActionCalled = false;
        string? executedError = null;

        // Act
        result.Switch(
            onSuccess: _ => successActionCalled = true,
            onFailure: error => executedError = error);

        // Assert
        successActionCalled.ShouldBeFalse();
        executedError.ShouldBe(errorMessage);
    }

    [Fact]
    public void Switch_WithOperationCanceledAndIncludeFlag_ShouldExecuteFailureAction()
    {
        // Arrange
        Result<string> result = Result.Failure<string>(new OperationCanceledException("Canceled"));
        bool successActionCalled = false;
        string? executedError = null;

        // Act
        result.Switch(
            onSuccess: _ => successActionCalled = true,
            onFailure: error => executedError = error,
            includeOperationCancelledFailures: true);

        // Assert
        successActionCalled.ShouldBeFalse();
        executedError.ShouldBe("Canceled");
    }

    [Fact]
    public void Switch_WithOperationCanceledAndNoIncludeFlag_ShouldIgnoreFailure()
    {
        // Arrange
        Result<string> result = Result.Failure<string>(new OperationCanceledException("Canceled"));
        bool successActionCalled = false;
        bool failureActionCalled = false;

        // Act
        result.Switch(
            onSuccess: _ => successActionCalled = true,
            onFailure: _ => failureActionCalled = true,
            includeOperationCancelledFailures: false);

        // Assert
        successActionCalled.ShouldBeFalse();
        failureActionCalled.ShouldBeFalse();
    }

    [Theory]
    [InlineData(ResultFailureType.Error)]
    [InlineData(ResultFailureType.Security)]
    [InlineData(ResultFailureType.Validation)]
    public void Switch_ComplexWithSpecificFailureTypes_ShouldRouteToCorrectHandler(ResultFailureType failureType)
    {
        // Arrange
        Result<string> result = CreateFailureByType<string>(failureType);
        bool successCalled = false;
        bool errorCalled = false;
        bool securityCalled = false;
        bool validationCalled = false;
        bool cancellationCalled = false;

        // Act
        result.Switch(
            onSuccess: _ => successCalled = true,
            onError: _ => errorCalled = true,
            onSecurityException: _ => securityCalled = true,
            onValidationException: _ => validationCalled = true,
            onOperationCanceledException: _ => cancellationCalled = true);

        // Assert
        successCalled.ShouldBeFalse();
        cancellationCalled.ShouldBeFalse();

        switch (failureType)
        {
            case ResultFailureType.Error:
                errorCalled.ShouldBeTrue();
                securityCalled.ShouldBeFalse();
                validationCalled.ShouldBeFalse();
                break;
            case ResultFailureType.Security:
                errorCalled.ShouldBeFalse();
                securityCalled.ShouldBeTrue();
                validationCalled.ShouldBeFalse();
                break;
            case ResultFailureType.Validation:
                errorCalled.ShouldBeFalse();
                securityCalled.ShouldBeFalse();
                validationCalled.ShouldBeTrue();
                break;
        }
    }

    #endregion Pattern Matching - Switch Tests

    #region TryGetValue Tests

    [Fact]
    public void TryGetValue_WithSuccessfulResult_ShouldReturnTrueAndValue()
    {
        // Arrange
        const string expectedValue = "test value";
        Result<string> result = Result.Success(expectedValue);

        // Act
        bool success = result.TryGetValue(out string? value);

        // Assert
        success.ShouldBeTrue();
        value.ShouldBe(expectedValue);
    }

    [Fact]
    public void TryGetValue_WithFailureResult_ShouldReturnFalseAndDefaultValue()
    {
        // Arrange
        Result<string> result = Result.Failure<string>("error");

        // Act
        bool success = result.TryGetValue(out string? value);

        // Assert
        success.ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void TryGetValue_WithSuccessfulValueType_ShouldReturnTrueAndValue()
    {
        // Arrange
        const int expectedValue = 42;
        Result<int> result = Result.Success(expectedValue);

        // Act
        bool success = result.TryGetValue(out int value);

        // Assert
        success.ShouldBeTrue();
        value.ShouldBe(expectedValue);
    }

    [Fact]
    public void TryGetValue_WithFailureValueType_ShouldReturnFalseAndDefaultValue()
    {
        // Arrange
        Result<int> result = Result.Failure<int>("error");

        // Act
        bool success = result.TryGetValue(out int value);

        // Assert
        success.ShouldBeFalse();
        value.ShouldBe(default(int));
    }

    #endregion TryGetValue Tests

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccessfulResult()
    {
        // Arrange & Act
        Result<string> result = "test value";

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out string? value).ShouldBeTrue();
        value.ShouldBe("test value");
    }

    [Fact]
    public void ImplicitConversion_FromResultT_ToResult_ShouldPreserveState()
    {
        // Arrange
        Result<string> successResult = Result.Success("test");
        Result<string> failureResult = Result.Failure<string>("error");

        // Act
        Result nonGenericSuccess = successResult;
        Result nonGenericFailure = failureResult;

        // Assert
        nonGenericSuccess.IsSuccess.ShouldBeTrue();
        nonGenericSuccess.ResultType.ShouldBe(successResult.ResultType);

        nonGenericFailure.IsFailure.ShouldBeTrue();
        nonGenericFailure.Error.ShouldBe(failureResult.Error);
        nonGenericFailure.FailureType.ShouldBe(failureResult.FailureType);
    }

    [Fact]
    public void ImplicitConversion_FromResultT_ToResult_WithValidationErrors_ShouldPreserveErrors()
    {
        // Arrange
        Dictionary<string, string[]> validationErrors = new() { ["Field"] = ["Error"] };
        Result<string> validationResult = Result.Failure<string>(validationErrors);

        // Act
        Result nonGenericResult = validationResult;

        // Assert
        nonGenericResult.IsFailure.ShouldBeTrue();
        nonGenericResult.FailureType.ShouldBe(ResultFailureType.Validation);
        nonGenericResult.Failures.ShouldBe(validationErrors);
    }

    #endregion Implicit Conversion Tests

    #region Generic Type Support Tests

    [Fact]
    public void Result_WithDifferentGenericTypes_ShouldMaintainTypeInformation()
    {
        // Arrange & Act
        Result<string> stringResult = Result.Success("text");
        Result<int> intResult = Result.Success(42);
        Result<bool> boolResult = Result.Success(true);
        Result<TestModel> modelResult = Result.Success(new TestModel { Id = 1, Name = "Test" });

        // Assert
        stringResult.TryGetValue(out string? stringValue).ShouldBeTrue();
        stringValue.ShouldBe("text");

        intResult.TryGetValue(out int intValue).ShouldBeTrue();
        intValue.ShouldBe(42);

        boolResult.TryGetValue(out bool boolValue).ShouldBeTrue();
        boolValue.ShouldBeTrue();

        modelResult.TryGetValue(out TestModel? modelValue).ShouldBeTrue();
        modelValue.ShouldNotBeNull();
        modelValue.Id.ShouldBe(1);
        modelValue.Name.ShouldBe("Test");
    }

    [Fact]
    public void Result_WithNullableTypes_ShouldThrowForNullValues()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Success<string?>(null));
        Should.Throw<ArgumentNullException>(() => Result.Success<int?>(null));
    }

    [Fact]
    public void Result_WithNullableTypes_ShouldHandleNonNullValues()
    {
        // Arrange & Act
        Result<string?> stringResult = Result.Success<string?>("test");
        Result<int?> intResult = Result.Success<int?>(42);

        // Assert
        stringResult.IsSuccess.ShouldBeTrue();
        stringResult.TryGetValue(out string? stringValue).ShouldBeTrue();
        stringValue.ShouldBe("test");

        intResult.IsSuccess.ShouldBeTrue();
        intResult.TryGetValue(out int? intValue).ShouldBeTrue();
        intValue.ShouldBe(42);
    }

    [Fact]
    public void Result_WithGenericFailure_ShouldMaintainFailureInformation()
    {
        // Arrange & Act
        Result<TestModel> result = Result.Failure<TestModel>("Model creation failed");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Model creation failed");
        result.TryGetValue(out TestModel? value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    #endregion Generic Type Support Tests

    #region Serialization Support Tests

    [Fact]
    public void Result_SuccessfulResult_ShouldHaveJsonIncludeAttributes()
    {
        // Arrange
        Type resultType = typeof(Result<>);

        // Act & Assert
        System.Reflection.PropertyInfo errorProperty = resultType.GetProperty("Error")!;
        System.Reflection.PropertyInfo failuresProperty = resultType.GetProperty("Failures")!;
        System.Reflection.PropertyInfo failureTypeProperty = resultType.GetProperty("FailureType")!;
        System.Reflection.PropertyInfo resultTypeProperty = resultType.GetProperty("ResultType")!;

        errorProperty.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonIncludeAttribute), false).ShouldNotBeEmpty();
        failuresProperty.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonIncludeAttribute), false).ShouldNotBeEmpty();
        failureTypeProperty.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonIncludeAttribute), false).ShouldNotBeEmpty();
        resultTypeProperty.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonIncludeAttribute), false).ShouldNotBeEmpty();
    }

    #endregion Serialization Support Tests

    #region Generic Combine Method Tests

    [Fact]
    public void Combine_WithNullResults_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => Result.Combine<string>(null!));
    }

    [Fact]
    public void Combine_WithEmptyResults_ShouldReturnFailure()
    {
        // Arrange & Act
        Result<string> result = Result.Combine<string>();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("No results to combine");
        result.FailureType.ShouldBe(ResultFailureType.Error);
        result.ResultType.ShouldBe(ResultType.Error);
        result.TryGetValue(out string? value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void Combine_WithAllSuccessResults_ShouldReturnSuccessWithFirstValue()
    {
        // Arrange
        const string firstValue = "first";
        const string secondValue = "second";
        const string thirdValue = "third";

        Result<string>[] successResults = [
            Result.Success(firstValue),
            Result.Success(secondValue, ResultType.Information),
            Result.Success(thirdValue, ResultType.Warning)
        ];

        // Act
        Result<string> result = Result.Combine(successResults);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeEmpty();
        result.Failures.ShouldBeEmpty();
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.ResultType.ShouldBe(ResultType.Success);
        result.TryGetValue(out string? value).ShouldBeTrue();
        value.ShouldBe(firstValue); // Should return first successful value
    }

    [Fact]
    public void Combine_WithSingleErrorResult_ShouldReturnFailureWithError()
    {
        // Arrange
        const string errorMessage = "Operation failed";
        const string successValue = "success";

        Result<string>[] results = [
            Result.Success(successValue),
            Result.Failure<string>(errorMessage),
            Result.Success("another success")
        ];

        // Act
        Result<string> result = Result.Combine(results);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.ResultType.ShouldBe(ResultType.Error);
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldContainKey(ResultFailureType.Error.ToString());
        result.Failures[ResultFailureType.Error.ToString()].ShouldContain(errorMessage);
        result.TryGetValue(out string? value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void Combine_WithMultipleErrorResults_ShouldAggregateAllErrors()
    {
        // Arrange
        const string error1 = "First error";
        const string error2 = "Second error";
        const string error3 = "Third error";

        Result<int>[] results = [
            Result.Failure<int>(error1),
            Result.Success(42),
            Result.Failure<int>(error2),
            Result.Failure<int>(error3)
        ];

        // Act
        Result<int> result = Result.Combine(results);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldContainKey(ResultFailureType.Error.ToString());
        string[] errorMessages = result.Failures[ResultFailureType.Error.ToString()];
        errorMessages.ShouldContain(error1);
        errorMessages.ShouldContain(error2);
        errorMessages.ShouldContain(error3);
        errorMessages.Length.ShouldBe(3);
        result.TryGetValue(out int value).ShouldBeFalse();
        value.ShouldBe(0);
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

        Result<TestModel>[] results = [
            Result.Failure<TestModel>(validationErrors1),
            Result.Success(new TestModel { Id = 1, Name = "Test" }),
            Result.Failure<TestModel>(validationErrors2)
        ];

        // Act
        Result<TestModel> result = Result.Combine(results);

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

        // Check other fields
        result.Failures.ShouldContainKey("Password");
        result.Failures["Password"].ShouldContain("Password is too short");
        result.Failures["Password"].Length.ShouldBe(1);

        result.Failures.ShouldContainKey("Username");
        result.Failures["Username"].ShouldContain("Username is required");
        result.Failures["Username"].Length.ShouldBe(1);

        result.TryGetValue(out TestModel? value).ShouldBeFalse();
        value.ShouldBeNull();
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

        Result<string>[] results = [
            Result.Failure<string>(generalError),
            Result.Failure<string>(new SecurityException(securityError)),
            Result.Failure<string>(validationErrors),
            Result.Failure<string>(new OperationCanceledException(cancelError)),
            Result.Success("success value")
        ];

        // Act
        Result<string> result = Result.Combine(results);

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
        result.TryGetValue(out string? value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void Combine_WithResultTypePreservation_ShouldPreserveFirstSuccessResultType()
    {
        // Arrange
        const string value1 = "info value";
        const string value2 = "warning value";

        Result<string>[] results = [
            Result.Success(value1, ResultType.Information),
            Result.Success(value2, ResultType.Warning)
        ];

        // Act
        Result<string> result = Result.Combine(results);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.ResultType.ShouldBe(ResultType.Information); // Should preserve first success result type
        result.TryGetValue(out string? value).ShouldBeTrue();
        value.ShouldBe(value1); // Should have first success value
    }

    #endregion Generic Combine Method Tests

    #region Implicit Operator Tests

    [Fact]
    public void ImplicitOperator_FromValueToResultT_WithValidValue_ShouldCreateSuccessResult()
    {
        // Arrange
        const string testValue = "Hello World";

        // Act
        Result<string> result = testValue; // Implicit conversion

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.ResultType.ShouldBe(ResultType.Success);
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.TryGetValue(out string? value).ShouldBeTrue();
        value.ShouldBe(testValue);
    }

    [Fact]
    public void ImplicitOperator_FromValueToResultT_WithNullValue_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? nullValue = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => { Result<string?> result = nullValue; });
    }

    [Fact]
    public void ImplicitOperator_FromValueToResultT_WithComplexType_ShouldCreateSuccessResult()
    {
        // Arrange
        TestModel testModel = new() { Id = 42, Name = "Test Model" };

        // Act
        Result<TestModel> result = testModel; // Implicit conversion

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.ResultType.ShouldBe(ResultType.Success);
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.TryGetValue(out TestModel? value).ShouldBeTrue();
        value.ShouldNotBeNull();
        value.Id.ShouldBe(42);
        value.Name.ShouldBe("Test Model");
    }

    [Fact]
    public void ImplicitOperator_FromResultTToResult_WithSuccessResult_ShouldPreserveSuccessState()
    {
        // Arrange
        Result<string> originalResult = Result.Success("Test Value");

        // Act
        Result convertedResult = originalResult; // Implicit conversion

        // Assert
        convertedResult.IsSuccess.ShouldBeTrue();
        convertedResult.IsFailure.ShouldBeFalse();
        convertedResult.ResultType.ShouldBe(ResultType.Success);
        convertedResult.FailureType.ShouldBe(ResultFailureType.None);
        convertedResult.Error.ShouldBeEmpty();
        convertedResult.Failures.ShouldBeEmpty();
    }

    [Fact]
    public void ImplicitOperator_FromResultTToResult_WithErrorResult_ShouldPreserveErrorState()
    {
        // Arrange
        const string errorMessage = "Something went wrong";
        Result<string> originalResult = Result.Failure<string>(errorMessage);

        // Act
        Result convertedResult = originalResult; // Implicit conversion

        // Assert
        convertedResult.IsFailure.ShouldBeTrue();
        convertedResult.IsSuccess.ShouldBeFalse();
        convertedResult.ResultType.ShouldBe(ResultType.Error);
        convertedResult.FailureType.ShouldBe(ResultFailureType.Error);
        convertedResult.Error.ShouldBe(errorMessage);
        convertedResult.Failures.ShouldBeEmpty(); // Basic errors don't populate Failures dictionary
    }

    [Fact]
    public void ImplicitOperator_FromResultTToResult_WithSecurityResult_ShouldPreserveSecurityState()
    {
        // Arrange
        const string securityMessage = "Access denied";
        Result<string> originalResult = Result.Failure<string>(new SecurityException(securityMessage));

        // Act
        Result convertedResult = originalResult; // Implicit conversion

        // Assert
        convertedResult.IsFailure.ShouldBeTrue();
        convertedResult.IsSuccess.ShouldBeFalse();
        convertedResult.ResultType.ShouldBe(ResultType.Error);
        convertedResult.FailureType.ShouldBe(ResultFailureType.Security);
        convertedResult.Error.ShouldBe(securityMessage);
        convertedResult.Failures.ShouldBeEmpty(); // Basic security errors don't populate Failures dictionary
    }

    [Fact]
    public void ImplicitOperator_FromResultTToResult_WithValidationResult_ShouldPreserveValidationState()
    {
        // Arrange
        Dictionary<string, string[]> validationErrors = new()
        {
            { "Email", ["Email is required", "Invalid email format"] },
            { "Password", ["Password is too short"] }
        };
        Result<string> originalResult = Result.Failure<string>(validationErrors);

        // Act
        Result convertedResult = originalResult; // Implicit conversion

        // Assert
        convertedResult.IsFailure.ShouldBeTrue();
        convertedResult.IsSuccess.ShouldBeFalse();
        convertedResult.ResultType.ShouldBe(ResultType.Error);
        convertedResult.FailureType.ShouldBe(ResultFailureType.Validation);
        convertedResult.Failures.ShouldContainKey("Email");
        convertedResult.Failures.ShouldContainKey("Password");
        convertedResult.Failures["Email"].ShouldContain("Email is required");
        convertedResult.Failures["Email"].ShouldContain("Invalid email format");
        convertedResult.Failures["Password"].ShouldContain("Password is too short");
    }

    [Fact]
    public void ImplicitOperator_FromResultTToResult_WithOperationCanceledResult_ShouldPreserveCancelationState()
    {
        // Arrange
        const string cancelMessage = "Operation was cancelled";
        Result<string> originalResult = Result.Failure<string>(new OperationCanceledException(cancelMessage));

        // Act
        Result convertedResult = originalResult; // Implicit conversion

        // Assert
        convertedResult.IsFailure.ShouldBeTrue();
        convertedResult.IsSuccess.ShouldBeFalse();
        convertedResult.ResultType.ShouldBe(ResultType.Warning);
        convertedResult.FailureType.ShouldBe(ResultFailureType.OperationCanceled);
        convertedResult.Error.ShouldBe(cancelMessage);
        convertedResult.Failures.ShouldBeEmpty(); // Basic operation canceled errors don't populate Failures dictionary
    }

    [Fact]
    public void ImplicitOperator_FromResultTToResult_WithInformationResult_ShouldPreserveResultType()
    {
        // Arrange
        Result<string> originalResult = Result.Success("Test", ResultType.Information);

        // Act
        Result convertedResult = originalResult; // Implicit conversion

        // Assert
        convertedResult.IsSuccess.ShouldBeTrue();
        convertedResult.ResultType.ShouldBe(ResultType.Information);
        convertedResult.FailureType.ShouldBe(ResultFailureType.None);
    }

    [Fact]
    public void ImplicitOperator_FromResultTToResult_WithWarningResult_ShouldPreserveResultType()
    {
        // Arrange
        Result<string> originalResult = Result.Success("Test", ResultType.Warning);

        // Act
        Result convertedResult = originalResult; // Implicit conversion

        // Assert
        convertedResult.IsSuccess.ShouldBeTrue();
        convertedResult.ResultType.ShouldBe(ResultType.Warning);
        convertedResult.FailureType.ShouldBe(ResultFailureType.None);
    }

    [Fact]
    public void ImplicitOperator_FromResultTToResult_WithNullResult_ShouldCreateFailureResult()
    {
        // Arrange
        Result<string>? nullResult = null;

        // Act
        Result convertedResult = nullResult!; // Implicit conversion

        // Assert
        convertedResult.IsFailure.ShouldBeTrue();
        convertedResult.IsSuccess.ShouldBeFalse();
        convertedResult.ResultType.ShouldBe(ResultType.Error);
        convertedResult.FailureType.ShouldBe(ResultFailureType.Error);
        convertedResult.Error.ShouldBe("Result is null");
    }

    #endregion Implicit Operator Tests

    #region Explicit Operator Tests

    [Fact]
    public void ExplicitOperator_FromResultTToValue_WithSuccessResult_ShouldReturnValue()
    {
        // Arrange
        const string expectedValue = "Hello World";
        Result<string> result = Result.Success(expectedValue);

        // Act
        string actualValue = (string)result; // Explicit conversion

        // Assert
        actualValue.ShouldBe(expectedValue);
    }

    [Fact]
    public void ExplicitOperator_FromResultTToValue_WithFailureResult_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Result<string> result = Result.Failure<string>("Something went wrong");

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => { string value = (string)result; })
            .Message.ShouldContain("Cannot extract value from a failed result");
    }

    [Fact]
    public void ExplicitOperator_FromResultTToValue_WithNullResult_ShouldThrowArgumentNullException()
    {
        // Arrange
        Result<string>? nullResult = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => { string value = (string)nullResult!; });
    }

    [Fact]
    public void ExplicitOperator_FromResultTToBool_WithSuccessResult_ShouldReturnTrue()
    {
        // Arrange
        Result<string> result = Result.Success("Test");

        // Act
        bool isSuccess = (bool)result; // Explicit conversion

        // Assert
        isSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ExplicitOperator_FromResultTToBool_WithFailureResult_ShouldReturnFalse()
    {
        // Arrange
        Result<string> result = Result.Failure<string>("Error");

        // Act
        bool isSuccess = (bool)result; // Explicit conversion

        // Assert
        isSuccess.ShouldBeFalse();
    }

    [Fact]
    public void ExplicitOperator_FromResultTToBool_WithNullResult_ShouldReturnFalse()
    {
        // Arrange
        Result<string>? nullResult = null;

        // Act
        bool isSuccess = (bool)nullResult!; // Explicit conversion

        // Assert
        isSuccess.ShouldBeFalse();
    }

    #endregion Explicit Operator Tests

    #region Helper Methods

    private static Result<T> CreateFailureByType<T>(ResultFailureType failureType)
    {
        return failureType switch
        {
            ResultFailureType.Error => Result.Failure<T>("General error"),
            ResultFailureType.Security => Result.Failure<T>(new SecurityException("Security error")),
            ResultFailureType.Validation => Result.Failure<T>(new Dictionary<string, string[]> { ["Field"] = ["Validation error"] }),
            ResultFailureType.OperationCanceled => Result.Failure<T>(new OperationCanceledException("Operation canceled")),
            _ => throw new ArgumentException($"Unsupported failure type: {failureType}")
        };
    }

    #endregion Helper Methods

    #region Test Models

    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion Test Models
}