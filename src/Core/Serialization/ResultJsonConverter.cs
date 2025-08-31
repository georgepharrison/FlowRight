using System.Text.Json;
using System.Text.Json.Serialization;
using FlowRight.Core.Results;

namespace FlowRight.Core.Serialization;

/// <summary>
/// Provides custom JSON serialization and deserialization for <see cref="Result"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// This converter ensures proper serialization of Result objects while maintaining immutability
/// and supporting all failure types including validation failures, security exceptions, and
/// operation cancellations. The converter handles both serialization and deserialization
/// in a way that preserves the exact state of Result objects.
/// </para>
/// <para>
/// The converter serializes Result objects as JSON objects with the following structure:
/// <code>
/// {
///   "error": "string",
///   "failures": { "field": ["error1", "error2"] },
///   "failureType": "None|Error|Security|Validation|OperationCanceled",
///   "resultType": "Success|Information|Warning|Error"
/// }
/// </code>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Register the converter globally
/// JsonSerializerOptions options = new()
/// {
///     Converters = { new ResultJsonConverter() }
/// };
/// 
/// Result result = Result.Success();
/// string json = JsonSerializer.Serialize(result, options);
/// Result deserialized = JsonSerializer.Deserialize&lt;Result&gt;(json, options);
/// </code>
/// </example>
public sealed class ResultJsonConverter : JsonConverter<Result>
{
    /// <summary>
    /// Gets a value indicating whether this converter can convert the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check for conversion capability.</param>
    /// <returns><see langword="true"/> if the type is <see cref="Result"/> or can be handled by this converter; otherwise, <see langword="false"/>.</returns>
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert == typeof(Result);

    /// <summary>
    /// Deserializes a JSON object into a <see cref="Result"/> instance.
    /// </summary>
    /// <param name="reader">The JSON reader positioned at the start of the Result object.</param>
    /// <param name="typeToConvert">The type to convert (should be <see cref="Result"/>).</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A <see cref="Result"/> instance representing the deserialized JSON data.</returns>
    /// <exception cref="JsonException">Thrown when the JSON structure is invalid or contains unexpected data.</exception>
    /// <remarks>
    /// <para>
    /// This method reconstructs Result objects from JSON by reading the serialized properties
    /// and using the appropriate static factory methods to create immutable Result instances.
    /// The method preserves the exact failure type and error information from the original object.
    /// </para>
    /// <para>
    /// The deserialization process handles all Result states:
    /// <list type="bullet">
    /// <item><description>Success states with appropriate ResultType</description></item>
    /// <item><description>General error failures</description></item>
    /// <item><description>Security failures</description></item>
    /// <item><description>Validation failures with field-specific errors</description></item>
    /// <item><description>Operation cancellation failures</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        string error = string.Empty;
        Dictionary<string, string[]> failures = [];
        ResultFailureType failureType = ResultFailureType.None;
        ResultType resultType = ResultType.Success;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            string? propertyName = reader.GetString();
            reader.Read();

            switch (propertyName?.ToLowerInvariant())
            {
                case "error":
                    error = reader.GetString() ?? string.Empty;
                    break;

                case "failures":
                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        failures = JsonSerializer.Deserialize<Dictionary<string, string[]>>(ref reader, options) ?? [];
                    }
                    break;

                case "failuretype":
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        string? failureTypeString = reader.GetString();
                        if (Enum.TryParse<ResultFailureType>(failureTypeString, true, out ResultFailureType parsedFailureType))
                        {
                            failureType = parsedFailureType;
                        }
                    }
                    else if (reader.TokenType == JsonTokenType.Number)
                    {
                        failureType = (ResultFailureType)reader.GetInt32();
                    }
                    break;

                case "resulttype":
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        string? resultTypeString = reader.GetString();
                        if (Enum.TryParse<ResultType>(resultTypeString, true, out ResultType parsedResultType))
                        {
                            resultType = parsedResultType;
                        }
                    }
                    else if (reader.TokenType == JsonTokenType.Number)
                    {
                        resultType = (ResultType)reader.GetInt32();
                    }
                    break;
            }
        }

        // Reconstruct the Result based on the deserialized state
        if (string.IsNullOrEmpty(error))
        {
            // Success case
            return Result.Success(resultType);
        }

        // Failure cases - use appropriate factory methods based on failure type
        return failureType switch
        {
            ResultFailureType.Validation when failures.Count > 0 => Result.ValidationFailure(failures),
            ResultFailureType.Security => Result.Failure(new System.Security.SecurityException(error)),
            ResultFailureType.OperationCanceled => Result.Failure(new OperationCanceledException(error)),
            _ => Result.Failure(error, resultType, failureType)
        };
    }

    /// <summary>
    /// Serializes a <see cref="Result"/> instance to JSON.
    /// </summary>
    /// <param name="writer">The JSON writer to write the serialized data to.</param>
    /// <param name="value">The <see cref="Result"/> instance to serialize.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <remarks>
    /// <para>
    /// This method serializes Result objects to JSON by writing their properties as a JSON object.
    /// The serialization preserves all error information, failure types, and result states
    /// to enable complete round-trip serialization.
    /// </para>
    /// <para>
    /// The method writes the following JSON structure:
    /// <list type="bullet">
    /// <item><description><c>error</c>: The error message string</description></item>
    /// <item><description><c>failures</c>: Dictionary of field-specific validation errors</description></item>
    /// <item><description><c>failureType</c>: The specific failure type enumeration</description></item>
    /// <item><description><c>resultType</c>: The general result type enumeration</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartObject();

        // Write Error property
        writer.WriteString("error", value.Error);

        // Write Failures property
        writer.WritePropertyName("failures");
        JsonSerializer.Serialize(writer, value.Failures, options);

        // Write FailureType property
        writer.WriteString("failureType", value.FailureType.ToString());

        // Write ResultType property
        writer.WriteString("resultType", value.ResultType.ToString());

        writer.WriteEndObject();
    }
}