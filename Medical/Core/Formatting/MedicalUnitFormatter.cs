using System.Text;
using System.Text.RegularExpressions;

namespace Xinglin.Medical.Formatting;

public static class MedicalUnitFormatter
{
    private static readonly Dictionary<string, string> KnownUnits = new(StringComparer.OrdinalIgnoreCase)
    {
        ["umol/L"] = "μmol/L",
        ["micromol/L"] = "μmol/L",
        ["ug"] = "μg",
        ["ug/L"] = "μg/L",
        ["10^6/L"] = "10⁶/L",
        ["10^9/L"] = "10⁹/L",
        ["10^12/L"] = "10¹²/L"
    };

    private static readonly Dictionary<char, char> Superscript = new()
    {
        ['0'] = '⁰', ['1'] = '¹', ['2'] = '²', ['3'] = '³', ['4'] = '⁴',
        ['5'] = '⁵', ['6'] = '⁶', ['7'] = '⁷', ['8'] = '⁸', ['9'] = '⁹',
        ['+'] = '⁺', ['-'] = '⁻'
    };

    public static string? FormatUnit(string? unit)
    {
        if (string.IsNullOrEmpty(unit))
            return unit;

        if (KnownUnits.TryGetValue(unit, out var known))
            return known;

        var formatted = Regex.Replace(unit, @"\^(\d+|[+-])", match => ToSuperscript(match.Groups[1].Value));
        return formatted
            .Replace("micro", "μ", StringComparison.OrdinalIgnoreCase)
            .Replace("mu", "μ", StringComparison.OrdinalIgnoreCase)
            .Replace("alpha", "α", StringComparison.OrdinalIgnoreCase)
            .Replace("beta", "β", StringComparison.OrdinalIgnoreCase)
            .Replace("gamma", "γ", StringComparison.OrdinalIgnoreCase)
            .Replace("delta", "δ", StringComparison.OrdinalIgnoreCase);
    }

    private static string ToSuperscript(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var ch in text)
            builder.Append(Superscript.GetValueOrDefault(ch, ch));
        return builder.ToString();
    }
}
