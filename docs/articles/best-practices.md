# FlowRight Best Practices Guide

A comprehensive guide to using FlowRight effectively in production applications, covering advanced patterns, performance considerations, and architectural guidance.

## Table of Contents

1. [Core Result Pattern Best Practices](#core-result-pattern-best-practices)
2. [Validation Composition Patterns](#validation-composition-patterns)
3. [Error Handling and Design Principles](#error-handling-and-design-principles)
4. [Async/Await Patterns](#asyncawait-patterns)
5. [HTTP Integration Best Practices](#http-integration-best-practices)
6. [Testing Strategies](#testing-strategies)
7. [Performance Considerations](#performance-considerations)
8. [Anti-Patterns and Common Pitfalls](#anti-patterns-and-common-pitfalls)
9. [Architectural Guidance](#architectural-guidance)
10. [Decision Trees](#decision-trees)

## Core Result Pattern Best Practices

### When to Use Results vs Exceptions

#### ✅ Use Results For:
- **Business logic validation** - Input validation, domain rule enforcement
- **Expected failure scenarios** - User not found, invalid credentials
- **Recoverable errors** - Network timeouts, temporary service unavailability
- **Composed operations** - Multiple validation steps that need aggregation
- **API boundaries** - Controller actions, service methods
- **Integration points** - External service calls, database operations

```csharp
// ✅ Good: Expected business scenario
public Result<User> GetUser(int userId)
{
    User? user = _repository.FindById(userId);
    return user is not null 
        ? Result.Success(user)
        : Result.NotFound($"User with ID {userId} not found");
}

// ✅ Good: Domain validation
public Result<Order> PlaceOrder(PlaceOrderRequest request)
{
    return new ValidationBuilder<Order>()
        .RuleFor(x => x.CustomerId, request.CustomerId)
            .NotEmpty()
            .Must(CustomerExists, "Customer must exist")
        .RuleFor(x => x.Items, request.Items)
            .NotEmpty("Order must contain at least one item")
            .Must(AllItemsInStock, "All items must be in stock")
        .Build(() => new Order(request.CustomerId, request.Items));
}
```

#### ❌ Use Exceptions For:
- **Programming errors** - Null reference exceptions, argument out of range
- **System-level failures** - Out of memory, stack overflow
- **Unrecoverable errors** - Database connection failure, configuration errors
- **Framework integration** - ASP.NET Core model binding, dependency injection

```csharp
// ❌ Bad: Programming error should use exception
public Result<string> GetSubstring(string input, int start, int length)
{
    if (start < 0) 
        return Result.Failure<string>("Start cannot be negative");
    // Should use ArgumentOutOfRangeException instead
}

// ✅ Good: Programming error uses exception
public string GetSubstring(string input, int start, int length)
{
    ArgumentOutOfRangeException.ThrowIfNegative(start);
    ArgumentOutOfRangeException.ThrowIfNegative(length);
    
    if (start + length > input.Length)
        return input[start..];
    
    return input.Substring(start, length);
}
```

### Result Composition Patterns

#### Sequential Validation with Early Exit
```csharp
// ✅ Good: Early exit pattern
public async Task<Result<ProcessedOrder>> ProcessOrderAsync(CreateOrderRequest request)
{
    Result<Customer> customerResult = await ValidateCustomerAsync(request.CustomerId);
    if (customerResult.IsFailure) return customerResult.ToResult<ProcessedOrder>();
    
    Result<Inventory> inventoryResult = await ValidateInventoryAsync(request.Items);
    if (inventoryResult.IsFailure) return inventoryResult.ToResult<ProcessedOrder>();
    
    Result<Payment> paymentResult = await ProcessPaymentAsync(request.Payment);
    if (paymentResult.IsFailure) return paymentResult.ToResult<ProcessedOrder>();
    
    return Result.Success(new ProcessedOrder(customerResult.Value!, request.Items, paymentResult.Value!));
}
```

#### Parallel Validation with Error Aggregation
```csharp
// ✅ Good: Parallel validation with aggregation
public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
{
    // Run validations in parallel
    Task<Result> emailValidation = ValidateEmailAsync(request.Email);
    Task<Result> usernameValidation = ValidateUsernameAsync(request.Username);
    Task<Result> passwordValidation = ValidatePasswordAsync(request.Password);
    
    Result[] results = await Task.WhenAll(emailValidation, usernameValidation, passwordValidation);
    
    // Combine all results - will aggregate all failures
    Result combinedResult = Result.Combine(results);
    
    return combinedResult.IsSuccess 
        ? Result.Success(new User(request.Email, request.Username))
        : Result.Failure<User>(combinedResult.Error, combinedResult.FailureType);
}
```

#### Functional Composition with Map and Bind
```csharp
// ✅ Good: Functional composition
public async Task<Result<string>> ProcessUserDataAsync(int userId)
{
    return await GetUserAsync(userId)
        .MapAsync(async user => await EnrichUserDataAsync(user))
        .ThenAsync(async enrichedUser => await FormatUserDisplayAsync(enrichedUser))
        .ThenAsync(async displayData => await LocalizeDisplayAsync(displayData));
}
```

## Validation Composition Patterns

### Basic ValidationBuilder Usage

```csharp
// ✅ Good: Clear, readable validation
public Result<Product> CreateProduct(CreateProductRequest request)
{
    return new ValidationBuilder<Product>()
        .RuleFor(x => x.Name, request.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Must(BeUniqueProductName, "Product name must be unique")
        .RuleFor(x => x.Price, request.Price)
            .GreaterThan(0)
            .Precision(2, 2)
        .RuleFor(x => x.Category, request.CategoryId)
            .NotEmpty()
            .Must(CategoryExists, "Category must exist")
        .Build(() => new Product(request.Name, request.Price, request.CategoryId));
}
```

### Complex Validation with Result Composition

```csharp
// ✅ Good: Compose validated sub-objects
public Result<Order> CreateComplexOrder(CreateOrderRequest request)
{
    return new ValidationBuilder<Order>()
        .RuleFor(x => x.Customer, Customer.Create(request.Customer), out Customer? validatedCustomer)
        .RuleFor(x => x.ShippingAddress, Address.Create(request.ShippingAddress), out Address? validatedAddress)
        .RuleFor(x => x.Items, OrderItems.Create(request.Items), out OrderItems? validatedItems)
        .Build(() => new Order(validatedCustomer!, validatedAddress!, validatedItems!));
}
```

### Conditional Validation

```csharp
// ✅ Good: Context-aware conditional validation
public Result<UserProfile> UpdateProfile(UpdateProfileRequest request, User currentUser)
{
    ValidationBuilder<UserProfile> builder = new();
    
    return builder
        .RuleFor(x => x.Email, request.Email)
            .NotEmpty()
            .EmailAddress()
            .When(value => value != currentUser.Email) // Only validate if changing
            .Must(BeUniqueEmail, "Email must be unique")
        .RuleFor(x => x.Phone, request.Phone)
            .NotEmpty()
            .When(value => request.RequirePhoneVerification)
            .Must(BeValidPhoneNumber, "Phone number format is invalid")
        .RuleFor(x => x.TwoFactorEnabled, request.EnableTwoFactor)
            .Must(value => !value || !string.IsNullOrEmpty(request.Phone), 
                  "Phone number required when enabling two-factor authentication")
        .Build(() => new UserProfile(request.Email, request.Phone, request.EnableTwoFactor));
}
```

### Custom Validation Rules

```csharp
// ✅ Good: Reusable custom validation
public class BusinessRules
{
    public static IRule<decimal> ValidBusinessExpense(decimal yearlyBudget) =>
        new MustRule<decimal>(
            value => value > 0 && value <= yearlyBudget,
            $"Expense must be between 0 and {yearlyBudget:C}"
        );
    
    public static IRule<DateTime> ValidFutureDate() =>
        new MustRule<DateTime>(
            value => value > DateTime.UtcNow,
            "Date must be in the future"
        );
    
    public static IRule<string> ValidProjectCode() =>
        new MustRule<string>(
            value => value?.Length == 6 && value.All(char.IsLetterOrDigit),
            "Project code must be 6 alphanumeric characters"
        );
}

// Usage
public Result<Project> CreateProject(CreateProjectRequest request)
{
    return new ValidationBuilder<Project>()
        .RuleFor(x => x.Code, request.Code)
            .Custom(BusinessRules.ValidProjectCode())
        .RuleFor(x => x.Budget, request.Budget)
            .Custom(BusinessRules.ValidBusinessExpense(request.YearlyBudget))
        .RuleFor(x => x.StartDate, request.StartDate)
            .Custom(BusinessRules.ValidFutureDate())
        .Build(() => new Project(request.Code, request.Budget, request.StartDate));
}
```

## Error Handling and Design Principles

### Error Message Design Principles

#### User-Friendly Messages
```csharp
// ✅ Good: Clear, actionable error messages
public Result<Account> CreateAccount(string email, string password)
{
    return new ValidationBuilder<Account>()
        .RuleFor(x => x.Email, email)
            .NotEmpty("Please enter your email address")
            .EmailAddress("Please enter a valid email address")
            .Must(BeUniqueEmail, "An account with this email already exists. Please use a different email or try signing in.")
        .RuleFor(x => x.Password, password)
            .NotEmpty("Please create a password")
            .MinimumLength(8, "Password must be at least 8 characters long")
            .Must(ContainSpecialCharacter, "Password must contain at least one special character (!@#$%^&*)")
            .Must(ContainUppercase, "Password must contain at least one uppercase letter")
        .Build(() => new Account(email, password));
}
```

#### Technical vs Business Error Context
```csharp
// ✅ Good: Appropriate error context for audience
public async Task<Result<PaymentResult>> ProcessPaymentAsync(PaymentRequest request)
{
    try 
    {
        PaymentResponse response = await _paymentGateway.ProcessAsync(request);
        
        return response.Status switch
        {
            PaymentStatus.Approved => Result.Success(new PaymentResult(response.TransactionId)),
            PaymentStatus.Declined => Result.Failure("Payment was declined. Please check your card details and try again."),
            PaymentStatus.InsufficientFunds => Result.Failure("Payment failed due to insufficient funds."),
            PaymentStatus.ExpiredCard => Result.Failure("Payment failed because the card has expired."),
            _ => Result.Failure("Payment could not be processed at this time. Please try again later.")
        };
    }
    catch (PaymentGatewayException ex)
    {
        // Log technical details but return user-friendly message
        _logger.LogError(ex, "Payment gateway error for request {RequestId}", request.Id);
        return Result.Failure("Payment service is temporarily unavailable. Please try again later.");
    }
}
```

### Structured Error Handling

#### Error Categorization Strategy
```csharp
// ✅ Good: Consistent error categorization
public class OrderService
{
    public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request)
    {
        // Validation errors - user input issues
        Result<Order> validationResult = ValidateOrderRequest(request);
        if (validationResult.IsFailure) return validationResult;
        
        // Business rule errors - domain logic violations
        Result businessRulesResult = await ValidateBusinessRulesAsync(request);
        if (businessRulesResult.IsFailure) 
            return Result.Failure<Order>(businessRulesResult.Error);
        
        // System errors - infrastructure issues
        try 
        {
            Order order = await _repository.CreateAsync(request);
            return Result.Success(order);
        }
        catch (DatabaseException ex)
        {
            _logger.LogError(ex, "Database error creating order");
            return Result.ServerError<Order>("Unable to create order due to system error");
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<Order>("Order creation was cancelled", ResultFailureType.OperationCanceled);
        }
    }
}
```

### Error Recovery Strategies

```csharp
// ✅ Good: Graceful degradation and retry logic
public class UserService
{
    public async Task<Result<UserProfile>> GetEnrichedUserProfileAsync(int userId)
    {
        // Core user data is required
        Result<User> userResult = await GetUserAsync(userId);
        if (userResult.IsFailure) return userResult.ToResult<UserProfile>();
        
        User user = userResult.Value!;
        
        // Optional enrichment data - degrade gracefully
        Result<Preferences> preferencesResult = await GetUserPreferencesAsync(userId);
        Result<Statistics> statisticsResult = await GetUserStatisticsAsync(userId);
        
        UserProfile profile = new(
            user,
            preferencesResult.IsSuccess ? preferencesResult.Value : UserPreferences.Default,
            statisticsResult.IsSuccess ? statisticsResult.Value : UserStatistics.Empty
        );
        
        return Result.Success(profile);
    }
    
    public async Task<Result<T>> WithRetryAsync<T>(Func<Task<Result<T>>> operation, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            Result<T> result = await operation();
            
            if (result.IsSuccess || result.FailureType != ResultFailureType.ServerError)
                return result;
            
            if (attempt < maxRetries)
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
        }
        
        return Result.ServerError<T>("Operation failed after multiple retries");
    }
}
```

## Async/Await Patterns

### Async Result Composition

```csharp
// ✅ Good: Clean async composition
public async Task<Result<ProcessedDocument>> ProcessDocumentAsync(Document document)
{
    return await ValidateDocumentAsync(document)
        .ThenAsync(async validDoc => await ExtractMetadataAsync(validDoc))
        .ThenAsync(async docWithMeta => await ApplyTransformationsAsync(docWithMeta))
        .ThenAsync(async transformedDoc => await SaveProcessedDocumentAsync(transformedDoc));
}

// Supporting extension method pattern
public static class AsyncResultExtensions
{
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> nextOperation)
    {
        Result<TIn> result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess 
            ? await nextOperation(result.Value!).ConfigureAwait(false)
            : Result.Failure<TOut>(result.Error, result.FailureType);
    }
}
```

### Async Pattern Matching

```csharp
// ✅ Good: Async pattern matching with proper resource management
public async Task<IActionResult> ProcessOrderAsync(CreateOrderRequest request)
{
    Result<Order> orderResult = await _orderService.CreateOrderAsync(request);
    
    return await orderResult.MatchAsync(
        onSuccess: async order => {
            await _eventPublisher.PublishAsync(new OrderCreatedEvent(order.Id));
            await _emailService.SendOrderConfirmationAsync(order);
            return Ok(new OrderResponse(order));
        },
        onFailure: async error => {
            await _auditService.LogFailureAsync("OrderCreation", error, request);
            return BadRequest(error);
        }
    );
}
```

### Cancellation Token Handling

```csharp
// ✅ Good: Proper cancellation support throughout the chain
public async Task<Result<Report>> GenerateReportAsync(
    ReportRequest request, 
    CancellationToken cancellationToken = default)
{
    try
    {
        Result<ReportData> dataResult = await GatherReportDataAsync(request, cancellationToken);
        if (dataResult.IsFailure) return dataResult.ToResult<Report>();
        
        Result<ProcessedData> processedResult = await ProcessDataAsync(dataResult.Value!, cancellationToken);
        if (processedResult.IsFailure) return processedResult.ToResult<Report>();
        
        Result<Report> reportResult = await FormatReportAsync(processedResult.Value!, cancellationToken);
        return reportResult;
    }
    catch (OperationCanceledException)
    {
        return Result.Failure<Report>("Report generation was cancelled", ResultFailureType.OperationCanceled);
    }
}

// Async validation with cancellation
public async Task<Result<User>> ValidateAndCreateUserAsync(
    CreateUserRequest request, 
    CancellationToken cancellationToken = default)
{
    ValidationBuilder<User> builder = new();
    
    return await builder
        .RuleForAsync(x => x.Email, request.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, ct) => await IsUniqueEmailAsync(email, ct), 
                      "Email must be unique", cancellationToken)
        .RuleForAsync(x => x.Username, request.Username)
            .NotEmpty()
            .MustAsync(async (username, ct) => await IsUniqueUsernameAsync(username, ct), 
                      "Username must be unique", cancellationToken)
        .BuildAsync(() => new User(request.Email, request.Username), cancellationToken);
}
```

## HTTP Integration Best Practices

### Controller Action Patterns

```csharp
// ✅ Good: Consistent controller pattern
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService) => _userService = userService;
    
    [HttpPost]
    public async Task<IActionResult> CreateUserAsync(CreateUserRequest request)
    {
        Result<User> result = await _userService.CreateUserAsync(request);
        
        return result.Match(
            onSuccess: user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserResponse(user)),
            onError: error => Problem(detail: error, statusCode: 500),
            onSecurityException: error => Problem(detail: "Access denied", statusCode: 403),
            onValidationException: errors => ValidationProblem(errors.ToDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value)),
            onOperationCanceledException: error => Problem(detail: "Request cancelled", statusCode: 408)
        );
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserAsync(int id)
    {
        Result<User> result = await _userService.GetUserAsync(id);
        
        return result.Match(
            onSuccess: user => Ok(new UserResponse(user)),
            onFailure: error => result.FailureType switch
            {
                ResultFailureType.NotFound => NotFound(error),
                ResultFailureType.Security => Forbid(error),
                _ => Problem(detail: error)
            }
        );
    }
}
```

### HTTP Client Integration

```csharp
// ✅ Good: HTTP client with Result pattern
public class ExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiClient> _logger;
    
    public async Task<Result<ApiResponse<T>>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint, cancellationToken);
            return await response.ToResultFromJsonAsync<ApiResponse<T>>(cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP request failed for endpoint {Endpoint}", endpoint);
            return Result.Failure<ApiResponse<T>>($"Failed to communicate with external service: {ex.Message}");
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Result.Failure<ApiResponse<T>>("Request was cancelled", ResultFailureType.OperationCanceled);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("HTTP request timed out for endpoint {Endpoint}", endpoint);
            return Result.Failure<ApiResponse<T>>("Request timed out", ResultFailureType.ServerError);
        }
    }
    
    public async Task<Result<T>> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
    {
        try
        {
            string json = JsonSerializer.Serialize(data);
            StringContent content = new(json, Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            
            return await response.MatchAsync(
                onSuccess: async () => {
                    T? result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
                    return result is not null 
                        ? Result.Success(result) 
                        : Result.Failure<T>("Empty response from server");
                },
                onFailure: async error => {
                    string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("API request failed: {Error}. Response: {Response}", error, responseBody);
                    return Result.Failure<T>(error);
                }
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error during API request to {Endpoint}", endpoint);
            return Result.Failure<T>($"Unexpected error: {ex.Message}");
        }
    }
}
```

### API Response Standardization

```csharp
// ✅ Good: Consistent API response structure
public static class ApiResponseExtensions
{
    public static IActionResult ToApiResponse<T>(this Result<T> result) =>
        result.Match(
            onSuccess: value => new OkObjectResult(new ApiResponse<T>
            {
                Success = true,
                Data = value,
                Message = "Operation completed successfully"
            }),
            onError: error => new ObjectResult(new ApiResponse<T>
            {
                Success = false,
                Message = error,
                Errors = null
            }) { StatusCode = 500 },
            onSecurityException: error => new ObjectResult(new ApiResponse<T>
            {
                Success = false,
                Message = "Access denied",
                Errors = null
            }) { StatusCode = 403 },
            onValidationException: errors => new BadRequestObjectResult(new ApiResponse<T>
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors
            }),
            onOperationCanceledException: error => new ObjectResult(new ApiResponse<T>
            {
                Success = false,
                Message = "Operation was cancelled",
                Errors = null
            }) { StatusCode = 408 }
        );
    
    public static IActionResult ToApiResponse(this Result result) =>
        result.Match(
            onSuccess: () => new OkObjectResult(new ApiResponse
            {
                Success = true,
                Message = "Operation completed successfully"
            }),
            onFailure: error => new ObjectResult(new ApiResponse
            {
                Success = false,
                Message = error
            }) { StatusCode = GetStatusCodeForFailureType(result.FailureType) }
        );
    
    private static int GetStatusCodeForFailureType(ResultFailureType failureType) =>
        failureType switch
        {
            ResultFailureType.Validation => 400,
            ResultFailureType.NotFound => 404,
            ResultFailureType.Security => 403,
            ResultFailureType.OperationCanceled => 408,
            _ => 500
        };
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public IDictionary<string, string[]>? Errors { get; set; }
}
```

## Testing Strategies

### Unit Testing Result-Based Code

```csharp
// ✅ Good: Comprehensive Result testing patterns
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository = new();
    private readonly UserService _service;
    
    public UserServiceTests()
    {
        _service = new UserService(_mockRepository.Object);
    }
    
    [Fact]
    public async Task CreateUserAsync_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        CreateUserRequest request = new("test@example.com", "validpassword");
        User expectedUser = new(request.Email, request.Password);
        
        _mockRepository.Setup(x => x.EmailExistsAsync(request.Email))
                      .ReturnsAsync(false);
        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<User>()))
                      .ReturnsAsync(expectedUser);
        
        // Act
        Result<User> result = await _service.CreateUserAsync(request);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Email.ShouldBe(request.Email);
    }
    
    [Fact]
    public async Task CreateUserAsync_WithDuplicateEmail_ReturnsValidationFailure()
    {
        // Arrange
        CreateUserRequest request = new("duplicate@example.com", "validpassword");
        
        _mockRepository.Setup(x => x.EmailExistsAsync(request.Email))
                      .ReturnsAsync(true);
        
        // Act
        Result<User> result = await _service.CreateUserAsync(request);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldContainKey("Email");
        result.Failures["Email"].ShouldContain(error => error.Contains("already exists"));
    }
    
    [Theory]
    [InlineData("", "Password required")]
    [InlineData("short", "Password must be at least 8 characters")]
    [InlineData("nouppercase", "Password must contain uppercase")]
    public async Task CreateUserAsync_WithInvalidPassword_ReturnsValidationError(
        string password, string expectedError)
    {
        // Arrange
        CreateUserRequest request = new("test@example.com", password);
        
        _mockRepository.Setup(x => x.EmailExistsAsync(request.Email))
                      .ReturnsAsync(false);
        
        // Act
        Result<User> result = await _service.CreateUserAsync(request);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Error.ShouldContain(expectedError);
    }
}
```

### Integration Testing with Results

```csharp
// ✅ Good: Integration test patterns
public class UserControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public UserControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        CreateUserRequest request = new("integration@example.com", "ValidPassword123!");
        StringContent content = new(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );
        
        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/users", content);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        
        string responseContent = await response.Content.ReadAsStringAsync();
        UserResponse? userResponse = JsonSerializer.Deserialize<UserResponse>(responseContent);
        
        userResponse.ShouldNotBeNull();
        userResponse.Email.ShouldBe(request.Email);
    }
    
    [Fact]
    public async Task CreateUser_WithInvalidData_ReturnsValidationProblem()
    {
        // Arrange
        CreateUserRequest request = new("invalid-email", "");
        StringContent content = new(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );
        
        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/users", content);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        
        string responseContent = await response.Content.ReadAsStringAsync();
        ValidationProblemDetails? problem = JsonSerializer.Deserialize<ValidationProblemDetails>(responseContent);
        
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("Email");
        problem.Errors.ShouldContainKey("Password");
    }
}
```

### Testing Custom Validators

```csharp
// ✅ Good: Custom validator testing
public class BusinessRuleValidatorTests
{
    [Fact]
    public void ValidBusinessExpense_WithValidAmount_ReturnsNull()
    {
        // Arrange
        decimal yearlyBudget = 100000m;
        decimal expense = 5000m;
        IRule<decimal> rule = BusinessRules.ValidBusinessExpense(yearlyBudget);
        
        // Act
        string? error = rule.Validate(expense, "Expense");
        
        // Assert
        error.ShouldBeNull();
    }
    
    [Theory]
    [InlineData(0, "Expense must be between 0 and")]
    [InlineData(-100, "Expense must be between 0 and")]
    [InlineData(200000, "Expense must be between 0 and")]
    public void ValidBusinessExpense_WithInvalidAmount_ReturnsError(
        decimal expense, string expectedErrorPart)
    {
        // Arrange
        decimal yearlyBudget = 100000m;
        IRule<decimal> rule = BusinessRules.ValidBusinessExpense(yearlyBudget);
        
        // Act
        string? error = rule.Validate(expense, "Expense");
        
        // Assert
        error.ShouldNotBeNull();
        error.ShouldContain(expectedErrorPart);
    }
}
```

## Performance Considerations

### Memory Allocation Optimization

```csharp
// ✅ Good: Minimize allocations on success path
public static class OptimizedResultFactories
{
    // Pre-allocated common success results
    private static readonly Result SuccessResult = Result.Success();
    private static readonly Result<bool> TrueResult = Result.Success(true);
    private static readonly Result<bool> FalseResult = Result.Success(false);
    
    public static Result GetCachedSuccess() => SuccessResult;
    public static Result<bool> GetCachedBoolResult(bool value) => value ? TrueResult : FalseResult;
    
    // String interning for common error messages
    private static readonly string RequiredFieldError = string.Intern("This field is required");
    private static readonly string InvalidFormatError = string.Intern("Invalid format");
    
    public static Result<T> RequiredFieldFailure<T>() =>
        Result.Failure<T>(RequiredFieldError, ResultFailureType.Validation);
}

// ✅ Good: Efficient validation with minimal allocations
public class PerformantValidator<T>
{
    private readonly List<string> _reusableErrorList = new(8); // Pre-allocated
    private readonly Dictionary<string, string[]> _reusableErrorDict = new(8);
    
    public Result<T> ValidateFast(T value, Func<T, T> factory)
    {
        _reusableErrorList.Clear();
        _reusableErrorDict.Clear();
        
        // Perform validations, adding to reusable collections
        ValidateRequired(value, _reusableErrorList);
        ValidateFormat(value, _reusableErrorList);
        
        if (_reusableErrorList.Count > 0)
        {
            _reusableErrorDict["Value"] = _reusableErrorList.ToArray();
            return Result.Failure<T>(_reusableErrorDict);
        }
        
        return Result.Success(factory(value));
    }
}
```

### Async Performance Patterns

```csharp
// ✅ Good: Efficient async patterns with Results
public class PerformantAsyncService
{
    // Use ValueTask for potentially synchronous operations
    public async ValueTask<Result<User>> GetUserFromCacheAsync(int userId)
    {
        if (_cache.TryGetValue(userId, out User? cachedUser))
        {
            return Result.Success(cachedUser); // Synchronous completion
        }
        
        User user = await _database.GetUserAsync(userId);
        _cache.Set(userId, user);
        return Result.Success(user);
    }
    
    // Batch operations for efficiency
    public async Task<Result<IEnumerable<ProcessedItem>>> ProcessItemsBatchAsync(
        IEnumerable<Item> items, 
        int batchSize = 100)
    {
        List<ProcessedItem> results = new();
        List<string> errors = new();
        
        await foreach (Item[] batch in items.Chunk(batchSize))
        {
            Task<Result<ProcessedItem>>[] tasks = batch
                .Select(ProcessSingleItemAsync)
                .ToArray();
                
            Result<ProcessedItem>[] batchResults = await Task.WhenAll(tasks);
            
            foreach (Result<ProcessedItem> result in batchResults)
            {
                if (result.IsSuccess)
                {
                    results.Add(result.Value!);
                }
                else
                {
                    errors.Add(result.Error);
                }
            }
        }
        
        return errors.Count > 0 
            ? Result.Failure<IEnumerable<ProcessedItem>>(string.Join("; ", errors))
            : Result.Success<IEnumerable<ProcessedItem>>(results);
    }
    
    // ConfigureAwait(false) for library code
    public async Task<Result<Data>> GetDataAsync()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("/api/data").ConfigureAwait(false);
            Result<Data> result = await response.ToResultFromJsonAsync<Data>().ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure<Data>($"Failed to get data: {ex.Message}");
        }
    }
}
```

### Validation Performance Optimization

```csharp
// ✅ Good: Optimized validation pipeline
public class FastValidationBuilder<T>
{
    private readonly List<Func<T, ValidationResult>> _validators = new();
    private readonly bool _shortCircuit;
    
    public FastValidationBuilder(bool shortCircuit = false)
    {
        _shortCircuit = shortCircuit;
    }
    
    public FastValidationBuilder<T> AddValidator(Func<T, ValidationResult> validator)
    {
        _validators.Add(validator);
        return this;
    }
    
    public Result<T> Validate(T value)
    {
        if (_shortCircuit)
        {
            // Fast-fail on first error
            foreach (Func<T, ValidationResult> validator in _validators)
            {
                ValidationResult result = validator(value);
                if (!result.IsValid)
                    return Result.Failure<T>(result.Error);
            }
        }
        else
        {
            // Collect all errors (more allocations but complete feedback)
            List<string>? errors = null;
            
            foreach (Func<T, ValidationResult> validator in _validators)
            {
                ValidationResult result = validator(value);
                if (!result.IsValid)
                {
                    errors ??= new List<string>();
                    errors.Add(result.Error);
                }
            }
            
            if (errors?.Count > 0)
                return Result.Failure<T>(string.Join("; ", errors));
        }
        
        return Result.Success(value);
    }
}
```

## Anti-Patterns and Common Pitfalls

### ❌ Anti-Pattern: Result Exception Anti-Pattern

```csharp
// ❌ Bad: Throwing exceptions from Results defeats the purpose
public Result<User> GetUser(int id)
{
    if (id <= 0)
        throw new ArgumentException("ID must be positive"); // Don't do this!
    
    User? user = _repository.Find(id);
    return user is not null ? Result.Success(user) : Result.NotFound("User not found");
}

// ✅ Good: Keep it consistent with Result pattern
public Result<User> GetUser(int id)
{
    if (id <= 0)
        return Result.Failure<User>("User ID must be a positive number");
    
    User? user = _repository.Find(id);
    return user is not null ? Result.Success(user) : Result.NotFound("User not found");
}
```

### ❌ Anti-Pattern: Ignored Result Values

```csharp
// ❌ Bad: Ignoring Result values
public async Task ProcessUserAsync(CreateUserRequest request)
{
    _userService.CreateUserAsync(request); // Result ignored!
    // User might not have been created, but we don't know
    
    await SendWelcomeEmailAsync(request.Email); // This might fail
}

// ✅ Good: Always handle Results
public async Task<Result> ProcessUserAsync(CreateUserRequest request)
{
    Result<User> userResult = await _userService.CreateUserAsync(request);
    if (userResult.IsFailure) return userResult.ToResult();
    
    Result emailResult = await SendWelcomeEmailAsync(userResult.Value!.Email);
    return emailResult;
}
```

### ❌ Anti-Pattern: Swallowing Failures

```csharp
// ❌ Bad: Converting failures to success
public Result<User> GetUserOrDefault(int id)
{
    Result<User> result = GetUser(id);
    return result.IsSuccess 
        ? result 
        : Result.Success(User.Default); // Lost failure information!
}

// ✅ Good: Explicit default handling with context
public Result<User> GetUserOrDefault(int id, User defaultUser)
{
    Result<User> result = GetUser(id);
    
    // Only use default for NotFound, preserve other failures
    return result.FailureType switch
    {
        ResultFailureType.NotFound => Result.Success(defaultUser),
        _ => result
    };
}
```

### ❌ Anti-Pattern: Mixing Result and Exception Patterns

```csharp
// ❌ Bad: Inconsistent error handling patterns
public class MixedPatternService
{
    public Result<User> GetUser(int id)
    {
        if (id <= 0) return Result.Failure<User>("Invalid ID");
        
        try
        {
            return Result.Success(_repository.GetById(id));
        }
        catch (NotFoundException)
        {
            return Result.NotFound<User>("User not found");
        }
        catch (Exception ex)
        {
            throw; // Inconsistent - sometimes Result, sometimes Exception
        }
    }
}

// ✅ Good: Consistent Result pattern throughout
public class ConsistentPatternService
{
    public Result<User> GetUser(int id)
    {
        if (id <= 0) return Result.Failure<User>("Invalid ID");
        
        try
        {
            User user = _repository.GetById(id);
            return Result.Success(user);
        }
        catch (NotFoundException)
        {
            return Result.NotFound<User>("User not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting user {UserId}", id);
            return Result.ServerError<User>("Unable to retrieve user");
        }
    }
}
```

### ❌ Anti-Pattern: Inefficient Result Chaining

```csharp
// ❌ Bad: Inefficient nested Result handling
public async Task<Result<ProcessedOrder>> ProcessOrderAsync(Order order)
{
    Result<ValidatedOrder> validationResult = await ValidateOrderAsync(order);
    if (validationResult.IsFailure) 
    {
        return Result.Failure<ProcessedOrder>(validationResult.Error); // Manual conversion
    }
    
    Result<EnrichedOrder> enrichmentResult = await EnrichOrderAsync(validationResult.Value!);
    if (enrichmentResult.IsFailure)
    {
        return Result.Failure<ProcessedOrder>(enrichmentResult.Error); // More manual conversion
    }
    
    Result<ProcessedOrder> finalResult = await FinalizeOrderAsync(enrichmentResult.Value!);
    return finalResult;
}

// ✅ Good: Efficient functional composition
public async Task<Result<ProcessedOrder>> ProcessOrderAsync(Order order)
{
    return await ValidateOrderAsync(order)
        .ThenAsync(validOrder => EnrichOrderAsync(validOrder))
        .ThenAsync(enrichedOrder => FinalizeOrderAsync(enrichedOrder));
}
```

### ❌ Anti-Pattern: Poor Error Context

```csharp
// ❌ Bad: Generic, unhelpful error messages
public Result<Account> CreateAccount(CreateAccountRequest request)
{
    return new ValidationBuilder<Account>()
        .RuleFor(x => x.Email, request.Email)
            .NotEmpty("Required") // Too generic
            .EmailAddress("Invalid") // No context
        .RuleFor(x => x.Password, request.Password)
            .MinimumLength(8, "Too short") // Not actionable
        .Build(() => new Account(request.Email, request.Password));
}

// ✅ Good: Clear, actionable error messages
public Result<Account> CreateAccount(CreateAccountRequest request)
{
    return new ValidationBuilder<Account>()
        .RuleFor(x => x.Email, request.Email)
            .NotEmpty("Please enter your email address")
            .EmailAddress("Please enter a valid email address (example: user@domain.com)")
        .RuleFor(x => x.Password, request.Password)
            .MinimumLength(8, "Password must be at least 8 characters long")
        .Build(() => new Account(request.Email, request.Password));
}
```

## Architectural Guidance

### Domain Layer Patterns

```csharp
// ✅ Good: Domain services with Result pattern
public class OrderDomainService
{
    public Result<Order> CreateOrder(Customer customer, IEnumerable<OrderItem> items, ShippingAddress address)
    {
        return new ValidationBuilder<Order>()
            .RuleFor(x => x.Customer, customer)
                .NotNull("Customer is required")
                .Must(c => c.IsActive, "Customer account must be active")
            .RuleFor(x => x.Items, items)
                .NotEmpty("Order must contain at least one item")
                .Must(ValidateItemsInStock, "All items must be in stock")
                .Must(ValidateOrderLimits, "Order exceeds customer limits")
            .RuleFor(x => x.ShippingAddress, address)
                .NotNull("Shipping address is required")
                .Must(ValidateShippingRegion, "We don't ship to this region")
            .Build(() => Order.Create(customer, items, address));
    }
    
    public Result ApplyDiscount(Order order, Discount discount)
    {
        return new ValidationBuilder<Order>()
            .RuleFor(x => x.Discount, discount)
                .Must(d => d.IsValid, "Discount is not valid")
                .Must(d => d.IsApplicableToOrder(order), "Discount cannot be applied to this order")
                .Must(d => !order.HasDiscount, "Order already has a discount applied")
            .Build(() => {
                order.ApplyDiscount(discount);
                return order;
            })
            .ToResult(); // Convert Result<Order> to Result since we're modifying existing order
    }
}
```

### Application Layer Integration

```csharp
// ✅ Good: Application service orchestration
public class OrderApplicationService
{
    private readonly OrderDomainService _domainService;
    private readonly IOrderRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    
    public async Task<Result<OrderCreatedResponse>> CreateOrderAsync(CreateOrderCommand command)
    {
        // Validate command
        Result<CreateOrderCommand> validationResult = ValidateCommand(command);
        if (validationResult.IsFailure) return validationResult.ToResult<OrderCreatedResponse>();
        
        // Get domain entities
        Result<Customer> customerResult = await GetCustomerAsync(command.CustomerId);
        if (customerResult.IsFailure) return customerResult.ToResult<OrderCreatedResponse>();
        
        Result<IEnumerable<OrderItem>> itemsResult = await BuildOrderItemsAsync(command.Items);
        if (itemsResult.IsFailure) return itemsResult.ToResult<OrderCreatedResponse>();
        
        // Create domain object
        Result<Order> orderResult = _domainService.CreateOrder(
            customerResult.Value!, 
            itemsResult.Value!, 
            command.ShippingAddress);
        if (orderResult.IsFailure) return orderResult.ToResult<OrderCreatedResponse>();
        
        // Persist and publish events
        try
        {
            Order savedOrder = await _repository.SaveAsync(orderResult.Value!);
            await _eventPublisher.PublishAsync(new OrderCreatedEvent(savedOrder));
            
            return Result.Success(new OrderCreatedResponse(savedOrder.Id, savedOrder.OrderNumber));
        }
        catch (Exception ex)
        {
            return Result.ServerError<OrderCreatedResponse>($"Failed to save order: {ex.Message}");
        }
    }
}
```

### Infrastructure Layer Patterns

```csharp
// ✅ Good: Repository with Result pattern
public class SqlOrderRepository : IOrderRepository
{
    private readonly DbContext _context;
    private readonly ILogger<SqlOrderRepository> _logger;
    
    public async Task<Result<Order>> GetByIdAsync(int id)
    {
        try
        {
            Order? order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            return order is not null
                ? Result.Success(order)
                : Result.NotFound<Order>($"Order with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return Result.ServerError<Order>("Unable to retrieve order");
        }
    }
    
    public async Task<Result<Order>> SaveAsync(Order order)
    {
        using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            if (order.Id == 0)
            {
                _context.Orders.Add(order);
            }
            else
            {
                _context.Orders.Update(order);
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return Result.Success(order);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return Result.Failure<Order>("Order has been modified by another user");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving order {OrderId}", order.Id);
            return Result.ServerError<Order>("Unable to save order");
        }
    }
}
```

### Dependency Injection Integration

```csharp
// ✅ Good: Service registration patterns
public static class ServiceRegistration
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services)
    {
        // Register domain services
        services.AddScoped<OrderDomainService>();
        services.AddScoped<PricingDomainService>();
        
        // Register application services  
        services.AddScoped<IOrderApplicationService, OrderApplicationService>();
        services.AddScoped<IOrderQueryService, OrderQueryService>();
        
        // Register repositories
        services.AddScoped<IOrderRepository, SqlOrderRepository>();
        services.AddScoped<ICustomerRepository, SqlCustomerRepository>();
        
        // Register validation services
        services.AddScoped<IValidator<CreateOrderCommand>, CreateOrderCommandValidator>();
        services.AddTransient<ValidationContext>();
        
        return services;
    }
}

// Service implementation using DI
public class OrderApplicationService : IOrderApplicationService
{
    private readonly OrderDomainService _domainService;
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IValidator<CreateOrderCommand> _validator;
    
    public OrderApplicationService(
        OrderDomainService domainService,
        IOrderRepository orderRepository, 
        ICustomerRepository customerRepository,
        IValidator<CreateOrderCommand> validator)
    {
        _domainService = domainService;
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _validator = validator;
    }
    
    public async Task<Result<OrderResponse>> CreateOrderAsync(CreateOrderCommand command)
    {
        // Use injected validator
        Result<CreateOrderCommand> validationResult = await _validator.ValidateAsync(command);
        if (validationResult.IsFailure) return validationResult.ToResult<OrderResponse>();
        
        // Implementation continues...
    }
}
```

## Decision Trees

### When to Use Result vs Exception

```
Is this a programming error (null reference, index out of range)?
├─ YES → Use Exception
└─ NO → Is this an expected business scenario?
    ├─ YES → Is this recoverable by the caller?
    │   ├─ YES → Use Result
    │   └─ NO → Is this critical system failure?
    │       ├─ YES → Use Exception
    │       └─ NO → Use Result with appropriate failure type
    └─ NO → Is this a system-level failure?
        ├─ YES → Use Exception (infrastructure issues)
        └─ NO → Use Result (validation, business rules)
```

### Result Type Selection

```
What type of operation is this?
├─ Returns a value → Use Result<T>
├─ Performs action only → Use Result
├─ Multiple possible failures → Use Result with specific ResultFailureType
├─ Validation-heavy → Use ValidationBuilder<T>
└─ HTTP integration → Use HttpResponseMessage extensions
```

### Error Handling Strategy

```
What's the failure context?
├─ User input validation → ResultFailureType.Validation
├─ Resource not found → ResultFailureType.NotFound  
├─ Permission denied → ResultFailureType.Security
├─ Operation cancelled → ResultFailureType.OperationCanceled
├─ System unavailable → ResultFailureType.ServerError
└─ General business rule → ResultFailureType.Error
```

### Async Pattern Selection

```
What's the async scenario?
├─ Single async operation → Use Task<Result<T>>
├─ Multiple sequential operations → Use ThenAsync chaining
├─ Multiple parallel operations → Use Task.WhenAll + Result.Combine
├─ Optional operations → Use graceful degradation pattern
└─ Stream processing → Use async enumerable with Result<T>
```

---

## Summary

This guide covers the essential patterns and practices for using FlowRight effectively:

- **Result Pattern Fundamentals**: When and how to use Results vs exceptions
- **Validation Composition**: Building complex validation logic with clear error messages
- **Error Handling**: Designing user-friendly errors and proper categorization
- **Async Integration**: Clean patterns for async/await with Results
- **HTTP Integration**: Controller patterns and HTTP client integration
- **Testing**: Comprehensive testing strategies for Result-based code
- **Performance**: Optimization techniques for production usage
- **Anti-Patterns**: Common mistakes and how to avoid them
- **Architecture**: Integration patterns across application layers

By following these practices, you'll build maintainable, resilient applications that handle errors gracefully and provide excellent developer experience.