using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class SdDomainListConfigurations
{
    public static void ConfigureSdDomainList(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SdDomainList>(entity =>
        {
            entity.ToTable("SD_DOMAIN_LIST");

            entity.HasNoKey();

            entity.Property(x => x.BusinessGroup)
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(x => x.Domain)
                .HasColumnName("DOMAIN")
                .HasMaxLength(5)
                .IsRequired();

            entity.Property(x => x.ListValue)
                .HasColumnName("DOL_LISTVAL")
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.Property(x => x.ListValueDescription)
               .HasColumnName("DOL_DESCRIPTION")
               .HasMaxLength(80)
                .IsUnicode(false);

            entity.Property(x => x.SystemValue)
               .HasColumnName("DOL_SYSTVAL")
               .HasMaxLength(30)
                .IsUnicode(false);
        });
    }
}