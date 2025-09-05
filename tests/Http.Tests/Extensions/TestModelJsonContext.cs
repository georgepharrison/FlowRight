using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace FlowRight.Http.Tests.Extensions;

/// <summary>
/// JsonSerializerContext for test models used in HttpResponseMessageExtensions tests.
/// </summary>
[JsonSerializable(typeof(TestModel))]
internal sealed partial class TestModelJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Test model for JSON and XML serialization testing.
/// </summary>
public sealed class TestModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Parameterless constructor required for XML deserialization
    public TestModel() { }
    
    public TestModel(int id, string name)
    {
        Id = id;
        Name = name;
    }
}