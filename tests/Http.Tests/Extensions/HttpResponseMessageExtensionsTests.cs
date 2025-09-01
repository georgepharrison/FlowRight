using FlowRight.Core.Results;
using FlowRight.Http.Extensions;
using FlowRight.Http.Models;
using Shouldly;
using System.Net;
using System.Net.Mime;
using System.Security;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FlowRight.Http.Tests.Extensions;

/// <summary>
/// Tests for HttpResponseMessageExtensions class.
/// </summary>
public sealed class HttpResponseMessageExtensionsTests
{
    /// <summary>
    /// Helper method to create HttpResponseMessage with content type that may contain parameters.
    /// </summary>
    private static HttpResponseMessage CreateResponseWithContent(HttpStatusCode statusCode, string content, Encoding encoding, string contentType)
    {
        HttpResponseMessage response = new(statusCode);
        
        // Handle content types with parameters
        if (contentType.Contains(';'))
        {
            string[] parts = contentType.Split(';', 2);
            string mediaType = parts[0].Trim();
            response.Content = new StringContent(content, encoding, mediaType);
            
            // Parse and set parameters
            if (parts.Length > 1)
            {
                string[] parameters = parts[1].Split(';');
                foreach (string param in parameters)
                {
                    string[] keyValue = param.Trim().Split('=', 2);
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0].Trim();
                        string value = keyValue[1].Trim();
                        if (key == "charset")
                        {
                            response.Content.Headers.ContentType!.CharSet = value;
                        }
                        else
                        {
                            response.Content.Headers.ContentType!.Parameters.Add(new System.Net.Http.Headers.NameValueHeaderValue(key, value));
                        }
                    }
                }
            }
        }
        else
        {
            response.Content = new StringContent(content, encoding, contentType);
        }
        
        return response;
    }
    #region ToResultAsync Tests

    [Fact]
    public async Task ToResultAsync_WithSuccessStatusCode_ReturnsSuccessResult()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.OK);

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ToResultAsync_WithCreatedStatusCode_ReturnsSuccessResult()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.Created);

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ToResultAsync_WithBadRequest_ReturnsFailureResult()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad request error", Encoding.UTF8, MediaTypeNames.Text.Plain)
        };

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Bad request error");
    }

    [Fact]
    public async Task ToResultAsync_WithBadRequestAndValidationProblem_ReturnsFailureResultWithErrors()
    {
        // Arrange
        ValidationProblemResponse problemResponse = new()
        {
            Errors = new Dictionary<string, string[]>
            {
                ["Name"] = ["Name is required", "Name must be at least 2 characters"],
                ["Email"] = ["Invalid email format"]
            }
        };

        string jsonContent = JsonSerializer.Serialize(problemResponse, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

        using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
        };

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Failures.ShouldNotBeNull();
        result.Failures.Count.ShouldBe(2);
        result.Failures["Name"].ShouldBe(["Name is required", "Name must be at least 2 characters"]);
        result.Failures["Email"].ShouldBe(["Invalid email format"]);
    }

    [Fact]
    public async Task ToResultAsync_WithUnauthorized_ReturnsFailureResultWithSecurityException()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.Unauthorized);

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.FailureType.ShouldBe(ResultFailureType.Security);
        result.Error.ShouldBe("Unauthorized");
    }

    [Fact]
    public async Task ToResultAsync_WithForbidden_ReturnsFailureResultWithSecurityException()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.Forbidden);

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.FailureType.ShouldBe(ResultFailureType.Security);
        result.Error.ShouldBe("Unauthorized");
    }

    [Fact]
    public async Task ToResultAsync_WithNotFound_ReturnsFailureResult()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.NotFound);

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Not Found");
    }

    [Fact]
    public async Task ToResultAsync_WithInternalServerError_ReturnsFailureResult()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.InternalServerError);

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Internal Server Error");
    }

    [Fact]
    public async Task ToResultAsync_WithUnexpectedStatusCode_ReturnsFailureResultWithStatusAndBody()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent("Service temporarily unavailable", Encoding.UTF8, MediaTypeNames.Text.Plain)
        };

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Unexpected ServiceUnavailable: Service temporarily unavailable");
    }

    [Fact]
    public async Task ToResultAsync_WithNullResponseMessage_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await ((HttpResponseMessage)null!).ToResultAsync());
    }

    [Fact]
    public async Task ToResultAsync_WithCancellationToken_ProperlyCancels()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.OK);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await response.ToResultAsync(cts.Token));
    }

    #endregion ToResultAsync Tests

    #region ToResultFromJsonAsync Tests

    [Fact]
    public async Task ToResultFromJsonAsync_WithSuccessStatusCodeAndValidJson_ReturnsSuccessResultWithValue()
    {
        // Arrange
        TestModel expected = new() { Id = 1, Name = "Test" };
        string jsonContent = JsonSerializer.Serialize(expected);

        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out TestModel? value).ShouldBeTrue();
        value.ShouldNotBeNull();
        value.Id.ShouldBe(expected.Id);
        value.Name.ShouldBe(expected.Name);
    }

    [Fact]
    public async Task ToResultFromJsonAsync_WithSuccessStatusCodeAndEmptyJson_ReturnsSuccessResultWithNull()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out TestModel? value).ShouldBeTrue();
        value.ShouldBeNull();
    }

    [Fact]
    public async Task ToResultFromJsonAsync_WithBadRequestAndValidationProblem_ReturnsFailureResultWithErrors()
    {
        // Arrange
        ValidationProblemResponse problemResponse = new()
        {
            Errors = new Dictionary<string, string[]>
            {
                ["Name"] = ["Name is required"],
                ["Email"] = ["Invalid email format"]
            }
        };

        string jsonContent = JsonSerializer.Serialize(problemResponse, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

        using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
        };

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Failures.ShouldNotBeNull();
        result.Failures.Count.ShouldBe(2);
        result.Failures["Name"].ShouldBe(["Name is required"]);
        result.Failures["Email"].ShouldBe(["Invalid email format"]);
    }

    [Fact]
    public async Task ToResultFromJsonAsync_WithUnauthorized_ReturnsFailureResultWithSecurityException()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.Unauthorized);

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.FailureType.ShouldBe(ResultFailureType.Security);
        result.Error.ShouldBe("Unauthorized");
    }

    [Fact]
    public async Task ToResultFromJsonAsync_WithNotFound_ReturnsFailureResult()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.NotFound);

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Not Found");
    }

    [Fact]
    public async Task ToResultFromJsonAsync_WithNullResponseMessage_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await ((HttpResponseMessage)null!).ToResultFromJsonAsync<TestModel>());
    }

    #endregion ToResultFromJsonAsync Tests

    #region ToResultFromJsonAsync with JsonTypeInfo Tests

    [Fact]
    public async Task ToResultFromJsonAsync_WithJsonTypeInfo_ReturnsSuccessResultWithValue()
    {
        // Arrange
        TestModel expected = new() { Id = 1, Name = "Test" };
        string jsonContent = JsonSerializer.Serialize(expected, TestModelJsonContext.Default.TestModel);

        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync(TestModelJsonContext.Default.TestModel);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out TestModel? value).ShouldBeTrue();
        value.ShouldNotBeNull();
        value.Id.ShouldBe(expected.Id);
        value.Name.ShouldBe(expected.Name);
    }

    [Fact]
    public async Task ToResultFromJsonAsync_WithJsonTypeInfoAndBadRequest_ReturnsFailureResultWithErrors()
    {
        // Arrange
        ValidationProblemResponse problemResponse = new()
        {
            Errors = new Dictionary<string, string[]>
            {
                ["Name"] = ["Name is required"]
            }
        };

        string jsonContent = JsonSerializer.Serialize(problemResponse, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

        using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
        };

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync(TestModelJsonContext.Default.TestModel);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Failures.ShouldNotBeNull();
        result.Failures["Name"].ShouldBe(["Name is required"]);
    }

    [Fact]
    public async Task ToResultFromJsonAsync_WithJsonTypeInfoAndNullResponseMessage_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await ((HttpResponseMessage)null!).ToResultFromJsonAsync(TestModelJsonContext.Default.TestModel));
    }

    #endregion ToResultFromJsonAsync with JsonTypeInfo Tests

    #region TASK-043: Null Handling Bug Tests

    public class ToResultFromJsonAsyncNullHandling
    {
        [Fact]
        public async Task ToResultFromJsonAsync_WithSuccessStatusCodeAndNullJson_ShouldReturnSuccessWithNullValue()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldBeNull();
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithSuccessStatusCodeAndEmptyJsonObject_ShouldReturnSuccessWithNullValue()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<string?> result = await response.ToResultFromJsonAsync<string>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out string? value).ShouldBeTrue();
            value.ShouldBeNull();
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithSuccessStatusCodeAndEmptyString_ShouldReturnSuccessWithNullValue()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("\"\"", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<string?> result = await response.ToResultFromJsonAsync<string>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out string? value).ShouldBeTrue();
            value.ShouldBe(string.Empty);
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithSuccessStatusCodeAndNullableIntNull_ShouldReturnSuccessWithNullValue()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<int?> result = await response.ToResultFromJsonAsync<int?>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out int? value).ShouldBeTrue();
            value.ShouldBeNull();
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithSuccessStatusCodeAndNullableStringNull_ShouldReturnSuccessWithNullValue()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<string?> result = await response.ToResultFromJsonAsync<string?>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out string? value).ShouldBeTrue();
            value.ShouldBeNull();
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithCustomJsonSerializerOptions_AndNullJson_ShouldReturnSuccessWithNullValue()
        {
            // Arrange
            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>(options);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldBeNull();
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithCancellationToken_AndNullJson_ShouldReturnSuccessWithNullValue()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            using CancellationTokenSource cts = new();

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>(cancellationToken: cts.Token);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldBeNull();
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithJsonTypeInfo_AndNullJson_ShouldReturnSuccessWithNullValue()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync(TestModelJsonContext.Default.TestModel);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldBeNull();
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithJsonTypeInfo_AndCancellationToken_AndNullJson_ShouldReturnSuccessWithNullValue()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            using CancellationTokenSource cts = new();

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync(TestModelJsonContext.Default.TestModel, cts.Token);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldBeNull();
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NoContent)]
        public async Task ToResultFromJsonAsync_WithSuccessStatusCodes_AndNullJson_ShouldReturnSuccessWithNullValue(HttpStatusCode statusCode)
        {
            // Arrange
            using HttpResponseMessage response = new(statusCode)
            {
                Content = new StringContent("null", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldBeNull();
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithArrayOfNulls_ShouldReturnSuccessWithArrayContainingNulls()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("[null, null, null]", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<TestModel?[]?> result = await response.ToResultFromJsonAsync<TestModel?[]>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out TestModel?[]? value).ShouldBeTrue();
            value.ShouldNotBeNull();
            value.Length.ShouldBe(3);
            value[0].ShouldBeNull();
            value[1].ShouldBeNull();
            value[2].ShouldBeNull();
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithEmptyJsonContent_ShouldReturnSuccessWithNullValue()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldBeNull();
        }

        [Fact]
        public async Task ToResultFromJsonAsync_ShouldNeverThrowArgumentNullExceptionForNullValues()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act & Assert - should not throw any exception
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

            // Verify it returns success with null value instead of throwing
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldBeNull();
        }
    }

    #endregion TASK-043: Null Handling Bug Tests

    #region TASK-045: Content Type Handling Tests

    public class ContentTypeDetection
    {
        [Theory]
        [InlineData("application/json")]
        [InlineData("application/json; charset=utf-8")]
        [InlineData("application/json-patch+json")]
        [InlineData("application/vnd.api+json")]
        [InlineData("application/ld+json")]
        [InlineData("application/hal+json")]
        public async Task ToResultFromJsonAsync_WithJsonContentTypeVariants_ShouldDeserializeSuccessfully(string contentType)
        {
            // Arrange
            TestModel expected = new() { Id = 1, Name = "Test" };
            string jsonContent = JsonSerializer.Serialize(expected);

            using HttpResponseMessage response = CreateResponseWithContent(HttpStatusCode.OK, jsonContent, Encoding.UTF8, contentType);

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldNotBeNull();
            value.Id.ShouldBe(expected.Id);
            value.Name.ShouldBe(expected.Name);
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("text/xml")]
        [InlineData("application/xml; charset=utf-8")]
        public async Task ToResultFromXmlAsync_WithXmlContentTypes_ShouldDeserializeSuccessfully(string contentType)
        {
            // Arrange
            string xmlContent = "<TestModel><Id>1</Id><Name>Test</Name></TestModel>";

            using HttpResponseMessage response = CreateResponseWithContent(HttpStatusCode.OK, xmlContent, Encoding.UTF8, contentType);

            // Act
            Result<TestModel?> result = await response.ToResultFromXmlAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldNotBeNull();
            value.Id.ShouldBe(1);
            value.Name.ShouldBe("Test");
        }

        [Theory]
        [InlineData("text/plain")]
        [InlineData("text/html")]
        [InlineData("text/csv")]
        [InlineData("text/markdown")]
        [InlineData("text/plain; charset=utf-8")]
        public async Task ToResultAsTextAsync_WithTextContentTypes_ShouldReturnStringContent(string contentType)
        {
            // Arrange
            string textContent = "This is test content";

            using HttpResponseMessage response = CreateResponseWithContent(HttpStatusCode.OK, textContent, Encoding.UTF8, contentType);

            // Act
            Result<string?> result = await response.ToResultAsTextAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out string? value).ShouldBeTrue();
            value.ShouldBe(textContent);
        }

        [Theory]
        [InlineData("application/octet-stream")]
        [InlineData("image/jpeg")]
        [InlineData("image/png")]
        [InlineData("application/pdf")]
        [InlineData("video/mp4")]
        public async Task ToResultAsBytesAsync_WithBinaryContentTypes_ShouldReturnByteArray(string contentType)
        {
            // Arrange
            byte[] binaryContent = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]; // PNG header

            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(binaryContent)
            };
            response.Content.Headers.ContentType = new(contentType);

            // Act
            Result<byte[]?> result = await response.ToResultAsBytesAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out byte[]? value).ShouldBeTrue();
            value.ShouldBe(binaryContent);
        }

        [Theory]
        [InlineData("application/x-www-form-urlencoded")]
        [InlineData("multipart/form-data")]
        public async Task ToResultAsFormDataAsync_WithFormContentTypes_ShouldReturnFormData(string contentType)
        {
            // Arrange
            string formContent = "name=test&id=1";

            using HttpResponseMessage response = CreateResponseWithContent(HttpStatusCode.OK, formContent, Encoding.UTF8, contentType);

            // Act
            Result<Dictionary<string, string>?> result = await response.ToResultAsFormDataAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Dictionary<string, string>? value).ShouldBeTrue();
            value.ShouldNotBeNull();
            value["name"].ShouldBe("test");
            value["id"].ShouldBe("1");
        }
    }

    public class ContentTypeMismatchHandling
    {
        [Fact]
        public async Task ToResultFromJsonAsync_WithNonJsonContentType_ShouldReturnFailureResult()
        {
            // Arrange
            string xmlContent = "<test>data</test>";

            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml")
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldContain("Content type mismatch");
            result.Error.ShouldContain("application/xml");
            result.Error.ShouldContain("JSON");
        }

        [Fact]
        public async Task ToResultFromXmlAsync_WithNonXmlContentType_ShouldReturnFailureResult()
        {
            // Arrange
            string jsonContent = "{\"id\": 1}";

            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromXmlAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldContain("Content type mismatch");
            result.Error.ShouldContain("application/json");
            result.Error.ShouldContain("XML");
        }

        [Fact]
        public async Task ToResultAsTextAsync_WithBinaryContentType_ShouldReturnFailureResult()
        {
            // Arrange
            byte[] binaryContent = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(binaryContent)
            };
            response.Content.Headers.ContentType = new("image/png");

            // Act
            Result<string?> result = await response.ToResultAsTextAsync();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldContain("Binary content cannot be read as text");
            result.Error.ShouldContain("image/png");
        }

        [Fact]
        public async Task ToResultAsBytesAsync_WithUnsupportedContentType_ShouldReturnFailureResult()
        {
            // Arrange
            string textContent = "This is text";

            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(textContent, Encoding.UTF8, "application/unsupported")
            };

            // Act
            Result<byte[]?> result = await response.ToResultAsBytesAsync();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldContain("Unsupported content type");
            result.Error.ShouldContain("application/unsupported");
        }
    }

    public class CharsetEncodingHandling
    {
        [Theory]
        [InlineData("utf-8")]
        [InlineData("utf-16")]
        [InlineData("iso-8859-1")]
        public async Task ToResultAsTextAsync_WithDifferentCharsets_ShouldHandleEncodingCorrectly(string charset)
        {
            // Arrange
            string textContent = "Special characters: äöü";
            Encoding encoding = charset switch
            {
                "utf-16" => Encoding.Unicode,
                "iso-8859-1" => Encoding.Latin1,
                _ => Encoding.UTF8
            };

            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(textContent, encoding, "text/plain")
            };
            
            // Manually set the content type to include charset parameter
            response.Content.Headers.ContentType!.CharSet = charset;

            // Act
            Result<string?> result = await response.ToResultAsTextAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out string? value).ShouldBeTrue();
            value.ShouldBe(textContent);
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithUnsupportedCharset_ShouldReturnFailureResult()
        {
            // Arrange
            string jsonContent = "{\"id\": 1}";

            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };
            
            // Manually set an unsupported charset
            response.Content.Headers.ContentType!.CharSet = "unsupported-charset";

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldContain("Unsupported charset");
            result.Error.ShouldContain("unsupported-charset");
        }
    }

    public class ContentTypeAwareExtensions
    {
        [Fact]
        public async Task ToResultWithContentTypeValidation_WithMatchingContentType_ShouldReturnSuccess()
        {
            // Arrange
            TestModel expected = new() { Id = 1, Name = "Test" };
            string jsonContent = JsonSerializer.Serialize(expected);

            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            // Act
            Result<TestModel?> result = await response.ToResultWithContentTypeValidation<TestModel>("application/json");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldNotBeNull();
            value.Id.ShouldBe(expected.Id);
            value.Name.ShouldBe(expected.Name);
        }

        [Fact]
        public async Task ToResultWithContentTypeValidation_WithNonMatchingContentType_ShouldReturnFailure()
        {
            // Arrange
            string xmlContent = "<test>data</test>";

            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml")
            };

            // Act
            Result<TestModel?> result = await response.ToResultWithContentTypeValidation<TestModel>("application/json");

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldContain("Expected content type 'application/json' but received 'application/xml'");
        }

        [Theory]
        [InlineData("application/json", "application/json", true)]
        [InlineData("application/json; charset=utf-8", "application/json", true)]
        [InlineData("application/json", "application/json; charset=utf-8", true)]
        [InlineData("application/xml", "application/json", false)]
        [InlineData("text/plain", "application/json", false)]
        public async Task IsContentType_WithVariousContentTypes_ShouldReturnCorrectMatch(string actualContentType, string expectedContentType, bool shouldMatch)
        {
            // Arrange
            using HttpResponseMessage response = CreateResponseWithContent(HttpStatusCode.OK, "test", Encoding.UTF8, actualContentType);

            // Act
            bool result = response.IsContentType(expectedContentType);

            // Assert
            result.ShouldBe(shouldMatch);
        }

        [Theory]
        [InlineData("application/json", true)]
        [InlineData("application/json-patch+json", true)]
        [InlineData("application/vnd.api+json", true)]
        [InlineData("application/xml", false)]
        [InlineData("text/plain", false)]
        public async Task IsJsonContentType_WithVariousContentTypes_ShouldIdentifyJsonCorrectly(string contentType, bool isJson)
        {
            // Arrange
            using HttpResponseMessage response = CreateResponseWithContent(HttpStatusCode.OK, "test", Encoding.UTF8, contentType);

            // Act
            bool result = response.IsJsonContentType();

            // Assert
            result.ShouldBe(isJson);
        }

        [Theory]
        [InlineData("application/xml", true)]
        [InlineData("text/xml", true)]
        [InlineData("application/soap+xml", true)]
        [InlineData("application/json", false)]
        [InlineData("text/plain", false)]
        public async Task IsXmlContentType_WithVariousContentTypes_ShouldIdentifyXmlCorrectly(string contentType, bool isXml)
        {
            // Arrange
            using HttpResponseMessage response = CreateResponseWithContent(HttpStatusCode.OK, "test", Encoding.UTF8, contentType);

            // Act
            bool result = response.IsXmlContentType();

            // Assert
            result.ShouldBe(isXml);
        }

        [Theory]
        [InlineData("text/plain", true)]
        [InlineData("text/html", true)]
        [InlineData("text/csv", true)]
        [InlineData("text/markdown", true)]
        [InlineData("application/json", false)]
        [InlineData("application/xml", false)]
        [InlineData("image/png", false)]
        public async Task IsTextContentType_WithVariousContentTypes_ShouldIdentifyTextCorrectly(string contentType, bool isText)
        {
            // Arrange
            using HttpResponseMessage response = CreateResponseWithContent(HttpStatusCode.OK, "test", Encoding.UTF8, contentType);

            // Act
            bool result = response.IsTextContentType();

            // Assert
            result.ShouldBe(isText);
        }

        [Theory]
        [InlineData("application/octet-stream", true)]
        [InlineData("image/jpeg", true)]
        [InlineData("image/png", true)]
        [InlineData("video/mp4", true)]
        [InlineData("application/pdf", true)]
        [InlineData("text/plain", false)]
        [InlineData("application/json", false)]
        [InlineData("application/xml", false)]
        public async Task IsBinaryContentType_WithVariousContentTypes_ShouldIdentifyBinaryCorrectly(string contentType, bool isBinary)
        {
            // Arrange
            using HttpResponseMessage response = CreateResponseWithContent(HttpStatusCode.OK, "test", Encoding.UTF8, contentType);

            // Act
            bool result = response.IsBinaryContentType();

            // Assert
            result.ShouldBe(isBinary);
        }
    }

    public class ContentTypeErrorHandling
    {
        [Fact]
        public async Task ToResultFromJsonAsync_WithMissingContentType_ShouldReturnFailureResult()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\": 1}", Encoding.UTF8)
            };
            response.Content.Headers.ContentType = null;

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldContain("Missing content type header");
        }

        [Fact]
        public async Task ToResultFromXmlAsync_WithInvalidXmlContent_ShouldReturnFailureResult()
        {
            // Arrange
            string invalidXml = "<invalid><unclosed>";

            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(invalidXml, Encoding.UTF8, "application/xml")
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromXmlAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldContain("Invalid XML content");
        }

        [Fact]
        public async Task ToResultAsFormDataAsync_WithInvalidFormData_ShouldReturnFailureResult()
        {
            // Arrange
            string invalidFormData = "invalid=form&data&malformed";

            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent(invalidFormData, Encoding.UTF8, "application/x-www-form-urlencoded")
            };

            // Act
            Result<Dictionary<string, string>?> result = await response.ToResultAsFormDataAsync();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldContain("Invalid form data format");
        }

        [Fact]
        public async Task GetContentTypeInfo_WithComplexContentType_ShouldParseCorrectly()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("test", Encoding.UTF8, "application/json")
            };
            
            // Manually set a more complex content type with parameters
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data");
            response.Content.Headers.ContentType.CharSet = "utf-8";
            response.Content.Headers.ContentType.Parameters.Add(new System.Net.Http.Headers.NameValueHeaderValue("boundary", "something"));

            // Act
            FlowRight.Http.Models.ContentTypeInfo contentTypeInfo = response.GetContentTypeInfo();

            // Assert
            contentTypeInfo.MediaType.ShouldBe("multipart/form-data");
            contentTypeInfo.Charset.ShouldBe("utf-8");
            contentTypeInfo.Parameters["boundary"].ShouldBe("something");
        }

        [Fact]
        public async Task ToResultWithStrictContentTypeValidation_WithUnsupportedContentType_ShouldReturnFailureResult()
        {
            // Arrange
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("test", Encoding.UTF8, "application/custom-unsupported")
            };

            string[] supportedTypes = ["application/json", "application/xml", "text/plain"];

            // Act
            Result result = response.ValidateContentType(supportedTypes);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldContain("Unsupported content type 'application/custom-unsupported'");
            result.Error.ShouldContain("Supported types: application/json, application/xml, text/plain");
        }
    }

    #endregion TASK-045: Content Type Handling Tests

    #region TASK-046: Explicit 2xx Status Code Mapping Tests

    public class Explicit2xxStatusCodeMappingTests
    {
        [Theory]
        [InlineData(HttpStatusCode.OK)]                   // 200
        [InlineData(HttpStatusCode.Created)]              // 201
        [InlineData(HttpStatusCode.Accepted)]             // 202
        [InlineData(HttpStatusCode.NonAuthoritativeInformation)] // 203
        [InlineData(HttpStatusCode.NoContent)]            // 204
        [InlineData(HttpStatusCode.ResetContent)]         // 205
        [InlineData(HttpStatusCode.PartialContent)]       // 206
        [InlineData((HttpStatusCode)207)]                 // 207 Multi-Status
        [InlineData((HttpStatusCode)208)]                 // 208 Already Reported
        [InlineData((HttpStatusCode)226)]                 // 226 IM Used
        public async Task ToResultAsync_WithSpecific2xxStatusCode_ShouldReturnSuccessResult(HttpStatusCode statusCode)
        {
            // Arrange
            using HttpResponseMessage response = new(statusCode);

            // Act
            Result result = await response.ToResultAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Error.ShouldBeNullOrEmpty();
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]                   // 200
        [InlineData(HttpStatusCode.Created)]              // 201
        [InlineData(HttpStatusCode.Accepted)]             // 202
        [InlineData(HttpStatusCode.NonAuthoritativeInformation)] // 203
        [InlineData(HttpStatusCode.NoContent)]            // 204
        [InlineData(HttpStatusCode.ResetContent)]         // 205
        [InlineData(HttpStatusCode.PartialContent)]       // 206
        [InlineData((HttpStatusCode)207)]                 // 207 Multi-Status
        [InlineData((HttpStatusCode)208)]                 // 208 Already Reported
        [InlineData((HttpStatusCode)226)]                 // 226 IM Used
        public async Task ToResultFromJsonAsync_WithSpecific2xxStatusCode_ShouldReturnSuccessResult(HttpStatusCode statusCode)
        {
            // Arrange
            TestModel expected = new() { Id = 1, Name = "Test" };
            string jsonContent = JsonSerializer.Serialize(expected);

            using HttpResponseMessage response = new(statusCode)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Error.ShouldBeNullOrEmpty();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldNotBeNull();
            value.Id.ShouldBe(expected.Id);
            value.Name.ShouldBe(expected.Name);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]                   // 200
        [InlineData(HttpStatusCode.Created)]              // 201
        [InlineData(HttpStatusCode.Accepted)]             // 202
        [InlineData(HttpStatusCode.NonAuthoritativeInformation)] // 203
        [InlineData(HttpStatusCode.NoContent)]            // 204
        [InlineData(HttpStatusCode.ResetContent)]         // 205
        [InlineData(HttpStatusCode.PartialContent)]       // 206
        [InlineData((HttpStatusCode)207)]                 // 207 Multi-Status
        [InlineData((HttpStatusCode)208)]                 // 208 Already Reported
        [InlineData((HttpStatusCode)226)]                 // 226 IM Used
        public async Task ToResultFromJsonAsync_WithSpecific2xxStatusCodeAndNullContent_ShouldReturnSuccessResultWithNull(HttpStatusCode statusCode)
        {
            // Arrange
            using HttpResponseMessage response = new(statusCode)
            {
                Content = new StringContent("null", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Error.ShouldBeNullOrEmpty();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldBeNull();
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]                   // 200
        [InlineData(HttpStatusCode.Created)]              // 201
        [InlineData(HttpStatusCode.Accepted)]             // 202
        [InlineData(HttpStatusCode.NonAuthoritativeInformation)] // 203
        [InlineData(HttpStatusCode.NoContent)]            // 204
        [InlineData(HttpStatusCode.ResetContent)]         // 205
        [InlineData(HttpStatusCode.PartialContent)]       // 206
        [InlineData((HttpStatusCode)207)]                 // 207 Multi-Status
        [InlineData((HttpStatusCode)208)]                 // 208 Already Reported
        [InlineData((HttpStatusCode)226)]                 // 226 IM Used
        public async Task ToResultFromJsonAsync_WithJsonTypeInfoAndSpecific2xxStatusCode_ShouldReturnSuccessResult(HttpStatusCode statusCode)
        {
            // Arrange
            TestModel expected = new() { Id = 42, Name = "JsonTypeInfo Test" };
            string jsonContent = JsonSerializer.Serialize(expected, TestModelJsonContext.Default.TestModel);

            using HttpResponseMessage response = new(statusCode)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync(TestModelJsonContext.Default.TestModel);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Error.ShouldBeNullOrEmpty();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldNotBeNull();
            value.Id.ShouldBe(expected.Id);
            value.Name.ShouldBe(expected.Name);
        }

        [Fact]
        public async Task ToResultAsync_WithCustom2xxStatusCode_ShouldReturnSuccessResult()
        {
            // Arrange - Using custom 2xx status code that's not in HttpStatusCode enum
            HttpStatusCode customSuccessCode = (HttpStatusCode)290; // Custom 2xx code

            using HttpResponseMessage response = new(customSuccessCode);

            // Act
            Result result = await response.ToResultAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Error.ShouldBeNullOrEmpty();
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithCustom2xxStatusCode_ShouldReturnSuccessResult()
        {
            // Arrange - Using custom 2xx status code that's not in HttpStatusCode enum
            HttpStatusCode customSuccessCode = (HttpStatusCode)299; // Custom 2xx code
            TestModel expected = new() { Id = 123, Name = "Custom Success Test" };
            string jsonContent = JsonSerializer.Serialize(expected);

            using HttpResponseMessage response = new(customSuccessCode)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Error.ShouldBeNullOrEmpty();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldNotBeNull();
            value.Id.ShouldBe(expected.Id);
            value.Name.ShouldBe(expected.Name);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NonAuthoritativeInformation)]
        [InlineData(HttpStatusCode.NoContent)]
        [InlineData(HttpStatusCode.ResetContent)]
        [InlineData(HttpStatusCode.PartialContent)]
        [InlineData((HttpStatusCode)207)]
        [InlineData((HttpStatusCode)208)]
        [InlineData((HttpStatusCode)226)]
        public async Task ToResultAsync_With2xxStatusCodeAndResponseContent_ShouldIgnoreContentForSuccess(HttpStatusCode statusCode)
        {
            // Arrange
            using HttpResponseMessage response = new(statusCode)
            {
                Content = new StringContent("Some response content", Encoding.UTF8, MediaTypeNames.Text.Plain)
            };

            // Act
            Result result = await response.ToResultAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Error.ShouldBeNullOrEmpty();
        }

        [Theory]
        [InlineData(HttpStatusCode.NoContent)]            // 204
        [InlineData(HttpStatusCode.ResetContent)]         // 205
        public async Task ToResultFromJsonAsync_WithNoContentStatusCodes_ShouldReturnSuccessResultWithNullValue(HttpStatusCode statusCode)
        {
            // Arrange - No Content status codes with null JSON content
            using HttpResponseMessage response = new(statusCode)
            {
                Content = new StringContent("null", Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Error.ShouldBeNullOrEmpty();
            result.TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldBeNull();
        }

        [Fact]
        public async Task ToResultAsync_WithAll2xxStatusCodesInRange_ShouldReturnSuccessResults()
        {
            // Arrange & Act & Assert - Test all 2xx status codes from 200-299
            for (int statusCode = 200; statusCode <= 299; statusCode++)
            {
                using HttpResponseMessage response = new((HttpStatusCode)statusCode);
                
                Result result = await response.ToResultAsync();
                
                result.IsSuccess.ShouldBeTrue($"Status code {statusCode} should return success");
                result.Error.ShouldBeNullOrEmpty($"Status code {statusCode} should have no error");
            }
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithAll2xxStatusCodesInRange_ShouldReturnSuccessResults()
        {
            // Arrange
            TestModel expected = new() { Id = 999, Name = "Range Test" };
            string jsonContent = JsonSerializer.Serialize(expected);

            // Act & Assert - Test all 2xx status codes from 200-299
            for (int statusCode = 200; statusCode <= 299; statusCode++)
            {
                // For NoContent status codes, use null JSON content
                string contentToUse = (statusCode == 204 || statusCode == 205) ? "null" : jsonContent;
                
                using HttpResponseMessage response = new((HttpStatusCode)statusCode)
                {
                    Content = new StringContent(contentToUse, Encoding.UTF8, MediaTypeNames.Application.Json)
                };
                
                Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();
                
                result.IsSuccess.ShouldBeTrue($"Status code {statusCode} should return success");
                result.Error.ShouldBeNullOrEmpty($"Status code {statusCode} should have no error");
                
                if (statusCode == 204 || statusCode == 205) // No Content status codes
                {
                    result.TryGetValue(out TestModel? value).ShouldBeTrue($"Status code {statusCode} should have value available");
                    value.ShouldBeNull($"Status code {statusCode} should have null value for No Content responses");
                }
                else
                {
                    result.TryGetValue(out TestModel? value).ShouldBeTrue($"Status code {statusCode} should have value available");
                    value.ShouldNotBeNull($"Status code {statusCode} should have non-null value");
                    value!.Id.ShouldBe(expected.Id, $"Status code {statusCode} should deserialize correctly");
                    value.Name.ShouldBe(expected.Name, $"Status code {statusCode} should deserialize correctly");
                }
            }
        }
    }

    #endregion TASK-046: Explicit 2xx Status Code Mapping Tests

    #region TASK-047: Full RFC 7807 ValidationProblemDetails Support Tests

    public class Rfc7807ValidationProblemDetailsTests
    {
        [Fact]
        public async Task ToResultAsync_WithFullRFC7807ValidationProblem_ReturnsFailureResultWithAllErrors()
        {
            // Arrange
            ValidationProblemResponse problemResponse = new()
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "One or more validation errors occurred.",
                Status = 400,
                Detail = "The request failed validation. See errors for details.",
                Instance = "/api/users/validation-error/12345",
                Errors = new Dictionary<string, string[]>
                {
                    ["FirstName"] = ["First name is required", "First name must be at least 2 characters"],
                    ["Email"] = ["Invalid email format", "Email is required"],
                    ["Age"] = ["Age must be positive"]
                }
            };

            string jsonContent = JsonSerializer.Serialize(problemResponse, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

            using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
            };

            // Act
            Result result = await response.ToResultAsync();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Failures.ShouldNotBeNull();
            result.Failures.Count.ShouldBe(3);
            result.Failures["FirstName"].ShouldBe(["First name is required", "First name must be at least 2 characters"]);
            result.Failures["Email"].ShouldBe(["Invalid email format", "Email is required"]);
            result.Failures["Age"].ShouldBe(["Age must be positive"]);
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithFullRFC7807ValidationProblem_ReturnsFailureResultWithAllErrors()
        {
            // Arrange
            ValidationProblemResponse problemResponse = new()
            {
                Type = "https://example.com/problem/validation-error",
                Title = "Validation Error",
                Status = 422,
                Detail = "Multiple fields failed validation",
                Instance = "/api/entities/123/validation",
                Errors = new Dictionary<string, string[]>
                {
                    ["Username"] = ["Username is already taken"],
                    ["Password"] = ["Password is too weak", "Password must contain numbers"]
                }
            };

            string jsonContent = JsonSerializer.Serialize(problemResponse, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

            using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Failures.ShouldNotBeNull();
            result.Failures.Count.ShouldBe(2);
            result.Failures["Username"].ShouldBe(["Username is already taken"]);
            result.Failures["Password"].ShouldBe(["Password is too weak", "Password must contain numbers"]);
        }

        [Fact]
        public async Task ToResultAsync_WithPartialRFC7807ValidationProblem_HandlesOptionalProperties()
        {
            // Arrange - Only include some RFC 7807 properties
            ValidationProblemResponse problemResponse = new()
            {
                Title = "Validation failed",
                Status = 400,
                // Type, Detail, Instance are omitted
                Errors = new Dictionary<string, string[]>
                {
                    ["Field"] = ["Field is invalid"]
                }
            };

            string jsonContent = JsonSerializer.Serialize(problemResponse, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

            using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
            };

            // Act
            Result result = await response.ToResultAsync();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Failures.ShouldNotBeNull();
            result.Failures.Count.ShouldBe(1);
            result.Failures["Field"].ShouldBe(["Field is invalid"]);
        }

        [Fact]
        public async Task ToResultAsync_WithMinimalValidationProblemResponse_MaintainsBackwardCompatibility()
        {
            // Arrange - Only errors property (legacy format)
            ValidationProblemResponse problemResponse = new()
            {
                Errors = new Dictionary<string, string[]>
                {
                    ["LegacyField"] = ["Legacy error message"]
                }
            };

            string jsonContent = JsonSerializer.Serialize(problemResponse, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

            using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
            };

            // Act
            Result result = await response.ToResultAsync();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Failures.ShouldNotBeNull();
            result.Failures.Count.ShouldBe(1);
            result.Failures["LegacyField"].ShouldBe(["Legacy error message"]);
        }

        [Fact]
        public async Task ToResultFromJsonAsync_WithJsonTypeInfo_HandlesFullRFC7807ValidationProblem()
        {
            // Arrange
            ValidationProblemResponse problemResponse = new()
            {
                Type = "https://example.com/problem/validation-error",
                Title = "Validation Error",
                Status = 422,
                Detail = "Multiple fields failed validation",
                Instance = "/api/entities/123/validation",
                Errors = new Dictionary<string, string[]>
                {
                    ["Model.Property"] = ["Property validation failed"]
                }
            };

            string jsonContent = JsonSerializer.Serialize(problemResponse, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

            using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
            };

            // Act
            Result<TestModel?> result = await response.ToResultFromJsonAsync(TestModelJsonContext.Default.TestModel);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Failures.ShouldNotBeNull();
            result.Failures.Count.ShouldBe(1);
            result.Failures["Model.Property"].ShouldBe(["Property validation failed"]);
        }

        [Fact]
        public async Task ToResultAsTextAsync_WithFullRFC7807ValidationProblem_ReturnsFailureWithAllErrors()
        {
            // Arrange
            ValidationProblemResponse problemResponse = new()
            {
                Type = "https://example.com/problem/validation-error",
                Title = "Validation Error",
                Status = 400,
                Detail = "Text endpoint validation failed",
                Instance = "/api/text/validation",
                Errors = new Dictionary<string, string[]>
                {
                    ["TextInput"] = ["Text input is required", "Text input is too long"]
                }
            };

            string jsonContent = JsonSerializer.Serialize(problemResponse, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

            using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
            };

            // Act
            Result<string?> result = await response.ToResultAsTextAsync();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Failures.ShouldNotBeNull();
            result.Failures.Count.ShouldBe(1);
            result.Failures["TextInput"].ShouldBe(["Text input is required", "Text input is too long"]);
        }

        [Theory]
        [InlineData("https://tools.ietf.org/html/rfc7231#section-6.5.1")]
        [InlineData("about:blank")]
        [InlineData("")]
        [InlineData(null)]
        public async Task ToResultAsync_WithVariousTypeValues_HandlesAllCasesCorrectly(string? typeValue)
        {
            // Arrange
            ValidationProblemResponse problemResponse = new()
            {
                Type = typeValue,
                Title = "Test Error",
                Status = 400,
                Errors = new Dictionary<string, string[]>
                {
                    ["TestField"] = ["Test error message"]
                }
            };

            string jsonContent = JsonSerializer.Serialize(problemResponse, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

            using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
            };

            // Act
            Result result = await response.ToResultAsync();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Failures.ShouldNotBeNull();
            result.Failures.Count.ShouldBe(1);
            result.Failures["TestField"].ShouldBe(["Test error message"]);
        }
    }

    #endregion TASK-047: Full RFC 7807 ValidationProblemDetails Support Tests
}