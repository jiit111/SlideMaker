using SlideMaker.Web.Models;

namespace SlideMaker.Web.Services.Templates;

public interface ITemplateService
{
    Task<List<Template>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Template?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Template?> GetDefaultAsync(CancellationToken cancellationToken = default);
    Task<Template> CreateAsync(Template template, CancellationToken cancellationToken = default);
    Task<Template> UpdateAsync(Template template, CancellationToken cancellationToken = default);
    Task<Template> DuplicateAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task SetDefaultAsync(int id, CancellationToken cancellationToken = default);
}
