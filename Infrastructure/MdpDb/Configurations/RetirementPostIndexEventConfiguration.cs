using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MdpDb.Configurations;

public static class RetirementPostIndexEventConfiguration
{
    public static void ConfigurePostIndexEvent(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RetirementPostIndexEvent>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.ToTable("RetirementPostIndexEvent").HasKey("Id");

            entity.Property(p => p.BusinessGroup)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(p => p.ReferenceNumber)
                .HasMaxLength(7)
                .IsRequired();

            entity.Property(p => p.BatchNumber)
                .IsRequired();

            entity.Property(p => p.CaseNumber)
                .IsRequired();

            entity.Property(p => p.RetirementApplicationImageId)
                .IsRequired();
            
            entity.Property(p => p.DbId)
                .HasMaxLength(6);

            entity.Property(p => p.Error);
        });
    }
}