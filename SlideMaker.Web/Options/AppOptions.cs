namespace SlideMaker.Web.Options;

public class AIOptions
{
    public const string SectionName = "AI";
    public string Provider { get; set; } = "Mock";
    public OllamaOptions Ollama { get; set; } = new();
    public OpenAICompatibleOptions OpenAICompatible { get; set; } = new();
}

public class OllamaOptions
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3";
}

public class OpenAICompatibleOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Tried in order if Model returns a transient error (e.g. provider overloaded) after
    /// exhausting retries. Optional — leave empty to only ever use Model.
    /// </summary>
    public string[] FallbackModels { get; set; } = [];
}

public class BrandingOptions
{
    public const string SectionName = "Branding";
    public string AcademyName { get; set; } = "PS Academy";
}

public class OcrOptions
{
    public const string SectionName = "OCR";
    public string Provider { get; set; } = "Tesseract";
    public string TessDataPath { get; set; } = "App_Data/tessdata";
    public string Languages { get; set; } = "eng+hin";
}

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";
    public string RootPath { get; set; } = "wwwroot/uploads";
    public int MaxFileSizeMB { get; set; } = 50;
    public string[] AllowedImageExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
    public string[] AllowedDocumentExtensions { get; set; } = [".pdf", ".docx"];
    public string[] AllowedTemplateExtensions { get; set; } = [".pptx"];
}
