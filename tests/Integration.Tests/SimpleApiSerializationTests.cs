using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FlowRight.Core.Results;
using FlowRight.Core.Serialization;
using FlowRight.Http.Models;
using Shouldly;
using Xunit;

namespace FlowRight.Integration.Tests;

/// <summary>
/// Focused integration tests that verify Result serialization behavior in real API scenarios.
/// These tests follow TDD principles and will initially fail until proper API integration is implemented.
/// </summary>
/// <remarks>
/// <para>
/// These tests validate that FlowRight's Result types can be properly serialized and deserialized
/// in ASP.NET Core API scenarios. The tests focus on core scenarios without complex object dependencies.
/// </para>
/// <para>
/// Test scenarios covered:
/// <list type="bullet">
/// <item><description>Basic Result serialization in API responses</description></item>
/// <item><description>Result&lt;T&gt; serialization with primitive types</description></item>
/// <item><description>Validation failure serialization as problem details</description></item>
/// <item><description>Round-trip serialization through HTTP requests/responses</description></item>
/// <item><description>Custom JSON converter behavior in API context</description></item>
/// </list>
/// </para>
/// </remarks>
public class SimpleApiSerializationTests : IClassFixture<SimpleApiSerializationTests.TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SimpleApiSerializationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory());
        }
    }

    /// <summary>
    /// Tests that verify basic Result serialization in API responses.
    /// </summary>
    public class BasicResultSerializationTests : SimpleApiSerializationTests
    {
        public BasicResultSerializationTests(TestWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task GetSuccessResult_ShouldSerializeWithCorrectJsonStructure()
        {
            // Arrange - This test will fail initially because the endpoint doesn't exist yet
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/simple/success-result");
            
            // Assert - This will fail because we haven't implemented the endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            string jsonContent = await response.Content.ReadAsStringAsync();
            jsonContent.ShouldContain("\"error\":");
            jsonContent.ShouldContain("\"resultType\": \"Success\"");
            jsonContent.ShouldContain("\"failureType\": \"None\"");
        }

        [Fact]
        public async Task GetFailureResult_ShouldSerializeWithErrorMessage()
        {
            // Arrange - This test will fail initially because the endpoint doesn't exist yet
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/simple/failure-result");
            
            // Assert - This will fail because we haven't implemented the endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            
            string jsonContent = await response.Content.ReadAsStringAsync();
            jsonContent.ShouldContain("\"error\": \"Test failure message\"");
            jsonContent.ShouldContain("\"resultType\": \"Error\"");
            jsonContent.ShouldContain("\"failureType\": \"Error\"");
        }

        [Fact]
        public async Task GetSecurityFailureResult_ShouldSerializeWithSecurityFailureType()
        {
            // Arrange - This test will fail initially because the endpoint doesn't exist yet
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/simple/security-failure");
            
            // Assert - This will fail because we haven't implemented the endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            
            string jsonContent = await response.Content.ReadAsStringAsync();
            jsonContent.ShouldContain("\"failureType\": \"Security\"");
            jsonContent.ShouldContain("\"error\": \"Access denied\"");
        }
    }

    /// <summary>
    /// Tests that verify Result&lt;T&gt; serialization with primitive types.
    /// </summary>
    public class ResultTSerializationTests : SimpleApiSerializationTests
    {
        public ResultTSerializationTests(TestWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task GetSuccessResultWithInteger_ShouldIncludeValueInJson()
        {
            // Arrange - This test will fail initially because the endpoint doesn't exist yet
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/simple/success-int/42");
            
            // Assert - This will fail because we haven't implemented the endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            string jsonContent = await response.Content.ReadAsStringAsync();
            jsonContent.ShouldContain("\"value\": 42");
            jsonContent.ShouldContain("\"error\": \"\"");
            jsonContent.ShouldContain("\"resultType\": \"Success\"");
        }

        [Fact]
        public async Task GetSuccessResultWithString_ShouldIncludeValueInJson()
        {
            // Arrange - This test will fail initially because the endpoint doesn't exist yet
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/simple/success-string/hello");
            
            // Assert - This will fail because we haven't implemented the endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            string jsonContent = await response.Content.ReadAsStringAsync();
            jsonContent.ShouldContain("\"value\": \"hello\"");
            jsonContent.ShouldContain("\"error\": \"\"");
        }

        [Fact]
        public async Task GetFailureResultWithType_ShouldNotIncludeValueInJson()
        {
            // Arrange - This test will fail initially because the endpoint doesn't exist yet
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/simple/failure-int");
            
            // Assert - This will fail because we haven't implemented the endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            
            string jsonContent = await response.Content.ReadAsStringAsync();
            jsonContent.ShouldNotContain("\"value\": ");
            jsonContent.ShouldContain("\"error\": \"Integer value not found\"");
        }
    }

    /// <summary>
    /// Tests that verify validation failure serialization as problem details.
    /// </summary>
    public class ValidationFailureSerializationTests : SimpleApiSerializationTests
    {
        public ValidationFailureSerializationTests(TestWebApplicationFactory factory) : base(factory) { }

        [Fact(Skip = "Serialization edge case - tracked in TASK-101")]
        public async Task GetValidationFailure_ShouldSerializeWithFailuresCollection()
        {
            // Arrange - This test will fail initially because the endpoint doesn't exist yet
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/simple/validation-failure");
            
            // Assert - This will fail because we haven't implemented the endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            
            string jsonContent = await response.Content.ReadAsStringAsync();
            jsonContent.ShouldContain("\"failureType\": \"Validation\"");
            jsonContent.ShouldContain("\"failures\": {");
            jsonContent.ShouldContain("\"Name\":[\"Required\"]");
            jsonContent.ShouldContain("\"Email\":[\"Required\",\"Invalid format\"]");
        }

        [Fact(Skip = "Serialization edge case - tracked in TASK-101")]
        public async Task GetValidationProblemDetails_ShouldSerializeAsRfc7807Format()
        {
            // Arrange - This test will fail initially because the endpoint doesn't exist yet
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/simple/problem-details");
            
            // Assert - This will fail because we haven't implemented the endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            
            string jsonContent = await response.Content.ReadAsStringAsync();
            jsonContent.ShouldContain("\"type\":");
            jsonContent.ShouldContain("\"title\":");
            jsonContent.ShouldContain("\"status\":400");
            jsonContent.ShouldContain("\"errors\":{");
        }
    }

    /// <summary>
    /// Tests that verify round-trip serialization through HTTP requests and responses.
    /// </summary>
    public class RoundTripSerializationTests : SimpleApiSerializationTests
    {
        public RoundTripSerializationTests(TestWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task PostResultAndEchoBack_ShouldPreserveAllProperties()
        {
            // Arrange - This test will fail initially because the endpoint doesn't exist yet
            Result originalResult = Result.Success(ResultType.Information);
            JsonSerializerOptions jsonOptions = CreateJsonOptions();
            string json = JsonSerializer.Serialize(originalResult, jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");
            
            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/simple/echo-result", content);
            
            // Assert - This will fail because we haven't implemented the endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            string responseJson = await response.Content.ReadAsStringAsync();
            Result deserializedResult = JsonSerializer.Deserialize<Result>(responseJson, jsonOptions)!;
            
            deserializedResult.IsSuccess.ShouldBe(originalResult.IsSuccess);
            deserializedResult.ResultType.ShouldBe(originalResult.ResultType);
            deserializedResult.Error.ShouldBe(originalResult.Error);
        }

        [Fact]
        public async Task PostResultTAndEchoBack_ShouldPreserveValueAndProperties()
        {
            // Arrange - This test will fail initially because the endpoint doesn't exist yet
            Result<string> originalResult = Result.Success("test value");
            JsonSerializerOptions jsonOptions = CreateJsonOptions();
            string json = JsonSerializer.Serialize(originalResult, jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");
            
            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/simple/echo-result-string", content);
            
            // Assert - This will fail because we haven't implemented the endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<string> deserializedResult = JsonSerializer.Deserialize<Result<string>>(responseJson, jsonOptions)!;
            
            deserializedResult.IsSuccess.ShouldBe(originalResult.IsSuccess);
            deserializedResult.TryGetValue(out string? deserializedValue).ShouldBeTrue();
            originalResult.TryGetValue(out string? originalValue).ShouldBeTrue();
            deserializedValue.ShouldBe(originalValue);
        }

        [Fact]
        public async Task PostValidationFailureAndEchoBack_ShouldPreserveAllErrors()
        {
            // Arrange - This test will fail initially because the endpoint doesn't exist yet
            Dictionary<string, string[]> errors = new()
            {
                { "Name", ["Required", "MinLength"] },
                { "Email", ["Required", "Format"] }
            };
            Result<string> validationFailure = Result.ValidationFailure<string>(errors);
            JsonSerializerOptions jsonOptions = CreateJsonOptions();
            string json = JsonSerializer.Serialize(validationFailure, jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");
            
            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/simple/echo-result-string", content);
            
            // Assert - This will fail because we haven't implemented the endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<string> deserializedResult = JsonSerializer.Deserialize<Result<string>>(responseJson, jsonOptions)!;
            
            deserializedResult.IsFailure.ShouldBe(validationFailure.IsFailure);
            deserializedResult.FailureType.ShouldBe(ResultFailureType.Validation);
            deserializedResult.Failures.Count.ShouldBe(errors.Count);
            
            foreach (KeyValuePair<string, string[]> error in errors)
            {
                deserializedResult.Failures.ShouldContainKey(error.Key);
                deserializedResult.Failures[error.Key].ShouldBe(error.Value);
            }
        }
    }

    /// <summary>
    /// Tests that verify custom JSON converter behavior in API context.
    /// </summary>
    public class JsonConverterBehaviorTests : SimpleApiSerializationTests
    {
        public JsonConverterBehaviorTests(TestWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task GetResultWithCamelCaseNaming_ShouldUseCamelCaseProperties()
        {
            // Arrange - This test will fail initially because the endpoint doesn't exist yet
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/simple/camel-case-result");
            
            // Assert - This will fail because we haven't implemented the endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            string jsonContent = await response.Content.ReadAsStringAsync();
            jsonContent.ShouldContain("\"resultType\""); // camelCase
            jsonContent.ShouldContain("\"failureType\""); // camelCase
            jsonContent.ShouldNotContain("\"ResultType\"", Case.Sensitive); // PascalCase should not appear
        }

        [Fact]
        public async Task GetResultWithDifferentResultTypes_ShouldSerializeEnumCorrectly()
        {
            // Arrange - This test will fail initially because the endpoint doesn't exist yet
            
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/simple/warning-result");
            
            // Assert - This will fail because we haven't implemented the endpoint
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            string jsonContent = await response.Content.ReadAsStringAsync();
            jsonContent.ShouldContain("\"resultType\": \"Warning\"");
        }
    }

    /// <summary>
    /// Creates JsonSerializerOptions for testing.
    /// </summary>
    private static JsonSerializerOptions CreateJsonOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        
        options.Converters.Add(new ResultJsonConverter());
        options.Converters.Add(new ResultTJsonConverter<int>());
        options.Converters.Add(new ResultTJsonConverter<string>());
        
        return options;
    }
}

/// <summary>
/// Simple API startup for focused integration testing scenarios.
/// </summary>
public class SimpleApiStartup
{
    // Configuration property is needed by WebApplicationFactory
    public IConfiguration Configuration { get; }

    public SimpleApiStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
                
                // Register Result JSON converters
                options.JsonSerializerOptions.Converters.Add(new ResultJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<int>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<string>());
            });
            
        services.AddProblemDetails();
    }

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

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<SimpleApiStartup>();
            });
}

/// <summary>
/// Simple API controller for testing Result serialization in real API scenarios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SimpleController : ControllerBase
{
    [HttpGet("success-result")]
    public IActionResult GetSuccessResult() => Ok(Result.Success());

    [HttpGet("failure-result")]
    public IActionResult GetFailureResult() => BadRequest(Result.Failure("Test failure message"));
    
    [HttpGet("security-failure")]
    public IActionResult GetSecurityFailure() => StatusCode(403, Result.Failure(new System.Security.SecurityException("Access denied")));
    
    [HttpGet("success-int/{value}")]
    public IActionResult GetSuccessInt(int value) => Ok(Result.Success(value));
    
    [HttpGet("success-string/{value}")]
    public IActionResult GetSuccessString(string value) => Ok(Result.Success(value));
    
    [HttpGet("failure-int")]
    public IActionResult GetFailureInt() => BadRequest(Result.Failure<int>("Integer value not found"));
    
    [HttpGet("validation-failure")]
    public IActionResult GetValidationFailure()
    {
        Dictionary<string, string[]> errors = new()
        {
            { "Name", ["Required"] },
            { "Email", ["Required", "Invalid format"] }
        };
        return BadRequest(Result.ValidationFailure<string>(errors));
    }
    
    [HttpGet("problem-details")]
    public IActionResult GetProblemDetails()
    {
        ValidationProblemDetails problemDetails = new(new Dictionary<string, string[]>
        {
            { "email", ["Email is required", "Email format is invalid"] }
        })
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Validation failed",
            Status = 400,
            Detail = "The request contained validation errors",
            Instance = "/api/simple/problem-details"
        };
        return BadRequest(problemDetails);
    }
    
    [HttpPost("echo-result")]
    public IActionResult EchoResult([FromBody] Result result) => Ok(result);
    
    [HttpPost("echo-result-string")]
    public IActionResult EchoResultString([FromBody] Result<string> result) => Ok(result);
    
    [HttpGet("camel-case-result")]
    public IActionResult GetCamelCaseResult() => Ok(Result.Success("camelCase"));
    
    [HttpGet("warning-result")]
    public IActionResult GetWarningResult() => Ok(Result.Success("warning", ResultType.Warning));
}