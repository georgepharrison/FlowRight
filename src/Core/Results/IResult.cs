using System.Diagnostics.CodeAnalysis;

namespace FlowRight.Core.Results;

/// <summary>
/// Represents the basic contract for Result pattern implementations, providing access to success/failure state
/// and error information without exposing the success value.
/// </summary>
/// <remarks>
/// This interface defines the core properties that all Result types must implement, including failure tracking,
/// success/failure state, and categorization of different failure types.
/// </remarks>
public interface IResult
{
    #region Public Properties

    /// <summary>
    /// Gets a dictionary of validation failures, where each key represents a field name
    /// and each value represents an array of error messages for that field.
    /// </summary>
    /// <value>
    /// An <see cref="IDictionary{TKey, TValue}"/> containing field-specific error messages.
    /// Returns an empty dictionary for successful results or non-validation failures.
    /// </value>
    IDictionary<string, string[]> Failures { get; }

    /// <summary>
    /// Gets the specific type of failure that occurred, providing detailed categorization
    /// of different error scenarios.
    /// </summary>
    /// <value>
    /// A <see cref="ResultFailureType"/> value indicating the specific type of failure,
    /// or <see cref="ResultFailureType.None"/> for successful results.
    /// </value>
    ResultFailureType FailureType { get; }

    /// <summary>
    /// Gets a value indicating whether the result represents a failure state.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the result represents a failure; otherwise, <see langword="false"/>.
    /// This property is always the logical inverse of <see cref="IsSuccess"/>.
    /// </value>
    bool IsFailure { get; }

    /// <summary>
    /// Gets a value indicating whether the result represents a successful state.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the result represents success; otherwise, <see langword="false"/>.
    /// This property is always the logical inverse of <see cref="IsFailure"/>.
    /// </value>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets the general category of the result, providing high-level classification
    /// of success, informational, warning, or error states.
    /// </summary>
    /// <value>
    /// A <see cref="ResultType"/> value indicating the general result category.
    /// </value>
    ResultType ResultType { get; }

    #endregion Public Properties
}

/// <summary>
/// Represents a Result pattern implementation that can contain a typed success value,
/// providing pattern matching and functional programming capabilities.
/// </summary>
/// <typeparam name="T">The type of the success value that this result can contain.</typeparam>
/// <remarks>
/// <para>
/// This interface extends <see cref="IResult"/> to add typed success value handling
/// and provides pattern matching methods for functional-style result processing.
/// The interface is covariant, allowing assignment from more derived types to less derived types.
/// </para>
/// <para>
/// The interface provides two styles of result handling:
/// <list type="bullet">
/// <item><description><strong>Match methods</strong>: Functional style that transforms the result into another value</description></item>
/// <item><description><strong>Switch methods</strong>: Imperative style that executes side-effect actions</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example of using IResult&lt;T&gt; with pattern matching
/// IResult&lt;User&gt; result = GetUser(userId);
/// 
/// string message = result.Match(
///     onSuccess: user => $"Welcome, {user.Name}!",
///     onFailure: error => $"Error: {error}"
/// );
/// 
/// // Example with detailed failure handling
/// result.Switch(
///     onSuccess: user => Console.WriteLine($"User loaded: {user.Name}"),
///     onError: error => LogError(error),
///     onSecurityException: error => LogSecurityViolation(error),
///     onValidationException: errors => LogValidationErrors(errors)
/// );
/// </code>
/// </example>
public interface IResult<out T> : IResult, IResultError<string>
{
    #region Public Methods

    /// <summary>
    /// Provides pattern matching for the result, executing one of two functions based on success or failure state.
    /// </summary>
    /// <typeparam name="TResult">The type of value returned by both success and failure handlers.</typeparam>
    /// <param name="onSuccess">Function to execute if the result is successful, receiving the success value.</param>
    /// <param name="onFailure">Function to execute if the result is a failure, receiving the error message.</param>
    /// <returns>
    /// The value returned by either <paramref name="onSuccess"/> or <paramref name="onFailure"/>
    /// depending on the result state.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// Result&lt;int&gt; result = GetNumber();
    /// 
    /// string message = result.Match(
    ///     onSuccess: value => $"The number is {value}",
    ///     onFailure: error => $"Failed: {error}"
    /// );
    /// </code>
    /// </example>
    TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure);

    /// <summary>
    /// Provides comprehensive pattern matching for the result with separate handlers for different failure types.
    /// </summary>
    /// <typeparam name="TResult">The type of value returned by all handlers.</typeparam>
    /// <param name="onSuccess">Function to execute for successful results, receiving the success value.</param>
    /// <param name="onError">Function to execute for general error failures, receiving the error message.</param>
    /// <param name="onSecurityException">Function to execute for security-related failures, receiving the error message.</param>
    /// <param name="onValidationException">Function to execute for validation failures, receiving the validation error dictionary.</param>
    /// <param name="onOperationCanceledException">Function to execute for operation canceled failures, receiving the error message.</param>
    /// <returns>
    /// The value returned by the appropriate handler based on the result state and failure type.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the required handler parameters is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; result = AuthenticateUser(credentials);
    /// 
    /// string response = result.Match(
    ///     onSuccess: user => $"Welcome {user.Name}",
    ///     onError: error => $"System error: {error}",
    ///     onSecurityException: error => "Access denied",
    ///     onValidationException: errors => $"Validation failed: {errors.Count} errors",
    ///     onOperationCanceledException: error => "Operation was cancelled"
    /// );
    /// </code>
    /// </example>
    TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onError, Func<string, TResult> onSecurityException, Func<IDictionary<string, string[]>, TResult> onValidationException, Func<string, TResult> onOperationCanceledException);

    /// <summary>
    /// Executes side-effect actions based on the result state, with simple success/failure handling.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful, receiving the success value.</param>
    /// <param name="onFailure">Action to execute if the result is a failure, receiving the error message.</param>
    /// <param name="includeOperationCancelledFailures">
    /// <see langword="true"/> to execute <paramref name="onFailure"/> for operation canceled failures;
    /// <see langword="false"/> to ignore operation canceled failures.
    /// Default is <see langword="false"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// Result&lt;Data&gt; result = ProcessData();
    /// 
    /// result.Switch(
    ///     onSuccess: data => SaveToDatabase(data),
    ///     onFailure: error => LogError(error)
    /// );
    /// </code>
    /// </example>
    void Switch(Action<T> onSuccess, Action<string> onFailure, bool includeOperationCancelledFailures = false);

    /// <summary>
    /// Executes side-effect actions based on the result state with separate handlers for different failure types.
    /// </summary>
    /// <param name="onSuccess">Action to execute for successful results, receiving the success value.</param>
    /// <param name="onError">Action to execute for general error failures, receiving the error message.</param>
    /// <param name="onSecurityException">Action to execute for security-related failures, receiving the error message.</param>
    /// <param name="onValidationException">Action to execute for validation failures, receiving the validation error dictionary.</param>
    /// <param name="onOperationCanceledException">
    /// Optional action to execute for operation canceled failures, receiving the error message.
    /// If <see langword="null"/>, operation canceled failures are ignored.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the required action parameters is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// Result&lt;Order&gt; result = ProcessOrder(orderData);
    /// 
    /// result.Switch(
    ///     onSuccess: order => SendConfirmationEmail(order),
    ///     onError: error => LogSystemError(error),
    ///     onSecurityException: error => AlertSecurityTeam(error),
    ///     onValidationException: errors => ShowValidationErrors(errors),
    ///     onOperationCanceledException: error => NotifyUserOfCancellation(error)
    /// );
    /// </code>
    /// </example>
    void Switch(Action<T> onSuccess, Action<string> onError, Action<string> onSecurityException, Action<IDictionary<string, string[]>> onValidationException, Action<string>? onOperationCanceledException = null);

    #endregion Public Methods
}

/// <summary>
/// Provides access to error messages from result operations, supporting covariant access to error information.
/// </summary>
/// <typeparam name="T">The type of error message or information provided by the result.</typeparam>
/// <remarks>
/// This interface is covariant, allowing assignment from more specific error types to more general ones.
/// In a Result pattern library, "Error" is the most appropriate and intuitive property name for accessing
/// error information, despite analyzer warnings about keyword conflicts in other .NET languages.
/// </remarks>
public interface IResultError<out T>
{
    #region Public Properties

    /// <summary>
    /// Gets the error message or information associated with a failed result.
    /// </summary>
    /// <value>
    /// The error message or information for failed results, or a default/empty value for successful results.
    /// The specific type and content depend on the implementing result type.
    /// </value>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
        Justification = "In a Result pattern library, 'Error' is the most intuitive property name for error information. " +
                       "The benefit of clear, domain-appropriate naming outweighs potential keyword conflicts in other languages.")]
    T Error { get; }

    #endregion Public Properties
}