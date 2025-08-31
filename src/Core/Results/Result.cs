using System.Security;
using System.Text.Json.Serialization;

namespace FlowRight.Core.Results;

public partial class Result : IResult
{
    #region Private Constructors

    [JsonConstructor]
    private Result()
    { }

    private Result(ResultType resultType = ResultType.Success) =>
        ResultType = resultType;

    private Result(string error, ResultType resultType, ResultFailureType resultFailureType)
    {
        Error = error;
        ResultType = resultType;
        FailureType = resultFailureType;
    }

    private Result(string key, string error)
    {
        FailureType = ResultFailureType.Validation;
        ResultType = ResultType.Error;
        Failures.Add(key, [error]);
        Error = GetValidationError(Failures);
    }

    private Result(SecurityException securityException)
    {
        FailureType = ResultFailureType.Security;
        ResultType = ResultType.Error;
        Error = securityException.Message;
    }

    private Result(OperationCanceledException operationCanceledException)
    {
        FailureType = ResultFailureType.OperationCanceled;
        ResultType = ResultType.Warning;
        Error = operationCanceledException.Message;
    }

    private Result(IDictionary<string, string[]> errors)
    {
        FailureType = ResultFailureType.Validation;
        ResultType = ResultType.Error;
        Failures = errors;
        Error = GetValidationError(errors);
    }

    #endregion Private Constructors

    #region Public Methods

    public static Result Combine(params Result[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        Dictionary<string, List<string>> failures = [];

        foreach (Result result in results.Where(r => r.IsFailure))
        {
            switch (result.FailureType)
            {
                case ResultFailureType.Error:
                case ResultFailureType.Security:
                case ResultFailureType.OperationCanceled:
                    if (!failures.TryGetValue(result.FailureType.ToString(), out List<string>? errors))
                    {
                        failures[result.FailureType.ToString()] = errors ??= [];
                    }
                    errors.Add(result.Error);
                    break;

                case ResultFailureType.Validation:
                    foreach (KeyValuePair<string, string[]> failure in result.Failures)
                    {
                        if (!failures.TryGetValue(failure.Key, out List<string>? resultFailures))
                        {
                            failures[failure.Key] = resultFailures ??= [];
                        }
                        resultFailures.AddRange(failure.Value);
                    }
                    break;

                default:
                    break;
            }
        }

        return failures.Count > 0
            ? Failure(failures.ToDictionary(x => x.Key, x => x.Value.ToArray()))
            : Success();
    }

    #endregion Public Methods

    #region Internal Methods

    internal static string GetValidationError(IDictionary<string, string[]> errors)
    {
        List<string> errorMessages = [$"One or more validation errors occurred.{Environment.NewLine}"];

        foreach (KeyValuePair<string, string[]> error in errors)
        {
            errorMessages.Add($"{error.Key}");

            foreach (string errorMessage in error.Value)
            {
                errorMessages.Add($"  - {errorMessage}");
            }
        }

        return string.Join(Environment.NewLine, errorMessages);
    }

    #endregion Internal Methods

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
}