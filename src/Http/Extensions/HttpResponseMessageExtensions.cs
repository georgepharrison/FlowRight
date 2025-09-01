using FlowRight.Core.Results;
using FlowRight.Http.Models;
using System.Collections.Specialized;
using System.Net;
using System.Net.Mime;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

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
        cancellationToken.ThrowIfCancellationRequested();

        if (IsSuccessStatusCode2xx(responseMessage.StatusCode))
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
        cancellationToken.ThrowIfCancellationRequested();

        if (IsSuccessStatusCode2xx(responseMessage.StatusCode))
        {
            // Validate content type for JSON variants
            ContentTypeInfo contentTypeInfo = responseMessage.GetContentTypeInfo();
            if (!contentTypeInfo.IsJson() && !string.IsNullOrEmpty(contentTypeInfo.MediaType))
            {
                return Result.Failure<T?>(CreateContentTypeMismatchError(contentTypeInfo.MediaType, "JSON"));
            }

            // Check for missing content type header
            if (string.IsNullOrEmpty(contentTypeInfo.MediaType))
            {
                return Result.Failure<T?>("Missing content type header");
            }

            // Validate charset if present
            if (!string.IsNullOrEmpty(contentTypeInfo.Charset) && !IsValidCharset(contentTypeInfo.Charset))
            {
                return Result.Failure<T?>(CreateUnsupportedCharsetError(contentTypeInfo.Charset));
            }

            await using Stream stream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                T? value = await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken)
                    .ConfigureAwait(false);

                return Result.SuccessOrNull(value);
            }
            catch (JsonException)
            {
                // Handle cases where JSON is invalid or empty - return null for these scenarios
                return Result.SuccessOrNull<T>(default(T));
            }
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
        cancellationToken.ThrowIfCancellationRequested();

        if (IsSuccessStatusCode2xx(responseMessage.StatusCode))
        {
            await using Stream stream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                T? value = await JsonSerializer.DeserializeAsync(stream, jsonTypeInfo, cancellationToken)
                    .ConfigureAwait(false);

                return Result.SuccessOrNull(value);
            }
            catch (JsonException)
            {
                // Handle cases where JSON is invalid or empty - return null for these scenarios
                return Result.SuccessOrNull<T>(default(T));
            }
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
    /// Converts an HTTP response message to a Result&lt;T&gt; by deserializing the XML content.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the XML content to.</typeparam>
    /// <param name="responseMessage">The HTTP response message to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A Result&lt;T&gt; containing the deserialized value on success, or failure information on error.</returns>
    /// <example>
    /// <code>
    /// HttpResponseMessage response = await client.GetAsync("/api/users/1");
    /// Result&lt;User?&gt; userResult = await response.ToResultFromXmlAsync&lt;User&gt;();
    /// if (userResult.IsSuccess)
    /// {
    ///     User? user = userResult.Value;
    /// }
    /// </code>
    /// </example>
    public static async Task<Result<T?>> ToResultFromXmlAsync<T>(this HttpResponseMessage responseMessage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);
        cancellationToken.ThrowIfCancellationRequested();

        if (IsSuccessStatusCode2xx(responseMessage.StatusCode))
        {
            // Validate content type
            if (!responseMessage.IsXmlContentType())
            {
                ContentTypeInfo contentTypeInfo = responseMessage.GetContentTypeInfo();
                return Result.Failure<T?>(CreateContentTypeMismatchError(contentTypeInfo.MediaType, "XML"));
            }

            string xmlContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(xmlContent))
            {
                return Result.SuccessOrNull<T>(default(T));
            }

            try
            {
                XmlSerializer serializer = new(typeof(T));
                using StringReader stringReader = new(xmlContent);
                using XmlReader xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null
                });
                T? value = (T?)serializer.Deserialize(xmlReader);
                return Result.SuccessOrNull(value);
            }
            catch (XmlException)
            {
                return Result.Failure<T?>("Invalid XML content");
            }
            catch (InvalidOperationException)
            {
                return Result.Failure<T?>("Invalid XML content");
            }
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
    /// Converts an HTTP response message to a Result&lt;string&gt; by reading the content as text.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A Result&lt;string&gt; containing the text content on success, or failure information on error.</returns>
    /// <example>
    /// <code>
    /// HttpResponseMessage response = await client.GetAsync("/api/text");
    /// Result&lt;string?&gt; textResult = await response.ToResultAsTextAsync();
    /// if (textResult.IsSuccess)
    /// {
    ///     string? text = textResult.Value;
    /// }
    /// </code>
    /// </example>
    public static async Task<Result<string?>> ToResultAsTextAsync(this HttpResponseMessage responseMessage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);
        cancellationToken.ThrowIfCancellationRequested();

        if (IsSuccessStatusCode2xx(responseMessage.StatusCode))
        {
            ContentTypeInfo contentTypeInfo = responseMessage.GetContentTypeInfo();
            
            // Check if it's a binary content type
            if (contentTypeInfo.IsBinary())
            {
                return Result.Failure<string?>(CreateBinaryContentError(contentTypeInfo.MediaType));
            }

            string content = await responseMessage.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            return Result.Success<string?>(content);
        }

        return responseMessage.StatusCode switch
        {
            HttpStatusCode.BadRequest => await responseMessage.HandleBadRequestAsync<string?>(cancellationToken).ConfigureAwait(false),
            HttpStatusCode.Unauthorized or
            HttpStatusCode.Forbidden => Result.Failure<string?>(new SecurityException("Unauthorized")),
            HttpStatusCode.NotFound => Result.Failure<string?>("Not Found"),
            HttpStatusCode.InternalServerError => Result.Failure<string?>("Internal Server Error"),
            _ => Result.Failure<string?>(await GetUnexpectedStatusCodeFailureAsync(responseMessage, cancellationToken).ConfigureAwait(false))
        };
    }

    /// <summary>
    /// Converts an HTTP response message to a Result&lt;byte[]&gt; by reading the content as bytes.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A Result&lt;byte[]&gt; containing the byte content on success, or failure information on error.</returns>
    /// <example>
    /// <code>
    /// HttpResponseMessage response = await client.GetAsync("/api/file");
    /// Result&lt;byte[]?&gt; bytesResult = await response.ToResultAsBytesAsync();
    /// if (bytesResult.IsSuccess)
    /// {
    ///     byte[]? bytes = bytesResult.Value;
    /// }
    /// </code>
    /// </example>
    public static async Task<Result<byte[]?>> ToResultAsBytesAsync(this HttpResponseMessage responseMessage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);
        cancellationToken.ThrowIfCancellationRequested();

        if (IsSuccessStatusCode2xx(responseMessage.StatusCode))
        {
            ContentTypeInfo contentTypeInfo = responseMessage.GetContentTypeInfo();
            
            // Only allow binary content types or common formats
            if (!contentTypeInfo.IsBinary() && !contentTypeInfo.IsJson() && !contentTypeInfo.IsXml() && !contentTypeInfo.IsText())
            {
                return Result.Failure<byte[]?>(CreateUnsupportedContentTypeError(contentTypeInfo.MediaType));
            }

            byte[] content = await responseMessage.Content.ReadAsByteArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            return Result.Success<byte[]?>(content);
        }

        return responseMessage.StatusCode switch
        {
            HttpStatusCode.BadRequest => await responseMessage.HandleBadRequestAsync<byte[]?>(cancellationToken).ConfigureAwait(false),
            HttpStatusCode.Unauthorized or
            HttpStatusCode.Forbidden => Result.Failure<byte[]?>(new SecurityException("Unauthorized")),
            HttpStatusCode.NotFound => Result.Failure<byte[]?>("Not Found"),
            HttpStatusCode.InternalServerError => Result.Failure<byte[]?>("Internal Server Error"),
            _ => Result.Failure<byte[]?>(await GetUnexpectedStatusCodeFailureAsync(responseMessage, cancellationToken).ConfigureAwait(false))
        };
    }

    /// <summary>
    /// Converts an HTTP response message to a Result&lt;Dictionary&lt;string, string&gt;&gt; by parsing form data content.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A Result&lt;Dictionary&lt;string, string&gt;&gt; containing the parsed form data on success, or failure information on error.</returns>
    /// <example>
    /// <code>
    /// HttpResponseMessage response = await client.PostAsync("/api/form", formContent);
    /// Result&lt;Dictionary&lt;string, string&gt;?&gt; formResult = await response.ToResultAsFormDataAsync();
    /// if (formResult.IsSuccess)
    /// {
    ///     Dictionary&lt;string, string&gt;? formData = formResult.Value;
    /// }
    /// </code>
    /// </example>
    public static async Task<Result<Dictionary<string, string>?>> ToResultAsFormDataAsync(this HttpResponseMessage responseMessage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);
        cancellationToken.ThrowIfCancellationRequested();

        if (IsSuccessStatusCode2xx(responseMessage.StatusCode))
        {
            ContentTypeInfo contentTypeInfo = responseMessage.GetContentTypeInfo();
            
            if (!contentTypeInfo.IsFormData())
            {
                return Result.Failure<Dictionary<string, string>?>(CreateContentTypeMismatchError(contentTypeInfo.MediaType, "form data"));
            }

            string content = await responseMessage.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(content))
            {
                return Result.Success<Dictionary<string, string>?>(new Dictionary<string, string>());
            }

            try
            {
                // Pre-validate form data format - each part should contain '=' or be empty
                string[] pairs = content.Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (string pair in pairs)
                {
                    if (!string.IsNullOrEmpty(pair) && !pair.Contains('='))
                    {
                        return Result.Failure<Dictionary<string, string>?>("Invalid form data format: missing '=' in key-value pair");
                    }
                }
                
                NameValueCollection parsed = HttpUtility.ParseQueryString(content);
                Dictionary<string, string> result = [];
                
                foreach (string? key in parsed.AllKeys)
                {
                    if (key is not null)
                    {
                        string? value = parsed[key];
                        result[key] = value ?? string.Empty;
                    }
                }

                return Result.Success<Dictionary<string, string>?>(result);
            }
            catch (Exception)
            {
                return Result.Failure<Dictionary<string, string>?>("Invalid form data format");
            }
        }

        return responseMessage.StatusCode switch
        {
            HttpStatusCode.BadRequest => await responseMessage.HandleBadRequestAsync<Dictionary<string, string>?>(cancellationToken).ConfigureAwait(false),
            HttpStatusCode.Unauthorized or
            HttpStatusCode.Forbidden => Result.Failure<Dictionary<string, string>?>(new SecurityException("Unauthorized")),
            HttpStatusCode.NotFound => Result.Failure<Dictionary<string, string>?>("Not Found"),
            HttpStatusCode.InternalServerError => Result.Failure<Dictionary<string, string>?>("Internal Server Error"),
            _ => Result.Failure<Dictionary<string, string>?>(await GetUnexpectedStatusCodeFailureAsync(responseMessage, cancellationToken).ConfigureAwait(false))
        };
    }

    /// <summary>
    /// Converts an HTTP response message to a Result&lt;T&gt; with content type validation.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the content to.</typeparam>
    /// <param name="responseMessage">The HTTP response message to convert.</param>
    /// <param name="expectedContentType">The expected content type.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A Result&lt;T&gt; containing the deserialized value on success, or failure information on error.</returns>
    /// <example>
    /// <code>
    /// HttpResponseMessage response = await client.GetAsync("/api/users/1");
    /// Result&lt;User?&gt; userResult = await response.ToResultWithContentTypeValidation&lt;User&gt;("application/json");
    /// if (userResult.IsSuccess)
    /// {
    ///     User? user = userResult.Value;
    /// }
    /// </code>
    /// </example>
    public static async Task<Result<T?>> ToResultWithContentTypeValidation<T>(this HttpResponseMessage responseMessage, string expectedContentType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);
        ArgumentNullException.ThrowIfNull(expectedContentType);
        cancellationToken.ThrowIfCancellationRequested();

        if (IsSuccessStatusCode2xx(responseMessage.StatusCode))
        {
            if (!responseMessage.IsContentType(expectedContentType))
            {
                ContentTypeInfo contentTypeInfo = responseMessage.GetContentTypeInfo();
                return Result.Failure<T?>(CreateExpectedContentTypeError(expectedContentType, contentTypeInfo.MediaType));
            }

            // Delegate to appropriate method based on content type
            ContentTypeInfo info = responseMessage.GetContentTypeInfo();
            if (info.IsJson())
            {
                return await responseMessage.ToResultFromJsonAsync<T>((JsonSerializerOptions?)null, cancellationToken).ConfigureAwait(false);
            }
            if (info.IsXml())
            {
                return await responseMessage.ToResultFromXmlAsync<T>(cancellationToken).ConfigureAwait(false);
            }

            return Result.Failure<T?>(CreateUnsupportedContentTypeError(info.MediaType));
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
    /// Gets detailed content type information from the HTTP response message.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message.</param>
    /// <returns>A ContentTypeInfo object containing parsed content type details.</returns>
    public static ContentTypeInfo GetContentTypeInfo(this HttpResponseMessage responseMessage)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);
        
        System.Net.Http.Headers.MediaTypeHeaderValue? mediaTypeHeader = responseMessage.Content.Headers.ContentType;
        if (mediaTypeHeader is null)
        {
            return new ContentTypeInfo();
        }
        
        // Convert MediaTypeHeaderValue to ContentType
        try
        {
            ContentType contentType = new(mediaTypeHeader.MediaType ?? string.Empty);
            if (!string.IsNullOrEmpty(mediaTypeHeader.CharSet))
            {
                contentType.CharSet = mediaTypeHeader.CharSet;
            }
            
            // Copy parameters
            foreach (System.Net.Http.Headers.NameValueHeaderValue parameter in mediaTypeHeader.Parameters)
            {
                if (parameter.Name is not null && parameter.Value is not null)
                {
                    contentType.Parameters[parameter.Name] = parameter.Value.Trim('"');
                }
            }
            
            return ContentTypeInfo.FromContentType(contentType);
        }
        catch
        {
            return new ContentTypeInfo();
        }
    }

    /// <summary>
    /// Determines if the response content type matches the specified media type.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message.</param>
    /// <param name="expectedMediaType">The expected media type to match.</param>
    /// <returns>True if the content types match (ignoring parameters like charset).</returns>
    public static bool IsContentType(this HttpResponseMessage responseMessage, string expectedMediaType)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);
        ArgumentNullException.ThrowIfNull(expectedMediaType);
        
        ContentTypeInfo contentTypeInfo = responseMessage.GetContentTypeInfo();
        return contentTypeInfo.IsMediaType(expectedMediaType);
    }

    /// <summary>
    /// Determines if the response content type is a JSON variant.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message.</param>
    /// <returns>True if the content type represents JSON content.</returns>
    public static bool IsJsonContentType(this HttpResponseMessage responseMessage)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);
        return responseMessage.GetContentTypeInfo().IsJson();
    }

    /// <summary>
    /// Determines if the response content type is an XML variant.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message.</param>
    /// <returns>True if the content type represents XML content.</returns>
    public static bool IsXmlContentType(this HttpResponseMessage responseMessage)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);
        return responseMessage.GetContentTypeInfo().IsXml();
    }

    /// <summary>
    /// Determines if the response content type represents text content.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message.</param>
    /// <returns>True if the content type represents text content.</returns>
    public static bool IsTextContentType(this HttpResponseMessage responseMessage)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);
        return responseMessage.GetContentTypeInfo().IsText();
    }

    /// <summary>
    /// Determines if the response content type represents binary content.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message.</param>
    /// <returns>True if the content type represents binary content.</returns>
    public static bool IsBinaryContentType(this HttpResponseMessage responseMessage)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);
        return responseMessage.GetContentTypeInfo().IsBinary();
    }

    /// <summary>
    /// Validates that the response content type is one of the supported types.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message.</param>
    /// <param name="supportedTypes">Array of supported content types.</param>
    /// <returns>A Result indicating whether the content type is supported.</returns>
    public static Result ValidateContentType(this HttpResponseMessage responseMessage, string[] supportedTypes)
    {
        ArgumentNullException.ThrowIfNull(responseMessage);
        ArgumentNullException.ThrowIfNull(supportedTypes);

        ContentTypeInfo contentTypeInfo = responseMessage.GetContentTypeInfo();
        
        foreach (string supportedType in supportedTypes)
        {
            if (contentTypeInfo.IsMediaType(supportedType))
            {
                return Result.Success();
            }
        }

        string supportedTypesString = string.Join(", ", supportedTypes);
        return Result.Failure($"Unsupported content type '{contentTypeInfo.MediaType}'. Supported types: {supportedTypesString}");
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

    private static string CreateContentTypeMismatchError(string actualContentType, string expectedContentType) =>
        $"Content type mismatch. Expected {expectedContentType} but received '{actualContentType}'";

    private static string CreateBinaryContentError(string contentType) =>
        $"Binary content cannot be read as text. Content type: {contentType}";

    private static string CreateUnsupportedContentTypeError(string contentType) =>
        $"Unsupported content type '{contentType}'";

    private static string CreateExpectedContentTypeError(string expectedContentType, string actualContentType) =>
        $"Expected content type '{expectedContentType}' but received '{actualContentType}'";

    private static string CreateUnsupportedCharsetError(string charset) =>
        $"Unsupported charset '{charset}'";

    private static readonly HashSet<string> ValidCharsets = new(StringComparer.OrdinalIgnoreCase)
    {
        "utf-8",
        "utf-16",
        "utf-32",
        "ascii",
        "iso-8859-1",
        "windows-1252"
    };

    private static bool IsValidCharset(string charset) =>
        ValidCharsets.Contains(charset);

    /// <summary>
    /// Determines if the HTTP status code represents a successful 2xx response with explicit mapping.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is in the 2xx range (200-299), otherwise false.</returns>
    /// <remarks>
    /// <para>
    /// This method provides explicit mapping for all standard HTTP 2xx status codes instead of relying
    /// on the generic <see cref="HttpResponseMessage.IsSuccessStatusCode"/> property. This ensures
    /// predictable behavior and supports custom 2xx status codes that may not be in the 
    /// <see cref="HttpStatusCode"/> enumeration.
    /// </para>
    /// <para>
    /// The following standard 2xx status codes are explicitly supported:
    /// <list type="bullet">
    /// <item><description>200 OK - Standard success response</description></item>
    /// <item><description>201 Created - Resource successfully created</description></item>
    /// <item><description>202 Accepted - Request accepted for processing</description></item>
    /// <item><description>203 Non-Authoritative Information - Success with modified response</description></item>
    /// <item><description>204 No Content - Success with no response body</description></item>
    /// <item><description>205 Reset Content - Success requiring client reset</description></item>
    /// <item><description>206 Partial Content - Partial resource delivered</description></item>
    /// <item><description>207 Multi-Status - WebDAV multiple status responses</description></item>
    /// <item><description>208 Already Reported - WebDAV resource already enumerated</description></item>
    /// <item><description>226 IM Used - HTTP Delta encoding successful</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Additionally, any custom status codes in the 200-299 range are supported to handle
    /// future HTTP standards or proprietary extensions.
    /// </para>
    /// </remarks>
    private static bool IsSuccessStatusCode2xx(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            // Standard 2xx status codes
            HttpStatusCode.OK => true,                              // 200
            HttpStatusCode.Created => true,                         // 201
            HttpStatusCode.Accepted => true,                        // 202
            HttpStatusCode.NonAuthoritativeInformation => true,     // 203
            HttpStatusCode.NoContent => true,                       // 204
            HttpStatusCode.ResetContent => true,                    // 205
            HttpStatusCode.PartialContent => true,                  // 206
            HttpStatusCode.MultiStatus => true,                     // 207
            HttpStatusCode.AlreadyReported => true,                 // 208
            HttpStatusCode.IMUsed => true,                          // 226
            
            // Handle any other status codes in the 2xx range (custom or future standards)
            _ => (int)statusCode >= 200 && (int)statusCode <= 299
        };
    }


    #endregion Private Methods
}