namespace FlowRight.Http.Tests.Extensions;

/// <summary>
/// Test model representing parsed content type information for testing content type handling.
/// This will be implemented in the actual extension methods.
/// </summary>
internal sealed class ContentTypeInfo
{
    public string MediaType { get; init; } = string.Empty;
    public string? Charset { get; init; }
    public Dictionary<string, string> Parameters { get; init; } = new();
}