using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class ObjectStatusConfigurations
{
    public static void ConfigureObjectStatus(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ObjectStatus>(entity =>
        {
            entity.ToTable("DD_OBJECT_STATUS").HasNoKey();

            entity.Property(p => p.BusinessGroup)
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(p => p.ObjectId)
                .HasColumnName("OBJECT_ID")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(p => p.StatusAccess)
               .HasColumnName("STATUS_ACCESS")
               .HasMaxLength(240);

            entity.Property(p => p.TableShort)
              .HasColumnName("TABLE_SHORT")
              .HasMaxLength(2);
        });
    }
}