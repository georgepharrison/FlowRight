using System.Security;
using FlowRight.Core.Results;

namespace FlowRight.Core.Extensions;

/// <summary>
/// Provides async-friendly extension methods for the Result and Result&lt;T&gt; types,
/// enabling seamless composition of asynchronous operations with the Result pattern.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods bridge the gap between synchronous Result pattern operations
/// and asynchronous workflows, allowing for clean and efficient async/await usage
/// while maintaining the same error handling semantics as the synchronous counterparts.
/// </para>
/// <para>
/// The async extensions preserve all error categorization, validation information,
/// and Result type classifications from the original Result pattern implementation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Async pattern matching
/// Result result = await GetDataAsync();
/// string response = await result.MatchAsync(
///     onSuccess: async () => await ProcessSuccessAsync(),
///     onFailure: async error => await HandleErrorAsync(error)
/// );
/// 
/// // Async operation chaining
/// Result&lt;User&gt; userResult = await GetUserAsync()
///     .ThenAsync(async user => await ValidateUserAsync(user))
///     .ThenAsync(async validUser => await SaveUserAsync(validUser));
/// 
/// // Async value transformation
/// Result&lt;string&gt; processedResult = await userResult
///     .MapAsync(async user => await FormatUserDisplayNameAsync(user));
/// </code>
/// </example>
public static class ResultAsyncExtensions
{
    #region MatchAsync Extensions for Result

    /// <summary>
    /// Transforms this result into a value of type <typeparamref name="TResult"/> using async pattern matching.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="result">The result to perform pattern matching on.</param>
    /// <param name="onSuccess">Async function to execute if the result is successful.</param>
    /// <param name="onFailure">Async function to execute if the result is a failure. Receives the error message.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the value returned by either <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This is the async equivalent of the <see cref="Result.Match{TResult}(Func{TResult}, Func{string, TResult})"/> method.
    /// It enables pattern matching with async handlers, allowing for clean async/await usage within result handling.
    /// </para>
    /// <para>
    /// This method treats all failure types uniformly, calling <paramref name="onFailure"/> with the error message.
    /// For more granular async failure handling, use the overload that provides separate handlers for each failure type.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result operationResult = await PerformOperationAsync();
    /// 
    /// string response = await operationResult.MatchAsync(
    ///     onSuccess: async () => {
    ///         await LogSuccessAsync("Operation completed");
    ///         return "Success!";
    ///     },
    ///     onFailure: async error => {
    ///         await LogErrorAsync($"Operation failed: {error}");
    ///         return "Failed";
    ///     }
    /// );
    /// </code>
    /// </example>
    public static async Task<TResult> MatchAsync<TResult>(
        this Result result,
        Func<Task<TResult>> onSuccess,
        Func<string, Task<TResult>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return result.IsSuccess
            ? await onSuccess().ConfigureAwait(false)
            : await onFailure(result.Error).ConfigureAwait(false);
    }

    /// <summary>
    /// Transforms this result into a value of type <typeparamref name="TResult"/> using async pattern matching with specific handlers for each failure type.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="result">The result to perform pattern matching on.</param>
    /// <param name="onSuccess">Async function to execute if the result is successful.</param>
    /// <param name="onError">Async function to execute if the result is a general error. Receives the error message.</param>
    /// <param name="onSecurityException">Async function to execute if the result is a security failure. Receives the error message.</param>
    /// <param name="onValidationException">Async function to execute if the result is a validation failure. Receives the validation errors dictionary.</param>
    /// <param name="onOperationCanceledException">Async function to execute if the result is a cancellation failure. Receives the error message.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the value returned by the appropriate handler function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> or any of the handler functions is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This async overload provides granular control over different failure types with async handlers,
    /// allowing you to implement specific async logic for each category of failure. This is particularly useful
    /// when different failure types require different async handling strategies (async logging, async notifications, etc.).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result operationResult = await ProcessRequestAsync();
    /// 
    /// string response = await operationResult.MatchAsync(
    ///     onSuccess: async () => await SendSuccessNotificationAsync(),
    ///     onError: async error => await HandleSystemErrorAsync(error),
    ///     onSecurityException: async error => await ReportSecurityViolationAsync(error),
    ///     onValidationException: async errors => await DisplayValidationErrorsAsync(errors),
    ///     onOperationCanceledException: async error => await HandleCancellationAsync(error)
    /// );
    /// </code>
    /// </example>
    public static async Task<TResult> MatchAsync<TResult>(
        this Result result,
        Func<Task<TResult>> onSuccess,
        Func<string, Task<TResult>> onError,
        Func<string, Task<TResult>> onSecurityException,
        Func<IDictionary<string, string[]>, Task<TResult>> onValidationException,
        Func<string, Task<TResult>> onOperationCanceledException)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        ArgumentNullException.ThrowIfNull(onSecurityException);
        ArgumentNullException.ThrowIfNull(onValidationException);
        ArgumentNullException.ThrowIfNull(onOperationCanceledException);

        if (result.IsSuccess)
        {
            return await onSuccess().ConfigureAwait(false);
        }

        return result.FailureType switch
        {
            ResultFailureType.Error => await onError(result.Error).ConfigureAwait(false),
            ResultFailureType.Security => await onSecurityException(result.Error).ConfigureAwait(false),
            ResultFailureType.Validation => await onValidationException(result.Failures).ConfigureAwait(false),
            ResultFailureType.OperationCanceled => await onOperationCanceledException(result.Error).ConfigureAwait(false),
            _ => throw new NotImplementedException($"FailureType {result.FailureType} is not implemented.")
        };
    }

    #endregion MatchAsync Extensions for Result

    #region MatchAsync Extensions for Result<T>

    /// <summary>
    /// Transforms this result into a value of type <typeparamref name="TResult"/> using async pattern matching.
    /// </summary>
    /// <typeparam name="T">The type of the success value in the result.</typeparam>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="result">The result to perform pattern matching on.</param>
    /// <param name="onSuccess">Async function to execute if the result is successful. Receives the success value.</param>
    /// <param name="onFailure">Async function to execute if the result is a failure. Receives the error message.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the value returned by either <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This is the async equivalent of the <see cref="Result{T}.Match{TResult}(Func{T, TResult}, Func{string, TResult})"/> method.
    /// It enables pattern matching with async handlers that can work with the success value.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; userResult = await GetUserAsync(userId);
    /// 
    /// string response = await userResult.MatchAsync(
    ///     onSuccess: async user => {
    ///         await AuditUserAccessAsync(user);
    ///         return $"Welcome, {user.Name}!";
    ///     },
    ///     onFailure: async error => {
    ///         await LogFailedLoginAsync(userId, error);
    ///         return "Login failed";
    ///     }
    /// );
    /// </code>
    /// </example>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Result<T> result,
        Func<T, Task<TResult>> onSuccess,
        Func<string, Task<TResult>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (result.IsSuccess && result.TryGetValue(out T? value))
        {
            return await onSuccess(value).ConfigureAwait(false);
        }

        return await onFailure(result.Error).ConfigureAwait(false);
    }

    /// <summary>
    /// Transforms this result into a value of type <typeparamref name="TResult"/> using async pattern matching with specific handlers for each failure type.
    /// </summary>
    /// <typeparam name="T">The type of the success value in the result.</typeparam>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="result">The result to perform pattern matching on.</param>
    /// <param name="onSuccess">Async function to execute if the result is successful. Receives the success value.</param>
    /// <param name="onError">Async function to execute if the result is a general error. Receives the error message.</param>
    /// <param name="onSecurityException">Async function to execute if the result is a security failure. Receives the error message.</param>
    /// <param name="onValidationException">Async function to execute if the result is a validation failure. Receives the validation errors dictionary.</param>
    /// <param name="onOperationCanceledException">Async function to execute if the result is a cancellation failure. Receives the error message.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the value returned by the appropriate handler function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> or any of the handler functions is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// Result&lt;Order&gt; orderResult = await ProcessOrderAsync(request);
    /// 
    /// IActionResult response = await orderResult.MatchAsync(
    ///     onSuccess: async order => {
    ///         await SendOrderConfirmationAsync(order);
    ///         return Ok(order);
    ///     },
    ///     onError: async error => await LogAndReturnErrorAsync(error),
    ///     onSecurityException: async error => await HandleSecurityViolationAsync(error),
    ///     onValidationException: async errors => await ReturnValidationErrorsAsync(errors),
    ///     onOperationCanceledException: async error => await HandleTimeoutAsync(error)
    /// );
    /// </code>
    /// </example>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Result<T> result,
        Func<T, Task<TResult>> onSuccess,
        Func<string, Task<TResult>> onError,
        Func<string, Task<TResult>> onSecurityException,
        Func<IDictionary<string, string[]>, Task<TResult>> onValidationException,
        Func<string, Task<TResult>> onOperationCanceledException)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        ArgumentNullException.ThrowIfNull(onSecurityException);
        ArgumentNullException.ThrowIfNull(onValidationException);
        ArgumentNullException.ThrowIfNull(onOperationCanceledException);

        if (result.IsSuccess && result.TryGetValue(out T? value))
        {
            return await onSuccess(value).ConfigureAwait(false);
        }

        return result.FailureType switch
        {
            ResultFailureType.Error => await onError(result.Error).ConfigureAwait(false),
            ResultFailureType.Security => await onSecurityException(result.Error).ConfigureAwait(false),
            ResultFailureType.Validation => await onValidationException(result.Failures).ConfigureAwait(false),
            ResultFailureType.OperationCanceled => await onOperationCanceledException(result.Error).ConfigureAwait(false),
            _ => throw new NotImplementedException($"FailureType {result.FailureType} is not implemented.")
        };
    }

    #endregion MatchAsync Extensions for Result<T>

    #region SwitchAsync Extensions for Result

    /// <summary>
    /// Executes async side-effect actions based on the result state.
    /// </summary>
    /// <param name="result">The result to perform switching on.</param>
    /// <param name="onSuccess">Async action to execute if the result is successful.</param>
    /// <param name="onFailure">Async action to execute if the result is a failure. Receives the error message.</param>
    /// <param name="includeOperationCancelledFailures">If <see langword="true"/>, operation cancelled failures will call <paramref name="onFailure"/>. If <see langword="false"/> (default), they will be ignored.</param>
    /// <returns>A <see cref="Task"/> representing the async operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This is the async equivalent of the imperative <see cref="Result.Switch(Action, Action{string}, bool)"/> method.
    /// It executes async actions with side effects rather than returning transformed values.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result result = await PerformOperationAsync();
    /// 
    /// await result.SwitchAsync(
    ///     onSuccess: async () => {
    ///         await LogSuccessAsync("Operation completed");
    ///         await NotifySubscribersAsync();
    ///     },
    ///     onFailure: async error => {
    ///         await LogErrorAsync($"Operation failed: {error}");
    ///         await SendAlertAsync(error);
    ///     }
    /// );
    /// </code>
    /// </example>
    public static async Task SwitchAsync(
        this Result result,
        Func<Task> onSuccess,
        Func<string, Task> onFailure,
        bool includeOperationCancelledFailures = false)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (result.IsSuccess)
        {
            await onSuccess().ConfigureAwait(false);
            return;
        }

        if (result.FailureType == ResultFailureType.OperationCanceled && !includeOperationCancelledFailures)
        {
            return;
        }

        await onFailure(result.Error).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes async side-effect actions based on the result state with separate handlers for different failure types.
    /// </summary>
    /// <param name="result">The result to perform switching on.</param>
    /// <param name="onSuccess">Async action to execute if the result is successful.</param>
    /// <param name="onError">Async action to execute if the result is a general error. Receives the error message.</param>
    /// <param name="onSecurityException">Async action to execute if the result is a security failure. Receives the error message.</param>
    /// <param name="onValidationException">Async action to execute if the result is a validation failure. Receives the validation errors dictionary.</param>
    /// <param name="onOperationCanceledException">Optional async action to execute if the result is a cancellation failure. Receives the error message. If <see langword="null"/>, cancellation failures are ignored.</param>
    /// <returns>A <see cref="Task"/> representing the async operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/>, <paramref name="onSuccess"/>, <paramref name="onError"/>, <paramref name="onSecurityException"/>, or <paramref name="onValidationException"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// Result result = await ProcessRequestAsync();
    /// 
    /// await result.SwitchAsync(
    ///     onSuccess: async () => await SendSuccessNotificationAsync(),
    ///     onError: async error => await LogSystemErrorAsync(error),
    ///     onSecurityException: async error => await AlertSecurityTeamAsync(error),
    ///     onValidationException: async errors => await DisplayValidationErrorsAsync(errors),
    ///     onOperationCanceledException: async error => await CleanupResourcesAsync()
    /// );
    /// </code>
    /// </example>
    public static async Task SwitchAsync(
        this Result result,
        Func<Task> onSuccess,
        Func<string, Task> onError,
        Func<string, Task> onSecurityException,
        Func<IDictionary<string, string[]>, Task> onValidationException,
        Func<string, Task>? onOperationCanceledException = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        ArgumentNullException.ThrowIfNull(onSecurityException);
        ArgumentNullException.ThrowIfNull(onValidationException);

        if (result.IsSuccess)
        {
            await onSuccess().ConfigureAwait(false);
            return;
        }

        switch (result.FailureType)
        {
            case ResultFailureType.Error:
                await onError(result.Error).ConfigureAwait(false);
                break;

            case ResultFailureType.Security:
                await onSecurityException(result.Error).ConfigureAwait(false);
                break;

            case ResultFailureType.Validation:
                await onValidationException(result.Failures).ConfigureAwait(false);
                break;

            case ResultFailureType.OperationCanceled:
                if (onOperationCanceledException is not null)
                {
                    await onOperationCanceledException(result.Error).ConfigureAwait(false);
                }
                break;

            default:
                throw new NotImplementedException($"FailureType {result.FailureType} is not implemented.");
        }
    }

    #endregion SwitchAsync Extensions for Result

    #region SwitchAsync Extensions for Result<T>

    /// <summary>
    /// Executes async side-effect actions based on the result state.
    /// </summary>
    /// <typeparam name="T">The type of the success value in the result.</typeparam>
    /// <param name="result">The result to perform switching on.</param>
    /// <param name="onSuccess">Async action to execute if the result is successful. Receives the success value.</param>
    /// <param name="onFailure">Async action to execute if the result is a failure. Receives the error message.</param>
    /// <param name="includeOperationCancelledFailures">Whether to execute <paramref name="onFailure"/> for <see cref="ResultFailureType.OperationCanceled"/> failures. Defaults to <see langword="false"/>.</param>
    /// <returns>A <see cref="Task"/> representing the async operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/>, <paramref name="onSuccess"/>, or <paramref name="onFailure"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; userResult = await GetUserAsync(userId);
    /// 
    /// await userResult.SwitchAsync(
    ///     onSuccess: async user => {
    ///         await LogUserAccessAsync(user);
    ///         await UpdateLastLoginAsync(user);
    ///     },
    ///     onFailure: async error => {
    ///         await LogFailedLoginAsync(userId, error);
    ///         await IncrementFailedLoginCountAsync(userId);
    ///     }
    /// );
    /// </code>
    /// </example>
    public static async Task SwitchAsync<T>(
        this Result<T> result,
        Func<T, Task> onSuccess,
        Func<string, Task> onFailure,
        bool includeOperationCancelledFailures = false)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (result.IsSuccess && result.TryGetValue(out T? value))
        {
            await onSuccess(value).ConfigureAwait(false);
            return;
        }

        switch (result.FailureType)
        {
            case ResultFailureType.Error:
            case ResultFailureType.Security:
            case ResultFailureType.Validation:
                await onFailure(result.Error).ConfigureAwait(false);
                break;

            case ResultFailureType.OperationCanceled:
                if (includeOperationCancelledFailures)
                {
                    await onFailure(result.Error).ConfigureAwait(false);
                }
                break;
        }
    }

    /// <summary>
    /// Executes async side-effect actions based on the result state with separate handlers for different failure types.
    /// </summary>
    /// <typeparam name="T">The type of the success value in the result.</typeparam>
    /// <param name="result">The result to perform switching on.</param>
    /// <param name="onSuccess">Async action to execute if the result is successful. Receives the success value.</param>
    /// <param name="onError">Async action to execute if the result is a general error. Receives the error message.</param>
    /// <param name="onSecurityException">Async action to execute if the result is a security failure. Receives the error message.</param>
    /// <param name="onValidationException">Async action to execute if the result is a validation failure. Receives the validation errors dictionary.</param>
    /// <param name="onOperationCanceledException">Optional async action to execute if the result is a cancellation failure. Receives the error message. If <see langword="null"/>, cancellation failures are ignored.</param>
    /// <returns>A <see cref="Task"/> representing the async operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/>, <paramref name="onSuccess"/>, <paramref name="onError"/>, <paramref name="onSecurityException"/>, or <paramref name="onValidationException"/> is <see langword="null"/>.</exception>
    public static async Task SwitchAsync<T>(
        this Result<T> result,
        Func<T, Task> onSuccess,
        Func<string, Task> onError,
        Func<string, Task> onSecurityException,
        Func<IDictionary<string, string[]>, Task> onValidationException,
        Func<string, Task>? onOperationCanceledException = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);
        ArgumentNullException.ThrowIfNull(onSecurityException);
        ArgumentNullException.ThrowIfNull(onValidationException);

        if (result.IsSuccess && result.TryGetValue(out T? value))
        {
            await onSuccess(value).ConfigureAwait(false);
            return;
        }

        switch (result.FailureType)
        {
            case ResultFailureType.Error:
                await onError(result.Error).ConfigureAwait(false);
                break;

            case ResultFailureType.Security:
                await onSecurityException(result.Error).ConfigureAwait(false);
                break;

            case ResultFailureType.Validation:
                await onValidationException(result.Failures).ConfigureAwait(false);
                break;

            case ResultFailureType.OperationCanceled:
                if (onOperationCanceledException is not null)
                {
                    await onOperationCanceledException(result.Error).ConfigureAwait(false);
                }
                break;

            default:
                throw new NotImplementedException($"FailureType {result.FailureType} is not implemented.");
        }
    }

    #endregion SwitchAsync Extensions for Result<T>

    #region ThenAsync Extensions

    /// <summary>
    /// Chains an async operation that returns a Result&lt;T&gt; to be executed only if this result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the next operation.</typeparam>
    /// <param name="result">The result to chain from.</param>
    /// <param name="nextAsync">The async function to execute if this result is successful.</param>
    /// <returns>
    /// A <see cref="Task{Result}"/> that contains the result of <paramref name="nextAsync"/> if this result is successful,
    /// or the failure information from this result if it failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> or <paramref name="nextAsync"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method enables async operation chaining while preserving the Result pattern semantics.
    /// If the current result is a failure, the next operation is not executed and the failure is propagated.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result result = await ValidateInputAsync();
    /// 
    /// Result&lt;User&gt; userResult = await result.ThenAsync(async () =>
    ///     await CreateUserAsync(validatedInput)
    /// );
    /// </code>
    /// </example>
    public static async Task<Result<T>> ThenAsync<T>(
        this Result result,
        Func<Task<Result<T>>> nextAsync)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(nextAsync);

        if (result.IsFailure)
        {
            return ConvertToGenericResult<T>(result);
        }

        return await nextAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Chains an async operation that returns a Result to be executed only if this result is successful.
    /// </summary>
    /// <param name="result">The result to chain from.</param>
    /// <param name="nextAsync">The async function to execute if this result is successful.</param>
    /// <returns>
    /// A <see cref="Task{Result}"/> that contains the result of <paramref name="nextAsync"/> if this result is successful,
    /// or the failure information from this result if it failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> or <paramref name="nextAsync"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// Result result = await ValidateInputAsync();
    /// 
    /// Result finalResult = await result.ThenAsync(async () =>
    ///     await ProcessValidatedInputAsync()
    /// );
    /// </code>
    /// </example>
    public static async Task<Result> ThenAsync(
        this Result result,
        Func<Task<Result>> nextAsync)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(nextAsync);

        if (result.IsFailure)
        {
            return result;
        }

        return await nextAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Chains an async operation that returns a Result&lt;TNext&gt; to be executed only if this result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the success value in the current result.</typeparam>
    /// <typeparam name="TNext">The type of the result returned by the next operation.</typeparam>
    /// <param name="result">The result to chain from.</param>
    /// <param name="nextAsync">The async function to execute if this result is successful. Receives the success value.</param>
    /// <returns>
    /// A <see cref="Task{Result}"/> that contains the result of <paramref name="nextAsync"/> if this result is successful,
    /// or the failure information from this result if it failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> or <paramref name="nextAsync"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; userResult = await GetUserAsync();
    /// 
    /// Result&lt;Order&gt; orderResult = await userResult.ThenAsync(async user =>
    ///     await CreateOrderForUserAsync(user)
    /// );
    /// </code>
    /// </example>
    public static async Task<Result<TNext>> ThenAsync<T, TNext>(
        this Result<T> result,
        Func<T, Task<Result<TNext>>> nextAsync)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(nextAsync);

        if (result.IsFailure)
        {
            return ConvertToGenericResult<TNext>(result);
        }

        if (result.TryGetValue(out T? value))
        {
            return await nextAsync(value).ConfigureAwait(false);
        }

        return Result.Failure<TNext>("Unable to extract value from successful result");
    }

    /// <summary>
    /// Chains an async operation that returns a Result to be executed only if this result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the success value in the current result.</typeparam>
    /// <param name="result">The result to chain from.</param>
    /// <param name="nextAsync">The async function to execute if this result is successful. Receives the success value.</param>
    /// <returns>
    /// A <see cref="Task{Result}"/> that contains the result of <paramref name="nextAsync"/> if this result is successful,
    /// or the failure information from this result if it failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> or <paramref name="nextAsync"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; userResult = await GetUserAsync();
    /// 
    /// Result finalResult = await userResult.ThenAsync(async user =>
    ///     await ValidateAndSaveUserAsync(user)
    /// );
    /// </code>
    /// </example>
    public static async Task<Result> ThenAsync<T>(
        this Result<T> result,
        Func<T, Task<Result>> nextAsync)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(nextAsync);

        if (result.IsFailure)
        {
            return result; // Implicit conversion from Result<T> to Result
        }

        if (result.TryGetValue(out T? value))
        {
            return await nextAsync(value).ConfigureAwait(false);
        }

        return Result.Failure("Unable to extract value from successful result");
    }

    #endregion ThenAsync Extensions

    #region MapAsync Extensions

    /// <summary>
    /// Transforms the success value of this result using an async function.
    /// </summary>
    /// <typeparam name="T">The type of the success value in the current result.</typeparam>
    /// <typeparam name="TResult">The type of the transformed result.</typeparam>
    /// <param name="result">The result to transform.</param>
    /// <param name="mapAsync">The async function to transform the success value.</param>
    /// <returns>
    /// A <see cref="Task{Result}"/> containing the transformed value if this result is successful,
    /// or the failure information from this result if it failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> or <paramref name="mapAsync"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method allows for async transformation of successful result values while preserving
    /// the Result pattern semantics. If the current result is a failure, the transformation is not
    /// executed and the failure is preserved.
    /// </para>
    /// <para>
    /// Unlike <see cref="ThenAsync{T, TNext}"/>, this method wraps the transformation result in a success Result,
    /// making it suitable for pure value transformations rather than operations that can themselves fail.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; userResult = await GetUserAsync();
    /// 
    /// Result&lt;string&gt; displayNameResult = await userResult.MapAsync(async user =>
    ///     await FormatUserDisplayNameAsync(user)
    /// );
    /// </code>
    /// </example>
    public static async Task<Result<TResult>> MapAsync<T, TResult>(
        this Result<T> result,
        Func<T, Task<TResult>> mapAsync)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(mapAsync);

        if (result.IsFailure)
        {
            return ConvertToGenericResult<TResult>(result);
        }

        if (result.TryGetValue(out T? value))
        {
            TResult transformedValue = await mapAsync(value).ConfigureAwait(false);
            return Result.Success(transformedValue, result.ResultType);
        }

        return Result.Failure<TResult>("Unable to extract value from successful result");
    }

    #endregion MapAsync Extensions

    #region CombineAsync Extensions

    /// <summary>
    /// Combines multiple async Result operations, waiting for all to complete and aggregating failure information.
    /// </summary>
    /// <param name="tasks">The array of async Result operations to combine.</param>
    /// <returns>
    /// A <see cref="Task{Result}"/> that is successful if all input operations are successful,
    /// or a failure result containing aggregated error information from all failed operations.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tasks"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method provides async aggregation of multiple Result operations, enabling parallel
    /// execution while collecting all errors rather than failing on the first error encountered.
    /// </para>
    /// <para>
    /// All tasks are awaited concurrently for optimal performance. The combining logic preserves
    /// error categorization just like the synchronous <see cref="Result.Combine(Result[])"/> method.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Task&lt;Result&gt;[] operations = [
    ///     ValidateUserAsync(user),
    ///     ValidatePermissionsAsync(user),
    ///     ValidateDataAsync(data)
    /// ];
    /// 
    /// Result combinedResult = await Result.CombineAsync(operations);
    /// 
    /// if (combinedResult.IsFailure)
    /// {
    ///     // Handle all collected errors at once
    ///     LogErrors(combinedResult.Failures);
    /// }
    /// </code>
    /// </example>
    public static async Task<Result> CombineAsync(params Task<Result>[] tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        Result[] results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return Result.Combine(results);
    }

    /// <summary>
    /// Combines multiple async Result&lt;T&gt; operations, waiting for all to complete and aggregating failure information.
    /// </summary>
    /// <typeparam name="T">The type of the success values in the results.</typeparam>
    /// <param name="tasks">The array of async Result&lt;T&gt; operations to combine.</param>
    /// <returns>
    /// A <see cref="Task{Result}"/> that is successful with the value from the first successful result if all input operations are successful,
    /// or a failure result containing aggregated error information from all failed operations.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tasks"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// Task&lt;Result&lt;User&gt;&gt;[] operations = [
    ///     ValidateUserAsync(user),
    ///     EnrichUserDataAsync(user),
    ///     CheckUserPermissionsAsync(user)
    /// ];
    /// 
    /// Result&lt;User&gt; combinedResult = await Result.CombineAsync(operations);
    /// 
    /// if (combinedResult.IsFailure)
    /// {
    ///     // Handle all collected errors at once
    ///     LogErrors(combinedResult.Failures);
    /// }
    /// </code>
    /// </example>
    public static async Task<Result<T>> CombineAsync<T>(params Task<Result<T>>[] tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        Result<T>[] results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return Result.Combine(results);
    }

    #endregion CombineAsync Extensions

    #region Private Helper Methods

    private static Result<T> ConvertToGenericResult<T>(Result result)
    {
        return result.FailureType switch
        {
            ResultFailureType.Error => Result.Failure<T>(result.Error, result.ResultType),
            ResultFailureType.Security => Result.Failure<T>(new SecurityException(result.Error)),
            ResultFailureType.Validation => Result.Failure<T>(result.Failures),
            ResultFailureType.OperationCanceled => Result.Failure<T>(new OperationCanceledException(result.Error)),
            _ => Result.Failure<T>("Unknown failure type")
        };
    }

    private static Result<T> ConvertToGenericResult<T, TSource>(Result<TSource> result)
    {
        return result.FailureType switch
        {
            ResultFailureType.Error => Result.Failure<T>(result.Error, result.ResultType),
            ResultFailureType.Security => Result.Failure<T>(new SecurityException(result.Error)),
            ResultFailureType.Validation => Result.Failure<T>(result.Failures),
            ResultFailureType.OperationCanceled => Result.Failure<T>(new OperationCanceledException(result.Error)),
            _ => Result.Failure<T>("Unknown failure type")
        };
    }

    #endregion Private Helper Methods
}