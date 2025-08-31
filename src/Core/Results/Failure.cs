using System.Security;

namespace FlowRight.Core.Results;

public partial class Result
{
    #region Public Methods

    /// <summary>
    /// Creates a failed result with the specified error message and failure classification.
    /// </summary>
    /// <param name="error">The error message describing what went wrong. Cannot be <see langword="null"/>.</param>
    /// <param name="resultType">The general result type classification. Defaults to <see cref="ResultType.Error"/>.</param>
    /// <param name="resultFailureType">The specific failure type classification. Defaults to <see cref="ResultFailureType.Error"/>.</param>
    /// <returns>A failed <see cref="Result"/> instance containing the error information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This is the primary method for creating general failure results. It allows full control
    /// over both the result type and failure type classifications, enabling precise error categorization.
    /// </para>
    /// <para>
    /// Use this method when you need to create custom failure scenarios that don't fit the
    /// specialized failure methods (validation, security, cancellation).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic error
    /// Result result = Result.Failure("Database connection failed");
    /// 
    /// // Business logic error
    /// Result businessError = Result.Failure(
    ///     "Insufficient inventory for order", 
    ///     ResultType.Error, 
    ///     ResultFailureType.Error
    /// );
    /// </code>
    /// </example>
    public static Result Failure(string error, ResultType resultType = ResultType.Error, ResultFailureType resultFailureType = ResultFailureType.Error) =>
        new(error ?? throw new ArgumentNullException(nameof(error)), resultType, resultFailureType);

    /// <summary>
    /// Creates a failed result with a single field validation error.
    /// </summary>
    /// <param name="key">The name of the field or property that failed validation. Cannot be <see langword="null"/>.</param>
    /// <param name="error">The validation error message for the specified field. Cannot be <see langword="null"/>.</param>
    /// <returns>A failed <see cref="Result"/> instance with <see cref="ResultFailureType.Validation"/> containing the field error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="error"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method creates a validation failure result for a single field. The resulting failure
    /// will have a <see cref="FailureType"/> of <see cref="ResultFailureType.Validation"/> and
    /// the field error will be available in the <see cref="Failures"/> dictionary.
    /// </para>
    /// <para>
    /// This is useful for simple validation scenarios where only one field needs to be validated
    /// or when building up validation errors one at a time.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Single field validation error
    /// Result emailError = Result.Failure("Email", "Email address is required");
    /// 
    /// // The error will be available in Failures dictionary
    /// if (emailError.FailureType == ResultFailureType.Validation)
    /// {
    ///     string[] emailErrors = emailError.Failures["Email"];
    /// }
    /// </code>
    /// </example>
    public static Result Failure(string key, string error) =>
        new(key ?? throw new ArgumentNullException(nameof(key)), error ?? throw new ArgumentNullException(nameof(error)));

    /// <summary>
    /// Creates a failed result from a security exception.
    /// </summary>
    /// <param name="securityException">The security exception that occurred. Cannot be <see langword="null"/>.</param>
    /// <returns>A failed <see cref="Result"/> instance with <see cref="ResultFailureType.Security"/> containing the security error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="securityException"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method creates a security failure result from a <see cref="SecurityException"/>.
    /// The resulting failure will have a <see cref="FailureType"/> of <see cref="ResultFailureType.Security"/>
    /// and should be handled with appropriate security considerations.
    /// </para>
    /// <para>
    /// Security failures typically require special handling such as audit logging,
    /// security monitoring integration, and careful error message sanitization to avoid
    /// information disclosure.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     // Some operation that might throw SecurityException
    ///     PerformSecuritySensitiveOperation();
    ///     return Result.Success();
    /// }
    /// catch (SecurityException ex)
    /// {
    ///     return Result.Failure(ex);
    /// }
    /// </code>
    /// </example>
    public static Result Failure(SecurityException securityException) =>
        new(securityException ?? throw new ArgumentNullException(nameof(securityException)));

    /// <summary>
    /// Creates a failed result from an operation canceled exception.
    /// </summary>
    /// <param name="operationCanceledException">The operation canceled exception that occurred. Cannot be <see langword="null"/>.</param>
    /// <returns>A failed <see cref="Result"/> instance with <see cref="ResultFailureType.OperationCanceled"/> containing the cancellation information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operationCanceledException"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method creates a cancellation failure result from an <see cref="OperationCanceledException"/>.
    /// The resulting failure will have a <see cref="FailureType"/> of <see cref="ResultFailureType.OperationCanceled"/>
    /// and a <see cref="ResultType"/> of <see cref="ResultType.Warning"/>, as cancellation is typically not an error condition.
    /// </para>
    /// <para>
    /// Cancellation failures represent normal application flow control rather than actual errors
    /// and may be handled differently from other failure types (such as being filtered out of error logging).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// async Task&lt;Result&gt; ProcessDataAsync(CancellationToken cancellationToken)
    /// {
    ///     try
    ///     {
    ///         await LongRunningOperation(cancellationToken);
    ///         return Result.Success();
    ///     }
    ///     catch (OperationCanceledException ex)
    ///     {
    ///         return Result.Failure(ex);
    ///     }
    /// }
    /// </code>
    /// </example>
    public static Result Failure(OperationCanceledException operationCanceledException) =>
        new(operationCanceledException ?? throw new ArgumentNullException(nameof(operationCanceledException)));

    /// <summary>
    /// Creates a failed result with multiple validation errors.
    /// </summary>
    /// <param name="errors">A dictionary containing field names as keys and arrays of error messages as values. Cannot be <see langword="null"/>.</param>
    /// <returns>A failed <see cref="Result"/> instance with <see cref="ResultFailureType.Validation"/> containing all the validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errors"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method creates a validation failure result containing multiple field errors.
    /// It's the primary method for creating comprehensive validation failures that can
    /// report errors on multiple fields simultaneously.
    /// </para>
    /// <para>
    /// The error dictionary structure allows for multiple error messages per field,
    /// which is useful for complex validation scenarios where a single field might
    /// fail multiple validation rules.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Dictionary&lt;string, string[]&gt; validationErrors = new()
    /// {
    ///     ["Email"] = ["Email is required", "Email format is invalid"],
    ///     ["Password"] = ["Password must be at least 8 characters"],
    ///     ["Age"] = ["Age must be between 18 and 120"]
    /// };
    /// 
    /// Result validationResult = Result.Failure(validationErrors);
    /// 
    /// // Access specific field errors
    /// if (validationResult.Failures.TryGetValue("Email", out string[] emailErrors))
    /// {
    ///     foreach (string error in emailErrors)
    ///     {
    ///         Console.WriteLine($"Email error: {error}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static Result Failure(IDictionary<string, string[]> errors) =>
        new(errors ?? throw new ArgumentNullException(nameof(errors)));

    /// <summary>
    /// Creates a failed result with the specified error message and result type classification.
    /// </summary>
    /// <typeparam name="T">The type parameter for the generic result.</typeparam>
    /// <param name="error">The error message describing what went wrong. Cannot be <see langword="null"/>.</param>
    /// <param name="resultType">The general result type classification. Defaults to <see cref="ResultType.Error"/>.</param>
    /// <returns>A failed <see cref="Result{T}"/> instance containing the error information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This is the primary method for creating general failure results that would have returned
    /// a typed value. The failure type is automatically set to <see cref="ResultFailureType.Error"/>.
    /// </para>
    /// <para>
    /// Use this method when you need to create custom failure scenarios for operations
    /// that normally return a specific type but have encountered an error condition.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Operation that would return a User
    /// Result&lt;User&gt; GetUserResult(int userId)
    /// {
    ///     if (userId &lt;= 0)
    ///         return Result.Failure&lt;User&gt;("Invalid user ID");
    ///         
    ///     User user = userRepository.GetById(userId);
    ///     return user != null 
    ///         ? Result.Success(user)
    ///         : Result.Failure&lt;User&gt;("User not found");
    /// }
    /// </code>
    /// </example>
    public static Result<T> Failure<T>(string error, ResultType resultType = ResultType.Error) =>
        new(error ?? throw new ArgumentNullException(nameof(error)), resultType);

    /// <summary>
    /// Creates a failed result with a single field validation error for a generic result type.
    /// </summary>
    /// <typeparam name="T">The type parameter for the generic result.</typeparam>
    /// <param name="key">The name of the field or property that failed validation. Cannot be <see langword="null"/>.</param>
    /// <param name="error">The validation error message for the specified field. Cannot be <see langword="null"/>.</param>
    /// <returns>A failed <see cref="Result{T}"/> instance with <see cref="ResultFailureType.Validation"/> containing the field error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="error"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method creates a validation failure result for a single field in the context
    /// of an operation that would normally return a typed value. The resulting failure
    /// will have a <see cref="FailureType"/> of <see cref="ResultFailureType.Validation"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;Order&gt; CreateOrder(CreateOrderRequest request)
    /// {
    ///     if (string.IsNullOrEmpty(request.CustomerEmail))
    ///         return Result.Failure&lt;Order&gt;("CustomerEmail", "Email is required");
    ///         
    ///     // Continue with order creation...
    /// }
    /// </code>
    /// </example>
    public static Result<T> Failure<T>(string key, string error) =>
        new(key ?? throw new ArgumentNullException(nameof(key)), error ?? throw new ArgumentNullException(nameof(error)));

    /// <summary>
    /// Creates a failed result from a security exception for a generic result type.
    /// </summary>
    /// <typeparam name="T">The type parameter for the generic result.</typeparam>
    /// <param name="securityException">The security exception that occurred. Cannot be <see langword="null"/>.</param>
    /// <returns>A failed <see cref="Result{T}"/> instance with <see cref="ResultFailureType.Security"/> containing the security error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="securityException"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method creates a security failure result from a <see cref="SecurityException"/>
    /// for operations that would normally return a typed value. Security failures require
    /// special handling and monitoring considerations.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;Document&gt; GetSecureDocument(int documentId)
    /// {
    ///     try
    ///     {
    ///         Document doc = secureRepository.GetDocument(documentId);
    ///         return Result.Success(doc);
    ///     }
    ///     catch (SecurityException ex)
    ///     {
    ///         return Result.Failure&lt;Document&gt;(ex);
    ///     }
    /// }
    /// </code>
    /// </example>
    public static Result<T> Failure<T>(SecurityException securityException) =>
        new(securityException ?? throw new ArgumentNullException(nameof(securityException)));

    /// <summary>
    /// Creates a failed result from an operation canceled exception for a generic result type.
    /// </summary>
    /// <typeparam name="T">The type parameter for the generic result.</typeparam>
    /// <param name="operationCanceledException">The operation canceled exception that occurred. Cannot be <see langword="null"/>.</param>
    /// <returns>A failed <see cref="Result{T}"/> instance with <see cref="ResultFailureType.OperationCanceled"/> containing the cancellation information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operationCanceledException"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method creates a cancellation failure result from an <see cref="OperationCanceledException"/>
    /// for operations that would normally return a typed value. Cancellation represents normal
    /// flow control rather than an error condition.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// async Task&lt;Result&lt;Data&gt;&gt; FetchDataAsync(CancellationToken cancellationToken)
    /// {
    ///     try
    ///     {
    ///         Data data = await dataService.GetAsync(cancellationToken);
    ///         return Result.Success(data);
    ///     }
    ///     catch (OperationCanceledException ex)
    ///     {
    ///         return Result.Failure&lt;Data&gt;(ex);
    ///     }
    /// }
    /// </code>
    /// </example>
    public static Result<T> Failure<T>(OperationCanceledException operationCanceledException) =>
        new(operationCanceledException ?? throw new ArgumentNullException(nameof(operationCanceledException)));

    /// <summary>
    /// Creates a failed result with multiple validation errors for a generic result type.
    /// </summary>
    /// <typeparam name="T">The type parameter for the generic result.</typeparam>
    /// <param name="errors">A dictionary containing field names as keys and arrays of error messages as values. Cannot be <see langword="null"/>.</param>
    /// <returns>A failed <see cref="Result{T}"/> instance with <see cref="ResultFailureType.Validation"/> containing all the validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errors"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method creates a validation failure result containing multiple field errors
    /// for operations that would normally return a typed value. It's used when comprehensive
    /// validation results in multiple field-level errors.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; CreateUser(CreateUserRequest request)
    /// {
    ///     Dictionary&lt;string, string[]&gt; validationErrors = ValidateUser(request);
    ///     
    ///     if (validationErrors.Any())
    ///         return Result.Failure&lt;User&gt;(validationErrors);
    ///         
    ///     User user = new User(request);
    ///     return Result.Success(user);
    /// }
    /// </code>
    /// </example>
    public static Result<T> Failure<T>(IDictionary<string, string[]> errors) =>
        new(errors ?? throw new ArgumentNullException(nameof(errors)));

    #endregion Public Methods
}