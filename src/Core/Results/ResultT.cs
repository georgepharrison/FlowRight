using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Text.Json.Serialization;

namespace FlowRight.Core.Results;

/// <summary>
/// Provides a generic Result implementation that represents the outcome of operations
/// that return a typed value, supporting both success and failure states with comprehensive error handling.
/// </summary>
/// <typeparam name="T">The type of the success value that this result can contain.</typeparam>
/// <remarks>
/// <para>
/// This class implements the Result pattern for operations that return a specific value type.
/// It provides the same comprehensive error tracking and categorization as the non-generic
/// <see cref="Result"/> class, while also safely encapsulating the success value.
/// </para>
/// <para>
/// The Result&lt;T&gt; class supports pattern matching through both functional (<see cref="Match{TResult}(Func{T, TResult}, Func{string, TResult})"/>)
/// and imperative (<see cref="Switch(Action{T}, Action{string}, bool)"/>) APIs, enabling both
/// functional programming and traditional procedural approaches.
/// </para>
/// <para>
/// The class is designed to be immutable after construction and provides full JSON serialization
/// support for both success values and error information.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Creating success results
/// Result&lt;User&gt; userResult = Result.Success(user);
/// Result&lt;int&gt; numberResult = Result.Success(42, ResultType.Information);
/// 
/// // Creating failure results  
/// Result&lt;User&gt; failedResult = Result.Failure&lt;User&gt;("User not found");
/// Result&lt;Order&gt; validationResult = Result.ValidationFailure&lt;Order&gt;(validationErrors);
/// 
/// // Pattern matching
/// string message = userResult.Match(
///     onSuccess: user => $"Welcome, {user.Name}!",
///     onFailure: error => $"Login failed: {error}"
/// );
/// 
/// // Imperative handling
/// userResult.Switch(
///     onSuccess: user => RedirectToHome(user),
///     onFailure: error => ShowErrorMessage(error)
/// );
/// </code>
/// </example>
public partial class Result<T> : IResult<T>
{
    #region Internal Constructors

    internal Result(T value, ResultType resultType)
    {
        ResultType = resultType;
        SuccessValue = value;
        FailureType = ResultFailureType.None;
    }

    internal Result(string error, ResultType resultType, ResultFailureType resultFailureType = ResultFailureType.Error)
    {
        Error = error;
        ResultType = resultType;
        FailureType = resultFailureType;
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

    /// <summary>
    /// Implicitly converts a <see cref="Result{T}"/> to a non-generic <see cref="Result"/>.
    /// </summary>
    /// <param name="result">The generic result to convert.</param>
    /// <returns>A non-generic <see cref="Result"/> that preserves the success/failure state and error information.</returns>
    /// <remarks>
    /// <para>
    /// This implicit conversion allows a <see cref="Result{T}"/> to be used anywhere a <see cref="Result"/>
    /// is expected, providing seamless interoperability between generic and non-generic result types.
    /// </para>
    /// <para>
    /// The conversion preserves all error information including failure type, error messages, and validation details.
    /// For successful results, only the success state is preserved (the value is discarded).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; userResult = GetUser();
    /// Result result = userResult; // Implicit conversion
    /// 
    /// if (result.IsSuccess)
    /// {
    ///     // Success state is preserved, but value is not available
    /// }
    /// </code>
    /// </example>
    public static implicit operator Result(Result<T> result) =>
        CreateResult(result ?? Result.Failure<T>("Result is null"));

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to a successful <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="value">The value to wrap in a successful result.</param>
    /// <returns>A successful <see cref="Result{T}"/> containing the specified value.</returns>
    /// <remarks>
    /// <para>
    /// This implicit conversion provides a convenient way to create successful results without
    /// explicitly calling <see cref="Result.Success{T}(T, ResultType)"/>. It simplifies code
    /// when working with methods that return <see cref="Result{T}"/>.
    /// </para>
    /// <para>
    /// If the value is already a <see cref="IResult{T}"/>, this conversion will extract the
    /// result information and create an appropriate <see cref="Result{T}"/> preserving the
    /// original result's success/failure state.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public Result&lt;string&gt; GetMessage()
    /// {
    ///     return "Hello, World!"; // Implicit conversion from string
    /// }
    /// 
    /// public Result&lt;int&gt; Calculate()
    /// {
    ///     return 42; // Implicit conversion from int
    /// }
    /// </code>
    /// </example>
    public static implicit operator Result<T>(T value) =>
        value is IResult<T> result
            ? result.Match(
                onSuccess: success => Result.Success(success, result.ResultType),
                onError: error => Result.Failure<T>(error, result.ResultType),
                onSecurityException: error => Result.Failure<T>(new SecurityException(error)),
                onValidationException: Result.Failure<T>,
                onOperationCanceledException: error => Result.Failure<T>(new OperationCanceledException(error)))
            : Result.Success(value);

    /// <summary>
    /// Transforms this result into a value of type <typeparamref name="TResult"/> using pattern matching.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="onSuccess">Function to execute if the result is successful. Receives the success value.</param>
    /// <param name="onFailure">Function to execute if the result is a failure. Receives the error message.</param>
    /// <returns>The value returned by either <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This is the primary pattern matching method for <see cref="Result{T}"/>. It provides a functional
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
    /// Result&lt;User&gt; userResult = GetUser(userId);
    /// 
    /// string message = userResult.Match(
    ///     onSuccess: user => $"Welcome, {user.Name}!",
    ///     onFailure: error => $"Failed to load user: {error}"
    /// );
    /// 
    /// Console.WriteLine(message);
    /// </code>
    /// </example>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess
            ? onSuccess(SuccessValue)
            : onFailure(Error);
    }

    /// <summary>
    /// Transforms this result into a value of type <typeparamref name="TResult"/> using pattern matching with specific handlers for each failure type.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="onSuccess">Function to execute if the result is successful. Receives the success value.</param>
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
    /// Result&lt;User&gt; userResult = CreateUser(request);
    /// 
    /// IActionResult response = userResult.Match(
    ///     onSuccess: user => Ok(user),
    ///     onError: error => StatusCode(500, error),
    ///     onSecurityException: error => Unauthorized(error),
    ///     onValidationException: errors => BadRequest(errors),
    ///     onOperationCanceledException: error => StatusCode(408, "Request timeout")
    /// );
    /// </code>
    /// </example>
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

    /// <summary>
    /// Executes an action based on the result state using imperative pattern matching.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful. Receives the success value.</param>
    /// <param name="onFailure">Action to execute if the result is a failure. Receives the error message.</param>
    /// <param name="includeOperationCancelledFailures">Whether to execute <paramref name="onFailure"/> for <see cref="ResultFailureType.OperationCanceled"/> failures. Defaults to <see langword="false"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This is the primary imperative pattern matching method for <see cref="Result{T}"/>. Unlike the functional
    /// <see cref="Match{TResult}(Func{T, TResult}, Func{string, TResult})"/> method, this executes actions with side effects
    /// rather than returning transformed values.
    /// </para>
    /// <para>
    /// By default, <see cref="ResultFailureType.OperationCanceled"/> failures are ignored because cancellation
    /// often represents normal flow control rather than an error condition. Set <paramref name="includeOperationCancelledFailures"/>
    /// to <see langword="true"/> if you need to handle cancellation scenarios.
    /// </para>
    /// <para>
    /// All other failure types (Error, Security, Validation) are handled uniformly by <paramref name="onFailure"/>.
    /// For more granular failure handling, use the overload that provides separate handlers for each failure type.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; userResult = GetUser(userId);
    /// 
    /// userResult.Switch(
    ///     onSuccess: user => {
    ///         Console.WriteLine($"Welcome, {user.Name}!");
    ///         LogUserLogin(user.Id);
    ///     },
    ///     onFailure: error => {
    ///         Console.WriteLine($"Login failed: {error}");
    ///         LogFailedLogin(userId);
    ///     }
    /// );
    /// </code>
    /// </example>
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

    /// <summary>
    /// Executes an action based on the result state using imperative pattern matching with specific handlers for each failure type.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful. Receives the success value.</param>
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
    /// <item><description><see cref="ResultFailureType.OperationCanceled"/> → <paramref name="onOperationCanceledException"/> (if provided)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The <paramref name="onOperationCanceledException"/> parameter is optional because cancellation often
    /// represents normal flow control rather than an error condition that requires specific handling.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;Order&gt; orderResult = ProcessOrder(request);
    /// 
    /// orderResult.Switch(
    ///     onSuccess: order => {
    ///         logger.LogInformation("Order {OrderId} processed successfully", order.Id);
    ///         emailService.SendOrderConfirmation(order);
    ///     },
    ///     onError: error => {
    ///         logger.LogError("Order processing failed: {Error}", error);
    ///         alertService.NotifySystemError(error);
    ///     },
    ///     onSecurityException: error => {
    ///         logger.LogWarning("Security violation during order processing: {Error}", error);
    ///         auditService.LogSecurityEvent(error);
    ///     },
    ///     onValidationException: errors => {
    ///         logger.LogInformation("Order validation failed: {ErrorCount} errors", errors.Count);
    ///         // Validation errors are expected, no alerting needed
    ///     },
    ///     onOperationCanceledException: error => {
    ///         logger.LogInformation("Order processing was cancelled: {Error}", error);
    ///     }
    /// );
    /// </code>
    /// </example>
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

    /// <summary>
    /// Attempts to retrieve the success value from this result.
    /// </summary>
    /// <param name="value">
    /// When this method returns, contains the success value if the result is successful; 
    /// otherwise, contains the default value for type <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the result is successful and <paramref name="value"/> contains the success value; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method provides a safe way to extract values from results using the standard .NET "Try" pattern,
    /// similar to <see cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/> or <see cref="int.TryParse(string, out int)"/>.
    /// </para>
    /// <para>
    /// The method uses the <see cref="NotNullWhenAttribute"/> attribute to help static analysis tools understand
    /// that <paramref name="value"/> will not be <see langword="null"/> when the method returns <see langword="true"/>,
    /// even for nullable reference types.
    /// </para>
    /// <para>
    /// This is particularly useful when you need to conditionally access the success value without
    /// pattern matching, or when integrating with code that expects the "Try" pattern.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; userResult = GetUser(userId);
    /// 
    /// if (userResult.TryGetValue(out User user))
    /// {
    ///     // user is guaranteed to be non-null here
    ///     Console.WriteLine($"Found user: {user.Name}");
    /// }
    /// else
    /// {
    ///     // Handle failure case
    ///     Console.WriteLine($"Failed to get user: {userResult.Error}");
    /// }
    /// </code>
    /// </example>
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

    #region Private Properties

    [JsonInclude]
    private T SuccessValue { get; set; } = default!;

    #endregion Private Properties
}