using System.Net;

namespace SlideMaker.Web.Services.Export;

/// <summary>
/// Builds the inline CSS for one rendered slide element. Shared by the HTML export
/// and the live preview so both always render a template identically.
/// </summary>
public static class SlideElementStyleHelper
{
    public static string BuildStyle(double positionX, double positionY, double width, double height,
        double? fontSize, string? fontFamily, string? alignment, string? textColor, string? background,
        double? borderRadius, double? padding, bool bold)
    {
        var style = $"left:{positionX}%;top:{positionY}%;width:{width}%;height:{height}%;" +
                    $"font-size:{fontSize ?? 20}px;font-family:{WebUtility.HtmlEncode(fontFamily ?? "inherit")};" +
                    $"text-align:{WebUtility.HtmlEncode(alignment ?? "left")};box-sizing:border-box;";

        if (!string.IsNullOrWhiteSpace(textColor))
        {
            style += $"color:{WebUtility.HtmlEncode(textColor)};";
        }
        if (!string.IsNullOrWhiteSpace(background))
        {
            style += $"background:{WebUtility.HtmlEncode(background)};";
        }
        if (borderRadius is > 0)
        {
            style += $"border-radius:{borderRadius}px;";
        }
        if (padding is > 0)
        {
            style += $"padding:{padding}px;";
        }
        if (bold)
        {
            style += "font-weight:700;";
        }

        return style;
    }
}
