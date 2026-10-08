using SlideMaker.Web.Models;

namespace SlideMaker.Web.Areas.SlideMaker.ViewModels;

public class QuestionReviewViewModel
{
    public int PresentationId { get; set; }
    public List<QuestionCardViewModel> Questions { get; set; } = new();
}

public class QuestionCardViewModel
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<QuestionOptionViewModel> Options { get; set; } = new();
    public string? CorrectAnswer { get; set; }
    public string? Explanation { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public ContentLanguage Language { get; set; }
}

public class QuestionOptionViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class QuestionBankPickerViewModel
{
    public int PresentationId { get; set; }
    public string? SearchTerm { get; set; }
    public int? SubjectId { get; set; }
    public List<SubjectOptionViewModel> Subjects { get; set; } = new();
    public List<QuestionCardViewModel> Results { get; set; } = new();
}

public class SubjectOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
