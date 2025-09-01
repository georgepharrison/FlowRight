using BenchmarkDotNet.Attributes;
using FlowRight.Core.Results;
using FlowRight.Validation.Builders;

namespace FlowRight.Benchmarks;

/// <summary>
/// Benchmarks for validation operations using ValidationBuilder.
/// </summary>
/// <remarks>
/// This class contains benchmarks for TASK-062: Measure validation performance.
/// Measures performance of ValidationBuilder operations, rule execution, and error aggregation.
/// </remarks>
[MemoryDiagnoser]
[SimpleJob]
public class ValidationBenchmarks
{
    private readonly ValidationBuilder<TestModel> _builder = new();
    private const string ValidEmail = "test@example.com";
    private const string InvalidEmail = "not-an-email";
    private const string ValidName = "John Doe";
    private const string InvalidName = "";
    private const int ValidAge = 25;
    private const int InvalidAge = -5;

    /// <summary>
    /// Test model for validation benchmarks.
    /// </summary>
    /// <param name="Name">The name property.</param>
    /// <param name="Email">The email property.</param>
    /// <param name="Age">The age property.</param>
    public record TestModel(string Name, string Email, int Age);

    /// <summary>
    /// Benchmarks simple single-rule validation that passes.
    /// </summary>
    /// <returns>A validation result.</returns>
    [Benchmark]
    public static Result<TestModel> SingleRuleValidationSuccess()
    {
        return new ValidationBuilder<TestModel>()
            .RuleFor(x => x.Name, ValidName)
                .NotEmpty()
            .Build(() => new TestModel(ValidName, ValidEmail, ValidAge));
    }

    /// <summary>
    /// Benchmarks simple single-rule validation that fails.
    /// </summary>
    /// <returns>A validation result.</returns>
    [Benchmark]
    public static Result<TestModel> SingleRuleValidationFailure()
    {
        return new ValidationBuilder<TestModel>()
            .RuleFor(x => x.Name, InvalidName)
                .NotEmpty()
            .Build(() => new TestModel(InvalidName, ValidEmail, ValidAge));
    }

    /// <summary>
    /// Benchmarks complex multi-rule validation that passes.
    /// </summary>
    /// <returns>A validation result.</returns>
    [Benchmark]
    public static Result<TestModel> MultiRuleValidationSuccess()
    {
        return new ValidationBuilder<TestModel>()
            .RuleFor(x => x.Name, ValidName)
                .NotEmpty()
                .MaximumLength(50)
                .MinimumLength(2)
            .RuleFor(x => x.Email, ValidEmail)
                .NotEmpty()
                .EmailAddress()
            .RuleFor(x => x.Age, ValidAge)
                .GreaterThan(0)
                .LessThan(120)
            .Build(() => new TestModel(ValidName, ValidEmail, ValidAge));
    }

    /// <summary>
    /// Benchmarks complex multi-rule validation that fails on multiple rules.
    /// </summary>
    /// <returns>A validation result.</returns>
    [Benchmark]
    public static Result<TestModel> MultiRuleValidationFailure()
    {
        return new ValidationBuilder<TestModel>()
            .RuleFor(x => x.Name, InvalidName)
                .NotEmpty()
                .MaximumLength(50)
                .MinimumLength(2)
            .RuleFor(x => x.Email, InvalidEmail)
                .NotEmpty()
                .EmailAddress()
            .RuleFor(x => x.Age, InvalidAge)
                .GreaterThan(0)
                .LessThan(120)
            .Build(() => new TestModel(InvalidName, InvalidEmail, InvalidAge));
    }
}