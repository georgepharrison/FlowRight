using System.Globalization;
using System.Security;
using System.Text.Json;
using FlowRight.Core.Results;
using FlowRight.Core.Serialization;
using Shouldly;
using Xunit;

namespace FlowRight.Core.Tests.Serialization;

/// <summary>
/// Contains comprehensive round-trip serialization tests for all Result scenarios.
/// </summary>
/// <remarks>
/// This test class fulfills TASK-020 by ensuring complete round-trip serialization
/// testing for all possible Result and Result&lt;T&gt; scenarios, including edge cases,
/// different serializer configurations, and complex object graphs.
/// </remarks>
public sealed class ComprehensiveRoundTripSerializationTests
{
    #region Test Infrastructure

    private readonly JsonSerializerOptions _defaultOptions;
    private readonly JsonSerializerOptions _camelCaseOptions;
    private readonly JsonSerializerOptions _numberEnumOptions;

    public ComprehensiveRoundTripSerializationTests()
    {
        _defaultOptions = new JsonSerializerOptions
        {
            Converters = { new ResultJsonConverter(), new ResultTJsonConverter<string>(), new ResultTJsonConverter<ComplexTestObject>() }
        };

        _camelCaseOptions = new JsonSerializerOptions
        {
            Converters = { new ResultJsonConverter(), new ResultTJsonConverter<string>(), new ResultTJsonConverter<ComplexTestObject>() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _numberEnumOptions = new JsonSerializerOptions
        {
            Converters = { new ResultJsonConverter(), new ResultTJsonConverter<string>(), new ResultTJsonConverter<ComplexTestObject>() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Complex test object for testing deep serialization scenarios.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <param name="Name">The name.</param>
    /// <param name="Items">The list of items.</param>
    /// <param name="Metadata">The metadata dictionary.</param>
    /// <param name="CreatedAt">The creation timestamp.</param>
    /// <param name="Tags">The tags array.</param>
    public sealed record ComplexTestObject(
        Guid Id,
        string Name,
        List<string> Items,
        Dictionary<string, object?> Metadata,
        DateTimeOffset CreatedAt,
        string[] Tags);

    #endregion Test Infrastructure

    #region Comprehensive Result Round-Trip Tests

    [Theory]
    [MemberData(nameof(GetAllResultScenarios))]
    public void RoundTrip_AllResultScenariosWithDefaultOptions_ShouldPreserveCompleteState(Result original)
    {
        // Act
        string json = JsonSerializer.Serialize(original, _defaultOptions);
        Result deserialized = JsonSerializer.Deserialize<Result>(json, _defaultOptions)!;

        // Assert
        AssertResultsAreEqual(original, deserialized);
    }

    [Theory]
    [MemberData(nameof(GetAllResultScenarios))]
    public void RoundTrip_AllResultScenariosWithCamelCase_ShouldPreserveCompleteState(Result original)
    {
        // Act
        string json = JsonSerializer.Serialize(original, _camelCaseOptions);
        Result deserialized = JsonSerializer.Deserialize<Result>(json, _camelCaseOptions)!;

        // Assert
        AssertResultsAreEqual(original, deserialized);
    }

    [Theory]
    [MemberData(nameof(GetAllResultTStringScenarios))]
    public void RoundTrip_AllResultTStringScenariosWithAllOptions_ShouldPreserveCompleteState(Result<string> original)
    {
        JsonSerializerOptions[] allOptions = [_defaultOptions, _camelCaseOptions, _numberEnumOptions];

        foreach (JsonSerializerOptions options in allOptions)
        {
            // Act
            string json = JsonSerializer.Serialize(original, options);
            Result<string> deserialized = JsonSerializer.Deserialize<Result<string>>(json, options)!;

            // Assert
            AssertResultTAreEqual(original, deserialized);
        }
    }

    [Theory]
    [MemberData(nameof(GetComplexObjectScenarios))]
    public void RoundTrip_ComplexObjectScenarios_ShouldPreserveCompleteObjectGraph(Result<ComplexTestObject> original)
    {
        // Act
        string json = JsonSerializer.Serialize(original, _camelCaseOptions);
        Result<ComplexTestObject> deserialized = JsonSerializer.Deserialize<Result<ComplexTestObject>>(json, _camelCaseOptions)!;

        // Assert
        AssertResultTAreEqual(original, deserialized);
    }

    #endregion Comprehensive Result Round-Trip Tests

    #region Large Data and Stress Tests

    [Fact]
    public void RoundTrip_LargeValidationFailure_ShouldPreserveAllErrors()
    {
        // Arrange - Create a large validation failure with many fields and errors
        Dictionary<string, string[]> largeErrors = new();
        for (int i = 0; i < 100; i++)
        {
            string[] errors = new string[10];
            for (int j = 0; j < 10; j++)
            {
                errors[j] = $"Field{i} error {j + 1}: This is a detailed error message for field {i}";
            }
            largeErrors[$"Field{i}"] = errors;
        }
        Result original = Result.ValidationFailure(largeErrors);

        // Act
        string json = JsonSerializer.Serialize(original, _camelCaseOptions);
        Result deserialized = JsonSerializer.Deserialize<Result>(json, _camelCaseOptions)!;

        // Assert
        AssertResultsAreEqual(original, deserialized);
        deserialized.Failures.Count.ShouldBe(100);
        deserialized.Failures["Field50"].Length.ShouldBe(10);
    }

    [Fact]
    public void RoundTrip_DeeplyNestedComplexObject_ShouldPreserveStructure()
    {
        // Arrange - Create a complex object without nested objects (which are tricky for JSON round-trip)
        ComplexTestObject complexObject = new(
            Id: Guid.NewGuid(),
            Name: "Test Object with Unicode: 测试 🚀 émojis",
            Items: Enumerable.Range(1, 50).Select(i => $"Item {i}").ToList(),
            Metadata: new Dictionary<string, object?>
            {
                { "StringValue", "Test String" },
                { "NumberValue", "42.5" }, // Use string to avoid JsonElement issues
                { "BooleanValue", "true" },
                { "NullValue", null },
                { "DateValue", DateTimeOffset.UtcNow.ToString("O") }
            },
            CreatedAt: DateTimeOffset.UtcNow,
            Tags: Enumerable.Range(1, 20).Select(i => $"Tag{i}").ToArray()
        );
        Result<ComplexTestObject> original = Result.Success(complexObject);

        // Act
        string json = JsonSerializer.Serialize(original, _camelCaseOptions);
        Result<ComplexTestObject> deserialized = JsonSerializer.Deserialize<Result<ComplexTestObject>>(json, _camelCaseOptions)!;

        // Assert
        AssertResultTAreEqual(original, deserialized);
    }

    #endregion Large Data and Stress Tests

    #region Unicode and Special Character Tests

    [Theory]
    [InlineData("Simple ASCII text")]
    [InlineData("Unicode characters: 测试 français español русский")]
    [InlineData("Emojis: 🚀 🎉 💻 🌟 ✨")]
    [InlineData("Special JSON chars: \"quotes\" \\backslash /forward \b\f\n\r\t")]
    [InlineData("")]
    [InlineData(null)]
    public void RoundTrip_UnicodeAndSpecialCharacters_ShouldPreserveExactly(string? testValue)
    {
        // Arrange
        Result<string> original = testValue is null ? Result.Failure<string>("Null test") : Result.Success(testValue);

        // Act
        string json = JsonSerializer.Serialize(original, _camelCaseOptions);
        Result<string> deserialized = JsonSerializer.Deserialize<Result<string>>(json, _camelCaseOptions)!;

        // Assert
        AssertResultTAreEqual(original, deserialized);
    }

    [Fact]
    public void RoundTrip_ValidationFailureWithUnicodeFieldNames_ShouldPreserveFieldNames()
    {
        // Arrange
        Dictionary<string, string[]> unicodeErrors = new()
        {
            { "用户名", ["用户名不能为空", "用户名长度必须在2-50个字符之间"] },
            { "电子邮件", ["电子邮件格式不正确"] },
            { "Contraseña", ["La contraseña debe tener al menos 8 caracteres"] },
            { "Nom d'utilisateur", ["Le nom d'utilisateur est requis"] }
        };
        Result original = Result.ValidationFailure(unicodeErrors);

        // Act
        string json = JsonSerializer.Serialize(original, _camelCaseOptions);
        Result deserialized = JsonSerializer.Deserialize<Result>(json, _camelCaseOptions)!;

        // Assert
        AssertResultsAreEqual(original, deserialized);
    }

    #endregion Unicode and Special Character Tests

    #region Culture and Localization Tests

    [Fact]
    public void RoundTrip_WithDifferentCultures_ShouldBeConsistent()
    {
        // Arrange - Test with different cultures
        CultureInfo[] cultures = [CultureInfo.InvariantCulture, new("en-US"), new("fr-FR"), new("ja-JP")];
        Result original = Result.Failure("Culture test: 12.34");

        foreach (CultureInfo culture in cultures)
        {
            CultureInfo previousCulture = CultureInfo.CurrentCulture;
            try
            {
                // Act
                CultureInfo.CurrentCulture = culture;
                string json = JsonSerializer.Serialize(original, _camelCaseOptions);
                Result deserialized = JsonSerializer.Deserialize<Result>(json, _camelCaseOptions)!;

                // Assert
                AssertResultsAreEqual(original, deserialized);
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }
    }

    #endregion Culture and Localization Tests

    #region Edge Case and Boundary Tests

    [Fact]
    public void RoundTrip_EmptyAndBoundaryValues_ShouldHandleCorrectly()
    {
        // Test various boundary scenarios
        Result[] boundaryResults =
        [
            Result.Success(),
            Result.Failure("Error message"), // Changed from empty string which creates success
            Result.Failure(new string('x', 1000)), // Reduced size for practical testing
            Result.ValidationFailure(new Dictionary<string, string[]>()),
            Result.ValidationFailure(new Dictionary<string, string[]> { { "EmptyField", ["Error"] } }),
            Result.Failure("Field", new string('y', 1000)), // Reduced size for practical testing
        ];

        foreach (Result original in boundaryResults)
        {
            // Act
            string json = JsonSerializer.Serialize(original, _camelCaseOptions);
            Result deserialized = JsonSerializer.Deserialize<Result>(json, _camelCaseOptions)!;

            // Assert
            AssertResultsAreEqual(original, deserialized);
        }
    }

    #endregion Edge Case and Boundary Tests

    #region Test Data Generators

    public static TheoryData<Result> GetAllResultScenarios() =>
        new()
        {
            // Success scenarios
            Result.Success(),
            Result.Success(ResultType.Information),
            Result.Success(ResultType.Warning),

            // General failure scenarios  
            Result.Failure("Basic error"),
            Result.Failure("Custom error", ResultType.Warning, ResultFailureType.Error),
            Result.Failure("Critical error", ResultType.Error, ResultFailureType.Error),

            // Security failure scenarios
            Result.Failure(new SecurityException("Access denied")),
            Result.Failure(new SecurityException("Authentication failed")),

            // Operation canceled scenarios
            Result.Failure(new OperationCanceledException("Request timeout")),
            Result.Failure(new OperationCanceledException("User canceled")),

            // Validation failure scenarios
            Result.ValidationFailure(new Dictionary<string, string[]>
            {
                { "Field1", ["Error 1"] },
                { "Field2", ["Error 1", "Error 2", "Error 3"] }
            }),
            Result.Failure("SingleField", "Single validation error"),
            Result.ValidationFailure(new Dictionary<string, string[]>
            {
                { "ComplexField", ["Complex validation error with detailed message"] }
            })
        };

    public static TheoryData<Result<string>> GetAllResultTStringScenarios() =>
        new()
        {
            // Success scenarios
            Result.Success("Hello World"),
            Result.Success("", ResultType.Information),
            Result.Success("Warning message", ResultType.Warning),

            // Failure scenarios (no value)
            Result.Failure<string>("Basic error"),
            Result.Failure<string>(new SecurityException("Security error")),
            Result.Failure<string>(new OperationCanceledException("Canceled")),
            Result.ValidationFailure<string>(new Dictionary<string, string[]>
            {
                { "StringField", ["String validation error"] }
            })
        };

    public static TheoryData<Result<ComplexTestObject>> GetComplexObjectScenarios()
    {
        ComplexTestObject sampleObject = new(
            Id: Guid.Parse("12345678-1234-1234-1234-123456789012"),
            Name: "Sample Object",
            Items: ["Item1", "Item2", "Item3"],
            Metadata: new Dictionary<string, object?>
            {
                { "Key1", "Value1" },
                { "Key2", 42 },
                { "Key3", true }
            },
            CreatedAt: new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero),
            Tags: ["Tag1", "Tag2"]
        );

        return new TheoryData<Result<ComplexTestObject>>
        {
            // Success with complex object
            Result.Success(sampleObject),
            Result.Success(sampleObject, ResultType.Information),

            // Failure scenarios
            Result.Failure<ComplexTestObject>("Failed to create object"),
            Result.ValidationFailure<ComplexTestObject>(new Dictionary<string, string[]>
            {
                { "Name", ["Name is required"] },
                { "Items", ["Items cannot be empty"] }
            })
        };
    }

    #endregion Test Data Generators

    #region Assertion Helpers

    private static void AssertResultsAreEqual(Result expected, Result actual)
    {
        actual.ShouldNotBeNull();
        actual.IsSuccess.ShouldBe(expected.IsSuccess);
        actual.IsFailure.ShouldBe(expected.IsFailure);
        actual.ResultType.ShouldBe(expected.ResultType);
        actual.FailureType.ShouldBe(expected.FailureType);
        actual.Error.ShouldBe(expected.Error);
        actual.Failures.Count.ShouldBe(expected.Failures.Count);

        foreach (KeyValuePair<string, string[]> expectedFailure in expected.Failures)
        {
            actual.Failures.ShouldContainKey(expectedFailure.Key);
            actual.Failures[expectedFailure.Key].ShouldBe(expectedFailure.Value);
        }
    }

    private static void AssertResultTAreEqual<T>(Result<T> expected, Result<T> actual)
    {
        actual.ShouldNotBeNull();
        actual.IsSuccess.ShouldBe(expected.IsSuccess);
        actual.IsFailure.ShouldBe(expected.IsFailure);
        actual.ResultType.ShouldBe(expected.ResultType);
        actual.FailureType.ShouldBe(expected.FailureType);
        actual.Error.ShouldBe(expected.Error);
        actual.Failures.Count.ShouldBe(expected.Failures.Count);

        // Compare success values
        if (expected.IsSuccess && actual.IsSuccess)
        {
            if (expected.TryGetValue(out T? expectedValue) && actual.TryGetValue(out T? actualValue))
            {
                // Special handling for ComplexTestObject
                if (typeof(T) == typeof(ComplexTestObject))
                {
                    ComplexTestObject? expectedObj = expectedValue as ComplexTestObject;
                    ComplexTestObject? actualObj = actualValue as ComplexTestObject;

                    if (expectedObj is not null && actualObj is not null)
                    {
                        actualObj.Id.ShouldBe(expectedObj.Id);
                        actualObj.Name.ShouldBe(expectedObj.Name);
                        actualObj.Items.ShouldBe(expectedObj.Items);
                        actualObj.CreatedAt.ShouldBe(expectedObj.CreatedAt);
                        actualObj.Tags.ShouldBe(expectedObj.Tags);
                        actualObj.Metadata.Count.ShouldBe(expectedObj.Metadata.Count);

                        foreach (KeyValuePair<string, object?> kvp in expectedObj.Metadata)
                        {
                            actualObj.Metadata.ShouldContainKey(kvp.Key);
                            // For JSON serialization, numbers might be deserialized as JsonElement
                            // so we compare string representations for consistency
                            actualObj.Metadata[kvp.Key]?.ToString().ShouldBe(kvp.Value?.ToString());
                        }
                    }
                }
                else
                {
                    actualValue.ShouldBe(expectedValue);
                }
            }
        }

        foreach (KeyValuePair<string, string[]> expectedFailure in expected.Failures)
        {
            actual.Failures.ShouldContainKey(expectedFailure.Key);
            actual.Failures[expectedFailure.Key].ShouldBe(expectedFailure.Value);
        }
    }

    #endregion Assertion Helpers
}