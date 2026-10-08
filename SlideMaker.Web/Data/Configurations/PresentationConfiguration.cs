using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlideMaker.Web.Models;

namespace SlideMaker.Web.Data.Configurations;

public class PresentationConfiguration : IEntityTypeConfiguration<Presentation>
{
    public void Configure(EntityTypeBuilder<Presentation> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(300).IsRequired();

        builder.HasOne(p => p.Template).WithMany(t => t.Presentations)
            .HasForeignKey(p => p.TemplateId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(p => p.CreatedByUser).WithMany(u => u.Presentations)
            .HasForeignKey(p => p.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.CreatedByUserId);
        builder.HasIndex(p => p.CreatedOn);

        builder.HasMany(p => p.Slides)
            .WithOne(s => s.Presentation)
            .HasForeignKey(s => s.PresentationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SlideConfiguration : IEntityTypeConfiguration<Slide>
{
    public void Configure(EntityTypeBuilder<Slide> builder)
    {
        builder.HasOne(s => s.Question).WithMany()
            .HasForeignKey(s => s.QuestionId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => new { s.PresentationId, s.SlideNumber });

        builder.HasMany(s => s.Elements)
            .WithOne(e => e.Slide)
            .HasForeignKey(e => e.SlideId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SlideElementConfiguration : IEntityTypeConfiguration<SlideElement>
{
    public void Configure(EntityTypeBuilder<SlideElement> builder)
    {
        builder.Property(e => e.Content).IsRequired();
    }
}
