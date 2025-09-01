using System.Text.Json;
using FlowRight.Http.Models;
using Shouldly;
using Xunit;

namespace FlowRight.Http.Tests.Models;

/// <summary>
/// Contains comprehensive tests for the <see cref="ValidationProblemResponse"/> class.
/// </summary>
/// <remarks>
/// These tests verify that the ValidationProblemResponse can properly serialize and deserialize
/// validation error data while maintaining compatibility with ASP.NET Core's ValidationProblemDetails
/// format and ensuring round-trip operations preserve all error information.
/// </remarks>
public sealed class ValidationProblemResponseTests
{
    #region Test Infrastructure

    private readonly JsonSerializerOptions _options;

    public ValidationProblemResponseTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    #endregion Test Infrastructure

    #region Serialization Tests

    [Fact]
    public void Serialize_EmptyValidationProblemResponse_ShouldProduceCorrectJson()
    {
        // Arrange
        ValidationProblemResponse response = new();

        // Act
        string json = JsonSerializer.Serialize(response, _options);

        // Assert
        json.ShouldNotBeNullOrWhiteSpace();
        json.ShouldContain("\"errors\"");
        json.ShouldContain("{}");
    }

    [Fact]
    public void Serialize_ValidationProblemResponseWithSingleError_ShouldProduceCorrectJson()
    {
        // Arrange
        ValidationProblemResponse response = new()
        {
            Errors = new Dictionary<string, string[]>
            {
                { "Email", ["Email is required"] }
            }
        };

        // Act
        string json = JsonSerializer.Serialize(response, _options);

        // Assert
        json.ShouldContain("\"email\"");
        json.ShouldContain("\"Email is required\"");
    }

    [Fact]
    public void Serialize_ValidationProblemResponseWithMultipleErrors_ShouldProduceCorrectJson()
    {
        // Arrange
        ValidationProblemResponse response = new()
        {
            Errors = new Dictionary<string, string[]>
            {
                { "Name", ["Name is required", "Name must be at least 2 characters"] },
                { "Email", ["Email is required", "Email format is invalid"] },
                { "Age", ["Age must be greater than 0"] }
            }
        };

        // Act
        string json = JsonSerializer.Serialize(response, _options);

        // Assert
        json.ShouldContain("\"name\"");
        json.ShouldContain("\"Name is required\"");
        json.ShouldContain("\"Name must be at least 2 characters\"");
        json.ShouldContain("\"email\"");
        json.ShouldContain("\"Email is required\"");
        json.ShouldContain("\"Email format is invalid\"");
        json.ShouldContain("\"age\"");
        json.ShouldContain("\"Age must be greater than 0\"");
    }

    #endregion Serialization Tests

    #region Deserialization Tests

    [Fact]
    public void Deserialize_EmptyErrorsJson_ShouldCreateCorrectResponse()
    {
        // Arrange
        string json = @"{
            ""errors"": {}
        }";

        // Act
        ValidationProblemResponse? response = JsonSerializer.Deserialize<ValidationProblemResponse>(json, _options);

        // Assert
        response.ShouldNotBeNull();
        response.Errors.ShouldNotBeNull();
        response.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Deserialize_SingleErrorJson_ShouldCreateCorrectResponse()
    {
        // Arrange
        string json = @"{
            ""errors"": {
                ""Email"": [""Email is required""]
            }
        }";

        // Act
        ValidationProblemResponse? response = JsonSerializer.Deserialize<ValidationProblemResponse>(json, _options);

        // Assert
        response.ShouldNotBeNull();
        response.Errors.ShouldNotBeNull();
        response.Errors.Count.ShouldBe(1);
        response.Errors.ShouldContainKey("Email");
        response.Errors["Email"].ShouldBe(["Email is required"]);
    }

    [Fact]
    public void Deserialize_MultipleErrorsJson_ShouldCreateCorrectResponse()
    {
        // Arrange
        string json = @"{
            ""errors"": {
                ""Name"": [""Name is required"", ""Name must be at least 2 characters""],
                ""Email"": [""Email is required"", ""Email format is invalid""],
                ""Age"": [""Age must be greater than 0""]
            }
        }";

        // Act
        ValidationProblemResponse? response = JsonSerializer.Deserialize<ValidationProblemResponse>(json, _options);

        // Assert
        response.ShouldNotBeNull();
        response.Errors.ShouldNotBeNull();
        response.Errors.Count.ShouldBe(3);

        response.Errors.ShouldContainKey("Name");
        response.Errors["Name"].ShouldBe(["Name is required", "Name must be at least 2 characters"]);

        response.Errors.ShouldContainKey("Email");
        response.Errors["Email"].ShouldBe(["Email is required", "Email format is invalid"]);

        response.Errors.ShouldContainKey("Age");
        response.Errors["Age"].ShouldBe(["Age must be greater than 0"]);
    }

    #endregion Deserialization Tests

    #region AOT Serialization Tests

    [Fact]
    public void Serialize_WithAOTContext_ShouldProduceCorrectJson()
    {
        // Arrange
        ValidationProblemResponse response = new()
        {
            Errors = new Dictionary<string, string[]>
            {
                { "Field1", ["Error 1", "Error 2"] }
            }
        };

        // Act
        string json = JsonSerializer.Serialize(response, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

        // Assert
        json.ShouldNotBeNullOrWhiteSpace();
        json.ShouldContain("Field1");
        json.ShouldContain("Error 1");
        json.ShouldContain("Error 2");
    }

    [Fact]
    public void Deserialize_WithAOTContext_ShouldCreateCorrectResponse()
    {
        // Arrange
        string json = @"{
            ""errors"": {
                ""TestField"": [""Test error message""]
            }
        }";

        // Act
        ValidationProblemResponse? response = JsonSerializer.Deserialize(json, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

        // Assert
        response.ShouldNotBeNull();
        response.Errors.ShouldNotBeNull();
        response.Errors.Count.ShouldBe(1);
        response.Errors.ShouldContainKey("TestField");
        response.Errors["TestField"].ShouldBe(["Test error message"]);
    }

    #endregion AOT Serialization Tests

    #region Round-trip Tests

    [Theory]
    [MemberData(nameof(GetValidationProblemResponseTestCases))]
    public void RoundTrip_AllValidationProblemResponses_ShouldPreserveOriginalState(ValidationProblemResponse originalResponse)
    {
        // Act
        string json = JsonSerializer.Serialize(originalResponse, _options);
        ValidationProblemResponse? deserializedResponse = JsonSerializer.Deserialize<ValidationProblemResponse>(json, _options);

        // Assert
        deserializedResponse.ShouldNotBeNull();
        deserializedResponse.Errors.ShouldNotBeNull();
        deserializedResponse.Errors.Count.ShouldBe(originalResponse.Errors.Count);

        foreach (KeyValuePair<string, string[]> error in originalResponse.Errors)
        {
            deserializedResponse.Errors.ShouldContainKey(error.Key);
            deserializedResponse.Errors[error.Key].ShouldBe(error.Value);
        }
    }

    [Theory]
    [MemberData(nameof(GetValidationProblemResponseTestCases))]
    public void RoundTrip_WithAOTContext_ShouldPreserveOriginalState(ValidationProblemResponse originalResponse)
    {
        // Act
        string json = JsonSerializer.Serialize(originalResponse, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);
        ValidationProblemResponse? deserializedResponse = JsonSerializer.Deserialize(json, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

        // Assert
        deserializedResponse.ShouldNotBeNull();
        deserializedResponse.Errors.ShouldNotBeNull();
        deserializedResponse.Errors.Count.ShouldBe(originalResponse.Errors.Count);

        foreach (KeyValuePair<string, string[]> error in originalResponse.Errors)
        {
            deserializedResponse.Errors.ShouldContainKey(error.Key);
            deserializedResponse.Errors[error.Key].ShouldBe(error.Value);
        }
    }

    public static TheoryData<ValidationProblemResponse> GetValidationProblemResponseTestCases() =>
        new()
        {
            new ValidationProblemResponse(),
            new ValidationProblemResponse
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "Email", ["Email is required"] }
                }
            },
            new ValidationProblemResponse
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "Name", ["Name is required", "Name must be at least 2 characters"] },
                    { "Email", ["Email is required", "Email format is invalid"] },
                    { "Age", ["Age must be greater than 0"] }
                }
            }
        };

    #endregion Round-trip Tests

    #region Edge Cases

    [Fact]
    public void Deserialize_EmptyJson_ShouldThrowJsonException()
    {
        // Arrange
        string json = "";

        // Act & Assert
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<ValidationProblemResponse>(json, _options));
    }

    [Fact]
    public void Deserialize_InvalidJson_ShouldThrowJsonException()
    {
        // Arrange
        string json = "{ invalid json }";

        // Act & Assert
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<ValidationProblemResponse>(json, _options));
    }

    [Fact]
    public void Deserialize_JsonWithoutErrorsProperty_ShouldCreateDefaultResponse()
    {
        // Arrange
        string json = @"{}";

        // Act
        ValidationProblemResponse? response = JsonSerializer.Deserialize<ValidationProblemResponse>(json, _options);

        // Assert
        response.ShouldNotBeNull();
        response.Errors.ShouldNotBeNull();
        response.Errors.ShouldBeEmpty();
    }

    #endregion Edge Cases

    #region TASK-047: RFC 7807 ProblemDetails Properties Tests

    [Fact]
    public void Serialize_FullRFC7807ValidationProblemResponse_ShouldProduceCompleteJson()
    {
        // Arrange
        ValidationProblemResponse response = new()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = 400,
            Detail = "The request failed validation. See errors for details.",
            Instance = "/api/users/validation-error/12345",
            Errors = new Dictionary<string, string[]>
            {
                ["Email"] = ["Email is required", "Email format is invalid"],
                ["Name"] = ["Name is required"]
            }
        };

        // Act
        string json = JsonSerializer.Serialize(response, _options);

        // Assert
        json.ShouldContain(@"""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1""");
        json.ShouldContain(@"""title"": ""One or more validation errors occurred.""");
        json.ShouldContain(@"""status"": 400");
        json.ShouldContain(@"""detail"": ""The request failed validation. See errors for details.""");
        json.ShouldContain(@"""instance"":");
        json.ShouldContain(@"""errors"":");
        json.ShouldContain(@"""Email"":");
        json.ShouldContain(@"""Name"":");
    }

    [Fact]
    public void Deserialize_FullRFC7807ValidationProblemResponse_ShouldProduceCorrectObject()
    {
        // Arrange
        string json = @"{
            ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
            ""title"": ""One or more validation errors occurred."",
            ""status"": 400,
            ""detail"": ""The request failed validation. See errors for details."",
            ""instance"": ""/api/users/validation-error/12345"",
            ""errors"": {
                ""Email"": [""Email is required"", ""Email format is invalid""],
                ""Name"": [""Name is required""]
            }
        }";

        // Act
        ValidationProblemResponse? response = JsonSerializer.Deserialize<ValidationProblemResponse>(json, _options);

        // Assert
        response.ShouldNotBeNull();
        response.Type.ShouldBe("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        response.Title.ShouldBe("One or more validation errors occurred.");
        response.Status.ShouldBe(400);
        response.Detail.ShouldBe("The request failed validation. See errors for details.");
        response.Instance.ShouldBe("/api/users/validation-error/12345");
        response.Errors.ShouldNotBeNull();
        response.Errors.Count.ShouldBe(2);
        response.Errors["Email"].ShouldBe(new[] { "Email is required", "Email format is invalid" });
        response.Errors["Name"].ShouldBe(new[] { "Name is required" });
    }

    [Fact]
    public void Serialize_PartialRFC7807ValidationProblemResponse_ShouldOmitNullProperties()
    {
        // Arrange - Only populate some RFC 7807 properties
        ValidationProblemResponse response = new()
        {
            Title = "Validation failed",
            Status = 400,
            // Type, Detail, Instance are null and should be omitted
            Errors = new Dictionary<string, string[]>
            {
                ["Field"] = ["Field is invalid"]
            }
        };

        // Act
        string json = JsonSerializer.Serialize(response, _options);

        // Assert
        json.ShouldContain(@"""title"": ""Validation failed""");
        json.ShouldContain(@"""status"": 400");
        json.ShouldContain(@"""errors"":");
        json.ShouldContain(@"""Field"":");
        
        // These should be omitted due to JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)
        json.ShouldNotContain(@"""type"":");
        json.ShouldNotContain(@"""detail"":");
        json.ShouldNotContain(@"""instance"":");
    }

    [Fact]
    public void Deserialize_MinimalValidationProblemResponse_ShouldHandleOptionalProperties()
    {
        // Arrange - JSON with only errors (backward compatibility)
        string json = @"{
            ""errors"": {
                ""Field"": [""Field is required""]
            }
        }";

        // Act
        ValidationProblemResponse? response = JsonSerializer.Deserialize<ValidationProblemResponse>(json, _options);

        // Assert
        response.ShouldNotBeNull();
        response.Type.ShouldBeNull();
        response.Title.ShouldBeNull();
        response.Status.ShouldBeNull();
        response.Detail.ShouldBeNull();
        response.Instance.ShouldBeNull();
        response.Errors.ShouldNotBeNull();
        response.Errors.Count.ShouldBe(1);
        response.Errors["Field"].ShouldBe(new[] { "Field is required" });
    }

    [Fact]
    public void RoundTrip_FullRFC7807ValidationProblemResponse_ShouldPreserveAllProperties()
    {
        // Arrange
        ValidationProblemResponse original = new()
        {
            Type = "https://example.com/problem/validation-error",
            Title = "Validation Error",
            Status = 422,
            Detail = "Multiple fields failed validation",
            Instance = "/api/entities/123/validation",
            Errors = new Dictionary<string, string[]>
            {
                ["FirstName"] = ["Required", "Too short"],
                ["LastName"] = ["Required"],
                ["Age"] = ["Must be positive"]
            }
        };

        // Act
        string json = JsonSerializer.Serialize(original, _options);
        ValidationProblemResponse? deserialized = JsonSerializer.Deserialize<ValidationProblemResponse>(json, _options);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.Type.ShouldBe(original.Type);
        deserialized.Title.ShouldBe(original.Title);
        deserialized.Status.ShouldBe(original.Status);
        deserialized.Detail.ShouldBe(original.Detail);
        deserialized.Instance.ShouldBe(original.Instance);
        deserialized.Errors.Count.ShouldBe(original.Errors.Count);
        
        foreach (KeyValuePair<string, string[]> kvp in original.Errors)
        {
            deserialized.Errors.ShouldContainKey(kvp.Key);
            deserialized.Errors[kvp.Key].ShouldBe(kvp.Value);
        }
    }

    [Fact]
    public void RoundTrip_WithAOTContext_FullRFC7807ValidationProblemResponse_ShouldWork()
    {
        // Arrange
        ValidationProblemResponse original = new()
        {
            Type = "https://example.com/problem/validation-error",
            Title = "Validation Error",
            Status = 422,
            Detail = "Multiple fields failed validation",
            Instance = "/api/entities/123/validation",
            Errors = new Dictionary<string, string[]>
            {
                ["Email"] = ["Invalid format"],
                ["Password"] = ["Too weak", "Too short"]
            }
        };

        // Act
        string json = JsonSerializer.Serialize(original, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);
        ValidationProblemResponse? deserialized = JsonSerializer.Deserialize(json, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.Type.ShouldBe(original.Type);
        deserialized.Title.ShouldBe(original.Title);
        deserialized.Status.ShouldBe(original.Status);
        deserialized.Detail.ShouldBe(original.Detail);
        deserialized.Instance.ShouldBe(original.Instance);
        deserialized.Errors.Count.ShouldBe(original.Errors.Count);
        deserialized.Errors["Email"].ShouldBe(original.Errors["Email"]);
        deserialized.Errors["Password"].ShouldBe(original.Errors["Password"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidationProblemResponse_WithVariousTypeValues_ShouldSerializeCorrectly(string? typeValue)
    {
        // Arrange
        ValidationProblemResponse response = new()
        {
            Type = typeValue,
            Title = "Test",
            Errors = new Dictionary<string, string[]> { ["Test"] = ["Error"] }
        };

        // Act
        string json = JsonSerializer.Serialize(response, _options);
        ValidationProblemResponse? deserialized = JsonSerializer.Deserialize<ValidationProblemResponse>(json, _options);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.Type.ShouldBe(typeValue);
        deserialized.Title.ShouldBe("Test");
    }

    #endregion TASK-047: RFC 7807 ProblemDetails Properties Tests
}