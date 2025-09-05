using BenchmarkDotNet.Running;

namespace FlowRight.Benchmarks;

/// <summary>
/// Entry point for FlowRight benchmark suite.
/// </summary>
/// <remarks>
/// Run all benchmarks: dotnet run -c Release
/// Run specific benchmark: dotnet run -c Release --filter "*ResultBenchmarks*"
/// </remarks>
public static class Program
{
    /// <summary>
    /// Main entry point for benchmark execution.
    /// </summary>
    /// <param name="args">Command line arguments passed to BenchmarkDotNet.</param>
    public static void Main(string[] args)
    {
        // If --simple-test is passed, run our simple performance test instead
        if (args.Length > 0 && args[0] == "--simple-test")
        {
            SimplePerformanceTest.RunBasicPerformanceTest();
            return;
        }
        
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}