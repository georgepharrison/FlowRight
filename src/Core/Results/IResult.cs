namespace FlowRight.Core.Results;

public interface IResult
{
    #region Public Properties

    IDictionary<string, string[]> Failures { get; }

    ResultFailureType FailureType { get; }

    bool IsFailure { get; }

    bool IsSuccess { get; }

    ResultType ResultType { get; }

    #endregion Public Properties
}

public interface IResult<out T> : IResult, IResultError<string>
{
    #region Public Methods

    TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure);

    TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onError, Func<string, TResult> onSecurityException, Func<IDictionary<string, string[]>, TResult> onValidationException, Func<string, TResult> onOperationCanceledException);

    void Switch(Action<T> onSuccess, Action<string> onFailure, bool includeOperationCancelledFailures = false);

    void Switch(Action<T> onSuccess, Action<string> onError, Action<string> onSecurityException, Action<IDictionary<string, string[]>> onValidationException, Action<string>? onOperationCanceledException = null);

    #endregion Public Methods
}

public interface IResultError<out T>
{
    #region Public Properties

    T Error { get; }

    #endregion Public Properties
}