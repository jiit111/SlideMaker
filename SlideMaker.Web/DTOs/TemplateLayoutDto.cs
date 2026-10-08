namespace SlideMaker.Web.DTOs;

public class TemplateLayoutDto
{
    /// <summary>Slide background — any valid CSS `background` value (hex, rgba, or a full gradient string).</summary>
    public string Background { get; set; } = "#ffffff";
    public string HeaderColor { get; set; } = "#0d6efd";
    public string AccentColor { get; set; } = "#0d6efd";
    public string FontFamily { get; set; } = "Segoe UI";
    public double TitleFontSize { get; set; } = 32;
    public double BodyFontSize { get; set; } = 22;

    /// <summary>Default text color for question/body text.</summary>
    public string TextColor { get; set; } = "#1a1a2e";

    /// <summary>Card background behind each element — hex, rgba, or gradient. Empty/null = no card (plain text).</summary>
    public string? QuestionBackground { get; set; }
    public string? OptionsBackground { get; set; }
    public string? AnswerBackground { get; set; }
    public string? ExplanationBackground { get; set; }

    /// <summary>Text color used specifically on the answer/explanation slide to stand out against AnswerBackground.</summary>
    public string? AnswerTextColor { get; set; }

    public double BorderRadius { get; set; } = 0;
    public double CardPadding { get; set; } = 0;

    public LayoutRegionDto QuestionPosition { get; set; } = new() { X = 5, Y = 15, Width = 90, Height = 30 };
    public LayoutRegionDto OptionsPosition { get; set; } = new() { X = 8, Y = 50, Width = 84, Height = 40 };
    public LayoutRegionDto AnswerPosition { get; set; } = new() { X = 5, Y = 20, Width = 90, Height = 20 };
    public LayoutRegionDto ExplanationPosition { get; set; } = new() { X = 5, Y = 45, Width = 90, Height = 40 };
}

public class LayoutRegionDto
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}
