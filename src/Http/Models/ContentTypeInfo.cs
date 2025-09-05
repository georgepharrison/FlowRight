using System.Collections.ObjectModel;
using System.Net.Mime;

namespace FlowRight.Http.Models;

/// <summary>
/// Represents parsed content type information including media type, charset, and parameters.
/// </summary>
public sealed class ContentTypeInfo
{
    /// <summary>
    /// Gets the media type (e.g., "application/json").
    /// </summary>
    public string MediaType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the charset parameter (e.g., "utf-8").
    /// </summary>
    public string? Charset { get; init; }

    /// <summary>
    /// Gets additional content type parameters.
    /// </summary>
    public ReadOnlyDictionary<string, string> Parameters { get; init; } = new(new Dictionary<string, string>());

    /// <summary>
    /// Creates a ContentTypeInfo instance from a ContentType object.
    /// </summary>
    /// <param name="contentType">The ContentType to parse.</param>
    /// <returns>A ContentTypeInfo instance with parsed information.</returns>
    public static ContentTypeInfo FromContentType(ContentType? contentType)
    {
        if (contentType is null)
        {
            return new ContentTypeInfo();
        }

        Dictionary<string, string> parameters = [];
        
        foreach (string key in contentType.Parameters.Keys)
        {
            string? value = contentType.Parameters[key];
            if (value is not null)
            {
                parameters[key] = value;
            }
        }

        return new ContentTypeInfo
        {
            MediaType = contentType.MediaType ?? string.Empty,
            Charset = contentType.CharSet,
            Parameters = new ReadOnlyDictionary<string, string>(parameters)
        };
    }

    /// <summary>
    /// Determines if this content type matches another media type.
    /// </summary>
    /// <param name="mediaType">The media type to compare against.</param>
    /// <returns>True if the media types match (ignoring parameters).</returns>
    public bool IsMediaType(string mediaType)
    {
        if (string.Equals(MediaType, mediaType, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        // Also check if the base media types match (ignoring parameters)
        string thisBaseType = MediaType?.Split(';')[0]?.Trim() ?? string.Empty;
        string otherBaseType = mediaType?.Split(';')[0]?.Trim() ?? string.Empty;
        
        return string.Equals(thisBaseType, otherBaseType, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if this content type is a JSON variant.
    /// </summary>
    /// <returns>True if the media type represents JSON content.</returns>
    public bool IsJson() =>
        IsMediaType(MediaTypeNames.Application.Json) ||
        MediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines if this content type is an XML variant.
    /// </summary>
    /// <returns>True if the media type represents XML content.</returns>
    public bool IsXml() =>
        IsMediaType(MediaTypeNames.Application.Xml) ||
        IsMediaType(MediaTypeNames.Text.Xml) ||
        MediaType.EndsWith("+xml", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines if this content type represents text content.
    /// </summary>
    /// <returns>True if the media type represents text content.</returns>
    public bool IsText() =>
        MediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines if this content type represents binary content.
    /// </summary>
    /// <returns>True if the media type represents binary content.</returns>
    public bool IsBinary() =>
        !IsText() && !IsJson() && !IsXml() &&
        (MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
         MediaType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) ||
         MediaType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) ||
         IsMediaType(MediaTypeNames.Application.Octet) ||
         IsMediaType(MediaTypeNames.Application.Pdf));

    /// <summary>
    /// Determines if this content type represents form data.
    /// </summary>
    /// <returns>True if the media type represents form data.</returns>
    public bool IsFormData() =>
        IsMediaType("application/x-www-form-urlencoded") ||
        IsMediaType("multipart/form-data");
}