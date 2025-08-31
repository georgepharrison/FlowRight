using System.Text.Json.Serialization;

namespace FlowRight.Http.Models;

internal sealed class ValidationProblemResponse
{
    #region Public Properties

    [JsonPropertyName("errors")]
    public Dictionary<string, string[]> Errors { get; set; } = [];

    #endregion Public Properties
}