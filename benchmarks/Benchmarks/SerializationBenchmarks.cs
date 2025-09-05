using BenchmarkDotNet.Attributes;
using FlowRight.Core.Results;
using System.Text.Json;

namespace FlowRight.Benchmarks;

/// <summary>
/// Benchmarks for Result serialization and deserialization operations.
/// </summary>
/// <remarks>
/// This class contains benchmarks for TASK-063: Profile memory allocations and 
/// serialization performance. Measures JSON serialization/deserialization performance
/// and memory usage for Result types.
/// </remarks>
[MemoryDiagnoser]
[SimpleJob]
public class SerializationBenchmarks
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly Result SuccessResult = Result.Success();
    private static readonly Result FailureResult = Result.Failure("Test error message");
    private static readonly Result<int> SuccessResultWithValue = Result.Success(42);
    private static readonly Result<int> FailureResultWithValue = Result.Failure<int>("Test error with value");
    
    private static readonly string SuccessJson = JsonSerializer.Serialize(SuccessResult, JsonOptions);
    private static readonly string FailureJson = JsonSerializer.Serialize(FailureResult, JsonOptions);
    private static readonly string SuccessWithValueJson = JsonSerializer.Serialize(SuccessResultWithValue, JsonOptions);
    private static readonly string FailureWithValueJson = JsonSerializer.Serialize(FailureResultWithValue, JsonOptions);

    /// <summary>
    /// Benchmarks serialization of successful Result.
    /// </summary>
    /// <returns>JSON string representation.</returns>
    [Benchmark]
    public static string SerializeSuccessResult() =>
        JsonSerializer.Serialize(SuccessResult, JsonOptions);

    /// <summary>
    /// Benchmarks serialization of failed Result.
    /// </summary>
    /// <returns>JSON string representation.</returns>
    [Benchmark]
    public static string SerializeFailureResult() =>
        JsonSerializer.Serialize(FailureResult, JsonOptions);

    /// <summary>
    /// Benchmarks serialization of successful Result&lt;T&gt;.
    /// </summary>
    /// <returns>JSON string representation.</returns>
    [Benchmark]
    public static string SerializeSuccessResultWithValue() =>
        JsonSerializer.Serialize(SuccessResultWithValue, JsonOptions);

    /// <summary>
    /// Benchmarks serialization of failed Result&lt;T&gt;.
    /// </summary>
    /// <returns>JSON string representation.</returns>
    [Benchmark]
    public static string SerializeFailureResultWithValue() =>
        JsonSerializer.Serialize(FailureResultWithValue, JsonOptions);

    /// <summary>
    /// Benchmarks deserialization of successful Result.
    /// </summary>
    /// <returns>Deserialized Result instance.</returns>
    [Benchmark]
    public static Result? DeserializeSuccessResult() =>
        JsonSerializer.Deserialize<Result>(SuccessJson, JsonOptions);

    /// <summary>
    /// Benchmarks deserialization of failed Result.
    /// </summary>
    /// <returns>Deserialized Result instance.</returns>
    [Benchmark]
    public static Result? DeserializeFailureResult() =>
        JsonSerializer.Deserialize<Result>(FailureJson, JsonOptions);

    /// <summary>
    /// Benchmarks deserialization of successful Result&lt;T&gt;.
    /// </summary>
    /// <returns>Deserialized Result&lt;T&gt; instance.</returns>
    [Benchmark]
    public static Result<int>? DeserializeSuccessResultWithValue() =>
        JsonSerializer.Deserialize<Result<int>>(SuccessWithValueJson, JsonOptions);

    /// <summary>
    /// Benchmarks deserialization of failed Result&lt;T&gt;.
    /// </summary>
    /// <returns>Deserialized Result&lt;T&gt; instance.</returns>
    [Benchmark]
    public static Result<int>? DeserializeFailureResultWithValue() =>
        JsonSerializer.Deserialize<Result<int>>(FailureWithValueJson, JsonOptions);

    /// <summary>
    /// Benchmarks round-trip serialization/deserialization of Result&lt;T&gt;.
    /// </summary>
    /// <returns>Deserialized Result&lt;T&gt; instance.</returns>
    [Benchmark]
    public static Result<int>? RoundTripSerialization()
    {
        string json = JsonSerializer.Serialize(SuccessResultWithValue, JsonOptions);
        return JsonSerializer.Deserialize<Result<int>>(json, JsonOptions);
    }
}