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
}