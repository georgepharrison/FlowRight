namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a numeric value is positive (greater than zero).
/// </summary>
/// <typeparam name="T">The type of value to validate.</typeparam>
public sealed class PositiveRule<T> : IRule<T>
{
    #region Public Methods

    /// <summary>
    /// Validates that the numeric value is positive (greater than zero).
    /// </summary>
    /// <param name="value">The numeric value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value is not positive; otherwise, null.</returns>
    public string? Validate(T value, string displayName)
    {
        if (value is null)
        {
            return null;
        }

        // Get the zero value for the type
        object? zero = GetZeroValue(typeof(T));
        if (zero is null)
        {
            return $"{displayName} must be positive.";
        }

        return Comparer<T>.Default.Compare(value, (T)zero) > 0
            ? null
            : $"{displayName} must be positive.";
    }

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// Gets the zero value for common numeric types.
    /// </summary>
    /// <param name="type">The type to get zero value for.</param>
    /// <returns>The zero value for the type.</returns>
    private static object? GetZeroValue(Type type)
    {
        Type nonNullableType = Nullable.GetUnderlyingType(type) ?? type;
        
        if (nonNullableType == typeof(int)) return 0;
        if (nonNullableType == typeof(long)) return 0L;
        if (nonNullableType == typeof(short)) return (short)0;
        if (nonNullableType == typeof(byte)) return (byte)0;
        if (nonNullableType == typeof(sbyte)) return (sbyte)0;
        if (nonNullableType == typeof(uint)) return 0U;
        if (nonNullableType == typeof(ulong)) return 0UL;
        if (nonNullableType == typeof(ushort)) return (ushort)0;
        if (nonNullableType == typeof(decimal)) return 0m;
        if (nonNullableType == typeof(double)) return 0.0;
        if (nonNullableType == typeof(float)) return 0.0f;
        
        return null;
    }

    #endregion Private Methods
}