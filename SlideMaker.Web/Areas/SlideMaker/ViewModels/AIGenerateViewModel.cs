using SlideMaker.Web.Models;

namespace SlideMaker.Web.Areas.SlideMaker.ViewModels;

public class AIGenerateViewModel
{
    public int PresentationId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? ClassLevel { get; set; }
    public string? Chapter { get; set; }
    public string? Topic { get; set; }
    public int NumberOfQuestions { get; set; } = 10;
    public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Mixed;
    public ContentLanguage Language { get; set; } = ContentLanguage.English;
    public string? AdditionalInstruction { get; set; }

    public List<string>? ValidationErrors { get; set; }
}

public class QuickAIViewModel
{
    public string RequirementText { get; set; } = string.Empty;
}
