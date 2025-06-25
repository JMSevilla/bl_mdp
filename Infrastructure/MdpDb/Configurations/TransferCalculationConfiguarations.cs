using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MdpDb.Configurations;

public static class TransferCalculationConfiguarations
{
    public static void ConfigureTransferCalculation(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransferCalculation>(entity =>
        {
            entity.ToTable("TransferCalculation");

            entity.Property(p => p.BusinessGroup)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(p => p.ReferenceNumber)
                .HasMaxLength(7)
                .IsRequired();

            entity.HasKey(x => new { x.BusinessGroup, x.ReferenceNumber });

            entity.Property(p => p.TransferQuoteJson);
            entity.Property(p => p.HasLockedInTransferQuote).IsRequired();
            entity.Property(p => p.CreatedAt).HasDefaultValue(DateTimeOffset.MinValue).IsRequired();
            entity.Property(p => p.Status)
                .HasConversion(
                v => v.ToString(),
                v => (TransferApplicationStatus)Enum.Parse(typeof(TransferApplicationStatus), v))
                .HasMaxLength(25);
        });
    }
}
