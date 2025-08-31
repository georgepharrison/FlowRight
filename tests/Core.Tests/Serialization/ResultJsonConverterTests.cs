using System.Security;
using System.Text.Json;
using FlowRight.Core.Results;
using FlowRight.Core.Serialization;
using Shouldly;
using Xunit;

namespace FlowRight.Core.Tests.Serialization;

/// <summary>
/// Contains comprehensive tests for the <see cref="ResultJsonConverter"/> class.
/// </summary>
/// <remarks>
/// These tests verify that the JsonConverter can properly serialize and deserialize
/// all types of Result objects while maintaining immutability and preserving all
/// failure information across round-trip operations.
/// </remarks>
public sealed class ResultJsonConverterTests
{
    #region Test Infrastructure

    private readonly JsonSerializerOptions _options;

    public ResultJsonConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new ResultJsonConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    #endregion Test Infrastructure

    #region CanConvert Tests

    [Fact]
    public void CanConvert_WithResultType_ShouldReturnTrue()
    {
        // Arrange
        ResultJsonConverter converter = new();

        // Act
        bool canConvert = converter.CanConvert(typeof(Result));

        // Assert
        canConvert.ShouldBeTrue();
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    [InlineData(typeof(Result<string>))]
    [InlineData(typeof(object))]
    public void CanConvert_WithNonResultType_ShouldReturnFalse(Type type)
    {
        // Arrange
        ResultJsonConverter converter = new();

        // Act
        bool canConvert = converter.CanConvert(type);

        // Assert
        canConvert.ShouldBeFalse();
    }

    #endregion CanConvert Tests

    #region Success Serialization Tests

    [Fact]
    public void Serialize_SuccessResult_ShouldProduceCorrectJson()
    {
        // Arrange
        Result result = Result.Success();

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldNotBeNullOrWhiteSpace();
        json.ShouldContain("\"error\":\"\"");
        json.ShouldContain("\"failures\":{}");
        json.ShouldContain("\"failureType\":\"None\"");
        json.ShouldContain("\"resultType\":\"Success\"");
    }

    [Fact]
    public void Serialize_InformationResult_ShouldProduceCorrectJson()
    {
        // Arrange
        Result result = Result.Success(ResultType.Information);

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"resultType\":\"Information\"");
        json.ShouldContain("\"failureType\":\"None\"");
        json.ShouldContain("\"error\":\"\"");
    }

    [Fact]
    public void Deserialize_SuccessJson_ShouldCreateCorrectResult()
    {
        // Arrange
        string json = @"{
            ""error"": """",
            ""failures"": {},
            ""failureType"": ""None"",
            ""resultType"": ""Success""
        }";

        // Act
        Result result = JsonSerializer.Deserialize<Result>(json, _options)!;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.ResultType.ShouldBe(ResultType.Success);
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.Error.ShouldBeEmpty();
        result.Failures.ShouldBeEmpty();
    }

    #endregion Success Serialization Tests

    #region General Failure Serialization Tests

    [Fact]
    public void Serialize_GeneralFailure_ShouldProduceCorrectJson()
    {
        // Arrange
        Result result = Result.Failure("Something went wrong");

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"error\":\"Something went wrong\"");
        json.ShouldContain("\"failureType\":\"Error\"");
        json.ShouldContain("\"resultType\":\"Error\"");
        json.ShouldContain("\"failures\":{}");
    }

    [Fact]
    public void Deserialize_GeneralFailureJson_ShouldCreateCorrectResult()
    {
        // Arrange
        string json = @"{
            ""error"": ""Database connection failed"",
            ""failures"": {},
            ""failureType"": ""Error"",
            ""resultType"": ""Error""
        }";

        // Act
        Result result = JsonSerializer.Deserialize<Result>(json, _options)!;

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.ResultType.ShouldBe(ResultType.Error);
        result.FailureType.ShouldBe(ResultFailureType.Error);
        result.Error.ShouldBe("Database connection failed");
        result.Failures.ShouldBeEmpty();
    }

    [Fact]
    public void Serialize_CustomResultAndFailureType_ShouldPreserveTypes()
    {
        // Arrange
        Result result = Result.Failure("Warning message", ResultType.Warning, ResultFailureType.Error);

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"error\":\"Warning message\"");
        json.ShouldContain("\"failureType\":\"Error\"");
        json.ShouldContain("\"resultType\":\"Warning\"");
    }

    #endregion General Failure Serialization Tests

    #region Security Failure Serialization Tests

    [Fact]
    public void Serialize_SecurityFailure_ShouldProduceCorrectJson()
    {
        // Arrange
        SecurityException ex = new("Access denied");
        Result result = Result.Failure(ex);

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"error\":\"Access denied\"");
        json.ShouldContain("\"failureType\":\"Security\"");
        json.ShouldContain("\"resultType\":\"Error\"");
        json.ShouldContain("\"failures\":{}");
    }

    [Fact]
    public void Deserialize_SecurityFailureJson_ShouldCreateCorrectResult()
    {
        // Arrange
        string json = @"{
            ""error"": ""Unauthorized access attempt"",
            ""failures"": {},
            ""failureType"": ""Security"",
            ""resultType"": ""Error""
        }";

        // Act
        Result result = JsonSerializer.Deserialize<Result>(json, _options)!;

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ResultType.ShouldBe(ResultType.Error);
        result.FailureType.ShouldBe(ResultFailureType.Security);
        result.Error.ShouldBe("Unauthorized access attempt");
    }

    #endregion Security Failure Serialization Tests

    #region Operation Canceled Serialization Tests

    [Fact]
    public void Serialize_OperationCanceledFailure_ShouldProduceCorrectJson()
    {
        // Arrange
        OperationCanceledException ex = new("Operation was cancelled");
        Result result = Result.Failure(ex);

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"error\":\"Operation was cancelled\"");
        json.ShouldContain("\"failureType\":\"OperationCanceled\"");
        json.ShouldContain("\"resultType\":\"Warning\"");
        json.ShouldContain("\"failures\":{}");
    }

    [Fact]
    public void Deserialize_OperationCanceledFailureJson_ShouldCreateCorrectResult()
    {
        // Arrange
        string json = @"{
            ""error"": ""Request timeout"",
            ""failures"": {},
            ""failureType"": ""OperationCanceled"",
            ""resultType"": ""Warning""
        }";

        // Act
        Result result = JsonSerializer.Deserialize<Result>(json, _options)!;

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ResultType.ShouldBe(ResultType.Warning);
        result.FailureType.ShouldBe(ResultFailureType.OperationCanceled);
        result.Error.ShouldBe("Request timeout");
    }

    #endregion Operation Canceled Serialization Tests

    #region Validation Failure Serialization Tests

    [Fact]
    public void Serialize_ValidationFailure_ShouldProduceCorrectJson()
    {
        // Arrange
        Dictionary<string, string[]> errors = new()
        {
            { "Name", ["Name is required", "Name must be at least 2 characters"] },
            { "Email", ["Email is required", "Email format is invalid"] }
        };
        Result result = Result.ValidationFailure(errors);

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"failureType\":\"Validation\"");
        json.ShouldContain("\"resultType\":\"Error\"");
        json.ShouldContain("\"Name\"");
        json.ShouldContain("\"Name is required\"");
        json.ShouldContain("\"Email\"");
        json.ShouldContain("\"Email is required\"");
    }

    [Fact]
    public void Deserialize_ValidationFailureJson_ShouldCreateCorrectResult()
    {
        // Arrange
        string json = @"{
            ""error"": ""One or more validation errors occurred.\r\nName\r\n  - Name is required\r\nEmail\r\n  - Email format is invalid"",
            ""failures"": {
                ""Name"": [""Name is required""],
                ""Email"": [""Email format is invalid""]
            },
            ""failureType"": ""Validation"",
            ""resultType"": ""Error""
        }";

        // Act
        Result result = JsonSerializer.Deserialize<Result>(json, _options)!;

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ResultType.ShouldBe(ResultType.Error);
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldNotBeEmpty();
        result.Failures.Count.ShouldBe(2);
        result.Failures["Name"].ShouldBe(["Name is required"]);
        result.Failures["Email"].ShouldBe(["Email format is invalid"]);
    }

    [Fact]
    public void Serialize_SingleFieldValidationFailure_ShouldProduceCorrectJson()
    {
        // Arrange
        Result result = Result.Failure("Email", "Email is required");

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"failureType\":\"Validation\"");
        json.ShouldContain("\"Email\"");
        json.ShouldContain("\"Email is required\"");
    }

    #endregion Validation Failure Serialization Tests

    #region Round-trip Serialization Tests

    [Theory]
    [MemberData(nameof(GetResultTestCases))]
    public void RoundTrip_AllResultTypes_ShouldPreserveOriginalState(Result originalResult)
    {
        // Act
        string json = JsonSerializer.Serialize(originalResult, _options);
        Result deserializedResult = JsonSerializer.Deserialize<Result>(json, _options)!;

        // Assert
        deserializedResult.ShouldNotBeNull();
        deserializedResult.IsSuccess.ShouldBe(originalResult.IsSuccess);
        deserializedResult.IsFailure.ShouldBe(originalResult.IsFailure);
        deserializedResult.ResultType.ShouldBe(originalResult.ResultType);
        deserializedResult.FailureType.ShouldBe(originalResult.FailureType);
        deserializedResult.Error.ShouldBe(originalResult.Error);
        deserializedResult.Failures.Count.ShouldBe(originalResult.Failures.Count);
        
        foreach (KeyValuePair<string, string[]> failure in originalResult.Failures)
        {
            deserializedResult.Failures.ShouldContainKey(failure.Key);
            deserializedResult.Failures[failure.Key].ShouldBe(failure.Value);
        }
    }

    public static TheoryData<Result> GetResultTestCases() =>
        new()
        {
            Result.Success(),
            Result.Success(ResultType.Information),
            Result.Failure("General error"),
            Result.Failure("Custom error", ResultType.Warning, ResultFailureType.Error),
            Result.Failure(new SecurityException("Security violation")),
            Result.Failure(new OperationCanceledException("Timeout occurred")),
            Result.ValidationFailure(new Dictionary<string, string[]>
            {
                { "Field1", ["Error 1", "Error 2"] },
                { "Field2", ["Error 3"] }
            }),
            Result.Failure("SingleField", "Single field error")
        };

    #endregion Round-trip Serialization Tests

    #region Edge Case Tests

    [Fact]
    public void Deserialize_EmptyJson_ShouldThrowJsonException()
    {
        // Arrange
        string json = "";

        // Act & Assert
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Result>(json, _options));
    }

    [Fact]
    public void Deserialize_InvalidJson_ShouldThrowJsonException()
    {
        // Arrange
        string json = "{ invalid json }";

        // Act & Assert
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Result>(json, _options));
    }

    [Fact]
    public void Deserialize_JsonArray_ShouldThrowJsonException()
    {
        // Arrange
        string json = "[]";

        // Act & Assert
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Result>(json, _options));
    }

    [Fact]
    public void Deserialize_JsonWithMissingProperties_ShouldCreateDefaultResult()
    {
        // Arrange
        string json = @"{}";

        // Act
        Result result = JsonSerializer.Deserialize<Result>(json, _options)!;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.ResultType.ShouldBe(ResultType.Success);
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.Error.ShouldBeEmpty();
        result.Failures.ShouldBeEmpty();
    }

    [Fact]
    public void Deserialize_EnumAsNumbers_ShouldWorkCorrectly()
    {
        // Arrange
        string json = @"{
            ""error"": ""Numeric enum test"",
            ""failures"": {},
            ""failureType"": 1,
            ""resultType"": 3
        }";

        // Act
        Result result = JsonSerializer.Deserialize<Result>(json, _options)!;

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Error);
        result.ResultType.ShouldBe(ResultType.Error);
        result.Error.ShouldBe("Numeric enum test");
    }

    [Fact]
    public void Write_WithNullResult_ShouldThrowArgumentNullException()
    {
        // Arrange
        ResultJsonConverter converter = new();
        MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            converter.Write(writer, null!, _options));
    }

    [Fact]
    public void Write_WithNullWriter_ShouldThrowArgumentNullException()
    {
        // Arrange
        ResultJsonConverter converter = new();
        Result result = Result.Success();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            converter.Write(null!, result, _options));
    }

    #endregion Edge Case Tests
}