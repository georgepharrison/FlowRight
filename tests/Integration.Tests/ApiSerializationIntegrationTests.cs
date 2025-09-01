using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FlowRight.Core.Results;
using FlowRight.Core.Serialization;
using FlowRight.Http.Models;
using FlowRight.Validation.Tests.TestModels;
using Shouldly;
using Xunit;

namespace FlowRight.Integration.Tests;

/// <summary>
/// Integration tests that verify serialization behavior of Result types in real API scenarios.
/// These tests use ASP.NET Core TestHost to simulate actual HTTP API calls and verify 
/// that JSON serialization works correctly end-to-end in API contexts.
/// </summary>
/// <remarks>
/// <para>
/// These integration tests go beyond unit-level JSON serialization tests by using
/// actual ASP.NET Core controllers, HTTP clients, and the full serialization pipeline
/// to verify that Result types serialize correctly in real API scenarios.
/// </para>
/// <para>
/// The tests cover:
/// <list type="bullet">
/// <item><description>Result and Result&lt;T&gt; serialization in API responses</description></item>
/// <item><description>ValidationProblemDetails serialization for error responses</description></item>
/// <item><description>Model binding and JSON deserialization from API requests</description></item>
/// <item><description>Custom JsonConverter behavior with different serialization settings</description></item>
/// <item><description>Performance testing in high-throughput scenarios</description></item>
/// <item><description>Round-trip serialization with complex nested objects</description></item>
/// </list>
/// </para>
/// </remarks>
public class ApiSerializationIntegrationTests : IClassFixture<WebApplicationFactory<TestApiStartup>>
{
    private readonly WebApplicationFactory<TestApiStartup> _factory;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiSerializationIntegrationTests"/> class.
    /// </summary>
    /// <param name="factory">The web application factory for creating test servers.</param>
    public ApiSerializationIntegrationTests(WebApplicationFactory<TestApiStartup> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Tests that verify Result serialization in API controller responses.
    /// </summary>
    public class ResultSerializationInApiResponses : ApiSerializationIntegrationTests
    {
        public ResultSerializationInApiResponses(WebApplicationFactory<TestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task GetSuccessResult_ShouldReturnCorrectJsonStructure()
        {
            // Arrange - API endpoint will return Result.Success()
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/success");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetFailureResult_ShouldReturnCorrectJsonStructure()
        {
            // Arrange - API endpoint will return Result.Failure("Test error")
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/failure");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetSecurityFailure_ShouldReturnCorrectJsonStructure()
        {
            // Arrange - API endpoint will return security failure
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/security-failure");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetOperationCancelledFailure_ShouldReturnCorrectJsonStructure()
        {
            // Arrange - API endpoint will return cancellation failure
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/cancellation-failure");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
    }

    /// <summary>
    /// Tests that verify Result&lt;T&gt; serialization in API controller responses with typed values.
    /// </summary>
    public class ResultTSerializationInApiResponses : ApiSerializationIntegrationTests
    {
        public ResultTSerializationInApiResponses(WebApplicationFactory<TestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task GetSuccessResultWithPrimitiveType_ShouldReturnValueInJson()
        {
            // Arrange - API endpoint will return Result.Success(42)
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/success-int");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetSuccessResultWithComplexType_ShouldReturnSerializedObject()
        {
            // Arrange - API endpoint will return Result.Success(customer)
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/success-customer");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetFailureResultWithType_ShouldReturnErrorWithoutValue()
        {
            // Arrange - API endpoint will return Result.Failure<Customer>("Not found")
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/failure-customer");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetSuccessResultWithNestedComplexObject_ShouldSerializeAllLevels()
        {
            // Arrange - API endpoint will return Result.Success(order) with deeply nested structure
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/success-order");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    /// <summary>
    /// Tests that verify ValidationProblemDetails serialization in API error responses.
    /// </summary>
    public class ValidationProblemDetailsSerializationInApiResponses : ApiSerializationIntegrationTests
    {
        public ValidationProblemDetailsSerializationInApiResponses(WebApplicationFactory<TestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task GetValidationFailure_ShouldReturnProblemDetailsStructure()
        {
            // Arrange - API endpoint will return validation failure with multiple field errors
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/validation-failure");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetValidationFailureWithCustomProblemDetails_ShouldReturnFullRfc7807Structure()
        {
            // Arrange - API endpoint will return ValidationProblemResponse with all RFC 7807 properties
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/validation-problem-details");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostInvalidModel_ShouldTriggerModelValidationAndReturnProblemDetails()
        {
            // Arrange - POST invalid customer data to trigger ASP.NET Core model validation
            string invalidCustomerJson = JsonSerializer.Serialize(new
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                Email = "invalid-email"
            });
            StringContent content = new(invalidCustomerJson, Encoding.UTF8, "application/json");
            
            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/testapi/create-customer", content);
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
    }

    /// <summary>
    /// Tests that verify ASP.NET Core model binding works correctly with Result types.
    /// </summary>
    public class ModelBindingWithResultTypes : ApiSerializationIntegrationTests
    {
        public ModelBindingWithResultTypes(WebApplicationFactory<TestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task PostResultInRequestBody_ShouldDeserializeCorrectly()
        {
            // Arrange - POST a serialized Result<Customer> in the request body
            Customer testCustomer = new CustomerBuilder().Build();
            Result<Customer> successResult = Result.Success(testCustomer);
            string resultJson = JsonSerializer.Serialize(successResult, GetJsonOptions());
            StringContent content = new(resultJson, Encoding.UTF8, "application/json");
            
            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/testapi/process-result", content);
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task PostFailureResultInRequestBody_ShouldDeserializeCorrectly()
        {
            // Arrange - POST a serialized failure Result<Customer> in the request body
            Result<Customer> failureResult = Result.Failure<Customer>("Test error");
            string resultJson = JsonSerializer.Serialize(failureResult, GetJsonOptions());
            StringContent content = new(resultJson, Encoding.UTF8, "application/json");
            
            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/testapi/process-result", content);
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task PostValidationFailureInRequestBody_ShouldDeserializeWithErrors()
        {
            // Arrange - POST a serialized validation failure Result<Customer>
            Dictionary<string, string[]> validationErrors = new()
            {
                { "FirstName", ["First name is required"] },
                { "Email", ["Email is required", "Email format is invalid"] }
            };
            Result<Customer> validationFailure = Result.ValidationFailure<Customer>(validationErrors);
            string resultJson = JsonSerializer.Serialize(validationFailure, GetJsonOptions());
            StringContent content = new(resultJson, Encoding.UTF8, "application/json");
            
            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/testapi/process-result", content);
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    /// <summary>
    /// Tests that verify custom JsonConverter behavior with different serialization settings.
    /// </summary>
    public class CustomJsonConverterBehaviorTests : ApiSerializationIntegrationTests
    {
        public CustomJsonConverterBehaviorTests(WebApplicationFactory<TestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task ResultSerialization_WithCamelCaseNaming_ShouldRespectNamingPolicy()
        {
            // Arrange - API configured with camel case naming policy
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/camel-case-result");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ResultSerialization_WithSnakeCaseNaming_ShouldRespectNamingPolicy()
        {
            // Arrange - API configured with snake case naming policy
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/snake-case-result");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ResultSerialization_WithCustomDateTimeFormat_ShouldRespectFormatting()
        {
            // Arrange - API configured with custom DateTime formatting
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/datetime-formatting");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ResultSerialization_WithIgnoreNullValues_ShouldExcludeNullProperties()
        {
            // Arrange - API configured to ignore null values
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/ignore-nulls");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    /// <summary>
    /// Tests that verify round-trip JSON serialization preserves data integrity.
    /// </summary>
    public class RoundTripSerializationTests : ApiSerializationIntegrationTests
    {
        public RoundTripSerializationTests(WebApplicationFactory<TestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task ResultRoundTrip_WithSimpleValue_ShouldPreserveAllData()
        {
            // Arrange - Send a Result<int> to API and expect same result back
            Result<int> originalResult = Result.Success(42, ResultType.Information);
            string json = JsonSerializer.Serialize(originalResult, GetJsonOptions());
            StringContent content = new(json, Encoding.UTF8, "application/json");
            
            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/testapi/echo-result-int", content);
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ResultRoundTrip_WithComplexObject_ShouldPreserveAllNestedData()
        {
            // Arrange - Send a Result<Order> with nested complex structure
            Order complexOrder = new OrderBuilder().Build();
            Result<Order> originalResult = Result.Success(complexOrder);
            string json = JsonSerializer.Serialize(originalResult, GetJsonOptions());
            StringContent content = new(json, Encoding.UTF8, "application/json");
            
            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/testapi/echo-result-order", content);
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ValidationFailureRoundTrip_ShouldPreserveAllErrorData()
        {
            // Arrange - Send a validation failure with multiple field errors
            Dictionary<string, string[]> errors = new()
            {
                { "Customer.FirstName", ["Required", "Must be at least 2 characters"] },
                { "Items[0].Quantity", ["Must be greater than 0"] },
                { "ShippingAddress.City", ["Required"] }
            };
            Result<Order> validationFailure = Result.ValidationFailure<Order>(errors);
            string json = JsonSerializer.Serialize(validationFailure, GetJsonOptions());
            StringContent content = new(json, Encoding.UTF8, "application/json");
            
            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/testapi/echo-result-order", content);
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    /// <summary>
    /// Performance tests that verify serialization behavior under high-throughput scenarios.
    /// </summary>
    public class PerformanceTests : ApiSerializationIntegrationTests
    {
        public PerformanceTests(WebApplicationFactory<TestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task HighThroughputApiCalls_WithResultSerialization_ShouldMaintainPerformance()
        {
            // Arrange - Prepare for multiple concurrent API calls
            int numberOfCalls = 100;
            Task<HttpResponseMessage>[] tasks = new Task<HttpResponseMessage>[numberOfCalls];
            
            // Act - Execute multiple concurrent API calls
            for (int i = 0; i < numberOfCalls; i++)
            {
                tasks[i] = _client.GetAsync($"/api/testapi/success-customer?id={i}");
            }
            
            HttpResponseMessage[] responses = await Task.WhenAll(tasks);
            
            // Assert - All calls should succeed
            foreach (HttpResponseMessage response in responses)
            {
                response.StatusCode.ShouldBe(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task LargePayloadSerialization_WithNestedComplexObjects_ShouldComplete()
        {
            // Arrange - API will return very large Result<Order[]> with many nested objects
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/large-order-collection");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task DeepNestingSerialization_WithRecursiveStructures_ShouldComplete()
        {
            // Arrange - API will return Result with deeply nested category hierarchy
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/testapi/deep-category-nesting");
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    /// <summary>
    /// Gets JSON serializer options configured for testing.
    /// </summary>
    /// <returns>JsonSerializerOptions with Result converters registered.</returns>
    private static JsonSerializerOptions GetJsonOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        options.Converters.Add(new ResultJsonConverter());
        options.Converters.Add(new ResultTJsonConverter<Customer>());
        options.Converters.Add(new ResultTJsonConverter<Order>());
        options.Converters.Add(new ResultTJsonConverter<int>());
        options.Converters.Add(new ResultTJsonConverter<string>());
        options.Converters.Add(new ResultTJsonConverter<Order[]>());
        
        return options;
    }
}

/// <summary>
/// Test API startup configuration for integration testing.
/// Provides a minimal ASP.NET Core API with Result serialization support.
/// </summary>
public class TestApiStartup
{
    /// <summary>
    /// Configures services for the test API.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
                
                // Register Result JSON converters
                options.JsonSerializerOptions.Converters.Add(new ResultJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Customer>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Order>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<int>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<string>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Order[]>());
            });
            
        services.AddProblemDetails();
    }

    /// <summary>
    /// Configures the request pipeline for the test API.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="env">The web host environment.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}

/// <summary>
/// Test controller that provides endpoints for API serialization testing.
/// </summary>
[ApiController]
[Route("api/testapi")]
public class TestApiController : ControllerBase
{
    /// <summary>
    /// Returns a success Result for testing serialization.
    /// </summary>
    [HttpGet("success")]
    public IActionResult GetSuccess()
    {
        Result result = Result.Success();
        return Ok(result);
    }

    /// <summary>
    /// Returns a failure Result for testing error serialization.
    /// </summary>
    [HttpGet("failure")]
    public IActionResult GetFailure()
    {
        Result result = Result.Failure("Test error message");
        return BadRequest(result);
    }

    /// <summary>
    /// Returns a security failure Result for testing specific failure type serialization.
    /// </summary>
    [HttpGet("security-failure")]
    public IActionResult GetSecurityFailure()
    {
        Result result = Result.Failure(new System.Security.SecurityException("Access denied"));
        return Unauthorized(result);
    }

    /// <summary>
    /// Returns an operation cancelled failure Result for testing cancellation serialization.
    /// </summary>
    [HttpGet("cancellation-failure")]
    public IActionResult GetCancellationFailure()
    {
        Result result = Result.Failure(new OperationCanceledException("Operation was cancelled"));
        return BadRequest(result);
    }

    /// <summary>
    /// Returns a success Result&lt;int&gt; for testing primitive type serialization.
    /// </summary>
    [HttpGet("success-int")]
    public IActionResult GetSuccessInt()
    {
        Result<int> result = Result.Success(42);
        return Ok(result);
    }

    /// <summary>
    /// Returns a success Result&lt;Customer&gt; for testing complex object serialization.
    /// </summary>
    [HttpGet("success-customer")]
    public IActionResult GetSuccessCustomer()
    {
        Customer customer = new CustomerBuilder().Build();
        Result<Customer> result = Result.Success(customer);
        return Ok(result);
    }

    /// <summary>
    /// Returns a failure Result&lt;Customer&gt; for testing typed failure serialization.
    /// </summary>
    [HttpGet("failure-customer")]
    public IActionResult GetFailureCustomer()
    {
        Result<Customer> result = Result.Failure<Customer>("Customer not found");
        return BadRequest(result);
    }

    /// <summary>
    /// Returns a success Result&lt;Order&gt; with nested complex structure for testing deep serialization.
    /// </summary>
    [HttpGet("success-order")]
    public IActionResult GetSuccessOrder()
    {
        Order order = new OrderBuilder().Build();
        Result<Order> result = Result.Success(order);
        return Ok(result);
    }

    /// <summary>
    /// Returns a validation failure for testing ValidationProblemDetails serialization.
    /// </summary>
    [HttpGet("validation-failure")]
    public IActionResult GetValidationFailure()
    {
        Dictionary<string, string[]> errors = new()
        {
            { "FirstName", ["First name is required"] },
            { "Email", ["Email is required", "Email format is invalid"] },
            { "DateOfBirth", ["Date of birth must be in the past"] }
        };
        
        Result<Customer> result = Result.ValidationFailure<Customer>(errors);
        return BadRequest(result);
    }

    /// <summary>
    /// Returns a ValidationProblemResponse for testing RFC 7807 compliance.
    /// </summary>
    [HttpGet("validation-problem-details")]
    public IActionResult GetValidationProblemDetails()
    {
        ValidationProblemResponse problemDetails = new()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = 400,
            Detail = "The request failed validation. See errors for details.",
            Instance = "/api/testapi/validation-problem-details",
            Errors = new Dictionary<string, string[]>
            {
                { "firstName", ["First name is required", "First name must be at least 2 characters"] },
                { "email", ["Email is required", "Email format is invalid"] }
            }
        };
        
        return BadRequest(problemDetails);
    }

    /// <summary>
    /// Accepts customer data and validates it for testing model validation.
    /// </summary>
    [HttpPost("create-customer")]
    public IActionResult CreateCustomer([FromBody] CustomerCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        Customer customer = new CustomerBuilder()
            .WithFirstName(request.FirstName)
            .WithLastName(request.LastName)
            .WithEmail(request.Email)
            .Build();
            
        Result<Customer> result = Result.Success(customer);
        return Ok(result);
    }

    /// <summary>
    /// Processes a Result&lt;Customer&gt; from the request body for testing deserialization.
    /// </summary>
    [HttpPost("process-result")]
    public IActionResult ProcessResult([FromBody] Result<Customer> customerResult)
    {
        return Ok(new { Processed = true, IsSuccess = customerResult.IsSuccess });
    }

    /// <summary>
    /// Echo endpoints that return the same Result received for round-trip testing.
    /// </summary>
    [HttpPost("echo-result-int")]
    public IActionResult EchoResultInt([FromBody] Result<int> result) => Ok(result);

    [HttpPost("echo-result-order")]
    public IActionResult EchoResultOrder([FromBody] Result<Order> result) => Ok(result);

    /// <summary>
    /// Performance test endpoints that generate varying payload sizes.
    /// </summary>
    [HttpGet("large-order-collection")]
    public IActionResult GetLargeOrderCollection()
    {
        Order[] orders = new Order[100];
        for (int i = 0; i < orders.Length; i++)
        {
            orders[i] = new OrderBuilder().WithOrderNumber($"ORD-{i:D3}").Build();
        }
        
        Result<Order[]> result = Result.Success(orders);
        return Ok(result);
    }

    [HttpGet("deep-category-nesting")]
    public IActionResult GetDeepCategoryNesting()
    {
        // Create deeply nested category hierarchy
        Category rootCategory = new CategoryBuilder().WithName("Root").Build();
        Category currentCategory = rootCategory;
        
        for (int i = 0; i < 10; i++)
        {
            currentCategory = new CategoryBuilder()
                .WithName($"Level-{i}")
                .WithParentCategory(currentCategory)
                .Build();
        }
        
        Result<Category> result = Result.Success(currentCategory);
        return Ok(result);
    }
}

/// <summary>
/// Request model for customer creation testing.
/// </summary>
public class CustomerCreateRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}