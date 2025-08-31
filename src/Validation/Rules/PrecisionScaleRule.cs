using System.Numerics;

namespace FlowRight.Validation.Rules;

public sealed class PrecisionScaleRule<TNumeric>(int precision, int scale) : IRule<TNumeric>
    where TNumeric : struct, INumber<TNumeric>
{
    #region Private Members

    private readonly int _precision = precision;
    private readonly int _scale = scale;

    #endregion Private Members

    #region Public Methods

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