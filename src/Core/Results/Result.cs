using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text.Json.Serialization;

namespace FlowRight.Core.Results;

/// <summary>
/// Provides a non-generic Result implementation that represents the outcome of operations
/// that don't return a value, supporting both success and failure states with detailed error information.
/// </summary>
/// <remarks>
/// <para>
/// This class implements the Result pattern for operations that perform actions but don't return
/// a specific value (similar to void methods). It provides comprehensive error tracking,
/// categorization of different failure types, and full JSON serialization support.
/// </para>
/// <para>
/// The Result class is designed to be immutable after construction and supports
/// both simple success/failure scenarios and complex error handling with validation failures,
/// security exceptions, and operation cancellation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Creating success results
/// Result successResult = Result.Success();
/// Result infoResult = Result.Success(ResultType.Information);
/// 
/// // Creating failure results
/// Result errorResult = Result.Failure("Operation failed");
/// Result validationResult = Result.ValidationFailure(validationErrors);
/// Result securityResult = Result.SecurityFailure("Access denied");
/// 
/// // Pattern matching with results
/// string message = result.Match(
///     onSuccess: () => "Operation completed successfully",
///     onFailure: error => $"Operation failed: {error}"
/// );
/// </code>
/// </example>
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
        Failures = new Dictionary<string, string[]> { [key] = [error] };
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

    /// <summary>
    /// Combines multiple Result instances into a single Result, aggregating all failure information
    /// and returning success only if all input results are successful.
    /// </summary>
    /// <param name="results">The array of Result instances to combine.</param>
    /// <returns>
    /// A <see cref="Result"/> that is successful if all input results are successful, 
    /// or a failure result containing aggregated error information from all failed results.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="results"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method provides a way to aggregate multiple operation results into a single result.
    /// It's particularly useful for batch operations where you want to collect all errors
    /// rather than failing on the first error encountered.
    /// </para>
    /// <para>
    /// The combining logic preserves error categorization:
    /// <list type="bullet">
    /// <item><description>Validation errors are merged by field name</description></item>
    /// <item><description>Other error types are grouped by failure type</description></item>
    /// <item><description>Multiple errors of the same type are collected together</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result[] operations = [
    ///     ValidateUser(user),
    ///     ValidatePermissions(user),
    ///     ValidateData(data)
    /// ];
    /// 
    /// Result combinedResult = Result.Combine(operations);
    /// 
    /// if (combinedResult.IsFailure)
    /// {
    ///     // Handle all collected errors at once
    ///     LogErrors(combinedResult.Failures);
    /// }
    /// </code>
    /// </example>
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
                case ResultFailureType.NotFound:
                case ResultFailureType.ServerError:
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

    /// <summary>
    /// Combines multiple Result{T} instances into a single Result{T}, aggregating all failure information
    /// and returning success only if all input results are successful.
    /// </summary>
    /// <typeparam name="T">The type of the success values in the results.</typeparam>
    /// <param name="results">The array of Result{T} instances to combine.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> that is successful with the value from the first successful result if all input results are successful, 
    /// or a failure result containing aggregated error information from all failed results.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="results"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method provides a way to aggregate multiple operation results into a single result.
    /// It's particularly useful for batch operations where you want to collect all errors
    /// rather than failing on the first error encountered.
    /// </para>
    /// <para>
    /// The combining logic preserves error categorization:
    /// <list type="bullet">
    /// <item><description>Validation errors are merged by field name</description></item>
    /// <item><description>Other error types are grouped by failure type</description></item>
    /// <item><description>Multiple errors of the same type are collected together</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If all results are successful, the returned result will contain the value from the first
    /// successful result. If you need to combine the actual values, use a different approach
    /// such as collecting the values after confirming all results are successful.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result{User}[] operations = [
    ///     ValidateUser(user),
    ///     ValidatePermissions(user),
    ///     ValidateData(data)
    /// ];
    /// 
    /// Result{User} combinedResult = Result.Combine(operations);
    /// 
    /// if (combinedResult.IsFailure)
    /// {
    ///     // Handle all collected errors at once
    ///     LogErrors(combinedResult.Failures);
    /// }
    /// </code>
    /// </example>
    public static Result<T> Combine<T>(params Result<T>[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        // Handle empty array case - return failure as there are no results to combine
        if (results.Length == 0)
        {
            return Failure<T>("No results to combine");
        }

        Dictionary<string, List<string>> failures = [];
        T? firstSuccessValue = default;
        ResultType firstSuccessResultType = ResultType.Success;
        bool hasSuccessValue = false;

        foreach (Result<T> result in results)
        {
            if (result.IsSuccess && !hasSuccessValue)
            {
                result.TryGetValue(out firstSuccessValue);
                firstSuccessResultType = result.ResultType;
                hasSuccessValue = true;
            }
            else if (result.IsFailure)
            {
                switch (result.FailureType)
                {
                    case ResultFailureType.Error:
                    case ResultFailureType.Security:
                    case ResultFailureType.NotFound:
                    case ResultFailureType.ServerError:
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
        }

        return failures.Count > 0
            ? Failure<T>(failures.ToDictionary(x => x.Key, x => x.Value.ToArray()))
            : Success(firstSuccessValue!, firstSuccessResultType);
    }

    /// <summary>
    /// Transforms this result into a value of type <typeparamref name="TResult"/> using pattern matching.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="onSuccess">Function to execute if the result is successful. Returns the success value.</param>
    /// <param name="onFailure">Function to execute if the result is a failure. Receives the error message.</param>
    /// <returns>The value returned by either <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This is the primary pattern matching method for non-generic <see cref="Result"/>. It provides a functional
    /// approach to handling both success and failure cases by requiring explicit handling of both scenarios.
    /// </para>
    /// <para>
    /// This method treats all failure types (Error, Security, Validation, OperationCanceled) uniformly,
    /// calling <paramref name="onFailure"/> with the error message. For more granular failure handling,
    /// use the overload that provides separate handlers for each failure type.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result operationResult = PerformOperation();
    /// 
    /// string message = operationResult.Match(
    ///     onSuccess: () => "Operation completed successfully!",
    ///     onFailure: error => $"Operation failed: {error}"
    /// );
    /// 
    /// Console.WriteLine(message);
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure)
    {
#if DEBUG
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
#endif

        return IsSuccess
            ? onSuccess()
            : onFailure(Error);
    }

    /// <summary>
    /// Transforms this result into a value of type <typeparamref name="TResult"/> using pattern matching with specific handlers for each failure type.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="onSuccess">Function to execute if the result is successful. Returns the success value.</param>
    /// <param name="onError">Function to execute if the result is a general error. Receives the error message.</param>
    /// <param name="onSecurityException">Function to execute if the result is a security failure. Receives the error message.</param>
    /// <param name="onValidationException">Function to execute if the result is a validation failure. Receives the validation errors dictionary.</param>
    /// <param name="onOperationCanceledException">Function to execute if the result is a cancellation failure. Receives the error message.</param>
    /// <returns>The value returned by the appropriate handler function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any of the handler functions is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This overload of Match provides granular control over different failure types, allowing you to
    /// implement specific logic for each category of failure. This is particularly useful when different
    /// failure types require different handling strategies.
    /// </para>
    /// <para>
    /// The method routes failures to the appropriate handler based on <see cref="FailureType"/>:
    /// <list type="bullet">
    /// <item><description><see cref="ResultFailureType.Error"/> → <paramref name="onError"/></description></item>
    /// <item><description><see cref="ResultFailureType.Security"/> → <paramref name="onSecurityException"/></description></item>
    /// <item><description><see cref="ResultFailureType.Validation"/> → <paramref name="onValidationException"/></description></item>
    /// <item><description><see cref="ResultFailureType.OperationCanceled"/> → <paramref name="onOperationCanceledException"/></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result operationResult = ProcessRequest();
    /// 
    /// string response = operationResult.Match(
    ///     onSuccess: () => "Success!",
    ///     onError: error => $"Error: {error}",
    ///     onSecurityException: error => "Access denied",
    ///     onValidationException: errors => $"Validation failed: {errors.Count} errors",
    ///     onOperationCanceledException: error => "Operation was cancelled"
    /// );
    /// </code>
    /// </example>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onError, Func<string, TResult> onSecurityException, Func<IDictionary<string, string[]>, TResult> onValidationException, Func<string, TResult> onOperationCanceledException)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        ArgumentNullException.ThrowIfNull(onSecurityException);
        ArgumentNullException.ThrowIfNull(onValidationException);
        ArgumentNullException.ThrowIfNull(onOperationCanceledException);

        return IsSuccess
            ? onSuccess()
            : FailureType switch
            {
                ResultFailureType.Error => onError(Error),
                ResultFailureType.Security => onSecurityException(Error),
                ResultFailureType.Validation => onValidationException(Failures),
                ResultFailureType.NotFound => onError(Error),
                ResultFailureType.ServerError => onError(Error),
                ResultFailureType.OperationCanceled => onOperationCanceledException(Error),
                _ => throw new NotImplementedException()
            };
    }

    /// <summary>
    /// Executes side-effect actions based on the result state, with simple success/failure handling.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful.</param>
    /// <param name="onFailure">Action to execute if the result is a failure. Receives the error message.</param>
    /// <param name="includeOperationCancelledFailures">If <see langword="true"/>, operation cancelled failures will call <paramref name="onFailure"/>. If <see langword="false"/> (default), they will be ignored.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This is the primary imperative pattern matching method for non-generic <see cref="Result"/>. Unlike the functional
    /// <see cref="Match{TResult}(Func{TResult}, Func{string, TResult})"/> method, this executes actions with side effects
    /// rather than returning transformed values.
    /// </para>
    /// <para>
    /// This method treats most failure types (Error, Security, Validation) uniformly, calling <paramref name="onFailure"/>
    /// with the error message. Operation cancelled failures are treated specially based on the
    /// <paramref name="includeOperationCancelledFailures"/> parameter. For more granular failure handling,
    /// use the overload that provides separate handlers for each failure type.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result operationResult = PerformOperation();
    /// 
    /// operationResult.Switch(
    ///     onSuccess: () => {
    ///         Console.WriteLine("Operation completed successfully!");
    ///         LogSuccess("Operation", DateTime.Now);
    ///     },
    ///     onFailure: error => {
    ///         Console.WriteLine($"Operation failed: {error}");
    ///         LogError("Operation", error);
    ///     }
    /// );
    /// </code>
    /// </example>
    public void Switch(Action onSuccess, Action<string> onFailure, bool includeOperationCancelledFailures = false)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess)
        {
            onSuccess();
            return;
        }

        if (FailureType == ResultFailureType.OperationCanceled && !includeOperationCancelledFailures)
        {
            return;
        }

        onFailure(Error);
    }

    /// <summary>
    /// Executes side-effect actions based on the result state with separate handlers for different failure types.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful.</param>
    /// <param name="onError">Action to execute if the result is a general error. Receives the error message.</param>
    /// <param name="onSecurityException">Action to execute if the result is a security failure. Receives the error message.</param>
    /// <param name="onValidationException">Action to execute if the result is a validation failure. Receives the validation errors dictionary.</param>
    /// <param name="onOperationCanceledException">Optional action to execute if the result is a cancellation failure. Receives the error message. If <see langword="null"/>, cancellation failures are ignored.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onSuccess"/>, <paramref name="onError"/>, <paramref name="onSecurityException"/>, or <paramref name="onValidationException"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This overload of Switch provides granular control over different failure types using imperative actions,
    /// allowing you to implement specific side effects for each category of failure. This is particularly useful
    /// when different failure types require different handling strategies (logging, notifications, etc.).
    /// </para>
    /// <para>
    /// The method routes failures to the appropriate handler based on <see cref="FailureType"/>:
    /// <list type="bullet">
    /// <item><description><see cref="ResultFailureType.Error"/> → <paramref name="onError"/></description></item>
    /// <item><description><see cref="ResultFailureType.Security"/> → <paramref name="onSecurityException"/></description></item>
    /// <item><description><see cref="ResultFailureType.Validation"/> → <paramref name="onValidationException"/></description></item>
    /// <item><description><see cref="ResultFailureType.OperationCanceled"/> → <paramref name="onOperationCanceledException"/> (if not null)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result operationResult = ProcessRequest();
    /// 
    /// operationResult.Switch(
    ///     onSuccess: () => {
    ///         logger.LogInformation("Request processed successfully");
    ///         SendSuccessNotification();
    ///     },
    ///     onError: error => {
    ///         logger.LogError("System error: {Error}", error);
    ///         SendErrorNotification(error);
    ///     },
    ///     onSecurityException: error => {
    ///         logger.LogWarning("Security violation: {Error}", error);
    ///         AlertSecurityTeam(error);
    ///     },
    ///     onValidationException: errors => {
    ///         logger.LogInformation("Validation failed: {ErrorCount} errors", errors.Count);
    ///         ShowValidationErrors(errors);
    ///     },
    ///     onOperationCanceledException: error => {
    ///         logger.LogInformation("Request was cancelled: {Error}", error);
    ///         CleanupResources();
    ///     }
    /// );
    /// </code>
    /// </example>
    public void Switch(Action onSuccess, Action<string> onError, Action<string> onSecurityException, Action<IDictionary<string, string[]>> onValidationException, Action<string>? onOperationCanceledException = null)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        ArgumentNullException.ThrowIfNull(onSecurityException);
        ArgumentNullException.ThrowIfNull(onValidationException);

        if (IsSuccess)
        {
            onSuccess();
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

            case ResultFailureType.NotFound:
                onError(Error);
                break;

            case ResultFailureType.ServerError:
                onError(Error);
                break;

            case ResultFailureType.OperationCanceled:
                onOperationCanceledException?.Invoke(Error);
                break;

            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Explicitly converts a <see cref="Result"/> to a boolean indicating success or failure.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns><see langword="true"/> if the result represents success; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// This explicit conversion provides a convenient way to use results in boolean contexts
    /// while making the conversion explicit to avoid accidental usage.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result result = PerformOperation();
    /// 
    /// if ((bool)result) // Explicit conversion
    /// {
    ///     Console.WriteLine("Operation succeeded!");
    /// }
    /// </code>
    /// </example>
    public static explicit operator bool(Result result) =>
        result?.IsSuccess ?? false;

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

    /// <summary>
    /// Gets the error message associated with this result.
    /// </summary>
    /// <value>
    /// A string containing the error message for failed results, or an empty string for successful results.
    /// For validation failures, this contains a formatted summary of all validation errors.
    /// </value>
    /// <remarks>
    /// This property implements <see cref="IResultError{T}.Error"/> and provides the primary
    /// error message for the result. The content varies by failure type:
    /// <list type="bullet">
    /// <item><description><strong>Success:</strong> Empty string</description></item>
    /// <item><description><strong>General errors:</strong> The specific error message</description></item>
    /// <item><description><strong>Validation errors:</strong> Formatted summary of all field errors</description></item>
    /// <item><description><strong>Security errors:</strong> Security exception message</description></item>
    /// <item><description><strong>Cancellation:</strong> Operation canceled message</description></item>
    /// </list>
    /// </remarks>
    [JsonInclude]
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
        Justification = "In a Result pattern library, 'Error' is the most intuitive property name for error information.")]
    public string Error { get; private set; } = string.Empty;

    /// <summary>
    /// Shared empty dictionary to avoid allocations in success cases.
    /// </summary>
    private static readonly IDictionary<string, string[]> EmptyFailures = new Dictionary<string, string[]>();
    
    /// <summary>
    /// Gets a dictionary of field-specific validation failures.
    /// </summary>
    /// <value>
    /// An <see cref="IDictionary{TKey, TValue}"/> where keys represent field names and values
    /// represent arrays of error messages for each field. Empty for non-validation failures.
    /// </value>
    /// <remarks>
    /// This property provides detailed field-level error information for validation failures.
    /// It allows consumers to display specific error messages for individual form fields
    /// or properties. For non-validation failures, this dictionary is empty.
    /// </remarks>
    [JsonInclude]
    public IDictionary<string, string[]> Failures { get; private set; } = EmptyFailures;

    /// <summary>
    /// Gets the specific type of failure that occurred.
    /// </summary>
    /// <value>
    /// A <see cref="ResultFailureType"/> value indicating the specific category of failure,
    /// or <see cref="ResultFailureType.None"/> for successful results.
    /// </value>
    /// <remarks>
    /// This property enables consumers to implement different handling strategies based on
    /// the type of failure. For example, validation failures might be displayed to users
    /// while security failures might be logged and result in access denial.
    /// </remarks>
    [JsonInclude]
    public ResultFailureType FailureType { get; private set; } = ResultFailureType.None;

    /// <summary>
    /// Gets a value indicating whether this result represents a failure state.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the result represents a failure; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This property is determined by checking if an error message is present.
    /// It is always the logical inverse of <see cref="IsSuccess"/>.
    /// </remarks>
    public bool IsFailure =>
        !string.IsNullOrEmpty(Error);

    /// <summary>
    /// Gets a value indicating whether this result represents a successful state.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the result represents success; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This property is determined by checking if no error message is present.
    /// It is always the logical inverse of <see cref="IsFailure"/>.
    /// </remarks>
    public bool IsSuccess =>
        string.IsNullOrEmpty(Error);

    /// <summary>
    /// Gets the general category of this result.
    /// </summary>
    /// <value>
    /// A <see cref="ResultType"/> value indicating the overall result category.
    /// </value>
    /// <remarks>
    /// This property provides high-level categorization that complements the more specific
    /// <see cref="FailureType"/>. It allows for broad categorization of results into
    /// success, informational, warning, or error states.
    /// </remarks>
    [JsonInclude]
    public ResultType ResultType { get; private set; } = ResultType.Information;

    #endregion Public Properties
}