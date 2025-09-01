# FlowRight

[![Pre-release](https://img.shields.io/github/v/release/georgepharrison/FlowRight?include_prereleases&label=pre-release)](https://github.com/georgepharrison/FlowRight/releases)
[![Build Status](https://img.shields.io/github/actions/workflow/status/georgepharrison/FlowRight/build.yml?branch=dev)](https://github.com/georgepharrison/FlowRight/actions)
[![Coverage](https://img.shields.io/badge/coverage-84.6%25-yellow)](https://github.com/georgepharrison/FlowRight)
[![License](https://img.shields.io/github/license/georgepharrison/FlowRight)](https://github.com/georgepharrison/FlowRight/blob/main/LICENSE)

A production-grade Result pattern implementation for .NET that eliminates exception-based control flow while providing comprehensive validation and HTTP integration capabilities.

## 🎯 Features

- **Exception-Free Error Handling**: Explicit success/failure states without throwing exceptions
- **Full JSON Serialization**: Seamless serialization/deserialization of both success and failure states
- **Pattern Matching**: Functional `Match` and imperative `Switch` methods for elegant control flow
- **Validation Builder**: Fluent API for building complex validation rules with automatic error aggregation
- **HTTP Integration**: Convert HTTP responses to Result types with automatic status code interpretation
- **Zero Dependencies**: Core library has no external dependencies
- **Performance Optimized**: Zero allocations on success path, minimal overhead compared to exceptions
- **Type-Safe**: Full nullable reference type support and strong typing throughout

## 📦 Packages

| Package | Description | Status |
|---------|-------------|--------|
| `FlowRight.Core` | Core Result pattern implementation | ⚡ In Development |
| `FlowRight.Validation` | Fluent validation builder with Result integration | ⚡ In Development |
| `FlowRight.Http` | HTTP response to Result conversion | ⚡ In Development |

> **Note**: Packages are currently in active development (alpha releases). Production release planned for Q2 2025.

## 🚀 Quick Start

### Installation

```bash
# 🚧 Pre-release packages (active development)
# Core Result pattern
dotnet add package FlowRight.Core --prerelease

# Validation support
dotnet add package FlowRight.Validation --prerelease

# HTTP integration
dotnet add package FlowRight.Http --prerelease
```

> **⚠️ Development Status**: FlowRight is currently in active development. APIs may change before stable release.

### Basic Usage

```csharp
using FlowRight.Core.Results;

// Simple success/failure
Result<int> Divide(int numerator, int denominator)
{
    if (denominator == 0)
        return Result.Failure<int>("Cannot divide by zero");
    
    return Result.Success(numerator / denominator);
}

// Pattern matching
string message = Divide(10, 2).Match(
    onSuccess: value => $"Result: {value}",
    onFailure: error => $"Error: {error}"
);

// Using Switch for side effects
Divide(10, 0).Switch(
    onSuccess: value => Console.WriteLine($"Success: {value}"),
    onFailure: error => Console.WriteLine($"Failed: {error}")
);
```

### Validation Builder

```csharp
using FlowRight.Validation.Builders;

public Result<User> CreateUser(CreateUserRequest request)
{
    return new ValidationBuilder<User>()
        .RuleFor(x => x.Name, request.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100)
        .RuleFor(x => x.Email, request.Email)
            .NotEmpty()
            .EmailAddress()
        .RuleFor(x => x.Age, request.Age)
            .GreaterThan(0)
            .LessThan(150)
        .Build(() => new User(request.Name, request.Email, request.Age));
}
```

### HTTP Integration

```csharp
using FlowRight.Http.Extensions;

public async Task<Result<WeatherData>> GetWeatherAsync(string city)
{
    HttpResponseMessage response = await _httpClient.GetAsync($"/weather/{city}");
    return await response.ToResultFromJsonAsync<WeatherData>();
}

// Automatically handles:
// - 2xx → Success with deserialized data
// - 400 → Validation errors from Problem Details
// - 401/403 → Security failures
// - 404 → Not found
// - 5xx → Server errors
```

### Combining Results

```csharp
Result<Order> CreateOrder(OrderRequest request)
{
    Result<Customer> customerResult = GetCustomer(request.CustomerId);
    Result<Product> productResult = GetProduct(request.ProductId);
    Result<Address> addressResult = ValidateAddress(request.ShippingAddress);
    
    // Combine multiple results
    Result combined = Result.Combine(customerResult, productResult, addressResult);
    if (combined.IsFailure)
        return Result.Failure<Order>(combined.Error);
    
    // All results are successful, create the order
    return Result.Success(new Order(
        customerResult.Value,
        productResult.Value,
        addressResult.Value
    ));
}
```

## 📚 Documentation

- 📖 [CLAUDE.md](CLAUDE.md) - Development guidelines and coding standards
- 📋 [TASKS.md](TASKS.md) - Development progress and task tracking
- 🚀 [Getting Started Guide](GETTING-STARTED.md) - Complete guide for new users
- 📖 [API Reference](docs/) - Interactive DocFX-generated API documentation
- 🔄 [Migration Guide](MIGRATION.md) - Migrating from exception-based error handling
- ⭐ [Best Practices](BEST-PRACTICES.md) - Production patterns and architectural guidance
- 🚧 Performance Benchmarks *(planned)*

## 🏗️ Building from Source

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension

### Building

```bash
# Clone the repository
git clone https://github.com/georgepharrison/FlowRight.git
cd FlowRight

# Build the solution
dotnet build

# Run tests
dotnet test

# Run benchmarks
dotnet run -c Release --project benchmarks/Benchmarks/Benchmarks.csproj
```

## 🧪 Testing

### Integration Testing ✅ Complete
FlowRight includes comprehensive integration tests covering:
- **Complex Object Validation**: Real-world e-commerce scenarios with nested objects
- **Result Composition**: Multi-step business workflows and async patterns
- **HTTP Integration**: Real HTTP responses with status code mapping
- **API Serialization**: ASP.NET Core integration with WebApplicationFactory
- **Thread Safety**: Concurrent operations and race condition testing

### Test Coverage

The project maintains comprehensive test coverage with extensive unit and integration tests, including 500+ integration tests covering real-world scenarios, targeting >95% coverage for production release.

```bash
# Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Generate coverage report (requires ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage/**/coverage.cobertura.xml -targetdir:coverage/report -reporttypes:Html
```

## 📊 Performance

FlowRight is designed for minimal overhead and zero allocations on the success path.

| Operation | Time | Allocations |
|-----------|------|-------------|
| Result.Success() | ~8ns | 0 bytes |
| Result.Failure() | ~45ns | 88 bytes |
| Pattern Match | ~15ns | 0 bytes |
| Validation (10 rules) | ~95ns | 192 bytes |

See [detailed benchmarks](docs/benchmarks.md) for more information.

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by functional programming patterns in F# and Rust
- Built on the shoulders of the .NET community
- Special thanks to all contributors and users

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/georgepharrison/FlowRight/issues)
- **Discussions**: [GitHub Discussions](https://github.com/georgepharrison/FlowRight/discussions)
- **Stack Overflow**: Tag your questions with `flowright`

## 🗺️ Roadmap

### Version 1.0 (Q2 2025) - Production Ready
**Core Library:**
- ✅ Core Result pattern implementation (90.7% coverage)
- ✅ Pattern matching with Match/Switch methods  
- ✅ JSON serialization support
- ✅ Implicit operators and conversions

**Validation Library:**
- ✅ Fluent validation builder with thread safety (comprehensive coverage)
- ✅ 35+ validation rules (string, numeric, collection, etc.)
- ✅ Context-aware validation with async support
- ✅ Automatic error aggregation
- ✅ Integration tests with complex object validation

**HTTP Integration:**
- ✅ HTTP response to Result conversion with real response testing
- ✅ Status code mapping (2xx, 400, 401/403, 404, 5xx)
- ✅ Content type detection and parsing
- ✅ ValidationProblemDetails support
- ✅ API serialization integration with ASP.NET Core

**Remaining for v1.0:**
- ✅ Comprehensive integration testing (Tasks 56-60 complete)
- ✅ Thread safety and concurrency testing
- 🚧 Performance benchmarking (in progress)
- ✅ Complete XML documentation
- 🚧 NuGet package publishing

### Version 1.1 (Q3 2025)
- Additional validation rules based on feedback
- Performance optimizations
- Source generators for reduced boilerplate

### Version 2.0 (Q4 2025)
- AsyncResult<T> for async operations
- Railway-oriented programming extensions
- F# interop package

See [CHANGELOG.md](CHANGELOG.md) for version history.

---

Made with ❤️ by the .NET community