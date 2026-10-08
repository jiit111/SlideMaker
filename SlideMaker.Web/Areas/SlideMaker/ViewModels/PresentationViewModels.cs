using SlideMaker.Web.Models;

namespace SlideMaker.Web.Areas.SlideMaker.ViewModels;

public class CreatePresentationViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SourceMode { get; set; } = "upload"; // upload | ai | bank
}

public class ChoosePatternViewModel
{
    public int PresentationId { get; set; }
    public string QuestionIdsCsv { get; set; } = string.Empty;
    public int TemplateId { get; set; }
    public SlidePattern Pattern { get; set; } = SlidePattern.OneQuestionPerSlide;
    public List<TemplateOptionViewModel> AvailableTemplates { get; set; } = new();
    public int QuestionCount { get; set; }

    // Branding: header (academy/class), calendar/special-day, and free-text banners.
    public string AcademyName { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public DateTime EventDate { get; set; } = DateTime.Today;
    public string? FestivalName { get; set; }
    public string? HeaderMessage { get; set; }
    public string? InstructorCredit { get; set; }
}

public class TemplateOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class PresentationPreviewViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PresentationStatus Status { get; set; }
    public string? TemplateName { get; set; }
    public SlidePattern SlidePattern { get; set; }
    public List<SlideViewModel> Slides { get; set; } = new();
}

public class SlideViewModel
{
    public int Id { get; set; }
    public int SlideNumber { get; set; }
    public SlideType SlideType { get; set; }
    public string? Title { get; set; }
    public string? Background { get; set; }
    public List<SlideElementViewModel> Elements { get; set; } = new();
}

public class SlideElementViewModel
{
    public int Id { get; set; }
    public SlideElementType ElementType { get; set; }
    public string Content { get; set; } = string.Empty;
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double? FontSize { get; set; }
    public string? FontFamily { get; set; }
    public string? Alignment { get; set; }
    public string? TextColor { get; set; }
    public string? Background { get; set; }
    public double? BorderRadius { get; set; }
    public double? Padding { get; set; }
    public bool Bold { get; set; }
}
