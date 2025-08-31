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
}