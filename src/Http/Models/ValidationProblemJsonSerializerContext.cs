using System.Text.Json.Serialization;

namespace FlowRight.Http.Models;

/// <summary>
/// Provides JSON serialization context for <see cref="ValidationProblemResponse"/> to enable AOT compilation.
/// </summary>
/// <remarks>
/// This context can be used with System.Text.Json for source-generated serialization,
/// improving performance and enabling Native AOT compilation scenarios.
/// </remarks>
/// <example>
/// <code>
/// ValidationProblemResponse response = new() { Errors = new() { { "Field", ["Error"] } } };
/// string json = JsonSerializer.Serialize(response, ValidationProblemJsonSerializerContext.Default.ValidationProblemResponse);
/// </code>
/// </example>
[JsonSerializable(typeof(ValidationProblemResponse))]
public sealed partial class ValidationProblemJsonSerializerContext : JsonSerializerContext
{ }