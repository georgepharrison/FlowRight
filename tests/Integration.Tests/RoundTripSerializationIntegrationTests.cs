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
/// Integration tests that verify complete round-trip JSON serialization preserves data integrity 
/// and correctness for Result types in real API scenarios.
/// </summary>
/// <remarks>
/// <para>
/// These tests ensure that Result types can be serialized to JSON, sent over HTTP, deserialized,
/// processed, re-serialized, and returned while maintaining complete data fidelity and correctness.
/// This validates that the custom JSON converters work correctly in both directions and handle
/// all edge cases properly.
/// </para>
/// <para>
/// Test scenarios include:
/// <list type="bullet">
/// <item><description>Simple success and failure Result round-trips</description></item>
/// <item><description>Complex nested object serialization and deserialization</description></item>
/// <item><description>Validation failure preservation with field-specific errors</description></item>
/// <item><description>Different failure types (security, cancellation, etc.)</description></item>
/// <item><description>Edge cases like null values, empty collections, and extreme values</description></item>
/// <item><description>Unicode characters and special string content</description></item>
/// <item><description>Large payload round-trips</description></item>
/// <item><description>Circular reference handling and deep nesting</description></item>
/// </list>
/// </para>
/// </remarks>
public class RoundTripSerializationIntegrationTests : IClassFixture<WebApplicationFactory<RoundTripTestApiStartup>>
{
    private readonly WebApplicationFactory<RoundTripTestApiStartup> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public RoundTripSerializationIntegrationTests(WebApplicationFactory<RoundTripTestApiStartup> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = CreateJsonOptions();
    }

    /// <summary>
    /// Tests that verify basic Result round-trip serialization correctness.
    /// </summary>
    public class BasicResultRoundTripTests : RoundTripSerializationIntegrationTests
    {
        public BasicResultRoundTripTests(WebApplicationFactory<RoundTripTestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task SuccessResult_RoundTrip_ShouldPreserveAllProperties()
        {
            // Arrange
            Result originalResult = Result.Success(ResultType.Information);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result deserializedResult = JsonSerializer.Deserialize<Result>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.IsSuccess.ShouldBe(originalResult.IsSuccess);
            deserializedResult.ResultType.ShouldBe(originalResult.ResultType);
            deserializedResult.Error.ShouldBe(originalResult.Error);
            deserializedResult.FailureType.ShouldBe(originalResult.FailureType);
            deserializedResult.Failures.ShouldBe(originalResult.Failures);
        }

        [Fact]
        public async Task FailureResult_RoundTrip_ShouldPreserveErrorMessage()
        {
            // Arrange
            Result originalResult = Result.Failure("Test error message", ResultType.Error);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result deserializedResult = JsonSerializer.Deserialize<Result>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.IsFailure.ShouldBe(originalResult.IsFailure);
            deserializedResult.Error.ShouldBe(originalResult.Error);
            deserializedResult.ResultType.ShouldBe(originalResult.ResultType);
            deserializedResult.FailureType.ShouldBe(originalResult.FailureType);
        }

        [Fact]
        public async Task SecurityFailureResult_RoundTrip_ShouldPreserveFailureType()
        {
            // Arrange
            Result originalResult = Result.Failure(new System.Security.SecurityException("Access denied"));
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result deserializedResult = JsonSerializer.Deserialize<Result>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.IsFailure.ShouldBe(originalResult.IsFailure);
            deserializedResult.FailureType.ShouldBe(ResultFailureType.Security);
            deserializedResult.Error.ShouldBe(originalResult.Error);
        }

        [Fact]
        public async Task CancellationFailureResult_RoundTrip_ShouldPreserveFailureType()
        {
            // Arrange
            Result originalResult = Result.Failure(new OperationCanceledException("Operation was cancelled"));
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result deserializedResult = JsonSerializer.Deserialize<Result>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.IsFailure.ShouldBe(originalResult.IsFailure);
            deserializedResult.FailureType.ShouldBe(ResultFailureType.OperationCanceled);
            deserializedResult.Error.ShouldBe(originalResult.Error);
        }
    }

    /// <summary>
    /// Tests that verify Result&lt;T&gt; round-trip serialization with typed values.
    /// </summary>
    public class ResultTRoundTripTests : RoundTripSerializationIntegrationTests
    {
        public ResultTRoundTripTests(WebApplicationFactory<RoundTripTestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task ResultWithPrimitiveType_RoundTrip_ShouldPreserveValue()
        {
            // Arrange
            Result<int> originalResult = Result.Success(42, ResultType.Information);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result-int", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<int> deserializedResult = JsonSerializer.Deserialize<Result<int>>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.IsSuccess.ShouldBe(originalResult.IsSuccess);
            deserializedResult.TryGetValue(out int deserializedValue).ShouldBeTrue();
            originalResult.TryGetValue(out int originalValue).ShouldBeTrue();
            deserializedValue.ShouldBe(originalValue);
            deserializedResult.ResultType.ShouldBe(originalResult.ResultType);
        }

        [Fact]
        public async Task ResultWithStringType_RoundTrip_ShouldPreserveStringValue()
        {
            // Arrange
            Result<string> originalResult = Result.Success("Test string value");
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result-string", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<string> deserializedResult = JsonSerializer.Deserialize<Result<string>>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.IsSuccess.ShouldBe(originalResult.IsSuccess);
            deserializedResult.TryGetValue(out string? deserializedValue).ShouldBeTrue();
            originalResult.TryGetValue(out string? originalValue).ShouldBeTrue();
            deserializedValue.ShouldBe(originalValue);
        }

        [Fact]
        public async Task ResultWithComplexObject_RoundTrip_ShouldPreserveAllObjectProperties()
        {
            // Arrange
            Customer originalCustomer = new CustomerBuilder()
                .WithFirstName("John")
                .WithLastName("Doe")
                .WithEmail("john.doe@example.com")
                .Build();
            Result<Customer> originalResult = Result.Success(originalCustomer);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result-customer", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<Customer> deserializedResult = JsonSerializer.Deserialize<Result<Customer>>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.IsSuccess.ShouldBe(originalResult.IsSuccess);
            deserializedResult.TryGetValue(out Customer? deserializedCustomer).ShouldBeTrue();
            originalResult.TryGetValue(out Customer? originalCustomerValue).ShouldBeTrue();
            
            deserializedCustomer!.FirstName.ShouldBe(originalCustomerValue!.FirstName);
            deserializedCustomer.LastName.ShouldBe(originalCustomerValue.LastName);
            deserializedCustomer.Email.ShouldBe(originalCustomerValue.Email);
            deserializedCustomer.Id.ShouldBe(originalCustomerValue.Id);
        }

        [Fact]
        public async Task FailureResultT_RoundTrip_ShouldNotHaveValue()
        {
            // Arrange
            Result<Customer> originalResult = Result.Failure<Customer>("Customer not found");
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result-customer", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<Customer> deserializedResult = JsonSerializer.Deserialize<Result<Customer>>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.IsFailure.ShouldBe(originalResult.IsFailure);
            deserializedResult.Error.ShouldBe(originalResult.Error);
            deserializedResult.TryGetValue(out Customer? _).ShouldBeFalse();
        }
    }

    /// <summary>
    /// Tests that verify validation failure round-trip serialization with complex error structures.
    /// </summary>
    public class ValidationFailureRoundTripTests : RoundTripSerializationIntegrationTests
    {
        public ValidationFailureRoundTripTests(WebApplicationFactory<RoundTripTestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task ValidationFailureWithMultipleFields_RoundTrip_ShouldPreserveAllErrors()
        {
            // Arrange
            Dictionary<string, string[]> originalErrors = new()
            {
                { "FirstName", ["First name is required", "First name must be at least 2 characters"] },
                { "Email", ["Email is required", "Email format is invalid"] },
                { "DateOfBirth", ["Date of birth must be in the past"] }
            };
            Result<Customer> originalResult = Result.ValidationFailure<Customer>(originalErrors);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result-customer", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<Customer> deserializedResult = JsonSerializer.Deserialize<Result<Customer>>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.IsFailure.ShouldBe(originalResult.IsFailure);
            deserializedResult.FailureType.ShouldBe(ResultFailureType.Validation);
            deserializedResult.Failures.Count.ShouldBe(originalErrors.Count);
            
            foreach (KeyValuePair<string, string[]> originalError in originalErrors)
            {
                deserializedResult.Failures.ShouldContainKey(originalError.Key);
                deserializedResult.Failures[originalError.Key].ShouldBe(originalError.Value);
            }
        }

        [Fact]
        public async Task ValidationFailureWithNestedFieldNames_RoundTrip_ShouldPreserveFieldStructure()
        {
            // Arrange
            Dictionary<string, string[]> originalErrors = new()
            {
                { "Customer.FirstName", ["Required"] },
                { "Customer.PrimaryAddress.Street", ["Street address is required"] },
                { "Items[0].Product.Name", ["Product name is required"] },
                { "Items[1].Quantity", ["Quantity must be greater than 0"] },
                { "ShippingAddress.PostalCode", ["Invalid postal code format"] }
            };
            Result<Order> originalResult = Result.ValidationFailure<Order>(originalErrors);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result-order", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<Order> deserializedResult = JsonSerializer.Deserialize<Result<Order>>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.IsFailure.ShouldBe(originalResult.IsFailure);
            deserializedResult.FailureType.ShouldBe(ResultFailureType.Validation);
            
            foreach (KeyValuePair<string, string[]> originalError in originalErrors)
            {
                deserializedResult.Failures.ShouldContainKey(originalError.Key);
                deserializedResult.Failures[originalError.Key].ShouldBe(originalError.Value);
            }
        }

        [Fact]
        public async Task ValidationFailureWithEmptyErrors_RoundTrip_ShouldHandleGracefully()
        {
            // Arrange
            Dictionary<string, string[]> emptyErrors = [];
            Result<Customer> originalResult = Result.ValidationFailure<Customer>(emptyErrors);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result-customer", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<Customer> deserializedResult = JsonSerializer.Deserialize<Result<Customer>>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.IsFailure.ShouldBe(originalResult.IsFailure);
            deserializedResult.Failures.ShouldBeEmpty();
        }
    }

    /// <summary>
    /// Tests that verify edge cases and special scenarios in round-trip serialization.
    /// </summary>
    public class EdgeCaseRoundTripTests : RoundTripSerializationIntegrationTests
    {
        public EdgeCaseRoundTripTests(WebApplicationFactory<RoundTripTestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task ResultWithNullValue_RoundTrip_ShouldHandleNullProperly()
        {
            // Arrange
            Result<string?> originalResult = Result.Success<string?>(null);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result-string", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<string?> deserializedResult = JsonSerializer.Deserialize<Result<string?>>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.IsSuccess.ShouldBe(originalResult.IsSuccess);
            deserializedResult.TryGetValue(out string? deserializedValue).ShouldBeTrue();
            deserializedValue.ShouldBeNull();
        }

        [Fact]
        public async Task ResultWithUnicodeCharacters_RoundTrip_ShouldPreserveEncoding()
        {
            // Arrange
            string unicodeString = "Test with Unicode: 你好世界 🌍 Ñoño café résumé";
            Result<string> originalResult = Result.Success(unicodeString);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result-string", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<string> deserializedResult = JsonSerializer.Deserialize<Result<string>>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.TryGetValue(out string? deserializedValue).ShouldBeTrue();
            deserializedValue.ShouldBe(unicodeString);
        }

        [Fact]
        public async Task ResultWithSpecialCharactersInError_RoundTrip_ShouldPreserveSpecialChars()
        {
            // Arrange
            string specialErrorMessage = "Error with \"quotes\", 'apostrophes', \\backslashes\\ and \n newlines \t tabs";
            Result originalResult = Result.Failure(specialErrorMessage);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result deserializedResult = JsonSerializer.Deserialize<Result>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.Error.ShouldBe(specialErrorMessage);
        }

        [Fact]
        public async Task ResultWithVeryLongErrorMessage_RoundTrip_ShouldPreserveEntireMessage()
        {
            // Arrange
            string longErrorMessage = string.Concat(Enumerable.Repeat("This is a very long error message that should be preserved completely during serialization. ", 100));
            Result originalResult = Result.Failure(longErrorMessage);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result deserializedResult = JsonSerializer.Deserialize<Result>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.Error.ShouldBe(longErrorMessage);
            deserializedResult.Error.Length.ShouldBe(longErrorMessage.Length);
        }

        [Fact]
        public async Task ResultWithExtremeDateTimeValues_RoundTrip_ShouldPreserveDates()
        {
            // Arrange
            Customer customerWithExtremeDates = new CustomerBuilder()
                .WithDateOfBirth(DateTime.MinValue)
                .Build();
            Result<Customer> originalResult = Result.Success(customerWithExtremeDates);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result-customer", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<Customer> deserializedResult = JsonSerializer.Deserialize<Result<Customer>>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.TryGetValue(out Customer? deserializedCustomer).ShouldBeTrue();
            deserializedCustomer!.DateOfBirth.ShouldBe(DateTime.MinValue);
        }
    }

    /// <summary>
    /// Tests that verify round-trip serialization with deeply nested and complex object structures.
    /// </summary>
    public class ComplexStructureRoundTripTests : RoundTripSerializationIntegrationTests
    {
        public ComplexStructureRoundTripTests(WebApplicationFactory<RoundTripTestApiStartup> factory) : base(factory) { }

        [Fact]
        public async Task ResultWithDeeplyNestedOrder_RoundTrip_ShouldPreserveAllNestingLevels()
        {
            // Arrange - Create order with multiple levels of nesting
            Order complexOrder = new OrderBuilder()
                .AddItem(new OrderItemBuilder()
                    .WithProduct(new ProductBuilder()
                        .WithCategory(new CategoryBuilder()
                            .WithParentCategory(new CategoryBuilder()
                                .WithName("Root Category")
                                .Build())
                            .WithName("Sub Category")
                            .Build())
                        .WithName("Complex Product")
                        .Build())
                    .WithQuantity(5)
                    .Build())
                .Build();

            Result<Order> originalResult = Result.Success(complexOrder);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result-order", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<Order> deserializedResult = JsonSerializer.Deserialize<Result<Order>>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.TryGetValue(out Order? deserializedOrder).ShouldBeTrue();
            
            // Verify nested structure preservation
            OrderItem firstItem = deserializedOrder!.Items.First();
            firstItem.Product.Name.ShouldBe("Complex Product");
            firstItem.Product.Category.Name.ShouldBe("Sub Category");
            firstItem.Product.Category.ParentCategory!.Name.ShouldBe("Root Category");
            firstItem.Quantity.ShouldBe(5);
        }

        [Fact]
        public async Task ResultWithCollectionOfComplexObjects_RoundTrip_ShouldPreserveAllItems()
        {
            // Arrange - Create order with multiple complex items
            OrderBuilder orderBuilder = new OrderBuilder();
            for (int i = 0; i < 5; i++)
            {
                Product product = new ProductBuilder()
                    .WithName($"Product {i}")
                    .WithSku($"SKU-{i:D3}")
                    .WithPrice(100m + i)
                    .Build();
                OrderItem item = new OrderItemBuilder()
                    .WithProduct(product)
                    .WithQuantity(i + 1)
                    .WithUnitPrice(product.Price)
                    .Build();
                orderBuilder.AddItem(item);
            }

            Order orderWithManyItems = orderBuilder.Build();
            Result<Order> originalResult = Result.Success(orderWithManyItems);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result-order", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<Order> deserializedResult = JsonSerializer.Deserialize<Result<Order>>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.TryGetValue(out Order? deserializedOrder).ShouldBeTrue();
            deserializedOrder!.Items.Count().ShouldBe(5);
            
            int index = 0;
            foreach (OrderItem item in deserializedOrder.Items)
            {
                item.Product.Name.ShouldBe($"Product {index}");
                item.Product.Sku.ShouldBe($"SKU-{index:D3}");
                item.Quantity.ShouldBe(index + 1);
                index++;
            }
        }

        [Fact]
        public async Task ResultWithArrayOfResults_RoundTrip_ShouldPreserveAllResults()
        {
            // Arrange - Array of different Result types
            Result<Customer>[] customerResults = 
            [
                Result.Success(new CustomerBuilder().WithFirstName("John").Build()),
                Result.Failure<Customer>("Customer not found"),
                Result.ValidationFailure<Customer>(new Dictionary<string, string[]> { { "Email", ["Invalid"] } })
            ];

            Result<Result<Customer>[]> originalResult = Result.Success(customerResults);
            string json = JsonSerializer.Serialize(originalResult, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/roundtrip/echo-result-customer-array", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            Result<Result<Customer>[]> deserializedResult = JsonSerializer.Deserialize<Result<Result<Customer>[]>>(responseJson, _jsonOptions)!;

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            deserializedResult.TryGetValue(out Result<Customer>[]? deserializedArray).ShouldBeTrue();
            deserializedArray!.Length.ShouldBe(3);
            
            deserializedArray[0].IsSuccess.ShouldBeTrue();
            deserializedArray[1].IsFailure.ShouldBeTrue();
            deserializedArray[2].FailureType.ShouldBe(ResultFailureType.Validation);
        }
    }

    /// <summary>
    /// Creates JsonSerializerOptions configured for round-trip testing.
    /// </summary>
    private static JsonSerializerOptions CreateJsonOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        
        // Register all necessary converters
        options.Converters.Add(new ResultJsonConverter());
        options.Converters.Add(new ResultTJsonConverter<int>());
        options.Converters.Add(new ResultTJsonConverter<string>());
        options.Converters.Add(new ResultTJsonConverter<Customer>());
        options.Converters.Add(new ResultTJsonConverter<Order>());
        options.Converters.Add(new ResultTJsonConverter<Result<Customer>[]>());
        
        return options;
    }
}

/// <summary>
/// Specialized API startup for round-trip testing scenarios.
/// </summary>
public class RoundTripTestApiStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                
                // Register all necessary converters for round-trip testing
                options.JsonSerializerOptions.Converters.Add(new ResultJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<int>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<string>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Customer>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Order>());
                options.JsonSerializerOptions.Converters.Add(new ResultTJsonConverter<Result<Customer>[]>());
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}

/// <summary>
/// Controller specialized for round-trip testing scenarios.
/// All endpoints simply echo back the received Result to test round-trip serialization.
/// </summary>
[ApiController]
[Route("api/roundtrip")]
public class RoundtripController : ControllerBase
{
    /// <summary>
    /// Echoes back a Result for round-trip testing.
    /// </summary>
    [HttpPost("echo-result")]
    public IActionResult EchoResult([FromBody] Result result) => Ok(result);

    /// <summary>
    /// Echoes back a Result&lt;int&gt; for round-trip testing.
    /// </summary>
    [HttpPost("echo-result-int")]
    public IActionResult EchoResultInt([FromBody] Result<int> result) => Ok(result);

    /// <summary>
    /// Echoes back a Result&lt;string&gt; for round-trip testing.
    /// </summary>
    [HttpPost("echo-result-string")]
    public IActionResult EchoResultString([FromBody] Result<string> result) => Ok(result);

    /// <summary>
    /// Echoes back a Result&lt;Customer&gt; for round-trip testing.
    /// </summary>
    [HttpPost("echo-result-customer")]
    public IActionResult EchoResultCustomer([FromBody] Result<Customer> result) => Ok(result);

    /// <summary>
    /// Echoes back a Result&lt;Order&gt; for round-trip testing.
    /// </summary>
    [HttpPost("echo-result-order")]
    public IActionResult EchoResultOrder([FromBody] Result<Order> result) => Ok(result);

    /// <summary>
    /// Echoes back a Result&lt;Result&lt;Customer&gt;[]&gt; for testing nested Result serialization.
    /// </summary>
    [HttpPost("echo-result-customer-array")]
    public IActionResult EchoResultCustomerArray([FromBody] Result<Result<Customer>[]> result) => Ok(result);
}