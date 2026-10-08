namespace SlideMaker.Web.Models;

public class Question
{
    public int Id { get; set; }
    public int? SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public int? ChapterId { get; set; }
    public Chapter? Chapter { get; set; }
    public int? TopicId { get; set; }
    public Topic? Topic { get; set; }

    public string QuestionText { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
    public ContentLanguage Language { get; set; } = ContentLanguage.English;
    public string? CorrectAnswer { get; set; }
    public string? Explanation { get; set; }
    public QuestionSourceType SourceType { get; set; } = QuestionSourceType.ManualEntry;

    public int SortOrder { get; set; }

    public int CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }

    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}

public class QuestionOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public string OptionLabel { get; set; } = string.Empty;
    public string OptionText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsCorrect { get; set; }
}
