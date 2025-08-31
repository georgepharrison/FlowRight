using FlowRight.Core.Results;
using FlowRight.Http.Models;
using System.Net;
using System.Net.Mime;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace FlowRight.Http.Extensions;

public static class HttpResponseMessageExtensions
{
    #region Public Methods

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