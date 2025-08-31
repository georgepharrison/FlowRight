using System.Text.Json;
using System.Text.Json.Serialization;
using FlowRight.Core.Results;

namespace FlowRight.Core.Serialization;

/// <summary>
/// Provides custom JSON serialization and deserialization for <see cref="Result{T}"/> instances.
/// </summary>
/// <typeparam name="T">The type of the success value that the result can contain.</typeparam>
/// <remarks>
/// <para>
/// This converter ensures proper serialization of Result&lt;T&gt; objects while maintaining immutability
/// and supporting all failure types including validation failures, security exceptions, and
/// operation cancellations. The converter handles both serialization and deserialization
/// in a way that preserves the exact state of Result&lt;T&gt; objects including their success values.
/// </para>
/// <para>
/// The converter serializes Result&lt;T&gt; objects as JSON objects with the following structure:
/// <code>
/// {
///   "value": T,
///   "error": "string",
///   "failures": { "field": ["error1", "error2"] },
///   "failureType": "None|Error|Security|Validation|OperationCanceled",
///   "resultType": "Success|Information|Warning|Error"
/// }
/// </code>
/// </para>
/// <para>
/// For successful results, the "value" property contains the success value. For failed results,
/// the "value" property is omitted from the JSON output.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Register the converter globally
/// JsonSerializerOptions options = new()
/// {
///     Converters = { new ResultTJsonConverter&lt;User&gt;() }
/// };
/// 
/// Result&lt;User&gt; result = Result.Success(user);
/// string json = JsonSerializer.Serialize(result, options);
/// Result&lt;User&gt; deserialized = JsonSerializer.Deserialize&lt;Result&lt;User&gt;&gt;(json, options);
/// </code>
/// </example>
public sealed class ResultTJsonConverter<T> : JsonConverter<Result<T>>
{
    /// <summary>
    /// Gets a value indicating whether this converter can convert the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check for conversion capability.</param>
    /// <returns><see langword="true"/> if the type is <see cref="Result{T}"/> of the matching generic type; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This converter only handles Result&lt;T&gt; where T matches the generic type parameter
    /// specified when the converter was instantiated. Each generic Result type requires
    /// its own converter instance.
    /// </remarks>
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert == typeof(Result<T>);

    /// <summary>
    /// Deserializes a JSON object into a <see cref="Result{T}"/> instance.
    /// </summary>
    /// <param name="reader">The JSON reader positioned at the start of the Result&lt;T&gt; object.</param>
    /// <param name="typeToConvert">The type to convert (should be <see cref="Result{T}"/>).</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A <see cref="Result{T}"/> instance representing the deserialized JSON data.</returns>
    /// <exception cref="JsonException">Thrown when the JSON structure is invalid or contains unexpected data.</exception>
    /// <remarks>
    /// <para>
    /// This method reconstructs Result&lt;T&gt; objects from JSON by reading the serialized properties
    /// and using the appropriate static factory methods to create immutable Result&lt;T&gt; instances.
    /// The method preserves the exact failure type, error information, and success value from the original object.
    /// </para>
    /// <para>
    /// The deserialization process handles all Result&lt;T&gt; states:
    /// <list type="bullet">
    /// <item><description>Success states with typed values and appropriate ResultType</description></item>
    /// <item><description>General error failures</description></item>
    /// <item><description>Security failures</description></item>
    /// <item><description>Validation failures with field-specific errors</description></item>
    /// <item><description>Operation cancellation failures</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When deserializing successful results, the "value" property is deserialized according to
    /// the type T. For failed results, the value is ignored even if present in the JSON.
    /// </para>
    /// </remarks>
    public override Result<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        T? value = default;
        string error = string.Empty;
        Dictionary<string, string[]> failures = [];
        ResultFailureType failureType = ResultFailureType.None;
        ResultType resultType = ResultType.Success;
        bool hasValue = false;

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
                case "value":
                    if (reader.TokenType != JsonTokenType.Null)
                    {
                        value = JsonSerializer.Deserialize<T>(ref reader, options);
                        hasValue = true;
                    }
                    break;

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

        // Reconstruct the Result<T> based on the deserialized state
        if (string.IsNullOrEmpty(error))
        {
            // Success case
            if (!hasValue)
            {
                // No value provided for a success result, treat as failure
                return Result.Failure<T>("No value provided for Result<T>");
            }

            return Result.Success(value!, resultType);
        }

        // Failure cases - use appropriate factory methods based on failure type
        return failureType switch
        {
            ResultFailureType.Validation when failures.Count > 0 => Result.ValidationFailure<T>(failures),
            ResultFailureType.Security => Result.Failure<T>(new System.Security.SecurityException(error)),
            ResultFailureType.OperationCanceled => Result.Failure<T>(new OperationCanceledException(error)),
            _ => Result.Failure<T>(error, resultType, failureType)
        };
    }

    /// <summary>
    /// Serializes a <see cref="Result{T}"/> instance to JSON.
    /// </summary>
    /// <param name="writer">The JSON writer to write the serialized data to.</param>
    /// <param name="value">The <see cref="Result{T}"/> instance to serialize.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <remarks>
    /// <para>
    /// This method serializes Result&lt;T&gt; objects to JSON by writing their properties as a JSON object.
    /// The serialization preserves all error information, failure types, result states, and success values
    /// to enable complete round-trip serialization.
    /// </para>
    /// <para>
    /// The method writes the following JSON structure:
    /// <list type="bullet">
    /// <item><description><c>value</c>: The success value of type T (only for successful results)</description></item>
    /// <item><description><c>error</c>: The error message string</description></item>
    /// <item><description><c>failures</c>: Dictionary of field-specific validation errors</description></item>
    /// <item><description><c>failureType</c>: The specific failure type enumeration</description></item>
    /// <item><description><c>resultType</c>: The general result type enumeration</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// For successful results, the "value" property is included with the success value serialized
    /// according to type T. For failed results, the "value" property is omitted entirely.
    /// </para>
    /// </remarks>
    public override void Write(Utf8JsonWriter writer, Result<T> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartObject();

        // Write Value property only for successful results
        if (value.IsSuccess && value.TryGetValue(out T? successValue))
        {
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, successValue, options);
        }

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