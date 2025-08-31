namespace FlowRight.Core.Results;

/// <summary>
/// Specifies the specific type of failure that occurred in a result operation,
/// providing detailed categorization for different error scenarios and enabling
/// appropriate error handling strategies.
/// </summary>
/// <remarks>
/// <para>
/// This enumeration allows consumers to distinguish between different categories
/// of failures and implement specific handling logic for each type. For example,
/// validation failures might be displayed to users differently than security failures.
/// </para>
/// <para>
/// The failure types are designed to align with common error scenarios in modern
/// applications, including validation, security, and operation lifecycle concerns.
/// </para>
/// </remarks>
public enum ResultFailureType
{
    /// <summary>
    /// Indicates no failure occurred. This value is used for successful results
    /// and represents the absence of any error condition.
    /// </summary>
    /// <remarks>
    /// This value should only be present in results where <see cref="IResult.IsSuccess"/>
    /// is <see langword="true"/>. It serves as the default value for successful operations.
    /// </remarks>
    None,

    /// <summary>
    /// Indicates a general error condition that doesn't fall into other specific categories.
    /// This represents system errors, unexpected exceptions, or business rule violations.
    /// </summary>
    /// <remarks>
    /// This is the default failure type for most error scenarios including:
    /// <list type="bullet">
    /// <item><description>System or infrastructure failures</description></item>
    /// <item><description>Unexpected exceptions</description></item>
    /// <item><description>Business logic violations</description></item>
    /// <item><description>General application errors</description></item>
    /// </list>
    /// </remarks>
    Error,

    /// <summary>
    /// Indicates a security-related failure such as authentication failures,
    /// authorization violations, or security policy breaches.
    /// </summary>
    /// <remarks>
    /// Security failures typically require special handling and may need to be:
    /// <list type="bullet">
    /// <item><description>Logged to security audit trails</description></item>
    /// <item><description>Reported to security monitoring systems</description></item>
    /// <item><description>Handled with specific error messages that don't reveal sensitive information</description></item>
    /// <item><description>Subject to rate limiting or account lockout policies</description></item>
    /// </list>
    /// </remarks>
    Security,

    /// <summary>
    /// Indicates that the operation failed due to validation errors in the input data,
    /// typically containing detailed field-level error information.
    /// </summary>
    /// <remarks>
    /// Validation failures are commonly encountered in user-facing applications and APIs.
    /// They typically:
    /// <list type="bullet">
    /// <item><description>Contain detailed field-specific error messages</description></item>
    /// <item><description>Can be displayed directly to users</description></item>
    /// <item><description>Should not be logged as system errors</description></item>
    /// <item><description>May include multiple validation errors for different fields</description></item>
    /// </list>
    /// The detailed validation errors are typically available through <see cref="IResult.Failures"/>.
    /// </remarks>
    Validation,

    /// <summary>
    /// Indicates that the operation was canceled before completion, typically due to
    /// cancellation tokens, timeouts, or explicit user cancellation requests.
    /// </summary>
    /// <remarks>
    /// Operation canceled failures represent a normal part of application flow control
    /// rather than actual errors. They may be handled differently from other failure types:
    /// <list type="bullet">
    /// <item><description>Often ignored in logging</description></item>
    /// <item><description>May not be displayed to users as errors</description></item>
    /// <item><description>Can be filtered out of error handling in some scenarios</description></item>
    /// <item><description>Support graceful shutdown and timeout scenarios</description></item>
    /// </list>
    /// </remarks>
    OperationCanceled
}