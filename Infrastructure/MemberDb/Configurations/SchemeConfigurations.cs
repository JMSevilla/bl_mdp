using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class SchemeConfigurations
{
    public static void ConfigureScheme(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Scheme>(entity =>
        {
            entity.ToTable("SCHEME_DETAIL");

            entity.Property<string>("SD01X")
                .HasColumnName("SD01X")
                .IsRequired();

            entity.Property<string>("BGROUP")
                .HasColumnName("BGROUP")
                .IsRequired();
            entity.HasKey("SD01X", "BGROUP");
            entity
                .Property(c => c.Name)
                .HasColumnName("SD02X");
            entity
                .Property(c => c.Type)
                .HasColumnName("WWSD82X");
            entity
                .Property(c => c.BaseCurrency)
                .HasColumnName("SD80X");
        });
    }
}