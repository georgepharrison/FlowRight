using System.Security;
using BenchmarkDotNet.Attributes;
using FlowRight.Core.Results;

namespace FlowRight.Benchmarks;

/// <summary>
/// Comprehensive benchmarks for core Result pattern operations.
/// </summary>
/// <remarks>
/// <para>
/// This class contains benchmarks for TASK-061: Benchmark Result creation and operations.
/// Measures performance against the following targets:
/// </para>
/// <list type="bullet">
/// <item><description>Result.Success(): &lt;10ns target, &lt;20ns acceptable</description></item>
/// <item><description>Result.Failure(): &lt;50ns target, &lt;100ns acceptable</description></item>
/// <item><description>Match operation: &lt;20ns target, &lt;50ns acceptable</description></item>
/// </list>
/// <para>
/// Includes comprehensive test scenarios for Result creation, operations, comparisons,
/// and advanced scenarios like nested operations and implicit conversions.
/// </para>
/// </remarks>
[MemoryDiagnoser]
[SimpleJob]
public class ResultBenchmarks
{
    #region Test Data Setup

    // Basic test data
    private const string ShortErrorMessage = "Error";
    private const string MediumErrorMessage = "A medium length error message for testing";
    private const string LongErrorMessage = "This is a very long error message that might be used in real-world scenarios where detailed error information needs to be provided to users or logged for debugging purposes. It contains multiple sentences and substantial text content.";
    private const int TestIntValue = 42;
    private const string TestStringValue = "Test String Value";
    private readonly DateTime _testDateValue = new(2024, 1, 15, 10, 30, 45);
    private readonly Guid _testGuidValue = Guid.Parse("12345678-1234-5678-9abc-123456789012");
    
    // Complex test object
    private readonly TestObject _testComplexObject = new("Complex Object", 12345, ["Item1", "Item2", "Item3"]);
    
    // Large collection for testing
    private readonly List<string> _largeStringCollection = Enumerable.Range(1, 1000).Select(i => $"Item_{i:D4}").ToList();
    
    // Validation errors for testing
    private readonly Dictionary<string, string[]> _singleValidationError = new()
    {
        ["Name"] = ["Name is required"]
    };
    
    private readonly Dictionary<string, string[]> _multipleValidationErrors = new()
    {
        ["Name"] = ["Name is required", "Name must be at least 3 characters"],
        ["Email"] = ["Email is invalid"],
        ["Age"] = ["Age must be between 18 and 100"]
    };
    
    // Pre-created results for operation benchmarks
    private readonly Result _successResult = Result.Success();
    private readonly Result _failureResult = Result.Failure(ShortErrorMessage);
    private readonly Result<int> _successIntResult = Result.Success(42);
    private readonly Result<int> _failureIntResult = Result.Failure<int>(ShortErrorMessage);
    private readonly Result<string> _successStringResult = Result.Success("Test Value");
    private readonly Result<string> _failureStringResult = Result.Failure<string>(ShortErrorMessage);
    
    /// <summary>
    /// Test object for complex benchmarking scenarios.
    /// </summary>
    public sealed record TestObject(string Name, int Value, List<string> Items);
    
    #endregion Test Data Setup
    
    #region Result Creation Benchmarks
    
    /// <summary>
    /// Benchmarks Result.Success() creation performance.
    /// Target: &lt;10ns, Acceptable: &lt;20ns
    /// </summary>
    /// <returns>A success Result.</returns>
    [Benchmark(Baseline = true)]
    public Result CreateSuccessResult() => Result.Success();
    
    /// <summary>
    /// Benchmarks Result.Success() with specific ResultType.
    /// </summary>
    /// <returns>A success Result with Information type.</returns>
    [Benchmark]
    public Result CreateSuccessResultWithType() => Result.Success(ResultType.Information);
    
    /// <summary>
    /// Benchmarks Result&lt;int&gt;.Success() creation performance.
    /// </summary>
    /// <returns>A success Result&lt;int&gt;.</returns>
    [Benchmark]
    public Result<int> CreateSuccessIntResult() => Result.Success(TestIntValue);
    
    /// <summary>
    /// Benchmarks Result&lt;string&gt;.Success() creation performance.
    /// </summary>
    /// <returns>A success Result&lt;string&gt;.</returns>
    [Benchmark]
    public Result<string> CreateSuccessStringResult() => Result.Success(TestStringValue);
    
    /// <summary>
    /// Benchmarks Result&lt;DateTime&gt;.Success() creation performance.
    /// </summary>
    /// <returns>A success Result&lt;DateTime&gt;.</returns>
    [Benchmark]
    public Result<DateTime> CreateSuccessDateTimeResult() => Result.Success(_testDateValue);
    
    /// <summary>
    /// Benchmarks Result&lt;Guid&gt;.Success() creation performance.
    /// </summary>
    /// <returns>A success Result&lt;Guid&gt;.</returns>
    [Benchmark]
    public Result<Guid> CreateSuccessGuidResult() => Result.Success(_testGuidValue);
    
    /// <summary>
    /// Benchmarks Result&lt;T&gt;.Success() with complex object.
    /// </summary>
    /// <returns>A success Result with complex object.</returns>
    [Benchmark]
    public Result<TestObject> CreateSuccessComplexObjectResult() => Result.Success(_testComplexObject);
    
    /// <summary>
    /// Benchmarks Result&lt;T&gt;.Success() with large collection.
    /// </summary>
    /// <returns>A success Result with large collection.</returns>
    [Benchmark]
    public Result<List<string>> CreateSuccessLargeCollectionResult() => Result.Success(_largeStringCollection);
    
    /// <summary>
    /// Benchmarks Result.Failure() creation performance with short error message.
    /// Target: &lt;50ns, Acceptable: &lt;100ns
    /// </summary>
    /// <returns>A failure Result.</returns>
    [Benchmark]
    public Result CreateFailureResultShortMessage() => Result.Failure(ShortErrorMessage);
    
    /// <summary>
    /// Benchmarks Result.Failure() creation performance with medium error message.
    /// </summary>
    /// <returns>A failure Result.</returns>
    [Benchmark]
    public Result CreateFailureResultMediumMessage() => Result.Failure(MediumErrorMessage);
    
    /// <summary>
    /// Benchmarks Result.Failure() creation performance with long error message.
    /// </summary>
    /// <returns>A failure Result.</returns>
    [Benchmark]
    public Result CreateFailureResultLongMessage() => Result.Failure(LongErrorMessage);
    
    /// <summary>
    /// Benchmarks Result&lt;T&gt;.Failure() creation performance.
    /// </summary>
    /// <returns>A failure Result&lt;int&gt;.</returns>
    [Benchmark]
    public Result<int> CreateFailureIntResult() => Result.Failure<int>(ShortErrorMessage);
    
    /// <summary>
    /// Benchmarks Result&lt;T&gt;.Failure() creation performance with complex type.
    /// </summary>
    /// <returns>A failure Result&lt;TestObject&gt;.</returns>
    [Benchmark]
    public Result<TestObject> CreateFailureComplexResult() => Result.Failure<TestObject>(ShortErrorMessage);
    
    /// <summary>
    /// Benchmarks Result.Failure() with single validation error.
    /// </summary>
    /// <returns>A validation failure Result.</returns>
    [Benchmark]
    public Result CreateSingleValidationFailure() => Result.ValidationFailure(_singleValidationError);
    
    /// <summary>
    /// Benchmarks Result.Failure() with multiple validation errors.
    /// </summary>
    /// <returns>A validation failure Result with multiple errors.</returns>
    [Benchmark]
    public Result CreateMultipleValidationFailures() => Result.ValidationFailure(_multipleValidationErrors);
    
    /// <summary>
    /// Benchmarks Result.Failure() with SecurityException.
    /// </summary>
    /// <returns>A security failure Result.</returns>
    [Benchmark]
    public Result CreateSecurityFailure() => Result.Failure(new SecurityException("Access denied"));
    
    /// <summary>
    /// Benchmarks Result.Failure() with OperationCanceledException.
    /// </summary>
    /// <returns>A cancellation failure Result.</returns>
    [Benchmark]
    public Result CreateCancellationFailure() => Result.Failure(new OperationCanceledException("Operation was cancelled"));
    
    /// <summary>
    /// Benchmarks Result.NotFound() creation performance.
    /// </summary>
    /// <returns>A not found failure Result.</returns>
    [Benchmark]
    public Result CreateNotFoundFailure() => Result.NotFound("Resource");
    
    /// <summary>
    /// Benchmarks Result.ServerError() creation performance.
    /// </summary>
    /// <returns>A server error failure Result.</returns>
    [Benchmark]
    public Result CreateServerErrorFailure() => Result.ServerError("Internal server error");
    
    #endregion Result Creation Benchmarks
    
    #region Result Operation Benchmarks
    
    /// <summary>
    /// Benchmarks Match operation on successful Result.
    /// Target: &lt;20ns, Acceptable: &lt;50ns
    /// </summary>
    /// <returns>The matched result value.</returns>
    [Benchmark]
    public string MatchSuccessOperation()
    {
        return _successIntResult.Match(
            value => $"Success: {value}",
            error => $"Failure: {error}"
        );
    }
    
    /// <summary>
    /// Benchmarks Match operation on failed Result.
    /// </summary>
    /// <returns>The matched result value.</returns>
    [Benchmark]
    public string MatchFailureOperation()
    {
        return _failureIntResult.Match(
            value => $"Success: {value}",
            error => $"Failure: {error}"
        );
    }
    
    /// <summary>
    /// Benchmarks Match operation on non-generic successful Result.
    /// </summary>
    /// <returns>The matched result value.</returns>
    [Benchmark]
    public string MatchNonGenericSuccessOperation()
    {
        return _successResult.Match(
            () => "Success",
            error => $"Failure: {error}"
        );
    }
    
    /// <summary>
    /// Benchmarks Match operation on non-generic failed Result.
    /// </summary>
    /// <returns>The matched result value.</returns>
    [Benchmark]
    public string MatchNonGenericFailureOperation()
    {
        return _failureResult.Match(
            () => "Success",
            error => $"Failure: {error}"
        );
    }
    
    /// <summary>
    /// Benchmarks Switch operation on successful Result.
    /// </summary>
    [Benchmark]
    public void SwitchSuccessOperation()
    {
        string result = string.Empty;
        _successIntResult.Switch(
            value => result = $"Success: {value}",
            error => result = $"Failure: {error}"
        );
    }
    
    /// <summary>
    /// Benchmarks Switch operation on failed Result.
    /// </summary>
    [Benchmark]
    public void SwitchFailureOperation()
    {
        string result = string.Empty;
        _failureIntResult.Switch(
            value => result = $"Success: {value}",
            error => result = $"Failure: {error}"
        );
    }
    
    /// <summary>
    /// Benchmarks TryGetValue operation on successful Result.
    /// </summary>
    /// <returns>True if value was retrieved.</returns>
    [Benchmark]
    public bool TryGetValueSuccessOperation()
    {
        return _successIntResult.TryGetValue(out int value);
    }
    
    /// <summary>
    /// Benchmarks TryGetValue operation on failed Result.
    /// </summary>
    /// <returns>False since result is failure.</returns>
    [Benchmark]
    public bool TryGetValueFailureOperation()
    {
        return _failureIntResult.TryGetValue(out int value);
    }
    
    /// <summary>
    /// Benchmarks IsSuccess property access.
    /// </summary>
    /// <returns>True for success result.</returns>
    [Benchmark]
    public bool IsSuccessPropertyAccess()
    {
        return _successIntResult.IsSuccess;
    }
    
    /// <summary>
    /// Benchmarks IsFailure property access.
    /// </summary>
    /// <returns>False for success result.</returns>
    [Benchmark]
    public bool IsFailurePropertyAccess()
    {
        return _successIntResult.IsFailure;
    }
    
    /// <summary>
    /// Benchmarks Error property access on failure result.
    /// </summary>
    /// <returns>The error message.</returns>
    [Benchmark]
    public string ErrorPropertyAccess()
    {
        return _failureIntResult.Error;
    }
    
    #endregion Result Operation Benchmarks
    
    #region Implicit Conversion Benchmarks
    
    /// <summary>
    /// Benchmarks implicit conversion from value to Result&lt;T&gt;.
    /// </summary>
    /// <returns>Result&lt;int&gt; created from implicit conversion.</returns>
    [Benchmark]
    public Result<int> ImplicitConversionFromValue()
    {
        Result<int> result = TestIntValue;
        return result;
    }
    
    /// <summary>
    /// Benchmarks implicit conversion from Result&lt;T&gt; to Result.
    /// </summary>
    /// <returns>Result created from implicit conversion.</returns>
    [Benchmark]
    public Result ImplicitConversionToNonGeneric()
    {
        Result result = _successIntResult;
        return result;
    }
    
    /// <summary>
    /// Benchmarks explicit conversion from Result&lt;T&gt; to value type.
    /// </summary>
    /// <returns>The extracted value.</returns>
    [Benchmark]
    public int ExplicitConversionToValue()
    {
        return (int)_successIntResult;
    }
    
    /// <summary>
    /// Benchmarks explicit conversion from Result to boolean.
    /// </summary>
    /// <returns>Boolean representation of result.</returns>
    [Benchmark]
    public bool ExplicitConversionToBoolean()
    {
        return (bool)_successResult;
    }
    
    #endregion Implicit Conversion Benchmarks
    
    #region Combine Operation Benchmarks
    
    /// <summary>
    /// Benchmarks Result.Combine() with all successful results.
    /// </summary>
    /// <returns>Combined result.</returns>
    [Benchmark]
    public Result CombineAllSuccessResults()
    {
        return Result.Combine(
            Result.Success(),
            Result.Success(),
            Result.Success()
        );
    }
    
    /// <summary>
    /// Benchmarks Result.Combine() with mixed success and failure results.
    /// </summary>
    /// <returns>Combined result.</returns>
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
    /// Benchmarks Result&lt;T&gt;.Combine() with all successful results.
    /// </summary>
    /// <returns>Combined generic result.</returns>
    [Benchmark]
    public Result<int> CombineAllSuccessGenericResults()
    {
        return Result.Combine(
            Result.Success(1),
            Result.Success(2),
            Result.Success(3)
        );
    }
    
    /// <summary>
    /// Benchmarks Result&lt;T&gt;.Combine() with mixed success and failure results.
    /// </summary>
    /// <returns>Combined generic result.</returns>
    [Benchmark]
    public Result<int> CombineMixedGenericResults()
    {
        return Result.Combine(
            Result.Success(1),
            Result.Failure<int>("Error 1"),
            Result.Success(3),
            Result.Failure<int>("Error 2")
        );
    }
    
    #endregion Combine Operation Benchmarks
    
    #region Comparison Benchmarks - Result vs Exception
    
    /// <summary>
    /// Benchmarks Result pattern success path.
    /// </summary>
    /// <returns>Success result with computed value.</returns>
    [Benchmark]
    public Result<int> ResultPatternSuccessPath()
    {
        try
        {
            int value = ComputeValueSuccess();
            return Result.Success(value);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>(ex.Message);
        }
    }
    
    /// <summary>
    /// Benchmarks Result pattern failure path.
    /// </summary>
    /// <returns>Failure result with error message.</returns>
    [Benchmark]
    public Result<int> ResultPatternFailurePath()
    {
        try
        {
            int value = ComputeValueFailure();
            return Result.Success(value);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>(ex.Message);
        }
    }
    
    /// <summary>
    /// Benchmarks traditional exception handling success path.
    /// </summary>
    /// <returns>The computed value.</returns>
    [Benchmark]
    public int ExceptionPatternSuccessPath()
    {
        return ComputeValueSuccess();
    }
    
    /// <summary>
    /// Benchmarks traditional exception handling failure path.
    /// </summary>
    /// <returns>Default value on exception.</returns>
    [Benchmark]
    public int ExceptionPatternFailurePath()
    {
        try
        {
            return ComputeValueFailure();
        }
        catch
        {
            return -1; // Default value
        }
    }
    
    /// <summary>
    /// Helper method that always succeeds for benchmarking.
    /// </summary>
    /// <returns>A computed integer value.</returns>
    private static int ComputeValueSuccess() => 42 * 2;
    
    /// <summary>
    /// Helper method that always throws for benchmarking.
    /// </summary>
    /// <returns>Never returns, always throws.</returns>
    /// <exception cref="InvalidOperationException">Always thrown for benchmarking purposes.</exception>
    private static int ComputeValueFailure() => throw new InvalidOperationException("Computation failed");
    
    #endregion Comparison Benchmarks - Result vs Exception
    
    #region Advanced Scenario Benchmarks
    
    /// <summary>
    /// Benchmarks nested Result operations with chaining.
    /// </summary>
    /// <returns>Result of nested operations.</returns>
    [Benchmark]
    public Result<string> NestedResultOperations()
    {
        Result<int> step1 = Result.Success(10);
        Result<int> step2 = step1.IsSuccess ? Result.Success(step1.TryGetValue(out int val1) ? val1 * 2 : 0) : Result.Failure<int>("Step 1 failed");
        Result<string> step3 = step2.IsSuccess ? Result.Success(step2.TryGetValue(out int val2) ? val2.ToString() : string.Empty) : Result.Failure<string>("Step 2 failed");
        return step3;
    }
    
    /// <summary>
    /// Benchmarks Result operations in a loop scenario.
    /// </summary>
    /// <returns>Final result after loop operations.</returns>
    [Benchmark]
    public Result<int> LoopResultOperations()
    {
        Result<int> accumulator = Result.Success(0);
        
        for (int i = 1; i <= 5; i++)
        {
            if (accumulator.IsFailure)
            {
                break;
            }
            
            bool success = accumulator.TryGetValue(out int currentValue);
            accumulator = success ? Result.Success(currentValue + i) : Result.Failure<int>("Accumulation failed");
        }
        
        return accumulator;
    }
    
    /// <summary>
    /// Benchmarks Result with large validation error dictionary.
    /// </summary>
    /// <returns>Result with large validation errors.</returns>
    [Benchmark]
    public Result<TestObject> LargeValidationErrorScenario()
    {
        Dictionary<string, string[]> largeErrors = [];
        
        for (int i = 1; i <= 50; i++)
        {
            largeErrors[$"Field{i}"] = [$"Error 1 for Field{i}", $"Error 2 for Field{i}", $"Error 3 for Field{i}"];
        }
        
        return Result.ValidationFailure<TestObject>(largeErrors);
    }
    
    /// <summary>
    /// Benchmarks complex Match operation with multiple result types.
    /// </summary>
    /// <returns>Processed string result.</returns>
    [Benchmark]
    public string ComplexMatchOperation()
    {
        Result<TestObject> complexResult = Result.Success(_testComplexObject);
        
        return complexResult.Match(
            obj => $"Success: {obj.Name} has {obj.Items.Count} items with value {obj.Value}",
            error => $"Failed to process complex object: {error}"
        );
    }
    
    #endregion Advanced Scenario Benchmarks
}