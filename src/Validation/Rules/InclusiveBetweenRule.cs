using System.Numerics;

namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures an integer value is between two bounds (inclusive).
/// </summary>
/// <param name="from">The lower bound (inclusive).</param>
/// <param name="to">The upper bound (inclusive).</param>
public sealed class InclusiveBetweenRule(int from, int to) : IRule<int>
{
    #region Private Members

    private readonly int _from = from;
    private readonly int _to = to;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the integer value is between the bounds (inclusive).
    /// </summary>
    /// <param name="value">The integer value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not between the bounds; otherwise, null.</returns>
    public string? Validate(int value, string displayName) =>
        value < _from || value > _to ? $"{displayName} must be between {_from} and {_to} (inclusive)" : null;

    #endregion Public Methods
}

/// <summary>
/// A validation rule that ensures a numeric value is between two bounds (inclusive).
/// </summary>
/// <typeparam name="TNumeric">The numeric type being validated (int, long, decimal, double, float, short, etc.).</typeparam>
/// <param name="from">The lower bound (inclusive).</param>
/// <param name="to">The upper bound (inclusive).</param>
public sealed class InclusiveBetweenRule<TNumeric>(TNumeric from, TNumeric to) : IRule<TNumeric>
    where TNumeric : struct, INumber<TNumeric>
{
    #region Private Members

    private readonly TNumeric _from = from;
    private readonly TNumeric _to = to;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the numeric value is between the bounds (inclusive).
    /// </summary>
    /// <param name="value">The numeric value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not between the bounds; otherwise, null.</returns>
    /// <example>
    /// <code>
    /// // Integer validation
    /// InclusiveBetweenRule&lt;int&gt; intRule = new(1, 10);
    /// string? error = intRule.Validate(5, "Age"); // Returns null (valid)
    /// string? error2 = intRule.Validate(15, "Age"); // Returns error message
    /// 
    /// // Decimal validation
    /// InclusiveBetweenRule&lt;decimal&gt; decimalRule = new(0.0m, 100.0m);
    /// string? error3 = decimalRule.Validate(50.5m, "Percentage"); // Returns null (valid)
    /// </code>
    /// </example>
    public string? Validate(TNumeric value, string displayName) =>
        value < _from || value > _to 
            ? $"{displayName} must be between {_from} and {_to} (inclusive)" 
            : null;

    #endregion Public Methods
}
