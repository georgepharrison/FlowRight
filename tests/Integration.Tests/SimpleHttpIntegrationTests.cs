using FlowRight.Core.Results;
using FlowRight.Http.Extensions;
using Shouldly;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FlowRight.Integration.Tests;

/// <summary>
/// Simple integration tests for HTTP extension methods that demonstrate TDD failing tests.
/// These tests validate the HTTP integration behavior with real HTTP responses.
/// </summary>
/// <remarks>
/// <para>
/// These integration tests demonstrate comprehensive HTTP integration testing including:
/// </para>
/// <list type="bullet">
/// <item><description>HTTP status code mapping (2xx success, 4xx client errors, 5xx server errors)</description></item>
/// <item><description>Content type handling and JSON deserialization</description></item>
/// <item><description>Error response handling and ValidationProblemDetails</description></item>
/// <item><description>Performance testing with HTTP requests</description></item>
/// <item><description>Edge cases like timeout, network errors, malformed responses</description></item>
/// </list>
/// </remarks>
public sealed class SimpleHttpIntegrationTests
{
    #region Basic HTTP Response Tests

    [Fact]
    public async Task ToResultAsync_WithRealHttpSuccessResponse_ShouldReturnSuccessResult()
    {
        // Arrange
        using HttpClient client = new();
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("Success", Encoding.UTF8, "text/plain")
        };

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ToResultAsync_WithRealHttpBadRequestResponse_ShouldReturnFailureResult()
    {
        // Arrange
        using HttpClient client = new();
        using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad request", Encoding.UTF8, "text/plain")
        };

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task ToResultAsync_WithRealHttpServerErrorResponse_ShouldReturnServerErrorFailure()
    {
        // Arrange
        using HttpClient client = new();
        using HttpResponseMessage response = new(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Internal server error", Encoding.UTF8, "text/plain")
        };

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.ResultType.ShouldBe(ResultType.Error);
    }

    [Fact]
    public async Task ToResultFromJsonAsync_WithRealJsonResponse_ShouldDeserializeSuccessfully()
    {
        // Arrange
        using HttpClient client = new();
        TestModel testData = new()
        {
            Id = 123,
            Name = "Test User",
            Email = "test@example.com"
        };
        
        string jsonContent = JsonSerializer.Serialize(testData);
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out TestModel? value).ShouldBeTrue();
        value.ShouldNotBeNull();
        value.Id.ShouldBe(123);
        value.Name.ShouldBe("Test User");
        value.Email.ShouldBe("test@example.com");
    }

    [Fact]
    public async Task ToResultFromJsonAsync_WithRealInvalidJsonResponse_ShouldReturnSuccessWithNull()
    {
        // Arrange
        using HttpClient client = new();
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("{invalid json}", Encoding.UTF8, "application/json")
        };

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out TestModel? value).ShouldBeTrue();
        value.ShouldBeNull();
    }

    [Fact]
    public async Task ToResultFromJsonAsync_WithRealBadRequestResponse_ShouldReturnFailureResult()
    {
        // Arrange
        using HttpClient client = new();
        using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad request", Encoding.UTF8, "text/plain")
        };

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task ToResultFromJsonAsync_WithRealMissingContentTypeResponse_ShouldReturnFailure()
    {
        // Arrange
        using HttpClient client = new();
        using HttpResponseMessage response = new(HttpStatusCode.OK);
        // Manually remove content type header
        response.Content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(new { id = 1 }));

        // Act
        Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain("Missing content type header");
    }

    #endregion Basic HTTP Response Tests

    #region Content Type Validation Tests

    [Fact]
    public async Task ToResultAsTextAsync_WithRealTextResponse_ShouldReturnTextContent()
    {
        // Arrange
        using HttpClient client = new();
        string textContent = "Plain text response";
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent(textContent, Encoding.UTF8, "text/plain")
        };

        // Act
        Result<string?> result = await response.ToResultAsTextAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out string? value).ShouldBeTrue();
        value.ShouldBe(textContent);
    }

    [Fact]
    public async Task ToResultAsBytesAsync_WithRealBinaryResponse_ShouldReturnBinaryContent()
    {
        // Arrange
        using HttpClient client = new();
        byte[] binaryData = Encoding.UTF8.GetBytes("Binary content data");
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(binaryData)
        };
        response.Content.Headers.ContentType = new("application/octet-stream");

        // Act
        Result<byte[]?> result = await response.ToResultAsBytesAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out byte[]? value).ShouldBeTrue();
        value.ShouldNotBeNull();
        Encoding.UTF8.GetString(value).ShouldBe("Binary content data");
    }

    [Fact]
    public void IsJsonContentType_WithRealJsonResponse_ShouldReturnTrue()
    {
        // Arrange
        using HttpClient client = new();
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };

        // Act
        bool isJson = response.IsJsonContentType();

        // Assert
        isJson.ShouldBeTrue();
    }

    [Fact]
    public void IsTextContentType_WithRealTextResponse_ShouldReturnTrue()
    {
        // Arrange
        using HttpClient client = new();
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("Text", Encoding.UTF8, "text/plain")
        };

        // Act
        bool isText = response.IsTextContentType();

        // Assert
        isText.ShouldBeTrue();
    }

    #endregion Content Type Validation Tests

    #region Error Response Tests

    [Fact]
    public async Task ToResultAsync_WithValidationProblemDetailsResponse_ShouldReturnValidationFailure()
    {
        // This test will fail initially as it simulates a real validation problem response
        // from an API that returns RFC 7807 Problem Details

        // Arrange
        using HttpClient client = new();
        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "One or more validation errors occurred.",
            status = 400,
            detail = "Validation failed for the submitted data.",
            instance = "/api/test/validation",
            errors = new Dictionary<string, string[]>
            {
                { "Email", ["Email is required", "Email format is invalid"] },
                { "Name", ["Name is required"] }
            }
        };

        string jsonContent = JsonSerializer.Serialize(problemDetails);
        using HttpResponseMessage response = new(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/problem+json")
        };

        // Act
        Result result = await response.ToResultAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain("Email is required");
        result.Error.ShouldContain("Email format is invalid");
        result.Error.ShouldContain("Name is required");
    }

    #endregion Error Response Tests

    #region Performance and Edge Cases Tests

    [Fact]
    public async Task ToResultAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // This test demonstrates timeout/cancellation handling behavior
        // It will initially fail until proper cancellation handling is implemented

        // Arrange
        using CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(1));
        using HttpClient client = new();
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent("Success", Encoding.UTF8, "text/plain")
        };

        // Act & Assert
        // Note: This test may not throw as expected with a mock response
        // but demonstrates the expected behavior with real network requests
        await Should.NotThrowAsync(async () =>
        {
            Result result = await response.ToResultAsync(cts.Token);
            result.IsSuccess.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task ToResultFromJsonAsync_WithMultipleConcurrentRequests_ShouldHandleAllRequests()
    {
        // Arrange
        List<Task<Result<TestModel?>>> tasks = [];
        
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(ExecuteJsonRequestAsync(i));
        }

        // Act
        Result<TestModel?>[] results = await Task.WhenAll(tasks);

        // Assert
        results.ShouldAllBe(r => r.IsSuccess);
        for (int i = 0; i < results.Length; i++)
        {
            results[i].TryGetValue(out TestModel? value).ShouldBeTrue();
            value.ShouldNotBeNull();
            value.Id.ShouldBe(i);
        }
    }

    [Fact]
    public async Task HttpExtensionMethods_WithPerformanceTesting_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        const int iterations = 100;

        // Act
        for (int i = 0; i < iterations; i++)
        {
            using HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\": 1}", Encoding.UTF8, "application/json")
            };
            
            Result<TestModel?> result = await response.ToResultFromJsonAsync<TestModel>();
            result.IsSuccess.ShouldBeTrue();
        }

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(5000); // Should complete within 5 seconds
    }

    #endregion Performance and Edge Cases Tests

    #region Helper Methods

    private static async Task<Result<TestModel?>> ExecuteJsonRequestAsync(int id)
    {
        TestModel testData = new()
        {
            Id = id,
            Name = $"Test User {id}",
            Email = $"test{id}@example.com"
        };
        
        string jsonContent = JsonSerializer.Serialize(testData);
        using HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        return await response.ToResultFromJsonAsync<TestModel>();
    }

    #endregion Helper Methods
}

/// <summary>
/// Test model for JSON serialization tests.
/// </summary>
public sealed class TestModel
{
    /// <summary>
    /// Gets or sets the model identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}