using System.Text.Json.Serialization;

namespace FlowRight.Http.Tests.Extensions;

/// <summary>
/// JsonSerializerContext for test models used in HttpResponseMessageExtensions tests.
/// </summary>
[JsonSerializable(typeof(TestModel))]
internal sealed partial class TestModelJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Test model for JSON serialization testing.
/// </summary>
internal sealed class TestModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}