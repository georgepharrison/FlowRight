using System.Numerics;

namespace FlowRight.Validation.Rules;

/// <summary>
/// A validation rule that ensures a numeric value meets precision and scale requirements.
/// </summary>
/// <typeparam name="TNumeric">The numeric type to validate.</typeparam>
/// <param name="precision">The maximum total number of digits.</param>
/// <param name="scale">The maximum number of decimal places.</param>
public sealed class PrecisionScaleRule<TNumeric>(int precision, int scale) : IRule<TNumeric>
    where TNumeric : struct, INumber<TNumeric>
{
    #region Private Members

    private readonly int _precision = precision;
    private readonly int _scale = scale;

    #endregion Private Members

    #region Public Methods

    /// <summary>
    /// Validates that the numeric value meets the precision and scale requirements.
    /// </summary>
    /// <param name="value">The numeric value to validate.</param>
    /// <param name="displayName">The display name for the property being validated.</param>
    /// <returns>An error message if the value exceeds precision or scale limits; otherwise, null.</returns>
    public string? Validate(TNumeric value, string displayName)
    {
        string valueString = value.ToString() ?? string.Empty;
        string[] parts = valueString.Split('.');

        int totalDigits = parts[0].Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).Length + (parts.Length > 1 ? parts[1].Length : 0);
        int scaleDigits = parts.Length > 1 ? parts[1].Length : 0;

        if (totalDigits > _precision)
        {
            return $"{displayName} must not have more than {_precision} digits in total";
        }

        if (scaleDigits > _scale)
        {
            return $"{displayName} must not have more than {_scale} decimal places";
        }

        return null;
    }

    #endregion Public Methods
}