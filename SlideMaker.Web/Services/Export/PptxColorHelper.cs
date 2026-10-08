using System.Text.RegularExpressions;

namespace SlideMaker.Web.Services.Export;

/// <summary>
/// PPTX shape fills only support solid RGB (no alpha, no gradients as a simple fill), while our
/// templates store CSS colors/gradients as free-form strings. This pulls the closest solid hex
/// out of whatever CSS value is given — the PPTX export is a "close visual match", not a
/// pixel-perfect reproduction of the HTML/preview rendering.
/// </summary>
public static partial class PptxColorHelper
{
    [GeneratedRegex(@"#([0-9a-fA-F]{6})")]
    private static partial Regex HexRegex();

    [GeneratedRegex(@"rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)")]
    private static partial Regex RgbRegex();

    public static string ResolveHex(string? cssColor, string fallbackHex)
    {
        if (string.IsNullOrWhiteSpace(cssColor))
        {
            return fallbackHex;
        }

        var hexMatch = HexRegex().Match(cssColor);
        if (hexMatch.Success)
        {
            return hexMatch.Groups[1].Value.ToUpperInvariant();
        }

        var rgbMatch = RgbRegex().Match(cssColor);
        if (rgbMatch.Success)
        {
            var r = int.Parse(rgbMatch.Groups[1].Value);
            var g = int.Parse(rgbMatch.Groups[2].Value);
            var b = int.Parse(rgbMatch.Groups[3].Value);
            return $"{r:X2}{g:X2}{b:X2}";
        }

        return fallbackHex;
    }
}
