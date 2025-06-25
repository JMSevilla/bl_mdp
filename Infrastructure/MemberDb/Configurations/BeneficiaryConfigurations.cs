using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members.Beneficiaries;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class BeneficiaryConfigurations
{
    public static void ConfigureBeneficiaries(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Beneficiary>(entity =>
        {
            entity.ToTable("NOMINATION_DETAIL");
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

            entity.Property(p => p.RevokeDate)
                .HasColumnName("ND19D")
                .HasConversion(MemberDbContext.DateTimeConverter);

            entity.Property(p => p.NominationDate)
                .HasColumnName("ND14D")
                .HasConversion(MemberDbContext.DateTimeConverter);

            entity.OwnsOne(p => p.BeneficiaryDetails, p =>
            {
                p.Property(pp => pp.Forenames).HasColumnName("ND03X").HasMaxLength(32);
                p.Property(pp => pp.Surname).HasColumnName("ND02X").HasMaxLength(20);
                p.Property(pp => pp.MixedCaseSurname).HasColumnName("ND20A").HasMaxLength(20);
                p.Property(pp => pp.DateOfBirth).HasColumnName("ND18D").HasConversion(MemberDbContext.DateTimeConverter);
                p.Property(pp => pp.LumpSumPercentage).HasColumnName("ND12N");
                p.Property(pp => pp.PensionPercentage).HasColumnName("ND13N");
                p.Property(pp => pp.CharityName).HasColumnName("ND28X").HasMaxLength(120);
                p.Property(pp => pp.CharityNumber).HasColumnName("ND29I");
                p.Property(pp => pp.Relationship).HasColumnName("ND11X").HasMaxLength(20);
                p.Property(pp => pp.Notes).HasColumnName("WWND24X").HasMaxLength(500);
            });
            entity.Navigation(b => b.BeneficiaryDetails).IsRequired();

            entity.OwnsOne(p => p.Address, p =>
            {
                p.Property(pp => pp.Line1).HasMaxLength(25).HasColumnName("ND07X");
                p.Property(pp => pp.Line2).HasMaxLength(25).HasColumnName("ND08X");
                p.Property(pp => pp.Line3).HasMaxLength(25).HasColumnName("ND09X");
                p.Property(pp => pp.Line4).HasMaxLength(25).HasColumnName("ND10X");
                p.Property(pp => pp.Line5).HasMaxLength(25).HasColumnName("ND15X");
                p.Property(pp => pp.PostCode).HasMaxLength(8).HasColumnName("ND17X");
                p.Property(pp => pp.Country).HasMaxLength(25).HasColumnName("ND16X");
                p.Property(pp => pp.CountryCode).HasMaxLength(3).HasColumnName("WWND23X");
            });
            entity.Navigation(b => b.Address).IsRequired();
        });
    }
}