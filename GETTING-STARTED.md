# Getting Started with FlowRight

Welcome to **FlowRight**, a production-grade Result pattern library for .NET that provides explicit error handling without exceptions. This guide will help you get started with FlowRight and understand how to use its core features effectively.

## Table of Contents

1. [Installation](#installation)
2. [Core Concepts](#core-concepts)
3. [Basic Result Usage](#basic-result-usage)
4. [Pattern Matching](#pattern-matching)
5. [Validation with FlowRight](#validation-with-flowright)
6. [HTTP Integration](#http-integration)
7. [Error Handling Best Practices](#error-handling-best-practices)
8. [Common Patterns](#common-patterns)
9. [Quick Reference](#quick-reference)
10. [Next Steps](#next-steps)

## Installation

FlowRight is distributed as three NuGet packages. Install only what you need:

```bash
# Core Result pattern (required)
dotnet add package FlowRight.Core --prerelease

# Validation support (optional)
dotnet add package FlowRight.Validation --prerelease  

# HTTP integration (optional)
dotnet add package FlowRight.Http --prerelease
```

> **Note**: FlowRight is currently in pre-release (alpha). The API is stable but may change before the v1.0 production release.

### Prerequisites

- **.NET 8.0** or later
- **Nullable reference types** enabled (recommended)

## Core Concepts

### What is the Result Pattern?

The Result pattern is a functional programming concept that makes success and failure explicit. Instead of throwing exceptions for expected failures, methods return a `Result<T>` that represents either:

- **Success**: Contains the expected value
- **Failure**: Contains error information

### Why Use the Result Pattern?

✅ **Explicit Error Handling**: Failures are part of the method signature  
✅ **Performance**: No exception throwing overhead  
✅ **Composability**: Results can be chained and combined  
✅ **Readability**: Clear success/failure paths  
✅ **Type Safety**: Compile-time error checking  

## Basic Result Usage

### Creating Results

```csharp
using FlowRight.Core.Results;

// Success results
Result<int> successResult = Result.Success(42);
Result operationResult = Result.Success();

// Failure results  
Result<int> failureResult = Result.Failure<int>("Something went wrong");
Result operationFailure = Result.Failure("Operation failed");
```

### Checking Result Status

```csharp
Result<string> result = GetUserName(userId);

if (result.IsSuccess)
{
    string userName = result.Value; // Safe to access
    Console.WriteLine($"Hello, {userName}!");
}
else
{
    string error = result.Error; // Contains error message
    Console.WriteLine($"Error: {error}");
}
```

### Working with Non-Generic Results

For operations that don't return a value (like `void` methods):

```csharp
public Result DeleteUser(int userId)
{
    try
    {
        // Perform deletion logic
        _userRepository.Delete(userId);
        return Result.Success();
    }
    catch (UserNotFoundException)
    {
        return Result.Failure("User not found");
    }
    catch (Exception ex)
    {
        return Result.Failure($"Failed to delete user: {ex.Message}");
    }
}

// Usage
Result deleteResult = DeleteUser(123);
if (deleteResult.IsFailure)
{
    Console.WriteLine($"Delete failed: {deleteResult.Error}");
}
```

## Pattern Matching

FlowRight provides two approaches for handling Results: functional (`Match`) and imperative (`Switch`).

### Functional Approach with Match

Use `Match` when you need to transform the result into another value:

```csharp
Result<User> userResult = GetUser(userId);

string message = userResult.Match(
    onSuccess: user => $"Welcome back, {user.Name}!",
    onFailure: error => $"Login failed: {error}"
);

Console.WriteLine(message);
```

### Imperative Approach with Switch

Use `Switch` when you need to perform actions based on the result:

```csharp
Result<Order> orderResult = CreateOrder(request);

orderResult.Switch(
    onSuccess: order => 
    {
        Console.WriteLine($"Order {order.Id} created successfully");
        SendConfirmationEmail(order);
    },
    onFailure: error => 
    {
        Console.WriteLine($"Order creation failed: {error}");
        LogError(error);
    }
);
```

### Advanced Pattern Matching

Handle different result types:

```csharp
Result<Product> productResult = GetProduct(productId);

string response = productResult.Match(
    onSuccess: product => $"Product: {product.Name} - ${product.Price}",
    onFailure: error => productResult.ResultType switch
    {
        ResultType.NotFound => "Product not found",
        ResultType.SecurityFailure => "Access denied",
        ResultType.ValidationFailure => $"Invalid request: {error}",
        _ => $"Error: {error}"
    }
);
```

## Validation with FlowRight

FlowRight includes a powerful fluent validation API that integrates seamlessly with the Result pattern.

### Basic Validation

```csharp
using FlowRight.Validation.Builders;

public Result<User> CreateUser(string name, string email, int age)
{
    return new ValidationBuilder<User>()
        .RuleFor(x => x.Name, name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(50)
        .RuleFor(x => x.Email, email)
            .NotEmpty()
            .EmailAddress()
        .RuleFor(x => x.Age, age)
            .GreaterThan(0)
            .LessThan(120)
        .Build(() => new User(name, email, age));
}
```

### Handling Validation Results

```csharp
Result<User> userResult = CreateUser("", "invalid-email", -5);

userResult.Switch(
    onSuccess: user => Console.WriteLine($"User created: {user.Email}"),
    onFailure: error => Console.WriteLine($"Validation failed: {error}")
);

// Output: "Validation failed: Name cannot be empty. Email must be a valid email address. Age must be greater than 0."
```

### Complex Validation Scenarios

```csharp
public Result<Order> CreateOrder(OrderRequest request)
{
    return new ValidationBuilder<Order>()
        .RuleFor(x => x.CustomerEmail, request.CustomerEmail)
            .NotEmpty()
            .EmailAddress()
        .RuleFor(x => x.Items, request.Items)
            .NotEmpty()
            .Must(items => items.Count <= 100)
            .WithMessage("Orders cannot contain more than 100 items")
        .RuleFor(x => x.TotalAmount, request.TotalAmount)
            .GreaterThan(0)
            .LessThan(10000)
        .RuleFor(x => x.ShippingAddress, request.ShippingAddress)
            .NotEmpty()
            .When(request => request.RequiresShipping)
        .Build(() => new Order(request));
}
```

### Available Validation Rules

FlowRight includes 35+ built-in validation rules:

```csharp
// String validation
.NotEmpty()
.MinimumLength(5)
.MaximumLength(100)
.Length(10, 20)
.Matches(@"^\d{3}-\d{2}-\d{4}$")
.EmailAddress()
.Url()
.AlphaNumeric()

// Numeric validation
.GreaterThan(0)
.GreaterThanOrEqualTo(1)
.LessThan(100)
.LessThanOrEqualTo(99)
.Between(1, 100)
.Positive()
.NotZero()

// Collection validation
.NotEmpty()
.MinCount(1)
.MaxCount(10)
.Unique()

// Custom validation
.Must(value => IsValidBusinessRule(value))
.WithMessage("Custom validation failed")
```

## HTTP Integration

FlowRight provides seamless integration with HTTP clients, automatically converting HTTP responses to Results.

### Basic HTTP Integration

```csharp
using FlowRight.Http.Extensions;

public class WeatherService
{
    private readonly HttpClient _httpClient;

    public async Task<Result<WeatherData>> GetWeatherAsync(string city)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/weather/{city}");
        return await response.ToResultFromJsonAsync<WeatherData>();
    }
}
```

### HTTP Status Code Mapping

FlowRight automatically maps HTTP status codes to appropriate Result types:

```csharp
// 2xx Status Codes → Success
Result<WeatherData> weather = await response.ToResultFromJsonAsync<WeatherData>();
// Returns: Success with deserialized WeatherData

// 400 Bad Request → ValidationFailure
Result<User> user = await response.ToResultFromJsonAsync<User>();
// Returns: ValidationFailure with parsed validation errors

// 401/403 → SecurityFailure  
Result<SecretData> secret = await response.ToResultFromJsonAsync<SecretData>();
// Returns: SecurityFailure("Access denied")

// 404 Not Found → NotFound
Result<Product> product = await response.ToResultFromJsonAsync<Product>();
// Returns: NotFound("Resource not found")

// 5xx Server Error → Failure
Result<Data> data = await response.ToResultFromJsonAsync<Data>();
// Returns: Failure("Server error occurred")
```

### Handling Different Response Types

```csharp
public async Task<Result<ApiResponse<T>>> CallApiAsync<T>(string endpoint)
{
    HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
    
    return response.StatusCode switch
    {
        HttpStatusCode.OK => await response.ToResultFromJsonAsync<ApiResponse<T>>(),
        HttpStatusCode.NotFound => Result.NotFound<ApiResponse<T>>("Resource not found"),
        HttpStatusCode.Unauthorized => Result.SecurityFailure<ApiResponse<T>>("Access denied"),
        _ => await response.ToResultAsync().ContinueWith<ApiResponse<T>>(
            r => Result.Failure<ApiResponse<T>>(r.Result.Error))
    };
}
```

### Working with Problem Details

FlowRight automatically handles ASP.NET Core Problem Details:

```csharp
// Server returns: {"type": "validation", "errors": {"Name": ["Required"], "Email": ["Invalid format"]}}
Result<User> result = await response.ToResultFromJsonAsync<User>();

if (result.IsFailure && result.ResultType == ResultType.ValidationFailure)
{
    // Parsed validation errors are included in the error message
    Console.WriteLine(result.Error);
    // Output: "Name is required. Email must be in a valid format."
}
```

## Error Handling Best Practices

### 1. Use Appropriate Result Types

```csharp
// Good: Specific result types
public Result<User> GetUser(int id) => 
    _users.ContainsKey(id) 
        ? Result.Success(_users[id])
        : Result.NotFound<User>("User not found");

public Result<User> ValidateUser(UserRequest request) =>
    string.IsNullOrEmpty(request.Name)
        ? Result.ValidationFailure<User>("Name is required")
        : Result.Success(new User(request.Name));

// Avoid: Generic failures for specific scenarios
public Result<User> GetUser(int id) => 
    Result.Failure<User>("Something went wrong"); // Too generic
```

### 2. Combine Multiple Results

```csharp
public Result<Order> CreateOrder(OrderRequest request)
{
    Result<Customer> customerResult = GetCustomer(request.CustomerId);
    Result<Product> productResult = GetProduct(request.ProductId);
    Result<decimal> priceResult = CalculatePrice(request.Items);

    // Combine results - if any fail, return the first failure
    Result combined = Result.Combine(customerResult, productResult, priceResult);
    if (combined.IsFailure)
        return Result.Failure<Order>(combined.Error);

    // All successful - create the order
    return Result.Success(new Order(
        customerResult.Value, 
        productResult.Value, 
        priceResult.Value
    ));
}
```

### 3. Chain Operations

```csharp
public Result<string> ProcessUserData(int userId)
{
    return GetUser(userId)
        .Match(
            onSuccess: user => ValidateUser(user)
                .Match(
                    onSuccess: validUser => FormatUserData(validUser),
                    onFailure: error => Result.Failure<string>(error)
                ),
            onFailure: error => Result.Failure<string>(error)
        );
}
```

### 4. Handle Different Error Scenarios

```csharp
public async Task<IActionResult> GetUserAsync(int id)
{
    Result<User> result = await _userService.GetUserAsync(id);

    return result.ResultType switch
    {
        ResultType.Success => Ok(result.Value),
        ResultType.NotFound => NotFound($"User {id} not found"),
        ResultType.SecurityFailure => Forbid(),
        ResultType.ValidationFailure => BadRequest(result.Error),
        _ => StatusCode(500, "An error occurred")
    };
}
```

## Common Patterns

### Repository Pattern with Results

```csharp
public interface IUserRepository
{
    Task<Result<User>> GetByIdAsync(int id);
    Task<Result<User>> CreateAsync(User user);
    Task<Result> UpdateAsync(User user);
    Task<Result> DeleteAsync(int id);
}

public class UserRepository : IUserRepository
{
    public async Task<Result<User>> GetByIdAsync(int id)
    {
        User? user = await _dbContext.Users.FindAsync(id);
        return user is not null 
            ? Result.Success(user)
            : Result.NotFound<User>($"User with ID {id} not found");
    }
}
```

### Service Layer Pattern

```csharp
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;

    public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        // Validate input
        Result<User> validationResult = ValidateCreateUserRequest(request);
        if (validationResult.IsFailure)
            return validationResult;

        // Create user
        Result<User> createResult = await _repository.CreateAsync(validationResult.Value);
        if (createResult.IsFailure)
            return createResult;

        // Send welcome email (don't fail if this fails)
        Result emailResult = await _emailService.SendWelcomeEmailAsync(createResult.Value);
        if (emailResult.IsFailure)
        {
            // Log warning but continue
            _logger.LogWarning("Failed to send welcome email: {Error}", emailResult.Error);
        }

        return createResult;
    }
}
```

### API Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        Result<User> result = await _userService.GetUserAsync(id);
        
        return result.Match<ActionResult<User>>(
            onSuccess: user => Ok(user),
            onFailure: error => result.ResultType switch
            {
                ResultType.NotFound => NotFound(error),
                ResultType.SecurityFailure => Forbid(),
                _ => BadRequest(error)
            }
        );
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(CreateUserRequest request)
    {
        Result<User> result = await _userService.CreateUserAsync(request);
        
        return result.Match<ActionResult<User>>(
            onSuccess: user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, user),
            onFailure: error => BadRequest(error)
        );
    }
}
```

## Quick Reference

### Essential Result Methods

```csharp
// Creating Results
Result.Success<T>(value)
Result.Failure<T>(message)
Result.NotFound<T>(message)
Result.ValidationFailure<T>(message)
Result.SecurityFailure<T>(message)

// Checking Results  
result.IsSuccess
result.IsFailure
result.Value      // Only safe when IsSuccess
result.Error      // Only meaningful when IsFailure
result.ResultType

// Pattern Matching
result.Match(onSuccess, onFailure)
result.Switch(onSuccess, onFailure)

// Combining Results
Result.Combine(result1, result2, result3)
```

### Essential Validation Rules

```csharp
// String Rules
.NotEmpty()
.MinimumLength(n)
.MaximumLength(n)
.EmailAddress()
.Url()

// Numeric Rules  
.GreaterThan(n)
.LessThan(n)
.Between(min, max)
.Positive()

// Collection Rules
.NotEmpty()
.MinCount(n)
.MaxCount(n)

// Custom Rules
.Must(predicate)
.WithMessage(message)
.When(condition)
```

### HTTP Integration

```csharp
// Convert HTTP response to Result
await response.ToResultAsync()
await response.ToResultFromJsonAsync<T>()

// Status code mappings
// 2xx → Success
// 400 → ValidationFailure  
// 401/403 → SecurityFailure
// 404 → NotFound
// 5xx → Failure
```

## Next Steps

Now that you understand FlowRight basics, explore these advanced topics:

1. **[Migration Guide](MIGRATION-GUIDE.md)** - Moving from exception-based code to Results
2. **[Best Practices](BEST-PRACTICES.md)** - Advanced patterns and architectural guidance  
3. **[API Reference](docs/api/README.md)** - Complete API documentation
4. **[Performance Guide](docs/performance.md)** - Optimization tips and benchmarks

### Sample Projects

Check out these example projects that demonstrate FlowRight in action:

- **Web API**: ASP.NET Core API using Results pattern
- **Blazor App**: Frontend application with validation
- **Console App**: Command-line tool with error handling

### Getting Help

- **Issues**: [GitHub Issues](https://github.com/georgepharrison/FlowRight/issues)
- **Discussions**: [GitHub Discussions](https://github.com/georgepharrison/FlowRight/discussions)  
- **Stack Overflow**: Tag questions with `flowright`

---

**Happy coding with FlowRight!** 🚀