using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlideMaker.Web.Models;

namespace SlideMaker.Web.Data.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(150).IsRequired();
        builder.HasIndex(s => s.Name).IsUnique();
    }
}

public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
{
    public void Configure(EntityTypeBuilder<Chapter> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.HasOne(c => c.Subject)
            .WithMany(s => s.Chapters)
            .HasForeignKey(c => c.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(c => new { c.SubjectId, c.Name });
    }
}

public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.HasOne(t => t.Chapter)
            .WithMany(c => c.Topics)
            .HasForeignKey(t => t.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(t => new { t.ChapterId, t.Name });
    }
}
