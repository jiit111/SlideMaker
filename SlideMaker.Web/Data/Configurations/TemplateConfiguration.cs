using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlideMaker.Web.Models;

namespace SlideMaker.Web.Data.Configurations;

public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.LayoutJson).IsRequired();

        builder.HasOne(t => t.CreatedByUser).WithMany(u => u.Templates)
            .HasForeignKey(t => t.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.CreatedByUserId);
        builder.HasIndex(t => t.Name);
    }
}
