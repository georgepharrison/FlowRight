namespace FlowRight.Cqrs.Http;

/// <summary>
/// Base interface for all CQRS requests (commands and queries) that can be executed via HTTP.
/// </summary>
public interface IRequest
{
    /// <summary>
    /// Gets the API endpoint path for this request.
    /// </summary>
    /// <returns>The relative URL path for the API endpoint.</returns>
    string GetApiEndpoint();
}