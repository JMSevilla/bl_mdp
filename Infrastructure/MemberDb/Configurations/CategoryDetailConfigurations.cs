using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class CategoryDetailConfigurations
{
    public static void ConfigureCategoryDetail(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoryDetail>(entity =>
        {
            entity.ToTable("CATEGORY_DETAIL");

            entity.Property<string>("CA26X")
                .HasColumnName("CA26X")
                .IsRequired();

            entity.Property<string>("BGROUP")
                .HasColumnName("BGROUP")
                .IsRequired();

            entity.HasKey("CA26X", "BGROUP");

            entity
                .Property(c => c.NormalRetirementAge)
                .HasColumnName("CA03I")
                .IsRequired();

            entity
                .Property(c => c.MinimumPensionAge)
                .HasColumnName("CA77I");
        });
    }
}