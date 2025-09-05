# FlowRight API Reference

This section contains the complete API reference for all FlowRight libraries.

## Libraries

### FlowRight.Core
The foundational Result pattern implementation with core types, extensions, and serialization support.

**Key Namespaces:**
- [`FlowRight.Core.Results`](FlowRight.Core.Results.yml) - Core Result types and patterns
- [`FlowRight.Core.Extensions`](FlowRight.Core.Extensions.yml) - Extension methods for Result composition
- [`FlowRight.Core.Serialization`](FlowRight.Core.Serialization.yml) - JSON serialization support

**Primary Types:**
- [`Result<T>`](FlowRight.Core.Results.Result-1.yml) - Generic result type with value
- [`Result`](FlowRight.Core.Results.Result.yml) - Non-generic result type  
- [`IResult<T>`](FlowRight.Core.Results.IResult-1.yml) - Generic result interface
- [`IResult`](FlowRight.Core.Results.IResult.yml) - Non-generic result interface
- [`ResultFailureType`](FlowRight.Core.Results.ResultFailureType.yml) - Enumeration of failure types

### FlowRight.Validation  
Fluent validation builders that integrate seamlessly with the Result pattern.

**Key Namespaces:**
- [`FlowRight.Validation.Builders`](FlowRight.Validation.Builders.yml) - Fluent validation builders
- [`FlowRight.Validation.Rules`](FlowRight.Validation.Rules.yml) - Comprehensive validation rules
- [`FlowRight.Validation.Validators`](FlowRight.Validation.Validators.yml) - Property-specific validators
- [`FlowRight.Validation.Context`](FlowRight.Validation.Context.yml) - Validation context and metadata

**Primary Types:**
- [`ValidationBuilder<T>`](FlowRight.Validation.Builders.ValidationBuilder-1.yml) - Main fluent validation API
- [`IRule<T>`](FlowRight.Validation.Rules.IRule-1.yml) - Base validation rule interface
- [`IValidationContext`](FlowRight.Validation.Context.IValidationContext.yml) - Validation execution context

### FlowRight.Http
HTTP response to Result pattern conversion utilities for web applications.

**Key Namespaces:**
- [`FlowRight.Http.Extensions`](FlowRight.Http.Extensions.yml) - HTTP response extension methods
- [`FlowRight.Http.Models`](FlowRight.Http.Models.yml) - HTTP-specific models and utilities

**Primary Types:**
- [`HttpResponseMessageExtensions`](FlowRight.Http.Extensions.HttpResponseMessageExtensions.yml) - Extension methods for HttpResponseMessage
- [`ValidationProblemResponse`](FlowRight.Http.Models.ValidationProblemResponse.yml) - Structured validation error responses
- [`ContentTypeInfo`](FlowRight.Http.Models.ContentTypeInfo.yml) - Content type detection utilities

## Quick Start

### Basic Result Usage
```csharp
using FlowRight.Core.Results;

// Create success result
Result<string> success = Result.Success("Hello World");

// Create failure result  
Result<string> failure = Result.Failure<string>("Something went wrong");

// Handle results with pattern matching
string output = result.Match(
    success: value => $"Success: {value}",
    failure: error => $"Error: {error}"
);
```

### Validation Example
```csharp
using FlowRight.Validation.Builders;

// Build fluent validation
ValidationBuilder<User> validator = new ValidationBuilder<User>()
    .RuleFor(x => x.Email, user.Email)
        .NotEmpty()
        .Email()
    .RuleFor(x => x.Age, user.Age)
        .GreaterThan(0)
        .LessThan(150);

// Validate and get result
Result<User> result = validator.Build(() => user);
```

### HTTP Integration
```csharp
using FlowRight.Http.Extensions;

// Convert HTTP response to Result
HttpResponseMessage response = await httpClient.GetAsync("/api/data");
Result<ApiData> result = await response.ToResultFromJsonAsync<ApiData>();
```

## Browse by Category

### Core Functionality
- [Result Types and Interfaces](FlowRight.Core.Results.yml)
- [Async Extensions and Composition](FlowRight.Core.Extensions.ResultAsyncExtensions.yml)
- [JSON Serialization](FlowRight.Core.Serialization.yml)

### Validation Rules
- [String Validation](FlowRight.Validation.Rules.yml#string-rules)
- [Numeric Validation](FlowRight.Validation.Rules.yml#numeric-rules)  
- [Collection Validation](FlowRight.Validation.Rules.yml#collection-rules)
- [Custom Validation](FlowRight.Validation.Rules.yml#custom-rules)

### HTTP Integration
- [Response Conversion](FlowRight.Http.Extensions.HttpResponseMessageExtensions.yml)
- [Error Response Models](FlowRight.Http.Models.yml)

---

*Generated API reference for FlowRight v1.0.0-preview.1*