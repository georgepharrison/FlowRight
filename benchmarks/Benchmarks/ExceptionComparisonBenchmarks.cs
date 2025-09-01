using System.ComponentModel.DataAnnotations;
using BenchmarkDotNet.Attributes;
using FlowRight.Core.Results;
using FlowRight.Validation.Builders;

namespace FlowRight.Benchmarks;

/// <summary>
/// Comprehensive benchmarks comparing Result pattern performance vs traditional exception handling.
/// </summary>
/// <remarks>
/// <para>
/// This class contains benchmarks for TASK-064: Compare with exception performance.
/// Provides concrete performance data demonstrating the advantages of the Result pattern
/// over traditional exception handling across multiple scenarios.
/// </para>
/// <para>
/// Key comparison areas:
/// </para>
/// <list type="bullet">
/// <item><description>Success Path: Result.Success() vs normal return (no exception)</description></item>
/// <item><description>Error Path: Result.Failure() vs throwing/catching exceptions</description></item>
/// <item><description>Validation: ValidationBuilder vs throwing ArgumentException/ValidationException</description></item>
/// <item><description>Try Pattern: Result.Try() vs try/catch blocks</description></item>
/// <item><description>Error Propagation: Result chaining vs exception bubbling</description></item>
/// <item><description>Memory Allocation: Result error handling vs exception allocation</description></item>
/// </list>
/// <para>
/// Performance focus areas:
/// </para>
/// <list type="bullet">
/// <item><description>Hot path performance (repeated operations)</description></item>
/// <item><description>Memory allocation differences and GC pressure</description></item>
/// <item><description>CPU performance differences</description></item>
/// <item><description>Call stack overhead analysis</description></item>
/// </list>
/// </remarks>
[MemoryDiagnoser]
[SimpleJob]
public class ExceptionComparisonBenchmarks
{
    #region Test Data Setup

    // Test data for benchmarks
    private const string ValidEmail = "user@example.com";
    private const string InvalidEmail = "not-an-email";
    private const string ValidName = "John Doe";
    private const string ValidPhoneNumber = "+1-555-123-4567";
    private const int ValidAge = 25;
    private const int InvalidAge = -5;
    private const decimal ValidSalary = 50000.00m;
    
    // Bulk operation data
    private readonly List<string> _validEmails = Enumerable.Range(1, 100)
        .Select(i => $"user{i}@example.com").ToList();
    private readonly List<string> _mixedEmails = Enumerable.Range(1, 100)
        .Select(i => i % 4 == 0 ? "invalid-email" : $"user{i}@example.com").ToList();
    
    // Complex test models
    private readonly UserProfile _validUser = new("John Doe", "john@example.com", 30, 75000m);
    private readonly UserProfile _invalidUser = new("", "invalid-email", -5, -1000m);
    
    /// <summary>
    /// Test user profile model for complex validation scenarios.
    /// </summary>
    /// <param name="Name">User's full name.</param>
    /// <param name="Email">User's email address.</param>
    /// <param name="Age">User's age in years.</param>
    /// <param name="Salary">User's annual salary.</param>
    public record UserProfile(string Name, string Email, int Age, decimal Salary);

    /// <summary>
    /// Database operation result for simulation benchmarks.
    /// </summary>
    /// <param name="Id">Record identifier.</param>
    /// <param name="Data">Record data content.</param>
    public record DatabaseRecord(int Id, string Data);

    #endregion Test Data Setup

    #region Success Path Comparison Benchmarks

    /// <summary>
    /// Benchmarks simple operation success path using Result pattern.
    /// Measures overhead of Result.Success() vs direct return.
    /// </summary>
    /// <returns>A successful Result containing computed value.</returns>
    [Benchmark(Baseline = true)]
    public Result<int> SimpleOperationResultSuccess()
    {
        int computedValue = ComputeSimpleValue(42);
        return Result.Success(computedValue);
    }

    /// <summary>
    /// Benchmarks simple operation success path using direct return (no exceptions).
    /// Represents the theoretical "fastest" approach when no errors occur.
    /// </summary>
    /// <returns>The computed value directly.</returns>
    [Benchmark]
    public int SimpleOperationDirectSuccess()
    {
        return ComputeSimpleValue(42);
    }

    /// <summary>
    /// Benchmarks string operation success path using Result pattern.
    /// </summary>
    /// <returns>A successful Result containing processed string.</returns>
    [Benchmark]
    public Result<string> StringOperationResultSuccess()
    {
        string processed = ProcessString("Hello World");
        return Result.Success(processed);
    }

    /// <summary>
    /// Benchmarks string operation success path using direct return.
    /// </summary>
    /// <returns>The processed string directly.</returns>
    [Benchmark]
    public string StringOperationDirectSuccess()
    {
        return ProcessString("Hello World");
    }

    /// <summary>
    /// Benchmarks complex object creation success path using Result pattern.
    /// </summary>
    /// <returns>A successful Result containing created user profile.</returns>
    [Benchmark]
    public Result<UserProfile> ComplexObjectResultSuccess()
    {
        UserProfile user = CreateUserProfile(ValidName, ValidEmail, ValidAge, ValidSalary);
        return Result.Success(user);
    }

    /// <summary>
    /// Benchmarks complex object creation success path using direct return.
    /// </summary>
    /// <returns>The created user profile directly.</returns>
    [Benchmark]
    public UserProfile ComplexObjectDirectSuccess()
    {
        return CreateUserProfile(ValidName, ValidEmail, ValidAge, ValidSalary);
    }

    #endregion Success Path Comparison Benchmarks

    #region Error Path Comparison Benchmarks

    /// <summary>
    /// Benchmarks simple operation error path using Result pattern.
    /// Measures Result.Failure() performance vs exception throwing/catching.
    /// </summary>
    /// <returns>A failure Result with error message.</returns>
    [Benchmark]
    public Result<int> SimpleOperationResultError()
    {
        try
        {
            int value = ComputeSimpleValueWithError();
            return Result.Success(value);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<int>(ex.Message);
        }
    }

    /// <summary>
    /// Benchmarks simple operation error path using traditional exception handling.
    /// Measures the cost of throwing and catching exceptions.
    /// </summary>
    /// <returns>Default value when exception occurs.</returns>
    [Benchmark]
    public int SimpleOperationExceptionError()
    {
        try
        {
            return ComputeSimpleValueWithError();
        }
        catch (InvalidOperationException)
        {
            return -1; // Default error value
        }
    }

    /// <summary>
    /// Benchmarks string operation error path using Result pattern.
    /// </summary>
    /// <returns>A failure Result with error message.</returns>
    [Benchmark]
    public Result<string> StringOperationResultError()
    {
        try
        {
            string processed = ProcessStringWithError();
            return Result.Success(processed);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<string>(ex.Message);
        }
    }

    /// <summary>
    /// Benchmarks string operation error path using traditional exception handling.
    /// </summary>
    /// <returns>Default string when exception occurs.</returns>
    [Benchmark]
    public string StringOperationExceptionError()
    {
        try
        {
            return ProcessStringWithError();
        }
        catch (ArgumentException)
        {
            return string.Empty; // Default error value
        }
    }

    /// <summary>
    /// Benchmarks complex object creation error path using Result pattern.
    /// </summary>
    /// <returns>A failure Result with error message.</returns>
    [Benchmark]
    public Result<UserProfile> ComplexObjectResultError()
    {
        try
        {
            UserProfile user = CreateUserProfileWithError();
            return Result.Success(user);
        }
        catch (ValidationException ex)
        {
            return Result.Failure<UserProfile>(ex.Message);
        }
    }

    /// <summary>
    /// Benchmarks complex object creation error path using traditional exception handling.
    /// </summary>
    /// <returns>Null when exception occurs.</returns>
    [Benchmark]
    public UserProfile? ComplexObjectExceptionError()
    {
        try
        {
            return CreateUserProfileWithError();
        }
        catch (ValidationException)
        {
            return null; // Default error value
        }
    }

    #endregion Error Path Comparison Benchmarks

    #region Validation Comparison Benchmarks

    /// <summary>
    /// Benchmarks email validation using ValidationBuilder (Result pattern).
    /// Demonstrates fluent validation with Result pattern error aggregation.
    /// </summary>
    /// <returns>Validation result for email.</returns>
    [Benchmark]
    public Result<string> EmailValidationResultSuccess()
    {
        return new ValidationBuilder<string>()
            .RuleFor(x => x, ValidEmail)
                .NotEmpty()
                .EmailAddress()
            .Build(() => ValidEmail);
    }

    /// <summary>
    /// Benchmarks email validation using traditional exception throwing.
    /// Throws ArgumentException for invalid email addresses.
    /// </summary>
    /// <returns>Valid email if validation passes.</returns>
    [Benchmark]
    public string EmailValidationExceptionSuccess()
    {
        return ValidateEmailWithExceptions(ValidEmail);
    }

    /// <summary>
    /// Benchmarks email validation failure using ValidationBuilder.
    /// </summary>
    /// <returns>Validation failure result.</returns>
    [Benchmark]
    public Result<string> EmailValidationResultError()
    {
        return new ValidationBuilder<string>()
            .RuleFor(x => x, InvalidEmail)
                .NotEmpty()
                .EmailAddress()
            .Build(() => InvalidEmail);
    }

    /// <summary>
    /// Benchmarks email validation failure using traditional exception throwing.
    /// </summary>
    /// <returns>Empty string when validation fails.</returns>
    [Benchmark]
    public string EmailValidationExceptionError()
    {
        try
        {
            return ValidateEmailWithExceptions(InvalidEmail);
        }
        catch (ArgumentException)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Benchmarks complex multi-field validation using ValidationBuilder (Result pattern).
    /// Validates user profile with multiple rules and error aggregation.
    /// </summary>
    /// <returns>Validation result for user profile.</returns>
    [Benchmark]
    public Result<UserProfile> ComplexValidationResultSuccess()
    {
        return new ValidationBuilder<UserProfile>()
            .RuleFor(x => x.Name, _validUser.Name)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100)
            .RuleFor(x => x.Email, _validUser.Email)
                .NotEmpty()
                .EmailAddress()
            .RuleFor(x => x.Age, _validUser.Age)
                .GreaterThan(0)
                .LessThan(120)
            .RuleFor(x => x.Salary, _validUser.Salary)
                .GreaterThanOrEqualTo(0)
            .Build(() => _validUser);
    }

    /// <summary>
    /// Benchmarks complex multi-field validation using traditional exception throwing.
    /// Validates user profile with multiple checks, throwing on first error.
    /// </summary>
    /// <returns>Valid user profile if all validations pass.</returns>
    [Benchmark]
    public UserProfile ComplexValidationExceptionSuccess()
    {
        return ValidateUserProfileWithExceptions(_validUser);
    }

    /// <summary>
    /// Benchmarks complex multi-field validation failure using ValidationBuilder.
    /// </summary>
    /// <returns>Validation failure result with aggregated errors.</returns>
    [Benchmark]
    public Result<UserProfile> ComplexValidationResultError()
    {
        return new ValidationBuilder<UserProfile>()
            .RuleFor(x => x.Name, _invalidUser.Name)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100)
            .RuleFor(x => x.Email, _invalidUser.Email)
                .NotEmpty()
                .EmailAddress()
            .RuleFor(x => x.Age, _invalidUser.Age)
                .GreaterThan(0)
                .LessThan(120)
            .RuleFor(x => x.Salary, _invalidUser.Salary)
                .GreaterThanOrEqualTo(0)
            .Build(() => _invalidUser);
    }

    /// <summary>
    /// Benchmarks complex multi-field validation failure using traditional exception throwing.
    /// </summary>
    /// <returns>Null when validation fails.</returns>
    [Benchmark]
    public UserProfile? ComplexValidationExceptionError()
    {
        try
        {
            return ValidateUserProfileWithExceptions(_invalidUser);
        }
        catch (ValidationException)
        {
            return null;
        }
    }

    #endregion Validation Comparison Benchmarks

    #region Try Pattern Comparison Benchmarks

    /// <summary>
    /// Benchmarks Result pattern with manual try/catch for safe operation execution.
    /// Manually catches exceptions and converts them to Result failures.
    /// </summary>
    /// <returns>Result of trying to parse integer.</returns>
    [Benchmark]
    public Result<int> TryPatternResultSuccess()
    {
        try
        {
            int value = int.Parse("42");
            return Result.Success(value);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>(ex.Message);
        }
    }

    /// <summary>
    /// Benchmarks traditional try/catch pattern for safe operation execution.
    /// </summary>
    /// <returns>Parsed integer or default value on failure.</returns>
    [Benchmark]
    public int TryPatternExceptionSuccess()
    {
        try
        {
            return int.Parse("42");
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Benchmarks Result pattern with operation that fails.
    /// </summary>
    /// <returns>Result failure when parsing fails.</returns>
    [Benchmark]
    public Result<int> TryPatternResultError()
    {
        try
        {
            int value = int.Parse("invalid");
            return Result.Success(value);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>(ex.Message);
        }
    }

    /// <summary>
    /// Benchmarks traditional try/catch pattern with operation that fails.
    /// </summary>
    /// <returns>Default value when parsing fails.</returns>
    [Benchmark]
    public int TryPatternExceptionError()
    {
        try
        {
            return int.Parse("invalid");
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Benchmarks Result pattern with complex operation that succeeds.
    /// </summary>
    /// <returns>Result of trying to perform file-like operation.</returns>
    [Benchmark]
    public Result<string> TryPatternComplexResultSuccess()
    {
        try
        {
            string content = SimulateFileReadOperation("valid-path.txt");
            return Result.Success(content);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(ex.Message);
        }
    }

    /// <summary>
    /// Benchmarks traditional try/catch pattern with complex operation that succeeds.
    /// </summary>
    /// <returns>File content or empty string on failure.</returns>
    [Benchmark]
    public string TryPatternComplexExceptionSuccess()
    {
        try
        {
            return SimulateFileReadOperation("valid-path.txt");
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Benchmarks Result pattern with complex operation that fails.
    /// </summary>
    /// <returns>Result failure when file operation fails.</returns>
    [Benchmark]
    public Result<string> TryPatternComplexResultError()
    {
        try
        {
            string content = SimulateFileReadOperation("invalid-path.txt");
            return Result.Success(content);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(ex.Message);
        }
    }

    /// <summary>
    /// Benchmarks traditional try/catch pattern with complex operation that fails.
    /// </summary>
    /// <returns>Empty string when file operation fails.</returns>
    [Benchmark]
    public string TryPatternComplexExceptionError()
    {
        try
        {
            return SimulateFileReadOperation("invalid-path.txt");
        }
        catch
        {
            return string.Empty;
        }
    }

    #endregion Try Pattern Comparison Benchmarks

    #region Deep Call Stack Error Propagation Benchmarks

    /// <summary>
    /// Benchmarks Result pattern error propagation through deep call stack.
    /// Demonstrates how Results bubble up through multiple method layers efficiently.
    /// </summary>
    /// <returns>Result from deep call stack operation.</returns>
    [Benchmark]
    public Result<string> DeepCallStackResultError()
    {
        return Level1Result(false);
    }

    /// <summary>
    /// Benchmarks exception propagation through deep call stack.
    /// Demonstrates how exceptions bubble up through multiple catch blocks.
    /// </summary>
    /// <returns>Default value when exception propagates up.</returns>
    [Benchmark]
    public string DeepCallStackExceptionError()
    {
        try
        {
            return Level1Exception(false);
        }
        catch
        {
            return "Error occurred in deep call stack";
        }
    }

    /// <summary>
    /// Benchmarks Result pattern success propagation through deep call stack.
    /// </summary>
    /// <returns>Result from deep call stack operation that succeeds.</returns>
    [Benchmark]
    public Result<string> DeepCallStackResultSuccess()
    {
        return Level1Result(true);
    }

    /// <summary>
    /// Benchmarks direct return through deep call stack (success path).
    /// </summary>
    /// <returns>Value from deep call stack operation that succeeds.</returns>
    [Benchmark]
    public string DeepCallStackDirectSuccess()
    {
        return Level1Exception(true);
    }

    #endregion Deep Call Stack Error Propagation Benchmarks

    #region Bulk Operations Comparison Benchmarks

    /// <summary>
    /// Benchmarks Result pattern for bulk email validation (mixed success/failure).
    /// Processes 100 emails with 25% failure rate, aggregating all errors.
    /// </summary>
    /// <returns>List of validation results.</returns>
    [Benchmark]
    public List<Result<string>> BulkOperationsResultMixed()
    {
        List<Result<string>> results = [];
        
        foreach (string email in _mixedEmails)
        {
            Result<string> result = new ValidationBuilder<string>()
                .RuleFor(x => x, email)
                    .NotEmpty()
                    .EmailAddress()
                .Build(() => email);
            results.Add(result);
        }
        
        return results;
    }

    /// <summary>
    /// Benchmarks exception pattern for bulk email validation (mixed success/failure).
    /// Processes 100 emails with 25% failure rate, catching individual exceptions.
    /// </summary>
    /// <returns>List of email validation results.</returns>
    [Benchmark]
    public List<string> BulkOperationsExceptionMixed()
    {
        List<string> results = [];
        
        foreach (string email in _mixedEmails)
        {
            try
            {
                string validEmail = ValidateEmailWithExceptions(email);
                results.Add(validEmail);
            }
            catch
            {
                results.Add(string.Empty); // Error indicator
            }
        }
        
        return results;
    }

    /// <summary>
    /// Benchmarks Result pattern for bulk database operations simulation.
    /// Simulates 50 database operations with mixed success/failure scenarios.
    /// </summary>
    /// <returns>List of database operation results.</returns>
    [Benchmark]
    public List<Result<DatabaseRecord>> BulkDatabaseOperationsResult()
    {
        List<Result<DatabaseRecord>> results = [];
        
        for (int i = 1; i <= 50; i++)
        {
            Result<DatabaseRecord> result = SimulateDatabaseOperationResult(i);
            results.Add(result);
        }
        
        return results;
    }

    /// <summary>
    /// Benchmarks exception pattern for bulk database operations simulation.
    /// </summary>
    /// <returns>List of database operation results.</returns>
    [Benchmark]
    public List<DatabaseRecord?> BulkDatabaseOperationsException()
    {
        List<DatabaseRecord?> results = [];
        
        for (int i = 1; i <= 50; i++)
        {
            try
            {
                DatabaseRecord record = SimulateDatabaseOperationException(i);
                results.Add(record);
            }
            catch
            {
                results.Add(null);
            }
        }
        
        return results;
    }

    #endregion Bulk Operations Comparison Benchmarks

    #region Real-World Scenario Comparison Benchmarks

    /// <summary>
    /// Benchmarks Result pattern for API call simulation with various error types.
    /// Simulates realistic API scenarios including network timeouts, authentication failures, etc.
    /// </summary>
    /// <returns>API call result.</returns>
    [Benchmark]
    public Result<string> ApiCallSimulationResult()
    {
        return SimulateApiCallResult("https://api.example.com/users/123");
    }

    /// <summary>
    /// Benchmarks exception pattern for API call simulation.
    /// </summary>
    /// <returns>API response or empty string on failure.</returns>
    [Benchmark]
    public string ApiCallSimulationException()
    {
        try
        {
            return SimulateApiCallException("https://api.example.com/users/123");
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Benchmarks Result pattern for business rule validation scenario.
    /// Simulates complex business logic with multiple validation points.
    /// </summary>
    /// <returns>Business operation result.</returns>
    [Benchmark]
    public Result<UserProfile> BusinessRuleValidationResult()
    {
        return ProcessBusinessRuleResult(_validUser);
    }

    /// <summary>
    /// Benchmarks exception pattern for business rule validation scenario.
    /// </summary>
    /// <returns>Processed user profile or null on failure.</returns>
    [Benchmark]
    public UserProfile? BusinessRuleValidationException()
    {
        try
        {
            return ProcessBusinessRuleException(_validUser);
        }
        catch
        {
            return null;
        }
    }

    #endregion Real-World Scenario Comparison Benchmarks

    #region Helper Methods for Benchmarks

    /// <summary>
    /// Computes a simple integer value for benchmarking.
    /// </summary>
    /// <param name="input">Input value to compute with.</param>
    /// <returns>Computed result.</returns>
    private static int ComputeSimpleValue(int input) => input * 2 + 10;

    /// <summary>
    /// Computes a simple value that throws an exception.
    /// </summary>
    /// <returns>Never returns, always throws.</returns>
    /// <exception cref="InvalidOperationException">Always thrown for benchmarking.</exception>
    private static int ComputeSimpleValueWithError() => throw new InvalidOperationException("Computation failed");

    /// <summary>
    /// Processes a string for benchmarking.
    /// </summary>
    /// <param name="input">Input string to process.</param>
    /// <returns>Processed string.</returns>
    private static string ProcessString(string input) => input.ToUpperInvariant();

    /// <summary>
    /// Processes a string that throws an exception.
    /// </summary>
    /// <returns>Never returns, always throws.</returns>
    /// <exception cref="ArgumentException">Always thrown for benchmarking.</exception>
    private static string ProcessStringWithError() => throw new ArgumentException("String processing failed");

    /// <summary>
    /// Creates a user profile for benchmarking.
    /// </summary>
    /// <param name="name">User name.</param>
    /// <param name="email">User email.</param>
    /// <param name="age">User age.</param>
    /// <param name="salary">User salary.</param>
    /// <returns>Created user profile.</returns>
    private static UserProfile CreateUserProfile(string name, string email, int age, decimal salary)
        => new(name, email, age, salary);

    /// <summary>
    /// Creates a user profile that throws an exception.
    /// </summary>
    /// <returns>Never returns, always throws.</returns>
    /// <exception cref="ValidationException">Always thrown for benchmarking.</exception>
    private static UserProfile CreateUserProfileWithError()
        => throw new ValidationException("User profile creation failed");

    /// <summary>
    /// Validates email address using traditional exception throwing approach.
    /// </summary>
    /// <param name="email">Email to validate.</param>
    /// <returns>Valid email if validation passes.</returns>
    /// <exception cref="ArgumentException">Thrown when email is invalid.</exception>
    private static string ValidateEmailWithExceptions(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");
        
        if (!email.Contains('@') || !email.Contains('.'))
            throw new ArgumentException("Email is not valid");
        
        return email;
    }

    /// <summary>
    /// Validates user profile using traditional exception throwing approach.
    /// </summary>
    /// <param name="user">User profile to validate.</param>
    /// <returns>Valid user profile if validation passes.</returns>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    private static UserProfile ValidateUserProfileWithExceptions(UserProfile user)
    {
        if (string.IsNullOrWhiteSpace(user.Name))
            throw new ValidationException("Name is required");
        
        if (user.Name.Length < 2 || user.Name.Length > 100)
            throw new ValidationException("Name must be between 2 and 100 characters");
        
        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ValidationException("Email is required");
        
        if (!user.Email.Contains('@') || !user.Email.Contains('.'))
            throw new ValidationException("Email is not valid");
        
        if (user.Age <= 0 || user.Age >= 120)
            throw new ValidationException("Age must be between 0 and 120");
        
        if (user.Salary < 0)
            throw new ValidationException("Salary cannot be negative");
        
        return user;
    }

    /// <summary>
    /// Simulates a file read operation for benchmarking.
    /// </summary>
    /// <param name="path">File path to simulate reading.</param>
    /// <returns>Simulated file content.</returns>
    /// <exception cref="FileNotFoundException">Thrown for invalid paths.</exception>
    private static string SimulateFileReadOperation(string path)
    {
        if (path.Contains("invalid"))
            throw new FileNotFoundException("File not found");
        
        return "File content from " + path;
    }

    /// <summary>
    /// Level 1 of deep call stack using Result pattern.
    /// </summary>
    /// <param name="shouldSucceed">Whether operation should succeed.</param>
    /// <returns>Result from deep call stack.</returns>
    private static Result<string> Level1Result(bool shouldSucceed) => Level2Result(shouldSucceed);

    /// <summary>
    /// Level 2 of deep call stack using Result pattern.
    /// </summary>
    /// <param name="shouldSucceed">Whether operation should succeed.</param>
    /// <returns>Result from deep call stack.</returns>
    private static Result<string> Level2Result(bool shouldSucceed) => Level3Result(shouldSucceed);

    /// <summary>
    /// Level 3 of deep call stack using Result pattern.
    /// </summary>
    /// <param name="shouldSucceed">Whether operation should succeed.</param>
    /// <returns>Result from deep call stack.</returns>
    private static Result<string> Level3Result(bool shouldSucceed) => Level4Result(shouldSucceed);

    /// <summary>
    /// Level 4 of deep call stack using Result pattern.
    /// </summary>
    /// <param name="shouldSucceed">Whether operation should succeed.</param>
    /// <returns>Result from deep call stack.</returns>
    private static Result<string> Level4Result(bool shouldSucceed) => Level5Result(shouldSucceed);

    /// <summary>
    /// Level 5 of deep call stack using Result pattern.
    /// </summary>
    /// <param name="shouldSucceed">Whether operation should succeed.</param>
    /// <returns>Result from deep call stack operation.</returns>
    private static Result<string> Level5Result(bool shouldSucceed)
    {
        return shouldSucceed 
            ? Result.Success("Success from deep call stack") 
            : Result.Failure<string>("Error occurred at level 5");
    }

    /// <summary>
    /// Level 1 of deep call stack using exception pattern.
    /// </summary>
    /// <param name="shouldSucceed">Whether operation should succeed.</param>
    /// <returns>Value from deep call stack.</returns>
    private static string Level1Exception(bool shouldSucceed) => Level2Exception(shouldSucceed);

    /// <summary>
    /// Level 2 of deep call stack using exception pattern.
    /// </summary>
    /// <param name="shouldSucceed">Whether operation should succeed.</param>
    /// <returns>Value from deep call stack.</returns>
    private static string Level2Exception(bool shouldSucceed) => Level3Exception(shouldSucceed);

    /// <summary>
    /// Level 3 of deep call stack using exception pattern.
    /// </summary>
    /// <param name="shouldSucceed">Whether operation should succeed.</param>
    /// <returns>Value from deep call stack.</returns>
    private static string Level3Exception(bool shouldSucceed) => Level4Exception(shouldSucceed);

    /// <summary>
    /// Level 4 of deep call stack using exception pattern.
    /// </summary>
    /// <param name="shouldSucceed">Whether operation should succeed.</param>
    /// <returns>Value from deep call stack.</returns>
    private static string Level4Exception(bool shouldSucceed) => Level5Exception(shouldSucceed);

    /// <summary>
    /// Level 5 of deep call stack using exception pattern.
    /// </summary>
    /// <param name="shouldSucceed">Whether operation should succeed.</param>
    /// <returns>Value from deep call stack operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when shouldSucceed is false.</exception>
    private static string Level5Exception(bool shouldSucceed)
    {
        return shouldSucceed 
            ? "Success from deep call stack" 
            : throw new InvalidOperationException("Error occurred at level 5");
    }

    /// <summary>
    /// Simulates database operation using Result pattern.
    /// </summary>
    /// <param name="id">Record ID to simulate.</param>
    /// <returns>Database operation result.</returns>
    private static Result<DatabaseRecord> SimulateDatabaseOperationResult(int id)
    {
        // Simulate 20% failure rate
        if (id % 5 == 0)
            return Result.Failure<DatabaseRecord>($"Database error for record {id}");
        
        return Result.Success(new DatabaseRecord(id, $"Data for record {id}"));
    }

    /// <summary>
    /// Simulates database operation using exception pattern.
    /// </summary>
    /// <param name="id">Record ID to simulate.</param>
    /// <returns>Database record.</returns>
    /// <exception cref="InvalidOperationException">Thrown for simulated failures.</exception>
    private static DatabaseRecord SimulateDatabaseOperationException(int id)
    {
        // Simulate 20% failure rate
        if (id % 5 == 0)
            throw new InvalidOperationException($"Database error for record {id}");
        
        return new DatabaseRecord(id, $"Data for record {id}");
    }

    /// <summary>
    /// Simulates API call using Result pattern.
    /// </summary>
    /// <param name="url">API URL to simulate calling.</param>
    /// <returns>API call result.</returns>
    private static Result<string> SimulateApiCallResult(string url)
    {
        // Simulate various API error scenarios
        Random random = new(url.GetHashCode());
        int scenario = random.Next(1, 6);
        
        return scenario switch
        {
            1 => Result.Failure<string>("Network timeout"),
            2 => Result.Failure<string>("Authentication failed"),
            3 => Result.NotFound<string>("Resource not found"),
            4 => Result.ServerError<string>("Internal server error"),
            _ => Result.Success($"API response from {url}")
        };
    }

    /// <summary>
    /// Simulates API call using exception pattern.
    /// </summary>
    /// <param name="url">API URL to simulate calling.</param>
    /// <returns>API response content.</returns>
    /// <exception cref="Exception">Various exceptions for different error scenarios.</exception>
    private static string SimulateApiCallException(string url)
    {
        // Simulate various API error scenarios
        Random random = new(url.GetHashCode());
        int scenario = random.Next(1, 6);
        
        return scenario switch
        {
            1 => throw new TimeoutException("Network timeout"),
            2 => throw new UnauthorizedAccessException("Authentication failed"),
            3 => throw new FileNotFoundException("Resource not found"),
            4 => throw new InvalidOperationException("Internal server error"),
            _ => $"API response from {url}"
        };
    }

    /// <summary>
    /// Processes business rule validation using Result pattern.
    /// </summary>
    /// <param name="user">User to process.</param>
    /// <returns>Business rule processing result.</returns>
    private static Result<UserProfile> ProcessBusinessRuleResult(UserProfile user)
    {
        // Simulate complex business rule validation
        if (user.Age < 18)
            return Result.Failure<UserProfile>("User must be at least 18 years old");
        
        if (user.Salary < 10000)
            return Result.Failure<UserProfile>("Salary must be at least $10,000");
        
        if (user.Name.Contains("test", StringComparison.OrdinalIgnoreCase))
            return Result.Failure<UserProfile>("Test users are not allowed in production");
        
        return Result.Success(user with { Salary = user.Salary * 1.1m }); // Apply business rule
    }

    /// <summary>
    /// Processes business rule validation using exception pattern.
    /// </summary>
    /// <param name="user">User to process.</param>
    /// <returns>Processed user profile.</returns>
    /// <exception cref="BusinessRuleException">Thrown when business rules are violated.</exception>
    private static UserProfile ProcessBusinessRuleException(UserProfile user)
    {
        // Simulate complex business rule validation
        if (user.Age < 18)
            throw new BusinessRuleException("User must be at least 18 years old");
        
        if (user.Salary < 10000)
            throw new BusinessRuleException("Salary must be at least $10,000");
        
        if (user.Name.Contains("test", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Test users are not allowed in production");
        
        return user with { Salary = user.Salary * 1.1m }; // Apply business rule
    }

    #endregion Helper Methods for Benchmarks

    /// <summary>
    /// Custom exception for business rule violations in benchmarking scenarios.
    /// </summary>
    public class BusinessRuleException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessRuleException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public BusinessRuleException(string message) : base(message) { }
    }
}