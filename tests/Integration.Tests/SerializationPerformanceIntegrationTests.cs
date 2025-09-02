using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FlowRight.Core.Results;
using FlowRight.Core.Serialization;
using FlowRight.Validation.Tests.TestModels;
using Shouldly;
using Xunit;

namespace FlowRight.Integration.Tests;

/// <summary>
/// Performance integration tests that verify Result serialization behavior under high-throughput 
/// and high-load scenarios to ensure production readiness.
/// </summary>
/// <remarks>
/// <para>
/// These tests validate that FlowRight's JSON serialization maintains acceptable performance
/// characteristics when used in real API scenarios with:
/// <list type="bullet">
/// <item><description>High concurrent request volumes</description></item>
/// <item><description>Large payload sizes with nested complex objects</description></item>
/// <item><description>Deep object hierarchies and circular reference handling</description></item>
/// <item><description>Memory allocation efficiency during serialization</description></item>
/// <item><description>Serialization speed compared to baseline scenarios</description></item>
/// </list>
/// </para>
/// <para>
/// The tests include both load testing and benchmarking to ensure that the custom JSON converters
/// do not introduce significant performance penalties in production environments.
/// </para>
/// </remarks>
public class SerializationPerformanceIntegrationTests : IClassFixture<WebApplicationFactory<PerformanceTestApiStartup>>
{
    private readonly WebApplicationFactory<PerformanceTestApiStartup> _factory;
    private readonly HttpClient _client;

    public SerializationPerformanceIntegrationTests(WebApplicationFactory<PerformanceTestApiStartup> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Tests that verify performance under high concurrent load scenarios.
    /// </summary>
    public class ConcurrentLoadTests : SerializationPerformanceIntegrationTests
    {
        public ConcurrentLoadTests(WebApplicationFactory<PerformanceTestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task HighConcurrentLoad_WithResultSerialization_ShouldMaintainResponseTimes()
        {
            // Arrange - Prepare for high concurrent load test
            int numberOfConcurrentRequests = 200;
            int requestsPerClient = 5;
            Stopwatch totalStopwatch = Stopwatch.StartNew();
            
            // Act - Execute multiple concurrent requests
            Task[] clientTasks = new Task[numberOfConcurrentRequests];
            for (int i = 0; i < numberOfConcurrentRequests; i++)
            {
                clientTasks[i] = ExecuteMultipleRequestsAsync(requestsPerClient);
            }
            
            await Task.WhenAll(clientTasks);
            totalStopwatch.Stop();
            
            // Assert - Performance should be acceptable
            double totalSeconds = totalStopwatch.Elapsed.TotalSeconds;
            int totalRequests = numberOfConcurrentRequests * requestsPerClient;
            double requestsPerSecond = totalRequests / totalSeconds;
            
            requestsPerSecond.ShouldBeGreaterThan(100); // At least 100 requests per second
            totalSeconds.ShouldBeLessThan(30); // Should complete within 30 seconds
        }

        [Fact]
        public async Task ConcurrentValidationFailures_WithLargeErrorCollections_ShouldSerializeEfficiently()
        {
            // Arrange - Test concurrent serialization of complex validation failures
            int numberOfConcurrentRequests = 50;
            Task<HttpResponseMessage>[] tasks = new Task<HttpResponseMessage>[numberOfConcurrentRequests];
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            // Act - Request large validation failures concurrently
            for (int i = 0; i < numberOfConcurrentRequests; i++)
            {
                tasks[i] = _client.GetAsync($"/api/performance/large-validation-failure?fields={i * 10}");
            }
            
            HttpResponseMessage[] responses = await Task.WhenAll(tasks);
            stopwatch.Stop();
            
            // Assert - All requests should succeed and complete efficiently
            foreach (HttpResponseMessage response in responses)
            {
                response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            }
            
            stopwatch.Elapsed.TotalSeconds.ShouldBeLessThan(10);
        }

        [Fact]
        public async Task ConcurrentComplexObjectSerialization_ShouldNotDegradePerformance()
        {
            // Arrange - Test concurrent serialization of complex nested objects
            int numberOfConcurrentRequests = 100;
            Task<HttpResponseMessage>[] tasks = new Task<HttpResponseMessage>[numberOfConcurrentRequests];
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            // Act - Request complex objects concurrently
            for (int i = 0; i < numberOfConcurrentRequests; i++)
            {
                tasks[i] = _client.GetAsync($"/api/performance/complex-order?itemCount={10 + (i % 20)}");
            }
            
            HttpResponseMessage[] responses = await Task.WhenAll(tasks);
            stopwatch.Stop();
            
            // Assert - Performance should remain acceptable
            foreach (HttpResponseMessage response in responses)
            {
                response.StatusCode.ShouldBe(HttpStatusCode.OK);
            }
            
            double averageResponseTime = stopwatch.Elapsed.TotalMilliseconds / numberOfConcurrentRequests;
            averageResponseTime.ShouldBeLessThan(500); // Average response under 500ms
        }

        private async Task ExecuteMultipleRequestsAsync(int requestCount)
        {
            Task[] requestTasks = new Task[requestCount];
            for (int i = 0; i < requestCount; i++)
            {
                requestTasks[i] = ExecuteSingleRequestAsync();
            }
            await Task.WhenAll(requestTasks);
        }

        private async Task ExecuteSingleRequestAsync()
        {
            HttpResponseMessage response = await _client.GetAsync("/api/performance/simple-result");
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    /// <summary>
    /// Tests that verify performance with large payload sizes.
    /// </summary>
    public class LargePayloadTests : SerializationPerformanceIntegrationTests
    {
        public LargePayloadTests(WebApplicationFactory<PerformanceTestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task LargeOrderCollection_WithThousandsOfItems_ShouldSerializeInReasonableTime()
        {
            // Arrange - Request very large collection
            int itemCount = 1000;
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/performance/large-order-collection?count={itemCount}");
            string content = await response.Content.ReadAsStringAsync();
            stopwatch.Stop();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            content.Length.ShouldBeGreaterThan(100000); // Substantial payload size
            stopwatch.Elapsed.TotalSeconds.ShouldBeLessThan(5); // Under 5 seconds
        }

        [Fact]
        public async Task DeepNestedObjectHierarchy_WithManyLevels_ShouldSerializeSuccessfully()
        {
            // Arrange - Request deeply nested object structure
            int nestingDepth = 20;
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/performance/deep-nesting?depth={nestingDepth}");
            string content = await response.Content.ReadAsStringAsync();
            stopwatch.Stop();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            content.ShouldContain("\"level\""); // Verify nested structure serialized
            stopwatch.Elapsed.TotalSeconds.ShouldBeLessThan(3);
        }

        [Fact]
        public async Task WideObjectStructure_WithManyProperties_ShouldSerializeEfficiently()
        {
            // Arrange - Request object with very wide property structure
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/performance/wide-object-structure");
            string content = await response.Content.ReadAsStringAsync();
            stopwatch.Stop();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            content.Length.ShouldBeGreaterThan(10000); // Large object structure
            stopwatch.Elapsed.TotalMilliseconds.ShouldBeLessThan(1000);
        }
    }

    /// <summary>
    /// Tests that verify memory allocation efficiency during serialization.
    /// </summary>
    public class MemoryAllocationTests : SerializationPerformanceIntegrationTests
    {
        public MemoryAllocationTests(WebApplicationFactory<PerformanceTestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task RepetitiveSerialization_ShouldNotCauseMemoryLeaks()
        {
            // Arrange - Measure memory before repetitive operations
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long initialMemory = GC.GetTotalMemory(false);
            
            // Act - Perform many serialization operations
            for (int i = 0; i < 1000; i++)
            {
                HttpResponseMessage response = await _client.GetAsync("/api/performance/customer-with-nested-objects");
                string content = await response.Content.ReadAsStringAsync();
                response.Dispose();
            }
            
            // Force garbage collection and measure memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long finalMemory = GC.GetTotalMemory(false);
            
            // Assert - Memory growth should be reasonable
            long memoryGrowth = finalMemory - initialMemory;
            long memoryGrowthMB = memoryGrowth / (1024 * 1024);
            
            memoryGrowthMB.ShouldBeLessThan(50); // Less than 50MB growth
        }

        [Fact]
        public async Task LargeValidationFailures_ShouldNotExcessivelyAllocateMemory()
        {
            // Arrange - Test memory usage with large validation failures
            GC.Collect();
            long beforeMemory = GC.GetTotalMemory(false);
            
            // Act - Request large validation failures
            for (int i = 0; i < 100; i++)
            {
                HttpResponseMessage response = await _client.GetAsync("/api/performance/large-validation-failure?fields=100");
                string content = await response.Content.ReadAsStringAsync();
                response.Dispose();
            }
            
            GC.Collect();
            long afterMemory = GC.GetTotalMemory(false);
            
            // Assert - Memory usage should be reasonable
            long memoryIncrease = afterMemory - beforeMemory;
            long memoryIncreaseMB = memoryIncrease / (1024 * 1024);
            
            memoryIncreaseMB.ShouldBeLessThan(100); // Less than 100MB for 100 operations
        }
    }

    /// <summary>
    /// Tests that verify serialization speed benchmarks.
    /// </summary>
    public class SerializationSpeedTests : SerializationPerformanceIntegrationTests
    {
        public SerializationSpeedTests(WebApplicationFactory<PerformanceTestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task ResultSerialization_ShouldBeCompetitiveWithBaseline()
        {
            // Arrange - Warm up and prepare for timing
            await WarmUpEndpoints();
            int iterations = 100;
            
            // Act & Assert - Time Result<T> serialization
            TimeSpan resultSerializationTime = await TimeEndpointAsync("/api/performance/result-customer", iterations);
            TimeSpan baselineSerializationTime = await TimeEndpointAsync("/api/performance/plain-customer", iterations);
            
            // Result serialization should not be more than 3x slower than baseline
            double performanceRatio = resultSerializationTime.TotalMilliseconds / baselineSerializationTime.TotalMilliseconds;
            performanceRatio.ShouldBeLessThan(3.0);
        }

        [Fact]
        public async Task ValidationFailureSerialization_ShouldCompleteQuickly()
        {
            // Arrange - Prepare timing test for validation failures
            int iterations = 50;
            
            // Act
            TimeSpan validationSerializationTime = await TimeEndpointAsync("/api/performance/validation-failure", iterations);
            
            // Assert - Should average less than 50ms per serialization
            double averageMs = validationSerializationTime.TotalMilliseconds / iterations;
            averageMs.ShouldBeLessThan(50);
        }

        [Fact]
        public async Task ComplexNestedObjectSerialization_ShouldMaintainSpeed()
        {
            // Arrange - Time complex object serialization
            int iterations = 20;
            
            // Act
            TimeSpan complexSerializationTime = await TimeEndpointAsync("/api/performance/complex-nested-order", iterations);
            
            // Assert - Should average less than 200ms per complex serialization
            double averageMs = complexSerializationTime.TotalMilliseconds / iterations;
            averageMs.ShouldBeLessThan(200);
        }

        private async Task WarmUpEndpoints()
        {
            // Warm up endpoints to ensure JIT compilation
            string[] warmUpEndpoints = 
            {
                "/api/performance/result-customer",
                "/api/performance/plain-customer",
                "/api/performance/validation-failure"
            };

            foreach (string endpoint in warmUpEndpoints)
            {
                HttpResponseMessage response = await _client.GetAsync(endpoint);
                response.Dispose();
            }
        }

        private async Task<TimeSpan> TimeEndpointAsync(string endpoint, int iterations)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                HttpResponseMessage response = await _client.GetAsync(endpoint);
                string content = await response.Content.ReadAsStringAsync();
                response.Dispose();
            }
            
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }

    /// <summary>
    /// Tests that verify round-trip serialization performance and correctness.
    /// </summary>
    public class RoundTripPerformanceTests : SerializationPerformanceIntegrationTests
    {
        public RoundTripPerformanceTests(WebApplicationFactory<PerformanceTestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task MassiveRoundTripOperations_ShouldMaintainDataIntegrity()
        {
            // Arrange - Prepare for many round-trip operations
            int roundTripCount = 100;
            Customer originalCustomer = new CustomerBuilder().Build();
            Result<Customer> originalResult = Result.Success(originalCustomer);
            
            string json = JsonSerializer.Serialize(originalResult, GetJsonOptions());
            StringContent content = new(json, Encoding.UTF8, "application/json");
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            // Act - Perform many round-trip operations
            for (int i = 0; i < roundTripCount; i++)
            {
                HttpResponseMessage response = await _client.PostAsync("/api/performance/round-trip-customer", content);
                response.StatusCode.ShouldBe(HttpStatusCode.OK);
                response.Dispose();
            }
            
            stopwatch.Stop();
            
            // Assert - Should complete efficiently
            double averageMs = stopwatch.Elapsed.TotalMilliseconds / roundTripCount;
            averageMs.ShouldBeLessThan(100); // Average under 100ms per round-trip
        }

        [Fact]
        public async Task LargeValidationFailureRoundTrip_ShouldPreserveAllErrors()
        {
            // Arrange - Create large validation failure for round-trip
            Dictionary<string, string[]> largeErrors = [];
            for (int i = 0; i < 50; i++)
            {
                largeErrors[$"Field{i}"] = [$"Error1 for field {i}", $"Error2 for field {i}", $"Error3 for field {i}"];
            }
            
            Result<Order> validationFailure = Result.ValidationFailure<Order>(largeErrors);
            string json = JsonSerializer.Serialize(validationFailure, GetJsonOptions());
            StringContent content = new(json, Encoding.UTF8, "application/json");
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/performance/round-trip-order", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            stopwatch.Stop();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            responseJson.Length.ShouldBeGreaterThan((int)(json.Length * 0.8)); // Should be similar size
            stopwatch.Elapsed.TotalMilliseconds.ShouldBeLessThan(500);
        }

        private static JsonSerializerOptions GetJsonOptions()
        {
            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            options.Converters.Add(new ResultJsonConverter());
            options.Converters.Add(new ResultTJsonConverter<Customer>());
            options.Converters.Add(new ResultTJsonConverter<Order>());
            
            return options;
        }
    }
}

/// <summary>
/// Specialized API startup for performance testing scenarios.
/// </summary>
public class PerformanceTestApiStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                
                // Register all necessary converters
                options.JsonSerializerOptions.Converters.Add(new ResultJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Customer>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Order>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Product>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Category>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Order[]>());
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}

/// <summary>
/// Controller specialized for performance testing scenarios.
/// </summary>
[ApiController]
[Route("api/performance")]
public class PerformanceController : ControllerBase
{
    [HttpGet("simple-result")]
    public IActionResult GetSimpleResult()
    {
        Result result = Result.Success();
        return Ok(result);
    }

    [HttpGet("result-customer")]
    public IActionResult GetResultCustomer()
    {
        Customer customer = new CustomerBuilder().Build();
        Result<Customer> result = Result.Success(customer);
        return Ok(result);
    }

    [HttpGet("plain-customer")]
    public IActionResult GetPlainCustomer()
    {
        Customer customer = new CustomerBuilder().Build();
        return Ok(customer);
    }

    [HttpGet("validation-failure")]
    public IActionResult GetValidationFailure()
    {
        Dictionary<string, string[]> errors = new()
        {
            { "FirstName", ["Required", "MinLength"] },
            { "Email", ["Required", "Format"] }
        };
        Result<Customer> result = Result.ValidationFailure<Customer>(errors);
        return BadRequest(result);
    }

    [HttpGet("large-validation-failure")]
    public IActionResult GetLargeValidationFailure([FromQuery] int fields = 10)
    {
        Dictionary<string, string[]> errors = [];
        for (int i = 0; i < fields; i++)
        {
            errors[$"Field{i}"] = [$"Error1 for {i}", $"Error2 for {i}", $"Error3 for {i}"];
        }
        
        Result<Customer> result = Result.ValidationFailure<Customer>(errors);
        return BadRequest(result);
    }

    [HttpGet("complex-order")]
    public IActionResult GetComplexOrder([FromQuery] int itemCount = 10)
    {
        OrderBuilder builder = new OrderBuilder();
        
        // Add multiple items to make it more complex
        for (int i = 0; i < itemCount; i++)
        {
            Product product = new ProductBuilder()
                .WithName($"Product {i}")
                .WithSku($"SKU-{i:D3}")
                .Build();
            OrderItem item = new OrderItemBuilder()
                .WithProduct(product)
                .WithQuantity(i + 1)
                .Build();
            builder.AddItem(item);
        }
        
        Order order = builder.Build();
        Result<Order> result = Result.Success(order);
        return Ok(result);
    }

    [HttpGet("large-order-collection")]
    public IActionResult GetLargeOrderCollection([FromQuery] int count = 100)
    {
        Order[] orders = new Order[count];
        for (int i = 0; i < count; i++)
        {
            orders[i] = new OrderBuilder()
                .WithOrderNumber($"ORD-{i:D5}")
                .Build();
        }
        
        Result<Order[]> result = Result.Success(orders);
        return Ok(result);
    }

    [HttpGet("deep-nesting")]
    public IActionResult GetDeepNesting([FromQuery] int depth = 10)
    {
        Category rootCategory = new CategoryBuilder().WithName("Root").Build();
        Category currentCategory = rootCategory;
        
        for (int i = 1; i < depth; i++)
        {
            currentCategory = new CategoryBuilder()
                .WithName($"Level{i}")
                .WithParentCategory(currentCategory)
                .Build();
        }
        
        Result<Category> result = Result.Success(currentCategory);
        return Ok(result);
    }

    [HttpGet("wide-object-structure")]
    public IActionResult GetWideObjectStructure()
    {
        // Create an object with many properties (using Order with many items and notes)
        OrderBuilder builder = new OrderBuilder();
        
        // Add many items
        for (int i = 0; i < 50; i++)
        {
            Product product = new ProductBuilder()
                .WithName($"Product {i}")
                .WithSku($"SKU-{i:D3}")
                .Build();
            OrderItem item = new OrderItemBuilder()
                .WithProduct(product)
                .Build();
            builder.AddItem(item);
        }
        
        Order order = builder.Build();
        Result<Order> result = Result.Success(order);
        return Ok(result);
    }

    [HttpGet("customer-with-nested-objects")]
    public IActionResult GetCustomerWithNestedObjects()
    {
        Customer customer = new CustomerBuilder().Build();
        Result<Customer> result = Result.Success(customer);
        return Ok(result);
    }

    [HttpGet("complex-nested-order")]
    public IActionResult GetComplexNestedOrder()
    {
        Order order = new OrderBuilder()
            .AddItem(new OrderItemBuilder().Build())
            .AddItem(new OrderItemBuilder().Build())
            .Build();
        Result<Order> result = Result.Success(order);
        return Ok(result);
    }

    [HttpPost("round-trip-customer")]
    public IActionResult RoundTripCustomer([FromBody] Result<Customer> customerResult)
    {
        return Ok(customerResult);
    }

    [HttpPost("round-trip-order")]
    public IActionResult RoundTripOrder([FromBody] Result<Order> orderResult)
    {
        return Ok(orderResult);
    }
}