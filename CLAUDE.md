# CLAUDE.md

This file provides guidance to Claude (Claude Code and Claude Chat) when working with code in this repository.

## Project Overview

FlowRight is a production-grade Result pattern library for .NET that provides explicit error handling without exceptions, featuring comprehensive validation and HTTP integration capabilities.

## Standard Development Workflow

**All tasks must follow this structured approach:**

1. **Analyze Repository**: Examine existing code structure, patterns, and organization
2. **Write Tests First**: Create failing tests that define expected behavior (TDD)
3. **Implement Code**: Write minimal code to make tests pass
4. **Refactor**: Improve code quality while keeping tests green
5. **Document**: Add XML documentation to all public members
6. **Mark Complete**: Update `TASKS.md` with `[x]` when done

### Repository Analysis Phase
Before implementing any task, analyze:
- Current project structure and file organization
- Existing namespaces and naming conventions
- Code patterns already established
- Test structure and conventions
- Dependencies and references between projects

## Coding Standards

### Core Principles
- **SOLID Principles**: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **Immutability**: Result types are immutable with init-only properties
- **Type Safety**: Strong typing throughout, no dynamic types
- **Zero Exceptions**: Never throw exceptions for control flow

### C# Language Standards

- **NO `var` keyword**: Always use explicit types for clarity
  ```csharp
  // ❌ Bad
  var result = GetResult();
  var builder = new ValidationBuilder<User>();
  
  // ✅ Good
  Result<User> result = GetResult();
  ValidationBuilder<User> builder = new();
  ```

- **Target-typed `new()` expressions**: When type is obvious from declaration
  ```csharp
  // ✅ Good
  Result<string> result = new();
  Dictionary<string, string[]> errors = new();
  List<IRule<T>> rules = new();
  ```

- **Collection expressions `[]`**: For collection initialization
  ```csharp
  // ❌ Bad
  List<string> errors = new List<string>();
  IEnumerable<int> numbers = new int[] { 1, 2, 3 };
  
  // ✅ Good
  List<string> errors = [];
  IEnumerable<int> numbers = [1, 2, 3];
  ```

- **File-scoped namespaces**: Single namespace per file
  ```csharp
  // ✅ Good
  namespace FlowRight.Core.Results;
  
  public sealed class Result
  {
      // Implementation
  }
  ```

- **Sealed classes**: Mark classes as sealed when not designed for inheritance
  ```csharp
  // ✅ Good
  public sealed class ValidationBuilder<T>
  public sealed class NotEmptyRule<T> : IRule<T>
  ```

- **Expression-bodied members**: For single-line implementations
  ```csharp
  // ✅ Good
  public bool IsSuccess => string.IsNullOrEmpty(Error);
  
  public string GetDisplayName() =>
      $"{FirstName} {LastName}";
  ```

- **Private field naming**: Use underscore prefix
  ```csharp
  private readonly Dictionary<string, List<string>> _errors = [];
  private readonly List<IRule<T>> _rules = [];
  ```

- **Nullable reference types**: Use nullable annotations consistently
  ```csharp
  // ✅ Good
  public string? Validate(T value, string displayName);
  public Result<T?> GetValueOrDefault(T? defaultValue = default);
  ```

### Documentation Standards

- **XML documentation on ALL public members**
  ```csharp
  /// <summary>
  /// Creates a success result with the specified value.
  /// </summary>
  /// <typeparam name="T">The type of the success value.</typeparam>
  /// <param name="value">The success value.</param>
  /// <returns>A success result containing the value.</returns>
  /// <example>
  /// <code>
  /// Result<int> result = Result.Success(42);
  /// </code>
  /// </example>
  public static Result<T> Success<T>(T value)
  ```

### Testing Standards

- **Arrange-Act-Assert pattern**: Clear test structure
- **One assertion per test**: Focused test scenarios
- **Test naming**: `MethodName_StateUnderTest_ExpectedBehavior`
- **Shouldly**: Use for readable assertions
- **Test data builders**: For complex test setup

### Package Standards

- **Semantic versioning**: Major.Minor.Patch
- **No external dependencies**: Core package must be dependency-free
- **Deterministic builds**: Reproducible package creation
- **Source Link enabled**: For debugging support
- **Symbol packages**: Always generate .snupkg files

## Architecture Guidelines

### Result Pattern Implementation
- Results are immutable once created
- Never throw exceptions from Result methods
- Support full JSON serialization/deserialization
- Provide both functional (Match) and imperative (Switch) APIs

### Validation Architecture
- Fluent API for intuitive usage
- Composable validation rules
- Automatic error aggregation
- Integration with Result<T> pattern

### Performance Considerations
- Zero allocations on success path
- Minimize string allocations for errors
- Use structs for small value types
- Cache common error messages

## Git Commit Standards

Use conventional commits:
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation changes
- `test:` Test additions or changes
- `refactor:` Code refactoring
- `perf:` Performance improvements
- `chore:` Build process or auxiliary tool changes

## Review Checklist

Before marking a task complete:
- [ ] Solution builds successfully (`dotnet build` from root)
- [ ] All tests pass
- [ ] Code coverage >95%
- [ ] No compiler warnings
- [ ] XML documentation complete
- [ ] Follows coding standards
- [ ] Performance benchmarks pass
- [ ] Integration tests pass