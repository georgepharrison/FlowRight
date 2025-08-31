namespace FlowRight.Core.Results;

/// <summary>
/// Specifies the general category or severity level of a result, providing high-level classification
/// for success states and various types of informational or error conditions.
/// </summary>
/// <remarks>
/// <para>
/// This enumeration provides a way to categorize results beyond simple success/failure,
/// allowing for more nuanced result handling in scenarios where different types of
/// non-failure outcomes need to be distinguished.
/// </para>
/// <para>
/// The values are ordered by severity, with <see cref="Success"/> representing the most
/// positive outcome and <see cref="Error"/> representing the most severe negative outcome.
/// </para>
/// </remarks>
public enum ResultType
{
    /// <summary>
    /// Indicates that the operation completed successfully without any issues.
    /// This is the most positive result state and typically contains a success value.
    /// </summary>
    Success,
    
    /// <summary>
    /// Indicates that the operation completed successfully and provides additional
    /// informational content that may be useful to the caller or end user.
    /// </summary>
    /// <remarks>
    /// This result type is useful for operations that succeed but want to communicate
    /// additional context, such as warnings about deprecated features, informational
    /// messages about data transformations, or notifications about side effects.
    /// </remarks>
    Information,
    
    /// <summary>
    /// Indicates that the operation completed successfully but encountered conditions
    /// that warrant attention or may lead to issues in the future.
    /// </summary>
    /// <remarks>
    /// Warning results are successful but highlight potential problems, configuration
    /// issues, or situations that should be addressed. For example, using deprecated
    /// APIs, approaching resource limits, or data quality concerns.
    /// </remarks>
    Warning,
    
    /// <summary>
    /// Indicates that the operation failed to complete successfully due to an error condition.
    /// This represents a failure state and typically contains error information instead of a success value.
    /// </summary>
    /// <remarks>
    /// Error results represent various types of failures including system errors, validation
    /// failures, security violations, and business rule violations. The specific type of
    /// error can be determined using <see cref="ResultFailureType"/>.
    /// </remarks>
    Error
}