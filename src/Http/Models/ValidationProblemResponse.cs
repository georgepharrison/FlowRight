using System.Text.Json.Serialization;

namespace FlowRight.Http.Models;

/// <summary>
/// Represents a validation problem response that contains field-specific error messages.
/// </summary>
/// <remarks>
/// This class is designed to be compatible with ASP.NET Core's ValidationProblemDetails
/// and provides serialization support for error responses from HTTP APIs.
/// </remarks>
public sealed class ValidationProblemResponse
{
    #region Public Properties

    /// <summary>
    /// Gets or sets the dictionary of validation errors keyed by field name.
    /// </summary>
    /// <value>
    /// A dictionary where keys are field names and values are arrays of error messages for that field.
    /// </value>
    /// <example>
    /// <code>
    /// ValidationProblemResponse response = new()
    /// {
    ///     Errors = new Dictionary&lt;string, string[]&gt;
    ///     {
    ///         { "Email", ["Email is required", "Email format is invalid"] },
    ///         { "Name", ["Name is required"] }
    ///     }
    /// };
    /// </code>
    /// </example>
    [JsonPropertyName("errors")]
    public Dictionary<string, string[]> Errors { get; set; } = [];

    #endregion Public Properties
}