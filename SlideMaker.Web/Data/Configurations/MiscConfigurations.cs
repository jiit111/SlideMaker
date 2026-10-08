using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlideMaker.Web.Models;

namespace SlideMaker.Web.Data.Configurations;

public class UploadedFileConfiguration : IEntityTypeConfiguration<UploadedFile>
{
    public void Configure(EntityTypeBuilder<UploadedFile> builder)
    {
        builder.Property(f => f.OriginalFileName).HasMaxLength(500).IsRequired();
        builder.Property(f => f.StoredFileName).HasMaxLength(300).IsRequired();
        builder.Property(f => f.StoragePath).HasMaxLength(1000).IsRequired();
        builder.Property(f => f.ContentType).HasMaxLength(150).IsRequired();

        builder.HasOne(f => f.CreatedByUser).WithMany()
            .HasForeignKey(f => f.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GenerationJobConfiguration : IEntityTypeConfiguration<GenerationJob>
{
    public void Configure(EntityTypeBuilder<GenerationJob> builder)
    {
        builder.HasOne(j => j.CreatedByUser).WithMany()
            .HasForeignKey(j => j.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(j => j.Status);
    }
}

public class AIRequestLogConfiguration : IEntityTypeConfiguration<AIRequestLog>
{
    public void Configure(EntityTypeBuilder<AIRequestLog> builder)
    {
        builder.Property(l => l.Provider).HasMaxLength(100).IsRequired();
        builder.Property(l => l.RequestType).HasMaxLength(100).IsRequired();

        builder.HasOne(l => l.CreatedByUser).WithMany()
            .HasForeignKey(l => l.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(l => l.CreatedOn);
    }
}
