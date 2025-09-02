using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
/// Integration tests that verify Result serialization behavior with different JsonSerializerOptions configurations.
/// These tests ensure that custom JSON settings (naming policies, formatting, etc.) work correctly 
/// with Result types in real API scenarios.
/// </summary>
/// <remarks>
/// <para>
/// These tests validate that FlowRight's custom JSON converters respect and work correctly with
/// various System.Text.Json serialization settings that applications might use.
/// </para>
/// <para>
/// Test scenarios include:
/// <list type="bullet">
/// <item><description>Different naming policies (camelCase, snake_case, PascalCase)</description></item>
/// <item><description>Custom DateTime formatting and converters</description></item>
/// <item><description>Null value handling configurations</description></item>
/// <item><description>Number handling and precision settings</description></item>
/// <item><description>Property ordering and indentation preferences</description></item>
/// <item><description>Case sensitivity and comparison settings</description></item>
/// </list>
/// </para>
/// </remarks>
public class JsonSerializationSettingsIntegrationTests
{
    /// <summary>
    /// Tests that verify Result serialization respects different JSON naming policies.
    /// </summary>
    public class NamingPolicyTests : IClassFixture<WebApplicationFactory<CamelCaseApiStartup>>
    {
        private readonly HttpClient _client;

        public NamingPolicyTests(WebApplicationFactory<CamelCaseApiStartup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ResultSerialization_WithCamelCaseNaming_ShouldUseCamelCasePropertyNames()
        {
            // Arrange - API configured with camelCase naming policy
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/naming/camel-case-result");
            string jsonContent = await response.Content.ReadAsStringAsync();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            jsonContent.ShouldContain("\"resultType\"");
            jsonContent.ShouldContain("\"failureType\"");
            jsonContent.ShouldNotContain("\"ResultType\"");
            jsonContent.ShouldNotContain("\"FailureType\"");
        }

        [Fact]
        public async Task ResultTSerialization_WithCamelCaseNaming_ShouldApplyNamingToNestedObjects()
        {
            // Arrange - API configured with camelCase naming policy
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/naming/camel-case-customer");
            string jsonContent = await response.Content.ReadAsStringAsync();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            jsonContent.ShouldContain("\"firstName\"");
            jsonContent.ShouldContain("\"lastName\"");
            jsonContent.ShouldContain("\"primaryAddress\"");
            jsonContent.ShouldNotContain("\"FirstName\"");
            jsonContent.ShouldNotContain("\"LastName\"");
        }
    }

    /// <summary>
    /// Tests that verify Result serialization with snake_case naming policy.
    /// </summary>
    public class SnakeCaseNamingTests : IClassFixture<WebApplicationFactory<SnakeCaseApiStartup>>
    {
        private readonly HttpClient _client;

        public SnakeCaseNamingTests(WebApplicationFactory<SnakeCaseApiStartup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ResultSerialization_WithSnakeCaseNaming_ShouldUseSnakeCasePropertyNames()
        {
            // Arrange - API configured with snake_case naming policy
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/naming/snake-case-result");
            string jsonContent = await response.Content.ReadAsStringAsync();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            jsonContent.ShouldContain("\"result_type\"");
            jsonContent.ShouldContain("\"failure_type\"");
            jsonContent.ShouldNotContain("\"resultType\"");
            jsonContent.ShouldNotContain("\"ResultType\"");
        }

        [Fact]
        public async Task ValidationFailureSerialization_WithSnakeCaseNaming_ShouldApplyToErrorDictionary()
        {
            // Arrange - API configured with snake_case naming policy
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/naming/snake-case-validation");
            string jsonContent = await response.Content.ReadAsStringAsync();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            jsonContent.ShouldContain("\"failure_type\"");
            jsonContent.ShouldContain("\"failures\"");
        }
    }

    /// <summary>
    /// Tests that verify Result serialization with custom DateTime formatting.
    /// </summary>
    public class DateTimeFormattingTests : IClassFixture<WebApplicationFactory<CustomDateTimeApiStartup>>
    {
        private readonly HttpClient _client;

        public DateTimeFormattingTests(WebApplicationFactory<CustomDateTimeApiStartup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CustomerSerialization_WithCustomDateTimeFormat_ShouldUseSpecifiedFormat()
        {
            // Arrange - API configured with custom DateTime format "yyyy-MM-dd HH:mm:ss"
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/datetime/customer-with-dates");
            string jsonContent = await response.Content.ReadAsStringAsync();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            // Should contain custom DateTime format (e.g., "2024-01-15 14:30:00")
            jsonContent.ShouldMatch(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}");
            jsonContent.ShouldNotContain("T"); // ISO format would contain T
        }

        [Fact]
        public async Task OrderSerialization_WithNestedDateTimes_ShouldFormatAllDates()
        {
            // Arrange - API configured with custom DateTime format
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/datetime/order-with-dates");
            string jsonContent = await response.Content.ReadAsStringAsync();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            // All DateTime properties should use custom format
            jsonContent.ShouldMatch(@"createdAt"":\s*""\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}""");
        }
    }

    /// <summary>
    /// Tests that verify Result serialization with null value handling configurations.
    /// </summary>
    public class NullValueHandlingTests : IClassFixture<WebApplicationFactory<IgnoreNullsApiStartup>>
    {
        private readonly HttpClient _client;

        public NullValueHandlingTests(WebApplicationFactory<IgnoreNullsApiStartup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ResultSerialization_WithIgnoreNullValues_ShouldExcludeNullProperties()
        {
            // Arrange - API configured to ignore null values
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/nulls/customer-with-nulls");
            string jsonContent = await response.Content.ReadAsStringAsync();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            jsonContent.ShouldNotContain("\"phone\":null");
            jsonContent.ShouldNotContain("\"completedAt\":null");
        }

        [Fact]
        public async Task FailureResultSerialization_WithIgnoreNullValues_ShouldIncludeEmptyStringError()
        {
            // Arrange - API configured to ignore null values but should still include empty error strings
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/nulls/success-result");
            string jsonContent = await response.Content.ReadAsStringAsync();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            jsonContent.ShouldContain("\"error\":\"\""); // Empty string should be included
        }
    }

    /// <summary>
    /// Tests that verify Result serialization with number handling and precision settings.
    /// </summary>
    public class NumberHandlingTests : IClassFixture<WebApplicationFactory<NumberHandlingApiStartup>>
    {
        private readonly HttpClient _client;

        public NumberHandlingTests(WebApplicationFactory<NumberHandlingApiStartup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task OrderSerialization_WithHighPrecisionDecimals_ShouldPreservePrecision()
        {
            // Arrange - API configured with specific decimal precision handling
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/numbers/high-precision-order");
            string jsonContent = await response.Content.ReadAsStringAsync();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            jsonContent.ShouldContain("99.999999"); // High precision decimal values
        }

        [Fact]
        public async Task ProductSerialization_WithNumberAsStrings_ShouldWriteNumbersAsStrings()
        {
            // Arrange - API configured to write numbers as strings
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/numbers/product-as-strings");
            string jsonContent = await response.Content.ReadAsStringAsync();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            jsonContent.ShouldContain("\"price\":\"99.99\""); // Decimal as string
            jsonContent.ShouldContain("\"stockQuantity\":\"10\""); // Integer as string
        }
    }

    /// <summary>
    /// Tests that verify Result deserialization is case insensitive when configured.
    /// </summary>
    public class CaseInsensitiveDeserializationTests : IClassFixture<WebApplicationFactory<CaseInsensitiveApiStartup>>
    {
        private readonly HttpClient _client;

        public CaseInsensitiveDeserializationTests(WebApplicationFactory<CaseInsensitiveApiStartup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ResultDeserialization_WithMixedCasePropertyNames_ShouldDeserializeSuccessfully()
        {
            // Arrange - Send Result JSON with mixed case property names
            string mixedCaseJson = """
                {
                    "ERROR": "",
                    "FAILURES": {},
                    "failureType": "None",
                    "RESULTTYPE": "Success"
                }
                """;
            StringContent content = new(mixedCaseJson, Encoding.UTF8, "application/json");
            
            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/case/process-mixed-case-result", content);
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ResultTDeserialization_WithMixedCasePropertyNames_ShouldDeserializeValue()
        {
            // Arrange - Send Result<Customer> JSON with mixed case property names
            string mixedCaseJson = """
                {
                    "VALUE": {
                        "FIRSTNAME": "John",
                        "lastname": "Doe",
                        "EMAIL": "john@example.com"
                    },
                    "error": "",
                    "FAILURES": {},
                    "FailureType": "None",
                    "resulttype": "Success"
                }
                """;
            StringContent content = new(mixedCaseJson, Encoding.UTF8, "application/json");
            
            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/case/process-mixed-case-customer", content);
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    /// <summary>
    /// Tests that verify Result serialization with property ordering preferences.
    /// </summary>
    public class PropertyOrderingTests : IClassFixture<WebApplicationFactory<TestApiStartup>>
    {
        private readonly HttpClient _client;

        public PropertyOrderingTests(WebApplicationFactory<TestApiStartup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ResultSerialization_ShouldMaintainConsistentPropertyOrder()
        {
            // Arrange - Test that Result properties are serialized in consistent order
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/ordering/result-property-order");
            string jsonContent = await response.Content.ReadAsStringAsync();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            // Verify properties appear in expected order (error, failures, failureType, resultType)
            int errorIndex = jsonContent.IndexOf("\"error\"");
            int failuresIndex = jsonContent.IndexOf("\"failures\"");
            int failureTypeIndex = jsonContent.IndexOf("\"failureType\"");
            int resultTypeIndex = jsonContent.IndexOf("\"resultType\"");
            
            errorIndex.ShouldBeLessThan(failuresIndex);
            failuresIndex.ShouldBeLessThan(failureTypeIndex);
            failureTypeIndex.ShouldBeLessThan(resultTypeIndex);
        }

        [Fact]
        public async Task ResultTSerialization_WithValue_ShouldPutValueFirst()
        {
            // Arrange - Test that value property appears first in Result<T>
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/ordering/result-t-property-order");
            string jsonContent = await response.Content.ReadAsStringAsync();
            
            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            // Value should appear before other Result properties
            int valueIndex = jsonContent.IndexOf("\"value\"");
            int errorIndex = jsonContent.IndexOf("\"error\"");
            
            valueIndex.ShouldBeLessThan(errorIndex);
        }
    }
}

/// <summary>
/// Test API startup for camelCase naming policy.
/// </summary>
public class CamelCaseApiStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new ResultJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Customer>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Order>());
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}

/// <summary>
/// Test API startup for snake_case naming policy.
/// </summary>
public class SnakeCaseApiStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                options.JsonSerializerOptions.Converters.Add(new ResultJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Customer>());
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}

/// <summary>
/// Test API startup with custom DateTime formatting.
/// </summary>
public class CustomDateTimeApiStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
                options.JsonSerializerOptions.Converters.Add(new ResultJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Customer>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Order>());
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}

/// <summary>
/// Test API startup that ignores null values.
/// </summary>
public class IgnoreNullsApiStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new ResultJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Customer>());
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}

/// <summary>
/// Test API startup with specific number handling.
/// </summary>
public class NumberHandlingApiStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.WriteAsString;
                options.JsonSerializerOptions.Converters.Add(new ResultJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Product>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Order>());
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}

/// <summary>
/// Test API startup with case insensitive property matching.
/// </summary>
public class CaseInsensitiveApiStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.Converters.Add(new ResultJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Customer>());
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}

/// <summary>
/// Custom DateTime converter for testing.
/// </summary>
public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? dateString = reader.GetString();
        return DateTime.ParseExact(dateString!, DateFormat, null);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateFormat));
    }
}

/// <summary>
/// Controller for testing different naming policies and serialization settings.
/// </summary>
[ApiController]
[Route("api/naming")]
public class NamingController : ControllerBase
{
    [HttpGet("camel-case-result")]
    public IActionResult GetCamelCaseResult()
    {
        Result result = Result.Success(ResultType.Information);
        return Ok(result);
    }

    [HttpGet("camel-case-customer")]
    public IActionResult GetCamelCaseCustomer()
    {
        Customer customer = new CustomerBuilder().Build();
        Result<Customer> result = Result.Success(customer);
        return Ok(result);
    }

    [HttpGet("snake-case-result")]
    public IActionResult GetSnakeCaseResult()
    {
        Result result = Result.Success(ResultType.Warning);
        return Ok(result);
    }

    [HttpGet("snake-case-validation")]
    public IActionResult GetSnakeCaseValidation()
    {
        Dictionary<string, string[]> errors = new()
        {
            { "FirstName", ["Required"] },
            { "Email", ["Invalid format"] }
        };
        Result<Customer> result = Result.ValidationFailure<Customer>(errors);
        return BadRequest(result);
    }
}

/// <summary>
/// Controller for testing DateTime formatting.
/// </summary>
[ApiController]
[Route("api/datetime")]
public class DateTimeController : ControllerBase
{
    [HttpGet("customer-with-dates")]
    public IActionResult GetCustomerWithDates()
    {
        Customer customer = new CustomerBuilder()
            .WithDateOfBirth(DateTime.Now.AddYears(-25))
            .Build();
        Result<Customer> result = Result.Success(customer);
        return Ok(result);
    }

    [HttpGet("order-with-dates")]
    public IActionResult GetOrderWithDates()
    {
        Order order = new OrderBuilder().Build();
        Result<Order> result = Result.Success(order);
        return Ok(result);
    }
}

/// <summary>
/// Controller for testing null value handling.
/// </summary>
[ApiController]
[Route("api/nulls")]
public class NullsController : ControllerBase
{
    [HttpGet("customer-with-nulls")]
    public IActionResult GetCustomerWithNulls()
    {
        Customer customer = new CustomerBuilder().Build();
        Result<Customer> result = Result.Success(customer);
        return Ok(result);
    }

    [HttpGet("success-result")]
    public IActionResult GetSuccessResult()
    {
        Result result = Result.Success();
        return Ok(result);
    }
}

/// <summary>
/// Controller for testing number handling.
/// </summary>
[ApiController]
[Route("api/numbers")]
public class NumbersController : ControllerBase
{
    [HttpGet("high-precision-order")]
    public IActionResult GetHighPrecisionOrder()
    {
        Order order = new OrderBuilder().WithTotalAmount(99.999999m).Build();
        Result<Order> result = Result.Success(order);
        return Ok(result);
    }

    [HttpGet("product-as-strings")]
    public IActionResult GetProductAsStrings()
    {
        Product product = new ProductBuilder().WithPrice(99.99m).Build();
        Result<Product> result = Result.Success(product);
        return Ok(result);
    }
}

/// <summary>
/// Controller for testing case insensitive deserialization.
/// </summary>
[ApiController]
[Route("api/case")]
public class CaseController : ControllerBase
{
    [HttpPost("process-mixed-case-result")]
    public IActionResult ProcessMixedCaseResult([FromBody] Result result)
    {
        return Ok(new { Success = result.IsSuccess });
    }

    [HttpPost("process-mixed-case-customer")]
    public IActionResult ProcessMixedCaseCustomer([FromBody] Result<Customer> customerResult)
    {
        return Ok(new { Success = customerResult.IsSuccess });
    }
}

/// <summary>
/// Controller for testing property ordering.
/// </summary>
[ApiController]
[Route("api/ordering")]
public class OrderingController : ControllerBase
{
    [HttpGet("result-property-order")]
    public IActionResult GetResultPropertyOrder()
    {
        Result result = Result.Success(ResultType.Information);
        return Ok(result);
    }

    [HttpGet("result-t-property-order")]
    public IActionResult GetResultTPropertyOrder()
    {
        Result<string> result = Result.Success("test value");
        return Ok(result);
    }
}