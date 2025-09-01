# FlowRight API Documentation

Welcome to the FlowRight API documentation. FlowRight is a production-grade Result pattern library for .NET that provides explicit error handling without exceptions.

## What is FlowRight?

FlowRight transforms traditional exception-based error handling into explicit, type-safe result patterns. Instead of throwing exceptions for business logic errors, FlowRight returns `Result<T>` objects that explicitly represent success or failure states.

## Key Features

- **Zero Exception Policy**: Never throw exceptions for control flow
- **Immutable Results**: Thread-safe, immutable result types
- **Rich Validation**: Fluent validation builders with comprehensive rules
- **HTTP Integration**: Seamless conversion between HTTP responses and Results
- **JSON Serialization**: Full support for System.Text.Json
- **Performance**: Minimal allocations and high-performance operations

## Getting Started

Choose your area of interest:

### [📚 Articles](articles/index.md)
Comprehensive guides, tutorials, and best practices for using FlowRight effectively.

### [🔍 API Reference](api/index.md)
Complete API documentation for all FlowRight libraries.

## Libraries

### FlowRight.Core
The foundational Result pattern implementation with core types like `Result<T>`, `Success<T>`, and `Failure`.

### FlowRight.Validation  
Fluent validation builders that integrate seamlessly with the Result pattern.

### FlowRight.Http
HTTP response to Result pattern conversion utilities for web applications.

## Quick Example

```csharp
using FlowRight.Core;
using FlowRight.Validation;

// Create a validation builder
ValidationBuilder<User> validator = new ValidationBuilder<User>()
    .RuleFor(u => u.Email)
        .NotEmpty()
        .Email()
    .RuleFor(u => u.Age)
        .GreaterThan(0)
        .LessThan(150);

// Validate and get a Result
Result<User> result = validator.Validate(user, nameof(user));

// Handle the result
return result.Match(
    success: user => Ok(user),
    failure: errors => BadRequest(errors)
);
```

## Contributing

FlowRight is open source. Visit our [GitHub repository](https://github.com/georgepharrison/FlowRight) to contribute.