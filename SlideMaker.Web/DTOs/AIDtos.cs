using SlideMaker.Web.Models;

namespace SlideMaker.Web.DTOs;

public class AIQuestionGenerationRequest
{
    public string Subject { get; set; } = string.Empty;
    public string? ClassLevel { get; set; }
    public string? Chapter { get; set; }
    public string? Topic { get; set; }
    public int NumberOfQuestions { get; set; } = 10;
    public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Mixed;
    public ContentLanguage Language { get; set; } = ContentLanguage.English;
    public string? AdditionalInstruction { get; set; }
}

/// <summary>Freeform natural-language request from the dashboard "Quick AI" box, e.g. Hinglish requirement text.</summary>
public class AIFreeformRequest
{
    public string RequirementText { get; set; } = string.Empty;
}

public class AIRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? SystemInstruction { get; set; }
    public int? MaxQuestions { get; set; }
}

public class AIResponse
{
    public bool Success { get; set; }
    public string? RawContent { get; set; }
    public string? ErrorMessage { get; set; }
    public List<AIGeneratedQuestionDto> Questions { get; set; } = new();
}

public class AIGeneratedQuestionDto
{
    public string Question { get; set; } = string.Empty;
    public List<QuestionOptionDto> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string? Difficulty { get; set; }
    public string? Topic { get; set; }
    public string? Chapter { get; set; }
    public string? QuestionType { get; set; }
}

public class AIValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
