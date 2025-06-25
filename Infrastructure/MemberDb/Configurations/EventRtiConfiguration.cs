using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class EventRtiConfiguration
{
    public static void ConfigureEventRti(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventRti>(entity =>
        {
            entity.ToTable("WW_EVENT_RTI_CONFIG");

            entity
                .Property(c => c.Score)
                .HasColumnName("SCORE")
                .IsRequired();

            entity
                .Property(c => c.BusinessGroup)
                .HasColumnName("BGROUP")
                .IsRequired();

            entity
                .Property(c => c.Status)
                .HasColumnName("STATUS")
                .HasMaxLength(2);

            entity
                .Property(c => c.CaseCode)
                .HasColumnName("CASECODE")
                .IsRequired()
                .HasMaxLength(4);

            entity.HasNoKey();
        });
    }
}