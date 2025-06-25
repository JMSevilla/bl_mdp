using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class RetirementTimelineConfiguration
{
    public static void ConfigureRetirementTimelines(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantRetirementTimeline>(entity =>
        {
            entity.ToTable("WW_TIMELINE_CONFIG");

            entity.Property<string>("BusinessGroup")
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();

            entity.Property<string>(x => x.OutputId)
                .HasColumnName("OUTPUT_ID")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property<string>(x => x.Event)
                .HasColumnName("EVENT")
                .HasMaxLength(2)
                .IsRequired();

            entity.Property(p => p.SequenceNumber)
               .HasColumnName("SEQNO")
               .IsRequired();

            entity.HasKey("BusinessGroup", "OutputId", "SequenceNumber");

            entity.Property(p => p.CategoryIdentification)
              .HasColumnName("CATEGORY_LIST")
              .HasMaxLength(100);

            entity.Property(p => p.SchemeIdentification)
              .HasColumnName("SCHEME_LIST")
              .HasMaxLength(100);

            entity.Property(p => p.DateCalculatorFormula)
              .HasColumnName("DATE_CALC_FORMULA")
              .HasMaxLength(1000);
        });
    }
}