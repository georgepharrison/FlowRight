using System.Security;

namespace FlowRight.Core.Results;

public partial class Result
{
    #region Public Methods

    public static Result Failure(string error, ResultType resultType = ResultType.Error, ResultFailureType resultFailureType = ResultFailureType.Error) =>
        new(error ?? throw new ArgumentNullException(nameof(error)), resultType, resultFailureType);

    public static Result Failure(string key, string error) =>
        new(key ?? throw new ArgumentNullException(nameof(key)), error ?? throw new ArgumentNullException(nameof(error)));

    public static Result Failure(SecurityException securityException) =>
        new(securityException ?? throw new ArgumentNullException(nameof(securityException)));

    public static Result Failure(OperationCanceledException operationCanceledException) =>
        new(operationCanceledException ?? throw new ArgumentNullException(nameof(operationCanceledException)));

    public static Result Failure(IDictionary<string, string[]> errors) =>
        new(errors ?? throw new ArgumentNullException(nameof(errors)));

    public static Result<T> Failure<T>(string error, ResultType resultType = ResultType.Error) =>
        new(error ?? throw new ArgumentNullException(nameof(error)), resultType);

    public static Result<T> Failure<T>(string key, string error) =>
        new(key ?? throw new ArgumentNullException(nameof(key)), error ?? throw new ArgumentNullException(nameof(error)));

    public static Result<T> Failure<T>(SecurityException securityException) =>
        new(securityException ?? throw new ArgumentNullException(nameof(securityException)));

    public static Result<T> Failure<T>(OperationCanceledException operationCanceledException) =>
        new(operationCanceledException ?? throw new ArgumentNullException(nameof(operationCanceledException)));

    public static Result<T> Failure<T>(IDictionary<string, string[]> errors) =>
        new(errors ?? throw new ArgumentNullException(nameof(errors)));

    #endregion Public Methods
}