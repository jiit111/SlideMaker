using Microsoft.EntityFrameworkCore;
using SlideMaker.Web.Data;
using SlideMaker.Web.Models;

namespace SlideMaker.Web.Services.Templates;

public class TemplateService : ITemplateService
{
    private readonly ApplicationDbContext _db;

    public TemplateService(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<List<Template>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _db.Templates.Where(t => t.IsActive).OrderByDescending(t => t.IsDefault).ThenBy(t => t.Name).ToListAsync(cancellationToken);

    public Task<Template?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Templates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Template?> GetDefaultAsync(CancellationToken cancellationToken = default) =>
        _db.Templates.Where(t => t.IsActive).OrderByDescending(t => t.IsDefault).FirstOrDefaultAsync(cancellationToken);

    public async Task<Template> CreateAsync(Template template, CancellationToken cancellationToken = default)
    {
        template.CreatedOn = DateTime.UtcNow;
        _db.Templates.Add(template);
        await _db.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task<Template> UpdateAsync(Template template, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Templates.FirstOrDefaultAsync(t => t.Id == template.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Template {template.Id} not found.");

        existing.Name = template.Name;
        existing.Description = template.Description;
        existing.LayoutJson = template.LayoutJson;
        existing.ModifiedOn = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<Template> DuplicateAsync(int id, CancellationToken cancellationToken = default)
    {
        var source = await _db.Templates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Template {id} not found.");

        var copy = new Template
        {
            Name = $"{source.Name} (Copy)",
            Description = source.Description,
            LayoutJson = source.LayoutJson,
            PreviewImagePath = source.PreviewImagePath,
            SourceFilePath = source.SourceFilePath,
            IsDefault = false,
            IsActive = true,
            CreatedByUserId = source.CreatedByUserId,
            CreatedOn = DateTime.UtcNow
        };

        _db.Templates.Add(copy);
        await _db.SaveChangesAsync(cancellationToken);
        return copy;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var template = await _db.Templates.FindAsync([id], cancellationToken);
        if (template is not null)
        {
            template.IsActive = false;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task SetDefaultAsync(int id, CancellationToken cancellationToken = default)
    {
        var templates = await _db.Templates.ToListAsync(cancellationToken);
        foreach (var t in templates)
        {
            t.IsDefault = t.Id == id;
        }
        await _db.SaveChangesAsync(cancellationToken);
    }
}
