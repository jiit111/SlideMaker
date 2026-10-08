using SlideMaker.Web.Models;

namespace SlideMaker.Web.ViewModels;

public class DashboardViewModel
{
    public List<PresentationSummaryViewModel> RecentPresentations { get; set; } = new();
    public List<TemplateSummaryViewModel> Templates { get; set; } = new();
    public int QuestionBankCount { get; set; }
}

public class PresentationSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SlideCount { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? TemplateName { get; set; }
    public PresentationStatus Status { get; set; }
}

public class TemplateSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreviewImagePath { get; set; }
    public bool IsDefault { get; set; }
}
