using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class NotificationSettingConfigurations
{
    public static void ConfigureNotificationSetting(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationSetting>(entity =>
        {
            entity.ToTable("WW_PREFERENCES");

            entity.Property<string>(x => x.ReferenceNumber)
                .HasColumnName("REFNO")
                .HasMaxLength(7)
                .IsRequired();

            entity.Property<string>(x => x.BusinessGroup)
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(p => p.SequenceNumber)
                .HasColumnName("SEQNO")
                .IsRequired();

            entity.Property(x => x.Scheme)
               .HasColumnName("SCHMEM")
               .HasMaxLength(1)
               .IsRequired();

            entity.HasKey("ReferenceNumber", "BusinessGroup", "SequenceNumber");

            entity.Property(p => p.StartDate)
               .HasColumnName("STARTD")
               .HasConversion(MemberDbContext.DateTimeConverter)
               .IsRequired();

            entity.Property(p => p.EndDate)
               .HasColumnName("ENDD")
               .HasConversion(MemberDbContext.DateTimeConverter);

            entity.Property(x => x.OnlineCommunicationConsent)
                .HasColumnName("ECOMM_CONSENT")
                .HasMaxLength(30);

            entity.Property(x => x.Settings)
                .HasColumnName("COMM_PREF")
                .HasMaxLength(30);

            entity.HasOne<Member>()
                .WithMany(p => p.NotificationSettings)
                .HasForeignKey("ReferenceNumber", "BusinessGroup")
                .IsRequired();
        });
    }
}