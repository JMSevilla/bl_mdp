using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class CalculationHistoryConfiguration
{
    public static void ConfigureCalculationHistory(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CalculationHistory>(entity =>
        {
            entity.ToTable("CALC_SYSTEM_HISTORY");

            entity.Property(p => p.BusinessGroup)
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(p => p.ReferenceNumber)
                .HasColumnName("REFNO")
                .HasMaxLength(7)
                .IsRequired();

            entity.Property(p => p.SequenceNumber)
                .HasColumnName("SEQNO")
                .IsRequired();

            entity
                .Property(c => c.Event)
                .HasColumnName("EVENT")
                .IsRequired();

            entity
                .Property(c => c.ImageId)
                .HasColumnName("IMAGE_ID");

            entity
                .Property(c => c.FileId)
                .HasColumnName("FILE_ID");

            entity.HasKey(calc => new { calc.ReferenceNumber, calc.BusinessGroup, calc.SequenceNumber, calc.Event });
        });
    }
}

