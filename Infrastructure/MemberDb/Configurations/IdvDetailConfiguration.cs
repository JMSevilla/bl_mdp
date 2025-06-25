using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class IdvDetailConfiguration
{
    public static void ConfigureIdvDetail(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdvDetail>(entity =>
        {
            entity.ToTable("WW_IDV_DETAIL");

            entity
                .Property(c => c.Id)
                .HasColumnName("DETAIL_ID")
                .IsRequired();

            entity
                .Property(c => c.ScanResult)
                .HasColumnName("WWIDV17X")
                .HasMaxLength(1);

            entity
                .Property(c => c.DocumentType)
                .HasColumnName("WWIDV18X")
                .HasMaxLength(255);

            entity
                .Property(c => c.EdmsNumber)
                .HasColumnName("WWIDV19I");

            entity.HasKey(x => x.Id);
        });
    }

}