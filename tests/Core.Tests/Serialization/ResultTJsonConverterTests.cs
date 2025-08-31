using System.Security;
using System.Text.Json;
using FlowRight.Core.Results;
using FlowRight.Core.Serialization;
using Shouldly;
using Xunit;

namespace FlowRight.Core.Tests.Serialization;

/// <summary>
/// Contains comprehensive tests for the <see cref="ResultTJsonConverter{T}"/> class.
/// </summary>
/// <remarks>
/// These tests verify that the JsonConverter can properly serialize and deserialize
/// all types of Result&lt;T&gt; objects while maintaining immutability and preserving all
/// failure information as well as success values across round-trip operations.
/// </remarks>
public sealed class ResultTJsonConverterTests
{
    #region Test Infrastructure

    private readonly JsonSerializerOptions _options;

    public ResultTJsonConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new ResultTJsonConverter<string>(), new ResultTJsonConverter<int>(), new ResultTJsonConverter<TestUser>() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public sealed record TestUser(string Name, string Email, int Age);

    #endregion Test Infrastructure

    #region CanConvert Tests

    [Fact]
    public void CanConvert_WithResultTType_ShouldReturnTrue()
    {
        // Arrange
        ResultTJsonConverter<string> converter = new();

        // Act
        bool canConvert = converter.CanConvert(typeof(Result<string>));

        // Assert
        canConvert.ShouldBeTrue();
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    [InlineData(typeof(Result))]
    [InlineData(typeof(object))]
    [InlineData(typeof(Result<int>))] // Different generic type
    public void CanConvert_WithNonMatchingResultTType_ShouldReturnFalse(Type type)
    {
        // Arrange
        ResultTJsonConverter<string> converter = new();

        // Act
        bool canConvert = converter.CanConvert(type);

        // Assert
        canConvert.ShouldBeFalse();
    }

    #endregion CanConvert Tests

    #region Success Serialization Tests

    [Fact]
    public void Serialize_SuccessResultWithStringValue_ShouldProduceCorrectJson()
    {
        // Arrange
        Result<string> result = Result.Success("Hello World");

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldNotBeNullOrWhiteSpace();
        json.ShouldContain("\"value\":\"Hello World\"");
        json.ShouldContain("\"error\":\"\"");
        json.ShouldContain("\"failures\":{}");
        json.ShouldContain("\"failureType\":\"None\"");
        json.ShouldContain("\"resultType\":\"Success\"");
    }

    [Fact]
    public void Serialize_SuccessResultWithIntValue_ShouldProduceCorrectJson()
    {
        // Arrange
        Result<int> result = Result.Success(42);

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"value\":42");
        json.ShouldContain("\"error\":\"\"");
        json.ShouldContain("\"failures\":{}");
        json.ShouldContain("\"failureType\":\"None\"");
        json.ShouldContain("\"resultType\":\"Success\"");
    }

    [Fact]
    public void Serialize_SuccessResultWithComplexObject_ShouldProduceCorrectJson()
    {
        // Arrange
        TestUser user = new("John Doe", "john@example.com", 30);
        Result<TestUser> result = Result.Success(user);

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"name\":\"John Doe\"");
        json.ShouldContain("\"email\":\"john@example.com\"");
        json.ShouldContain("\"age\":30");
        json.ShouldContain("\"error\":\"\"");
        json.ShouldContain("\"failureType\":\"None\"");
        json.ShouldContain("\"resultType\":\"Success\"");
    }

    [Fact]
    public void Serialize_InformationResultWithValue_ShouldProduceCorrectJson()
    {
        // Arrange
        Result<string> result = Result.Success("Info message", ResultType.Information);

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"value\":\"Info message\"");
        json.ShouldContain("\"resultType\":\"Information\"");
        json.ShouldContain("\"failureType\":\"None\"");
        json.ShouldContain("\"error\":\"\"");
    }

    [Fact]
    public void Deserialize_SuccessStringJson_ShouldCreateCorrectResult()
    {
        // Arrange
        string json = @"{
            ""value"": ""Test Value"",
            ""error"": """",
            ""failures"": {},
            ""failureType"": ""None"",
            ""resultType"": ""Success""
        }";

        // Act
        Result<string> result = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.ResultType.ShouldBe(ResultType.Success);
        result.FailureType.ShouldBe(ResultFailureType.None);
        result.Error.ShouldBeEmpty();
        result.Failures.ShouldBeEmpty();
        result.TryGetValue(out string? value).ShouldBeTrue();
        value.ShouldBe("Test Value");
    }

    #endregion Success Serialization Tests

    #region General Failure Serialization Tests

    [Fact]
    public void Serialize_GeneralFailure_ShouldProduceCorrectJson()
    {
        // Arrange
        Result<string> result = Result.Failure<string>("Something went wrong");

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"error\":\"Something went wrong\"");
        json.ShouldContain("\"failureType\":\"Error\"");
        json.ShouldContain("\"resultType\":\"Error\"");
        json.ShouldContain("\"failures\":{}");
        json.ShouldNotContain("\"value\":");
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
        Result<string> result = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        result.ResultType.ShouldBe(ResultType.Error);
        result.FailureType.ShouldBe(ResultFailureType.Error);
        result.Error.ShouldBe("Database connection failed");
        result.Failures.ShouldBeEmpty();
        result.TryGetValue(out string? value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    #endregion General Failure Serialization Tests

    #region Security Failure Serialization Tests

    [Fact]
    public void Serialize_SecurityFailure_ShouldProduceCorrectJson()
    {
        // Arrange
        SecurityException ex = new("Access denied");
        Result<int> result = Result.Failure<int>(ex);

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"error\":\"Access denied\"");
        json.ShouldContain("\"failureType\":\"Security\"");
        json.ShouldContain("\"resultType\":\"Error\"");
        json.ShouldContain("\"failures\":{}");
        json.ShouldNotContain("\"value\":");
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
        Result<int> result = JsonSerializer.Deserialize<Result<int>>(json, _options)!;

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ResultType.ShouldBe(ResultType.Error);
        result.FailureType.ShouldBe(ResultFailureType.Security);
        result.Error.ShouldBe("Unauthorized access attempt");
        result.TryGetValue(out int value).ShouldBeFalse();
        value.ShouldBe(default(int));
    }

    #endregion Security Failure Serialization Tests

    #region Operation Canceled Serialization Tests

    [Fact]
    public void Serialize_OperationCanceledFailure_ShouldProduceCorrectJson()
    {
        // Arrange
        OperationCanceledException ex = new("Operation was cancelled");
        Result<string> result = Result.Failure<string>(ex);

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"error\":\"Operation was cancelled\"");
        json.ShouldContain("\"failureType\":\"OperationCanceled\"");
        json.ShouldContain("\"resultType\":\"Warning\"");
        json.ShouldContain("\"failures\":{}");
        json.ShouldNotContain("\"value\":");
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
        Result<string> result = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

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
        Result<TestUser> result = Result.ValidationFailure<TestUser>(errors);

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"failureType\":\"Validation\"");
        json.ShouldContain("\"resultType\":\"Error\"");
        json.ShouldContain("\"Name\"");
        json.ShouldContain("\"Name is required\"");
        json.ShouldContain("\"Email\"");
        json.ShouldContain("\"Email is required\"");
        json.ShouldNotContain("\"value\":");
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
        Result<TestUser> result = JsonSerializer.Deserialize<Result<TestUser>>(json, _options)!;

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.ResultType.ShouldBe(ResultType.Error);
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldNotBeEmpty();
        result.Failures.Count.ShouldBe(2);
        result.Failures["Name"].ShouldBe(["Name is required"]);
        result.Failures["Email"].ShouldBe(["Email format is invalid"]);
        result.TryGetValue(out TestUser? value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    #endregion Validation Failure Serialization Tests

    #region Round-trip Serialization Tests

    [Theory]
    [MemberData(nameof(GetResultTestCases))]
    public void RoundTrip_AllResultTypes_ShouldPreserveOriginalState(Result<string> originalResult)
    {
        // Act
        string json = JsonSerializer.Serialize(originalResult, _options);
        Result<string> deserializedResult = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

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

        // Test success value
        bool originalHasValue = originalResult.TryGetValue(out string? originalValue);
        bool deserializedHasValue = deserializedResult.TryGetValue(out string? deserializedValue);

        originalHasValue.ShouldBe(deserializedHasValue);
        originalValue.ShouldBe(deserializedValue);
    }

    [Theory]
    [MemberData(nameof(GetComplexResultTestCases))]
    public void RoundTrip_ComplexObjectResults_ShouldPreserveOriginalState(Result<TestUser> originalResult)
    {
        // Act
        string json = JsonSerializer.Serialize(originalResult, _options);
        Result<TestUser> deserializedResult = JsonSerializer.Deserialize<Result<TestUser>>(json, _options)!;

        // Assert
        deserializedResult.ShouldNotBeNull();
        deserializedResult.IsSuccess.ShouldBe(originalResult.IsSuccess);
        deserializedResult.ResultType.ShouldBe(originalResult.ResultType);
        deserializedResult.FailureType.ShouldBe(originalResult.FailureType);

        // Test success value for complex objects
        bool originalHasValue = originalResult.TryGetValue(out TestUser? originalValue);
        bool deserializedHasValue = deserializedResult.TryGetValue(out TestUser? deserializedValue);

        originalHasValue.ShouldBe(deserializedHasValue);
        if (originalHasValue)
        {
            deserializedValue.ShouldNotBeNull();
            deserializedValue.Name.ShouldBe(originalValue!.Name);
            deserializedValue.Email.ShouldBe(originalValue.Email);
            deserializedValue.Age.ShouldBe(originalValue.Age);
        }
    }

    public static TheoryData<Result<string>> GetResultTestCases() =>
        new()
        {
            Result.Success("Hello World"),
            Result.Success("Info message", ResultType.Information),
            Result.Failure<string>("General error"),
            Result.Failure<string>("Custom error", ResultType.Warning, ResultFailureType.Error),
            Result.Failure<string>(new SecurityException("Security violation")),
            Result.Failure<string>(new OperationCanceledException("Timeout occurred")),
            Result.ValidationFailure<string>(new Dictionary<string, string[]>
            {
                { "Field1", ["Error 1", "Error 2"] },
                { "Field2", ["Error 3"] }
            }),
            Result.Failure<string>("SingleField", "Single field error")
        };

    public static TheoryData<Result<TestUser>> GetComplexResultTestCases() =>
        new()
        {
            Result.Success(new TestUser("John", "john@test.com", 25)),
            Result.Failure<TestUser>("User not found"),
            Result.ValidationFailure<TestUser>(new Dictionary<string, string[]>
            {
                { "Name", ["Name is required"] },
                { "Email", ["Invalid email format"] }
            })
        };

    #endregion Round-trip Serialization Tests

    #region Edge Case Tests

    [Fact]
    public void Deserialize_EmptyJson_ShouldThrowJsonException()
    {
        // Arrange
        string json = "";

        // Act & Assert
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Result<string>>(json, _options));
    }

    [Fact]
    public void Deserialize_InvalidJson_ShouldThrowJsonException()
    {
        // Arrange
        string json = "{ invalid json }";

        // Act & Assert
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Result<string>>(json, _options));
    }

    [Fact]
    public void Deserialize_JsonArray_ShouldThrowJsonException()
    {
        // Arrange
        string json = "[]";

        // Act & Assert
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Result<string>>(json, _options));
    }

    [Fact]
    public void Deserialize_JsonWithMissingProperties_ShouldCreateDefaultResult()
    {
        // Arrange
        string json = @"{}";

        // Act
        Result<string> result = JsonSerializer.Deserialize<Result<string>>(json, _options)!;

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue(); // No value provided, so it should be a failure
        result.Error.ShouldBe("No value provided for Result<T>");
    }

    [Fact]
    public void Serialize_NullableStringValue_ShouldProduceCorrectJson()
    {
        // Arrange - Create a success result with empty string instead of null
        Result<string> result = Result.Success(string.Empty);

        // Act
        string json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.ShouldContain("\"value\":\"\"");
        json.ShouldContain("\"resultType\":\"Success\"");
    }

    [Fact]
    public void Write_WithNullResult_ShouldThrowArgumentNullException()
    {
        // Arrange
        ResultTJsonConverter<string> converter = new();
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
        ResultTJsonConverter<string> converter = new();
        Result<string> result = Result.Success("test");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            converter.Write(null!, result, _options));
    }

    #endregion Edge Case Tests
}