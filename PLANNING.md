# Technical Planning Document: FlowRight

*Version 1.0 - Production NuGet Package Development*  
*Last Updated: January 2025*

## Architecture Overview

### Package Structure

```
FlowRight/
├── src/
│   ├── Core/
│   │   ├── Core.csproj
│   │   ├── Results/
│   │   │   ├── Result.cs
│   │   │   ├── Result{T}.cs
│   │   │   ├── IResult.cs
│   │   │   └── Enums/
│   │   ├── Extensions/
│   │   └── Serialization/
│   ├── Validation/
│   │   ├── Validation.csproj
│   │   ├── Builders/
│   │   ├── Validators/
│   │   └── Rules/
│   └── Http/
│       ├── Http.csproj
│       ├── Extensions/
│       └── Models/
├── tests/
│   ├── Core.Tests/
│   │   └── Core.Tests.csproj
│   ├── Validation.Tests/
│   │   └── Validation.Tests.csproj
│   ├── Http.Tests/
│   │   └── Http.Tests.csproj
│   └── Integration.Tests/
│       └── Integration.Tests.csproj
├── benchmarks/
│   └── Benchmarks/
│       └── Benchmarks.csproj
├── docs/
│   ├── getting-started.md
│   ├── migration-guide.md
│   └── api-reference/
└── samples/
    ├── WebApi.Sample/
    │   └── WebApi.Sample.csproj
    └── DomainModel.Sample/
        └── DomainModel.Sample.csproj
```

### Project Configuration Example

```xml
<!-- src/Core/Core.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;net6.0</TargetFrameworks>
    <AssemblyName>FlowRight.Core</AssemblyName>
    <RootNamespace>FlowRight.Core</RootNamespace>
    <PackageId>FlowRight.Core</PackageId>
    <!-- Other package properties -->
  </PropertyGroup>
</Project>

<!-- src/Validation/Validation.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;net6.0</TargetFrameworks>
    <AssemblyName>FlowRight.Validation</AssemblyName>
    <RootNamespace>FlowRight.Validation</RootNamespace>
    <PackageId>FlowRight.Validation</PackageId>
    <!-- Other package properties -->
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Core\Core.csproj" />
  </ItemGroup>
</Project>
```

---

## Technical Implementation

### Core Library Design

#### Result Implementation
```csharp
namespace FlowRight.Core.Results;

public partial class Result : IResult
{
    // Serialization-friendly private constructor
    [JsonConstructor]
    private Result() { }
    
    // Immutable properties with JsonInclude
    [JsonInclude]
    public string Error { get; private init; } = string.Empty;
    
    [JsonInclude]
    public IDictionary<string, string[]> Failures { get; private init; }
    
    [JsonInclude]
    public ResultType ResultType { get; private init; }
    
    [JsonInclude]
    public ResultFailureType FailureType { get; private init; }
    
    // Computed properties (not serialized)
    public bool IsSuccess => string.IsNullOrEmpty(Error);
    public bool IsFailure => !IsSuccess;
}
```

#### Serialization Strategy
```csharp
namespace FlowRight.Core.Serialization;

public sealed class ResultJsonConverter : JsonConverter<Result>
{
    public override Result Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        // Handle both success and failure deserialization
        // Never throw exceptions
        // Preserve all error information
    }
}
```

### Validation Library Design

#### Builder Architecture
```csharp
namespace FlowRight.Validation.Builders;

public class ValidationBuilder<T>
{
    private readonly Dictionary<string, List<string>> _errors = [];
    
    public Result<T> Build(Func<T> factory)
    {
        return HasErrors
            ? Result.Failure<T>(GetErrors())
            : Result.Success(factory());
    }
    
    // Fluent API for property validation
    public StringPropertyValidator<T> RuleFor(
        Expression<Func<T, string>> propertySelector,
        string value,
        string? displayName = null)
    {
        // Create type-specific validator
    }
}
```

#### Rule System
```csharp
namespace FlowRight.Validation.Rules;

public interface IRule<in T>
{
    string? Validate(T value, string displayName);
}

public sealed class NotEmptyRule<T> : IRule<T>
{
    public string? Validate(T value, string displayName) =>
        value switch
        {
            null => $"{displayName} must not be empty",
            string s when string.IsNullOrWhiteSpace(s) => 
                $"{displayName} must not be empty",
            IEnumerable e when !e.Cast<object>().Any() => 
                $"{displayName} must not be empty",
            _ => null
        };
}
```

### HTTP Library Design

#### Extension Methods
```csharp
namespace FlowRight.Http.Extensions;

public static class HttpResponseMessageExtensions
{
    public static async Task<Result<T?>> ToResultFromJsonAsync<T>(
        this HttpResponseMessage response,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
        {
            // Deserialize and return success
        }
        
        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => 
                await HandleBadRequestAsync<T>(response, cancellationToken),
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => 
                Result.Failure<T?>(new SecurityException("Unauthorized")),
            HttpStatusCode.NotFound => 
                Result.Failure<T?>("Not Found"),
            _ => Result.Failure<T?>(
                await GetErrorMessageAsync(response, cancellationToken))
        };
    }
}
```

---

## Testing Strategy

### Unit Testing

#### Core Tests
```csharp
public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        // Arrange & Act
        Result result = Result.Success();
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeEmpty();
    }
    
    [Fact]
    public void Failure_WithError_ShouldCreateFailureResult()
    {
        // Arrange
        string error = "Operation failed";
        
        // Act
        Result result = Result.Failure(error);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);
    }
}
```

#### Serialization Tests
```csharp
public class SerializationTests
{
    [Fact]
    public async Task Result_ShouldSerializeAndDeserialize()
    {
        // Arrange
        Result<int> original = Result.Success(42);
        
        // Act
        string json = JsonSerializer.Serialize(original);
        Result<int>? deserialized = JsonSerializer.Deserialize<Result<int>>(json);
        
        // Assert
        deserialized.ShouldNotBeNull();
        deserialized!.IsSuccess.ShouldBeTrue();
        deserialized.Match(
            onSuccess: value => value.ShouldBe(42),
            onFailure: _ => throw new XunitException("Should be success"));
    }
}
```

### Integration Testing

#### Validation Integration
```csharp
public class ValidationIntegrationTests
{
    [Fact]
    public void ValidationBuilder_WithMultipleRules_ShouldAggregateErrors()
    {
        // Arrange
        CreateUserRequest request = new("", "invalid-email", -5);
        
        // Act
        Result<User> result = new ValidationBuilder<User>()
            .RuleFor(x => x.Name, request.Name)
                .NotEmpty()
            .RuleFor(x => x.Email, request.Email)
                .EmailAddress()
            .RuleFor(x => x.Age, request.Age)
                .GreaterThan(0)
            .Build(() => new User(request.Name, request.Email, request.Age));
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Failures.Count.ShouldBe(3);
    }
}
```

### Performance Testing

#### Benchmarks
```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class ResultBenchmarks
{
    [Benchmark]
    public Result<int> CreateSuccess() => 
        Result.Success(42);
    
    [Benchmark]
    public Result<int> CreateFailure() => 
        Result.Failure<int>("Error");
    
    [Benchmark]
    public void PatternMatch()
    {
        Result<int> result = Result.Success(42);
        int value = result.Match(
            onSuccess: v => v * 2,
            onFailure: _ => 0);
    }
}
```

---

## Documentation Plan

### API Documentation
- XML comments on all public members
- Examples in XML documentation
- IntelliSense-friendly descriptions
- Nullable reference type annotations

### Getting Started Guide
1. Installation via NuGet
2. Basic Result usage
3. Validation builder introduction
4. HTTP integration examples
5. Best practices

### Migration Guide
1. From exception-based handling
2. From other Result libraries
3. Gradual adoption strategies
4. Common pitfalls

### Sample Projects
1. **WebApi.Sample**: REST API using FlowRight
2. **DomainModel.Sample**: DDD with Result pattern
3. **Validation.Sample**: Complex validation scenarios

---

## Build & Release Pipeline

### Build Configuration

#### Directory.Build.props
```xml
<Project>
  <PropertyGroup>
    <TargetFrameworks>net8.0;net6.0</TargetFrameworks>
    <LangVersion>12.0</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
  </PropertyGroup>
</Project>
```

#### Package Properties
```xml
<PropertyGroup>
  <Authors>Your Name</Authors>
  <Company>Your Company</Company>
  <Product>FlowRight</Product>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageProjectUrl>https://github.com/yourusername/FlowRight</PackageProjectUrl>
  <RepositoryUrl>https://github.com/yourusername/FlowRight</RepositoryUrl>
  <PackageTags>result;error-handling;validation;functional</PackageTags>
  <PackageReadmeFile>README.md</PackageReadmeFile>
  <PackageIcon>icon.png</PackageIcon>
  
  <!-- Build properties -->
  <Deterministic>true</Deterministic>
  <ContinuousIntegrationBuild Condition="'$(CI)' == 'true'">true</ContinuousIntegrationBuild>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>
```

### CI/CD Pipeline

#### GitHub Actions
```yaml
name: Build and Release

on:
  push:
    branches: [ main ]
    tags: [ 'v*' ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Test
      run: dotnet test --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage"
    
    - name: Pack
      if: startsWith(github.ref, 'refs/tags/v')
      run: dotnet pack --configuration Release --no-build --output ./artifacts
    
    - name: Push to NuGet
      if: startsWith(github.ref, 'refs/tags/v')
      run: dotnet nuget push ./artifacts/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json
```

---

## Quality Assurance

### Code Quality Tools
- **Analyzers**: Roslyn analyzers for code quality
- **Code Coverage**: Coverlet for coverage reports
- **Benchmarking**: BenchmarkDotNet for performance
- **Documentation**: DocFX for API documentation

### Testing Requirements
- Unit test coverage > 95%
- Integration test coverage > 80%
- Performance regression tests
- Serialization round-trip tests
- Thread safety verification

### Review Process
1. Automated PR checks
2. Code review requirements
3. Documentation review
4. API surface review
5. Breaking change detection

---

## Versioning Strategy

### Semantic Versioning
- **Major**: Breaking API changes
- **Minor**: New features, backward compatible
- **Patch**: Bug fixes, performance improvements

### Version Workflow
1. Development on `main` branch
2. Feature branches for new features
3. Release branches for stabilization
4. Tags for releases (v1.0.0)

### Breaking Change Policy
- Deprecation warnings one minor version before
- Migration guide for breaking changes
- Extended support for previous major version

---

## Performance Targets

### Benchmarks
- Result creation: < 10ns
- Pattern matching: < 20ns
- Serialization: < 1μs
- Validation (10 rules): < 100ns

### Memory Targets
- Zero allocations for success path
- Minimal allocations for errors
- String interning for common errors
- Struct-based internals where possible

---

## Security Considerations

### Package Security
- Signed assemblies
- Source Link enabled
- Deterministic builds
- Dependency scanning
- SBOM generation

### API Security
- No sensitive data in errors
- Sanitized error messages
- Safe deserialization
- No code execution paths