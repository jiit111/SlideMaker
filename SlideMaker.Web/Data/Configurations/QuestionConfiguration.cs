using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlideMaker.Web.Models;

namespace SlideMaker.Web.Data.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.Property(q => q.QuestionText).IsRequired();
        builder.Property(q => q.CorrectAnswer).HasMaxLength(1000);

        // Restrict (not SetNull) to avoid SQL Server's "multiple cascade paths" error, since
        // Subject -> Chapter -> Topic already cascades and Questions reference all three.
        builder.HasOne(q => q.Subject).WithMany().HasForeignKey(q => q.SubjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(q => q.Chapter).WithMany().HasForeignKey(q => q.ChapterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(q => q.Topic).WithMany().HasForeignKey(q => q.TopicId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(q => q.CreatedByUser).WithMany().HasForeignKey(q => q.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(q => q.SubjectId);
        builder.HasIndex(q => q.ChapterId);
        builder.HasIndex(q => q.TopicId);
        builder.HasIndex(q => q.Language);
        builder.HasIndex(q => q.CreatedOn);

        builder.HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.Property(o => o.OptionLabel).HasMaxLength(10).IsRequired();
        builder.Property(o => o.OptionText).IsRequired();
    }
}
