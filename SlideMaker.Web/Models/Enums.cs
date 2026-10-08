namespace SlideMaker.Web.Models;

public enum QuestionType
{
    MultipleChoice,
    TrueFalse,
    FillInTheBlank,
    ShortAnswer,
    LongAnswer
}

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard,
    Mixed
}

public enum ContentLanguage
{
    English,
    Hindi,
    Hinglish,
    Bilingual
}

public enum QuestionSourceType
{
    ManualEntry,
    ImageOcr,
    PdfExtraction,
    DocxExtraction,
    AIGenerated
}

public enum SlidePattern
{
    OneQuestionPerSlide,
    QuestionAndAnswerSameSlide,
    QuestionThenSolution,
    MultipleQuestionsPerSlide
}

public enum SlideType
{
    Title,
    Question,
    Answer,
    Content,
    Section
}

public enum SlideElementType
{
    Heading,
    QuestionText,
    OptionsList,
    AnswerText,
    ExplanationText,
    Image,
    Background,
    Footer
}

public enum PresentationStatus
{
    Draft,
    Generated,
    Published,
    Archived
}

public enum GenerationJobType
{
    QuestionExtraction,
    AIQuestionGeneration,
    SlideGeneration,
    Export
}

public enum GenerationJobStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public enum UploadPurpose
{
    QuestionImage,
    QuestionPdf,
    QuestionDocx,
    TemplateFile,
    TemplatePreview,
    PresentationAsset
}

public enum AIProviderType
{
    Mock,
    Ollama,
    OpenAICompatible
}
