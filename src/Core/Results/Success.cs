namespace FlowRight.Core.Results;

public partial class Result
{
    #region Public Methods

    /// <summary>
    /// Creates a successful result without a value.
    /// </summary>
    /// <param name="resultType">The type classification for this successful result. Defaults to <see cref="ResultType.Success"/>.</param>
    /// <returns>A successful <see cref="Result"/> instance.</returns>
    /// <remarks>
    /// <para>
    /// This method creates a non-generic result that represents a successful operation
    /// that doesn't return a specific value (similar to a void method that completed successfully).
    /// </para>
    /// <para>
    /// Common usage scenarios include:
    /// <list type="bullet">
    /// <item><description>Operations that modify state but don't return data</description></item>
    /// <item><description>Validation operations that either pass or fail</description></item>
    /// <item><description>Delete or update operations</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic success
    /// Result result = Result.Success();
    /// 
    /// // Success with additional classification
    /// Result infoResult = Result.Success(ResultType.Information);
    /// Result warningResult = Result.Success(ResultType.Warning);
    /// </code>
    /// </example>
    public static Result Success(ResultType resultType = ResultType.Success) =>
        new(resultType);

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="value">The value to wrap in the result. Cannot be <see langword="null"/>.</param>
    /// <param name="resultType">The type classification for this successful result. Defaults to <see cref="ResultType.Success"/>.</param>
    /// <returns>A successful <see cref="Result{T}"/> instance containing the specified value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This is the primary method for creating successful results that return typed values.
    /// The result will be immutable and the value will be safely encapsulated.
    /// </para>
    /// <para>
    /// The result type parameter allows for additional classification of successful operations:
    /// <list type="bullet">
    /// <item><description><see cref="ResultType.Success"/> - Standard successful operation</description></item>
    /// <item><description><see cref="ResultType.Information"/> - Success with informational context</description></item>
    /// <item><description><see cref="ResultType.Warning"/> - Success but with warnings</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create successful result with value
    /// User user = GetCurrentUser();
    /// Result&lt;User&gt; userResult = Result.Success(user);
    /// 
    /// // Success with additional context
    /// Result&lt;string&gt; messageResult = Result.Success("Operation completed", ResultType.Information);
    /// 
    /// // Pattern matching usage
    /// string displayMessage = userResult.Match(
    ///     onSuccess: user => $"Welcome, {user.Name}!",
    ///     onFailure: error => $"Error: {error}"
    /// );
    /// </code>
    /// </example>
    public static Result<T> Success<T>(T value, ResultType resultType = ResultType.Success) =>
        new(value ?? throw new ArgumentNullException(nameof(value)), resultType);

    #endregion Public Methods
}