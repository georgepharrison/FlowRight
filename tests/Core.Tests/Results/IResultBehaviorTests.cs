using FlowRight.Core.Results;
using Shouldly;

namespace FlowRight.Core.Tests.Results;

/// <summary>
/// Tests to verify expected behavioral contracts for IResult and IResult{T} interfaces
/// through mock implementations that validate the interface contracts work as expected.
/// </summary>
public class IResultBehaviorTests
{
    #region Test Implementations

    /// <summary>
    /// Simple test implementation of IResult for failure scenarios
    /// </summary>
    private sealed class TestFailureResult : IResult
    {
        #region Public Constructors

        public TestFailureResult(ResultFailureType failureType, IDictionary<string, string[]>? failures = null)
        {
            FailureType = failureType;
            Failures = failures ?? new Dictionary<string, string[]> { ["Error"] = ["Test error"] };
        }

        #endregion Public Constructors

        #region Public Properties

        public IDictionary<string, string[]> Failures { get; }
        public ResultFailureType FailureType { get; }
        public bool IsFailure => true;
        public bool IsSuccess => false;
        public ResultType ResultType => ResultType.Error;

        #endregion Public Properties
    }

    /// <summary>
    /// Test implementation of IResult{T} for failure scenarios
    /// </summary>
    private sealed class TestFailureResult<T> : IResult<T>
    {
        #region Public Constructors

        public TestFailureResult(string error, ResultFailureType failureType = ResultFailureType.Error,
            IDictionary<string, string[]>? failures = null)
        {
            Error = error;
            FailureType = failureType;
            Failures = failures ?? new Dictionary<string, string[]> { ["Error"] = [error] };
        }

        #endregion Public Constructors

        #region Public Methods

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        {
            return onFailure(Error);
        }

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onError,
            Func<string, TResult> onSecurityException, Func<IDictionary<string, string[]>, TResult> onValidationException,
            Func<string, TResult> onOperationCanceledException)
        {
            return FailureType switch
            {
                ResultFailureType.Security => onSecurityException(Error),
                ResultFailureType.Validation => onValidationException(Failures),
                ResultFailureType.OperationCanceled => onOperationCanceledException(Error),
                _ => onError(Error)
            };
        }

        public void Switch(Action<T> onSuccess, Action<string> onFailure, bool includeOperationCancelledFailures = false)
        {
            if (FailureType == ResultFailureType.OperationCanceled && !includeOperationCancelledFailures)
                return;

            onFailure(Error);
        }

        public void Switch(Action<T> onSuccess, Action<string> onError, Action<string> onSecurityException,
            Action<IDictionary<string, string[]>> onValidationException, Action<string>? onOperationCanceledException = null)
        {
            switch (FailureType)
            {
                case ResultFailureType.Security:
                    onSecurityException(Error);
                    break;

                case ResultFailureType.Validation:
                    onValidationException(Failures);
                    break;

                case ResultFailureType.OperationCanceled:
                    onOperationCanceledException?.Invoke(Error);
                    break;

                default:
                    onError(Error);
                    break;
            }
        }

        #endregion Public Methods

        #region Public Properties

        public string Error { get; }
        public IDictionary<string, string[]> Failures { get; }
        public ResultFailureType FailureType { get; }
        public bool IsFailure => true;
        public bool IsSuccess => false;
        public ResultType ResultType => ResultType.Error;

        #endregion Public Properties
    }

    /// <summary>
    /// Simple test implementation of IResult for success scenarios
    /// </summary>
    private sealed class TestSuccessResult : IResult
    {
        #region Public Properties

        public IDictionary<string, string[]> Failures => new Dictionary<string, string[]>();
        public ResultFailureType FailureType => ResultFailureType.None;
        public bool IsFailure => false;
        public bool IsSuccess => true;
        public ResultType ResultType => ResultType.Success;

        #endregion Public Properties
    }

    /// <summary>
    /// Test implementation of IResult{T} for success scenarios
    /// </summary>
    private sealed class TestSuccessResult<T> : IResult<T>
    {
        #region Private Members

        private readonly T _value;

        #endregion Private Members

        #region Public Constructors

        public TestSuccessResult(T value)
        {
            _value = value;
        }

        #endregion Public Constructors

        #region Public Methods

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        {
            return onSuccess(_value);
        }

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onError,
            Func<string, TResult> onSecurityException, Func<IDictionary<string, string[]>, TResult> onValidationException,
            Func<string, TResult> onOperationCanceledException)
        {
            return onSuccess(_value);
        }

        public void Switch(Action<T> onSuccess, Action<string> onFailure, bool includeOperationCancelledFailures = false)
        {
            onSuccess(_value);
        }

        public void Switch(Action<T> onSuccess, Action<string> onError, Action<string> onSecurityException,
            Action<IDictionary<string, string[]>> onValidationException, Action<string>? onOperationCanceledException = null)
        {
            onSuccess(_value);
        }

        #endregion Public Methods

        #region Public Properties

        public string Error => string.Empty;
        public IDictionary<string, string[]> Failures => new Dictionary<string, string[]>();
        public ResultFailureType FailureType => ResultFailureType.None;
        public bool IsFailure => false;
        public bool IsSuccess => true;
        public ResultType ResultType => ResultType.Success;

        #endregion Public Properties
    }

    #endregion Test Implementations

    #region IResult Behavior Tests

    [Fact]
    public void IResult_FailureImplementation_ShouldHaveCorrectProperties()
    {
        // Arrange
        Dictionary<string, string[]> failures = new() { ["Field"] = ["Error message"] };

        // Act
        IResult result = new TestFailureResult(ResultFailureType.Validation, failures);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.ResultType.ShouldBe(ResultType.Error);
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldBe(failures);
    }

    [Fact]
    public void IResult_SuccessImplementation_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        IResult result = new TestSuccessResult();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.ResultType.ShouldBe(ResultType.Success);
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.Failures.ShouldBeEmpty();
    }

    #endregion IResult Behavior Tests

    #region IResult<T> Behavior Tests

    [Theory]
    [InlineData(ResultFailureType.Security)]
    [InlineData(ResultFailureType.Validation)]
    [InlineData(ResultFailureType.OperationCanceled)]
    [InlineData(ResultFailureType.Error)]
    public void IResultT_ComplexMatch_ShouldRouteToCorrectHandler(ResultFailureType failureType)
    {
        // Arrange
        Dictionary<string, string[]> validationErrors = new() { ["Field"] = ["Validation error"] };
        IResult<int> result = new TestFailureResult<int>("Test error", failureType, validationErrors);

        bool errorCalled = false;
        bool securityCalled = false;
        bool validationCalled = false;
        bool operationCanceledCalled = false;

        // Act
        string output = result.Match(
            onSuccess: value => "Success",
            onError: error => { errorCalled = true; return $"Error: {error}"; },
            onSecurityException: error => { securityCalled = true; return $"Security: {error}"; },
            onValidationException: errors => { validationCalled = true; return $"Validation: {errors.Count}"; },
            onOperationCanceledException: error => { operationCanceledCalled = true; return $"Canceled: {error}"; });

        // Assert
        switch (failureType)
        {
            case ResultFailureType.Security:
                securityCalled.ShouldBeTrue();
                errorCalled.ShouldBeFalse();
                validationCalled.ShouldBeFalse();
                operationCanceledCalled.ShouldBeFalse();
                output.ShouldBe("Security: Test error");
                break;

            case ResultFailureType.Validation:
                validationCalled.ShouldBeTrue();
                errorCalled.ShouldBeFalse();
                securityCalled.ShouldBeFalse();
                operationCanceledCalled.ShouldBeFalse();
                output.ShouldBe("Validation: 1");
                break;

            case ResultFailureType.OperationCanceled:
                operationCanceledCalled.ShouldBeTrue();
                errorCalled.ShouldBeFalse();
                securityCalled.ShouldBeFalse();
                validationCalled.ShouldBeFalse();
                output.ShouldBe("Canceled: Test error");
                break;

            default:
                errorCalled.ShouldBeTrue();
                securityCalled.ShouldBeFalse();
                validationCalled.ShouldBeFalse();
                operationCanceledCalled.ShouldBeFalse();
                output.ShouldBe("Error: Test error");
                break;
        }
    }

    [Fact]
    public void IResultT_FailureImplementation_Match_ShouldExecuteFailureFunc()
    {
        // Arrange
        IResult<int> result = new TestFailureResult<int>("Something went wrong");
        bool successCalled = false;
        bool failureCalled = false;

        // Act
        string output = result.Match(
            onSuccess: value => { successCalled = true; return $"Success: {value}"; },
            onFailure: error => { failureCalled = true; return $"Failure: {error}"; });

        // Assert
        successCalled.ShouldBeFalse();
        failureCalled.ShouldBeTrue();
        output.ShouldBe("Failure: Something went wrong");
    }

    [Fact]
    public void IResultT_FailureImplementation_Switch_ShouldExecuteFailureAction()
    {
        // Arrange
        IResult<string> result = new TestFailureResult<string>("error occurred");
        bool successCalled = false;
        bool failureCalled = false;
        string? capturedError = null;

        // Act
        result.Switch(
            onSuccess: value => { successCalled = true; },
            onFailure: error => { failureCalled = true; capturedError = error; });

        // Assert
        successCalled.ShouldBeFalse();
        failureCalled.ShouldBeTrue();
        capturedError.ShouldBe("error occurred");
    }

    [Fact]
    public void IResultT_SuccessImplementation_Match_ShouldExecuteSuccessFunc()
    {
        // Arrange
        IResult<int> result = new TestSuccessResult<int>(42);
        bool successCalled = false;
        bool failureCalled = false;

        // Act
        string output = result.Match(
            onSuccess: value => { successCalled = true; return $"Success: {value}"; },
            onFailure: error => { failureCalled = true; return $"Failure: {error}"; });

        // Assert
        successCalled.ShouldBeTrue();
        failureCalled.ShouldBeFalse();
        output.ShouldBe("Success: 42");
    }

    [Fact]
    public void IResultT_SuccessImplementation_Switch_ShouldExecuteSuccessAction()
    {
        // Arrange
        IResult<string> result = new TestSuccessResult<string>("test value");
        bool successCalled = false;
        bool failureCalled = false;
        string? capturedValue = null;

        // Act
        result.Switch(
            onSuccess: value => { successCalled = true; capturedValue = value; },
            onFailure: error => { failureCalled = true; });

        // Assert
        successCalled.ShouldBeTrue();
        failureCalled.ShouldBeFalse();
        capturedValue.ShouldBe("test value");
    }

    #endregion IResult<T> Behavior Tests

    #region Covariance Tests

    [Fact]
    public void IResultError_ShouldSupportCovariance()
    {
        // Arrange
        IResultError<string> stringError = new TestFailureResult<int>("error");

        // Act & Assert - Should be able to assign to IResultError<object> due to covariance
        IResultError<object> objectError = stringError;
        objectError.ShouldNotBeNull();
        objectError.Error.ShouldBe("error");
    }

    [Fact]
    public void IResultT_ShouldSupportCovariance()
    {
        // Arrange
        IResult<string> stringResult = new TestSuccessResult<string>("test");

        // Act & Assert - Should be able to assign to IResult<object> due to covariance
        IResult<object> objectResult = stringResult;
        objectResult.ShouldNotBeNull();
    }

    #endregion Covariance Tests
}