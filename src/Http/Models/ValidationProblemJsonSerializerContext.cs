using System.Text.Json.Serialization;

namespace FlowRight.Http.Models;

[JsonSerializable(typeof(ValidationProblemResponse))]
internal sealed partial class ValidationProblemJsonSerializerContext : JsonSerializerContext
{ }