using System.Diagnostics;
using FlowRight.Core.Results;
using FlowRight.Validation.Builders;

namespace FlowRight.Benchmarks;

/// <summary>
/// Simple performance test to measure baseline performance of Result operations.
/// </summary>
public static class SimplePerformanceTest
{
    /// <summary>
    /// Runs a simple performance test and outputs basic timing information.
    /// </summary>
    public static void RunBasicPerformanceTest()
    {
        const int iterations = 1_000_000;
        
        Console.WriteLine("=== FlowRight Performance Analysis ===");
        Console.WriteLine($"Testing {iterations:N0} iterations of each operation");
        Console.WriteLine();
        
        // Test Result.Success() creation
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            Result result = Result.Success();
        }
        sw.Stop();
        double successNs = (double)sw.ElapsedTicks / iterations * 1_000_000_000.0 / Stopwatch.Frequency;
        
        Console.WriteLine($"Result.Success(): {successNs:F2}ns per operation");
        Console.WriteLine($"  Target: <10ns, Acceptable: <20ns, Status: {(successNs < 10 ? "EXCELLENT" : successNs < 20 ? "GOOD" : "NEEDS OPTIMIZATION")}");
        
        // Test Result<int>.Success() creation
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            Result<int> result = Result.Success(42);
        }
        sw.Stop();
        double genericSuccessNs = (double)sw.ElapsedTicks / iterations * 1_000_000_000.0 / Stopwatch.Frequency;
        
        Console.WriteLine($"Result<int>.Success(): {genericSuccessNs:F2}ns per operation");
        
        // Test Result.Failure() creation  
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            Result result = Result.Failure("Test error");
        }
        sw.Stop();
        double failureNs = (double)sw.ElapsedTicks / iterations * 1_000_000_000.0 / Stopwatch.Frequency;
        
        Console.WriteLine($"Result.Failure(): {failureNs:F2}ns per operation");
        Console.WriteLine($"  Target: <50ns, Acceptable: <100ns, Status: {(failureNs < 50 ? "EXCELLENT" : failureNs < 100 ? "GOOD" : "NEEDS OPTIMIZATION")}");
        
        // Test Match operation (with pre-compiled delegates to avoid allocation in hot path)
        Result<int> testResult = Result.Success(42);
        Func<int, string> onSuccess = value => $"Success: {value}";
        Func<string, string> onFailure = error => $"Error: {error}";
        
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            string result = testResult.Match(onSuccess, onFailure);
        }
        sw.Stop();
        double matchNs = (double)sw.ElapsedTicks / iterations * 1_000_000_000.0 / Stopwatch.Frequency;
        
        Console.WriteLine($"Match operation: {matchNs:F2}ns per operation");
        Console.WriteLine($"  Target: <20ns, Acceptable: <50ns, Status: {(matchNs < 20 ? "EXCELLENT" : matchNs < 50 ? "GOOD" : "NEEDS OPTIMIZATION")}");
        
        // Test TryGetValue operation
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            bool success = testResult.TryGetValue(out int value);
        }
        sw.Stop();
        double tryGetValueNs = (double)sw.ElapsedTicks / iterations * 1_000_000_000.0 / Stopwatch.Frequency;
        
        Console.WriteLine($"TryGetValue operation: {tryGetValueNs:F2}ns per operation");
        
        // Test IsSuccess property access
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            bool isSuccess = testResult.IsSuccess;
        }
        sw.Stop();
        double isSuccessNs = (double)sw.ElapsedTicks / iterations * 1_000_000_000.0 / Stopwatch.Frequency;
        
        Console.WriteLine($"IsSuccess property: {isSuccessNs:F2}ns per operation");
        
        // Test simple validation
        sw.Restart();
        for (int i = 0; i < iterations / 100; i++) // Fewer iterations for validation
        {
            ValidationBuilder<TestModel> builder = new();
            Result<TestModel> result = builder
                .RuleFor(x => x.Name, "John")
                    .NotEmpty()
                .RuleFor(x => x.Age, 25)
                    .GreaterThan(0)
                .Build(() => new TestModel("John", 25));
        }
        sw.Stop();
        double validationNs = (double)sw.ElapsedTicks / (iterations / 100) * 1_000_000_000.0 / Stopwatch.Frequency;
        
        // Test optimized validation with explicit property names (to bypass Expression parsing)
        sw.Restart();
        for (int i = 0; i < iterations / 100; i++) // Fewer iterations for validation
        {
            ValidationBuilder<TestModel> builder = new();
            Result<TestModel> result = builder
                .RuleFor(x => x.Name, "John", "Name")
                    .NotEmpty()
                .RuleFor(x => x.Age, 25, "Age")
                    .GreaterThan(0)
                .Build(() => new TestModel("John", 25));
        }
        sw.Stop();
        double optimizedValidationNs = (double)sw.ElapsedTicks / (iterations / 100) * 1_000_000_000.0 / Stopwatch.Frequency;
        
        Console.WriteLine($"Simple validation (2 rules): {validationNs:F2}ns per operation");
        Console.WriteLine($"  Scaled to 10 rules: ~{validationNs * 5:F2}ns");
        Console.WriteLine($"  Target: <100ns (10 rules), Acceptable: <200ns, Status: {(validationNs * 5 < 100 ? "EXCELLENT" : validationNs * 5 < 200 ? "GOOD" : "NEEDS OPTIMIZATION")}");
        
        Console.WriteLine($"Optimized validation (explicit names): {optimizedValidationNs:F2}ns per operation");
        Console.WriteLine($"  Scaled to 10 rules: ~{optimizedValidationNs * 5:F2}ns");
        Console.WriteLine($"  Improvement: {validationNs / optimizedValidationNs:F1}x faster");
        
        Console.WriteLine();
        Console.WriteLine("=== Performance Summary ===");
        Console.WriteLine($"Overall assessment: {GetOverallAssessment(successNs, failureNs, matchNs, validationNs * 5)}");
    }
    
    /// <summary>
    /// Test model for validation benchmarks.
    /// </summary>
    public record TestModel(string Name, int Age);
    
    private static string GetOverallAssessment(double successNs, double failureNs, double matchNs, double validationNs)
    {
        int score = 0;
        score += successNs < 10 ? 2 : successNs < 20 ? 1 : 0;
        score += failureNs < 50 ? 2 : failureNs < 100 ? 1 : 0;
        score += matchNs < 20 ? 2 : matchNs < 50 ? 1 : 0;
        score += validationNs < 100 ? 2 : validationNs < 200 ? 1 : 0;
        
        return score switch
        {
            8 => "EXCELLENT - All targets met",
            6 or 7 => "GOOD - Most targets met",
            4 or 5 => "ACCEPTABLE - Some optimization needed",
            _ => "NEEDS OPTIMIZATION - Significant performance issues"
        };
    }
}