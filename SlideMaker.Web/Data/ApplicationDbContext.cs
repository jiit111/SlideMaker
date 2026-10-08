using Microsoft.EntityFrameworkCore;
using SlideMaker.Web.Models;

namespace SlideMaker.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<Presentation> Presentations => Set<Presentation>();
    public DbSet<Slide> Slides => Set<Slide>();
    public DbSet<SlideElement> SlideElements => Set<SlideElement>();
    public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();
    public DbSet<GenerationJob> GenerationJobs => Set<GenerationJob>();
    public DbSet<AIRequestLog> AIRequestLogs => Set<AIRequestLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
