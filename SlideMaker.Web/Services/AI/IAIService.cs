using SlideMaker.Web.DTOs;

namespace SlideMaker.Web.Services.AI;

public interface IAIService
{
    string ProviderName { get; }

    Task<AIResponse> GenerateContentAsync(AIRequest request, CancellationToken cancellationToken = default);
}
