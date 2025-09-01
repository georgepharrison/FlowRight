using System.Collections.Concurrent;
using System.Security;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using FlowRight.Core.Results;
using FlowRight.Validation.Builders;

namespace FlowRight.Benchmarks;

/// <summary>
/// Comprehensive memory allocation benchmarks for the FlowRight library.
/// </summary>
/// <remarks>
/// <para>
/// This class contains benchmarks for TASK-063: Profile memory allocations.
/// Focuses specifically on identifying memory allocation patterns and ensuring the
/// FlowRight library meets its memory allocation targets:
/// </para>
/// <list type="bullet">
/// <item><description>Success path: 0 bytes target, 24 bytes maximum</description></item>
/// <item><description>Single error: &lt;100 bytes target, 200 bytes maximum</description></item>
/// <item><description>Validation errors: &lt;500 bytes target, 1KB maximum</description></item>
/// <item><description>Serialized JSON: &lt;200 bytes target, 500 bytes maximum</description></item>
/// </list>
/// <para>
/// Includes scenarios for zero-allocation success paths, minimal allocation error paths,
/// collection allocation patterns, string interning opportunities, and GC pressure analysis.
/// </para>
/// </remarks>
[MemoryDiagnoser]
[SimpleJob]
public class MemoryAllocationBenchmarks
{
    #region Test Data Setup
    
    // Pre-allocated test data to avoid measurement interference
    private static readonly string ShortMessage = "Error";
    private static readonly string MediumMessage = "A validation error occurred while processing the request";
    private static readonly string LongMessage = "This is a comprehensive error message that might occur in complex validation scenarios where multiple validation rules fail simultaneously and detailed diagnostic information needs to be provided to both users and system administrators for proper troubleshooting and resolution of the underlying issues that caused the validation failure.";
    
    private static readonly TestModel ValidTestModel = new("John Doe", "john.doe@example.com", 30, ["hobby1", "hobby2"]);
    private static readonly TestModel InvalidTestModel = new("", "invalid-email", -5, []);
    
    private static readonly Dictionary<string, string[]> SingleValidationError = new()
    {
        ["Name"] = ["Name is required"]
    };
    
    private static readonly Dictionary<string, string[]> MultipleValidationErrors = new()
    {
        ["Name"] = ["Name is required", "Name must be at least 2 characters"],
        ["Email"] = ["Email is invalid", "Email format is incorrect"],
        ["Age"] = ["Age must be positive", "Age must be between 0 and 120"],
        ["Hobbies"] = ["At least one hobby is required"]
    };
    
    private static readonly Dictionary<string, string[]> LargeValidationErrors;
    
    // Pre-created results for memory testing
    private static readonly Result SuccessResult = Result.Success();
    private static readonly Result<int> SuccessIntResult = Result.Success(42);
    private static readonly Result<string> SuccessStringResult = Result.Success("Success Value");
    private static readonly Result<TestModel> SuccessModelResult = Result.Success(ValidTestModel);
    
    private static readonly Result FailureResult = Result.Failure(ShortMessage);
    private static readonly Result<int> FailureIntResult = Result.Failure<int>(ShortMessage);
    private static readonly Result<TestModel> FailureModelResult = Result.Failure<TestModel>(ShortMessage);
    
    // JSON serialization options
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
    
    static MemoryAllocationBenchmarks()
    {
        // Create large validation errors for testing
        Dictionary<string, string[]> largeErrors = [];
        for (int i = 1; i <= 20; i++)
        {
            largeErrors[$"Field{i}"] = [$"Error 1 for Field{i}", $"Error 2 for Field{i}"];
        }
        LargeValidationErrors = largeErrors;
    }
    
    /// <summary>
    /// Test model for memory allocation benchmarks.
    /// </summary>
    /// <param name="Name">The name property.</param>
    /// <param name="Email">The email property.</param>
    /// <param name="Age">The age property.</param>
    /// <param name="Hobbies">The hobbies collection.</param>
    public sealed record TestModel(string Name, string Email, int Age, List<string> Hobbies);
    
    #endregion Test Data Setup
    
    #region Zero-Allocation Success Path Benchmarks
    
    /// <summary>
    /// Benchmarks Result.Success() for zero allocations.
    /// Target: 0 bytes, Maximum: 24 bytes
    /// </summary>
    /// <returns>A success Result.</returns>
    [Benchmark(Baseline = true)]
    public Result ZeroAllocationSuccessResult() => Result.Success();
    
    /// <summary>
    /// Benchmarks Result&lt;int&gt;.Success() with value type for zero allocations.
    /// Value types should not allocate on success path.
    /// </summary>
    /// <returns>A success Result&lt;int&gt;.</returns>
    [Benchmark]
    public Result<int> ZeroAllocationSuccessIntResult() => Result.Success(42);
    
    /// <summary>
    /// Benchmarks Result&lt;bool&gt;.Success() with boolean for zero allocations.
    /// </summary>
    /// <returns>A success Result&lt;bool&gt;.</returns>
    [Benchmark]
    public Result<bool> ZeroAllocationSuccessBoolResult() => Result.Success(true);
    
    /// <summary>
    /// Benchmarks Result&lt;DateTime&gt;.Success() with DateTime struct.
    /// </summary>
    /// <returns>A success Result&lt;DateTime&gt;.</returns>
    [Benchmark]
    public Result<DateTime> ZeroAllocationSuccessDateTimeResult() => Result.Success(DateTime.UtcNow);
    
    /// <summary>
    /// Benchmarks Result&lt;Guid&gt;.Success() with Guid struct.
    /// </summary>
    /// <returns>A success Result&lt;Guid&gt;.</returns>
    [Benchmark]
    public Result<Guid> ZeroAllocationSuccessGuidResult() => Result.Success(Guid.NewGuid());
    
    /// <summary>
    /// Benchmarks property access on successful Result (should be zero allocation).
    /// </summary>
    /// <returns>Boolean indicating success.</returns>
    [Benchmark]
    public bool ZeroAllocationPropertyAccess() => SuccessResult.IsSuccess;
    
    /// <summary>
    /// Benchmarks TryGetValue on successful Result&lt;int&gt; (should be zero allocation).
    /// </summary>
    /// <returns>Boolean indicating successful value extraction.</returns>
    [Benchmark]
    public bool ZeroAllocationTryGetValue() => SuccessIntResult.TryGetValue(out int _);
    
    #endregion Zero-Allocation Success Path Benchmarks
    
    #region Minimal Allocation Error Path Benchmarks
    
    /// <summary>
    /// Benchmarks Result.Failure() with short message for minimal allocations.
    /// Target: &lt;100 bytes, Maximum: 200 bytes
    /// </summary>
    /// <returns>A failure Result.</returns>
    [Benchmark]
    public Result MinimalAllocationFailureShort() => Result.Failure(ShortMessage);
    
    /// <summary>
    /// Benchmarks Result.Failure() with medium message.
    /// </summary>
    /// <returns>A failure Result.</returns>
    [Benchmark]
    public Result MinimalAllocationFailureMedium() => Result.Failure(MediumMessage);
    
    /// <summary>
    /// Benchmarks Result.Failure() with long message to test string allocation impact.
    /// </summary>
    /// <returns>A failure Result.</returns>
    [Benchmark]
    public Result FailureLongMessage() => Result.Failure(LongMessage);
    
    /// <summary>
    /// Benchmarks Result&lt;int&gt;.Failure() with short message.
    /// </summary>
    /// <returns>A failure Result&lt;int&gt;.</returns>
    [Benchmark]
    public Result<int> MinimalAllocationFailureGeneric() => Result.Failure<int>(ShortMessage);
    
    /// <summary>
    /// Benchmarks Result.NotFound() specialized failure.
    /// </summary>
    /// <returns>A not found failure Result.</returns>
    [Benchmark]
    public Result MinimalAllocationNotFound() => Result.NotFound("Resource");
    
    /// <summary>
    /// Benchmarks Result.ServerError() specialized failure.
    /// </summary>
    /// <returns>A server error failure Result.</returns>
    [Benchmark]
    public Result MinimalAllocationServerError() => Result.ServerError("Server error");
    
    /// <summary>
    /// Benchmarks Result.Failure() with SecurityException.
    /// </summary>
    /// <returns>A security failure Result.</returns>
    [Benchmark]
    public Result FailureWithSecurityException() => Result.Failure(new SecurityException("Access denied"));
    
    /// <summary>
    /// Benchmarks Result.Failure() with OperationCanceledException.
    /// </summary>
    /// <returns>A cancellation failure Result.</returns>
    [Benchmark]
    public Result FailureWithCancellationException() => Result.Failure(new OperationCanceledException("Cancelled"));
    
    #endregion Minimal Allocation Error Path Benchmarks
    
    #region Validation Error Memory Patterns
    
    /// <summary>
    /// Benchmarks Result.ValidationFailure() with single error.
    /// Target: &lt;100 bytes, Maximum: 200 bytes
    /// </summary>
    /// <returns>A validation failure Result.</returns>
    [Benchmark]
    public Result ValidationSingleError() => Result.ValidationFailure(SingleValidationError);
    
    /// <summary>
    /// Benchmarks Result.ValidationFailure() with multiple errors.
    /// Target: &lt;500 bytes, Maximum: 1KB
    /// </summary>
    /// <returns>A validation failure Result.</returns>
    [Benchmark]
    public Result ValidationMultipleErrors() => Result.ValidationFailure(MultipleValidationErrors);
    
    /// <summary>
    /// Benchmarks Result.ValidationFailure() with large error dictionary.
    /// Tests upper bounds of validation error memory usage.
    /// </summary>
    /// <returns>A validation failure Result.</returns>
    [Benchmark]
    public Result ValidationLargeErrors() => Result.ValidationFailure(LargeValidationErrors);
    
    /// <summary>
    /// Benchmarks ValidationBuilder with single rule success.
    /// Target: &lt;100 bytes, Maximum: 200 bytes
    /// </summary>
    /// <returns>A validation Result.</returns>
    [Benchmark]
    public Result<TestModel> ValidationBuilderSingleRuleSuccess()
    {
        return new ValidationBuilder<TestModel>()
            .RuleFor(x => x.Name, ValidTestModel.Name)
                .NotEmpty()
            .Build(() => ValidTestModel);
    }
    
    /// <summary>
    /// Benchmarks ValidationBuilder with single rule failure.
    /// </summary>
    /// <returns>A validation Result.</returns>
    [Benchmark]
    public Result<TestModel> ValidationBuilderSingleRuleFailure()
    {
        return new ValidationBuilder<TestModel>()
            .RuleFor(x => x.Name, InvalidTestModel.Name)
                .NotEmpty()
            .Build(() => InvalidTestModel);
    }
    
    /// <summary>
    /// Benchmarks ValidationBuilder with multiple rules success.
    /// </summary>
    /// <returns>A validation Result.</returns>
    [Benchmark]
    public Result<TestModel> ValidationBuilderMultiRuleSuccess()
    {
        return new ValidationBuilder<TestModel>()
            .RuleFor(x => x.Name, ValidTestModel.Name)
                .NotEmpty()
                .MinimumLength(2)
            .RuleFor(x => x.Email, ValidTestModel.Email)
                .NotEmpty()
                .EmailAddress()
            .RuleFor(x => x.Age, ValidTestModel.Age)
                .GreaterThan(0)
            .Build(() => ValidTestModel);
    }
    
    /// <summary>
    /// Benchmarks ValidationBuilder with multiple rules failure.
    /// Target: &lt;500 bytes, Maximum: 1KB
    /// </summary>
    /// <returns>A validation Result.</returns>
    [Benchmark]
    public Result<TestModel> ValidationBuilderMultiRuleFailure()
    {
        return new ValidationBuilder<TestModel>()
            .RuleFor(x => x.Name, InvalidTestModel.Name)
                .NotEmpty()
                .MinimumLength(2)
            .RuleFor(x => x.Email, InvalidTestModel.Email)
                .NotEmpty()
                .EmailAddress()
            .RuleFor(x => x.Age, InvalidTestModel.Age)
                .GreaterThan(0)
            .Build(() => InvalidTestModel);
    }
    
    #endregion Validation Error Memory Patterns
    
    #region Collection Allocation Patterns
    
    /// <summary>
    /// Benchmarks Result.Combine() with successful results.
    /// Tests collection allocation patterns in result combining.
    /// </summary>
    /// <returns>A combined Result.</returns>
    [Benchmark]
    public Result CombineSuccessResults()
    {
        return Result.Combine(
            Result.Success(),
            Result.Success(),
            Result.Success()
        );
    }
    
    /// <summary>
    /// Benchmarks Result.Combine() with mixed results.
    /// Tests error collection allocation patterns.
    /// </summary>
    /// <returns>A combined Result.</returns>
    [Benchmark]
    public Result CombineMixedResults()
    {
        return Result.Combine(
            Result.Success(),
            Result.Failure("Error 1"),
            Result.Success(),
            Result.Failure("Error 2")
        );
    }
    
    /// <summary>
    /// Benchmarks Result&lt;List&gt;.Success() with collection.
    /// Tests reference type allocation patterns.
    /// </summary>
    /// <returns>A Result with List value.</returns>
    [Benchmark]
    public Result<List<string>> CollectionSuccessResult()
    {
        List<string> collection = ["item1", "item2", "item3"];
        return Result.Success(collection);
    }
    
    /// <summary>
    /// Benchmarks Result&lt;List&gt;.Success() with empty collection.
    /// </summary>
    /// <returns>A Result with empty List.</returns>
    [Benchmark]
    public Result<List<string>> EmptyCollectionResult()
    {
        List<string> emptyCollection = [];
        return Result.Success(emptyCollection);
    }
    
    /// <summary>
    /// Benchmarks Result&lt;List&gt;.Success() with large collection.
    /// Tests impact of large reference type allocations.
    /// </summary>
    /// <returns>A Result with large List.</returns>
    [Benchmark]
    public Result<List<int>> LargeCollectionResult()
    {
        List<int> largeCollection = Enumerable.Range(1, 100).ToList();
        return Result.Success(largeCollection);
    }
    
    #endregion Collection Allocation Patterns
    
    #region String Allocation Patterns
    
    /// <summary>
    /// Benchmarks error message string allocation patterns.
    /// Tests string interning opportunities for common errors.
    /// </summary>
    /// <returns>A failure Result.</returns>
    [Benchmark]
    public Result CommonErrorMessages()
    {
        // Test common error messages that could benefit from interning
        return Result.Failure("Value cannot be null");
    }
    
    /// <summary>
    /// Benchmarks formatted error message allocation.
    /// Tests string interpolation vs concatenation memory impact.
    /// </summary>
    /// <returns>A failure Result.</returns>
    [Benchmark]
    public Result FormattedErrorMessage()
    {
        string fieldName = "UserName";
        int minLength = 3;
        return Result.Failure($"{fieldName} must be at least {minLength} characters long");
    }
    
    /// <summary>
    /// Benchmarks string concatenation error messages.
    /// </summary>
    /// <returns>A failure Result.</returns>
    [Benchmark]
    public Result ConcatenatedErrorMessage()
    {
        string fieldName = "UserName";
        int minLength = 3;
        return Result.Failure(fieldName + " must be at least " + minLength + " characters long");
    }
    
    /// <summary>
    /// Benchmarks StringBuilder error message creation.
    /// Tests StringBuilder vs string interpolation for complex messages.
    /// </summary>
    /// <returns>A failure Result.</returns>
    [Benchmark]
    public Result StringBuilderErrorMessage()
    {
        System.Text.StringBuilder builder = new();
        builder.Append("Multiple validation errors occurred: ");
        builder.Append("Name is required, ");
        builder.Append("Email is invalid, ");
        builder.Append("Age must be positive");
        return Result.Failure(builder.ToString());
    }
    
    #endregion String Allocation Patterns
    
    #region Serialization Memory Patterns
    
    /// <summary>
    /// Benchmarks JSON serialization of successful Result.
    /// Target: &lt;200 bytes, Maximum: 500 bytes
    /// </summary>
    /// <returns>JSON string.</returns>
    [Benchmark]
    public string SerializeSuccessResultMemory() => JsonSerializer.Serialize(SuccessResult, JsonOptions);
    
    /// <summary>
    /// Benchmarks JSON serialization of failure Result.
    /// </summary>
    /// <returns>JSON string.</returns>
    [Benchmark]
    public string SerializeFailureResultMemory() => JsonSerializer.Serialize(FailureResult, JsonOptions);
    
    /// <summary>
    /// Benchmarks JSON serialization of Result&lt;int&gt; success.
    /// </summary>
    /// <returns>JSON string.</returns>
    [Benchmark]
    public string SerializeSuccessIntResultMemory() => JsonSerializer.Serialize(SuccessIntResult, JsonOptions);
    
    /// <summary>
    /// Benchmarks JSON serialization of Result&lt;TestModel&gt;.
    /// Tests complex object serialization memory usage.
    /// </summary>
    /// <returns>JSON string.</returns>
    [Benchmark]
    public string SerializeComplexResultMemory() => JsonSerializer.Serialize(SuccessModelResult, JsonOptions);
    
    /// <summary>
    /// Benchmarks JSON serialization of validation failure.
    /// Tests validation error serialization memory patterns.
    /// </summary>
    /// <returns>JSON string.</returns>
    [Benchmark]
    public string SerializeValidationFailureMemory()
    {
        Result validationResult = Result.ValidationFailure(MultipleValidationErrors);
        return JsonSerializer.Serialize(validationResult, JsonOptions);
    }
    
    /// <summary>
    /// Benchmarks JSON deserialization memory patterns.
    /// Tests memory allocation during deserialization.
    /// </summary>
    /// <returns>Deserialized Result.</returns>
    [Benchmark]
    public Result<int>? DeserializeResultMemory()
    {
        string json = """{"isSuccess":true,"value":42,"error":null,"resultType":"Success","resultFailureType":"None","validationErrors":null}""";
        return JsonSerializer.Deserialize<Result<int>>(json, JsonOptions);
    }
    
    #endregion Serialization Memory Patterns
    
    #region Advanced Memory Profiling Scenarios
    
    /// <summary>
    /// Benchmarks nested Result operations for memory allocation patterns.
    /// Tests cumulative memory impact of chained operations.
    /// </summary>
    /// <returns>Final nested Result.</returns>
    [Benchmark]
    public Result<string> NestedOperationsMemory()
    {
        Result<int> step1 = Result.Success(10);
        Result<int> step2 = step1.IsSuccess ? Result.Success(step1.TryGetValue(out int val) ? val * 2 : 0) : Result.Failure<int>("Step 1 failed");
        Result<string> step3 = step2.IsSuccess ? Result.Success(step2.TryGetValue(out int val2) ? val2.ToString() : string.Empty) : Result.Failure<string>("Step 2 failed");
        return step3;
    }
    
    /// <summary>
    /// Benchmarks Result operations in concurrent scenarios.
    /// Tests memory patterns under concurrent access.
    /// </summary>
    /// <returns>Collection of concurrent Results.</returns>
    [Benchmark]
    public List<Result<int>> ConcurrentOperationsMemory()
    {
        ConcurrentBag<Result<int>> results = [];
        
        Parallel.For(0, 10, i =>
        {
            Result<int> result = i % 2 == 0 
                ? Result.Success(i) 
                : Result.Failure<int>($"Error for {i}");
            results.Add(result);
        });
        
        return [.. results];
    }
    
    /// <summary>
    /// Benchmarks memory pressure scenarios with many Results.
    /// Tests GC pressure from large numbers of Result allocations.
    /// </summary>
    /// <returns>Collection of Results.</returns>
    [Benchmark]
    public List<Result> MemoryPressureScenario()
    {
        List<Result> results = [];
        
        for (int i = 0; i < 100; i++)
        {
            Result result = (i % 3) switch
            {
                0 => Result.Success(),
                1 => Result.Failure($"Error {i}"),
                _ => Result.ValidationFailure(new Dictionary<string, string[]> { [$"Field{i}"] = [$"Error for field {i}"] })
            };
            results.Add(result);
        }
        
        return results;
    }
    
    /// <summary>
    /// Benchmarks object pooling scenario for Result creation.
    /// Tests potential for object pooling optimizations.
    /// </summary>
    /// <returns>Pooled Result instance.</returns>
    [Benchmark]
    public Result ObjectPoolingScenario()
    {
        // Simulate reusable Result pattern
        Result result = Result.Success();
        
        // Multiple operations using the same pattern
        for (int i = 0; i < 5; i++)
        {
            if (!result.IsSuccess)
            {
                break;
            }
            // Simulate some operation that might fail
            result = i == 3 ? Result.Failure("Simulated failure") : Result.Success();
        }
        
        return result;
    }
    
    /// <summary>
    /// Benchmarks async operation memory patterns.
    /// Tests Task allocation patterns with Result.
    /// </summary>
    /// <returns>Task with Result.</returns>
    [Benchmark]
    public async Task<Result<int>> AsyncOperationMemory()
    {
        await Task.Delay(1); // Minimal async operation
        return Result.Success(42);
    }
    
    /// <summary>
    /// Benchmarks ValueTask operation memory patterns.
    /// Tests ValueTask vs Task allocation differences.
    /// </summary>
    /// <returns>ValueTask with Result.</returns>
    [Benchmark]
    public ValueTask<Result<int>> ValueTaskOperationMemory()
    {
        return ValueTask.FromResult(Result.Success(42));
    }
    
    #endregion Advanced Memory Profiling Scenarios
}