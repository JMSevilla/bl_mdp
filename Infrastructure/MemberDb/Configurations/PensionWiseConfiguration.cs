using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class PensionWiseConfiguration
{
    public static void ConfigurePensionWise(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PensionWise>(entity =>
        {
            entity.ToTable("PENSION_WISE");

            entity.Property(p => p.BusinessGroup)
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(p => p.ReferenceNumber)
                .HasColumnName("REFNO")
                .HasMaxLength(7)
                .IsRequired();

            entity.Property(p => p.SequenceNumber)
                .HasValueGenerator((_, _) => new SequenceNumberGenerator<PensionWise>(wise => wise.SequenceNumber,
                    current => query =>
                        current.BusinessGroup == query.BusinessGroup &&
                        current.ReferenceNumber == query.ReferenceNumber))
                .HasColumnName("SEQNO")
                .IsRequired();

            entity.HasKey(wise => new {wise.ReferenceNumber, wise.BusinessGroup, wise.SequenceNumber});

            entity.Property(p => p.PensionWiseSettlementCaseType)
                .HasColumnName("PW_SETTLEMENT")
                .HasMaxLength(15);

            entity.Property(p => p.CaseNumber)
                .HasColumnName("CASENO")
                .HasMaxLength(7);

            entity.Property(p => p.PwResponse)
                .HasColumnName("PW_RESPONSE")
                .HasMaxLength(20);

            entity.Property(p => p.ReasonForExemption)
                .HasColumnName("PW_EXEMPTION")
                .HasMaxLength(1);

            entity
                .Property(c => c.FinancialAdviseDate)
                .HasColumnName("FADV_DATE")
                .HasConversion(MemberDbContext.DateTimeConverter);

            entity
                .Property(c => c.PensionWiseDate)
                .HasColumnName("PADV_DATE")
                .HasConversion(MemberDbContext.DateTimeConverter);
        });
    }
}