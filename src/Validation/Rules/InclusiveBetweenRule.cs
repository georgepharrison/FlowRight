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