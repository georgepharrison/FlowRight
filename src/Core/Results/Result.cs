using System.Diagnostics.CodeAnalysis;
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
    public IDictionary<string, string[]> Failures { get; private set; } = new Dictionary<string, string[]>();

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
        !IsFailure;

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