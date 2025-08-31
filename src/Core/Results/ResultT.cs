using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Text.Json.Serialization;

namespace FlowRight.Core.Results;

public partial class Result<T> : IResult<T>
{
    #region Internal Constructors

    internal Result(T value, ResultType resultType)
    {
        ResultType = resultType;
        SuccessValue = value;
        FailureType = ResultFailureType.None;
    }

    internal Result(string error, ResultType resultType)
    {
        Error = error;
        ResultType = resultType;
    }

    internal Result(string key, string error)
    {
        FailureType = ResultFailureType.Validation;
        ResultType = ResultType.Error;
        Failures.Add(key, [error]);
        Error = Result.GetValidationError(Failures);
    }

    internal Result(SecurityException securityException)
    {
        FailureType = ResultFailureType.Security;
        ResultType = ResultType.Error;
        Error = securityException.Message;
    }

    internal Result(OperationCanceledException operationCanceledException)
    {
        FailureType = ResultFailureType.OperationCanceled;
        ResultType = ResultType.Warning;
        Error = operationCanceledException.Message;
    }

    internal Result(IDictionary<string, string[]> errors)
    {
        FailureType = ResultFailureType.Validation;
        ResultType = ResultType.Error;
        Failures = errors;
        Error = Result.GetValidationError(errors);
    }

    #endregion Internal Constructors

    #region Private Constructors

    [JsonConstructor]
    private Result()
    { }

    #endregion Private Constructors

    #region Public Methods

    public static implicit operator Result(Result<T> result) =>
        CreateResult(result ?? Result.Failure<T>("Result is null"));

    public static implicit operator Result<T>(T value) =>
        value is IResult<T> result
            ? result.Match(
                onSuccess: success => Result.Success(success, result.ResultType),
                onError: error => Result.Failure<T>(error, result.ResultType),
                onSecurityException: error => Result.Failure<T>(new SecurityException(error)),
                onValidationException: Result.Failure<T>,
                onOperationCanceledException: error => Result.Failure<T>(new OperationCanceledException(error)))
            : Result.Success(value);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess
            ? onSuccess(SuccessValue)
            : onFailure(Error);
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onError, Func<string, TResult> onSecurityException, Func<IDictionary<string, string[]>, TResult> onValidationException, Func<string, TResult> onOperationCanceledException)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        ArgumentNullException.ThrowIfNull(onSecurityException);
        ArgumentNullException.ThrowIfNull(onValidationException);
        ArgumentNullException.ThrowIfNull(onOperationCanceledException);

        return IsSuccess
            ? onSuccess(SuccessValue)
            : FailureType switch
            {
                ResultFailureType.Error => onError(Error),
                ResultFailureType.Security => onSecurityException(Error),
                ResultFailureType.Validation => onValidationException(Failures),
                ResultFailureType.OperationCanceled => onOperationCanceledException(Error),
                _ => throw new NotImplementedException()
            };
    }

    public void Switch(Action<T> onSuccess, Action<string> onFailure, bool includeOperationCancelledFailures = false)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess)
        {
            onSuccess(SuccessValue);
            return;
        }

        switch (FailureType)
        {
            case ResultFailureType.Error:
            case ResultFailureType.Security:
            case ResultFailureType.Validation:
                onFailure(Error);
                break;

            case ResultFailureType.OperationCanceled:
                if (includeOperationCancelledFailures)
                {
                    onFailure(Error);
                }
                break;
        }
    }

    public void Switch(Action<T> onSuccess, Action<string> onError, Action<string> onSecurityException, Action<IDictionary<string, string[]>> onValidationException, Action<string>? onOperationCanceledException = null)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        ArgumentNullException.ThrowIfNull(onSecurityException);
        ArgumentNullException.ThrowIfNull(onValidationException);

        if (IsSuccess)
        {
            onSuccess(SuccessValue);
            return;
        }

        switch (FailureType)
        {
            case ResultFailureType.Error:
                onError(Error);
                break;

            case ResultFailureType.Security:
                onSecurityException(Error);
                break;

            case ResultFailureType.Validation:
                onValidationException(Failures);
                break;

            case ResultFailureType.OperationCanceled:
                if (onOperationCanceledException is not null)
                {
                    onOperationCanceledException?.Invoke(Error);
                }
                break;
        }
    }

    public bool TryGetValue([NotNullWhen(returnValue: true)] out T? value)
    {
        value = IsSuccess ? SuccessValue : default;

        return IsSuccess;
    }

    #endregion Public Methods

    #region Private Methods

    private static Result CreateResult(Result<T> result) =>
        result.FailureType switch
        {
            ResultFailureType.None => Result.Success(result.ResultType),
            ResultFailureType.Error => Result.Failure(result.Error, result.ResultType),
            ResultFailureType.Security => Result.Failure(new SecurityException(result.Error)),
            ResultFailureType.Validation => Result.Failure(result.Failures),
            ResultFailureType.OperationCanceled => Result.Failure(new OperationCanceledException(result.Error)),
            _ => throw new NotImplementedException()
        };

    #endregion Private Methods

    #region Public Properties

    [JsonInclude]
    public string Error { get; private set; } = string.Empty;

    [JsonInclude]
    public IDictionary<string, string[]> Failures { get; private set; } = new Dictionary<string, string[]>();

    [JsonInclude]
    public ResultFailureType FailureType { get; private set; } = ResultFailureType.None;

    public bool IsFailure =>
        !string.IsNullOrEmpty(Error);

    public bool IsSuccess =>
        !IsFailure;

    [JsonInclude]
    public ResultType ResultType { get; private set; } = ResultType.Information;

    #endregion Public Properties

    #region Private Properties

    [JsonInclude]
    private T SuccessValue { get; set; } = default!;

    #endregion Private Properties
}