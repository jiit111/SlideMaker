namespace SlideMaker.Web.Models;

/// <summary>
/// Tracks a unit of extraction/generation work. Processed synchronously in Phase 1,
/// but modeled as a discrete job now so a background worker can pick these up later
/// without changing callers.
/// </summary>
public class GenerationJob
{
    public int Id { get; set; }
    public GenerationJobType JobType { get; set; }
    public GenerationJobStatus Status { get; set; } = GenerationJobStatus.Pending;

    public string? RequestPayloadJson { get; set; }
    public string? ResultPayloadJson { get; set; }
    public string? ErrorMessage { get; set; }

    public int CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? CompletedOn { get; set; }
}

public class AIRequestLog
{
    public int Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string? PromptSummary { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int? QuestionCountRequested { get; set; }
    public int? QuestionCountReturned { get; set; }

    public int CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    public DateTime CreatedOn { get; set; }
}
