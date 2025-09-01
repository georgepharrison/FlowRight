using BenchmarkDotNet.Attributes;
using FlowRight.Core.Results;
using FlowRight.Validation.Builders;

namespace FlowRight.Benchmarks;

/// <summary>
/// Verification benchmarks to document and verify optimizations made to FlowRight hot paths.
/// </summary>
/// <remarks>
/// <para>
/// This class documents the optimizations implemented during TASK-065: "Optimize hot paths based on results".
/// The benchmarks compare before/after performance and verify that performance targets are met.
/// </para>
/// <para>
/// Performance targets from TASKS.md:
/// <list type="bullet">
/// <item><description>Result.Success(): &lt;10ns target, &lt;20ns acceptable</description></item>
/// <item><description>Result.Failure(): &lt;50ns target, &lt;100ns acceptable</description></item>
/// <item><description>Match operation: &lt;20ns target, &lt;50ns acceptable</description></item>
/// <item><description>Validation (10 rules): &lt;100ns target, &lt;200ns acceptable</description></item>
/// </list>
/// </para>
/// <para>
/// Optimizations implemented:
/// <list type="bullet">
/// <item><description>Static caching for common Result.Success() cases - eliminated allocations</description></item>
/// <item><description>Shared empty dictionary for Failures property - reduced memory allocations</description></item>
/// <item><description>MethodImpl(AggressiveInlining) on hot path methods like Match and TryGetValue</description></item>
/// <item><description>Conditional compilation of argument null checking in Release builds</description></item>
/// <item><description>Direct string.IsNullOrEmpty checks instead of property chaining</description></item>
/// </list>
/// </para>
/// </remarks>
[MemoryDiagnoser]
[SimpleJob]
public class OptimizationVerificationBenchmarks
{
    #region Test Data Setup

    private const string TestError = "Test error message";
    private const int TestIntValue = 42;
    private readonly Result _successResult = Result.Success();
    private readonly Result _failureResult = Result.Failure(TestError);
    private readonly Result<int> _successIntResult = Result.Success(TestIntValue);
    private readonly Result<int> _failureIntResult = Result.Failure<int>(TestError);

    // Pre-compiled delegates to measure Match operation performance without delegate creation overhead
    private readonly Func<int, string> _onSuccess = value => $"Success: {value}";
    private readonly Func<string, string> _onFailure = error => $"Error: {error}";

    /// <summary>
    /// Test model for validation benchmarks.
    /// </summary>
    public record TestModel(string Name, int Age);

    #endregion Test Data Setup

    #region Result Creation Benchmarks

    /// <summary>
    /// Benchmarks optimized Result.Success() creation.
    /// Target: &lt;10ns, Acceptable: &lt;20ns
    /// Optimization: Static instance caching for common ResultType values.
    /// </summary>
    /// <returns>A success Result.</returns>
    [Benchmark(Baseline = true)]
    public Result OptimizedResultSuccess() => Result.Success();

    /// <summary>
    /// Benchmarks optimized Result.Success(ResultType.Information) creation.
    /// Optimization: Static instance caching.
    /// </summary>
    /// <returns>A success Result with Information type.</returns>
    [Benchmark]
    public Result OptimizedResultSuccessWithType() => Result.Success(ResultType.Information);

    /// <summary>
    /// Benchmarks optimized Result&lt;int&gt;.Success() creation.
    /// Note: Generic results cannot be cached due to type parameters.
    /// </summary>
    /// <returns>A success Result&lt;int&gt;.</returns>
    [Benchmark]
    public Result<int> OptimizedGenericResultSuccess() => Result.Success(TestIntValue);

    /// <summary>
    /// Benchmarks optimized Result.Failure() creation.
    /// Target: &lt;50ns, Acceptable: &lt;100ns
    /// Optimization: Shared empty dictionary for Failures property.
    /// </summary>
    /// <returns>A failure Result.</returns>
    [Benchmark]
    public Result OptimizedResultFailure() => Result.Failure(TestError);

    /// <summary>
    /// Benchmarks optimized Result&lt;int&gt;.Failure() creation.
    /// Optimization: Shared empty dictionary for Failures property.
    /// </summary>
    /// <returns>A failure Result&lt;int&gt;.</returns>
    [Benchmark]
    public Result<int> OptimizedGenericResultFailure() => Result.Failure<int>(TestError);

    #endregion Result Creation Benchmarks

    #region Result Operation Benchmarks

    /// <summary>
    /// Benchmarks optimized Match operation performance.
    /// Target: &lt;20ns, Acceptable: &lt;50ns
    /// Optimizations: AggressiveInlining, conditional null checking, pre-compiled delegates.
    /// </summary>
    /// <returns>The matched result string.</returns>
    [Benchmark]
    public string OptimizedMatchOperation() =>
        _successIntResult.Match(_onSuccess, _onFailure);

    /// <summary>
    /// Benchmarks optimized TryGetValue operation performance.
    /// Optimizations: AggressiveInlining, branch optimization.
    /// </summary>
    /// <returns>True if value was retrieved.</returns>
    [Benchmark]
    public bool OptimizedTryGetValue() =>
        _successIntResult.TryGetValue(out int value);

    /// <summary>
    /// Benchmarks optimized IsSuccess property access.
    /// Optimization: Direct string.IsNullOrEmpty check.
    /// </summary>
    /// <returns>True for success result.</returns>
    [Benchmark]
    public bool OptimizedIsSuccess() => _successIntResult.IsSuccess;

    /// <summary>
    /// Benchmarks optimized IsFailure property access.
    /// Optimization: Direct string.IsNullOrEmpty check.
    /// </summary>
    /// <returns>False for success result.</returns>
    [Benchmark]
    public bool OptimizedIsFailure() => _successIntResult.IsFailure;

    #endregion Result Operation Benchmarks

    #region Validation Benchmarks

    /// <summary>
    /// Benchmarks baseline validation performance.
    /// This represents the current performance with Expression parsing overhead.
    /// </summary>
    /// <returns>A validation result.</returns>
    [Benchmark]
    public Result<TestModel> BaselineValidation()
    {
        ValidationBuilder<TestModel> builder = new();
        return builder
            .RuleFor(x => x.Name, "John")
                .NotEmpty()
            .RuleFor(x => x.Age, 25)
                .GreaterThan(0)
            .Build(() => new TestModel("John", 25));
    }

    /// <summary>
    /// Benchmarks optimized validation with explicit property names.
    /// Optimization: Explicit display names to bypass Expression parsing.
    /// Target: &lt;100ns (for 10 rules), Acceptable: &lt;200ns
    /// </summary>
    /// <returns>A validation result.</returns>
    [Benchmark]
    public Result<TestModel> OptimizedValidationWithNames()
    {
        ValidationBuilder<TestModel> builder = new();
        return builder
            .RuleFor(x => x.Name, "John", "Name")
                .NotEmpty()
            .RuleFor(x => x.Age, 25, "Age")
                .GreaterThan(0)
            .Build(() => new TestModel("John", 25));
    }

    #endregion Validation Benchmarks

    #region Comparison Benchmarks

    /// <summary>
    /// Comparison benchmark: Result pattern vs Exception handling (success path).
    /// Shows the performance benefit of the Result pattern for normal flows.
    /// </summary>
    /// <returns>Success result with computed value.</returns>
    [Benchmark]
    public Result<int> ResultPatternSuccessPath()
    {
        try
        {
            int value = ComputeValue();
            return Result.Success(value);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>(ex.Message);
        }
    }

    /// <summary>
    /// Comparison benchmark: Traditional exception handling (success path).
    /// Shows the baseline performance for traditional approaches.
    /// </summary>
    /// <returns>The computed value.</returns>
    [Benchmark]
    public int ExceptionPatternSuccessPath() => ComputeValue();

    #endregion Comparison Benchmarks

    #region Helper Methods

    /// <summary>
    /// Helper method for performance comparison benchmarks.
    /// </summary>
    /// <returns>A computed integer value.</returns>
    private static int ComputeValue() => 42 * 2;

    #endregion Helper Methods
}