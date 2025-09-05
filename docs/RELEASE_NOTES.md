# FlowRight Release Notes

## Version 1.0.0-preview.1

### Initial Preview Release

This is the initial preview release of FlowRight, a production-grade Result pattern library for .NET.

### Features

#### FlowRight.Core
- **Result Pattern Implementation**: Complete Result and Result<T> with immutable design
- **Pattern Matching**: Match() and Switch() methods for functional and imperative styles
- **JSON Serialization**: Full System.Text.Json support with custom converters
- **Error Aggregation**: Combine() method for collecting multiple errors
- **Implicit Conversions**: Seamless conversion from values and errors

#### FlowRight.Validation
- **Fluent Validation Builder**: Intuitive ValidationBuilder<T> with method chaining
- **Property Validators**: Support for strings, numbers, collections, and custom types
- **Built-in Rules**: Comprehensive validation rules (length, range, email, regex, etc.)
- **Conditional Validation**: When() and Unless() for complex validation scenarios
- **Result Integration**: Direct integration with Result<T> for seamless error handling

#### FlowRight.Http
- **HTTP Extensions**: ToResultAsync() and ToResultFromJsonAsync() extension methods
- **Status Code Mapping**: Automatic mapping of HTTP status codes to Result types
- **Validation Problem Support**: Built-in handling of ValidationProblemDetails
- **AOT Compatible**: Support for JsonTypeInfo for Native AOT scenarios

### Technical Details

- **Target Frameworks**: .NET 6.0, .NET 8.0, .NET 9.0
- **Dependencies**: Zero external dependencies (Core package)
- **Performance**: Optimized for minimal allocations on success path
- **Thread Safety**: All types are thread-safe for read operations

### Breaking Changes
- None (initial release)

### Known Issues
- None at this time

### Migration Guide
- See README.md for migration from exception-based error handling

---

## Release Notes Template for Future Versions

### Version X.X.X

#### New Features
- Feature descriptions with links to issues/PRs

#### Improvements
- Enhancement descriptions

#### Bug Fixes
- Bug fix descriptions with issue references

#### Breaking Changes
- Any breaking changes with migration guidance

#### Performance
- Performance improvements and benchmarks

#### Dependencies
- Dependency updates and changes

---

## Versioning Strategy

FlowRight follows [Semantic Versioning](https://semver.org/):

- **Major** (X.0.0): Breaking changes
- **Minor** (1.X.0): New features, backward compatible
- **Patch** (1.1.X): Bug fixes, backward compatible
- **Preview** (1.0.0-preview.X): Pre-release versions

## Support

- **Issues**: [GitHub Issues](https://github.com/georgepharrison/FlowRight/issues)
- **Discussions**: [GitHub Discussions](https://github.com/georgepharrison/FlowRight/discussions)
- **Documentation**: [Project README](https://github.com/georgepharrison/FlowRight)