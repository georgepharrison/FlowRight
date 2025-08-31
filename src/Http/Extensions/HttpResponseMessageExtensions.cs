using FlowRight.Core.Results;
using FlowRight.Http.Models;
using System.Net;
using System.Net.Mime;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace FlowRight.Http.Extensions;

/// <summary>
/// Provides extension methods for converting HTTP response messages to Result types.
/// </summary>
public static class HttpResponseMessageExtensions
{
    #region Public Methods

    /// <summary>
    /// Converts an HTTP response message to a Result, handling various HTTP status codes appropriately.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A Result representing the success or failure of the HTTP operation.</returns>
    /// <example>
    /// <code>
    /// HttpResponseMessage response = await client.GetAsync("/api/users");
    /// Result result = await response.ToResultAsync();
    /// if (result.IsSuccess)
    /// {
    ///     // Handle success
    /// }
    /// </code>
    /// </example>
    public static async Task<Result> ToResultAsync(this HttpResponseMessage responseMessage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);

        if (responseMessage.IsSuccessStatusCode)
        {
            return Result.Success();
        }

        return responseMessage.StatusCode switch
        {
            HttpStatusCode.BadRequest => await responseMessage.HandleBadRequestAsync(cancellationToken).ConfigureAwait(false),
            HttpStatusCode.Unauthorized or
            HttpStatusCode.Forbidden => Result.Failure(new SecurityException("Unauthorized")),
            HttpStatusCode.NotFound => Result.Failure("Not Found"),
            HttpStatusCode.InternalServerError => Result.Failure("Internal Server Error"),
            _ => Result.Failure(await GetUnexpectedStatusCodeFailureAsync(responseMessage, cancellationToken).ConfigureAwait(false))
        };
    }

    /// <summary>
    /// Converts an HTTP response message to a Result&lt;T&gt; by deserializing the JSON content.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON content to.</typeparam>
    /// <param name="responseMessage">The HTTP response message to convert.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A Result&lt;T&gt; containing the deserialized value on success, or failure information on error.</returns>
    /// <example>
    /// <code>
    /// HttpResponseMessage response = await client.GetAsync("/api/users/1");
    /// Result&lt;User?&gt; userResult = await response.ToResultFromJsonAsync&lt;User&gt;();
    /// if (userResult.IsSuccess)
    /// {
    ///     User? user = userResult.Value;
    /// }
    /// </code>
    /// </example>
    public static async Task<Result<T?>> ToResultFromJsonAsync<T>(this HttpResponseMessage responseMessage, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);

        if (responseMessage.IsSuccessStatusCode)
        {
            await using Stream stream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            T? value = await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken)
                .ConfigureAwait(false);

            return Result.Success(value);
        }

        return responseMessage.StatusCode switch
        {
            HttpStatusCode.BadRequest => await responseMessage.HandleBadRequestAsync<T?>(cancellationToken).ConfigureAwait(false),
            HttpStatusCode.Unauthorized or
            HttpStatusCode.Forbidden => Result.Failure<T?>(new SecurityException("Unauthorized")),
            HttpStatusCode.NotFound => Result.Failure<T?>("Not Found"),
            HttpStatusCode.InternalServerError => Result.Failure<T?>("Internal Server Error"),
            _ => Result.Failure<T?>(await GetUnexpectedStatusCodeFailureAsync(responseMessage, cancellationToken).ConfigureAwait(false))
        };
    }

    /// <summary>
    /// Converts an HTTP response message to a Result&lt;T&gt; by deserializing the JSON content using a JsonTypeInfo.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON content to.</typeparam>
    /// <param name="responseMessage">The HTTP response message to convert.</param>
    /// <param name="jsonTypeInfo">The JsonTypeInfo for source generation serialization.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A Result&lt;T&gt; containing the deserialized value on success, or failure information on error.</returns>
    /// <example>
    /// <code>
    /// HttpResponseMessage response = await client.GetAsync("/api/users/1");
    /// Result&lt;User?&gt; userResult = await response.ToResultFromJsonAsync(UserJsonContext.Default.User);
    /// if (userResult.IsSuccess)
    /// {
    ///     User? user = userResult.Value;
    /// }
    /// </code>
    /// </example>
    public static async Task<Result<T?>> ToResultFromJsonAsync<T>(this HttpResponseMessage responseMessage, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);

        if (responseMessage.IsSuccessStatusCode)
        {
            await using Stream stream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            T? value = await JsonSerializer.DeserializeAsync(stream, jsonTypeInfo, cancellationToken)
                .ConfigureAwait(false);

            return Result.Success(value);
        }

        return responseMessage.StatusCode switch
        {
            HttpStatusCode.BadRequest => await responseMessage.HandleBadRequestAsync<T?>(cancellationToken).ConfigureAwait(false),
            HttpStatusCode.Unauthorized or
            HttpStatusCode.Forbidden => Result.Failure<T?>(new SecurityException("Unauthorized")),
            HttpStatusCode.NotFound => Result.Failure<T?>("Not Found"),
            HttpStatusCode.InternalServerError => Result.Failure<T?>("Internal Server Error"),
            _ => Result.Failure<T?>(await GetUnexpectedStatusCodeFailureAsync(responseMessage, cancellationToken).ConfigureAwait(false))
        };
    }

    #endregion Public Methods

    #region Private Methods

    private static async Task<string> GetUnexpectedStatusCodeFailureAsync(HttpResponseMessage responseMessage, CancellationToken cancellationToken)
    {
        string body = await responseMessage.Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        return $"Unexpected {responseMessage.StatusCode}: {body}";
    }

    private static async Task<Result> HandleBadRequestAsync(this HttpResponseMessage responseMessage, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);

        if (responseMessage.Content.Headers.ContentType?.MediaType == MediaTypeNames.Application.ProblemJson)
        {
            await using Stream stream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            ValidationProblemResponse validationProblems = await JsonSerializer.DeserializeAsync(stream, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse, cancellationToken)
                .ConfigureAwait(false) ?? new ValidationProblemResponse();

            return Result.Failure(validationProblems.Errors);
        }

        string error = await responseMessage.Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result.Failure(error);
    }

    private static async Task<Result<T>> HandleBadRequestAsync<T>(this HttpResponseMessage responseMessage, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);

        if (responseMessage.Content.Headers.ContentType?.MediaType == MediaTypeNames.Application.ProblemJson)
        {
            await using Stream stream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            ValidationProblemResponse validationProblems = await JsonSerializer.DeserializeAsync(stream, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse, cancellationToken)
                .ConfigureAwait(false) ?? new ValidationProblemResponse();

            return Result.Failure<T>(validationProblems.Errors);
        }

        string error = await responseMessage.Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result.Failure<T>(error);
    }

    #endregion Private Methods
}