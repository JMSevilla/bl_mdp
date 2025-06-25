using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members.Dependants;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class DependantConfigurations
{
    public static void ConfigureDependants(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dependant>(entity =>
        {
            entity.ToTable("DEPENDANTS_DETAIL");
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

            entity.HasKey(b => new { b.ReferenceNumber, b.BusinessGroup, b.SequenceNumber });

            entity.Property(pp => pp.Forenames)
                .HasColumnName("DD03A")
                .HasMaxLength(32);

            entity.Property(pp => pp.Surname)
                .HasColumnName("DD02A")
                .HasMaxLength(20);

            entity.Property(pp => pp.DateOfBirth)
                .HasColumnName("DD07D")
                .HasConversion(MemberDbContext.DateTimeConverter);

            entity.Property(pp => pp.RelationshipCode).HasColumnName("DD28X").HasMaxLength(20);
            entity.Property(pp => pp.Gender).HasColumnName("DD06A").HasMaxLength(1);

            entity.Property(p => p.StartDate)
               .HasColumnName("DD29D")
               .HasConversion(MemberDbContext.DateTimeConverter);

            entity.Property(p => p.EndDate)
                .HasColumnName("DD30D")
                .HasConversion(MemberDbContext.DateTimeConverter);

            entity.OwnsOne(p => p.Address, p =>
            {
                p.Property(pp => pp.Line1).HasMaxLength(25).HasColumnName("DD13X");
                p.Property(pp => pp.Line2).HasMaxLength(25).HasColumnName("DD14X");
                p.Property(pp => pp.Line3).HasMaxLength(25).HasColumnName("DD15X");
                p.Property(pp => pp.Line4).HasMaxLength(25).HasColumnName("DD16X");
                p.Property(pp => pp.Line5).HasMaxLength(25).HasColumnName("DD17X");
                p.Property(pp => pp.PostCode).HasMaxLength(8).HasColumnName("DD19X");
                p.Property(pp => pp.Country).HasMaxLength(25).HasColumnName("DD18X");
            });
            entity.Navigation(b => b.Address).IsRequired();
        });
    }
}