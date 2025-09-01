using System.Net;
using System.Net.Mime;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FlowRight.Core.Results;
using FlowRight.Http.Extensions;
using FlowRight.Http.Models;
using Shouldly;
using Xunit;

namespace FlowRight.Http.Tests.Extensions;

/// <summary>
/// Contains comprehensive tests for the <see cref="HttpResponseMessageExtensions"/> class.
/// </summary>
/// <remarks>
/// These tests verify that HTTP response messages can be properly converted to Result types,
/// handling various HTTP status codes, content types, and serialization scenarios while
/// maintaining proper error handling and validation problem response parsing.
/// </remarks>
public sealed class HttpResponseMessageExtensionsTests
{
    #region Test Infrastructure

    private readonly JsonSerializerOptions _jsonOptions;

    public HttpResponseMessageExtensionsTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    #endregion Test Infrastructure

    #region ToResultAsync Tests

    [Fact]
    public async Task ToResultAsync_SuccessStatusCode_ShouldReturnSuccess()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.OK);

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.Accepted)]
    public async Task ToResultAsync_AnySuccessStatusCode_ShouldReturnSuccess(HttpStatusCode statusCode)
    {
        // Arrange
        using HttpResponseMessage response = new(statusCode);

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ToResultAsync_BadRequestWithoutProblemDetails_ShouldReturnFailureWithBody()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad request error", Encoding.UTF8, MediaTypeNames.Text.Plain)
        };

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Bad request error");
    }

    [Fact]
    public async Task ToResultAsync_BadRequestWithValidationProblem_ShouldReturnFailureWithValidationErrors()
    {
        // Arrange
        ValidationProblemResponse validationProblem = new()
        {
            Errors = new Dictionary<string, string[]>
            {
                { "Email", ["Email is required", "Email format is invalid"] },
                { "Name", ["Name is required"] }
            }
        };

        string json = JsonSerializer.Serialize(validationProblem, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

        using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
        };

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Failures.ShouldNotBeNull();
        result.Failures.Count.ShouldBe(2);
        result.Failures.ShouldContainKey("Email");
        result.Failures["Email"].ShouldBe(["Email is required", "Email format is invalid"]);
        result.Failures.ShouldContainKey("Name");
        result.Failures["Name"].ShouldBe(["Name is required"]);
    }

    [Fact]
    public async Task ToResultAsync_Unauthorized_ShouldReturnFailureWithSecurityException()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.Unauthorized);

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Unauthorized");
    }

    [Fact]
    public async Task ToResultAsync_Forbidden_ShouldReturnFailureWithSecurityException()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.Forbidden);

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Unauthorized");
    }

    [Fact]
    public async Task ToResultAsync_NotFound_ShouldReturnFailureWithNotFoundError()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.NotFound);

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Not Found");
    }

    [Fact]
    public async Task ToResultAsync_InternalServerError_ShouldReturnFailureWithServerError()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.InternalServerError);

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Internal Server Error");
    }

    [Fact]
    public async Task ToResultAsync_UnexpectedStatusCode_ShouldReturnFailureWithStatusAndBody()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent("Service temporarily unavailable", Encoding.UTF8, MediaTypeNames.Text.Plain)
        };

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Unexpected ServiceUnavailable: Service temporarily unavailable");
    }

    [Fact]
    public async Task ToResultAsync_NullResponseMessage_ShouldThrowArgumentNullException()
    {
        // Arrange
        HttpResponseMessage? response = null;

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() => response!.ToResultAsync());
    }

    [Fact]
    public async Task ToResultAsync_WithCancellationToken_ShouldHonorCancellation()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        cts.Cancel();

        using HttpResponseMessage response = new(HttpStatusCode.OK);

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() => response.ToResultAsync(cts.Token));
    }

    #endregion ToResultAsync Tests

    #region ToResultFromJsonAsync<T> Tests

    [Fact]
    public async Task ToResultFromJsonAsync_SuccessWithValidJson_ShouldReturnSuccessWithDeserializedValue()
    {
        // Arrange
        TestModel expected = new() { Id = 42, Name = "Test" };
        string json = JsonSerializer.Serialize(expected, _jsonOptions);

        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>(_jsonOptions);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out TestModel? value).ShouldBeTrue();
        value.ShouldNotBeNull();
        value!.Id.ShouldBe(expected.Id);
        value.Name.ShouldBe(expected.Name);
    }

    [Fact]
    public async Task ToResultFromJsonAsync_SuccessWithNullJson_ShouldReturnSuccessWithNull()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>(_jsonOptions);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out TestModel? value).ShouldBeTrue();
        value.ShouldBeNull();
    }

    [Fact]
    public async Task ToResultFromJsonAsync_BadRequestWithValidationProblem_ShouldReturnFailureWithValidationErrors()
    {
        // Arrange
        ValidationProblemResponse validationProblem = new()
        {
            Errors = new Dictionary<string, string[]>
            {
                { "Id", ["Id must be positive"] }
            }
        };

        string json = JsonSerializer.Serialize(validationProblem, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

        using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
        };

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>(_jsonOptions);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Failures.ShouldNotBeNull();
        result.Failures.Count.ShouldBe(1);
        result.Failures.ShouldContainKey("Id");
        result.Failures["Id"].ShouldBe(["Id must be positive"]);
    }

    [Fact]
    public async Task ToResultFromJsonAsync_BadRequestWithoutProblemDetails_ShouldReturnFailureWithError()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad request", Encoding.UTF8, MediaTypeNames.Text.Plain)
        };

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>(_jsonOptions);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Bad request");
    }

    [Fact]
    public async Task ToResultFromJsonAsync_Unauthorized_ShouldReturnFailureWithSecurityException()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.Unauthorized);

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>(_jsonOptions);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Unauthorized");
    }

    [Fact]
    public async Task ToResultFromJsonAsync_NotFound_ShouldReturnFailureWithNotFoundError()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.NotFound);

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>(_jsonOptions);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Not Found");
    }

    [Fact]
    public async Task ToResultFromJsonAsync_InternalServerError_ShouldReturnFailureWithServerError()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.InternalServerError);

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>(_jsonOptions);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("Internal Server Error");
    }

    [Fact]
    public async Task ToResultFromJsonAsync_WithoutOptions_ShouldUseDefaultOptions()
    {
        // Arrange
        TestModel expected = new() { Id = 42, Name = "Test" };
        string json = JsonSerializer.Serialize(expected);

        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out TestModel? value).ShouldBeTrue();
        value.ShouldNotBeNull();
        value!.Id.ShouldBe(expected.Id);
        value.Name.ShouldBe(expected.Name);
    }

    [Fact]
    public async Task ToResultFromJsonAsync_NullResponseMessage_ShouldThrowArgumentNullException()
    {
        // Arrange
        HttpResponseMessage? response = null;

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() => response!.ToResultFromJsonAsync<TestModel>());
    }

    #endregion ToResultFromJsonAsync<T> Tests

    #region Edge Cases and Error Handling

    [Fact]
    public async Task ToResultAsync_BadRequestWithEmptyValidationProblem_ShouldReturnFailureWithEmptyErrors()
    {
        // Arrange
        ValidationProblemResponse validationProblem = new();
        string json = JsonSerializer.Serialize(validationProblem, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

        using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
        };

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Failures.ShouldNotBeNull();
        result.Failures.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToResultFromJsonAsync_InvalidJson_ShouldThrowJsonException()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("{ invalid json }", Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        // Act & Assert
        await Should.ThrowAsync<JsonException>(() => response.ToResultFromJsonAsync<TestModel>());
    }

    [Fact]
    public async Task ToResultAsync_BadRequestWithMalformedValidationProblem_ShouldFallbackToStringError()
    {
        // Arrange
        using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{ invalid json }", Encoding.UTF8, MediaTypeNames.Application.ProblemJson)
        };

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe("{ invalid json }");
    }

    [Theory]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task ToResultAsync_VariousErrorStatusCodes_ShouldReturnUnexpectedStatusCodeFailure(HttpStatusCode statusCode)
    {
        // Arrange
        using HttpResponseMessage response = new(statusCode)
        {
            Content = new StringContent("Error details", Encoding.UTF8, MediaTypeNames.Text.Plain)
        };

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe($"Unexpected {statusCode}: Error details");
    }

    #endregion Edge Cases and Error Handling

    #region Test Models and Infrastructure

    public sealed class TestModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }


    #endregion Test Models and Infrastructure
}